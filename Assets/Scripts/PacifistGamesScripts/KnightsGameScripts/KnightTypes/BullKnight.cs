using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BullKnight : KnightBehavior
{
    bool hasPushed = false;
    protected override void Awake()
    {
        base.Awake();
        movementType = true;
        grounded = true;
    }
    protected override void OnDepart()
    {
        canContinue = true;
        movementDirections = new Vector2Int[0];

        base.OnDepart();

    }
    protected override void OnApproach(KnightsSquareScript nextSquare)
    {
        if (stepsMoved == 0 && movementDirections != null && movementDirections.Length > 0)
        {
            lastDir = movementDirections[movementDirections.Length - 1];
        }

        if (hasPushed)
        {
            hasPushed = false;

            stepsMoved = 2;

            StartCoroutine(SelfPush());
            canContinue = false;
            return;
        }
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
                StartCoroutine(PushForce(front.knight, dir, 2, allowIce: true));
                hasPushed = true;
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
        moveIndex = 0;
        stepsMoved = 0;
        KnightsGameManager.instance.canMove = true;
    }
    IEnumerator SelfPush()
    {
        KnightsGameManager.instance.canMove = false;
        yield return StartCoroutine(PushForce(this, lastDir, 1));
        yield return new WaitUntil(() => !isMoving);
        movementPaused = false;
        KnightsGameManager.instance.canMove = true;
    }
    public override void ConsumeMovementDirection()
    {
        base.ConsumeMovementDirection();

        lastMovement = stepsMoved >= 3;
    }
}
