using UnityEngine;

public class Fireball : MonoBehaviour
{
    private Transform target;
    private float damage;
    
    public float speed = 10f;
    public float explosionRadius = 0f;  // 0 = single target, >0 = splash damage
    public GameObject impactEffect;
    
    public void Seek(Transform _target, float _damage)
    {
        target = _target;
        damage = _damage;
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
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
    
    void HitTarget()
    {
        if (impactEffect != null)
        {
            GameObject effectIns = Instantiate(impactEffect, transform.position, transform.rotation);
            Destroy(effectIns, 2f);
        }
        
        if (explosionRadius > 0f)
        {
            Explode();
        }
        else
        {
            DamageTarget(target.gameObject);
        }
        
        Destroy(gameObject);
    }
    
    void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        
        foreach (Collider2D col in colliders)
        {
            // Damage towers and heroes in explosion radius
            if (col.CompareTag("Tower") || col.CompareTag("Hero"))
            {
                DamageTarget(col.gameObject);
            }
        }
    }
    
    void DamageTarget(GameObject target)
    {
        // Try TowerHealth first
        TowerHealth towerHealth = target.GetComponent<TowerHealth>();
        if (towerHealth != null)
        {
            towerHealth.TakeDamage(damage);
            return;
        }
        
        // Try FlamethrowerTower
        FlamethrowerTower flameTower = target.GetComponent<FlamethrowerTower>();
        if (flameTower != null)
        {
            flameTower.TakeDamage((int)damage);
            return;
        }
        
        // Try Hero
        Hero hero = target.GetComponent<Hero>();
        if (hero != null)
        {
            hero.TakeDamage((int)damage);
            return;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
