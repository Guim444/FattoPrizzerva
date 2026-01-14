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

    public Vector2Int slideDirection; //for ice squares.

    public int player;

    public int stepsMoved = 0;

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
        return target.empty || target.knight != null;
    }

    public virtual IEnumerator MoveKnight(KnightsSquareScript targetSquare)
    {
        bool terrainManipulation = false;
        KnightsGameManager.instance.canMove = false;

        OnDepart();

        isMoving = true;

        KnightsSquareScript startSquare = currentSquare;
        startSquare.empty = true;
        startSquare.knight = null;

        transitSquare = startSquare;

        List<KnightsSquareScript> path = GetPath(startSquare, targetSquare);

        for (int i = 0; i < path.Count; i++)
        {
            KnightsSquareScript sq = path[i];
            KnightsSquareScript from = currentSquare;

            currentSquare.knight = null;
            currentSquare.empty = true;

            yield return StartCoroutine(SmoothMove(sq.knightPosition));

            transitSquare = from;

            yield return StartCoroutine(OnApproachCoroutine(sq));

            currentSquare = sq;
            sq.knight = this;
            sq.empty = false;

            if (sq.isIceSquare && grounded)
            {
                terrainManipulation = true;

                slideDirection = new Vector2Int(sq.SquareColumn - from.SquareColumn, sq.SquareRow - from.SquareRow);

                yield return StartCoroutine(SlideOnIce());

                int remainingSteps = 3 - stepsMoved;
                if (remainingSteps <= 0)
                    break;

                targetSquare = GetNewTargetSquare(currentSquare, remainingSteps);
                path = GetPath(currentSquare, targetSquare);
                i = -1;
            }


            transitSquare = null;
        }
        if (!terrainManipulation)
            currentSquare = targetSquare;
        transitSquare = null;


        if (!grounded)
        {
            targetSquare.empty = false;
            targetSquare.knight = this;
        }

        yield return StartCoroutine(OnArriveCoroutine(targetSquare));

        isMoving = false;

        KnightsGameManager.instance.canMove = true;
    }


    IEnumerator OnApproachCoroutine(KnightsSquareScript square)
    {
        stepsMoved++;
        OnApproach(square);
        while (!KnightsGameManager.instance.canMove)
            yield return null;
    }
    IEnumerator OnArriveCoroutine(KnightsSquareScript square)
    {
        OnArrive(square);
        while (!KnightsGameManager.instance.canMove)
            yield return null;
    }
    protected virtual void OnDepart() { }
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

        for (int i = stepsMoved; i < 3; i++)
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
    protected List<KnightsSquareScript> GetRemainingPath(List<KnightsSquareScript> originalPath, int remainingSteps)
    {
        List<KnightsSquareScript> newPath = new List<KnightsSquareScript>();

        int startIndex = stepsMoved;
        for (int i = 0; i < remainingSteps; i++)
        {
            if (startIndex + i < originalPath.Count)
            {
                newPath.Add(originalPath[startIndex + i]);
            }
            else
            {
                KnightsSquareScript last = newPath.Count > 0 ? newPath[newPath.Count - 1] : currentSquare;
                char col = last.SquareColumn;
                int row = last.SquareRow;

                int longStep = movementType ? 2 : 1;
                int shortStep = movementType ? 1 : 2;

                bool longFirst = movementType;

                bool doLong = longFirst ? (stepsMoved + i < 2) : (stepsMoved + i > 0);

                if (Mathf.Abs(longStep) > Mathf.Abs(shortStep))
                {
                    if (doLong) col += (char)longStep;
                    else row += shortStep;
                }
                else
                {
                    if (doLong) row += longStep;
                    else col += (char)shortStep;
                }

                KnightsSquareScript next = KnightsBoardManager.instance.GetSquare(col.ToString() + row);
                if (next != null)
                    newPath.Add(next);
            }
        }

        return newPath;
    }
    protected KnightsSquareScript GetNewTargetSquare(KnightsSquareScript start, int remainingSteps)
    {
        char col = start.SquareColumn;
        int row = start.SquareRow;

        int longStep = movementType ? 2 : 1;
        int shortStep = movementType ? 1 : 2;
        bool longFirst = movementType;

        for (int i = 0; i < remainingSteps; i++)
        {
            bool doLong = longFirst ? i < 2 : i > 0;

            if (Mathf.Abs(longStep) > Mathf.Abs(shortStep))
            {
                if (doLong) col += (char)longStep;
                else row += shortStep;
            }
            else
            {
                if (doLong) row += longStep;
                else col += (char)shortStep;
            }
        }

        return KnightsBoardManager.instance.GetSquare(col.ToString() + row);
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

    public IEnumerator SlideOnIce()
    {
        Debug.Log("Hielo");
        while (currentSquare.isIceSquare)
        {
            char c = (char)(currentSquare.SquareColumn + slideDirection.x);
            int r = currentSquare.SquareRow + slideDirection.y;

            if (!KnightsBoardManager.instance.squares.TryGetValue(c.ToString() + r, out var next))
                yield break;

            if (next.isIceSquare && !next.empty)
                yield break;

            KnightsSquareScript from = currentSquare;

            from.knight = null;
            from.empty = true;

            currentSquare = next;
            next.knight = this;
            next.empty = false;

            yield return StartCoroutine(SmoothMove(next.knightPosition));
        }
    }
}
