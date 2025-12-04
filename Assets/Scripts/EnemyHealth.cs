using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 20;
    int currentHealth;
    Animator animator;

    // Event for resolve gain on kill
    public static event Action OnEnemyDied;

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
            // Notify listeners (ResolveManager)
            OnEnemyDied?.Invoke();
            // optional: play death animation, then destroy
            // animator.SetTrigger("Die");
            Destroy(gameObject);
        }
        else
        {
            // optional hurt animation
            if (animator != null)
                animator.SetTrigger("Hurt");
        }
    }
}
