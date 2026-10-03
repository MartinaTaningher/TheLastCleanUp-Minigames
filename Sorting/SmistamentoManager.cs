using UnityEngine;

public class SmistamentoManager : PausableBehaviour, IMinigame, ILifeBasedMinigame
{
    [Header("Configurazione Gioco")]
    [SerializeField] private int _maxLives = 3;
    [SerializeField] private float _gameDuration = 60f;
    [SerializeField] private ItemSpawner _itemSpawner; 
    [SerializeField] private AudioClip _smistamentoAudioClip; 

    private int _currentLives;
    private float _gameTimer;
    public int CurrentLives => _currentLives;
    public int MaxLives => _maxLives;


    protected override void Awake()
    {
        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.RegisterMinigame(Consts.SceneNames.Smistamento, this);
        }
        else
        {
            Debug.LogError("SmistamentoManager: MinigameManager non trovato! Assicurati che sia presente nella scena e inizializzato correttamente.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (_paused || MinigameManager.Instance.IsGameOver) return;

        _gameTimer += Time.deltaTime;

        if (_gameTimer >= _gameDuration)
        {
            CheckWin(); 
        }
    }

    public void StartGame()
    {
        _currentLives = _maxLives;
        _gameTimer = 0f;
        Debug.Log("SmistamentoManager: Gioco iniziato!");

        if (_itemSpawner != null)
        {
            _itemSpawner.StartSpawning(); 
        }
        else
        {
            Debug.LogError("SmistamentoManager: ItemSpawner non assegnato! Assegnalo nell'Inspector.");
        }

        AudioManager.Instance.PlayBGM(_smistamentoAudioClip);
    }

    public void LoseLife()
    {
        if (_paused || MinigameManager.Instance.IsGameOver) return;

        _currentLives--;
        Debug.Log("SmistamentoManager: Vita persa! Vite rimaste: " + _currentLives);

        if (_currentLives <= 0)
        {
            MinigameManager.Instance.OnGameOver(); 
        }
    }

    public void CheckWin()
    {
        if (MinigameManager.Instance.IsGameOver) return;

        if (_itemSpawner != null)
        {
            _itemSpawner.StopSpawning();
        }

        CleanUpSpawnedItems();

        Debug.Log("SmistamentoManager: HAI VINTO! Condizioni di vittoria soddisfatte.");
        MinigameManager.Instance.OnVictory(); 
        MinigameManager.Instance.OnMinigameCompleted();
    }

    private void CleanUpSpawnedItems()
    {
        DragDropItem[] allSpawnedItems = FindObjectsByType<DragDropItem>(FindObjectsSortMode.None);
        foreach (DragDropItem item in allSpawnedItems)
        {
            Destroy(item.gameObject);
        }
        Debug.Log($"SmistamentoManager: Distrutti {allSpawnedItems.Length} oggetti giocabili in scena.");
    }
}
