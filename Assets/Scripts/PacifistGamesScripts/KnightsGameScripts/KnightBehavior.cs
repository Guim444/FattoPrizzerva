using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class KnightBehavior : MonoBehaviour
{
    public List<KnightsSquareScript> possiblePaths = new List<KnightsSquareScript>();
    public KnightsSquareScript currentSquare;
    public KnightsSquareScript transitSquare;

    protected Renderer rend;
    protected Material mat;

    public bool movementType; // true = long then short, false = short then long
    public bool grounded = true; // true = hits obstacles, false = ignores. Default true.
    public bool isMoving = false;

    public int player;

    protected virtual void Awake()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;
    }

    protected virtual void OnMouseDown()
    {
        if (player == KnightsGameManager.instance.currentPlayer)
        {
            KnightsGameManager.instance.selectedKnight = this;
            CalcPaths();
        }
    }

    public virtual void CalcPaths()
    {
        foreach (var sq in possiblePaths)
        {
            sq.selectableSquare = false;
            sq.ToggleGlow(false, 1);
        }
        possiblePaths.Clear();

        int longStep = movementType ? 2 : 1;
        int shortStep = movementType ? 1 : 2;

        Vector2Int[] moveOffsets = new Vector2Int[]
        {
            new Vector2Int( longStep,  shortStep),
            new Vector2Int( longStep, -shortStep),
            new Vector2Int(-longStep,  shortStep),
            new Vector2Int(-longStep, -shortStep),
            new Vector2Int( shortStep,  longStep),
            new Vector2Int( shortStep, -longStep),
            new Vector2Int(-shortStep,  longStep),
            new Vector2Int(-shortStep, -longStep)
        };

        foreach (Vector2Int offset in moveOffsets)
        {
            char col = (char)(currentSquare.SquareColumn + offset.x);
            int row = currentSquare.SquareRow + offset.y;

            KnightsSquareScript target =
                KnightsBoardManager.instance.GetSquare(col.ToString() + row);

            if (target == null)
                continue;

            if (CanMoveTo(target))
            {
                possiblePaths.Add(target);
                target.selectableSquare = true;
                target.ToggleGlow(true, 1);
            }
        }
    }

    protected virtual bool CanMoveTo(KnightsSquareScript target)
    {
        return target.empty || (target.knight != null && target.knight.player != player);
    }

    public virtual IEnumerator MoveKnight(KnightsSquareScript targetSquare)
    {
        KnightsGameManager.instance.canMove = false;

        isMoving = true;

        KnightsSquareScript startSquare = currentSquare;
        startSquare.empty = true;
        startSquare.knight = null;

        transitSquare = startSquare;

        List<KnightsSquareScript> path = GetPath(startSquare, targetSquare);

        foreach (var sq in path)
        {
            KnightsSquareScript from = currentSquare;

            currentSquare.knight = null;
            currentSquare.empty = true;

            yield return StartCoroutine(SmoothMove(sq.knightPosition));

            transitSquare = from;

            yield return StartCoroutine(OnApproachCoroutine(sq));

            currentSquare = sq;
            sq.knight = this;
            sq.empty = false;

            transitSquare = null;
        }

        currentSquare = targetSquare;
        transitSquare = null;

        if (!grounded)
        {
            targetSquare.empty = false;
            targetSquare.knight = this;
        }

        yield return StartCoroutine(OnArriveCoroutine(targetSquare));
        isMoving = false;
    }


    IEnumerator OnApproachCoroutine(KnightsSquareScript square)
    {
        OnApproach(square);
        while (!KnightsGameManager.instance.canMove)
            yield return null;
        yield return new WaitForSeconds(0.5f);
    }
    IEnumerator OnArriveCoroutine(KnightsSquareScript square)
    {
        OnArrive(square);
        while (!KnightsGameManager.instance.canMove)
            yield return null;
        yield return new WaitForSeconds(0.5f);
    }
    protected virtual void OnApproach(KnightsSquareScript square) { }
    protected virtual void OnArrive(KnightsSquareScript square) { }

    protected virtual List<KnightsSquareScript> GetPath(KnightsSquareScript start, KnightsSquareScript end)
    {
        List<KnightsSquareScript> path = new List<KnightsSquareScript>();

        int dx = end.SquareColumn - start.SquareColumn;
        int dy = end.SquareRow - start.SquareRow;

        int stepX = dx == 0 ? 0 : dx / Mathf.Abs(dx);
        int stepY = dy == 0 ? 0 : dy / Mathf.Abs(dy);

        bool longIsX = Mathf.Abs(dx) > Mathf.Abs(dy);
        bool longFirst = movementType;

        char col = start.SquareColumn;
        int row = start.SquareRow;

        for (int i = 0; i < 3; i++)
        {
            bool doLong = longFirst ? i < 2 : i > 0;

            if (doLong)
            {
                if (longIsX) col += (char)stepX;
                else row += stepY;
            }
            else
            {
                if (longIsX) row += stepY;
                else col += (char)stepX;
            }

            path.Add(KnightsBoardManager.instance.GetSquare(col.ToString() + row));
        }

        return path;
    }

    public IEnumerator SmoothMove(Vector3 targetPos)
    {
        float elapsed = 0f;
        float duration = 0.3f;

        Vector3 startPos = transform.position;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed);
            yield return null;
        }

        transform.position = targetPos;
    }

    public void Deselect()
    {
        foreach (var sq in possiblePaths)
        {
            sq.selectableSquare = false;
            sq.ToggleGlow(false, 1);
        }
        possiblePaths.Clear();
    }
}
