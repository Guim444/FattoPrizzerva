using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines.ExtrusionShapes;

public class AgileKnight : KnightBehavior
{
    protected override void Awake()
    {
        base.Awake();
        movementType = true;
        grounded = true;
    }

    protected override void OnDepart()
    {
        grounded = false;
        base.OnDepart();
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
            if (front.knight != null && stepsMoved == 2)
            {
                StartCoroutine(PushForce(front.knight, dir, 2, pushThroughAir: true, allowIce: true, waitTime: 0.25f));
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
        /*grounded = true;

        if (enemyToPush != null && pushDirection != Vector2Int.zero && enemyStartSquare == square)
        {
            StartCoroutine(PushForce(enemyToPush, pushDirection, 2, allowIce: true));
        }

        enemyToPush = null;
        enemyStartSquare = null;
        pushDirection = Vector2Int.zero;

        while (isMoving)
        {
            KnightsGameManager.instance.canMove = false;
        }
        KnightsGameManager.instance.canMove = true;
        */
    }


    public override void ConsumeMovementDirection()
    {
        base.ConsumeMovementDirection();
        lastMovement = stepsMoved >= 3;
        grounded = lastMovement;
    }
}
