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
    public int turnCounter = 0;

    public KnightBehavior selectedKnight;
    public KnightsSquareScript selectedSquare;

    public bool gameHasStarted;
    public bool playerIsActive;

    public int playerSelectionCount = 0;

    public List<int> knightValues = new List<int>();

    public bool canMove;

    bool isRotating = false;

    public List<FloatyKnight> floatyKnights = new List<FloatyKnight>();    

    public HashSet<KnightBehavior> activeMovements = new();
    public HashSet<KnightBehavior> movementsInTheRound = new();
    public HashSet<KnightBehavior> knightsInLava = new();
    public int lavaTurns = 0;

    public bool revivedThisTurn = false;

    [Header("Knight Prefabs")]
    public GameObject agileKnightPrefab;
    public GameObject tucutuKnightPrefab;
    public GameObject jumpyKnightPrefab;
    public GameObject bullKnightPrefab;
    public GameObject shakyKnightPrefab;
    public GameObject ghostKnightPrefab;
    public GameObject shiftKnightPrefab;

    [Header("Assets Prefabs")]
    public GameObject arrowPrefab;
    public GameObject invertedArrowPrefab;
    public List<GameObject> arrows;

    [Header("UI Elements")]
    public GameObject button;
    public GameObject startPositionCanvas;
    public List<GameObject> knightDropdowns;

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

        for (int i = 0; i < knightDropdowns.Count; i++)
        {
            switch (knightDropdowns[i].GetComponent<TextMeshProUGUI>().text)
            {
                case "Agile":
                    knightValues.Add(0);
                    break;
                case "Tucutu":
                    knightValues.Add(1);
                    break;
                case "Shaky":
                    knightValues.Add(2);
                    break;
                case "Bull":
                    knightValues.Add(3);
                    break;
                case "Shapeshifter":
                    knightValues.Add(4);
                    break;
                case "Jumpy":
                    knightValues.Add(5);
                    break;
                case "Ghost":
                    knightValues.Add(6);
                    break;
            }
        }
    }

    public void StartGame(GameObject sender)
    {
        MapEditorData.instance.editMode = false;

        sender.SetActive(false);
        foreach (KnightsSquareScript square in KnightsBoardManager.instance.squares.Values)
        {
            StartCoroutine(square.InitializeSquare());
        }

        startPositionCanvas.SetActive(true);
        KnightsBoardManager.instance.TestStartZone();
        KnightsBoardManager.instance.SetObstacles();
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

    public void OnDelete(InputValue value)
    {
        if (value.isPressed)
        {
            if (MapEditorData.instance.selectedObject != null)
            {
                RockObstacleScript rock = MapEditorData.instance.selectedObject.GetComponent<RockObstacleScript>();
                if (rock != null)
                {
                    rock.currentSquare.rock = null;
                    rock.currentSquare = null;
                    Destroy(rock.gameObject);
                    MapEditorData.instance.selectedObject = null;
                }
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
            if (MapEditorData.instance.selectedObject.TryGetComponent<WaterCourse>(out WaterCourse water))
            {
                if (water.courseDirection == Vector2Int.up)
                {
                    StartCoroutine(WaterCourseRotate(water.gameObject.transform, Quaternion.Euler(90, 0, 90)));
                    water.courseDirection = Vector2Int.right;
                }
                else if (water.courseDirection == Vector2Int.right)
                {
                    StartCoroutine(WaterCourseRotate(water.gameObject.transform, Quaternion.Euler(90, 0, 0)));
                    water.courseDirection = Vector2Int.down;
                }
                else if (water.courseDirection == Vector2Int.down)
                {
                    StartCoroutine(WaterCourseRotate(water.gameObject.transform, Quaternion.Euler(90, 0, -90)));
                    water.courseDirection = Vector2Int.left;
                }
                else if (water.courseDirection == Vector2Int.left)
                {
                    StartCoroutine(WaterCourseRotate(water.gameObject.transform, Quaternion.Euler(90, 0, 180)));
                    water.courseDirection = Vector2Int.up;
                }
            }
            else
            {
                StartCoroutine(RotateCoroutine(MapEditorData.instance.selectedObject.transform));
            }
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
    IEnumerator WaterCourseRotate(Transform target, Quaternion targetAngle)
    {
        isRotating = true;
        Quaternion startRot = target.rotation;

        float t = 0f;
        float duration = 0.25f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            target.rotation = Quaternion.Lerp(startRot, targetAngle, t);
            yield return null;
        }

        target.rotation = targetAngle;
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
    public IEnumerator NextPlayer()
    {
        yield return new WaitUntil(() => activeMovements.Count == 0);
        turnCounter++;

        if (KnightsBoardManager.instance.player1StartZoneActive)
            KnightsBoardManager.instance.CheckStartZone(1);

        if (KnightsBoardManager.instance.player2StartZoneActive)
            KnightsBoardManager.instance.CheckStartZone(2);

        if (KnightsBoardManager.instance.lavaSquares.Count > 0)
            LavaBurn(1, KnightsBoardManager.instance.lavaSquares);

        if (KnightsBoardManager.instance.lavaStartSquaresPlayer1.Count > 0)
        {
            List<KnightsSquareScript> lavaStartSquares = new List<KnightsSquareScript>();
            lavaStartSquares.AddRange(KnightsBoardManager.instance.lavaStartSquaresPlayer1);
            lavaStartSquares.AddRange(KnightsBoardManager.instance.lavaStartSquaresPlayer2);
            LavaBurn(2, lavaStartSquares);
        }
        currentPlayer = currentPlayer == 1 ? 2 : 1;
        playerIsActive = true;

        foreach (KnightsSquareScript sq in KnightsBoardManager.instance.squares.Values)
        {
            if (sq.knight != null)
            {
                if (sq.knight.currentSquare != sq)
                {
                    //Handler
                    sq.knight = null;
                    sq.empty = true;
                    continue;
                }

                if (!sq.knight.isDead)
                {
                    sq.knight.currentSquare.knight = sq.knight;
                    sq.knight.currentSquare.empty = false;
                }
                else
                {
                    sq.knight.deathSquare.knight = sq.knight;
                    sq.knight.deathSquare.empty = false;
                }

                if (sq.knight.player != currentPlayer)
                    sq.knight.GetComponent<BoxCollider>().enabled = false;
            }
        }

        foreach (WaterCourse wc in KnightsBoardManager.instance.waterCourses)
        {
            if (wc.dryCourse)
            {

                if (wc.dryTurns == 0 && wc.activeTurns > 0)
                {
                    wc.activeTurns--;
                }
                else
                {
                    wc.dryTurns--;
                }

                if (wc.dryTurns == 0)
                {
                    wc.activeTurns = MapEditorData.instance.turnsWithWater;
                    wc.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                }
                else if (wc.dryTurns < 0)
                {
                    wc.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
                    wc.dryTurns = MapEditorData.instance.dryTurnsMax;
                }
            }
        }

        if (movementsInTheRound.Count > 0 && KnightsBoardManager.instance.waterCourses.Count > 0)
        {
            WaterCourse();
        }

        List<KnightBehavior> newKnightList = new List<KnightBehavior>();
        newKnightList.AddRange(KnightsBoardManager.instance.knightList);

        foreach (KnightBehavior knight in newKnightList)
        {
            if (knight.player == currentPlayer)
                knight.GetComponent<BoxCollider>().enabled = true;
        }

        foreach (ShiftKnight shapeshifter in KnightsBoardManager.instance.shapeshifters)
        {
            if (shapeshifter.currentKnight.player == currentPlayer)
            {
                shapeshifter.ChangeForm();
            }
        }
        movementsInTheRound.Clear();
        revivedThisTurn = false;
    }

    public void WaterCourse()
    {
        foreach (WaterCourse waterCourse in KnightsBoardManager.instance.waterCourses)
        {
            if (waterCourse.dryCourse && waterCourse.dryTurns != 0)
                continue;

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
    public void LavaBurn(int multiplier, List<KnightsSquareScript> squares) //This is used for the lava start squares, to make them less punishing, because those last more to kill.
    {
        foreach (KnightsSquareScript sq in squares)
        {
            if (sq.isLava && sq.knight != null && currentPlayer == sq.knight.player)
            {
                KnightBehavior knight = sq.knight;
                if (knightsInLava.Contains(knight))
                {
                    if (knight.turnsInLava < (lavaTurns * multiplier))
                    {
                        Debug.Log("Burn in " + (lavaTurns * multiplier - knight.turnsInLava) + " turns.");
                        knight.turnsInLava++;
                    }
                    else
                    {
                        StartCoroutine(knight.KillKnight(sq));
                    }
                }
                else
                {
                    knightsInLava.Add(knight);
                }
            }
        }

        var toRemove = new List<KnightBehavior>();

        foreach (var knight in knightsInLava)
        {
            if (!knight.currentSquare.isLava)
            {
                knight.turnsInLava = 0;
                toRemove.Add(knight);
            }
        }
        foreach (var knight in toRemove)
        {
            knightsInLava.Remove(knight);
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

    public void CheckKnights(int player)
    {
        List<KnightBehavior> deadKnights = new List<KnightBehavior>();
        List<KnightBehavior> rivalDeadKnights = new List<KnightBehavior>();

        foreach (KnightBehavior knight in KnightsBoardManager.instance.deadKnightList)
        {
            if (knight.player == player)
            {
                deadKnights.Add(knight);
            }
            else
            {
                rivalDeadKnights.Add(knight);
            }
        }

        if (deadKnights.Count == 2 && rivalDeadKnights.Count == 2)
        {
            Debug.Log("Mi bombo");
            int randIndex = Random.Range(0, deadKnights.Count);
            KnightBehavior knightToRevive = deadKnights[randIndex];

            StartCoroutine(knightToRevive.ReviveKnight(knightToRevive.currentSquare));
        }
    }
}
