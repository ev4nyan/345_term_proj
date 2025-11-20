using UnityEngine;

public class WizardController : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 60;
    public int attackDamage = 15;

    [Header("Attack")]
    public float attackRange = 4f;      // how far the wizard can shoot
    public Vector2 attackOffset;        // where the attack area is, relative to wizard
    public float attackCooldown = 1.2f; // time between casts
    public LayerMask enemyLayer;        // set in Inspector

    int currentHealth;
    float nextAttackTime = 0f;
    Animator animator;
    bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isDead) return;

        if (Time.time >= nextAttackTime)
        {
            // center of the detection/cast area
            Vector2 center = (Vector2)transform.position + attackOffset;

            // find all enemies in range
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRange, enemyLayer);

            if (hits.Length > 0)
            {
                // pick the closest enemy
                Collider2D target = GetClosestEnemy(center, hits);

                if (target != null)
                {
                    nextAttackTime = Time.time + attackCooldown;

                    // face the target (left/right)
                    Vector3 dir = target.transform.position - transform.position;
                    if (dir.x != 0f)
                    {
                        var s = transform.localScale;
                        s.x = Mathf.Sign(dir.x) * Mathf.Abs(s.x);
                        transform.localScale = s;
                    }

                    // play attack animation
                    animator.SetTrigger("Atk");

                    // apply damage immediately (for now)
                    EnemyHealth eh = target.GetComponent<EnemyHealth>();
                    if (eh != null)
                        eh.TakeDamage(attackDamage);
                }
            }
        }
    }

    Collider2D GetClosestEnemy(Vector2 center, Collider2D[] hits)
    {
        Collider2D closest = null;
        float minDist = Mathf.Infinity;

        foreach (var h in hits)
        {
            float d = Vector2.SqrMagnitude((Vector2)h.transform.position - center);
            if (d < minDist)
            {
                minDist = d;
                closest = h;
            }
        }

        return closest;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            animator.SetTrigger("Hit");
        }
        else
        {
            currentHealth = 0;
            isDead = true;
            animator.SetTrigger("Die");
            // TODO: remove wizard, show game over, etc.
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector2 center = (Vector2)transform.position + attackOffset;
        Gizmos.DrawWireSphere(center, attackRange);
    }
}
