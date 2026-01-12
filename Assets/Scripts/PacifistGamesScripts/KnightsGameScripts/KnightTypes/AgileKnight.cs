using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgileKnight : KnightBehavior
{
    private Vector2Int pushDirection;
    private KnightBehavior enemyToPush;
    private KnightsSquareScript enemyStartSquare;

    protected override void Awake()
    {
        base.Awake();
        movementType = true;
        grounded = false;
    }

    protected override void OnApproach(KnightsSquareScript square)
    {
        if (square.knight == null || square.knight.player == player)
        {
            KnightsGameManager.instance.canMove = true;
            return;
        }

        enemyToPush = square.knight;
        enemyStartSquare = square;

        int dx = square.SquareColumn - transitSquare.SquareColumn;
        int dy = square.SquareRow - transitSquare.SquareRow;

        pushDirection = new Vector2Int(
            dx != 0 ? (int)Mathf.Sign(dx) : 0,
            dy != 0 ? (int)Mathf.Sign(dy) : 0
        );

        KnightsGameManager.instance.canMove = true;
    }

    protected override void OnArrive(KnightsSquareScript square)
    {
        if (enemyToPush != null && pushDirection != Vector2Int.zero)
        {
            enemyToPush.currentSquare = enemyStartSquare;
            enemyStartSquare.knight = enemyToPush;
            enemyStartSquare.empty = false;

            Vector2Int dir = pushDirection;
            StartCoroutine(PushEnemy(enemyToPush, dir, 2));
        }

        enemyToPush = null;
        enemyStartSquare = null;
        pushDirection = Vector2Int.zero;

        KnightsGameManager.instance.canMove = true;
    }

    public IEnumerator PushEnemy(KnightBehavior enemy, Vector2Int dir, int steps)
    {
        KnightsSquareScript current = enemy.currentSquare;

        for (int i = 0; i < steps; i++)
        {
            char newCol = (char)(current.SquareColumn + dir.x);
            int newRow = current.SquareRow + dir.y;
            string name = newCol.ToString() + newRow;

            if (!KnightsBoardManager.instance.squares.TryGetValue(name, out KnightsSquareScript nextSquare))
            {
                Destroy(enemy.gameObject);
                current.knight = null;
                current.empty = true;
                yield break;
            }

            if (!nextSquare.empty && nextSquare.knight != this)
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
