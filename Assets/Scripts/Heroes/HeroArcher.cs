using UnityEngine;

/// <summary>
/// Archer hero - Ranged projectile attacks.
/// Shoots arrows at the nearest enemy in range.
/// </summary>
public class HeroArcher : Hero
{
    [Header("Archer Attack")]
    public Transform firePoint;
    public GameObject arrowPrefab;

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
            Attack();
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

    void Attack()
    {
        if (target == null || heroData == null) return;

        if (animator != null)
            animator.SetTrigger("Atk");

        if (arrowPrefab != null)
        {
            // Calculate direction to target
            Vector2 direction = ((Vector2)target.position - (Vector2)firePoint.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
            Projectile projectile = arrow.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Seek(target, heroData.attackDamage);
            }
        }
        else
        {
            // Direct damage if no projectile
            DealDamageToTarget(target.gameObject, heroData.attackDamage);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw attack range
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
