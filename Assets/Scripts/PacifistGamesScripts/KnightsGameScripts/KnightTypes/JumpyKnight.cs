using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class JumpyKnight : KnightBehavior
{
    bool hasUsedDoubleJump = false;
    bool surpassed;

    protected override void Awake()
    {
        base.Awake();
        movementType = true;
        grounded = true;
        hasUsedDoubleJump = false;
    }
    protected override void OnDepart()
    {
        grounded = false;
        base.OnDepart();
        surpassed = false;
    }
    protected override void OnApproach(KnightsSquareScript nextSquare)
    {
        int dx = nextSquare.SquareColumn - transitSquare.SquareColumn;
        int dy = nextSquare.SquareRow - transitSquare.SquareRow;

        Vector2Int dir = new Vector2Int(dx != 0 ? (int)Mathf.Sign(dx) : 0, dy != 0 ? (int)Mathf.Sign(dy) : 0);

        char col = (char)(transitSquare.SquareColumn + dir.x);
        int row = transitSquare.SquareRow + dir.y;

        if (KnightsBoardManager.instance.squares.TryGetValue(col.ToString() + row, out var front))
        {
            if (front.knight != null && stepsMoved == 2)
            {
                StartCoroutine(PushForce(front.knight, dir, 1, allowIce: true, waitTime: 0.25f));
            }
        }
        while (isMoving)
        {
            KnightsGameManager.instance.canMove = false;
        }
        KnightsGameManager.instance.canMove = true;
    }

    public override void ConsumeMovementDirection()
    {
        base.ConsumeMovementDirection();

        lastMovement = stepsMoved >= 3;
        grounded = lastMovement;
    }

    public override void StepOnSquare(KnightsSquareScript sq, bool isGrounded)
    {
        base.StepOnSquare(sq, isGrounded);

        if (hasUsedDoubleJump || surpassed)
            return;

        if (stepsMoved == 1)
        {
            int dx = currentSquare.SquareColumn - previousSquare.SquareColumn;
            int dy = currentSquare.SquareRow - previousSquare.SquareRow;

            firstDir = new Vector2Int(dx != 0 ? (int)Mathf.Sign(dx) : 0, dy != 0 ? (int)Mathf.Sign(dy) : 0);
        }
        if (stepsMoved == 2)
        {
            surpassed = true;

            if (sq.knight != null || sq.rock != null)
            {
                if (sq.knight != null)
                    StartCoroutine(PushForce(sq.knight, -lastDir, 1));

                restartTarget = GetLTargetFromFirstStep(currentSquare);
                restartMovement = true;
                stepsMoved = 3;
            }
        }
    }
    KnightsSquareScript GetLTargetFromFirstStep(KnightsSquareScript start)
    {
        char col = start.SquareColumn;
        int row = start.SquareRow;

        col += (char)(lastDir.x * 2);
        row += lastDir.y * 2;

        col += (char)(firstDir.x);
        row += firstDir.y;

        string key = col.ToString() + row;

        KnightsSquareScript square = KnightsBoardManager.instance.GetSquare(key);

        if (square == null)
            square = KnightsBoardManager.instance.GetOutsideSquare("OUT_" + key);

        return square;
    }
}