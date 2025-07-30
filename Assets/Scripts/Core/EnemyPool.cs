// This script manages the enemy object pool, including spawning and respawning.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolManager : SingletonBase<EnemyPoolManager>
{
    [Header("Pool Configuration")]
    [Tooltip("The enemy prefab to be pooled.")]
    [SerializeField] private GameObject enemyPrefab;
    [Tooltip("The initial number of enemies to create in the pool.")]
    [SerializeField] private int poolSize = 20;
    [Tooltip("The time in seconds before a defeated enemy respawns.")]
    [SerializeField] private float respawnTime = 5f;

    [Header("Spawn Area Configuration")]
    [Tooltip("The center of the area where enemies can spawn.")]
    [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero;
    [Tooltip("The size (width, height, depth) of the spawn area. For 2D, use X and Y for width and height, and keep Z at 0 or a negligible value.")]
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(50, 50, 0); // Adjusted default for 2D: X and Y are used.

    // The list that holds all our pooled enemy objects.
    private List<GameObject> enemyPool;

    // Note: The Awake() method that handled the singleton logic has been removed,
    // as your base class should now handle it.

    private void Start()
    {
        InitializePool();
        // Call this method to spawn all enemies at the start of the game.
        SpawnAllEnemiesAtStart();
    }

    /// <summary>
    /// Creates the enemy objects and adds them to the pool in a deactivated state.
    /// </summary>
    private void InitializePool()
    {
        enemyPool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
            // Parent the new enemy to this manager for a cleaner scene hierarchy.
            enemy.transform.SetParent(transform);
            enemy.SetActive(false); // Start with the enemy deactivated.
            enemyPool.Add(enemy);
        }
    }

    /// <summary>
    /// Spawns all enemies currently in the pool at random positions within the spawn area.
    /// </summary>
    public void SpawnAllEnemiesAtStart()
    {
        foreach (GameObject enemy in enemyPool)
        {
            // Calculate a new random position for 2D (X and Y)
            float spawnX = Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2, spawnAreaCenter.x + spawnAreaSize.x / 2);
            float spawnY = Random.Range(spawnAreaCenter.y - spawnAreaSize.y / 2, spawnAreaCenter.y + spawnAreaSize.y / 2); // Corrected for 2D
            float spawnZ = spawnAreaCenter.z; // Keep Z at the center's Z for 2D, or 0 if your game is purely X/Y
            Vector3 randomPosition = new Vector3(spawnX, spawnY, spawnZ);

            enemy.transform.position = randomPosition;
            enemy.SetActive(true); // Activate the enemy
            // IMPORTANT: If your enemy script has an OnEnable() method to reset stats,
            // it will be called here when SetActive(true) is invoked.
        }
    }

    /// <summary>
    /// Retrieves an inactive enemy from the pool, if available.
    /// </summary>
    /// <returns>An inactive enemy GameObject, or null if all enemies are active.</returns>
    public GameObject GetPooledEnemy()
    {
        foreach (GameObject enemy in enemyPool)
        {
            if (!enemy.activeInHierarchy)
            {
                return enemy;
            }
        }
        // Optionally, you could expand the pool here if no enemies are available.
        // For now, we return null if all enemies are currently active.
        Debug.LogWarning("No inactive enemies available in the pool! Consider increasing pool size.");
        return null;
    }

    /// <summary>
    /// Called from the Enemy script when it "dies".
    /// This deactivates the enemy and starts the respawn timer.
    /// </summary>
    /// <param name="enemy">The enemy GameObject that was defeated.</param>
    public void ReturnEnemyToPool(GameObject enemy)
    {
        enemy.SetActive(false);
        StartCoroutine(RespawnEnemyCoroutine(enemy));
    }

    /// <summary>
    /// A coroutine that waits for the respawn time and then reactivates the enemy
    /// at a new random position.
    /// </summary>
    /// <param name="enemy">The enemy to be respawned.</param>
    private IEnumerator RespawnEnemyCoroutine(GameObject enemy)
    {
        // Wait for the specified duration.
        yield return new WaitForSeconds(respawnTime);

        // --- Calculate a new random position for 2D (X and Y) ---
        float spawnX = Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2, spawnAreaCenter.x + spawnAreaSize.x / 2);
        float spawnY = Random.Range(spawnAreaCenter.y - spawnAreaSize.y / 2, spawnAreaCenter.y + spawnAreaSize.y / 2); // Corrected for 2D
        float spawnZ = spawnAreaCenter.z; // Keep Z at the center's Z for 2D, or 0 if your game is purely X/Y
        Vector3 randomPosition = new Vector3(spawnX, spawnY, spawnZ);

        // Move the enemy to the new position.
        enemy.transform.position = randomPosition;
        
        // IMPORTANT: Reset any enemy-specific stats (like health) here.
        // The Enemy script's OnEnable() method is a great place to handle this.

        // Reactivate the enemy.
        enemy.SetActive(true);
    }

    /// <summary>
    /// Draws a helpful visual representation of the spawn area in the Scene view.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.3f, 0.4f); // A nice green color
        Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize); // This will correctly draw a 2D square/rectangle if spawnAreaSize.z is 0
    }
}