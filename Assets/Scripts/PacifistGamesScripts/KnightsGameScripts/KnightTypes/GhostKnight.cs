using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GhostKnight : KnightBehavior
{
    public List<GhostSquare> adjacentSquares = new List<GhostSquare>();

    protected override void Awake()
    {
        base.Awake();
        movementType = true;
        grounded = true;

        for (int i = 0; i < 4; i++)
        {
            adjacentSquares.Add(new GhostSquare());
        }
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

        CalcAdjacent(square);

        foreach (var adj in adjacentSquares)
        {
            if (adj.sq != null && adj.sq.knight != null)
            {
                adj.occupied = true;
            }
        }

        GhostKnightAdjacentHandler(this);
    }

    public override void StepOnSquare(KnightsSquareScript sq, bool isGrounded)
    {
        base.StepOnSquare(sq, isGrounded);

        if (isGrounded)
        {
            CalcAdjacent(sq);
        }

        GhostKnightAdjacentHandler(this);
    }

    public void CalcAdjacent(KnightsSquareScript sq)
    {
        Vector2Int[] directions =
        {
            new(0, 1),
            new(0, -1),
            new(-1, 0),
            new(1, 0),
        };

        for (int i = 0; i < directions.Length; i++)
        {
            var direction = directions[i];

            char targetCol = (char)(sq.SquareColumn + direction.x);
            int targetRow = sq.SquareRow + direction.y;

            string key = targetCol.ToString() + targetRow;

            if (adjacentSquares[i].sq != null)
                adjacentSquares[i].sq.knightAdjacent.Remove(this);

            if (KnightsBoardManager.instance.squares.TryGetValue(key, out var newSquare))
            {
                adjacentSquares[i].sq = newSquare;

                if (!newSquare.knightAdjacent.Contains(this))
                    newSquare.knightAdjacent.Add(this);
            }
            else
            {
                adjacentSquares[i].sq = null;
            }
        }
    }


    public override void ConsumeMovementDirection()
    {
        base.ConsumeMovementDirection();
        lastMovement = stepsMoved >= 3;
        grounded = lastMovement;
    }

    internal void GhostKnightAdjacentHandler(KnightBehavior knightBehavior)
    {
        bool anyKnightNearby = adjacentSquares.Any(adj => adj.sq != null && adj.sq.knight != null);

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

[Serializable]
public class GhostSquare
{
    public KnightsSquareScript sq;
    public bool occupied;
}