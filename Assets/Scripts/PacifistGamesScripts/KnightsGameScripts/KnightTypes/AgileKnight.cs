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
        grounded = true;
    }

    protected override void OnDepart()
    {
        grounded = false;
        base.OnDepart();
    }
    protected override void OnApproach(KnightsSquareScript square)
    {
        if (square.knight == null)
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
        grounded = true;

        if (enemyToPush != null && pushDirection != Vector2Int.zero && enemyStartSquare == square)
        {
            StartCoroutine(PushForce(enemyToPush, pushDirection, 2, allowIce: true));
        }

        enemyToPush = null;
        enemyStartSquare = null;
        pushDirection = Vector2Int.zero;

        if (KnightsGameManager.instance.activeMovements.Count == 1 && KnightsGameManager.instance.activeMovements.Contains(this))
        {
            KnightsGameManager.instance.canMove = true;
        }
    }

    public override void ConsumeMovementDirection()
    {
        base.ConsumeMovementDirection();
        if (stepsMoved >= 3)
            grounded = true;
        else
            grounded = false;
    }
}
