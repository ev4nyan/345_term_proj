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
        // Check if fallen (only matters if hasPermadeath)
        if (hero.hasPermadeath && fallenHeroes.Contains(hero))
            return false;

        // Check if already deployed (only matters if singlePlacement)
        if (hero.singlePlacement && deployedHeroes.Contains(hero))
            return false;

        // Check resolve
        if (ResolveManager.Instance == null || !ResolveManager.Instance.CanAfford(hero.resolveCost))
            return false;

        return true;
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

        // Only add to fallen if hasPermadeath is true
        if (data.hasPermadeath && !fallenHeroes.Contains(data))
        {
            fallenHeroes.Add(data);
            Debug.Log($"{data.heroName} has fallen forever. They will be remembered.");
        }
        else
        {
            // Return to available pool
            if (!availableHeroes.Contains(data))
            {
                availableHeroes.Add(data);
            }
            Debug.Log($"{data.heroName} has fallen but can be resummoned.");
        }

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

    /// <summary>
    /// Revive all fallen heroes (used by Cleric ultimate).
    /// Returns the number of heroes revived.
    /// </summary>
    public int ReviveAllFallenHeroes()
    {
        int count = fallenHeroes.Count;
        
        foreach (var hero in fallenHeroes)
        {
            if (!availableHeroes.Contains(hero))
            {
                availableHeroes.Add(hero);
            }
        }
        
        fallenHeroes.Clear();
        OnRosterChanged?.Invoke();
        
        Debug.Log($"Revived {count} fallen heroes!");
        return count;
    }
}
