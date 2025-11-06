using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private float damage;
    
    public float speed = 15f;
    public float explosionRadius = 0f;
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
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f); // -90 if sprite faces up
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
            DamageTarget(target);
        }
        
        Destroy(gameObject);
    }
    
    void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                DamageTarget(col.transform);
            }
        }
    }
    
    void DamageTarget(Transform enemy)
    {
        Enemy e = enemy.GetComponent<Enemy>();
        if (e != null)
        {
            e.TakeDamage(damage);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}