using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class KnightsGameManager : MonoBehaviour
{
    public static KnightsGameManager instance;
    public int currentPlayer = 1;

    public KnightBehavior selectedKnight;
    public KnightsSquareScript selectedSquare;
    bool isSelectingMovement = false;
    public bool canMove;

    [Header("Knight Prefabs")]
    public GameObject agileKnightPrefab;
    public GameObject tucutuKnightPrefab;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }
    public void StartGame()
    {
        KnightsBoardManager.instance.SpawnKnights(4);
    }
    public void OnClick(InputValue value)
    {
        if (value.isPressed)
        {
            if (ClickHitsSelectableSquare())
            {
                isSelectingMovement = true;
                return;
            }

            if (selectedKnight != null)
            {
                selectedKnight.Deselect();
                selectedKnight = null;
            }
        }
    }
    bool ClickHitsSelectableSquare()
    {
        if (Camera.main != null && Mouse.current != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                KnightsSquareScript square = hit.collider.GetComponent<KnightsSquareScript>();
                if (square != null && square.selectableSquare)
                {
                    selectedSquare = square;
                    return true;
                }
            }

            return false;
        }
        return false;
    }
}
