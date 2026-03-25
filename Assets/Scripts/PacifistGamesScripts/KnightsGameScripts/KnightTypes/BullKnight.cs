using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BullKnight : KnightBehavior
{
    bool hasPushed = false;
    bool tired = false;
    protected override void Awake()
    {
        base.Awake();
        movementType = true;
        grounded = true;
    }
    protected override void OnDepart()
    {
        tired = false;
        canContinue = true;
        movementDirections = new Vector2Int[0];

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
        if (stepsMoved == 0 && movementDirections != null && movementDirections.Length > 0)
        {
            lastDir = movementDirections[movementDirections.Length - 1];
        }

        if (hasPushed)
        {
            hasPushed = false;

            stepsMoved = 2;

            StartCoroutine(SelfPush(nextSquare));
            canContinue = false;
            return;
        }


        if (KnightsBoardManager.instance.squares.TryGetValue(col.ToString() + row, out var front))
        {
            if (front.knight != null)
            {
                if (!tired && stepsMoved < 2)
                {
                    StartCoroutine(PushForce(front.knight, dir, 2, allowIce: true));
                    hasPushed = true;
                    tired = true;
                }
                else
                {
                    canContinue = false;
                    KnightsGameManager.instance.EndMovement(this);
                    return;
                }
            }
            else if (!front.empty)
            {
                canContinue = false;
                KnightsGameManager.instance.EndMovement(this);
                return;
            }
        }
        while (isMoving)
        {
            KnightsGameManager.instance.canMove = false;
        }
        KnightsGameManager.instance.canMove = true;
        tired = false;
    }
    protected override void OnArrive(KnightsSquareScript square)
    {
        base.OnArrive(square);
        moveIndex = 0;
        stepsMoved = 0;
        KnightsGameManager.instance.canMove = true;
    }
    IEnumerator SelfPush(KnightsSquareScript next)
    {
        if (tired && !next.empty)
        {
            KnightsGameManager.instance.EndMovement(this);
            yield break;
        }

        KnightsGameManager.instance.canMove = false;
        yield return StartCoroutine(PushForce(this, lastDir, 1));
        yield return new WaitUntil(() => !isMoving);
        movementPaused = false;
        KnightsGameManager.instance.canMove = true;
    }
    public override void ConsumeMovementDirection()
    {
        Vector2Int previousDir = lookingDirection;

        base.ConsumeMovementDirection();

        if (previousDir != Vector2Int.zero && lookingDirection != previousDir)
        {
            tired = true;
        }

        lastMovement = stepsMoved >= 3;
    }
}
