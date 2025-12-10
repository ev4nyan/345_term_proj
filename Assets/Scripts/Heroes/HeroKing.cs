using System.Collections;
using UnityEngine;

/// <summary>
/// King hero - Melee AoE attacks.
/// Ultimate: Summons a friendly dragon to fight enemies.
/// Has permadeath and single placement.
/// </summary>
public class HeroKing : Hero
{
    [Header("King Attack")]
    public Vector2 attackOffset;  // Where the attack circle is, relative to hero

    [Header("Ultimate - Dragon Summon")]
    public GameObject friendlyDragonPrefab;  // Assign a dragon prefab that fights for you

    public AudioClip attackClip;
    private AudioSource audioSource;

    protected override void Start()
    {
        base.Start();
        audioSource = FindFirstObjectByType<Canvas>().GetComponent<AudioSource>();
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
                {
                    animator.SetTrigger("Atk");
                    audioSource.PlayOneShot(attackClip);
                }

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

    protected override void ExecuteDragonSummon()
    {
        Debug.Log($"{heroData.heroName} summons a DRAGON!");

        if (friendlyDragonPrefab != null)
        {
            // Spawn dragon near the king
            Vector3 spawnPos = transform.position + Vector3.up * 2f;
            GameObject dragon = Instantiate(friendlyDragonPrefab, spawnPos, Quaternion.identity);
            
            // Set up the friendly dragon
            FriendlyDragon fd = dragon.GetComponent<FriendlyDragon>();
            if (fd != null)
            {
                fd.damage = heroData.ultimateDamage;
                fd.lifetime = 10f; // Dragon lasts 10 seconds
            }
        }
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
