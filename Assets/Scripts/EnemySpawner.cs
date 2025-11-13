using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;
    public int maxEnemies = 20;

    public Transform[] pathWaypoints;

    private int enemiesSpawned = 0;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(2);
        while (enemiesSpawned < maxEnemies)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, pathWaypoints[0].position, Quaternion.identity);

        EnemyPathFollower follower = enemy.GetComponent<EnemyPathFollower>();
        if (follower != null)
            follower.waypoints = pathWaypoints;

        enemiesSpawned++;

    }
}
