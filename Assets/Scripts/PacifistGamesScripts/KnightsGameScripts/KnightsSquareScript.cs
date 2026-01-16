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

    public KnightBehavior knight;

    private Material mat;

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
        if (KnightsGameManager.instance.selectedKnight != null && selectableSquare)
        {
            KnightBehavior thisKnight = KnightsGameManager.instance.selectedKnight;
            StartCoroutine(thisKnight.MoveKnight(this));
            thisKnight.Deselect();
            thisKnight = null;

            KnightsGameManager.instance.NextPlayer();
        }
    }
    void OnMouseEnter()
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
    }
    IEnumerator InitializeSquare()
    {
        yield return new WaitForSeconds(0.1f);

        KnightsBoardManager.instance.squares.Add(name, this);
        if (isIceSquare)
        {
            mat.color = Color.cyan * 0.5f;
        }
        if (KnightsBoardManager.instance.squares.Count == KnightsBoardManager.instance.height * KnightsBoardManager.instance.width)
        {
            //KnightsGameManager.instance.StartGame();
            KnightsBoardManager.instance.Test();
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

            mat.color = isIceSquare ? Color.cyan * 0.5f : Color.white;
        }
        else
        {
            mat.SetColor("_EmissionColor", Color.white);
            mat.DisableKeyword("_EMISSION");

            mat.color = isIceSquare ? Color.cyan * 0.5f : originalColor;
            
        }
    }
}