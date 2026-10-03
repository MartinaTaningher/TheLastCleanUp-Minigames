using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Impostazioni Spawn")]
    [SerializeField] private GameObject[] itemPrefabs; 
    [SerializeField] private Transform[] spawnPoints; 
    [SerializeField] private Transform itemDrop; 
    [SerializeField] private float minSpawnDelay = 2f; 
    [SerializeField] private float maxSpawnDelay = 4f; 
    [SerializeField] private SmistamentoManager _smistamentoManager;

    private bool _isSpawningActive = false;

    public void StartSpawning()
    {
        if (_smistamentoManager == null)
        {
            Debug.LogError("ItemSpawner: SmistamentoManager non assegnato! Assegnalo nell'Inspector.");
            return;
        }

        _isSpawningActive = true;

        float initialSpawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
        Invoke(nameof(SpawnRandomItem), initialSpawnDelay);
        Debug.Log("ItemSpawner: Iniziato lo spawn degli oggetti.");
    }

    public void StopSpawning()
    {
        _isSpawningActive = false;
        CancelInvoke(nameof(SpawnRandomItem));
        Debug.Log("ItemSpawner: Fermato lo spawn degli oggetti.");
    }

    public void SpawnRandomItem()
    {
        if (!_isSpawningActive) return;

        if (itemPrefabs.Length == 0)
        {
            Debug.LogWarning("Nessun prefab di oggetto assegnato all'ItemSpawner!");
            return;
        }
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("Nessun punto di spawn assegnato all'ItemSpawner!");
            return;
        }

        int randomPrefabIndex = Random.Range(0, itemPrefabs.Length);
        GameObject selectedPrefab = itemPrefabs[randomPrefabIndex];

        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
        Vector3 spawnPosition = spawnPoints[randomSpawnIndex].position;

        GameObject spawnedItem = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

        DragDropItem dragDropScript = spawnedItem.GetComponent<DragDropItem>();
        if (dragDropScript != null)
        {
            dragDropScript.SetItemDrop(itemDrop);

            dragDropScript.SetSmistamentoManager(_smistamentoManager);
        }
        else
        {
            Debug.LogWarning("L'oggetto spawnato non ha un componente DragDropItem!");
        }

				//Programmo le prossime chiamate a SpawnRandomItem dopo nextSpawnDelay secondi, creando un ciclo finché non chiamo stop spawning
        if (_isSpawningActive)
        {
            float nextSpawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
            Invoke(nameof(SpawnRandomItem), nextSpawnDelay);
        }
    }
}
