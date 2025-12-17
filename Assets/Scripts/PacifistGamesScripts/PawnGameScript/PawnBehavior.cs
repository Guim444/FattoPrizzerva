using System.Collections;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.UI;

public class PawnBehavior : MonoBehaviour
{
    public ChessSquareScript currentSquare;
    bool isMoving = false; //We avoid strange movements
    public List<ChessSquareScript> possiblePaths = new List<ChessSquareScript>();

    public List<int> possibleMovements, killRange;
    public List<int> tier1Movements, tier2Movements, tier3Movements;
    public List<int> tier1KillRange, tier2KillRange, tier3KillRange;

    public bool canBeEaten = false;

    public int player;

    public char originalBenchColumn; //Only for benched ones

    public bool startingPawn = true;

    public int pawnTier;

    //The different pawn rulesets
    Dictionary<PawnSets, (List<int> t1Moves, List<int> t2Moves, List<int> t3Moves,
                                               List<int> t1Kills, List<int> t2Kills, List<int> t3Kills)>
    PawnRules = new()
    {
        {
            PawnSets.DefaultSet,
            (new(){1}, new(){1}, new(){1}, new(){1}, new(){2}, new(){1,2,3})
        },
        {
            PawnSets.SSet,
            (new(){1}, new(){1}, new(){1}, new(){1,2}, new(){2,3}, new(){1,2,3})
        },
        {
            PawnSets.TSet,
            (new(){1}, new(){1}, new(){1}, new(){1}, new(){1,2}, new(){1,2,3})
        },
        {
        PawnSets.USet,
            (new(){1}, new(){1}, new(){1}, new(){2}, new(){1,2}, new(){1,2,3})
        },
        {
            PawnSets.VSet,
            (new(){1}, new(){1}, new(){1}, new(){1}, new(){2}, new(){1,2})
        },
        {
            PawnSets.WSet,
            (new(){1}, new(){1}, new(){1}, new(){1,2}, new(){1,2,3}, new(){-3,-2,-1,1,2,3})
        },
        {
            PawnSets.XSet,
            (new(){1}, new(){1}, new(){1}, new(){2}, new(){1}, new(){2,3})
        },
        {
            PawnSets.YSet,
            (new(){1}, new(){1}, new(){1,2}, new(){1,2}, new(){1,2,3}, new(){1,2,3})
        },
        {
            PawnSets.ZSet,
            (new(){1}, new(){1}, new(){1,2}, new(){1,2,3}, new(){-3,-2,-1,1,2,3}, new(){-3,-2,-1,1,2,3})
        }
    };

