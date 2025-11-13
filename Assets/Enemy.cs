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
    
    [Header("Tower Attack")]
    public float towerDetectionRange = 3f;
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public float attackRate = 1f;
    private float attackCooldown = 0f;
    public string towerTag = "Tower";
    private Transform targetTower;
    
    private enum EnemyState { FollowingPath, AttackingTower }
    private EnemyState currentState = EnemyState.FollowingPath;
    
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
        // Update attack cooldown
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }
        
        // Check for nearby towers
        DetectTowers();
        
        // Behavior based on state
        switch (currentState)
        {
            case EnemyState.FollowingPath:
                Move();
                break;
            case EnemyState.AttackingTower:
                AttackTower();
                break;
        }
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
    
    void DetectTowers()
    {
        // Find all towers in range
        GameObject[] towers = GameObject.FindGameObjectsWithTag(towerTag);
        float closestDistance = Mathf.Infinity;
        Transform closestTower = null;
        
        foreach (GameObject tower in towers)
        {
            float distance = Vector2.Distance(transform.position, tower.transform.position);
            
            if (distance < towerDetectionRange && distance < closestDistance)
            {
                closestDistance = distance;
                closestTower = tower.transform;
            }
        }
        
        // Switch state based on tower detection
        if (closestTower != null)
        {
            targetTower = closestTower;
            currentState = EnemyState.AttackingTower;
        }
        else
        {
            targetTower = null;
            currentState = EnemyState.FollowingPath;
        }
    }
    
    void AttackTower()
    {
        if (targetTower == null)
        {
            currentState = EnemyState.FollowingPath;
            return;
        }
        
        float distanceToTower = Vector2.Distance(transform.position, targetTower.position);
        
        // Move towards tower if not in attack range
        if (distanceToTower > attackRange)
        {
            Vector2 direction = ((Vector2)targetTower.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
        }
        else
        {
            // In attack range - attack!
            if (attackCooldown <= 0)
            {
                DealDamageToTower(targetTower.gameObject);
                attackCooldown = 1f / attackRate;
            }
        }
    }
    
    void DealDamageToTower(GameObject tower)
    {
        // Try to damage the tower using a health component if it exists
        TowerHealth towerHealth = tower.GetComponent<TowerHealth>();
        if (towerHealth != null)
        {
            towerHealth.TakeDamage(attackDamage);
        }
        else
        {
            // If no health component, just destroy the tower after enough hits
            Debug.Log($"Enemy attacking tower at {tower.transform.position}");
            // You can add a simple counter system here if needed
        }
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
    
    void OnDrawGizmosSelected()
    {
        // Draw tower detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, towerDetectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}