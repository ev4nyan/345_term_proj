using UnityEngine;

/// <summary>
/// Arrow projectile - homing projectile for HeroArcher.
/// </summary>
public class Arrow : MonoBehaviour
{
    private Transform target;
    private float damage;
    private float speed = 15f;
    private bool initialized = false;

    public void Seek(Transform _target, float _damage, float _speed = 15f)
    {
        target = _target;
        damage = _damage;
        speed = _speed;
        initialized = true;

        Destroy(gameObject, 5f);  // Safety cleanup
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

        // Rotate to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);  // -90 if arrow sprite points up
    }

    void HitTarget()
    {
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
