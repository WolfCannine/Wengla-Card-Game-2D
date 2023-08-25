using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController gc;
    public bool allowBuzzer;
    public int exposeCardID;
    public int buzzerCallerID;
    public int playerTurnNumber;
    public int firstPlayerNumber;
    public int previousTurnNumber;
    public Coroutine turnRoutine;
    public Transform centerPlace;
    public Transform dealtCardPlace;
    public List<Card> cards;
    public List<Transform> cardPlaces;
    public List<CardController> players;

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
        if (playerTurnNumber == ((firstPlayerNumber - 1) % players.Count))
        {
            ResetBuzzerOption();
            return;
        }
        playerTurnNumber = (current + 1) % players.Count;
        if (!players[playerTurnNumber].haveBuzzerOption)
        {
            SetPlayerTurn(playerTurnNumber);
        }
    }

    public void ResetBuzzerOption()
    {
        playerTurnNumber = firstPlayerNumber;
        foreach (CardController pc in players)
        {
            pc.haveBuzzerOption = true;
            pc.haveFaceDownPileOption = false;
            GameplayUI.gUI.buzzerCountText.text = "1";
        }
    }

    public void ExposeCardOnTable(int cardID, bool pickable = false)
    {
        cards[cardID].pickable = pickable;
        cards[cardID].gameObject.SetActive(true);
        cards[cardID].transform.SetPositionAndRotation(centerPlace.position, centerPlace.rotation);
        cards[cardID].transform.localScale = centerPlace.localScale;
    }

    public void SetTurnText()
    {
        players[playerTurnNumber].SetTurnText();
    }

    public void ResetTurnText(int playerNumber)
    {
        players[playerNumber].ResetTurnText();
    }

    public void SetCardPickable(int cardId, bool pickable = false)
    {
        cards[cardId].pickable = pickable;
    }
}
