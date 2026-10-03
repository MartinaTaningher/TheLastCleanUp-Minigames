using UnityEngine;
using System.Collections;

public class PuzzleManager : PausableBehaviour, IMinigame
{
    [SerializeField] private Transform _emptySpace = null;
    [SerializeField] private TileBlock[] _tiles = new TileBlock[16]; //Contiene le tessere del gioco
    [SerializeField] private AudioClip _puzzleAudioClip;

    private Vector2[] _gridPositions = new Vector2[16]; //Memorizza le posizioni di ogni tessera
    private Camera _camera;  
    private int _emptySpaceIndex = 15; //Indice che corrisponde allo spazio vuoto
    private bool _isShuffling = true; //Booleano usato per prevenire gli input durante lo shuffle
    private bool _isMovingTile = false; //Indica se una tessera è in movimento

		protected override void Awake()
    {
        base.Awake();
        if (MinigameManager.Instance == null)
        {
            Debug.LogError("PuzzleManager: GameManager non trovato! Assicurati che sia presente nella scena e inizializzato correttamente.", this);
        }
        else
        {
            MinigameManager.Instance.RegisterMinigame(Consts.SceneNames.Puzzle15, this);
        }
    }

    private void Start()
    {
        _camera = Camera.main;

        if (_camera == null)
        {
            Debug.LogError("Main Camera non trovata! Assicurati che la tua telecamera abbia il tag 'MainCamera'.");
            enabled = false;
            return;
        }

        InitializeGrid(); //Imposto le posizioni iniziali delle tessere e dello spazio vuoto
    }

    private void Update()
    {
        if (_paused || MinigameManager.Instance.IsGameOver || _isShuffling || _isMovingTile) return;
				
				//Debug di vittoria forzata
        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("DEBUG: Vittoria forzata");
            MinigameManager.Instance.OnVictory();
            MinigameManager.Instance.OnMinigameCompleted();
        }

				//Quando premo il bottone sinistro del mouse elaboro il click
        if (Input.GetMouseButtonDown(0))
        {
            HandleTileClick();
        }
    }

    private void InitializeGrid()
    {
        for (int i = 0; i < _tiles.Length; i++)
        {
            if (_tiles[i] != null)
            {
                _gridPositions[i] = _tiles[i].transform.position; //Salvo la posizione attuale della tessera, così da avere i target position quando muovo le tessere
                _tiles[i].currentIndex = i; //Assegno l'indice corrente, sarà l'indice logico della tessera
                _tiles[i].correctPosition = _tiles[i].transform.position; //Memorizzo la posizione iniziale di ogni tessera come la posizione corretta per verificare la vittoria
            }
            else
            {
                _gridPositions[i] = _emptySpace.position; //Salvo la posizione corrente dello spazio vuoto
                _emptySpaceIndex = i; //Aggiorno l'indice corrente
            }
        }
    }

    private void HandleTileClick()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition); //Creo un raggio che parte dalla posizione del mouse e si estende sulle z tramite la telecamera
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction); //Faccio un raycast lungo il raggio e prendo le informazioni dell'oggetto che ho colpito per primo

        TileBlock clickedTile = hit.transform?.GetComponent<TileBlock>(); //Prendo la componente TileBlock dall'oggetto colpito, con ?. prevengo gli errori in caso sia null
        if (clickedTile != null && IsTileAdjacent(clickedTile.currentIndex))
        {
            MoveTile(clickedTile);
            StartCoroutine(WaitForTileMovement(clickedTile));
        }
    }

		//Coroutine che attende che la tessera finisca di muoversi prima di fare il check della vittoria
    private IEnumerator WaitForTileMovement(TileBlock tile)
    {
        _isMovingTile = true; // Blocco gli input durante il movimento

        // Attendo finché la tessera non ha raggiunto la sua posizione target
        yield return new WaitUntil(() => tile.HasReachedTarget());

        _isMovingTile = false; // Sblocco gli input

        CheckWin(); // Controllo la vittoria dopo che la tessera si è mossa
    }

		//Controlla se una tessera è adiacente allo spazio vuoto
    private bool IsTileAdjacent(int index)
    {
        int row = index / 4;
        int col = index % 4;
        int emptyRow = _emptySpaceIndex / 4;
        int emptyCol = _emptySpaceIndex % 4;

        return (row == emptyRow && Mathf.Abs(col - emptyCol) == 1) ||
               (col == emptyCol && Mathf.Abs(row - emptyRow) == 1);
    }

		
		//Scambio logico delle tessere
    private void MoveTile(TileBlock tile)
    {
        int tileIndex = tile.currentIndex; //Memorizzo l'indice della tessera che si sta spostando

        _tiles[_emptySpaceIndex] = tile; //Nello spazio vuoto inserisco la tessera che si sta spostando
        _tiles[tileIndex] = null; //Nello spazio della tessera metto lo spazio vuoto

        tile.currentIndex = _emptySpaceIndex; //Aggiorno l'indice logico della tessera con quello dello spazio vuoto
        tile.targetPosition = _gridPositions[_emptySpaceIndex]; //Metto la posizione target della tessera sulla posizione fisica dello spazio che prima dello scambio era vuoto (utilizzo lo spostamento di TileBlock)
        _emptySpace.position = _gridPositions[tileIndex]; //Allo stesso modo sposto le posizioni fisiche dello spazio che prima aveva la tessera ed ora è vuoto
        _emptySpaceIndex = tileIndex; //Aggiorno l'indice dello spazio vutoo
    }

    public void Shuffle()
    {
        _isShuffling = true; //Blocco gli input durante il mescolamento

				//Per ogni elemento, partendo dall'ultimo scambio la posizione con un elemento casuale incontrato prima o con sè stesso
        for (int i = _tiles.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            TileBlock temp = _tiles[i];
            _tiles[i] = _tiles[randomIndex];
            _tiles[randomIndex] = temp;
        }

				//Riassegno gli indici ad ogni tessera
        for (int i = 0; i < _tiles.Length; i++)
        {
            if (_tiles[i] != null)
            {
                _tiles[i].currentIndex = i;
                _tiles[i].targetPosition = _gridPositions[i];
                _tiles[i].transform.position = _gridPositions[i];
            }
            else
            {
                _emptySpaceIndex = i;
                _emptySpace.position = _gridPositions[i];
            }
        }

        _isShuffling = false; //Sblocco gli input
    }

    public void StartGame()
    {
        Debug.Log("PuzzleManager: StartGame chiamato, inizio shuffle.");
        Shuffle();
        AudioManager.Instance.PlayBGM(_puzzleAudioClip);

    }

    public void CheckWin()
    {
        for (int i = 0; i < _tiles.Length; i++)
        {
            // Se la posizione attuale non contiene una tessera (quindi è lo spazio vuoto)
            // O se la tessera presente non è nella sua posizione corretta
            if (_tiles[i] != null && _tiles[i].correctPosition != _gridPositions[i])
            {
                return; // Se trovo anche una sola tessera fuori posto, il puzzle non è risolto.
            }
        }

        // Se il ciclo termina, significa che tutte le tessere sono nella loro posizione corretta.
        Debug.Log("PuzzleManager: puzzle completato!");
        MinigameManager.Instance.OnVictory();
        MinigameManager.Instance.OnMinigameCompleted();
    }
}