    void Awake()
    {
        if (currentSquare != null)
        {
            transform.position = currentSquare.pawnPosition;
        }
    }
    private void OnMouseDown()
    {
        if (player == PawnsGameManager.instance.activePlayer)
        {
            ClickManager.instance.selectedPawn = this;
            TrackAllPaths(true);
            TrackDiagonals(true);
        }
    }
    public void SetPawnRuleset()
    {
        //First we clear all the lists. Just to avoid issues.
        tier1Movements.Clear();
        tier2Movements.Clear();
        tier3Movements.Clear();
        tier1KillRange.Clear();
        tier2KillRange.Clear();
        tier3KillRange.Clear();

        if (PawnRules.TryGetValue(PawnsGameManager.instance.pawnRuleset, out var rules))
        {
            //we add the ranges needed.
            tier1Movements.AddRange(rules.t1Moves);
            tier2Movements.AddRange(rules.t2Moves);
            tier3Movements.AddRange(rules.t3Moves);

            tier1KillRange.AddRange(rules.t1Kills);
            tier2KillRange.AddRange(rules.t2Kills);
            tier3KillRange.AddRange(rules.t3Kills);
        }
    }
    public ChessSquareScript FindNextSquare(int moveAmount)
    {
        int nextRow = currentSquare.SquareRow;

        //The player 1 moves forward, while the player 2 is 180º rotated so it is a "backwards move".
        if (player == 1)
            nextRow += moveAmount;
        else
            nextRow -= moveAmount;

        string id = currentSquare.SquareColumn.ToString() + nextRow.ToString(); 

        return BoardManager.instance.GetSquare(id);
    }
    public void TrackAllPaths(bool glow)
    {
        foreach (ChessSquareScript sq in possiblePaths)
        {
            sq.ToggleGlow(false);
            sq.selectableSquare = false;
        }
        possiblePaths.Clear();

        foreach (int move in possibleMovements)
        {
            ChessSquareScript square = FindNextSquare(move);
            if (square == null) break;

            if (square.empty)
            {
                //Square found!
                possiblePaths.Add(square);
                square.ToggleGlow(glow);
                if (glow) square.selectableSquare = true;
            }
            else break;
        }
        if (startingPawn && !possibleMovements.Contains(2))
        {
            ChessSquareScript square = FindNextSquare(2);

            if (square != null && square.empty)
            {
                //Square found! But just 1st possible movement
                possiblePaths.Add(square);
                square.ToggleGlow(glow);
                if (glow) square.selectableSquare = true;
            }
        }
    }
    public void TrackDiagonals(bool glow)
    {
            List<ChessSquareScript> diagonalSquares = new List<ChessSquareScript>();

        int direction = (player == 1) ? 1 : -1;

        foreach (int dist in killRange)
        {
            int nextRow = currentSquare.SquareRow + dist * direction;

            char leftCol = (char)(currentSquare.SquareColumn - dist);
            char rightCol = (char)(currentSquare.SquareColumn + dist);

            if (leftCol >= 'A' && leftCol < 'A' + BoardManager.instance.width && nextRow >= 1 && nextRow <= BoardManager.instance.height)
            {
                string id = leftCol.ToString() + nextRow.ToString();
                ChessSquareScript upLeft = BoardManager.instance.GetSquare(id);

                if (upLeft != null && !upLeft.empty && upLeft.pawn != null && upLeft.pawn.player != player)
                {
                    upLeft.pawn.ToggleGlow(glow);
                    upLeft.pawn.canBeEaten = true;
                    if (glow) upLeft.selectableSquare = true;
                    diagonalSquares.Add(upLeft);
                }
            }

            if (rightCol >= 'A' && rightCol < 'A' + BoardManager.instance.width && nextRow >= 1 && nextRow <= BoardManager.instance.height)
            {
                string id = rightCol.ToString() + nextRow.ToString();
                ChessSquareScript upRight = BoardManager.instance.GetSquare(id);

                if (upRight != null && !upRight.empty && upRight.pawn != null && upRight.pawn.player != player)
                {
                    upRight.pawn.ToggleGlow(glow);
                    upRight.pawn.canBeEaten = true;
                    if (glow) upRight.selectableSquare = true;
                    diagonalSquares.Add(upRight);
                }
            }
        }

        possiblePaths.AddRange(diagonalSquares);
    }
    public void Deselect()
    {
        ClickManager.instance.selectedPawn = null;
        foreach (var square in possiblePaths)
        {
            square.selectableSquare = false;
            square.ToggleGlow(false);

            if (square.pawn != null && square.pawn.canBeEaten)
            {
                square.pawn.ToggleGlow(false);
                square.pawn.canBeEaten = false;
            }
        }
        possiblePaths.Clear();
    }
    public IEnumerator MoveForward(ChessSquareScript nextSquare)
    {
        startingPawn = false;
        isMoving = true;
        currentSquare.empty = true;
        currentSquare.pawn = null;
        currentSquare = nextSquare;

        if (!nextSquare.empty)
        {
            nextSquare.pawn.TeleportPawnToGraveyard(player, 1);
        }

        nextSquare.empty = false;
        nextSquare.pawn = this;

        yield return StartCoroutine(SmoothMove(nextSquare.pawnPosition));

        isMoving = false;

        if (player == 1 && currentSquare.SquareRow == BoardManager.instance.height)
        {
            TeleportPawnToGraveyard(player, 2);
            yield return new WaitForSeconds(0.75f);
        }
        else if (player == 2 && currentSquare.SquareRow == 1)
        {
            TeleportPawnToGraveyard(player, 2);
            yield return new WaitForSeconds(0.75f);
        }

        PawnsGameManager.instance.NextPlayerTurn();
    }
    public IEnumerator SmoothMove(Vector3 nextPosition)
    {
        float elapsed = 0;


        while (elapsed < 1)
        {
            elapsed += Time.deltaTime;

            transform.position = Vector3.Lerp(transform.position, nextPosition, elapsed);
            yield return null;
        }

        transform.position = nextPosition;
    }

