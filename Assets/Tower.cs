using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Targeting")]
    public float range = 10f;
    public string enemyTag = "Enemy";
    private Transform target;
    
    [Header("Combat")]
    public float fireRate = 1f;
    private float fireCountdown = 0f;
    public float damage = 25f;
    
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    
    [Header("Rotation")]
    public float rotationSpeed = 10f;
    public Transform partToRotate;
    
    void Start()
    {
        // If no fire point assigned, use tower position
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        // If no part to rotate, rotate the whole tower
        if (partToRotate == null)
        {
            partToRotate = transform;
        }
        
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }
    
    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;
        
        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }
        
        if (nearestEnemy != null && shortestDistance <= range)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }
    
    void Update()
    {
        if (target == null)
            return;
        
        // Rotate towards target (2D)
        Vector2 direction = (Vector2)target.position - (Vector2)partToRotate.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle - 90f); // -90 if sprite faces up
        partToRotate.rotation = Quaternion.Lerp(partToRotate.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        
        // Shoot at target
        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }
        
        fireCountdown -= Time.deltaTime;
    }
    
    void Shoot()
    {
        if (projectilePrefab != null)
        {
            GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Projectile projectile = projectileGO.GetComponent<Projectile>();
            
            if (projectile != null)
            {
                projectile.Seek(target, damage);
            }
        }
        else
        {
            // Instant damage if no projectile
            Enemy enemyScript = target.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(damage);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw range in editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}