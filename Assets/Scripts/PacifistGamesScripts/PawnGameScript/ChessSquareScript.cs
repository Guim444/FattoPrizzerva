using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChessSquareScript : MonoBehaviour
{
    public char SquareColumn;
    public int SquareRow;
    public bool empty = true; //default = true
    public Vector3 pawnPosition;
    public bool selectableSquare = false;

    public PawnBehavior pawn;

    void Awake()
    {
    }
    private void OnMouseDown()
    {
        if (ClickManager.instance.selectedPawn != null && selectableSquare)
        {
            PawnBehavior thisPawn = ClickManager.instance.selectedPawn;
            PawnsGameManager.instance.playerTimer[PawnsGameManager.instance.activePlayer - 1] += PawnsGameManager.instance.extraTimeAddedPerTurn;
            StartCoroutine(thisPawn.MoveForward(this));
            thisPawn.Deselect();
            thisPawn = null;
        }
    }
    int GetRow()
    {
        if (transform.parent == null)
        {
            return -1;
        }

        string parentName = transform.parent.name;

        Match match = Regex.Match(parentName, @"\d+");

        if (match.Success)
        {
            return int.Parse(match.Value);
        }

        return -1;
    }
    public void CreateSquare()
    {
        pawnPosition = new Vector3(transform.position.x, transform.position.y + 0.75f, transform.position.z);
        SquareRow = GetRow();
        name = SquareColumn.ToString() + SquareRow.ToString();

        if (BoardManager.instance != null)
        {
            BoardManager.instance.RegisterSquare(this);
        }
    }
    public void ToggleGlow(bool glow)
    {
        Renderer rend = GetComponent<Renderer>();
        if (glow)
        {
            Color glowColor = new Color(0, 1, 1);
            float intensity = 0.3f;
            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", glowColor * intensity);
        }
        else
        {
            rend.material.SetColor("_EmissionColor", Color.black);
        }
    }
}
