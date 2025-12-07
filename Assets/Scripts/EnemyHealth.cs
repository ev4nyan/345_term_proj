using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    public static event Action OnEnemyDied;

    public int maxHealth = 20;
    public int essenceReward = 5;

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
            EssenceManager.Instance.Add(essenceReward);
            OnEnemyDied?.Invoke();
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
