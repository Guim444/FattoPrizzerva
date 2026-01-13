using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

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
        if (enemyToPush != null &&
            pushDirection != Vector2Int.zero &&
            enemyStartSquare == square)
        {
            enemyToPush.currentSquare = enemyStartSquare;
            StartCoroutine(PushForce(enemyToPush, pushDirection, 2));
        }

        enemyToPush = null;
        enemyStartSquare = null;
        pushDirection = Vector2Int.zero;

        KnightsGameManager.instance.canMove = true;
    }
    public IEnumerator PushForce(KnightBehavior enemy, Vector2Int dir, int steps)
    {
        if (enemy == null || steps <= 0)
            yield break;

        KnightsSquareScript from = enemy.currentSquare;

        char c = (char)(from.SquareColumn + dir.x);
        int r = from.SquareRow + dir.y;

        if (!KnightsBoardManager.instance.squares.TryGetValue(c.ToString() + r, out KnightsSquareScript next))
        {
            from.knight = null;
            from.empty = true;
            Destroy(enemy.gameObject);
            yield break;
        }

        if (!next.empty)
        {
            KnightBehavior hit = next.knight;

            from.knight = null;
            from.empty = true;

            next.knight = enemy;
            next.empty = false;
            enemy.currentSquare = next;

            yield return StartCoroutine(enemy.SmoothMove(next.knightPosition));

            yield return StartCoroutine(PushForce(hit, dir, steps));
            yield break;
        }

        from.knight = null;
        from.empty = true;

        next.knight = enemy;
        next.empty = false;
        enemy.currentSquare = next;

        yield return StartCoroutine(enemy.SmoothMove(next.knightPosition));

        yield return StartCoroutine(PushForce(enemy, dir, steps - 1));
    }
}
