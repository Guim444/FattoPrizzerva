using UnityEngine;
using System.Collections.Generic;

public class RockObstacleScript : MonoBehaviour
{
    public bool isTall; //this will define if it can be jumped.
    public bool isBreakable;
    public KnightsSquareScript currentSquare;
    public List<GameObject> spikes;
    public List<KnightsSquareScript> dangerousSquares;

    private void OnEnable()
    {
        if (dangerousSquares == null)
            dangerousSquares = new List<KnightsSquareScript>();
    }

    public void SetDangerousSquares()
    {
        foreach (var s in spikes)
        {
            Vector2 dir = new Vector2(currentSquare.knightPosition.x - s.transform.position.x, currentSquare.knightPosition.z - s.transform.position.z);
            KnightsSquareScript sq = GetNearestSquareInDirection(dir);
            if (sq != null)
                dangerousSquares.Add(sq);
        }
    }

    KnightsSquareScript GetNearestSquareInDirection(Vector2 dir)
    {
        int stepX = dir.x > 0 ? 1 : dir.x < 0 ? -1 : 0;
        int stepY = dir.y > 0 ? 1 : dir.y < 0 ? -1 : 0;

        char col = currentSquare.SquareColumn;
        int row = currentSquare.SquareRow;

        col += (char)stepY;
        row += stepX;

        Debug.Log((char)col + "" + row);

        return KnightsBoardManager.instance.GetSquare(col.ToString() + row);
    }
}
