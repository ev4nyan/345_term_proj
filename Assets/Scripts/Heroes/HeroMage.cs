using UnityEngine;
using System.Collections;

/// <summary>
/// Mage hero - Fires projectiles at enemies.
/// Ultimate: Devastating fire storm that damages all enemies in a large area.
/// </summary>
public class HeroMage : Hero
{
    [Header("Mage Projectile Attack")]
    public GameObject projectilePrefab;    // Assign a fireball/magic projectile prefab
    public Transform firePoint;            // Where projectiles spawn from
    public float projectileSpeed = 10f;

    [Header("Ultimate - Fire Storm")]
    public GameObject fireStormEffectPrefab;  // Optional visual effect
    public int fireStormTicks = 5;            // Number of damage ticks
    public float fireStormTickRate = 0.5f;    // Time between ticks

    protected override void Start()
    {
        base.Start();
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.25f);
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        // Fire projectile at target
        if (target != null && Time.time >= nextAttackTime)
        {
            FireProjectile();
            nextAttackTime = Time.time + (1f / heroData.attackRate);
        }
    }

    void UpdateTarget()
    {
        if (isDead || heroData == null) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance && distance <= heroData.attackRange)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        target = nearestEnemy != null ? nearestEnemy.transform : null;
    }

    void FireProjectile()
    {
        if (target == null) return;

        // Play attack animation
        if (animator != null)
            animator.SetTrigger("Atk");

        // Determine spawn position
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            
            // Try Fireball component (homing)
            Fireball fireball = proj.GetComponent<Fireball>();
            if (fireball != null)
            {
                fireball.speed = projectileSpeed;
                fireball.Seek(target, heroData.attackDamage);
                return;
            }

            // Try MagicProjectile component (directional)
            MagicProjectile magicProj = proj.GetComponent<MagicProjectile>();
            if (magicProj != null)
            {
                Vector2 dir = (target.position - spawnPos).normalized;
                magicProj.speed = projectileSpeed;
                magicProj.Init(dir, (int)heroData.attackDamage, enemyLayer);
            }
        }
        else
        {
            // Fallback: direct damage if no prefab assigned
            DealDamageToTarget(target.gameObject, heroData.attackDamage);
        }
    }

    protected override void ExecuteFireStorm()
    {
        Debug.Log($"{heroData.heroName} unleashes FIRE STORM!");

        // Spawn visual effect if available
        if (fireStormEffectPrefab != null)
        {
            GameObject effect = Instantiate(fireStormEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, fireStormTicks * fireStormTickRate + 1f);
        }

        // Start the fire storm coroutine
        StartCoroutine(FireStormDamage());
    }

    IEnumerator FireStormDamage()
    {
        float damagePerTick = heroData.ultimateDamage / fireStormTicks;

        for (int i = 0; i < fireStormTicks; i++)
        {
            // Damage all enemies in radius
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, heroData.ultimateRadius, enemyLayer);
            foreach (var enemy in enemies)
            {
                DealDamageToTarget(enemy.gameObject, damagePerTick);
            }

            yield return new WaitForSeconds(fireStormTickRate);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (heroData == null) return;

        // Draw attack range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, heroData.attackRange);

        // Draw ultimate range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, heroData.ultimateRadius);
    }
}
