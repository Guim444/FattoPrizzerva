using System.Collections;
using System.Linq;
using UnityEngine;

public class KnightsSquareScript : MonoBehaviour
{
    public char SquareColumn;
    public int SquareRow;
    public bool empty = true; //default = true
    public Vector3 knightPosition;
    public bool selectableSquare = false;

    public KnightBehavior knight;

    private Material mat;

    public void Awake()
    {
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
            KnightsGameManager.instance.currentPlayer = KnightsGameManager.instance.currentPlayer == 1 ? 2 : 1;
        }
    }
    IEnumerator InitializeSquare()
    {
        yield return new WaitForSeconds(0.1f);

        KnightsBoardManager.instance.squares.Add(name, this);
        if (KnightsBoardManager.instance.squares.Count == KnightsBoardManager.instance.height * KnightsBoardManager.instance.width)
        {
            //KnightsGameManager.instance.StartGame();
            KnightsBoardManager.instance.SpawnKnightInMiddle();
        }
    }
    public void ToggleGlow(bool glow, float intensity)
    {
        if (glow)
        {
            Color glowColor = Color.cyan;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", glowColor * intensity);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
        else
        {
            mat.SetColor("_EmissionColor", Color.white);
            mat.DisableKeyword("_EMISSION");
        }
    }
}
