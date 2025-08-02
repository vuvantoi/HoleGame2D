using Unity.VisualScripting;
using UnityEngine;

public class SpawnAIPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject aiPlayerPrefab;
    public GameObject playerFolder;
    [SerializeField] private int maxAIPlayers = 50;
    void Start()
    {
        // Spawn the AI player at the start
        for (int i = 0; i < maxAIPlayers; i++)
        {
            SpawnPlayer();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SpawnPlayer()
    {
        if (aiPlayerPrefab != null)
        {
            Vector2 position = new Vector2(Random.Range(-100, 100), Random.Range(-100, 100));
            GameObject playerSpawned = Instantiate(aiPlayerPrefab, position, Quaternion.identity);
            //make player a child of the playerFolder
            playerSpawned.transform.SetParent(playerFolder.transform);
        }
        else
        {
            Debug.LogError("AI Player Prefab is not assigned.");
        }
    }
}

        
    

