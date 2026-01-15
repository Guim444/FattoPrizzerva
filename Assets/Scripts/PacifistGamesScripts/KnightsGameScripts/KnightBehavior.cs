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
    public Vector2Int[] movementDirections;

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
        stepsMoved = 0;
        bool terrainManipulation = false;

        KnightsGameManager.instance.BeginMovement(this);

        OnDepart();

        KnightsSquareScript startSquare = currentSquare;
        startSquare.empty = true;
        startSquare.knight = null;

        transitSquare = startSquare;

        List<KnightsSquareScript> path = GetPath(startSquare, targetSquare);
        movementDirections = SavePathDirections(path);
        slideDirection = movementDirections[0];

        int i = 0;

        while (i < path.Count && stepsMoved < 3)
        {
            KnightsSquareScript sq = path[i];
            KnightsSquareScript from = currentSquare;

            currentSquare.knight = null;
            currentSquare.empty = true;

            transitSquare = from;

            yield return StartCoroutine(OnApproachCoroutine(sq));
            yield return StartCoroutine(SmoothMove(sq.knightPosition));

            if (!sq.isIceSquare || !grounded)
                ConsumeMovementDirection();

            currentSquare = sq;
            sq.knight = this;
            sq.empty = false;

            if (sq.isIceSquare && grounded)
            {
                terrainManipulation = true;

                slideDirection = new Vector2Int(
                    sq.SquareColumn - from.SquareColumn,
                    sq.SquareRow - from.SquareRow
                );

                yield return StartCoroutine(SlideOnIce());

                if (stepsMoved >= 3)
                    break;

                path = RecalculatePathFromDirections(currentSquare);
                i = 0;
                continue;
            }

            i++;
            transitSquare = null;
        }

        targetSquare = currentSquare; // in case of terrain manipulation, ensure targetSquare is correct. It always will be the currentSquare here.

        if (!terrainManipulation)
            currentSquare = targetSquare;
        transitSquare = null;


        yield return StartCoroutine(OnArriveCoroutine(targetSquare));

        targetSquare.empty = false;
        targetSquare.knight = this;

        KnightsGameManager.instance.EndMovement(this);

    }


    IEnumerator OnApproachCoroutine(KnightsSquareScript square)
    {
        OnApproach(square);

        yield return new WaitUntil(() => KnightsGameManager.instance.canMove);

        if (stepsMoved < 3)
            yield return new WaitForSeconds(0.1f);
    }
    IEnumerator OnArriveCoroutine(KnightsSquareScript square)
    {
        OnArrive(square);

        yield return new WaitUntil(() => KnightsGameManager.instance.canMove);
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
    protected List<KnightsSquareScript> RecalculatePathFromDirections(KnightsSquareScript start)
    {
        List<KnightsSquareScript> path = new();
        char col = start.SquareColumn;
        int row = start.SquareRow;

        foreach (var dir in movementDirections)
        {
            col += (char)dir.x;
            row += dir.y;

            KnightsSquareScript sq = KnightsBoardManager.instance.GetSquare(col.ToString() + row);
            if (sq == null)
                break;

            path.Add(sq);
        }

        return path;
    }
    protected Vector2Int[] SavePathDirections(List<KnightsSquareScript> path)
    {
        if (path == null || path.Count < 2)
            return System.Array.Empty<Vector2Int>();

        movementDirections = new Vector2Int[path.Count - 1];

        for (int i = 0; i < path.Count - 1; i++)
        {
            KnightsSquareScript from = path[i];
            KnightsSquareScript to = path[i + 1];

            movementDirections[i] = new Vector2Int(to.SquareColumn - from.SquareColumn, to.SquareRow - from.SquareRow);
        }

        return movementDirections;
    }
    public virtual void ConsumeMovementDirection()
    {
        stepsMoved++;

        if (movementDirections == null || movementDirections.Length == 0)
            return;

        Vector2Int[] newDirs = new Vector2Int[movementDirections.Length - 1];
        for (int i = 1; i < movementDirections.Length; i++)
            newDirs[i - 1] = movementDirections[i];

        movementDirections = newDirs;
        slideDirection = movementDirections.Length > 0 ? movementDirections[0] : slideDirection;
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
        KnightsSquareScript target = KnightsBoardManager.instance.GetSquare(col.ToString() + row);

        Debug.Log(target.name);

        return target;
    }
    

    public IEnumerator SmoothMove(Vector3 targetPos)
    {
        isMoving = true;
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
        isMoving = false;
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
    protected virtual IEnumerator PushForce(KnightBehavior enemy, Vector2Int dir, int steps, bool allowIce = true)
    {
        if (enemy == null || steps <= 0)
            yield break;

        KnightsSquareScript from = enemy.currentSquare;

        char c = (char)(from.SquareColumn + dir.x);
        int r = from.SquareRow + dir.y;

        if (!KnightsBoardManager.instance.squares.TryGetValue(c.ToString() + r, out var next))
        {
            from.knight = null;
            from.empty = true;
            Destroy(enemy.gameObject);
            yield break;
        }

        if (!next.empty)
        {
            yield return StartCoroutine(PushForce(next.knight, dir, steps, allowIce));
        }

        from.knight = null;
        from.empty = true;

        enemy.currentSquare = next;
        next.knight = enemy;
        next.empty = false;

        yield return StartCoroutine(PushMoveCoroutine(enemy, next.knightPosition));

        if (allowIce && next.isIceSquare)
        {
            enemy.slideDirection = dir;
            yield return null;
            yield return StartCoroutine(enemy.SlideOnIce());
        }

        yield return StartCoroutine(PushForce(enemy, dir, steps - 1, allowIce));
    }
    private IEnumerator PushMoveCoroutine(KnightBehavior knight, Vector3 targetPos)
    {
        KnightsGameManager.instance.BeginMovement(knight);
        yield return knight.SmoothMove(targetPos);
        KnightsGameManager.instance.EndMovement(knight);
    }
    public IEnumerator SlideOnIce()
    {
        while (true)
        {
            char c = (char)(currentSquare.SquareColumn + slideDirection.x);
            int r = currentSquare.SquareRow + slideDirection.y;

            if (!KnightsBoardManager.instance.squares.TryGetValue(c.ToString() + r, out var next))
                yield break;

            if (!next.empty && next.knight != null)
            {
                yield return StartCoroutine(PushForce(next.knight, slideDirection, 1, allowIce: true));
            }

            if (!next.empty)
                yield break;

            KnightsSquareScript from = currentSquare;

            from.knight = null;
            from.empty = true;

            currentSquare = next;
            next.knight = this;
            next.empty = false;

            yield return StartCoroutine(SmoothMove(next.knightPosition));

            if (!next.isIceSquare)
                yield break;
        }
    }

}
