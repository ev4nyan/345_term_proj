using System.Collections;
using UnityEngine;

public class EvanEnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject slimePrefab;
    public GameObject dragonPrefab;

    [Header("Spawning")]
    public float spawnInterval = 2f;
    public int maxEnemies = 20;
    [Range(0f, 1f)]
    public float dragonSpawnChance = 0.2f;  // 20% chance to spawn dragon

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
        // Decide which enemy to spawn
        bool spawnDragon = dragonPrefab != null && Random.value < dragonSpawnChance;
        GameObject prefabToSpawn = spawnDragon ? dragonPrefab : slimePrefab;

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("No enemy prefab assigned!");
            return;
        }

        GameObject enemy = Instantiate(prefabToSpawn, pathWaypoints[0].position, Quaternion.identity);

        // Setup path for slime
        EnemyPathFollower follower = enemy.GetComponent<EnemyPathFollower>();
        if (follower != null)
            follower.waypoints = pathWaypoints;

        // Setup path for dragon
        DragonPathFollower dragonFollower = enemy.GetComponent<DragonPathFollower>();
        if (dragonFollower != null)
            dragonFollower.waypoints = pathWaypoints;

        enemiesSpawned++;
    }
}
