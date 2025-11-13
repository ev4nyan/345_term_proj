using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathFollower : MonoBehaviour
{
    [Header("Corner Waypoints (big path points)")]
    public Transform[] waypoints;

    [Header("Hop Settings")]
    public float hopDistance = 0.5f;   // world units per jump
    public float startupDuration = 0.9f;  // length of the jump-start clip
    public float jumpDuration = 0.5f;  // time per jump
    public float idleDuration = 0.15f;
    public float jumpHeight = 0.25f;

    private List<Vector3> hopPoints = new List<Vector3>();
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        BuildHopPoints();
        if (hopPoints.Count > 0)
            transform.position = hopPoints[0];

        StartCoroutine(FollowHops());
    }

    // Build evenly spaced hop points along the path
    void BuildHopPoints()
    {
        hopPoints.Clear();
        if (waypoints == null || waypoints.Length == 0) return;

        Vector3 current = waypoints[0].position;
        hopPoints.Add(current);

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Vector3 start = current;
            Vector3 end = waypoints[i + 1].position;

            Vector3 segDir = (end - start).normalized;
            float segRemaining = Vector3.Distance(start, end);

            // march along this segment at hopDistance steps
            while (segRemaining >= hopDistance)
            {
                current += segDir * hopDistance;
                hopPoints.Add(current);
                segRemaining -= hopDistance;
            }

            // snap exactly to the corner waypoint
            current = end;
            hopPoints.Add(current);
        }
    }

    IEnumerator FollowHops()
    {
        // assume idle is the default state, and DoJump trigger plays the whole jump chain
        int index = 0;
        while (index < hopPoints.Count - 1)
        {
            Vector3 start = hopPoints[index];
            Vector3 end = hopPoints[index + 1];

            // face movement direction (left/right)
            Vector3 dir = end - start;
            if (dir.x != 0f)
            {
                var s = transform.localScale;
                s.x = Mathf.Sign(dir.x) * Mathf.Abs(s.x);
                transform.localScale = s;
            }

            // fire the jump animation chain
            if (animator != null)
            {
                animator.ResetTrigger("DoJump");
                animator.SetTrigger("DoJump");
            }
            // wait for the startup animation time (0.9 s)
            yield return new WaitForSeconds(startupDuration);

            float t = 0f;
            while (t < jumpDuration)
            {
                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / jumpDuration);

                Vector3 pos = Vector3.Lerp(start, end, n);
                float height = 4f * jumpHeight * n * (1f - n); // nice parabola
                pos.y += height;

                transform.position = pos;
                yield return null;
            }

            index++;

            // little idle pause on each tile
            yield return new WaitForSeconds(idleDuration);
        }

        // reached the end of the path
        KingController king = FindFirstObjectByType<KingController>();
        if (king != null)
        {
            king.TakeDamage(10);   // or whatever damage per slime
        }
        Destroy(gameObject);
    }
}
