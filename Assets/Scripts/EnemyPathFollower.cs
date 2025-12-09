using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathFollower : MonoBehaviour
{
    [Header("Corner Waypoints (big path points)")]
    public Transform[] waypoints;

    [Header("Hop Settings")]
    public float hopDistance = 0.5f;   // world units per jump
    public float startupDuration = 0.9f;  // length of the jump-start clip
    public float jumpDuration = 0.5f;  // time per jump
    public float idleDuration = 0.15f;
    public float jumpHeight = 0.25f;

    [Header("Hero Attack")]
    public float heroDetectionRange = 8f;
    public float attackRange = 1.5f;
    public int attackDamage = 10;
    public float attackRate = 1f;
    public string heroTag = "Hero";

    [Header("End of Path Damage")]
    public int endOfPathDamage = 10;

    private List<Vector3> hopPoints = new List<Vector3>();
    private Animator animator;
    private Transform targetHero;
    private bool isAttackingHero = false;
    private Coroutine currentBehavior;
    private int currentHopIndex = 0;

    void Start()
    {
        animator = GetComponent<Animator>();

        BuildHopPoints();
        if (hopPoints.Count > 0)
            transform.position = hopPoints[0];

        currentBehavior = StartCoroutine(FollowHops());
    }

    void Update()
    {
        // Continuously check for heroes
        DetectHeroes();
    }

    void DetectHeroes()
    {
        GameObject[] heroes = GameObject.FindGameObjectsWithTag(heroTag);
        float closestDistance = Mathf.Infinity;
        Transform closestHero = null;

        foreach (GameObject hero in heroes)
        {
            Hero h = hero.GetComponent<Hero>();
            if (h != null && h.isDead) continue;

            float distance = Vector2.Distance(transform.position, hero.transform.position);
            if (distance < heroDetectionRange && distance < closestDistance)
            {
                closestDistance = distance;
                closestHero = hero.transform;
            }
        }

        // Switch to attacking if we found a hero and aren't already attacking
        if (closestHero != null && !isAttackingHero)
        {
            targetHero = closestHero;
            isAttackingHero = true;
            if (currentBehavior != null)
                StopCoroutine(currentBehavior);
            currentBehavior = StartCoroutine(AttackHeroBehavior());
        }
        // If we were attacking but hero is gone, resume path
        else if (closestHero == null && isAttackingHero)
        {
            isAttackingHero = false;
            targetHero = null;
            if (currentBehavior != null)
                StopCoroutine(currentBehavior);
            currentBehavior = StartCoroutine(FollowHops());
        }
    }

    IEnumerator AttackHeroBehavior()
    {
        float attackCooldown = 0f;

        while (isAttackingHero && targetHero != null)
        {
            Hero h = targetHero.GetComponent<Hero>();
            if (h != null && h.isDead)
            {
                isAttackingHero = false;
                targetHero = null;
                currentBehavior = StartCoroutine(FollowHops());
                yield break;
            }

            float distanceToHero = Vector2.Distance(transform.position, targetHero.position);

            if (distanceToHero > attackRange)
            {
                // Hop toward the hero
                Vector3 dir = (targetHero.position - transform.position).normalized;
                Vector3 hopTarget = transform.position + dir * hopDistance;

                // Face movement direction
                if (dir.x != 0f)
                {
                    var s = transform.localScale;
                    s.x = Mathf.Sign(dir.x) * Mathf.Abs(s.x);
                    transform.localScale = s;
                }

                // Play jump animation
                if (animator != null)
                {
                    animator.ResetTrigger("DoJump");
                    animator.SetTrigger("DoJump");
                }
                yield return new WaitForSeconds(startupDuration);

                // Hop toward hero
                Vector3 start = transform.position;
                float t = 0f;
                while (t < jumpDuration)
                {
                    t += Time.deltaTime;
                    float n = Mathf.Clamp01(t / jumpDuration);
                    Vector3 pos = Vector3.Lerp(start, hopTarget, n);
                    float height = 4f * jumpHeight * n * (1f - n);
                    pos.y += height;
                    transform.position = pos;
                    yield return null;
                }

                yield return new WaitForSeconds(idleDuration);
            }
            else
            {
                // In attack range - deal damage to hero
                if (attackCooldown <= 0f)
                {
                    h?.TakeDamage(attackDamage);
                    attackCooldown = 1f / attackRate;

                    // Play attack animation if available
                    if (animator != null)
                    {
                        animator.ResetTrigger("DoJump");
                        animator.SetTrigger("DoJump");
                    }
                }
                attackCooldown -= Time.deltaTime;
                yield return null;
            }
        }

        // Hero destroyed or out of range, resume path
        isAttackingHero = false;
        currentBehavior = StartCoroutine(FollowHops());
    }

    // Build evenly spaced hop points along the path
    void BuildHopPoints()
    {
        hopPoints.Clear();
        if (waypoints == null || waypoints.Length == 0) return;

        Vector3 current = waypoints[0].position;
        hopPoints.Add(current);

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Vector3 start = current;
            Vector3 end = waypoints[i + 1].position;

            Vector3 segDir = (end - start).normalized;
            float segRemaining = Vector3.Distance(start, end);

            // march along this segment at hopDistance steps
            while (segRemaining >= hopDistance)
            {
                current += segDir * hopDistance;
                hopPoints.Add(current);
                segRemaining -= hopDistance;
            }

            // snap exactly to the corner waypoint
            current = end;
            hopPoints.Add(current);
        }
    }

    IEnumerator FollowHops()
    {
        // Find closest hop point to current position (for resuming after tower attack)
        if (currentHopIndex == 0 && hopPoints.Count > 0)
        {
            float closestDist = Mathf.Infinity;
            for (int i = 0; i < hopPoints.Count; i++)
            {
                float dist = Vector3.Distance(transform.position, hopPoints[i]);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    currentHopIndex = i;
                }
            }
        }

        while (currentHopIndex < hopPoints.Count - 1)
        {
            Vector3 start = hopPoints[currentHopIndex];
            Vector3 end = hopPoints[currentHopIndex + 1];

            // face movement direction (left/right)
            Vector3 dir = end - start;
            if (dir.x != 0f)
            {
                var s = transform.localScale;
                s.x = Mathf.Sign(dir.x) * Mathf.Abs(s.x);
                transform.localScale = s;
            }

            // fire the jump animation chain
            if (animator != null)
            {
                animator.ResetTrigger("DoJump");
                animator.SetTrigger("DoJump");
            }
            // wait for the startup animation time (0.9 s)
            yield return new WaitForSeconds(startupDuration);

            float t = 0f;
            while (t < jumpDuration)
            {
                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / jumpDuration);

                Vector3 pos = Vector3.Lerp(start, end, n);
                float height = 4f * jumpHeight * n * (1f - n); // nice parabola
                pos.y += height;

                transform.position = pos;
                yield return null;
            }

            currentHopIndex++;

            // little idle pause on each tile
            yield return new WaitForSeconds(idleDuration);
        }

        // reached the end of the path - deal damage (deduct resolve)
        if (ResolveManager.Instance != null)
        {
            ResolveManager.Instance.SpendResolve(endOfPathDamage);
            Debug.Log($"Enemy reached the end! Lost {endOfPathDamage} resolve.");
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // Draw hero detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, heroDetectionRange);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
