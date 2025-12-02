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
    public int[] possibleMovements;

    void Awake()
    {
        if (currentSquare != null)
        {
            transform.position = currentSquare.pawnPosition;
        }
    }
    private void OnMouseDown()
    {
        ClickManager.instance.selectedPawn = this;
        TrackAllPaths();
    }
    public ChessSquareScript FindNextSquare(int moveAmount)
    {
        int nextRow = currentSquare.SquareRow;
        nextRow += moveAmount;
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

            square.ToggleGlow(true);
            possiblePaths.Add(square);
        }
    }
    public void Deselect()
    {
        ClickManager.instance.selectedPawn = null;
        foreach (var square in possiblePaths)
            square.ToggleGlow(false);

        possiblePaths.Clear();
    }
    public IEnumerator MoveForward(ChessSquareScript nextSquare)
    {
        isMoving = true;
        float elapsed = 0;

        while (elapsed < 1)
        {
            elapsed += Time.deltaTime;

            transform.position = Vector3.Lerp(transform.position, nextSquare.pawnPosition, elapsed);
            yield return null;
        }

        transform.position = nextSquare.pawnPosition;
        currentSquare = nextSquare;

        isMoving = false;
    }
}