    public void ToggleGlow(bool value)
    {
        Renderer rend = GetComponent<Renderer>();
        if (value)
        {
            Color glowColor = rend.material.color;
            float intensity = 0.3f;
            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", glowColor * intensity);
        }
        else
        {
            rend.material.SetColor("_EmissionColor", Color.black);
        }
    }
    public void TeleportPawnToGraveyard(int activePlayer, int points) //used when a pawn is captured or promoted
    {
        ToggleGlow(false);
        canBeEaten = false;

        Vector3 graveyardPos;
        if (PawnsGameManager.instance.playerTier[activePlayer - 1] < 3)
        {
            graveyardPos = BoardManager.instance.GetGraveyardPosition(activePlayer, PawnsGameManager.instance.playerTier[activePlayer - 1]);
        }
        else
        {
            graveyardPos = BoardManager.instance.GetGraveyardPosition(activePlayer, 3);
        }
            transform.position = graveyardPos;
        currentSquare.empty = true;
        currentSquare.pawn = null;
        if (player == 1)
        {
            BoardManager.instance.whitePawns.Remove(this);
        }
        else
        {
            BoardManager.instance.blackPawns.Remove(this);
        }
        GetComponent<BoxCollider>().enabled = false;

        PawnsGameManager.instance.AddPoints(activePlayer, points);
    }

    public void SetPawnTier()
    {
        possibleMovements.Clear();
        killRange.Clear();

        switch (pawnTier)
        {
            case 1:
                possibleMovements.AddRange(tier1Movements);
                killRange.AddRange(tier1KillRange);
                break;
            case 2:
                possibleMovements.AddRange(tier2Movements);
                killRange.AddRange(tier2KillRange);
                break;
            case 3:
                possibleMovements.AddRange(tier3Movements);
                killRange.AddRange(tier3KillRange);
                break;
            default:
                possibleMovements.AddRange(tier1Movements);
                killRange.AddRange(tier1KillRange);

                Debug.LogWarning($"{name}: invalid pawnTier ({pawnTier}).");
                break;
        }
    }

    public IEnumerator MoveToBoard(int player, BenchSquare bs)
    {
        ChessSquareScript boardPosition = BoardManager.instance.GetBenchToBoardPosition(player, bs);

        if (boardPosition == null)
        {
            yield break;
        }

        MoveToSquareImmediately(boardPosition);

        if (player == 1)
        {
            BoardManager.instance.whitePawns.Add(this);
        }
        else
        {
            BoardManager.instance.blackPawns.Add(this);
        }

        yield return StartCoroutine(SmoothMove(boardPosition.pawnPosition));

        yield return new WaitForSeconds(1f);
    }


    public void MoveToSquareImmediately(ChessSquareScript targetSquare)
    {
        if (targetSquare == null) return;

        if (currentSquare != null)
        {
            currentSquare.pawn = null;
            currentSquare.empty = true;
        }

        currentSquare = targetSquare;
        targetSquare.pawn = this;
        targetSquare.empty = false;
    }
}