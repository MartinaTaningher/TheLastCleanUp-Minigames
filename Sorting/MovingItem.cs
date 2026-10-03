using UnityEngine;

public class MovingItem : PausableBehaviour
{
    [SerializeField] public float _moveSpeed = 2f;

    private Camera _mainCamera;
    private float _timeOfSpawn;

    private DragDropItem _dragDropItem;

    void Start()
    {
        _mainCamera = Camera.main;
        _timeOfSpawn = Time.time; //Registra il tempo totale dall'inizio del gioco, utilizzata per calcolare quanto lontano l'oggetto dovrebbe aver viaggiato
        _dragDropItem = GetComponent<DragDropItem>();
    }

    void Update()
    {
        if (_paused || MinigameManager.Instance.IsGameOver) return;

        transform.Translate(Vector3.left * _moveSpeed * Time.deltaTime); //Sposto l'oggetto 

        Vector3 viewportPos = _mainCamera.WorldToViewportPoint(transform.position); //Converto la posizione corrente dell'oggetto nel mondo in una posizione della viewport, mi serve per controllare se poi uscirà dallo schermo

        if (viewportPos.x < -0.1f)
        {
            if (_dragDropItem != null && gameObject.CompareTag(_dragDropItem.correctDropTag))
            {
                Debug.Log(gameObject.name + " è uscito dalla camera ed era un oggetto da buttare");
                FindAnyObjectByType<SmistamentoManager>().LoseLife();
            }

            Destroy(gameObject);
            Debug.Log(gameObject.name + " è stato distrutto");
        }
    }

		//Calcolo la posizione attesa dell'oggetto se avesse continuato a muoversi senza interruzioni
    public Vector3 GetExpectedPosition()
    {
        float timeElapsed = Time.time - _timeOfSpawn; //Tempo trascorso da qunado l'oggetto è stato generato
        float distanceTraveled = timeElapsed * _moveSpeed; //Distanza totale che l'oggetto avrebbe dovuto percorrere

        return Vector3.left * distanceTraveled; //Rappresenta lo spostamento dall'origine, che l'oggetto avrebbe dovuto avere
    }
}
