using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 20;
    int currentHealth;
    Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // optional: play death animation, then destroy
            // animator.SetTrigger("Die");
            Destroy(gameObject);
        }
        else
        {
            // optional hurt animation
            animator.SetTrigger("Hurt");
        }
    }
}
