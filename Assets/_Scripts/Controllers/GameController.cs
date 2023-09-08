using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController gc;
    public bool allowBuzzer;
    public int sortingTime;
    public int exposeCardID;
    public int buzzerCallerID;
    public int playerTurnNumber;
    public int firstPlayerNumber;
    public int previousTurnNumber;
    public Coroutine turnRoutine;
    public Coroutine sortingRoutine;
    public Transform centerPlace;
    public Transform dealtCardPlace;
    public List<Card> cards;
    public List<Transform> cardPlaces;
    public List<CardController> players;
    private CardManager Cm => CardManager.cm;
    private GameplayUI Gui => GameplayUI.gUI;

    private void Awake()
    {
        gc = this;
        for (int i = 0; i < 138; i++) { cards[i].cardID = i; }
    }

    public void CheckIfAllPlayersReady()
    {
        foreach (CardController cc in players)
        {
            if (!cc.ready)
            {
                return;
            }
        }
        Cm.AllPlayersReady();
        ResetReadyText();
    }

    private void ResetReadyText()
    {
        foreach (CardController cc in players)
        {
            cc.ResetReadyText();
        }
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

/*
 * TODO
 * 
 * shuffle cards ..
 * assign cards ..
 * 13th assign ..
 * give time for soting ..
 * run 60 sec routine ..
 * randomly set ready by each player ..
 * think about card discard by the player with 13th card .
 * if all player ready start game..
 * if the human player not set ready make him ready after 60 sec ..
 * 
 * after game start
 * 
 * set turn time for player with 13th card to 20 seconds and he will discard a card
 * if he not after 20 second a random cad will be discarded
 * then the turn is of next player and if someone else press the buzzer he will lose his turn
 * and the next player will call the buzzer
 * 
 * 
 */
