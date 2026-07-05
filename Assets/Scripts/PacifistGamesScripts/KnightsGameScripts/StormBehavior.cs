using System;
using System.Collections.Generic;
using UnityEngine;

public class StormBehavior : MonoBehaviour
{
    public bool stormEnabled = true;
    public static StormBehavior instance;
    public int turnsToStart;
    public int turnsToExpand;
    public bool stormStarted = false;
    public int timesExpanded = 0;
    public int turnsSinceLastExpansion = 0;

    public void Awake()
    {
        instance = this;
    }

    internal void ExpandStorm()
    {
        if (!stormEnabled)
            return;
        List<KnightsSquareScript> squaresToVoid = new List<KnightsSquareScript>();

        int minRow = 1 + timesExpanded;
        int maxRow = KnightsBoardManager.instance.height - timesExpanded;

        char minCol = (char)('A' + timesExpanded);
        char maxCol = (char)('A' + KnightsBoardManager.instance.width - 1 - timesExpanded);

        foreach (KnightsSquareScript sq in KnightsBoardManager.instance.squares.Values)
        {
            if (sq.isVoid)
                continue;

            bool outerRing =
                sq.SquareRow == minRow || sq.SquareRow == maxRow || sq.SquareColumn == minCol || sq.SquareColumn == maxCol;

            if (outerRing)
                squaresToVoid.Add(sq);
        }

        foreach (KnightsSquareScript sq in squaresToVoid)
        {
            sq.TurnVoid(true);

            if (sq.knight != null && !sq.knight.isDead)
            {
                StartCoroutine(sq.knight.KillKnight(sq));
            }
        }

        timesExpanded++;
    }

    internal void StartStorm()
    {
        if (!stormEnabled)
            return;
        Debug.Log("Storm started!");
        stormStarted = true;

        ExpandStorm();
    }
}
