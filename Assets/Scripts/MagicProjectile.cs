using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 10;
    public float lifeTime = 3f;
    public LayerMask targetLayer;   // towers

    private Vector2 direction;

    // Call this right after instantiation
    public void Init(Vector2 dir, int dmg, LayerMask layer)
    {
        direction = dir.normalized;
        damage = dmg;
        targetLayer = layer;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if collider is on the target layer
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            IDamageable dmg = other.GetComponent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}
