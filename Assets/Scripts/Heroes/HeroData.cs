using UnityEngine;

public enum HeroType { Knight, Archer, Mage }
public enum UltimateType { UnbreakableShield, ArrowStorm, Meteor }

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
