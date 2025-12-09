using UnityEngine;
using System.Collections;

/// <summary>
/// A friendly dragon summoned by the King's ultimate.
/// Flies around and attacks enemies for a limited time.
/// </summary>
public class FriendlyDragon : MonoBehaviour
{
    [Header("Stats")]
    public float damage = 50f;
    public float attackRange = 6f;
    public float attackRate = 1f;
    public float moveSpeed = 3f;
    public float lifetime = 10f;

    [Header("Visual")]
    public float bobAmplitude = 0.3f;
    public float bobFrequency = 3f;

    [Header("Projectile")]
    public GameObject fireballPrefab;
    public Transform firePoint;

    private Transform target;
    private float nextAttackTime = 0f;
    private float bobOffset;
    private Vector3 basePosition;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        bobOffset = Random.Range(0f, Mathf.PI * 2f);
        basePosition = transform.position;

        if (firePoint == null)
            firePoint = transform;

        // Start targeting
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.25f);

        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move toward target if we have one
        if (target != null)
        {
            float distance = Vector2.Distance(transform.position, target.position);

            if (distance > attackRange * 0.8f)
            {
                // Move toward target
                Vector3 dir = (target.position - transform.position).normalized;
                basePosition += dir * moveSpeed * Time.deltaTime;

                // Face movement direction
                if (dir.x != 0f)
                {
                    var s = transform.localScale;
                    s.x = -Mathf.Sign(dir.x) * Mathf.Abs(s.x);
                    transform.localScale = s;
                }
            }

            // Attack if in range and ready
            if (distance <= attackRange && Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + (1f / attackRate);
            }
        }

        // Apply bobbing
        float bob = Mathf.Sin((Time.time + bobOffset) * bobFrequency) * bobAmplitude;
        Vector3 finalPos = basePosition;
        finalPos.y += bob;
        transform.position = finalPos;
    }

    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        target = nearestEnemy != null ? nearestEnemy.transform : null;
    }

    void Attack()
    {
        if (target == null) return;

        // Play attack animation
        if (animator != null)
            animator.SetTrigger("Atk");

        if (fireballPrefab != null)
        {
            // Shoot fireball
            Vector2 direction = ((Vector2)target.position - (Vector2)firePoint.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
            
            // Try to use Fireball component for seeking
            Fireball fb = fireball.GetComponent<Fireball>();
            if (fb != null)
            {
                fb.Seek(target, damage);
            }
        }
        else
        {
            // Direct damage
            EnemyHealth eh = target.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                eh.TakeDamage((int)damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
