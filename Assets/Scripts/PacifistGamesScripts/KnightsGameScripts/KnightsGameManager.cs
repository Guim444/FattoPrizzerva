using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KnightsGameManager : MonoBehaviour
{
    public static KnightsGameManager instance;
    public int currentPlayer = 1;

    public KnightBehavior selectedKnight;
    public KnightsSquareScript selectedSquare;

    public bool gameHasStarted;
    public bool playerIsActive;

    public int playerSelectionCount = 0;

    public bool canMove;

    bool isRotating = false;

    public HashSet<KnightBehavior> activeMovements = new();
    public HashSet<KnightBehavior> movementsInTheRound = new();

    [Header("Knight Prefabs")]
    public GameObject agileKnightPrefab;
    public GameObject tucutuKnightPrefab;

    [Header("Assets Prefabs")]
    public GameObject arrowPrefab;
    public GameObject invertedArrowPrefab;
    public List<GameObject> arrows;

    [Header("UI Elements")]
    public GameObject button;

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

    public void StartBoard(GameObject confirm)
    {
        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        if (MapEditorData.instance != null)
        {
            if (int.TryParse(MapEditorData.instance.boardSizeX.text, out int x))
            {
                KnightsBoardManager.instance.width = x;
            }

            if (int.TryParse(MapEditorData.instance.boardSizeY.text, out int y))
            {
                KnightsBoardManager.instance.height = y;
            }
            KnightsBoardManager.instance.GenerateBoard();
        }

        sender.SetActive(false);
        confirm.SetActive(true);
    }

    public void StartGame()
    {
        //KnightsBoardManager.instance.SpawnKnights(4);
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
    public void OnRotate(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (isRotating)
            return;

        if (MapEditorData.instance.selectedObject != null)
        {
            StartCoroutine(RotateCoroutine(MapEditorData.instance.selectedObject.transform));
        }
    }
    IEnumerator RotateCoroutine(Transform target)
    {
        isRotating = true;
        Quaternion startRot = target.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 90f, 0f);

        float t = 0f;
        float duration = 0.25f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            target.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        target.rotation = endRot;
        isRotating = false;
    }
    bool ClickHitsSelectableSquare()
    {
        if (Camera.main != null && Mouse.current != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (gameHasStarted)
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
                else
                {
                    KnightsSquareScript square = hit.collider.GetComponent<KnightsSquareScript>();
                    if (square != null && square.empty && (square.heavenStartZone || square.hellStartZone))
                    {
                        selectedSquare = square;
                        return true;
                    }
                }
            }

            return false;
        }
        return false;
    }
    public void NextPlayer()
    {
        if (KnightsBoardManager.instance.player1StartZoneActive)
            KnightsBoardManager.instance.CheckStartZone(1);

        if (KnightsBoardManager.instance.player2StartZoneActive)
            KnightsBoardManager.instance.CheckStartZone(2);

        currentPlayer = currentPlayer == 1 ? 2 : 1;
        playerIsActive = true;

        foreach (KnightBehavior knight in KnightsBoardManager.instance.knightList)
        {
            if (!knight.isDead)
            {
                knight.currentSquare.knight = knight;
                knight.currentSquare.empty = false;
            }

            if (knight.player != currentPlayer)
                knight.GetComponent<BoxCollider>().enabled = false;
        }

        if (movementsInTheRound.Count > 0 && KnightsBoardManager.instance.waterCourses.Count > 0)
        {
            WaterCourse();
        }

        foreach (KnightBehavior knight in KnightsBoardManager.instance.knightList)
        {
            if (knight.player == currentPlayer)
                knight.GetComponent<BoxCollider>().enabled = true;
        }
        movementsInTheRound.Clear();
    }

    public void WaterCourse()
    {
        foreach (WaterCourse waterCourse in KnightsBoardManager.instance.waterCourses)
        {
            foreach (KnightsSquareScript sq in waterCourse.waterCourseSquares)
            {
                if (movementsInTheRound.Contains(sq.knight))
                {
                    KnightBehavior knight = sq.knight;

                    Debug.Log(knight.name);

                    int steps;

                    if (!waterCourse.dryCourse)
                        steps = 1;
                    else
                        steps = 2;
                    
                    StartCoroutine(knight.WaterCourseCoroutine(sq, steps, waterCourse));
                }
            }
        }
    }

    public void BeginMovement(KnightBehavior knight)
    {
        activeMovements.Add(knight);
        if(!movementsInTheRound.Contains(knight))
            movementsInTheRound.Add(knight);
        canMove = false;
    }

    public void EndMovement(KnightBehavior knight)
    {
        if (activeMovements.Contains(knight))
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

    public void ConfirmStartPosition()
    {
        currentPlayer = currentPlayer == 1 ? 2 : 1;

        playerSelectionCount++;

        if (playerSelectionCount > 2)
        {
            gameHasStarted = true;
            List<KnightsSquareScript> allStartZones = new List<KnightsSquareScript>();
            allStartZones.AddRange(KnightsBoardManager.instance.player1StartZone);
            allStartZones.AddRange(KnightsBoardManager.instance.player2StartZone);

            foreach (KnightsSquareScript sq in allStartZones)
            {
                sq.ToggleGlow(false, 1);

                if (sq.knight != null)
                {
                    sq.knight.GetComponent<Renderer>().enabled = true;
                }
            }

            for (int i = KnightsBoardManager.instance.player1StartZone.Count - 1; i >= 0; i--)
            {
                if (KnightsBoardManager.instance.player1StartZone[i].knight == null)
                {
                    KnightsBoardManager.instance.player1StartZone.RemoveAt(i);
                }
            }
            for (int i = KnightsBoardManager.instance.player2StartZone.Count - 1; i >= 0; i--)
            {
                if (KnightsBoardManager.instance.player2StartZone[i].knight == null)
                {
                    KnightsBoardManager.instance.player2StartZone.RemoveAt(i);
                }
            }

            button.SetActive(false);            
        }
        else
        {
            if (currentPlayer == 1)
            {
                foreach (KnightsSquareScript sq in KnightsBoardManager.instance.player1StartZone)
                {
                    sq.ToggleGlow(true, 1);

                    if (sq.knight != null)
                    {
                        sq.knight.GetComponent<Renderer>().enabled = true;
                    }
                }
                foreach (KnightsSquareScript sq in KnightsBoardManager.instance.player2StartZone)
                {
                    sq.ToggleGlow(false, 1);

                    if (sq.knight != null)
                    {
                        sq.knight.GetComponent<Renderer>().enabled = false;
                    }
                }
            }

            else if (currentPlayer == 2)
            {
                foreach (KnightsSquareScript sq in KnightsBoardManager.instance.player1StartZone)
                {
                    sq.ToggleGlow(false, 1);

                    if (sq.knight != null)
                    {
                        sq.knight.GetComponent<Renderer>().enabled = false;
                    }
                }
                foreach (KnightsSquareScript sq in KnightsBoardManager.instance.player2StartZone)
                {
                    sq.ToggleGlow(true, 1);

                    if (sq.knight != null)
                    {
                        sq.knight.GetComponent<Renderer>().enabled = true;
                    }
                }
            }

            button.GetComponentInChildren<TextMeshProUGUI>().text = "Player " + currentPlayer + "'s turn";
        }
    }
}
