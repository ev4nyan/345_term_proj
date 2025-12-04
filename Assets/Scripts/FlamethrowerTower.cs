using UnityEngine;

public class FlamethrowerTower : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 150;
    public float attackDamage = 5f;

    [Header("Cone Attack")]
    public float coneRange = 4f;           // how far the flame reaches
    public float coneAngle = 45f;          // half-angle of the cone (total cone is 2x this)
    public Vector2 attackOffset;           // offset from tower position
    public float attackCooldown = 0.2f;    // rapid fire damage ticks
    public LayerMask enemyLayer;

    [Header("Rotation")]
    public float rotationSpeed = 10f;
    public Transform partToRotate;         // part of tower that rotates to aim
    public string enemyTag = "Enemy";

    [Header("Visual")]
    public Color coneGizmoColor = new Color(1f, 0.5f, 0f, 0.3f); // orange with transparency
    public Color coneOutlineColor = Color.red;

    int currentHealth;
    float nextAttackTime = 0f;
    Animator animator;
    bool isDead = false;
    Transform target;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        if (partToRotate == null)
        {
            partToRotate = transform;
        }

        InvokeRepeating("UpdateTarget", 0f, 0.25f);
    }

    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy < shortestDistance && distanceToEnemy <= coneRange)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }

    void Update()
    {
        if (isDead) return;

        // Rotate towards target
        if (target != null)
        {
            Vector2 direction = (Vector2)target.position - (Vector2)partToRotate.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle - 90f);
            partToRotate.rotation = Quaternion.Lerp(partToRotate.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Attack all enemies in cone
        if (Time.time >= nextAttackTime)
        {
            AttackEnemiesInCone();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void AttackEnemiesInCone()
    {
        Vector2 origin = (Vector2)transform.position + attackOffset;
        
        // Get the direction the tower is facing (right direction in 2D after rotation - 90 degrees clockwise from up)
        Vector2 forwardDir = partToRotate.right;

        // Find all enemies in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, coneRange, enemyLayer);

        bool hitAny = false;

        foreach (var hit in hits)
        {
            Vector2 dirToEnemy = ((Vector2)hit.transform.position - origin).normalized;
            float angleToEnemy = Vector2.Angle(forwardDir, dirToEnemy);

            // Check if enemy is within the cone angle
            if (angleToEnemy <= coneAngle)
            {
                hitAny = true;

                // Try EnemyHealth first, then Enemy
                EnemyHealth eh = hit.GetComponent<EnemyHealth>();
                if (eh != null)
                {
                    eh.TakeDamage((int)attackDamage);
                }
                else
                {
                    Enemy enemy = hit.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(attackDamage);
                    }
                }
            }
        }

        // Trigger attack animation if we hit something
        if (hitAny && animator != null)
        {
            animator.SetTrigger("Atk");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            if (animator != null)
                animator.SetTrigger("Hit");
        }
        else
        {
            currentHealth = 0;
            isDead = true;
            if (animator != null)
                animator.SetTrigger("Die");
            
            // Destroy after a delay to allow death animation
            Destroy(gameObject, 1f);
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    void OnDrawGizmosSelected()
    {
        Vector2 origin = (Vector2)transform.position + attackOffset;
        Vector2 forwardDir = partToRotate != null ? (Vector2)partToRotate.right : Vector2.right;

        // Draw cone outline
        Gizmos.color = coneOutlineColor;

        // Calculate cone edges
        float leftAngle = Mathf.Atan2(forwardDir.y, forwardDir.x) * Mathf.Rad2Deg + coneAngle;
        float rightAngle = Mathf.Atan2(forwardDir.y, forwardDir.x) * Mathf.Rad2Deg - coneAngle;

        Vector2 leftDir = new Vector2(Mathf.Cos(leftAngle * Mathf.Deg2Rad), Mathf.Sin(leftAngle * Mathf.Deg2Rad));
        Vector2 rightDir = new Vector2(Mathf.Cos(rightAngle * Mathf.Deg2Rad), Mathf.Sin(rightAngle * Mathf.Deg2Rad));

        Vector3 leftEnd = (Vector3)(origin + leftDir * coneRange);
        Vector3 rightEnd = (Vector3)(origin + rightDir * coneRange);
        Vector3 originV3 = (Vector3)origin;

        // Draw the two edge lines
        Gizmos.DrawLine(originV3, leftEnd);
        Gizmos.DrawLine(originV3, rightEnd);

        // Draw arc segments
        int segments = 20;
        float angleStep = (coneAngle * 2f) / segments;
        float startAngle = rightAngle;

        for (int i = 0; i < segments; i++)
        {
            float a1 = (startAngle + angleStep * i) * Mathf.Deg2Rad;
            float a2 = (startAngle + angleStep * (i + 1)) * Mathf.Deg2Rad;

            Vector3 p1 = originV3 + new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0) * coneRange;
            Vector3 p2 = originV3 + new Vector3(Mathf.Cos(a2), Mathf.Sin(a2), 0) * coneRange;

            Gizmos.DrawLine(p1, p2);
        }

        // Draw filled cone using Handles (only in editor)
#if UNITY_EDITOR
        UnityEditor.Handles.color = coneGizmoColor;
        
        // Draw filled arc
        Vector3[] arcPoints = new Vector3[segments + 2];
        arcPoints[0] = originV3;
        
        for (int i = 0; i <= segments; i++)
        {
            float a = (startAngle + angleStep * i) * Mathf.Deg2Rad;
            arcPoints[i + 1] = originV3 + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * coneRange;
        }
        
        UnityEditor.Handles.DrawAAConvexPolygon(arcPoints);
#endif

        // Draw range circle for reference
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(origin, coneRange);
    }
}
