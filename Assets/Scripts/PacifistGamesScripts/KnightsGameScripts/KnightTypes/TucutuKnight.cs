using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;

public class TucutuKnight : KnightBehavior
{
    protected override void Awake()
    {
        base.Awake();
        movementType = false;
        grounded = true;
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

        if (KnightsBoardManager.instance.squares.TryGetValue(col.ToString() + row, out var front))
        {
            if (front.knight != null)
            {
                StartCoroutine(PushForce(front.knight, dir, 1, allowIce: true));
            }
        }
        while (isMoving)
        {
            KnightsGameManager.instance.canMove = false;
        }
        KnightsGameManager.instance.canMove = true;
    }
    protected override void OnArrive(KnightsSquareScript square)
    {
        base.OnArrive(square);
        KnightsGameManager.instance.canMove = true;
    }
}
