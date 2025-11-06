using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;
    
    [Header("Path")]
    public Transform[] waypoints;
    private int waypointIndex = 0;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        // If no waypoints assigned, try to find them
        if (waypoints == null || waypoints.Length == 0)
        {
            GameObject waypointParent = GameObject.Find("Waypoints");
            if (waypointParent != null)
            {
                waypoints = new Transform[waypointParent.transform.childCount];
                for (int i = 0; i < waypoints.Length; i++)
                {
                    waypoints[i] = waypointParent.transform.GetChild(i);
                }
            }
        }
    }
    
    void Update()
    {
        Move();
    }
    
    void Move()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        
        // Get current target waypoint
        Transform targetWaypoint = waypoints[waypointIndex];
        
        // Move towards waypoint (2D)
        Vector2 direction = ((Vector2)targetWaypoint.position - (Vector2)transform.position).normalized;
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
        
        // Check if reached waypoint
        if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            waypointIndex++;
            
            // Check if reached end
            if (waypointIndex >= waypoints.Length)
            {
                ReachedEnd();
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        // Add death effects, currency rewards, etc. here
        Destroy(gameObject);
    }
    
    void ReachedEnd()
    {
        // Enemy reached the end - damage player, etc.
        Destroy(gameObject);
    }
    
    void OnDrawGizmos()
    {
        // Draw path in editor
        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }
    }
}