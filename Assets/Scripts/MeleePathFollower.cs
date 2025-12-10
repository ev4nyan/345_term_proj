using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Path follower for melee enemies. Walks smoothly along waypoints
/// and attacks heroes when in range.
/// </summary>
public class MeleePathFollower : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Melee Attack")]
    public float heroDetectionRange = 5f;
    public float attackRange = 1.2f;
    public int attackDamage = 10;
    public float attackRate = 1f;
    public string heroTag = "Hero";

    [Header("End of Path Damage")]
    public int endOfPathDamage = 10;

    private Animator animator;
    private int currentWaypointIndex = 0;
    private Transform targetHero;
    private bool isAttackingHero = false;
    private float nextAttackTime = 0f;
    private Coroutine currentBehavior;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (waypoints != null && waypoints.Length > 0)
            transform.position = waypoints[0].position;

        currentBehavior = StartCoroutine(FollowPath());
    }

    void Update()
    {
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

        // Switch to attacking if we found a hero
        if (closestHero != null && !isAttackingHero)
        {
            targetHero = closestHero;
            isAttackingHero = true;
            if (currentBehavior != null)
                StopCoroutine(currentBehavior);
            currentBehavior = StartCoroutine(AttackHeroBehavior());
        }
        // Resume path if hero is gone
        else if (closestHero == null && isAttackingHero)
        {
            isAttackingHero = false;
            targetHero = null;
            if (currentBehavior != null)
                StopCoroutine(currentBehavior);
            currentBehavior = StartCoroutine(FollowPath());
        }
    }

    IEnumerator AttackHeroBehavior()
    {
        while (isAttackingHero && targetHero != null)
        {
            Hero h = targetHero.GetComponent<Hero>();
            if (h != null && h.isDead)
            {
                isAttackingHero = false;
                targetHero = null;
                currentBehavior = StartCoroutine(FollowPath());
                yield break;
            }

            float distanceToHero = Vector2.Distance(transform.position, targetHero.position);
            Vector3 dir = (targetHero.position - transform.position).normalized;

            // Face the hero
            FaceDirection(dir.x);

            if (distanceToHero > attackRange)
            {
                // Walk toward hero
                if (animator != null)
                    animator.SetBool("IsWalking", true);

                transform.position += dir * moveSpeed * Time.deltaTime;
            }
            else
            {
                // In attack range - attack!
                if (animator != null)
                    animator.SetBool("IsWalking", false);

                if (Time.time >= nextAttackTime)
                {
                    if (animator != null)
                        animator.SetTrigger("Atk");

                    h?.TakeDamage(attackDamage);
                    nextAttackTime = Time.time + (1f / attackRate);
                }
            }

            yield return null;
        }

        // Hero gone, resume path
        isAttackingHero = false;
        currentBehavior = StartCoroutine(FollowPath());
    }

    IEnumerator FollowPath()
    {
        // Find closest waypoint to resume from
        if (waypoints == null || waypoints.Length == 0)
        {
            Destroy(gameObject);
            yield break;
        }

        // Find nearest waypoint ahead of current position
        float closestDist = Mathf.Infinity;
        for (int i = currentWaypointIndex; i < waypoints.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, waypoints[i].position);
            if (dist < closestDist)
            {
                closestDist = dist;
                currentWaypointIndex = i;
            }
        }

        while (currentWaypointIndex < waypoints.Length)
        {
            Vector3 targetPos = waypoints[currentWaypointIndex].position;
            Vector3 dir = (targetPos - transform.position).normalized;

            // Face movement direction
            FaceDirection(dir.x);

            // Start walking animation
            if (animator != null)
                animator.SetBool("IsWalking", true);

            // Move toward waypoint
            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                // Recalculate direction in case we got pushed
                dir = (targetPos - transform.position).normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
                yield return null;
            }

            // Snap to waypoint
            transform.position = targetPos;
            currentWaypointIndex++;
        }

        // Stop walking animation
        if (animator != null)
            animator.SetBool("IsWalking", false);

        // Reached end of path - deal damage
        if (ResolveManager.Instance != null)
        {
            ResolveManager.Instance.SpendResolve(endOfPathDamage);
            Debug.Log($"Melee enemy reached the end! Lost {endOfPathDamage} resolve.");
        }
        Destroy(gameObject);
    }

    void FaceDirection(float xDir)
    {
        if (xDir == 0f) return;

        var s = transform.localScale;
        s.x = Mathf.Sign(xDir) * Mathf.Abs(s.x);
        transform.localScale = s;
    }

    void OnDrawGizmosSelected()
    {
        // Hero detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, heroDetectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
