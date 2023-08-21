using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController gc;
    public bool allowBuzzer;
    public int exposeCardID;
    public int buzzerCallerID;
    public int playerTurnNumber;
    public Transform centerPlace;
    public Transform dealtCardPlace;
    public List<Card> cards;
    public List<Transform> cardPlaces;
    public List<PlayerController> players;

    private void Awake()
    {
        gc = this;
        for (int i = 0; i < 138; i++) { cards[i].cardID = i; }
    }

    public void ExposeCardID(int cardID = -1)
    {
        exposeCardID = cardID;
    }

    public void BuzzerCall(int caller = -1)
    {
        buzzerCallerID = caller;
    }

    public void SetPlayerTurn(int current = -1)
    {
        playerTurnNumber = (current + 1) % players.Count;
        if (!players[playerTurnNumber].haveBuzzerOption)
        {
            SetPlayerTurn(playerTurnNumber);
        }
        if (playerTurnNumber == 0) { ResetBuzzerOption(); }
    }

    public void ResetBuzzerOption()
    {
        playerTurnNumber = 0;
        foreach (PlayerController pc in players)
        {
            pc.haveBuzzerOption = true;
            pc.haveFaceDownPileOption = false;
        }
    }
}
