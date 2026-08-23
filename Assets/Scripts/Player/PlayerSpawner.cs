using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Fallback Settings")]
    [SerializeField] private GameObject defaultCharacterPrefab;
    [SerializeField] private Vector2 spawnPosition = Vector2.zero;

    private void Awake()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        GameObject prefabToSpawn = StageRunContext.SelectedCharacterPrefab;

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("No character selected in StageRunContext. Spawning default character.", this);
            prefabToSpawn = defaultCharacterPrefab;
        }

        if (prefabToSpawn == null)
        {
            Debug.LogError("PlayerSpawner has no valid prefab to spawn!", this);
            return;
        }

        GameObject player = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        if (!player.CompareTag("Player"))
        {
            player.tag = "Player";
        }

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.BindPlayerReferences(player);
        }
    }
}