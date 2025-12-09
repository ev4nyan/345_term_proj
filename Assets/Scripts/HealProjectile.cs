using UnityEngine;

/// <summary>
/// Healing projectile that seeks an allied hero and heals them on contact.
/// </summary>
public class HealProjectile : MonoBehaviour
{
    private Transform target;
    private float healAmount;

    public float speed = 8f;
    public GameObject impactEffect;

    public void Seek(Transform _target, float _healAmount)
    {
        target = _target;
        healAmount = _healAmount;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Move towards target (2D)
        Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (direction.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);

        // Rotate to face direction (2D)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void HitTarget()
    {
        if (impactEffect != null)
        {
            GameObject effectIns = Instantiate(impactEffect, transform.position, transform.rotation);
            Destroy(effectIns, 2f);
        }

        // Heal the target hero
        Hero hero = target.GetComponent<Hero>();
        if (hero != null && !hero.isDead && hero.heroData != null)
        {
            int previousHealth = hero.currentHealth;
            hero.currentHealth = Mathf.Min(hero.currentHealth + (int)healAmount, hero.heroData.maxHealth);
            int actualHeal = hero.currentHealth - previousHealth;
            Debug.Log($"Healed {hero.heroData.heroName} for {actualHeal} HP!");
        }

        Destroy(gameObject);
    }
}
