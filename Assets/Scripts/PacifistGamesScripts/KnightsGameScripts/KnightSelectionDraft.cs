using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine.UI;
using TMPro;

public class KnightSelectionDraft : MonoBehaviour
{
    bool banPhase = true;
    public List<int> player1DraftKnights, player2DraftKnights;
    public List<int> bannedKnights;
    public Dictionary<int, bool> playerHasBanned;
    public Canvas nextCanvas;

    public List<TextMeshProUGUI> banText, picksPlayer1, picksPlayer2;

    public void StartRound()
    {
        bannedKnights.Clear();
        playerHasBanned.Add(1, false);
        playerHasBanned.Add(2, false);
    }
    public void OnEnable()
    {
        bannedKnights = new List<int>();
        playerHasBanned = new Dictionary<int, bool>();
        StartRound();
    }

    public string KnightNameByIndex(int knightIndex)
    {
        switch (knightIndex)
        {
            case 0: return "Agile";
            case 1: return "Tucutu";
            case 2: return "Shaky";
            case 3: return "Bull";
            case 4: return "Shift";
            case 5: return "Jumpy";
            case 6: return "Ghost";
            default: return "Unknown Knight";
        }
    }

    public void BanKnight(int knight)
    {
        if (playerHasBanned[1] == false)
        {
            bannedKnights.Add(knight);
            playerHasBanned[1] = true;
            banText[0].text = KnightNameByIndex(knight);
        }
        else if (playerHasBanned[2] == false)
        {
            bannedKnights.Add(knight);
            playerHasBanned[2] = true;
            banPhase = false;
            banText[1].text = KnightNameByIndex(knight);
        }
    }
    public void AddKnightToDraft(int knightIndex)
    {
        if (!bannedKnights.Contains(knightIndex))
        {
            if (player1DraftKnights.Count < 3)
            {
                player1DraftKnights.Add(knightIndex);
                picksPlayer1[player1DraftKnights.Count - 1].text = KnightNameByIndex(knightIndex);
            }
            
            if (player2DraftKnights.Count < 3)
            {
                player2DraftKnights.Add(knightIndex);
                picksPlayer2[player2DraftKnights.Count - 1].text = KnightNameByIndex(knightIndex);
            }
        }

        if (player1DraftKnights.Count == 3 && player2DraftKnights.Count == 3)
        {
            StartCoroutine(TransitionToNextCanvas());
        }
    }

    IEnumerator TransitionToNextCanvas()
    {
        yield return new WaitForSeconds(1f);
        nextCanvas.gameObject.SetActive(true);
        gameObject.SetActive(false);
        KnightsGameManager.instance.knightValues = player1DraftKnights;
    }
    public void KnightAction(int knightIndex)
    {
        if (bannedKnights.Contains(knightIndex))
        {
            return;
        }

        if (banPhase)
        {
            BanKnight(knightIndex);
        }
        else
        {
            AddKnightToDraft(knightIndex);
        }
    }
}
