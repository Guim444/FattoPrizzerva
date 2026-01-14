using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class TucutuKnight : KnightBehavior
{
    private bool hasPushed = false;

    protected override void Awake()
    {
        base.Awake();
        movementType = false;
        grounded = true;
    }
    protected override void OnDepart()
    {
        stepsMoved = 0;
    }

    protected override void OnApproach(KnightsSquareScript nextSquare)
    {
        int dx = nextSquare.SquareColumn - transitSquare.SquareColumn;
        int dy = nextSquare.SquareRow - transitSquare.SquareRow;

        Vector2Int dir = new Vector2Int(
            dx != 0 ? (int)Mathf.Sign(dx) : 0,
            dy != 0 ? (int)Mathf.Sign(dy) : 0
        );

        char col = (char)(transitSquare.SquareColumn + dir.x);
        int row = transitSquare.SquareRow + dir.y;

        if (!KnightsBoardManager.instance.squares.TryGetValue(col.ToString() + row, out var front))
        {
            KnightsGameManager.instance.canMove = true;
            return;
        }

        if (front.knight != null)
            StartCoroutine(PushForce(front, dir));

        KnightsGameManager.instance.canMove = true;
    }

    private IEnumerator PushForce(KnightsSquareScript origin, Vector2Int dir)
    {
        List<KnightsSquareScript> chain = new();
        KnightsSquareScript current = origin;

        while (current != null && current.knight != null)
        {
            chain.Add(current);

            char c = (char)(current.SquareColumn + dir.x);
            int r = current.SquareRow + dir.y;

            if (!KnightsBoardManager.instance.squares.TryGetValue(c.ToString() + r, out current))
            {
                KnightsSquareScript lastSquare = chain[^1];
                Destroy(lastSquare.knight.gameObject);
                lastSquare.knight = null;
                lastSquare.empty = true;
                chain.RemoveAt(chain.Count - 1);
                break;
            }
        }

        if (chain.Count == 0)
            yield break;

        KnightsSquareScript last = chain[^1];
        char nextCol = (char)(last.SquareColumn + dir.x);
        int nextRow = last.SquareRow + dir.y;

        if (!KnightsBoardManager.instance.squares.TryGetValue(nextCol.ToString() + nextRow, out var target))
            yield break;

        if (!target.empty)
            yield break;

        for (int i = chain.Count - 1; i >= 0; i--)
        {
            KnightsSquareScript from = chain[i];

            char c = (char)(from.SquareColumn + dir.x);
            int r = from.SquareRow + dir.y;

            KnightsSquareScript to = KnightsBoardManager.instance.GetSquare(c.ToString() + r);

            if (to == null || !to.empty)
                continue;

            to.knight = from.knight;
            to.empty = false;
            to.knight.currentSquare = to;

            StartCoroutine(to.knight.SmoothMove(to.knightPosition));

            from.knight = null;
            from.empty = true;
        }
    }
    protected override void OnArrive(KnightsSquareScript square)
    {
        hasPushed = false;
        KnightsGameManager.instance.canMove = true;
    }
}
