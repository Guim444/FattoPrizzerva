using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class ChessSquareScript : MonoBehaviour
{
    public char SquareColumn;
    public int SquareRow;
    public bool empty = true; //default = true
    public Vector3 pawnPosition;

    void Awake()
    {
        pawnPosition = new Vector3(transform.position.x, transform.position.y + 0.75f, transform.position.z);
        SquareRow = GetRow();
        name = SquareColumn.ToString() + SquareRow.ToString();

        BoardManager.instance.RegisterSquare(this);

        if (BoardManager.instance.squares.Count == 80)
        {
            Debug.Log("Finished");
            BoardManager.instance.SpawnPawns(8);
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
}
