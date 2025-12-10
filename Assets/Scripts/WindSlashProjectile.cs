using UnityEngine;

/// <summary>
/// Projectile that damages and pushes enemies back.
/// Used by HeroSamurai.
/// </summary>
public class WindSlashProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float damage = 20f;
    public float pushForce = 3f;
    public float lifeTime = 3f;

    private Transform target;
    private bool initialized = false;

    public void Init(Transform _target, float _damage, float _pushForce, float _speed)
    {
        target = _target;
        damage = _damage;
        pushForce = _pushForce;
        speed = _speed;
        initialized = true;

        // Rotate to face target
        if (target != null)
        {
            Vector2 dir = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (!initialized) return;

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Move toward target
        Vector2 direction = (target.position - transform.position).normalized;
        float distanceThisFrame = speed * Time.deltaTime;

        if (Vector2.Distance(transform.position, target.position) <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(direction * distanceThisFrame, Space.World);

        // Update rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void HitTarget()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Deal damage
        EnemyHealth eh = target.GetComponent<EnemyHealth>();
        if (eh != null)
        {
            eh.TakeDamage((int)damage);
        }

        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        // Push enemy back
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 pushDir = (target.position - transform.position).normalized;
            rb.AddForce(pushDir * pushForce, ForceMode2D.Impulse);
        }
        else
        {
            // Manual pushback if no rigidbody
            Vector2 pushDir = (target.position - transform.position).normalized;
            target.position += (Vector3)(pushDir * pushForce * 0.5f);
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            target = other.transform;
            HitTarget();
        }
    }
}
