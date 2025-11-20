using System.Collections;
using UnityEngine;


[System.Serializable]
public class Wave
{
    // Enemies will spawn in this exact order
    public GameObject[] spawnOrder;

    public float timeBetweenSpawns = 1f;   // delay between each enemy in this wave
    public float delayAfterWave = 3f;     // pause before the next wave starts
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Path")]
    public Transform[] pathWaypoints;   // your existing waypoint array

    [Header("Waves")]
    public Wave[] waves;               // set up in Inspector

    [Header("Loop Waves?")]
    public bool loop = false;          // optional: repeat waves

    private void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    private IEnumerator SpawnWaves()
    {
        do
        {
            for (int w = 0; w < waves.Length; w++)
            {
                Wave wave = waves[w];

                // go through enemies in this wave, in order
                for (int i = 0; i < wave.spawnOrder.Length; i++)
                {
                    GameObject prefab = wave.spawnOrder[i];
                    if (prefab != null)
                        SpawnEnemy(prefab);

                    yield return new WaitForSeconds(wave.timeBetweenSpawns);
                }

                // pause before next wave
                yield return new WaitForSeconds(wave.delayAfterWave);
            }
        }
        while (loop);
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        // spawn at the start of the path
        GameObject enemy = Instantiate(enemyPrefab,
                                       pathWaypoints[0].position,
                                       Quaternion.identity);

        // give it the path (works for both slimes & enemy wizards)
        var jumper = enemy.GetComponent<EnemyPathFollower>();
        if (jumper != null)
            jumper.waypoints = pathWaypoints;

        var shooter = enemy.GetComponent<EnemyWizardPathShooter>();
        if (shooter != null)
            shooter.waypoints = pathWaypoints;
    }
}
