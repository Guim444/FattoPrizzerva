using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines.ExtrusionShapes;

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

            //Force the knight to face the direction it is going to be pushed in

            StartCoroutine(RotateTowards(dir));

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
                    currentSquare.knight = this;
                    currentSquare.empty = false;
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
    }
    protected override void OnArrive(KnightsSquareScript square)
    {
        base.OnArrive(square);
        moveIndex = 0;
        stepsMoved = 0;
        KnightsGameManager.instance.canMove = true;
        currentSquare = square;
        currentSquare.knight = this;
    }
    IEnumerator SelfPush(KnightsSquareScript next)
    {
        char col = (char)(currentSquare.SquareColumn + lastDir.x);
        int row = currentSquare.SquareRow + lastDir.y;

        KnightsSquareScript target =
            KnightsBoardManager.instance.GetSquare(col.ToString() + row);

        if (target != null && target.knight != null)
        {
            currentSquare.knight = this;
            currentSquare.empty = false;

            KnightsGameManager.instance.EndMovement(this);
            yield break;
        }

        if (tired && !next.empty)
        {
            currentSquare.knight = this;
            currentSquare.empty = false;

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
    