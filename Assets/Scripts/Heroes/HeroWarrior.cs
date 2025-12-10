using UnityEngine;

/// <summary>
/// Warrior hero - Melee attacks with periodic shield ability.
/// Ultimate: Creates a black hole that pulls and damages enemies.
/// </summary>
public class HeroWarrior : Hero
{
    [Header("Warrior Attack")]
    public Vector2 attackOffset;  // Where the attack circle is, relative to hero

    [Header("Shield Ability")]
    public float shieldDuration = 2f;      // How long shield lasts
    public float shieldCooldown = 8f;      // Time between shields
    public Color shieldColor = new Color(0.5f, 0.8f, 1f, 0.8f);  // Visual tint when shielded

    [Header("Ultimate - Black Hole")]
    public GameObject blackHolePrefab;  // Optional prefab for visual effect

    private float nextShieldTime = 0f;
    private float shieldEndTime = 0f;
    private bool isShielded = false;

    protected override void Start()
    {
        base.Start();
        nextShieldTime = Time.time + shieldCooldown;
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        // Handle shield timing
        UpdateShield();

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

    void UpdateShield()
    {
        // Check if shield should end
        if (isShielded && Time.time >= shieldEndTime)
        {
            DeactivateShield();
        }

        // Check if we should activate shield
        if (!isShielded && Time.time >= nextShieldTime)
        {
            ActivateShield();
        }
    }

    void ActivateShield()
    {
        isShielded = true;
        shieldEndTime = Time.time + shieldDuration;
        nextShieldTime = Time.time + shieldCooldown;

        // Visual feedback
        if (spriteRenderer != null)
            spriteRenderer.color = shieldColor;

        // Play shield animation if available
        if (animator != null)
            animator.SetTrigger("Block");

        Debug.Log($"{heroData?.heroName} raises shield!");
    }

    void DeactivateShield()
    {
        isShielded = false;

        // Restore original color
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        Debug.Log($"{heroData?.heroName} shield down.");
    }

    // Override TakeDamage to block damage while shielded
    public override void TakeDamage(int damage)
    {
        if (isDead) return;

        if (isShielded)
        {
            Debug.Log($"{heroData?.heroName} blocked {damage} damage!");
            // Optional: play block sound/effect here
            return;
        }

        // Call base damage handling
        base.TakeDamage(damage);
    }

    Vector2 GetAttackOffset()
    {
        return new Vector2(facingRight ? attackOffset.x : -attackOffset.x, attackOffset.y);
    }

    protected override void ExecuteBlackHole()
    {
        Debug.Log($"{heroData.heroName} creates a BLACK HOLE!");

        // Spawn black hole at warrior's position
        if (blackHolePrefab != null)
        {
            GameObject blackHole = Instantiate(blackHolePrefab, transform.position, Quaternion.identity);
            BlackHole bh = blackHole.GetComponent<BlackHole>();
            if (bh != null)
            {
                bh.damage = heroData.ultimateDamage;
                bh.radius = heroData.ultimateRadius;
                bh.duration = 5f;
                bh.enemyLayer = enemyLayer;
            }
        }
        else
        {
            // Fallback: just spawn the black hole component directly
            GameObject blackHoleObj = new GameObject("BlackHole");
            blackHoleObj.transform.position = transform.position;
            BlackHole bh = blackHoleObj.AddComponent<BlackHole>();
            bh.damage = heroData.ultimateDamage;
            bh.radius = heroData.ultimateRadius;
            bh.duration = 5f;
            bh.enemyLayer = enemyLayer;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.yellow;
        Vector2 center = (Vector2)transform.position + GetAttackOffset();
        float range = heroData != null ? heroData.attackRange : 1.2f;
        Gizmos.DrawWireSphere(center, range);

        // Draw shield indicator if active
        if (isShielded)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.8f);
        }

        // Draw ultimate range
        if (heroData != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, heroData.ultimateRadius);
        }
    }
}
