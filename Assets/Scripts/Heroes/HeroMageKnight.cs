using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// MageKnight hero - Melee attacks that chain lightning to nearby enemies.
/// Ultimate: Thunder God - Calls down lightning on all enemies, chaining between them.
/// </summary>
public class HeroMageKnight : Hero
{
    [Header("Melee Attack")]
    public Vector2 attackOffset;
    public AudioClip attackClip;

    [Header("Chain Lightning")]
    public int maxChainTargets = 3;
    public float chainRange = 4f;
    public float chainDamageMultiplier = 0.7f;  // Each chain does 70% of previous
    public GameObject lightningEffectPrefab;
    public Color lightningColor = Color.cyan;

    [Header("Ultimate")]
    public AudioClip ultClip;

    private AudioSource audioSource;

    protected override void Start()
    {
        base.Start();
        audioSource = FindFirstObjectByType<Canvas>()?.GetComponent<AudioSource>();
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        // Melee attack with chain lightning
        if (Time.time >= nextAttackTime)
        {
            Vector2 center = (Vector2)transform.position + GetAttackOffset();
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, heroData.attackRange, enemyLayer);

            if (hits.Length > 0)
            {
                nextAttackTime = Time.time + (1f / heroData.attackRate);

                if (animator != null)
                    animator.SetTrigger("Atk");

                if (audioSource != null && attackClip != null)
                    audioSource.PlayOneShot(attackClip);

                // Hit primary target
                Collider2D primaryTarget = hits[0];
                DealDamageToTarget(primaryTarget.gameObject, heroData.attackDamage);
                target = primaryTarget.transform;

                // Spawn lightning from hero to primary target
                SpawnLightningEffect(transform.position, primaryTarget.transform.position);

                // Chain lightning from primary target
                ChainLightning(primaryTarget.transform, heroData.attackDamage);
            }
            else
            {
                target = null;
            }
        }
    }

    void ChainLightning(Transform startTarget, float baseDamage)
    {
        List<Transform> hitTargets = new List<Transform> { startTarget };
        Transform currentTarget = startTarget;
        float currentDamage = baseDamage;

        for (int i = 0; i < maxChainTargets; i++)
        {
            // Find next closest enemy that hasn't been hit
            Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(currentTarget.position, chainRange, enemyLayer);
            Transform nextTarget = null;
            float closestDist = Mathf.Infinity;

            foreach (var enemy in nearbyEnemies)
            {
                if (hitTargets.Contains(enemy.transform)) continue;

                float dist = Vector2.Distance(currentTarget.position, enemy.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    nextTarget = enemy.transform;
                }
            }

            if (nextTarget == null) break;

            // Calculate chain damage
            currentDamage *= chainDamageMultiplier;

            // Spawn lightning effect between targets
            SpawnLightningEffect(currentTarget.position, nextTarget.position);

            // Deal damage
            DealDamageToTarget(nextTarget.gameObject, currentDamage);

            hitTargets.Add(nextTarget);
            currentTarget = nextTarget;
        }
    }

    void SpawnLightningEffect(Vector3 from, Vector3 to)
    {
        if (lightningEffectPrefab != null)
        {
            Vector3 midpoint = (from + to) / 2f;
            midpoint.z = -1f;  // Spawn in front of terrain

            GameObject effect = Instantiate(lightningEffectPrefab, midpoint, Quaternion.identity);
            
            // Scale and rotate to connect points
            float distance = Vector3.Distance(from, to);
            Vector3 dir = (to - from).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            
            effect.transform.rotation = Quaternion.Euler(0, 0, angle);
            effect.transform.localScale = new Vector3(distance, 0.2f, 1f);
            
            Destroy(effect, 1f);
        }
        else
        {
            // Draw debug line
            Debug.DrawLine(from, to, lightningColor, 0.2f);
        }
    }

    Vector2 GetAttackOffset()
    {
        return new Vector2(facingRight ? attackOffset.x : -attackOffset.x, attackOffset.y);
    }

    // Ultimate: Thunder God - massive chain lightning to all enemies
    protected override void ExecuteUltimate()
    {
        Debug.Log($"{heroData.heroName} becomes the THUNDER GOD!");

        if (audioSource != null && ultClip != null)
            audioSource.PlayOneShot(ultClip);

        StartCoroutine(ThunderGod());
    }

    System.Collections.IEnumerator ThunderGod()
    {
        // Store values before hero potentially dies
        Vector3 heroPos = transform.position;
        float ultRadius = heroData.ultimateRadius;
        float damagePerEnemy = heroData.ultimateDamage;
        
        Collider2D[] allEnemies = Physics2D.OverlapCircleAll(heroPos, ultRadius, enemyLayer);
        
        if (allEnemies.Length == 0) yield break;

        List<Transform> enemies = new List<Transform>();

        foreach (var e in allEnemies)
        {
            enemies.Add(e.transform);
        }

        // Strike from hero to first enemy
        if (enemies.Count > 0 && enemies[0] != null)
        {
            SpawnLightningEffect(heroPos, enemies[0].position);
            EnemyHealth eh = enemies[0].GetComponent<EnemyHealth>();
            if (eh != null) eh.TakeDamage((int)damagePerEnemy);
        }

        yield return new WaitForSeconds(0.05f);

        // Chain between all enemies
        for (int i = 0; i < enemies.Count - 1; i++)
        {
            if (enemies[i] == null || enemies[i + 1] == null) continue;

            SpawnLightningEffect(enemies[i].position, enemies[i + 1].position);
            
            // Deal damage directly since hero may be dead
            EnemyHealth eh = enemies[i + 1].GetComponent<EnemyHealth>();
            if (eh != null) eh.TakeDamage((int)damagePerEnemy);

            yield return new WaitForSeconds(0.05f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Attack range
        Gizmos.color = Color.yellow;
        Vector2 center = (Vector2)transform.position + GetAttackOffset();
        float range = heroData != null ? heroData.attackRange : 1.5f;
        Gizmos.DrawWireSphere(center, range);

        // Chain range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chainRange);

        // Ultimate range
        if (heroData != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, heroData.ultimateRadius);
        }
    }
}
