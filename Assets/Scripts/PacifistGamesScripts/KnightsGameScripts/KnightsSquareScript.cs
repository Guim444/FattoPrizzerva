using System.Collections;
using System.Linq;
using UnityEngine;

public class KnightsSquareScript : MonoBehaviour
{
    public Color originalColor;

    public char SquareColumn;
    public int SquareRow;
    public bool empty = true; //default = true
    public Vector3 knightPosition;
    public bool selectableSquare = false;

    public bool pathSquare = false;
    public KnightsSquareScript targetSquare;
    public bool isIceSquare = false;

    public bool isVoid;

    public KnightBehavior knight;
    public RockObstacleScript rock;

    private Material mat;

    public bool heavenStartZone = false, hellStartZone = false;

    public void Awake()
    {
        originalColor = GetComponent<Renderer>().material.color;

        Renderer rend = GetComponent<Renderer>();
        mat = new Material(rend.material);
        rend.material = mat;
        ToggleGlow(false, 1);
        name = SquareColumn.ToString() + SquareRow;
        knightPosition = new Vector3(transform.position.x, transform.position.y + 0.75f, transform.position.z);

        StartCoroutine(InitializeSquare());
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
        else
            Debug.Log("Por algun motivo no entra directamente");
    }
    /*void OnMouseEnter()
    {
        if (!selectableSquare && !pathSquare)
            return;

        if (pathSquare)
        {
            ToggleGlow(true, 0.6f);
        }
        else
        {
            ToggleGlow(true, 1f);
        }
    }

    void OnMouseExit()
    {
        if (!selectableSquare && !pathSquare)
            return;

        if (!selectableSquare)
            ToggleGlow(true, 0.2f);
        else
            ToggleGlow(true, 0.6f);
    }*/
    IEnumerator InitializeSquare()
    {
        yield return new WaitUntil(() => KnightsBoardManager.instance != null);

        KnightsBoardManager.instance.squares.Add(name, this);
        if (isIceSquare)
        {
            mat.color = Color.cyan * 0.5f;
        }
        else if (isVoid)
        {
            GetComponent<Renderer>().enabled = false;
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
    public void ToggleGlow(bool glow, float intensity)
    {
        if (glow)
        {
            Color glowColor = Color.purple;

            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", glowColor * intensity);

            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;

            mat.color = isIceSquare ? Color.cyan * 0.5f : mat.color;
        }
        else
        {
            mat.SetColor("_EmissionColor", Color.white);
            mat.DisableKeyword("_EMISSION");

            mat.color = isIceSquare ? Color.cyan * 0.5f : originalColor;
        }
    }

    public void TurnVoid()
    {
        isVoid = true;
        GetComponent<MeshRenderer>().enabled = false;
    }
}