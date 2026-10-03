using UnityEngine;

public class TileBlock : PausableBehaviour
{
    public Vector2 targetPosition; //Memorizzo la posizione X e Y verso cui la tessera si sta spostando
    public Vector2 correctPosition; //Memorizzo la posizione X e Y originale e corretta
    public int currentIndex; //Indice della tessera

    [SerializeField] private float moveSpeed = 0.1f; //Velocità con cui la tessera si sposta verso la target position

    //Soglia per gestire l'imprecisione dei float
    private const float _positionThreshold = 0.01f;

    protected override void Awake()
    {
        base.Awake();
        correctPosition = transform.position; //Inizializzo la posizione corretta della tessera alla posizione attuale quando il gioco inizia, questa sarà la posizione in cui il puzzle deve trovarsi per essere risolto
        targetPosition = correctPosition; //La destinazione all'inizio è la sua posizione attuale
    }

    void Update()
    {
        if (_paused) return; //Impedisco il movimento delle tessere

        // Se la tessera non è ancora alla posizione target, si muove
        if (Vector2.Distance(transform.position, targetPosition) > _positionThreshold)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime * 60f); //Movimento della tessera
        }
        else if (transform.position != (Vector3)targetPosition) //Se non ho la posizione esatta e la soglia è stata superata
        {
            transform.position = targetPosition; // Snap alla posizione finale
        }
    }

		//Restituisce un bool per sapere se la tesssera ha finito di muoversi 
    public bool HasReachedTarget()
    {
        return Vector2.Distance(transform.position, targetPosition) <= _positionThreshold;
        //Restituisce true se la distanza tra la posizione attuale e quella target è minore o uguale a alla posizione soglia
    }
}
