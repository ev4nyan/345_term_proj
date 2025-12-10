using UnityEngine;

/// <summary>
/// Archer hero - Classic ranged attacker with high attack speed.
/// Fires arrows at enemies from long range.
/// Ultimate: Arrow Rain - Rains arrows on all enemies in a large area.
/// </summary>
public class HeroArcher : Hero
{
    [Header("Archer Attack")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float arrowSpeed = 15f;
    public AudioClip attackClip;

    [Header("Ultimate - Arrow Rain")]
    public GameObject arrowRainEffectPrefab;
    public int arrowRainCount = 20;
    public float arrowRainDuration = 2f;
    public AudioClip ultClip;

    private AudioSource audioSource;

    protected override void Start()
    {
        base.Start();
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.2f);
        audioSource = FindFirstObjectByType<Canvas>()?.GetComponent<AudioSource>();

        if (firePoint == null)
            firePoint = transform;
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        if (target != null && Time.time >= nextAttackTime)
        {
            FireArrow();
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

    void FireArrow()
    {
        if (target == null) return;

        if (animator != null)
            animator.SetTrigger("Atk");

        if (audioSource != null && attackClip != null)
            audioSource.PlayOneShot(attackClip);

        Vector3 spawnPos = firePoint.position;

        if (arrowPrefab != null)
        {
            GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);

            // Try Arrow component first
            Arrow arrowScript = arrow.GetComponent<Arrow>();
            if (arrowScript != null)
            {
                arrowScript.Seek(target, heroData.attackDamage, arrowSpeed);
                return;
            }

            // Try Fireball as fallback (same homing behavior)
            Fireball fireball = arrow.GetComponent<Fireball>();
            if (fireball != null)
            {
                fireball.speed = arrowSpeed;
                fireball.Seek(target, heroData.attackDamage);
                return;
            }

            // Last resort: add Arrow component
            arrowScript = arrow.AddComponent<Arrow>();
            arrowScript.Seek(target, heroData.attackDamage, arrowSpeed);
        }
        else
        {
            // Direct damage fallback
            DealDamageToTarget(target.gameObject, heroData.attackDamage);
        }
    }

    // Ultimate: Arrow Rain
    protected override void ExecuteUltimate()
    {
        Debug.Log($"{heroData.heroName} calls down ARROW RAIN!");

        if (audioSource != null && ultClip != null)
            audioSource.PlayOneShot(ultClip);

        StartCoroutine(ArrowRain());
    }

    System.Collections.IEnumerator ArrowRain()
    {
        float damagePerArrow = heroData.ultimateDamage / arrowRainCount;
        float timeBetweenArrows = arrowRainDuration / arrowRainCount;

        // Store position in case hero dies
        Vector3 rainCenter = transform.position;

        // Spawn effect
        if (arrowRainEffectPrefab != null)
        {
            GameObject effect = Instantiate(arrowRainEffectPrefab, rainCenter, Quaternion.identity);
            Destroy(effect, arrowRainDuration + 1f);
        }

        for (int i = 0; i < arrowRainCount; i++)
        {
            // Random position within ultimate radius (use stored center)
            Vector2 randomOffset = Random.insideUnitCircle * heroData.ultimateRadius;
            Vector3 strikePos = rainCenter + (Vector3)randomOffset;

            // Find enemies near strike position
            Collider2D[] hits = Physics2D.OverlapCircleAll(strikePos, 1f, enemyLayer);
            foreach (var hit in hits)
            {
                DealDamageToTarget(hit.gameObject, damagePerArrow);
            }

            // Visual feedback (spawn arrow at strike position)
            if (arrowPrefab != null)
            {
                Vector3 spawnPos = strikePos + Vector3.up * 5f;
                GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.Euler(0, 0, -180f));
                
                // Make it fall down
                Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
                if (rb == null) rb = arrow.AddComponent<Rigidbody2D>();
                rb.gravityScale = 3f;
                rb.freezeRotation = true;  // Keep arrow pointing down
                
                Destroy(arrow, 1f);
            }

            yield return new WaitForSeconds(timeBetweenArrows);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Attack range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, heroData != null ? heroData.attackRange : 8f);

        // Ultimate range
        if (heroData != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, heroData.ultimateRadius);
        }
    }
}
