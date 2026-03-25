using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;

public class ShakyKnight : KnightBehavior
{
    bool shake = false;
    Vector2Int dir;
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

        dir = new Vector2Int(dx != 0 ? (int)Mathf.Sign(dx) : 0, dy != 0 ? (int)Mathf.Sign(dy) : 0);

        StartCoroutine(ApproachCoroutine(dir));

        if (!canContinue)
            return;
        
    }


    IEnumerator ApproachCoroutine(Vector2Int dir)
    {
        if (stepsMoved == 2)
        {
            shake = true;
            movementPaused = true;

            yield return new WaitUntil(() => !isMoving);
            //invulnerable = true;
            StartCoroutine(ShakePush(dir));

        }

        yield return new WaitUntil(() => !shake);

        char col = (char)(transitSquare.SquareColumn + dir.x);
        int row = transitSquare.SquareRow + dir.y;

        if (KnightsBoardManager.instance.squares.TryGetValue(col.ToString() + row, out var front))
        {
            if (front.knight != null && grounded)
            {
                yield return StartCoroutine(PushForce(front.knight, dir, 1, allowIce: true));
            }
        }
        KnightsGameManager.instance.canMove = false;
        yield return new WaitUntil(() => !isMoving);
        KnightsGameManager.instance.canMove = true;
    }
    IEnumerator ShakePush(Vector2Int dir)
    {
        string sqName = ((char)(currentSquare.SquareColumn - dir.x)).ToString() + (currentSquare.SquareRow - dir.y).ToString();
        
        if (KnightsBoardManager.instance.GetSquare(sqName) != null)
        {
            yield return StartCoroutine(PushForce(this, -dir, 1, allowIce: true));
            yield return new WaitUntil(() => !isMoving);

            yield return StartCoroutine(PushForce(this, dir, 1, allowIce: true));
            yield return new WaitUntil(() => !isMoving);
        }

        shake = false;
        movementPaused = false;
        invulnerable = false;
    }
    protected override void OnArrive(KnightsSquareScript square)
    {
        base.OnArrive(square);
        KnightsGameManager.instance.canMove = true;
    }
    public override void ConsumeMovementDirection()
    {
        if (shake)
            return;

        base.ConsumeMovementDirection();

        lastMovement = stepsMoved >= 3;
        if (stepsMoved >= 2)
            grounded = true;
    }
}
