using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class KnightsSquareScript : MonoBehaviour
{
    public Color originalColor;
    public Color squareColor;

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
    public bool isFragile;

    public bool heavenStartZone = false, hellStartZone = false;

    public void OnEnable()
    {
        originalColor = GetComponent<Renderer>().material.color;
        squareColor = originalColor;

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
                if (!isVoid)
                {
                    /*if (heavenStartZone)
                    {
                        if (KnightsBoardManager.instance.fragileFloorStartPlayer1 == null)
                        {
                            KnightsBoardManager.instance.fragileFloorStartPlayer1 = new List<KnightsSquareScript>();
                        }

                        KnightsBoardManager.instance.fragileFloorStartPlayer1.Add(this);
                    }
                    else if (hellStartZone)
                    {
                        if (KnightsBoardManager.instance.fragileFloorStartPlayer2 == null)
                        {
                            KnightsBoardManager.instance.fragileFloorStartPlayer2 = new List<KnightsSquareScript>();
                        }

                        KnightsBoardManager.instance.fragileFloorStartPlayer2.Add(this);
                    }*/
                    if (heavenStartZone || hellStartZone)
                    {
                        StartCoroutine(MapEditorData.instance.WarningMessage("You can't set a start square as void. Only as a fragile floor square."));
                    }
                    else
                    {
                        isVoid = true;
                        TurnVoid();
                    }
                }
                else
                {
                    /*if (heavenStartZone)
                    {
                        KnightsBoardManager.instance.fragileFloorStartPlayer1.Remove(this);
                    }
                    else if (hellStartZone)
                    {
                        KnightsBoardManager.instance.fragileFloorStartPlayer2.Remove(this);
                    }*/
                    isVoid = false;
                    GetComponent<Renderer>().enabled = true;
                }

                /*MapEditorData.instance.voidSelected = false;
                Destroy(MapEditorData.instance.selectedObject);*/
            }
            else if (MapEditorData.instance.lavaSelected)
            {
                if (!isLava)
                {
                    isLava = true;
                    mat.color = Color.orangeRed;
                    squareColor = mat.color;
                    normalSquare = false;

                    if (heavenStartZone)
                    {
                        KnightsBoardManager.instance.lavaStartSquaresPlayer1.Add(this);
                    }
                    else if (hellStartZone)
                    {
                        KnightsBoardManager.instance.lavaStartSquaresPlayer2.Add(this);
                    }
                    else
                    {
                        KnightsBoardManager.instance.lavaSquares.Add(this);
                    }
                }
                else
                {
                    isLava = false;
                    mat.color = originalColor;
                    squareColor = mat.color;
                    normalSquare = true;
                    if (heavenStartZone)
                    {
                        KnightsBoardManager.instance.lavaStartSquaresPlayer1.Remove(this);
                    }
                    else if (hellStartZone)
                    {
                        KnightsBoardManager.instance.lavaStartSquaresPlayer2.Remove(this);
                    }
                    else
                    {
                        KnightsBoardManager.instance.lavaSquares.Remove(this);
                    }
                }

                /*MapEditorData.instance.lavaSelected = false;
                Destroy(MapEditorData.instance.selectedObject);*/
            }
            else if (MapEditorData.instance.fragileFloorSelected)
            {
                if (isFragile)
                {
                    isFragile = true;
                    mat.color *= new Color(0.9f, 0.75f, 0.6f);
                    squareColor = mat.color;
                    normalSquare = false;

                    if (heavenStartZone)
                    {
                        KnightsBoardManager.instance.fragileFloorStartPlayer1.Add(this);
                    }
                    else if (hellStartZone)
                    {
                        KnightsBoardManager.instance.fragileFloorStartPlayer2.Add(this);
                    }
                    else
                    {
                        KnightsBoardManager.instance.fragileFloor.Add(this);
                    }
                }
            }
            else
            {

            }

            //MapEditorData.instance.selectedObject = null;
        }
        else if (MapEditorData.instance.chooseHeaven)
        {
            if (!hellStartZone)
            {
                if (!KnightsBoardManager.instance.player1StartZone.Contains(this))
                {
                    if (MapEditorData.instance.heavenSelected == 0)
                    {
                        StartCoroutine(MapEditorData.instance.WarningMessage("There are no Heaven start zones left."));
                    }
                    else
                    {
                        StartColor(true);
                        MapEditorData.instance.heavenSelected--;
                        heavenStartZone = true;
                        KnightsBoardManager.instance.player1StartZone.Add(this);
                    }
                }
                else
                {
                    ToggleGlow(false, 1);
                    MapEditorData.instance.heavenSelected++;
                    KnightsBoardManager.instance.player1StartZone.Remove(this);
                    heavenStartZone = false;
                }

                MapEditorData.instance.heavenButton.GetComponentInChildren<TextMeshProUGUI>().text = "Heaven (" + MapEditorData.instance.heavenSelected + ") left";
            }
        }
        else if (MapEditorData.instance.chooseHell)
        {
            if (!heavenStartZone)
            {
                if (!KnightsBoardManager.instance.player2StartZone.Contains(this))
                {
                    if (MapEditorData.instance.hellSelected == 0)
                    {
                        StartCoroutine(MapEditorData.instance.WarningMessage("There are no Hell start zones left."));
                    }
                    else
                    {
                        StartColor(false);
                        MapEditorData.instance.hellSelected--;
                        hellStartZone = true;
                        KnightsBoardManager.instance.player2StartZone.Add(this);
                    }

                }
                else
                {
                    ToggleGlow(false, 1);
                    MapEditorData.instance.hellSelected++;
                    KnightsBoardManager.instance.player2StartZone.Remove(this);
                    hellStartZone = false;
                }

                MapEditorData.instance.hellButton.GetComponentInChildren<TextMeshProUGUI>().text = "Hell (" + MapEditorData.instance.hellSelected + ") left";
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
            squareColor = mat.color;
            normalSquare = false;
        }
        else if (isLava)
        {
            mat.color = Color.orangeRed;
            squareColor = mat.color;
            normalSquare = false;
        }
        else if (isVoid)
        {
            GetComponent<Renderer>().enabled = false;
            normalSquare = false;
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

            mat.color = squareColor;
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