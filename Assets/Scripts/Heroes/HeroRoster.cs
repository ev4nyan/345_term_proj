using UnityEngine;
using System.Collections.Generic;

public class HeroRoster : MonoBehaviour
{
    public static HeroRoster Instance { get; private set; }

    [Header("Hero Database")]
    public List<HeroData> allHeroes = new List<HeroData>();

    [Header("Roster State")]
    public List<HeroData> availableHeroes = new List<HeroData>(); // Can be summoned
    public List<HeroData> deployedHeroes = new List<HeroData>();  // Currently on field
    public List<HeroData> fallenHeroes = new List<HeroData>();    // Dead forever

    private Dictionary<HeroData, Hero> deployedHeroInstances = new Dictionary<HeroData, Hero>();

    // Event to notify drag handlers to update their visuals
    public static event System.Action OnRosterChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Initialize all heroes as available
        availableHeroes = new List<HeroData>(allHeroes);

        // Subscribe to hero events
        Hero.OnHeroDied += OnHeroDied;
        Hero.OnHeroSacrificed += OnHeroSacrificed;
    }

    void OnDestroy()
    {
        Hero.OnHeroDied -= OnHeroDied;
        Hero.OnHeroSacrificed -= OnHeroSacrificed;
    }

    public bool CanSummonHero(HeroData hero)
    {
        return availableHeroes.Contains(hero) &&
               !deployedHeroes.Contains(hero) &&
               !fallenHeroes.Contains(hero) &&
               ResolveManager.Instance.CanAfford(hero.resolveCost);
    }

    public void DeployHero(HeroData heroData, Hero heroInstance)
    {
        if (!availableHeroes.Contains(heroData)) return;

        availableHeroes.Remove(heroData);
        deployedHeroes.Add(heroData);
        deployedHeroInstances[heroData] = heroInstance;

        OnRosterChanged?.Invoke();
    }

    void OnHeroDied(Hero hero)
    {
        if (hero.heroData == null) return;

        HeroData data = hero.heroData;

        // Remove from deployed
        if (deployedHeroes.Contains(data))
        {
            deployedHeroes.Remove(data);
            deployedHeroInstances.Remove(data);
        }

        // PERMADEATH - Add to fallen, never available again
        if (!fallenHeroes.Contains(data))
        {
            fallenHeroes.Add(data);
        }

        Debug.Log($"{data.heroName} has fallen. They will be remembered.");

        OnRosterChanged?.Invoke();
    }

    void OnHeroSacrificed(Hero hero)
    {
        Debug.Log($"{hero.heroData.heroName} made the ultimate sacrifice.");
    }

    public Hero GetDeployedHeroInstance(HeroData data)
    {
        return deployedHeroInstances.ContainsKey(data) ? deployedHeroInstances[data] : null;
    }

    public bool IsHeroFallen(HeroData hero)
    {
        return fallenHeroes.Contains(hero);
    }

    public bool IsHeroDeployed(HeroData hero)
    {
        return deployedHeroes.Contains(hero);
    }
}
