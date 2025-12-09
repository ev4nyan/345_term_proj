using UnityEngine;

/// <summary>
/// Cleric hero - Healer support.
/// Shoots healing projectiles at the most damaged allied hero in range.
/// Ultimate: Revives all fallen heroes (permadeath heroes can be summoned again).
/// </summary>
public class HeroCleric : Hero
{
    [Header("Cleric Healing")]
    public Transform firePoint;
    public GameObject healProjectilePrefab;
    public float healAmount = 15f;
    public string heroTag = "Hero";

    [Header("Ultimate - Mass Resurrection")]
    public GameObject resurrectionEffectPrefab;  // Optional visual effect

    protected override void Start()
    {
        base.Start();

        if (firePoint == null)
            firePoint = transform;

        InvokeRepeating(nameof(UpdateTarget), 0f, 0.25f);
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        if (target != null && Time.time >= nextAttackTime)
        {
            Heal();
            nextAttackTime = Time.time + (1f / heroData.attackRate);
        }
    }

    void UpdateTarget()
    {
        if (isDead || heroData == null) return;

        // Find allied heroes that need healing
        GameObject[] heroes = GameObject.FindGameObjectsWithTag(heroTag);
        float mostDamage = 0f;
        GameObject mostDamagedHero = null;

        foreach (GameObject heroObj in heroes)
        {
            // Don't target self
            if (heroObj == gameObject) continue;

            Hero hero = heroObj.GetComponent<Hero>();
            if (hero == null || hero.isDead) continue;

            // Check if in range
            float distance = Vector2.Distance(transform.position, heroObj.transform.position);
            if (distance > heroData.attackRange) continue;

            // Check if damaged
            int missingHealth = hero.heroData.maxHealth - hero.currentHealth;
            if (missingHealth > mostDamage)
            {
                mostDamage = missingHealth;
                mostDamagedHero = heroObj;
            }
        }

        target = mostDamagedHero != null ? mostDamagedHero.transform : null;
    }

    void Heal()
    {
        if (target == null || heroData == null) return;

        if (animator != null)
            animator.SetTrigger("Atk");

        if (healProjectilePrefab != null)
        {
            // Calculate direction to target
            Vector2 direction = ((Vector2)target.position - (Vector2)firePoint.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            GameObject projectile = Instantiate(healProjectilePrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
            HealProjectile healProj = projectile.GetComponent<HealProjectile>();
            if (healProj != null)
            {
                healProj.Seek(target, healAmount);
            }
        }
        else
        {
            // Direct heal if no projectile
            HealTarget(target.gameObject);
        }
    }

    void HealTarget(GameObject targetObj)
    {
        Hero hero = targetObj.GetComponent<Hero>();
        if (hero != null && !hero.isDead)
        {
            hero.currentHealth = Mathf.Min(hero.currentHealth + (int)healAmount, hero.heroData.maxHealth);
            Debug.Log($"{heroData?.heroName} healed {hero.heroData?.heroName} for {healAmount}!");
        }
    }

    protected override void ExecuteMassResurrection()
    {
        Debug.Log($"{heroData.heroName} casts MASS RESURRECTION!");

        // Spawn visual effect if available
        if (resurrectionEffectPrefab != null)
        {
            GameObject effect = Instantiate(resurrectionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        // Revive all fallen heroes
        if (HeroRoster.Instance != null)
        {
            int revived = HeroRoster.Instance.ReviveAllFallenHeroes();
            Debug.Log($"Revived {revived} fallen heroes!");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw heal range
        Gizmos.color = Color.green;
        float range = heroData != null ? heroData.attackRange : 8f;
        Gizmos.DrawWireSphere(transform.position, range);

        // Draw ultimate range
        if (heroData != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, heroData.ultimateRadius);
        }
    }
}
