using UnityEngine;

/// <summary>
/// Mage hero - Cone attacks like FlamethrowerTower.
/// Damages all enemies in a cone in front of the mage.
/// </summary>
public class HeroMage : Hero
{
    [Header("Mage Cone Attack")]
    public float coneAngle = 45f;          // Half-angle of the cone
    public Vector2 attackOffset;

    [Header("Visual")]
    public Color coneGizmoColor = new Color(0.5f, 0f, 1f, 0.3f); // Purple
    public Color coneOutlineColor = Color.magenta;

    protected override void Start()
    {
        base.Start();
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.25f);
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        // Attack all enemies in cone
        if (Time.time >= nextAttackTime)
        {
            AttackEnemiesInCone();
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

    void AttackEnemiesInCone()
    {
        Vector2 origin = (Vector2)transform.position + GetAttackOffset();

        // Forward direction based on facing
        Vector2 forwardDir = facingRight ? Vector2.right : Vector2.left;

        // Find all enemies in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, heroData.attackRange, enemyLayer);

        bool hitAny = false;

        foreach (var hit in hits)
        {
            Vector2 dirToEnemy = ((Vector2)hit.transform.position - origin).normalized;
            float angleToEnemy = Vector2.Angle(forwardDir, dirToEnemy);

            // Check if enemy is within the cone angle
            if (angleToEnemy <= coneAngle)
            {
                hitAny = true;
                DealDamageToTarget(hit.gameObject, heroData.attackDamage);

                // Track target for flipping
                if (target == null)
                    target = hit.transform;
            }
        }

        // Trigger attack animation if we hit something
        if (hitAny && animator != null)
        {
            animator.SetTrigger("Atk");
        }
    }

    Vector2 GetAttackOffset()
    {
        return new Vector2(facingRight ? attackOffset.x : -attackOffset.x, attackOffset.y);
    }

    void OnDrawGizmosSelected()
    {
        if (heroData == null) return;

        Vector2 origin = (Vector2)transform.position + GetAttackOffset();
        Vector2 forwardDir = facingRight ? Vector2.right : Vector2.left;

        // Draw cone outline
        Gizmos.color = coneOutlineColor;

        // Calculate cone edges
        float baseAngle = facingRight ? 0f : 180f;
        float leftAngle = baseAngle + coneAngle;
        float rightAngle = baseAngle - coneAngle;

        Vector2 leftDir = new Vector2(Mathf.Cos(leftAngle * Mathf.Deg2Rad), Mathf.Sin(leftAngle * Mathf.Deg2Rad));
        Vector2 rightDir = new Vector2(Mathf.Cos(rightAngle * Mathf.Deg2Rad), Mathf.Sin(rightAngle * Mathf.Deg2Rad));

        Vector3 leftEnd = (Vector3)(origin + leftDir * heroData.attackRange);
        Vector3 rightEnd = (Vector3)(origin + rightDir * heroData.attackRange);
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

            Vector3 p1 = originV3 + new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0) * heroData.attackRange;
            Vector3 p2 = originV3 + new Vector3(Mathf.Cos(a2), Mathf.Sin(a2), 0) * heroData.attackRange;

            Gizmos.DrawLine(p1, p2);
        }

        // Draw ultimate range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, heroData.ultimateRadius);
    }
}
