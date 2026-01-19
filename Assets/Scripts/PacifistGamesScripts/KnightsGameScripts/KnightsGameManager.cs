using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KnightsGameManager : MonoBehaviour
{
    public static KnightsGameManager instance;
    public int currentPlayer = 1;

    public KnightBehavior selectedKnight;
    public KnightsSquareScript selectedSquare;
    public bool canMove;

    public HashSet<KnightBehavior> activeMovements = new();

    [Header("Knight Prefabs")]
    public GameObject agileKnightPrefab;
    public GameObject tucutuKnightPrefab;

    [Header("Assets Prefabs")]
    public GameObject arrowPrefab;
    public GameObject invertedArrowPrefab;
    public List<GameObject> arrows;

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
                canMove = true;
                activeMovements.Clear();
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
                if (hit.collider.tag == "Arrow")
                {
                    selectedSquare = hit.collider.GetComponent<LArrow>().target;
                    return true;
                }

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
    public void NextPlayer()
    {
        currentPlayer = currentPlayer == 1 ? 2 : 1;

        foreach (KnightBehavior knight in KnightsBoardManager.instance.knightList)
        {
            if (knight.player == currentPlayer)
                knight.GetComponent<BoxCollider>().enabled = true;
            else
                knight.GetComponent<BoxCollider>().enabled = false;
        }
    }

    public void BeginMovement(KnightBehavior knight)
    {
        activeMovements.Add(knight);
        canMove = false;
    }

    public void EndMovement(KnightBehavior knight)
    {
        StartCoroutine(EndMovementWhenStopped(knight));
    }

    private IEnumerator EndMovementWhenStopped(KnightBehavior knight)
    {
        yield return new WaitUntil(() => !knight.isMoving);

        activeMovements.Remove(knight);
        canMove = activeMovements.Count == 0;
    }
    public void CallMoveCoroutine(KnightBehavior knight, KnightsSquareScript targetSquare)
    {
        //This is needed to start the movement when the arrow is clicked, because when it is destroyed, the coroutine stops.
        StartCoroutine(knight.MoveKnight(targetSquare));
    }
}
