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
    public KnightsSquareScript deathSquare;

    protected Renderer rend;
    protected Material mat;

    public bool movementType; // true = long then short, false = short then long
    public bool grounded = true; // true = hits obstacles, false = ignores. Default true.
    public bool isMoving = false;
    public bool canContinue = true;
    public bool movementPaused = false;
    public bool canBreakRocks;
    public bool invulnerable = false;

    public bool isDead = false;
    bool spikyRockCrash = false;
    public bool lastMovement = false;

    public Vector2Int slideDirection; //for ice squares.
    public Vector2Int lookingDirection;
    public Vector2Int[] movementDirections;
    public Vector2Int firstDir, lastDir;

    public int player;

    public int stepsMoved = 0;
    public int moveIndex = 0;

    public int turnsInLava = 0;

    protected bool restartMovement = false;
    protected KnightsSquareScript restartTarget;
    protected virtual void Awake()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;
    }

    void OnDisable()
    {
        StopAllCoroutines();
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
        moveIndex = 0;
        bool terrainManipulation = false;

        OnDepart();

        KnightsSquareScript startSquare = currentSquare;
        startSquare.knight = null;
        startSquare.empty = true;

        transitSquare = startSquare;

        List<KnightsSquareScript> path;

        if (movementDirections == null || movementDirections.Length == 0)
        {
            path = GetPath(startSquare, targetSquare);
            movementDirections = SavePathDirections(path);
        }
        else
        {
            path = RecalculatePathFromDirections(startSquare);
        }


        if (movementDirections.Length > 0)
            slideDirection = movementDirections[0];

        lookingDirection = movementDirections[0];

        moveIndex = 0;

        while (moveIndex < path.Count && stepsMoved < 3)
        {
            /*if (stepsMoved == 2)
                lastMovement = true;*/

            lookingDirection = new Vector2Int(path[moveIndex].SquareColumn - currentSquare.SquareColumn, path[moveIndex].SquareRow - currentSquare.SquareRow);

            yield return StartCoroutine(RotateTowards(lookingDirection));

            if (!CheckRow(lookingDirection))
            {
                canContinue = false;
            }
            yield return StartCoroutine(WaitWhileOtherMovementsActive());

            KnightsSquareScript sq = path[moveIndex];
            KnightsSquareScript from = currentSquare;

            if (grounded && canContinue)
            {
                currentSquare.knight = null;
                currentSquare.empty = true;
            }

            transitSquare = from;

            KnightsGameManager.instance.BeginMovement(this);

            yield return StartCoroutine(OnApproachCoroutine(sq));

            yield return new WaitUntil(() => !movementPaused);

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

                    StepOnSquare(sq, true);
                }
                else
                {
                    StepOnSquare(sq, false);
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
                    moveIndex = 0;
                    continue;
                }

                moveIndex++;
            }
            else
            {
                stepsMoved = 3;
                stepsMoved = 0;

                if (spikyRockCrash)
                {
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

                yield return new WaitUntil(() => KnightsGameManager.instance.canMove && KnightsGameManager.instance.activeMovements.Count == 0);
                yield return StartCoroutine(KnightsGameManager.instance.NextPlayer());
                yield break;
            }

            if (isDead)
            {
                moveIndex = path.Count;
                stepsMoved = 3;
            }

            transitSquare = null;
        }

        if (restartMovement)
        {

            restartMovement = false;

            ResetMovementState();

            grounded = false;
            canContinue = true;

            if (restartTarget != null)
                yield return StartCoroutine(MoveKnight(restartTarget));

            yield break;
        }

        if (isDead)
        {
            KnightsGameManager.instance.EndMovement(this);
            yield return new WaitUntil(() => KnightsGameManager.instance.canMove);
            yield return StartCoroutine(KillKnight(currentSquare));
            yield return StartCoroutine(KnightsGameManager.instance.NextPlayer());
            yield break;
        }

        targetSquare = currentSquare;
        targetSquare.knight = this;
        targetSquare.empty = false;

        yield return StartCoroutine(OnArriveCoroutine(targetSquare));

        yield return new WaitUntil(() => KnightsGameManager.instance.canMove);
        yield return StartCoroutine(KnightsGameManager.instance.NextPlayer());
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
    protected virtual void OnArrive(KnightsSquareScript square)
    {
        if (!KnightsGameManager.instance.movementsInTheRound.Contains(this))
            KnightsGameManager.instance.movementsInTheRound.Add(this);
    }

    public bool CheckRow(Vector2Int dir)
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
                if (sq.rock.isBreakable && canBreakRocks)
                {
                    RockObstacleScript rock = sq.rock;
                    sq.rock = null;

                    if (KnightsBoardManager.instance.obstacles.Contains(rock))
                        KnightsBoardManager.instance.obstacles.Remove(rock);

                    Destroy(rock.gameObject);
                }
                else if (sq.rock.spikes.Count == 0)
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
        if (movementDirections != null && movementDirections.Length > 0)
        {
            Vector2Int dir = movementDirections[0];

            if (stepsMoved == 0)
                firstDir = dir;

            if (stepsMoved == 1)
                lastDir = dir;
        }
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
    /*public IEnumerator GetImpaled(RockObstacleScript rock)
    {
        if (isDead)
        {
            isDead = false;

            isMoving = true;
            float elapsed = 0f;
            float duration = 0.3f;
            Vector3 startPos = transform.position;
            string sqName = ((char)(currentSquare.SquareColumn - lookingDirection.x)).ToString() + (currentSquare.SquareRow - lookingDirection.y).ToString();
            KnightsSquareScript sq = KnightsBoardManager.instance.GetSquare(sqName);

            Vector3 endPos = sq.knightPosition;

            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(startPos, endPos, elapsed);
                yield return null;
            }
            Debug.Log("A");
            isMoving = false;

            if (!sq.empty && sq.knight != null)
            {
                Vector2Int dir;
                do
                {
                    dir = new Vector2Int(Random.Range(0, 2), Random.Range(0, 2));
                }
                while (KnightsBoardManager.instance.GetSquare(((char)(sq.SquareColumn + dir.x)).ToString() + sq.SquareRow + dir.y) == null);

                yield return StartCoroutine(PushForce(sq.knight, dir, 1, allowIce: true));
            }
            currentSquare = sq;

            currentSquare.knight = this;
            currentSquare.empty = false;
            deathSquare = null;

            yield break;
        }
        else
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
            /*KnightsBoardManager.instance.knightList.Remove(this);
            rock.dangerousSquares.Remove(currentSquare);

            GetComponent<BoxCollider>().enabled = false;
            previousSquare = currentSquare;
            currentSquare = rock.currentSquare;

            yield return StartCoroutine(KillKnight(currentSquare));
        }
    }*/

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
            (rock.currentSquare.knightPosition.y + 0.25f) * 5,
            rock.currentSquare.knightPosition.z * 4 + currentSquare.knightPosition.z
        ) / 5;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, endPos, elapsed);
            yield return null;
        }

        isMoving = false;

        previousSquare = currentSquare;

        yield return StartCoroutine(KillKnight(previousSquare));

        rock.dangerousSquares.Remove(previousSquare);
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

        currentSquare = previousSquare;

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
    internal virtual IEnumerator PushForce(KnightBehavior enemy, Vector2Int dir, int steps, bool pushThroughAir = false, bool allowIce = true, float waitTime = 0)
    {
        if (enemy.invulnerable)
            yield break;

        if (enemy == null || steps <= 0)
            yield break;

        if (pushThroughAir && steps > 1)
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
            KnightsGameManager.instance.BeginMovement(enemy);
            if (next != null)
            {
                yield return new WaitForSeconds(waitTime);
                StartCoroutine(enemy.SmoothMove(next.knightPosition, 0.3f));
                yield return new WaitUntil(() => !enemy.isMoving);
            }

            from.knight = null;
            from.empty = true;
            KnightsGameManager.instance.EndMovement(enemy); // This ensure that the movement ends when the enemy is destroyed.
            KnightsBoardManager.instance.knightList.Remove(enemy);

            if (KnightsGameManager.instance.movementsInTheRound.Contains(enemy))
                KnightsGameManager.instance.movementsInTheRound.Remove(enemy);

            if (!KnightsGameManager.instance.movementsInTheRound.Contains(this))
                KnightsGameManager.instance.movementsInTheRound.Add(this);

            //Destroy(enemy.gameObject);

            KnightsSquareScript outside = KnightsBoardManager.instance.GetOutsideSquare("OUT_" + c.ToString() + r);
            if (outside != null)
            {
                enemy.deathSquare = outside;

                if (enemy.currentSquare.knight == enemy)
                {
                    enemy.currentSquare.knight = null;
                    enemy.currentSquare.empty = true;
                }
                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(enemy.SmoothMove(outside.knightPosition, 0.2f));

                StartCoroutine(enemy.KillKnight(outside));
                //currentSquare = null;

                /*if (steps == 0)
                {
                }

                else
                {
                    dir = new Vector2Int(outside.SquareColumn - from.SquareColumn, outside.SquareRow - from.SquareRow);
                    StartCoroutine( hForce(enemy, dir, steps - 1, allowIce));
                }*/
            }
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
            grounded = true;
            StartCoroutine(PushForce(next.knight, dir, steps, pushThroughAir: pushThroughAir, allowIce));
            steps = 1;
            enemy.grounded = true;
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

        if (enemy.grounded)
        {
            StepOnSquare(destination, true);
        }
        else
        {
            StepOnSquare(destination, false);
        }

        yield return new WaitForSeconds(waitTime);
        KnightsGameManager.instance.BeginMovement(enemy);
        yield return StartCoroutine(PushMoveCoroutine(enemy, destination.knightPosition));
        yield return new WaitUntil(() => !enemy.isMoving);
        KnightsGameManager.instance.EndMovement(enemy);


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

        if (steps > 1)
        {
            StartCoroutine(PushForce(enemy, dir, steps - 1, pushThroughAir: pushThroughAir, allowIce));
        }
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
                    StartCoroutine(PushForce(next.knight, slideDirection, 1, allowIce: true));
                }
                else
                {
                    currentSquare.knight = this;
                    currentSquare.empty = false;

                    StartCoroutine(SmoothMove(currentSquare.knightPosition, 0.3f));

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

            if (grounded)
            {
                StepOnSquare(next, true);
            }
            else
            {
                StepOnSquare(next, false);
            }

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
    public IEnumerator KillKnight(KnightsSquareScript square)
    {
        if (invulnerable)
            yield break;

        yield return null;
        isDead = true;
        KnightsGameManager.instance.EndMovement(this);
        KnightsBoardManager.instance.knightList.Remove(this);
        GetComponent<BoxCollider>().enabled = false;
        KnightsBoardManager.instance.deadKnightList.Add(this);

        if (square.isVoid)
        {
            square.TurnVoid(false);
            square.knight = null;
            deathSquare = square;

            square.empty = true;
        }
        else if (square.rock != null)
        {
            if (previousSquare != null && previousSquare.knight == this)
            {
                previousSquare.knight = null;
                previousSquare.empty = true;
            }
            deathSquare = square;
        }
        else if (square.isLava)
        {
            yield return StartCoroutine(SinkInLava(square.knight, true));
            square.knight = null;
            square.empty = true;
            deathSquare = square;
            KnightsGameManager.instance.knightsInLava.Remove(this);

            square.isLava = false;
        }
        else if (square.isWaterSquare)
        {
            string outsideName = "OUT_" + ((char)(square.SquareColumn + square.waterCourseDirection.x)).ToString() + (square.SquareRow + square.waterCourseDirection.y);
            KnightsSquareScript target = KnightsBoardManager.instance.GetOutsideSquare(outsideName);
            yield return StartCoroutine(SmoothMove(target.knightPosition, 0.3f));

            previousSquare.knight = null;
            previousSquare.empty = true;
            deathSquare = target;
        }
        else
        {
            yield return StartCoroutine(FadeOutByFalling(this, true));
        }
        if (KnightsGameManager.instance.movementsInTheRound.Contains(this))
            KnightsGameManager.instance.movementsInTheRound.Remove(this);

        deathSquare = square;

        if (!KnightsGameManager.instance.revivedThisTurn)
        {
            if (player == 1)
                KnightsGameManager.instance.CheckKnights(2);
            else if (player == 2)
                KnightsGameManager.instance.CheckKnights(1);

            KnightsGameManager.instance.revivedThisTurn = true;
        }
    }

    public IEnumerator ReviveKnight(KnightsSquareScript sq)
    {
        Debug.Log("A");
        yield return null;
        isDead = false;
        mat.color = player == 1 ? Color.skyBlue : Color.red;

        KnightsBoardManager.instance.deadKnightList.Remove(this);
        KnightsBoardManager.instance.knightList.Add(this);
        GetComponent<BoxCollider>().enabled = true;

        if (KnightsBoardManager.instance.lavaSquares.Contains(sq) || KnightsBoardManager.instance.lavaStartSquaresPlayer1.Contains(sq) || KnightsBoardManager.instance.lavaStartSquaresPlayer2.Contains(sq))
        {
            sq.isLava = true;
            if (!sq.empty && sq.knight != null)
            {
                turnsInLava = 0;
                Vector2Int dir;
                do
                {
                    dir = new Vector2Int(Random.Range(0, 2), Random.Range(0, 2));
                }
                while (KnightsBoardManager.instance.GetSquare(((char)(sq.SquareColumn + dir.x)).ToString() + (sq.SquareRow + dir.y).ToString()) == null);

                StartCoroutine(PushForce(sq.knight, dir, 1, allowIce: true));
            }
            sq.knight = this;
            sq.empty = false;
            yield return StartCoroutine(SinkInLava(sq.knight, false));
            deathSquare = null;
        }
        else if (deathSquare.rock != null)
        {
            yield return StartCoroutine(GetOutOfImpalation(deathSquare.rock));
        }
        else
        {
            GetComponent<Renderer>().enabled = true;
            if (!sq.empty && sq.knight != null)
            {
                StartCoroutine(PushForce(sq.knight, -lookingDirection, 1, allowIce: true));
            }

            sq.knight = this;
            sq.empty = false;
            yield return StartCoroutine(SmoothMove(sq.knightPosition, 0.3f));
            deathSquare = null;
        }

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


    public IEnumerator WaterCourseCoroutine(KnightsSquareScript sq, int steps, WaterCourse waterCourse)
    {
        canContinue = true;
        KnightsGameManager.instance.BeginMovement(this);

        if (isDead)
        {
            KnightsGameManager.instance.EndMovement(this);
            yield break;
        }

        /*if (sq.waterCourseDirection == Vector2Int.zero || !CheckRow(sq.waterCourseDirection))
        {
            canContinue = false;
        }*/

        if (sq.waterCourseDirection == Vector2Int.zero)
        {
            canContinue = false;
        }
        else
        {
            char nextCol = (char)(sq.SquareColumn + sq.waterCourseDirection.x);
            int nextRow = sq.SquareRow + sq.waterCourseDirection.y;
            KnightsSquareScript nextSq = KnightsBoardManager.instance.GetSquare(nextCol.ToString() + nextRow);

            if (nextSq != null && (nextSq.isWaterSquare || nextSq.isWaterCourseCrossing))
            {
                if (!CheckRow(sq.waterCourseDirection))
                    canContinue = false;
            }
        }
        if (sq.isWaterCourseCrossing)
        {
            List<Vector2Int> possibleOutcomes = new List<Vector2Int>();

            foreach (WaterCourse wc in KnightsBoardManager.instance.waterCourses)
            {
                if (wc.waterCourseSquares.Contains(sq))
                {
                    possibleOutcomes.Add(wc.courseDirection);
                }
            }

            int i = KnightsGameManager.instance.turnCounter % possibleOutcomes.Count;
            Debug.Log(KnightsGameManager.instance.turnCounter + "%" + possibleOutcomes.Count + "=" + i);
            sq.waterCourseDirection = possibleOutcomes[i];
        }

        char col = (char)(sq.SquareColumn + sq.waterCourseDirection.x);
        int row = (int)(sq.SquareRow + sq.waterCourseDirection.y);
        KnightsSquareScript target = KnightsBoardManager.instance.GetSquare(col.ToString() + row);

        if (target.rock != null && target.rock.dangerousSquares.Contains(currentSquare))
        {
            yield return StartCoroutine(GetImpaled(target.rock));
            currentSquare.empty = true;
            currentSquare.knight = null;
            currentSquare = null;

            deathSquare = target;

            yield break;
        }

        if (canContinue)
        {
            if (target == null)
            {
                KnightsGameManager.instance.EndMovement(this);
                Debug.Log("Target fuera del tablero");
                yield return StartCoroutine(KillKnight(sq));
                yield break;
            }
            else if (target.knight != null)
            {
                if (target.isWaterSquare)
                {
                    if (target.waterCourseDirection == currentSquare.waterCourseDirection)
                    {
                        yield return StartCoroutine(EnemyWaterCourseBehavior(target, sq, steps, waterCourse));
                    }
                    else
                    {
                        canContinue = false;
                        KnightsGameManager.instance.EndMovement(this);
                        yield break;
                    }
                }
            }

            StartCoroutine(SmoothMove(target.knightPosition, 0.6f));

            yield return new WaitUntil(() => !isMoving);

            currentSquare.knight = null;
            currentSquare.empty = true;

            currentSquare = target;

            target.knight = this;
            target.empty = false;

            if (target.isWaterCourseCrossing)
            {
                foreach (WaterCourse wc in KnightsBoardManager.instance.waterCourses)
                {
                    if (wc.waterCourseSquares.Contains(currentSquare) && wc != waterCourse)
                    {
                        sq = currentSquare;
                        sq.waterCourseDirection = wc.courseDirection;
                        yield return StartCoroutine(WaterCourseCoroutine(currentSquare, steps, wc));

                        yield return new WaitUntil(() => KnightsGameManager.instance.activeMovements.Count == 0);
                        KnightsGameManager.instance.movementsInTheRound.Remove(this);
                        sq.waterCourseDirection = new Vector2Int(0, 0);
                    }
                }
            }
            else
            {
                steps--;

                if (steps > 0)
                {
                    yield return StartCoroutine(WaterCourseCoroutine(currentSquare, steps, waterCourse));
                }
                else
                {
                    KnightsGameManager.instance.EndMovement(this);
                }
            }
        }
    }
    public IEnumerator EnemyWaterCourseBehavior(KnightsSquareScript target, KnightsSquareScript sq, int steps, WaterCourse waterCourse)
    {
        KnightBehavior pushedKnight = target.knight;

        StartCoroutine(PushForce(pushedKnight, sq.waterCourseDirection, 1, true));

        yield return new WaitUntil(() => pushedKnight == null || !pushedKnight.isMoving);
        yield return new WaitForSeconds(0.1f); //short pause between push and water course movement

        if (pushedKnight != null && pushedKnight.currentSquare != null && pushedKnight.currentSquare.isWaterSquare)
        {
            yield return StartCoroutine(pushedKnight.WaterCourseCoroutine(pushedKnight.currentSquare, steps, waterCourse));
            yield return new WaitUntil(() => !pushedKnight.isMoving);

            if (KnightsGameManager.instance.movementsInTheRound.Contains(pushedKnight))
                KnightsGameManager.instance.movementsInTheRound.Remove(pushedKnight);

        }
    }

    public virtual void StepOnSquare(KnightsSquareScript sq, bool isGrounded)
    {
        if (sq == null)
            return;

        // Lógica original
        if (isGrounded && sq.isFragile)
        {
            sq.FragileFloor();
        }

        if (!isGrounded)
            return;

        if (sq.snake != null)
        {
            sq.snake.SnakeEffect(this);
        }

        sq.CheckGhostAdjacency(sq);

        if (previousSquare != null)
        {
            foreach (var floaty in KnightsGameManager.instance.floatyKnights)
            {
                bool wasInside = floaty.adjacentSquares.Contains(previousSquare);
                bool isInside = floaty.adjacentSquares.Contains(sq);

                if (wasInside && !isInside)
                {
                    floaty.FloatyKnightAdjacentHandler(this);
                }
            }
        }
        foreach (var floaty in KnightsGameManager.instance.floatyKnights)
        {
            if (floaty.adjacentSquares.Contains(sq))
            {
                floaty.FloatyKnightAdjacentHandler(this);
            }
        }
    }

    public IEnumerator SinkInLava(KnightBehavior knight, bool isSinking)
    {
        Vector3 targetPos;
        Color finalColor;
        if (isSinking)
        {
            finalColor = Color.black;
            targetPos = new Vector3(knight.currentSquare.knightPosition.x, knight.currentSquare.knightPosition.y - 0.6f, knight.currentSquare.knightPosition.z);
        }
        else
        {
            finalColor = player == 1 ? Color.skyBlue : Color.red;
            targetPos = knight.deathSquare.knightPosition;
        }

        float elapsed = 0f;

        Vector3 startPos = transform.position;


        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime / 0.3f;
            transform.position = Vector3.Lerp(startPos, new Vector3(targetPos.x, targetPos.y, targetPos.z), elapsed);
            mat.color = Color.Lerp(mat.color, finalColor, elapsed);
            yield return null;
        }

        transform.position = new Vector3(targetPos.x, targetPos.y, targetPos.z);
    }

    public IEnumerator FadeOutByFalling(KnightBehavior knight, bool fade)
    {
        Color finalColor;
        if (fade)
        {
            finalColor = new Color(0, 0, 0, 0);
        }
        else
        {
            finalColor = player == 1? Color.skyBlue : Color.red;
        }

        float elapsed = 0f;

        Vector3 startPos = transform.position;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime / 0.3f;
            mat.color = Color.Lerp(mat.color, finalColor, elapsed);
            yield return null;
        }

        mat.color = finalColor;

        GetComponent<Renderer>().enabled = false;
    }
    void ResetMovementState()
    {
        stepsMoved = 0;
        moveIndex = 0;

        movementDirections = null;

        slideDirection = Vector2Int.zero;
        lookingDirection = Vector2Int.zero;

        firstDir = Vector2Int.zero;
        lastDir = Vector2Int.zero;

        transitSquare = null;
        previousSquare = null;

        spikyRockCrash = false;
    }

    IEnumerator RotateTowards(Vector2Int dir)
    {
        if (dir == Vector2Int.zero)
            yield break;

        Vector3 lookDir = new Vector3(dir.x, 0, dir.y);
        Quaternion targetRot = Quaternion.LookRotation(lookDir);

        float rotateSpeed = 720f;

        while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.rotation = targetRot;
    }
}