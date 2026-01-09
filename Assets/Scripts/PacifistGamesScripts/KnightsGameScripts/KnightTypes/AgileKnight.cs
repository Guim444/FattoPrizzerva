using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AgileKnight : KnightBehavior
{
    private Vector2Int pushDirection;
    private KnightBehavior enemyToPush;

    protected override void Awake()
    {
        base.Awake();
        movementType = true;
        grounded = false;
    }

    protected override void OnApproach(KnightsSquareScript square)
    {
        if (square.knight != null && square.knight.player != player)
        {
            enemyToPush = square.knight;

            int dx = square.SquareColumn - currentSquare.SquareColumn;
            int dy = square.SquareRow - currentSquare.SquareRow;

            int longStep = movementType ? 2 : 1;
            int shortStep = movementType ? 1 : 2;

            if (Mathf.Abs(dx) == longStep && Mathf.Abs(dy) == shortStep)
                pushDirection = new Vector2Int(0, dy > 0 ? 1 : -1);
            else if (Mathf.Abs(dx) == shortStep && Mathf.Abs(dy) == longStep)
                pushDirection = new Vector2Int(dx > 0 ? 1 : -1, 0);
            else
                pushDirection = Vector2Int.zero;
        }
        else
        {
            enemyToPush = null;
            pushDirection = Vector2Int.zero;
        }
    }

    protected override void OnArrive(KnightsSquareScript square)
    {
        if (enemyToPush == null || pushDirection == Vector2Int.zero)
            return;

        StartCoroutine(PushEnemy(enemyToPush, 2));
    }

    public IEnumerator PushEnemy(KnightBehavior enemy, int steps)
    {
        KnightsSquareScript current = enemy.currentSquare;

        for (int i = 0; i < steps; i++)
        {
            char newCol = (char)(current.SquareColumn + pushDirection.x);
            int newRow = current.SquareRow + pushDirection.y;
            string name = newCol.ToString() + newRow;

            if (!KnightsBoardManager.instance.squares.TryGetValue(name, out KnightsSquareScript nextSquare))
            {
                Destroy(enemy.gameObject);
                current.knight = null;
                current.empty = true;
                yield break;
            }

            if (!nextSquare.empty)
                break;

            nextSquare.knight = enemy;
            nextSquare.empty = false;

            enemy.currentSquare = nextSquare;
            yield return StartCoroutine(enemy.SmoothMove(nextSquare.knightPosition));

            current.knight = null;
            current.empty = true;

            current = nextSquare;
        }
    }
}
