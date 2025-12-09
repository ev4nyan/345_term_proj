using UnityEngine;
using System.Collections;

/// <summary>
/// A black hole created by the Warrior's ultimate.
/// Pulls enemies toward it and deals damage over time.
/// </summary>
public class BlackHole : MonoBehaviour
{
    [Header("Stats")]
    public float damage = 100f;        // Total damage over duration
    public float radius = 5f;          // Pull and damage radius
    public float duration = 5f;        // How long the black hole lasts
    public float pullStrength = 5f;    // How strongly enemies are pulled

    [Header("Targeting")]
    public LayerMask enemyLayer;

    [Header("Visual")]
    public Color gizmoColor = new Color(0.5f, 0f, 0.5f, 0.5f);

    private float damagePerTick;
    private float tickRate = 0.25f;
    private float endTime;

    void Start()
    {
        endTime = Time.time + duration;
        damagePerTick = damage / (duration / tickRate);

        // Start damage ticks
        StartCoroutine(DamageTick());

        // Destroy after duration
        Destroy(gameObject, duration + 0.1f);
    }

    void Update()
    {
        if (Time.time >= endTime) return;

        // Pull all enemies toward center
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        
        foreach (var enemy in enemies)
        {
            Vector2 direction = ((Vector2)transform.position - (Vector2)enemy.transform.position).normalized;
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            
            // Pull strength increases as enemies get closer
            float pullFactor = 1f - (distance / radius);
            Vector3 pullForce = (Vector3)(direction * pullStrength * pullFactor * Time.deltaTime);
            
            enemy.transform.position += pullForce;
        }

        // Visual rotation effect
        transform.Rotate(0, 0, 180f * Time.deltaTime);
    }

    IEnumerator DamageTick()
    {
        while (Time.time < endTime)
        {
            // Damage all enemies in radius
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
            
            foreach (var enemy in enemies)
            {
                EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
                if (eh != null)
                {
                    eh.TakeDamage((int)damagePerTick);
                }
            }

            yield return new WaitForSeconds(tickRate);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
