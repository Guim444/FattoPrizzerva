using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class KnightsSquareScript : MonoBehaviour
{
    public Color originalColor;

    public char SquareColumn;
    public int SquareRow;
    public bool empty = true; //default = true
    public Vector3 knightPosition;
    public KnightsSquareScript targetSquare;

    public KnightBehavior knight;
    public RockObstacleScript rock;

    private Material mat;

    public bool selectableSquare = false;
    public bool pathSquare = false;

    public Vector2Int waterCourseDirection = Vector2Int.zero;

    [Header("Terrain flags")]
    bool normalSquare = true;
    public bool isIceSquare = false;
    public bool isWaterSquare = false;
    public bool isWaterCourseCrossing = false;
    public bool isVoid;
    public bool isLava;

    public bool heavenStartZone = false, hellStartZone = false;

    public void OnEnable()
    {
        originalColor = GetComponent<Renderer>().material.color;

        Renderer rend = GetComponent<Renderer>();
        mat = new Material(rend.material);
        rend.material = mat;
        //StartCoroutine(InitializeSquare());
    }
    public void OnMouseDown()
    {
        if (KnightsGameManager.instance.selectedKnight != null)
        {
            KnightBehavior thisKnight = KnightsGameManager.instance.selectedKnight;

            if (KnightsGameManager.instance.gameHasStarted && selectableSquare)
            {
                StartCoroutine(thisKnight.MoveKnight(this));
                thisKnight.Deselect();
                thisKnight = null;
            }
            else
            {
                if ((KnightsGameManager.instance.currentPlayer == 1 && heavenStartZone) || (KnightsGameManager.instance.currentPlayer == 2 && hellStartZone))
                {
                    thisKnight.ToggleGlow(false, 1);

                    thisKnight.transform.position = knightPosition;

                    thisKnight.currentSquare.knight = null;
                    thisKnight.currentSquare.empty = true;

                    thisKnight.currentSquare = this;

                    knight = thisKnight;
                    empty = false;

                    thisKnight = null;
                }
            }
        }
        else if (MapEditorData.instance.selectedObject != null)
        {
            if (MapEditorData.instance.selectedObject.TryGetComponent<RockObstacleScript>(out RockObstacleScript currentRock))
            {
                rock = currentRock;
                Vector3 newPos = new Vector3(knightPosition.x, currentRock.transform.position.y, knightPosition.z);

                StartCoroutine(rock.MoveRock(newPos));

                currentRock.currentSquare = this;
                currentRock.SetDangerousSquares();

                currentRock.GetComponent<BoxCollider>().enabled = true;

                rock.ActivateGlow(false);

                KnightsBoardManager.instance.obstacles.Add(rock);
            }
            else if (MapEditorData.instance.voidSelected)
            {
                isVoid = true;
                TurnVoid();

                MapEditorData.instance.voidSelected = false;
                Destroy(MapEditorData.instance.selectedObject);
            }
            else
            {

            }

            MapEditorData.instance.selectedObject = null;
        }
        else if (MapEditorData.instance.chooseHeaven)
        {
            if (!KnightsBoardManager.instance.player1StartZone.Contains(this))
            {
                StartColor(false);
                if (MapEditorData.instance.heavenSelected == 0)
                {
                    KnightsSquareScript sq = KnightsBoardManager.instance.player1StartZone[0];
                    sq.heavenStartZone = false;
                    sq.ToggleGlow(false, 0);
                    KnightsBoardManager.instance.player1StartZone.Remove(sq);
                }
                else
                {
                    MapEditorData.instance.heavenSelected--;
                    MapEditorData.instance.heavenButton.GetComponentInChildren<TextMeshProUGUI>().text = "Heaven (" + MapEditorData.instance.heavenSelected + ") left";
                }
                heavenStartZone = true;
                KnightsBoardManager.instance.player1StartZone.Add(this);
            }
            else
            {
                KnightsBoardManager.instance.player1StartZone.Remove(this);
                heavenStartZone = false;
            }
        }
        else if (MapEditorData.instance.chooseHell)
        {
            if (!KnightsBoardManager.instance.player2StartZone.Contains(this))
            {
                StartColor(true);
                if (MapEditorData.instance.hellSelected == 0)
                {
                    KnightsSquareScript sq = KnightsBoardManager.instance.player2StartZone[0];
                    sq.hellStartZone = false;
                    sq.ToggleGlow(false, 0);
                    KnightsBoardManager.instance.player2StartZone.Remove(sq);
                }
                else
                {
                    MapEditorData.instance.hellSelected--;
                    MapEditorData.instance.hellButton.GetComponentInChildren<TextMeshProUGUI>().text = "Hell (" + MapEditorData.instance.hellSelected + ") left";
                }
                hellStartZone = true;
                KnightsBoardManager.instance.player2StartZone.Add(this);
            }
            else
            {
                KnightsBoardManager.instance.player2StartZone.Remove(this);
                hellStartZone = false;
            }
        }
    }
    private void OnMouseEnter()
    {
        if (KnightsGameManager.instance.selectedKnight != null && MapEditorData.instance.selectedObject != null)
            ToggleGlow(true, 0.5f);
    }
    private void OnMouseExit()
    {
        if (KnightsGameManager.instance.selectedKnight != null && MapEditorData.instance.selectedObject != null)
            ToggleGlow(false, 0);
    }
    public IEnumerator InitializeSquare()
    {
        yield return new WaitUntil(() => KnightsBoardManager.instance != null);

        ToggleGlow(false, 1);

        if (isIceSquare)
        {
            mat.color = Color.cyan * 0.5f;
            originalColor = mat.color;
            normalSquare = false;
        }
        else if (isLava)
        {
            mat.color = Color.orangeRed;
            originalColor = mat.color;
            normalSquare = false;
        }
        else if (isVoid)
        {
            GetComponent<Renderer>().enabled = false;
            normalSquare = false;
        }
        if (KnightsBoardManager.instance.squares.Count == KnightsBoardManager.instance.height * KnightsBoardManager.instance.width)
        {
            //KnightsGameManager.instance.StartGame();

            foreach (var data in KnightsBoardManager.instance.squares)
            {
                if (data.Value.heavenStartZone)
                    KnightsBoardManager.instance.player1StartZone.Add(data.Value);
                else if (data.Value.hellStartZone)
                    KnightsBoardManager.instance.player2StartZone.Add(data.Value);
            }

            //KnightsBoardManager.instance.Test();
            KnightsBoardManager.instance.TestStartZone();
            KnightsBoardManager.instance.SetObstacles();
        }
    }
    public void TurnVoid()
    {
        isVoid = true;
        GetComponent<MeshRenderer>().enabled = false;
    }
    public void ToggleGlow(bool glow, float intensity)
    {
        if (glow)
        {
            Color glowColor = Color.purple;

            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", glowColor * intensity);

            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
        else
        {
            mat.SetColor("_EmissionColor", Color.white);
            mat.DisableKeyword("_EMISSION");

            mat.color = originalColor;
        }
    }
    public void StartColor(bool heavenOrHell)
    {
        Color glowColor = heavenOrHell ? Color.skyBlue : Color.red;

        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", glowColor * 0.5f);

        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
    }
}