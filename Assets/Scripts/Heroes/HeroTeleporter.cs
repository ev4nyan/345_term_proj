using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Teleporter hero - Teleports to random enemy every few seconds and melee attacks.
/// Ultimate: Teleport Strike - Rapidly teleports to and damages ALL enemies on the field.
/// </summary>
public class HeroTeleporter : Hero
{
    [Header("Teleport Settings")]
    public float teleportCooldown = 5f;
    public float teleportRange = 15f;  // Max range to detect enemies
    public GameObject teleportEffectPrefab;
    public AudioClip teleportClip;

    [Header("Melee Attack")]
    public Vector2 attackOffset;
    public AudioClip attackClip;

    [Header("Grid Snapping")]
    [SerializeField] private Grid grid;

    private float nextTeleportTime = 0f;
    private AudioSource audioSource;

    protected override void Start()
    {
        base.Start();
        nextTeleportTime = Time.time + teleportCooldown;
        audioSource = FindFirstObjectByType<Canvas>()?.GetComponent<AudioSource>();
        
        // Try to find grid if not assigned
        if (grid == null)
            grid = FindFirstObjectByType<Grid>();
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        // Handle teleporting
        if (Time.time >= nextTeleportTime)
        {
            TryTeleportToEnemy();
        }

        // Melee attack enemies in range
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

                foreach (var hit in hits)
                {
                    DealDamageToTarget(hit.gameObject, heroData.attackDamage);

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

    void TryTeleportToEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return;

        // Filter enemies within teleport range
        List<GameObject> validEnemies = new List<GameObject>();
        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist <= teleportRange)
            {
                validEnemies.Add(enemy);
            }
        }

        if (validEnemies.Count == 0) return;

        // Pick random enemy from valid targets
        GameObject randomEnemy = validEnemies[Random.Range(0, validEnemies.Count)];
        TeleportNear(randomEnemy.transform.position);
        nextTeleportTime = Time.time + teleportCooldown;
    }

    void TeleportNear(Vector3 targetPos)
    {
        animator.SetTrigger("Teleport");
        // Calculate position near enemy (offset so we don't land on them)
        Vector3 dir = (transform.position - targetPos).normalized;
        Vector3 teleportPos = targetPos + dir * 1f;

        // Snap to grid
        teleportPos = SnapToGrid(teleportPos);

        // Spawn effect at old position
        if (teleportEffectPrefab != null)
        {
            GameObject effect = Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        // Play sound
        if (audioSource != null && teleportClip != null)
            audioSource.PlayOneShot(teleportClip);

        // Teleport
        transform.position = teleportPos;

        // Spawn effect at new position
        if (teleportEffectPrefab != null)
        {
            GameObject effect = Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        Debug.Log($"{heroData?.heroName} teleported!");
    }

    Vector3 SnapToGrid(Vector3 worldPos)
    {
        if (grid == null)
            return worldPos;

        Vector3Int cell = grid.WorldToCell(worldPos);
        Vector3 snapped = grid.GetCellCenterWorld(cell);
        snapped.z = 0f;
        return snapped;
    }

    Vector2 GetAttackOffset()
    {
        return new Vector2(facingRight ? attackOffset.x : -attackOffset.x, attackOffset.y);
    }

    // Ultimate: Teleport Strike - rapidly teleport to and damage all enemies
    protected override void ExecuteUltimate()
    {
        Debug.Log($"{heroData.heroName} unleashes TELEPORT STRIKE!");
        StartCoroutine(TeleportStrike());
    }

    System.Collections.IEnumerator TeleportStrike()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || isDead) continue;

            // Teleport to enemy
            Vector3 teleportPos = SnapToGrid(enemy.transform.position);
            
            if (teleportEffectPrefab != null)
            {
                GameObject effect = Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 0.5f);
            }

            transform.position = teleportPos;

            if (audioSource != null && teleportClip != null)
                audioSource.PlayOneShot(teleportClip);

            // Deal ultimate damage
            DealDamageToTarget(enemy, heroData.ultimateDamage / enemies.Length);

            if (animator != null)
                animator.SetTrigger("Atk");

            yield return new WaitForSeconds(0.15f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Attack range
        Gizmos.color = Color.yellow;
        Vector2 center = (Vector2)transform.position + GetAttackOffset();
        float range = heroData != null ? heroData.attackRange : 1.2f;
        Gizmos.DrawWireSphere(center, range);

        // Teleport range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, teleportRange);

        // Ultimate range
        if (heroData != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, heroData.ultimateRadius);
        }
    }
}
