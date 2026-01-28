using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Splines.ExtrusionShapes;

public abstract class KnightBehavior : MonoBehaviour
{
    public List<KnightsSquareScript> possiblePaths = new List<KnightsSquareScript>();
    public KnightsSquareScript currentSquare;
    public KnightsSquareScript transitSquare;
    public KnightsSquareScript previousSquare;

    protected Renderer rend;
    protected Material mat;

    public bool movementType; // true = long then short, false = short then long
    public bool grounded = true; // true = hits obstacles, false = ignores. Default true.
    public bool isMoving = false;
    public bool canContinue = true;

    public bool isDead = false;
    bool spikyRockCrash = false;

    public Vector2Int slideDirection; //for ice squares.
    public Vector2Int lookingDirection;
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
        if (KnightsGameManager.instance.gameHasStarted)
        {
            if (player == KnightsGameManager.instance.currentPlayer && KnightsGameManager.instance.playerIsActive)
            {
                KnightsGameManager.instance.selectedKnight = this;
                CalcPaths();
            }
        }
        else
        {
            if (player == KnightsGameManager.instance.currentPlayer)
            {
                KnightsGameManager.instance.selectedKnight = this;
                ToggleGlow(true, 0.5f);
            }
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

                Quaternion arrowRot;
                bool longIsX = Mathf.Abs(offset.x) > Mathf.Abs(offset.y);

                Vector3 finalPosition = new Vector3(currentSquare.knightPosition.x, 0.655f, currentSquare.knightPosition.z);

                GameObject arrow =
                    movementType
                    ? Instantiate(KnightsGameManager.instance.arrowPrefab, finalPosition, Quaternion.identity)
                    : Instantiate(KnightsGameManager.instance.invertedArrowPrefab, finalPosition, Quaternion.identity);

                LArrow arrowScript = arrow.GetComponent<LArrow>();
                arrowScript.target = target;
                arrowScript.knight = this;

                KnightsGameManager.instance.arrows.Add(arrow);

                bool flip;
                if (longIsX)
                {
                    if (offset.x > 0)
                    {
                        arrowRot = Quaternion.Euler(90, 0, 0);
                        flip = offset.y > 0;
                    }
                    else
                    {
                        arrowRot = Quaternion.Euler(90, 0, 180);
                        flip = offset.y < 0;
                    }
                }
                else
                {
                    if (offset.y > 0)
                    {
                        arrowRot = Quaternion.Euler(90, 0, 90);
                        flip = offset.x < 0;
                    }
                    else
                    {
                        arrowRot = Quaternion.Euler(90, 0, -90);
                        flip = offset.x > 0;
                    }
                }

                arrow.transform.rotation = arrowRot;
                FlipSpriteAndCollider(arrow, flip);

                List<KnightsSquareScript> path = GetPath(currentSquare, target);

                foreach (var sq in path)
                {
                    if (sq == target)
                        continue;

                    sq.pathSquare = true;
                }
            }
        }
    }

    protected virtual bool CanMoveTo(KnightsSquareScript target)
    {
        return target.empty || target.knight != null || target.rock != null;
    }

    public virtual IEnumerator MoveKnight(KnightsSquareScript targetSquare)
    {
        KnightsGameManager.instance.playerIsActive = false;

        canContinue = true;
        stepsMoved = 0;
        bool terrainManipulation = false;

        OnDepart();

        KnightsSquareScript startSquare = currentSquare;
        startSquare.knight = null;
        startSquare.empty = true;

        transitSquare = startSquare;

        List<KnightsSquareScript> path = GetPath(startSquare, targetSquare);
        movementDirections = SavePathDirections(path);


        if (movementDirections.Length > 0)
            slideDirection = movementDirections[0];

        lookingDirection = movementDirections[0];

        int i = 0;

        while (i < path.Count && stepsMoved < 3)
        {
            lookingDirection = new Vector2Int(path[i].SquareColumn - currentSquare.SquareColumn, path[i].SquareRow - currentSquare.SquareRow);

            if (!CheckRow(lookingDirection))
            {
                canContinue = false;
            }
            yield return StartCoroutine(WaitWhileOtherMovementsActive());

            KnightsSquareScript sq = path[i];
            KnightsSquareScript from = currentSquare;

            if (grounded && canContinue)
            {
                currentSquare.knight = null;
                currentSquare.empty = true;
            }

            transitSquare = from;

            KnightsGameManager.instance.BeginMovement(this);

            yield return StartCoroutine(OnApproachCoroutine(sq));

            if (canContinue)
            {
                previousSquare = currentSquare;
                yield return StartCoroutine(SmoothMove(sq.knightPosition, 0.3f));

                KnightsGameManager.instance.EndMovement(this);

                if (!sq.isIceSquare || !grounded)
                    ConsumeMovementDirection();
                
                currentSquare = sq;

                if (grounded)
                {
                    sq.knight = this;
                    sq.empty = false;
                }

                if (sq.isIceSquare && grounded)
                {
                    terrainManipulation = true;

                    slideDirection = new Vector2Int(sq.SquareColumn - from.SquareColumn, sq.SquareRow - from.SquareRow);

                    KnightsGameManager.instance.BeginMovement(this);
                    yield return StartCoroutine(SlideOnIce());
                    KnightsGameManager.instance.EndMovement(this);
                        
                    if (stepsMoved >= 3)
                        break;

                    path = RecalculatePathFromDirections(currentSquare);
                    i = 0;
                    continue;
                }

                i++;
            }
            else
            {

                Debug.Log("Pasa");
                stepsMoved = 3;

                if (spikyRockCrash)
                {
                    Debug.Log("Por aqui?");
                    yield return StartCoroutine(GetImpaled(sq.rock));
                }
                else if (!grounded)
                {
                    yield return StartCoroutine(ForceLanding(currentSquare.knightPosition));

                    grounded = true;
                    canContinue = true;

                    currentSquare.knight = this;
                    currentSquare.empty = false;
                }

                KnightsGameManager.instance.EndMovement(this);

                yield return new WaitUntil(() => KnightsGameManager.instance.canMove);
                KnightsGameManager.instance.NextPlayer();
                yield break;
            }

            if (isDead)
            {
                i = path.Count;
                stepsMoved = 3;
            }

            transitSquare = null;
        }

        if (isDead)
        {
            yield return new WaitUntil(() => KnightsGameManager.instance.canMove);
            KnightsGameManager.instance.NextPlayer();
            yield break;
        }

        targetSquare = currentSquare;
        targetSquare.knight = this;
        targetSquare.empty = false;

        yield return StartCoroutine(OnArriveCoroutine(targetSquare));

        yield return new WaitUntil(() => KnightsGameManager.instance.canMove);
        KnightsGameManager.instance.NextPlayer();
    }

    IEnumerator OnApproachCoroutine(KnightsSquareScript square)
    {
        if (!canContinue)
        {
            stepsMoved = 2;

            if (!grounded)
                StartCoroutine(SmoothMove(currentSquare.knightPosition, 0.3f));
            else
                stepsMoved++;

            KnightsGameManager.instance.EndMovement(this);
        }
        else if (square.isVoid && grounded)
        {
            isDead = true;
        }
        else if (square.rock != null && square.rock.dangerousSquares.Contains(currentSquare))
        {
            Debug.Log("F");
            spikyRockCrash = true;
            canContinue = false;

            KnightsGameManager.instance.EndMovement(this);
        }

        if (canContinue) //true by default
            OnApproach(square);

        yield return new WaitUntil(() => KnightsGameManager.instance.canMove);

        if (stepsMoved < 3)
            yield return new WaitForSeconds(0.1f);
    }
    IEnumerator OnArriveCoroutine(KnightsSquareScript square)
    {
        if (square.isVoid && grounded)
        {
            yield return StartCoroutine(KillKnight(square));
        }
        else if (square.rock != null)
        {
            currentSquare = previousSquare;
            yield return StartCoroutine(ForceLanding(currentSquare.knightPosition));
            OnArrive(currentSquare);
        }
        else
        {
            OnArrive(square);
        }
        yield return new WaitUntil(() => KnightsGameManager.instance.canMove);
    }
    protected virtual void OnDepart() { }
    protected virtual void OnApproach(KnightsSquareScript square) { }
    protected virtual void OnArrive(KnightsSquareScript square) { }

    bool CheckRow(Vector2Int dir)
    {
        char col = currentSquare.SquareColumn;
        int row = currentSquare.SquareRow;

        while (true)
        {
            KnightsSquareScript prevSq = KnightsBoardManager.instance.GetSquare(col.ToString() + row);

            col += (char)dir.x;
            row += dir.y;

            KnightsSquareScript sq = KnightsBoardManager.instance.GetSquare(col.ToString() + row);

            if (sq == null)
            {
                //Border situation. It may push a knight to its death.
                return true;
            }

            if (sq.knight != null)
            {
                // Found a knight
                char nextCol = (char)(col + lookingDirection.x);
                int nextRow = row + lookingDirection.y;

                KnightsSquareScript nextSq =
                    KnightsBoardManager.instance.GetSquare(nextCol.ToString() + nextRow);

                if (grounded && nextSq != null && nextSq.rock != null && nextSq.rock.spikes.Count == 0)
                {
                    return false;
                }

                sq.knight.lookingDirection = lookingDirection;
                continue;
            }

            if (sq.rock != null)
            {
                //Found a rock
                if (sq.rock.spikes.Count == 0)
                {
                    if (grounded || (!grounded && sq.rock.isTall))
                    {
                        return false;
                    }
                }
                else if (sq.rock.dangerousSquares.Contains(prevSq))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (sq.empty)
            {
                //Found a free square
                return true;
            }
        }
    }


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
    

    public IEnumerator SmoothMove(Vector3 targetPos, float duration)
    {
        isMoving = true;
        float elapsed = 0f;

        Vector3 startPos = transform.position;

        float jump = grounded? 0 : 1.5f;

        while (elapsed < 1f)
        {
            if (stepsMoved >= 2)
            {
                jump = 0;
            }

            elapsed += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, new Vector3(targetPos.x, targetPos.y + jump, targetPos.z), elapsed);
            yield return null;
        }

        transform.position = new Vector3(targetPos.x, targetPos.y + jump, targetPos.z);
        isMoving = false;
    }

    public IEnumerator ForceLanding(Vector3 targetPos)
    {
        grounded = true;

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
    public IEnumerator GetImpaled(RockObstacleScript rock)
    {
        isDead = true;

        grounded = true;
        isMoving = true;

        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(
            rock.currentSquare.knightPosition.x * 4 + currentSquare.knightPosition.x,
            (rock.currentSquare.knightPosition.y + 0.25f) * 5, //just *5 cuz at the end of the line we do /5. I want "rock.currentSquare.knightPosition.y + 0.25f" to be the final result.
            rock.currentSquare.knightPosition.z * 4 + currentSquare.knightPosition.z) / 5;

        while (elapsed < 1)
        {
            elapsed += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, endPos, elapsed);
            yield return null;
        }

        isMoving = false;
        KnightsBoardManager.instance.knightList.Remove(this);
        rock.dangerousSquares.Remove(currentSquare);

        GetComponent<BoxCollider>().enabled = false;
    }
    public IEnumerator GetOutOfImpalation(RockObstacleScript rock)
    {
        isMoving = true;
        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 startPos = transform.position;
        Vector3 endPos = currentSquare.knightPosition;

        while (elapsed < 1)
        {
            elapsed += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, endPos, elapsed);
            yield return null;
        }

        isMoving = false;
        KnightsBoardManager.instance.knightList.Add(this);
        rock.dangerousSquares.Add(currentSquare);

        if (KnightsGameManager.instance.currentPlayer == player)
            GetComponent<BoxCollider>().enabled = true;
    }

    public void Deselect()
    {
        if (!KnightsGameManager.instance.gameHasStarted)
        {
            ToggleGlow(false, 1);
        }
        foreach (var sq in possiblePaths)
        {
            sq.selectableSquare = false;
        }
        foreach (var arrow in KnightsGameManager.instance.arrows)
        {
            Destroy(arrow);
        }

        foreach (var sq in KnightsBoardManager.instance.squares.Values)
        {
            if (sq.pathSquare)
            {
                sq.pathSquare = false;
                sq.ToggleGlow(false, 1);
            }
        }

        KnightsGameManager.instance.arrows.Clear();
        possiblePaths.Clear();
        KnightsGameManager.instance.selectedKnight = null;
    }
    protected virtual IEnumerator PushForce(KnightBehavior enemy, Vector2Int dir, int steps, bool allowIce = true)
    {
        if (enemy == null || steps <= 0)
            yield break;

        if (steps > 1)
            enemy.grounded = false;
        else
        {
            enemy.grounded = true;
        }

        enemy.stepsMoved = 0;

        KnightsSquareScript from = enemy.currentSquare;

        char c = (char)(from.SquareColumn + dir.x);
        int r = from.SquareRow + dir.y;

        if (!KnightsBoardManager.instance.squares.TryGetValue(c.ToString() + r, out var next) || (next.isVoid && enemy.grounded))
        {
            if (next != null)
            {
                StartCoroutine(enemy.SmoothMove(next.knightPosition, 0.3f));
                yield return new WaitUntil(() => !enemy.isMoving);
            }

            from.knight = null;
            from.empty = true;
            KnightsGameManager.instance.EndMovement(enemy); // This ensure that the movement ends when the enemy is destroyed.
            KnightsBoardManager.instance.knightList.Remove(enemy);

            if (KnightsGameManager.instance.movementsInTheRound.Contains(enemy))
                KnightsGameManager.instance.movementsInTheRound.Remove(enemy);

            Destroy(enemy.gameObject);
            yield break;
        }

        KnightsSquareScript origin = enemy.currentSquare;
        KnightsSquareScript destination = next;

        bool willPush = true;
        KnightsSquareScript enemyEndSquare = CalcFinalSquare(next, dir, steps);
        if (enemyEndSquare != null && enemyEndSquare.rock != null)
        {
            Debug.Log("A");
            willPush = false;

            if (next.rock.dangerousSquares.Count > 0 && next.rock.dangerousSquares.Contains(currentSquare))
            {
                currentSquare.knight = this;
                currentSquare.empty = false;

                KnightsGameManager.instance.BeginMovement(enemy);
                yield return StartCoroutine(enemy.GetImpaled(next.rock));
                KnightsGameManager.instance.EndMovement(enemy);
            }
            yield break;
        }

        if (!next.empty && willPush)
        {
            yield return StartCoroutine(PushForce(next.knight, dir, steps, allowIce));
        }

        origin.knight = null;
        origin.empty = true;

        if (enemy.grounded)
        {
            destination.knight = enemy;
            destination.empty = false;
        }

        enemy.currentSquare = destination;

        if (destination != null)
            KnightsGameManager.instance.movementsInTheRound.Add(enemy);

        yield return StartCoroutine(PushMoveCoroutine(enemy, destination.knightPosition));

        if (allowIce && next.isIceSquare)
        {
            enemy.currentSquare = next;
            next.knight = enemy;
            next.empty = false;

            currentSquare.knight = this;
            currentSquare.empty = false;

            enemy.slideDirection = dir;
            yield return null;

            KnightsGameManager.instance.BeginMovement(enemy);
            yield return StartCoroutine(enemy.SlideOnIce());
            yield return new WaitUntil(() => !enemy.isMoving);
            KnightsGameManager.instance.EndMovement(enemy);
        }

        yield return StartCoroutine(PushForce(enemy, dir, steps - 1, allowIce));
    }
    internal KnightsSquareScript CalcFinalSquare(KnightsSquareScript current, Vector2Int direction, int steps)
    {
        return current;
    }
    private IEnumerator PushMoveCoroutine(KnightBehavior knight, Vector3 targetPos)
    {
        yield return knight.SmoothMove(targetPos, 0.3f);
    }
    public IEnumerator SlideOnIce()
    {
        while (true)
        {
            yield return StartCoroutine(WaitWhileOtherMovementsActive());

            char c = (char)(currentSquare.SquareColumn + slideDirection.x);
            int r = currentSquare.SquareRow + slideDirection.y;

            if (!KnightsBoardManager.instance.squares.TryGetValue(c.ToString() + r, out var next))
                yield break;

            if (!next.empty && next.knight != null)
            {
                if (CanPushFromIce(next.knight, slideDirection))
                {
                    yield return StartCoroutine(PushForce(next.knight, slideDirection, 1, allowIce: true));
                }
                else
                {
                    currentSquare.knight = this;
                    currentSquare.empty = false;

                    yield return StartCoroutine(SmoothMove(currentSquare.knightPosition, 0.3f));

                    yield break;
                }
            }

            if (!next.empty)
            {
                if (next.rock != null && next.rock.dangerousSquares.Count > 0 && next.rock.dangerousSquares.Contains(currentSquare))
                {
                    currentSquare.knight = null;
                    currentSquare.empty = true;
                    KnightsGameManager.instance.BeginMovement(this);
                    yield return StartCoroutine(GetImpaled(next.rock));
                    KnightsGameManager.instance.EndMovement(this);
                }
                
                yield break;
            }

            KnightsSquareScript from = currentSquare;

            from.knight = null;
            from.empty = true;

            currentSquare = next;
            next.knight = this;
            next.empty = false;

            yield return StartCoroutine(SmoothMove(next.knightPosition, 0.3f));

            if (!next.isIceSquare)
                yield break;
        }
    }

    bool CanPushFromIce(KnightBehavior enemy, Vector2Int dir)
    {
        char col = enemy.currentSquare.SquareColumn;
        int row = enemy.currentSquare.SquareRow;

        while (true)
        {
            col += (char)dir.x;
            row += dir.y;

            KnightsSquareScript sq =
                KnightsBoardManager.instance.GetSquare(col.ToString() + row);

            if (sq == null)
                return false;

            if (sq.rock != null)
                return false;

            if (sq.empty)
                return true;

            if (sq.knight != null)
                continue;
        }
    }

    protected IEnumerator WaitWhileOtherMovementsActive()
    {
        yield return new WaitUntil(() => KnightsGameManager.instance.activeMovements.Count == 0 || (KnightsGameManager.instance.activeMovements.Count == 1 && KnightsGameManager.instance.activeMovements.Contains(this)));
    }

    public static void FlipSpriteAndCollider(GameObject obj, bool flipX)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        LArrow arrow = obj.GetComponent<LArrow>();

        if (sr != null)
            sr.flipX = flipX;

        if (arrow != null && arrow.boxColliders != null)
        {
            foreach (var collider in arrow.boxColliders)
            {
                Vector3 originalCenter = collider.Value;
                originalCenter.x *= flipX? -1 : 1;
                collider.Key.center = originalCenter;
            }
        }
    }
    IEnumerator KillKnight(KnightsSquareScript square)
    {
        yield return new WaitUntil(() => !isMoving);
        if (square.knight != null)
        {
            square.knight = null;
            square.empty = true;
            square = null;
        }

        KnightsBoardManager.instance.knightList.Remove(this);

        if (KnightsGameManager.instance.movementsInTheRound.Contains(this))
            KnightsGameManager.instance.movementsInTheRound.Remove(this);

        Destroy(gameObject);
    }

    public void ToggleGlow(bool glow, float intensity)
    {
        if (glow)
        {
            Color glowColor = mat.color;

            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", glowColor * intensity);

            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
        else
        {
            mat.SetColor("_EmissionColor", Color.white);
            mat.DisableKeyword("_EMISSION");
        }
    }

    public IEnumerator WaterCourseCoroutine(KnightsSquareScript sq)
    {
        canContinue = true;
        KnightsGameManager.instance.BeginMovement(this);

        char col = (char)(sq.SquareColumn + sq.waterCourseDirection.x);
        int row = (int)(sq.SquareRow + sq.waterCourseDirection.y);
        KnightsSquareScript target = KnightsBoardManager.instance.GetSquare(col.ToString() + row);

        if (sq.waterCourseDirection == Vector2Int.zero || !CheckRow(sq.waterCourseDirection))
        {
            canContinue = false;
        }

        if (sq.waterCourseDirection == Vector2Int.zero)
        {
            yield return StartCoroutine(KillKnight(sq));
        }

        if (canContinue)
        {
            if (target.knight != null)
            {
                KnightBehavior pushedKnight = target.knight;

                yield return StartCoroutine(PushForce(pushedKnight, sq.waterCourseDirection, 1, true));

                yield return new WaitUntil(() => pushedKnight == null || !pushedKnight.isMoving);

                if (pushedKnight != null && pushedKnight.currentSquare != null && pushedKnight.currentSquare.isWaterSquare)
                {
                    yield return StartCoroutine(pushedKnight.WaterCourseCoroutine(pushedKnight.currentSquare));
                }
            }

            yield return new WaitForSeconds(0.1f);
            StartCoroutine(SmoothMove(target.knightPosition, 0.6f));
                
            yield return new WaitUntil(() => !isMoving);
            KnightsGameManager.instance.EndMovement(this);
            currentSquare.knight = null;
            currentSquare.empty = true;

            currentSquare = target;

            target.knight = this;
            target.empty = false;
        }
    }
}