using System.Collections.Generic;
using System.Linq;
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
    public int wenglaCollectors;
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
        if (!players.All(p => p.ready)) { return; }
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

    public void SetBuzzerCallerID(int caller = -1, bool allowBuzzer = false)
    {
        buzzerCallerID = caller;
        this.allowBuzzer = allowBuzzer;
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
        foreach (CardController cc in players)
        {
            cc.haveBuzzerOption = true;
            GameplayUI.gUI.ResetPlayerBuzzerCountText();
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
 * give time for sorting ..
 * run 60 sec routine ..
 * randomly set ready by each player ..
 * think about card discard by the player with 13th card .
 * if all player ready start game..
 * if the human player not set ready make him ready after 60 sec ..
 * 
 * after game start
 * 
 * set turn time for player with 13th card to 20 seconds and he will discard a card
 * if he not after 20 second a random card will be discarded
 * then the turn is of next player and if someone else press the buzzer he will lose his turn
 * and the next player will call the buzzer
 * 
 * 
 */
//r d h j m h j, n m i A
////