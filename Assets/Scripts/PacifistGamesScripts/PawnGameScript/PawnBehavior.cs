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
    List<ChessSquareScript> possiblePaths = new List<ChessSquareScript>();

    public List<int> possibleMovements, killRange;
    public List<int> tier1Movements, tier2Movements, tier3Movements;
    public List<int> tier1KillRange, tier2KillRange, tier3KillRange;

    public bool canBeEaten = false;

    public int player;

    public bool startingPawn = true;

    public int pawnTier;

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
            TrackAllPaths();
            TrackDiagonals();
        }
    }
    public ChessSquareScript FindNextSquare(int moveAmount)
    {
        int nextRow = currentSquare.SquareRow;

        if (PawnsGameManager.instance.activePlayer == 1)
        {
            nextRow += moveAmount;
        }
        else nextRow -= moveAmount;

            ChessSquareScript nextSquare = BoardManager.instance.GetSquare(currentSquare.SquareColumn.ToString() + nextRow.ToString());
        if (nextSquare != null)
        {
            return nextSquare;
        }
        return currentSquare;
    }
    public void TrackAllPaths()
    {
        foreach (int move in possibleMovements)
        {
            ChessSquareScript square = FindNextSquare(move);

            if (square.empty)
            {
                square.ToggleGlow(true);
                square.selectableSquare = true;
                possiblePaths.Add(square);
            }
            else return; //If it finds an occupied square, it won't continue.
        }

        if (startingPawn && !possibleMovements.Contains(2))
        {
            ChessSquareScript square = FindNextSquare(2);

            if (square.empty)
            {
                square.ToggleGlow(true);
                square.selectableSquare = true;
                possiblePaths.Add(square);
            }
        }
    }
    public void TrackDiagonals()
    {
        List<ChessSquareScript> diagonalSquares = new List<ChessSquareScript>();

        int direction = player == 1? 1 : -1;

        int nextRow = currentSquare.SquareRow + direction;
        char leftCol = (char)(currentSquare.SquareColumn - 1);
        char rightCol = (char)(currentSquare.SquareColumn + 1);

        if (leftCol >= 'A' && leftCol < 'A' + BoardManager.instance.width && nextRow >= 1 && nextRow <= BoardManager.instance.height)
        {
            string id = leftCol.ToString() + nextRow.ToString();
            ChessSquareScript upLeft = BoardManager.instance.GetSquare(id);

            if (upLeft != null && !upLeft.empty)
            {
                if (upLeft.pawn.player != player)
                {
                    upLeft.pawn.ToggleGlow(true);
                    upLeft.pawn.canBeEaten = true;
                    upLeft.selectableSquare = true;
                    diagonalSquares.Add(upLeft);
                }
            }
        }

        if (rightCol >= 'A' && rightCol < 'A' + BoardManager.instance.width && nextRow >= 1 && nextRow <= BoardManager.instance.height)
        {
            string id = rightCol.ToString() + nextRow.ToString();
            ChessSquareScript upRight = BoardManager.instance.GetSquare(id);

            if (upRight != null && !upRight.empty)
            {
                if (upRight.pawn.player != player)
                {
                    upRight.pawn.ToggleGlow(true);
                    upRight.pawn.canBeEaten = true;
                    upRight.selectableSquare = true;
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

        float elapsed = 0;


        while (elapsed < 1)
        {
            elapsed += Time.deltaTime;

            transform.position = Vector3.Lerp(transform.position, nextSquare.pawnPosition, elapsed);
            yield return null;
        }

        transform.position = nextSquare.pawnPosition;

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
        Vector3 graveyardPos = BoardManager.instance.GetGraveyardPosition(activePlayer, PawnsGameManager.instance.playerTier[activePlayer - 1]);
        transform.position = graveyardPos;
        currentSquare.empty = true;
        currentSquare.pawn = null;
        GetComponent<BoxCollider>().enabled = false;

        PawnsGameManager.instance.AddPoints(activePlayer, points);
    }

    public void SetPawnTier()
    {
        switch (pawnTier)
        {
            case 1:
                foreach (int move in tier1Movements)
                {
                    possibleMovements.Add(move);
                }
                foreach (int kill in tier1KillRange)
                {
                    killRange.Add(kill);
                }
                break;
            case 2:
                foreach (int move in tier2Movements)
                {
                    possibleMovements.Add(move);
                }
                foreach (int kill in tier2KillRange)
                {
                    killRange.Add(kill);
                }
                break;
            case 3:
                foreach (int move in tier3Movements)
                {
                    possibleMovements.Add(move);
                }
                foreach (int kill in tier3KillRange)
                {
                    killRange.Add(kill);
                }
                break;
            default:
                possibleMovements = new List<int>(tier1Movements);
                killRange = new List<int>(tier1KillRange);
                break;
        }
    }
}