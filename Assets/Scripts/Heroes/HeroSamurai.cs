using UnityEngine;

/// <summary>
/// Samurai hero - Fires wind slash projectiles that push enemies back.
/// Ultimate: Thousand Cuts - Unleashes a barrage of slashes that push all enemies to the start.
/// </summary>
public class HeroSamurai : Hero
{
    [Header("Samurai Projectile")]
    public GameObject windSlashPrefab;
    public Transform firePoint;
    public float projectileSpeed = 12f;
    public float pushForce = 3f;
    public AudioClip attackClip;

    [Header("Ultimate")]
    public AudioClip ultClip;

    private AudioSource audioSource;

    protected override void Start()
    {
        base.Start();
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.25f);
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
            FireWindSlash();
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

    void FireWindSlash()
    {
        if (target == null) return;

        if (animator != null)
            animator.SetTrigger("Atk");

        if (audioSource != null && attackClip != null)
            audioSource.PlayOneShot(attackClip);

        Vector3 spawnPos = firePoint.position;

        if (windSlashPrefab != null)
        {
            GameObject proj = Instantiate(windSlashPrefab, spawnPos, Quaternion.identity);
            
            WindSlashProjectile slash = proj.GetComponent<WindSlashProjectile>();
            if (slash != null)
            {
                slash.Init(target, heroData.attackDamage, pushForce, projectileSpeed);
            }
            else
            {
                // Fallback: add component if not present
                slash = proj.AddComponent<WindSlashProjectile>();
                slash.Init(target, heroData.attackDamage, pushForce, projectileSpeed);
            }
        }
        else
        {
            // Direct damage fallback
            DealDamageToTarget(target.gameObject, heroData.attackDamage);
        }
    }

    // Ultimate: Thousand Cuts - push all enemies back significantly
    protected override void ExecuteUltimate()
    {
        Debug.Log($"{heroData.heroName} unleashes THOUSAND CUTS!");

        if (audioSource != null && ultClip != null)
            audioSource.PlayOneShot(ultClip);

        StartCoroutine(ThousandCuts());
    }

    System.Collections.IEnumerator ThousandCuts()
    {
        int slashCount = 10;
        float damagePerSlash = heroData.ultimateDamage / slashCount;

        for (int i = 0; i < slashCount; i++)
        {
            if (isDead) yield break;

            // Damage and push all enemies
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, heroData.ultimateRadius, enemyLayer);
            
            foreach (var enemy in enemies)
            {
                DealDamageToTarget(enemy.gameObject, damagePerSlash);

                // Strong pushback
                Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 pushDir = (enemy.transform.position - transform.position).normalized;
                    rb.AddForce(-pushDir * pushForce * 5f, ForceMode2D.Impulse);
                }
            }

            if (animator != null)
                animator.SetTrigger("Atk");

            yield return new WaitForSeconds(0.1f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Attack range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, heroData != null ? heroData.attackRange : 6f);

        // Ultimate range
        if (heroData != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, heroData.ultimateRadius);
        }
    }
}
