using UnityEngine;

/// <summary>
/// Knight hero - Melee AoE attacks like KingController.
/// Attacks all enemies in a circle around the attack point.
/// </summary>
public class HeroKnight : Hero
{
    [Header("Knight Attack")]
    public Vector2 attackOffset;  // Where the attack circle is, relative to hero

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        // Check for enemies in attack range
        if (Time.time >= nextAttackTime)
        {
            Vector2 center = (Vector2)transform.position + GetAttackOffset();
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, heroData.attackRange, enemyLayer);

            if (hits.Length > 0)
            {
                nextAttackTime = Time.time + (1f / heroData.attackRate);

                // Play attack animation
                if (animator != null)
                    animator.SetTrigger("Atk");

                // Deal damage to all enemies in range
                foreach (var hit in hits)
                {
                    DealDamageToTarget(hit.gameObject, heroData.attackDamage);
                    
                    // Track target for flipping
                    if (target == null)
                        target = hit.transform;
                }
            }
            else
            {
                target = null;
            }
        }
    }

    Vector2 GetAttackOffset()
    {
        // Flip offset based on facing direction
        return new Vector2(facingRight ? attackOffset.x : -attackOffset.x, attackOffset.y);
    }

    void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.yellow;
        Vector2 center = (Vector2)transform.position + GetAttackOffset();
        float range = heroData != null ? heroData.attackRange : 1.2f;
        Gizmos.DrawWireSphere(center, range);

        // Draw ultimate range
        if (heroData != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, heroData.ultimateRadius);
        }
    }
}
