using UnityEngine;

public class KingController : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public int attackDamage = 10;

    [Header("Attack")]
    public float attackRange = 1.2f;
    public Vector2 attackOffset;       // where the circle is, relative to King
    public float attackCooldown = 0.8f;
    public LayerMask enemyLayer;

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
            // center attack area a bit in front of the king
            Vector2 center = (Vector2)transform.position + attackOffset;

            Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRange, enemyLayer);

            if (hits.Length > 0)
            {
                nextAttackTime = Time.time + attackCooldown;

                // play attack animation
                animator.SetTrigger("Atk");

                // deal damage (for now, do it instantly)
                foreach (var h in hits)
                {
                    EnemyHealth eh = h.GetComponent<EnemyHealth>();
                    if (eh != null)
                        eh.TakeDamage(attackDamage);
                }
            }
        }
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
            // TODO: game over logic
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 center = (Vector2)transform.position + attackOffset;
        Gizmos.DrawWireSphere(center, attackRange);
    }
}
