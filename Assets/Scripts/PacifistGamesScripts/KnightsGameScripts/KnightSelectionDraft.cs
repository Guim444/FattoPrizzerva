using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine.UI;

public class KnightSelectionDraft : MonoBehaviour
{
    public List<Image> draftKnights;
    public List<KnightBehavior> bannedKnights;
    public Dictionary<int, bool> playerHasBanned;

    public void StartRound()
    {
        bannedKnights.Clear();
        playerHasBanned.Add(1, false);
        playerHasBanned.Add(2, false);
    }
    public void BanKnight(KnightBehavior knight)
    {
        if (playerHasBanned[1] == false)
        {
            bannedKnights.Add(knight);
            playerHasBanned[1] = true;
        }
        else if (playerHasBanned[2] == false)
        {
            bannedKnights.Add(knight);
            playerHasBanned[2] = true;
        }
    }
}
