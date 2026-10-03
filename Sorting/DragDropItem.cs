using UnityEngine;

public class DragDropItem : PausableBehaviour
{
    private Transform itemDrop; 
    private bool isPlacedInDrop = false; 
    private Vector3 _originalPosition; 
    private MovingItem _movingItemScript; 

    private SmistamentoManager _currentSmistamentoManager;

    public string correctDropTag = "CorrectlyDestroyable";

    private void Start()
    {
        _originalPosition = transform.position; //Salva la posizione attuale del game object
        _movingItemScript = GetComponent<MovingItem>(); //Ottengo il riferimento alla componente MovingItem
    }

    private void OnMouseDrag()
    {
        if (_paused || MinigameManager.Instance.IsGameOver) return;

				//Se non è ancora eliminato nel drop e la sua componente moving non è null aallora la disabilito
        if (!isPlacedInDrop)
        {
            if (_movingItemScript != null)
            {
                _movingItemScript.enabled = false;
            }

            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //Converto le coordinate del mouse sullo schermo in coordinate del mondo di gioco
            pos.z = 0f; //Rimango sullo stesso piano
            transform.position = pos; //Aggiorno la posizione dell'oggetto a quella calcolata dal mouse, in questo modo segue il cursore
        }
    }

    private void OnMouseUp()
    {
        if (_paused || MinigameManager.Instance.IsGameOver) return;

				//Se non è ancora eliminato nel drop, controllo se esiste la zona di drop e se la distanza dell'oggetto e della zona di drop sia minore di 1 
        if (!isPlacedInDrop)
        {
            if (itemDrop != null && Vector3.Distance(transform.position, itemDrop.position) < 1f)
            {
		            //Se ha asseganto uno smistamentomanager e non ha asseganto il tag perde una vita
                if (_currentSmistamentoManager != null)
                {
                    if (gameObject.CompareTag(correctDropTag))
                    {
                        Debug.Log(gameObject.name + " dropped correctly!");
                    }
                    else
                    {
                        Debug.Log(gameObject.name + " dropped incorrectly! Losing a life.");
                        _currentSmistamentoManager.LoseLife();
                    }
                }
                else
                {
                    Debug.LogWarning("DragDropItem: SmistamentoManager.Instance non trovato! Impossibile notificare il risultato.");
                }

                transform.position = itemDrop.position; //Snap dell'oggetto
                isPlacedInDrop = true; //Rilascio di un oggetto nell'area di drop
                Destroy(gameObject); //Distruggo l'oggetto
            }
            else 
            {
                if (_movingItemScript != null)
                {
										//Torna alla posizione di scorrimento
                    transform.position = _movingItemScript.GetExpectedPosition() + _originalPosition; 
                    _movingItemScript.enabled = true; 
                    return;
                }
            }
            transform.position = _originalPosition;
        }
    }

		//Assegno il transform della zona di drop
     public void SetItemDrop(Transform drop)
    {
        itemDrop = drop;
    }

		//Assegno un riferimento al smistamento manager all'oggetto
    public void SetSmistamentoManager(SmistamentoManager manager)
    {
        _currentSmistamentoManager = manager;
    }
}
