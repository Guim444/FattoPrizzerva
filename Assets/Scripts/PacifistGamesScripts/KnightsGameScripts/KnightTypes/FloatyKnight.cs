using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloatyKnight : KnightBehavior
{
    public List<KnightsSquareScript> adjacentSquares = new List<KnightsSquareScript>();

    protected override void Awake()
    {
        base.Awake();

        KnightsGameManager.instance.floatyKnights.Add(this);

        movementType = true;
        grounded = true;
    }

    protected override void OnDepart()
    {
        grounded = false;
        base.OnDepart();
        invulnerable = true;
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
                StartCoroutine(PushForce(front.knight, dir, 1, allowIce: true, waitTime: 0.5f));
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
    }

    public override void StepOnSquare(KnightsSquareScript sq, bool isGrounded)
    {
        base.StepOnSquare(sq, isGrounded);
        if (isGrounded)
        {
            CalcAdjacent(sq);
        }

        foreach (KnightsSquareScript square in adjacentSquares)
        {
            FloatyKnightAdjacentHandler(this);
        }
    }

    public void CalcAdjacent(KnightsSquareScript sq)
    {
        foreach (var old in adjacentSquares)
        {
            old.knightAdjacent.Remove(this);
        }

        adjacentSquares.Clear();

        Vector2Int[] directions =
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
        };

        foreach (var direction in directions)
        {
            char targetCol = (char)(sq.SquareColumn + direction.x);
            int targetRow = sq.SquareRow + direction.y;

            string key = targetCol.ToString() + targetRow;

            if (KnightsBoardManager.instance.squares.TryGetValue(key, out var adj))
            {
                adjacentSquares.Add(adj);

                if (!adj.knightAdjacent.Contains(this))
                    adj.knightAdjacent.Add(this);
            }
        }
    }

    public override void ConsumeMovementDirection()
    {
        base.ConsumeMovementDirection();
        lastMovement = stepsMoved >= 3;
        grounded = lastMovement;
    }

    internal void FloatyKnightAdjacentHandler(KnightBehavior knightBehavior)
    {
        bool anyKnightNearby = adjacentSquares.Any(adj => adj.knight != null);

        if (anyKnightNearby)
        {
            invulnerable = false;
        }
        else
        {
            invulnerable = true;
        }
    }
}