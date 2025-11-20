using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWizardPathShooter : MonoBehaviour
{
    [Header("Path (corner waypoints)")]
    public Transform[] waypoints;       // assign from spawner

    [Header("Movement")]
    public float stepDistance = 0.5f;   // how far each "walk chunk" is
    public float moveSpeed = 2f;        // units per second
    public float pauseAfterStep = 0.2f; // small pause before attacking

    [Header("Attack")]
    public float attackRange = 4f;
    public Vector2 attackOffset;        // where to search, relative to wizard
    public float attackCooldown = 1.5f;
    public int attackDamage = 10;
    public float attackWindup = 0.4f;   // time from start of Attack anim to impact
    public LayerMask towerLayer;        // set to "Tower" layer

    [Header("Health")]
    public int maxHealth = 50;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint; // where the spell appears (e.g. staff tip)

    private int currentHealth;
    private Animator animator;
    private bool isDead = false;
    private float nextAttackTime = 0f;

    private List<Vector3> walkPoints = new List<Vector3>();

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        BuildWalkPoints();
        if (walkPoints.Count > 0)
            transform.position = walkPoints[0];

        StartCoroutine(MoveAndAttackLoop());
    }

    void BuildWalkPoints()
    {
        walkPoints.Clear();
        if (waypoints == null || waypoints.Length == 0) return;

        Vector3 current = waypoints[0].position;
        walkPoints.Add(current);

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Vector3 start = current;
            Vector3 end = waypoints[i + 1].position;

            Vector3 dir = (end - start).normalized;
            float remaining = Vector3.Distance(start, end);

            while (remaining >= stepDistance)
            {
                current += dir * stepDistance;
                walkPoints.Add(current);
                remaining -= stepDistance;
            }

            // snap to the exact corner point
            current = end;
            walkPoints.Add(current);
        }
    }

    IEnumerator MoveAndAttackLoop()
    {
        int index = 0;

        while (!isDead && index < walkPoints.Count - 1)
        {
            // 1) Walk one step along the path
            Vector3 start = walkPoints[index];
            Vector3 end = walkPoints[index + 1];

            Vector3 dir = end - start;

            // flip to face direction of travel
            if (dir.x != 0f)
            {
                var s = transform.localScale;
                s.x = Mathf.Sign(dir.x) * Mathf.Abs(s.x);
                transform.localScale = s;
            }

            animator.SetBool("IsWalking", true);

            float dist = Vector3.Distance(start, end);
            float t = 0f;
            float duration = dist / moveSpeed;

            while (t < duration && !isDead)
            {
                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / duration);
                transform.position = Vector3.Lerp(start, end, n);
                yield return null;
            }

            animator.SetBool("IsWalking", false);
            index++;

            // 2) Small pause before deciding to attack
            if (pauseAfterStep > 0f)
                yield return new WaitForSeconds(pauseAfterStep);

            // 3) Try to attack nearest tower (if off cooldown and in range)
            if (Time.time >= nextAttackTime && !isDead)
            {
                Transform targetTransform;
                IDamageable target = FindClosestTargetInRange(out targetTransform);
                if (target != null)
                {
                    nextAttackTime = Time.time + attackCooldown;

                    // face tower
                    Vector3 tdir = targetTransform.position - transform.position;
                    if (tdir.x != 0f)
                    {
                        var s = transform.localScale;
                        s.x = Mathf.Sign(tdir.x) * Mathf.Abs(s.x);
                        transform.localScale = s;
                    }

                    // play attack anim
                    animator.SetTrigger("Atk");

                    // wait for windup so damage / projectile lines up with animation
                    yield return new WaitForSeconds(attackWindup);

                    if (target != null && targetTransform != null)
                    {
                        // direction from spawn point to target
                        Vector2 spawnPos = projectileSpawnPoint != null
                            ? (Vector2)projectileSpawnPoint.position
                            : (Vector2)transform.position;

                        Vector2 direc = (Vector2)targetTransform.position - spawnPos;

                        // spawn projectile
                        GameObject projGO = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

                        MagicProjectile proj = projGO.GetComponent<MagicProjectile>();
                        if (proj != null)
                        {
                            proj.Init(direc, attackDamage, towerLayer);
                        }
                    }

                    // wait for remainder of attack anim here
                }
            }
        }

        if (!isDead)
        {
            // reached end of path: damage  base / king / etc. here
            //FindFirstObjectByType<KingController>()?.TakeDamage(10);
            Destroy(gameObject);
        }
    }

    IDamageable FindClosestTargetInRange(out Transform targetTransform)
    {
        targetTransform = null;

        Vector2 center = (Vector2)transform.position + attackOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRange, towerLayer);

        IDamageable best = null;
        float bestDistSq = Mathf.Infinity;

        foreach (var h in hits)
        {
            IDamageable dmg = h.GetComponent<IDamageable>();
            if (dmg == null) continue;

            float d2 = ((Vector2)h.transform.position - center).sqrMagnitude;
            if (d2 < bestDistSq)
            {
                bestDistSq = d2;
                best = dmg;
                targetTransform = h.transform;
            }
        }

        return best;
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        if (currentHealth > 0)
        {
            animator.SetTrigger("Hit");
        }
        else
        {
            isDead = true;
            animator.SetTrigger("Die");
            StopAllCoroutines();
            // destroy after death animation length (tweak as needed)
            Destroy(gameObject, 1.0f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Vector2 center = (Vector2)transform.position + attackOffset;
        Gizmos.DrawWireSphere(center, attackRange);
    }
}
