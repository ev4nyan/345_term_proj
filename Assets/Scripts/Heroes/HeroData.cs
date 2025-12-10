using UnityEngine;

public enum HeroType { King, Warrior, Warlock, Mage, Teleporter, Samurai, MageKnight, Archer, Cleric }
public enum UltimateType { DragonSummon, BlackHole, FireStorm, MassResurrection, TeleportStrike, ThousandCuts, ThunderGod, ArrowRain }

[CreateAssetMenu(fileName = "NewHero", menuName = "Tower Defense/Hero Data")]
public class HeroData : ScriptableObject
{
    [Header("Identity")]
    public string heroName = "Unknown Hero";
    public HeroType heroType;
    public Sprite portrait;
    public Sprite deadPortrait; // Grayed out version
    public GameObject heroPrefab;

    [Header("Lore")]
    [TextArea(3, 5)]
    public string backstory;

    [Header("Stats")]
    public int maxHealth = 100;
    public float attackDamage = 20f;
    public float attackRange = 5f;
    public float attackRate = 1f;

    [Header("Cost")]
    public int resolveCost = 50; // Cost to summon this hero

    [Header("Deployment Rules")]
    public bool hasPermadeath = false;    // If true, hero dies forever when killed
    public bool singlePlacement = false;  // If true, can only have one on field at a time

    [Header("Ultimate - Final Stand")]
    public UltimateType ultimateType;
    public string ultimateName = "Final Stand";
    [TextArea(2, 3)]
    public string ultimateDescription;
    public float ultimateDamage = 500f;
    public float ultimateRadius = 5f;

    [Header("Sacrifice")]
    public int sacrificeResolveGain = 30; // Resolve gained when sacrificed
}
