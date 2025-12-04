using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonPathFollower : MonoBehaviour
{
    [Header("Corner Waypoints (big path points)")]
    public Transform[] waypoints;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float bobAmplitude = 0.15f;  // how much the dragon bobs up/down
    public float bobFrequency = 2f;     // how fast it bobs

    [Header("Fireball Attack")]
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireballRange = 6f;
    public float fireballDamage = 15f;
    public float fireRate = 1.5f;       // seconds between shots
    public string towerTag = "Tower";
    public string heroTag = "Hero";

    private List<Vector3> pathPoints = new List<Vector3>();
    private Animator animator;
    private int currentPointIndex = 0;
    private float nextFireTime = 0f;
    private float bobOffset = 0f;
    private Vector3 basePosition;

    void Start()
    {
        animator = GetComponent<Animator>();
        bobOffset = Random.Range(0f, Mathf.PI * 2f); // randomize bob phase

        BuildPathPoints();
        if (pathPoints.Count > 0)
            transform.position = pathPoints[0];

        if (firePoint == null)
            firePoint = transform;

        StartCoroutine(FollowPath());
    }

    void Update()
    {
        // Check for targets to shoot
        TryShootFireball();
    }

    void TryShootFireball()
    {
        if (Time.time < nextFireTime) return;
        if (fireballPrefab == null) return;

        // Find closest tower or hero in range
        Transform target = FindClosestTarget();
        
        if (target != null)
        {
            ShootFireball(target);
            nextFireTime = Time.time + fireRate;
        }
    }

    Transform FindClosestTarget()
    {
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        // Check towers
        GameObject[] towers = GameObject.FindGameObjectsWithTag(towerTag);
        foreach (GameObject tower in towers)
        {
            float distance = Vector2.Distance(transform.position, tower.transform.position);
            if (distance < fireballRange && distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = tower.transform;
            }
        }

        // Check heroes
        GameObject[] heroes = GameObject.FindGameObjectsWithTag(heroTag);
        foreach (GameObject hero in heroes)
        {
            float distance = Vector2.Distance(transform.position, hero.transform.position);
            if (distance < fireballRange && distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = hero.transform;
            }
        }

        return closestTarget;
    }

    void ShootFireball(Transform target)
    {
        // Play attack animation if available
        if (animator != null)
        {
            animator.SetTrigger("Atk");
        }

        // Calculate direction to target
        Vector2 direction = ((Vector2)target.position - (Vector2)firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
        Fireball fb = fireball.GetComponent<Fireball>();
        if (fb != null)
        {
            fb.Seek(target, fireballDamage);
        }
    }

    // Build path points along waypoints
    void BuildPathPoints()
    {
        pathPoints.Clear();
        if (waypoints == null || waypoints.Length == 0) return;

        // Just use waypoints directly for smooth flight
        foreach (Transform wp in waypoints)
        {
            pathPoints.Add(wp.position);
        }
    }

    IEnumerator FollowPath()
    {
        while (currentPointIndex < pathPoints.Count - 1)
        {
            Vector3 start = pathPoints[currentPointIndex];
            Vector3 end = pathPoints[currentPointIndex + 1];

            // Face movement direction (left/right)
            Vector3 dir = end - start;
            if (dir.x != 0f)
            {
                var s = transform.localScale;
                s.x = Mathf.Sign(dir.x) * Mathf.Abs(s.x);
                transform.localScale = s;
            }

            // Smoothly move to next point
            float journeyLength = Vector3.Distance(start, end);
            float journeyTime = journeyLength / moveSpeed;
            float t = 0f;

            while (t < journeyTime)
            {
                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / journeyTime);

                // Base position along path
                basePosition = Vector3.Lerp(start, end, n);

                // Add bobbing motion
                float bob = Mathf.Sin((Time.time + bobOffset) * bobFrequency) * bobAmplitude;
                Vector3 finalPos = basePosition;
                finalPos.y += bob;

                transform.position = finalPos;
                yield return null;
            }

            currentPointIndex++;
        }

        // Reached the end of the path - damage the king
        KingController king = FindFirstObjectByType<KingController>();
        if (king != null)
        {
            king.TakeDamage(15); // dragons do more damage than slimes
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // Draw fireball range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fireballRange);
    }
}
