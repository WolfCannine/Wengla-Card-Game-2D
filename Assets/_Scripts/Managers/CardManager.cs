//Implement a CardManager script to manage the deck, shuffling, dealing, and drawing cards.
// Dealer
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager cm;
    public int colorCount = 4;
    public int cornerType = 4;
    public int numberType = 8;
    public int jokerCount = 10;
    public int totalCards = 138;
    public int numberOfPlayers = 10;
    public int cardsPerPlayer = 12;
    public GameObject cardsParentGO;
    public GameObject faceDownPilePlaceGO;
    public List<int> faceDownPile;
    [SerializeField]
    private List<int> playerCardsIDs = new();
    [SerializeField]
    private List<int> discardPile;
    private int ID = 0;
    private GameplayUI gUI;
    private GameController gc;
    private int randomPlayer;

    private void Awake()
    {
        cm = this;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(3);
        gUI = GameplayUI.gUI;
        gc = GameController.gc;
        Invoke(nameof(ShuffleCards), 0f);
    }

    public void ShuffleCards()
    {
        gUI.CallNotification("Dealer is Shufling Cards!");
        Helper.Shuffle(playerCardsIDs);
        Invoke(nameof(DealCards), 0f);
        //GameplayUI.gUI.ActivateDealCardButton();
    }

    public void DealCards()
    {
        gUI.CallNotification("Dealer is Assigning Cards!");
        //GameplayUI.gUI.dealButtonGO.SetActive(false);
        _ = StartCoroutine(AssignCardsToHand());
    }

    public void AssignBeginnerCard()
    {
        gc.firstPlayerNumber = randomPlayer = Random.Range(0, 10);
        gc.ResetBuzzerOption();
        gc.SetPlayerTurn(randomPlayer);
        PlayerController randomPlayerController = gc.players[randomPlayer];
        randomPlayerController.AssignBeginnerCard(playerCardsIDs[ID]);
        gUI.CallNotification("Beginner Card is Assign to Player: " + randomPlayerController.playerName, 5);
        Invoke(nameof(NotifyForSorting), 5);
        ID++;
        SetFaceDownPile();
    }

    private IEnumerator AssignCardsToHand()
    {
        int currentPlayer = 0;
        while (currentPlayer < numberOfPlayers)
        {
            int currentCard = 0;
            List<int> cradID_List = new();
            while (currentCard < cardsPerPlayer)
            {
                cradID_List.Add(playerCardsIDs[ID]);
                currentCard++;
                ID++;
            }
            gc.players[currentPlayer].AssignCardID_List(cradID_List);
            currentPlayer++;
        }
        yield return new WaitForSeconds(3f);
        AssignBeginnerCard();
    }

    private void SetFaceDownPile()
    {
        while (ID < 138)
        {
            faceDownPile.Add(playerCardsIDs[ID]);
            ID++;
        }
        ActivateFaceDownPile();
    }

    private void ActivateFaceDownPile()
    {
        foreach (int i in faceDownPile)
        {
            GameObject cardGO = GameController.gc.cards[i].gameObject;
            cardGO.transform.position = faceDownPilePlaceGO.transform.position;
            cardGO.transform.localScale = faceDownPilePlaceGO.transform.localScale;
            cardGO.SetActive(true);
        }
    }

    private void AddToDiscardPile(int cardID)
    {
        discardPile.Add(cardID);
        if (faceDownPile.Contains(cardID)) { _ = faceDownPile.Remove(cardID); }
    }

    private void NotifyForSorting()
    {
        _ = StartCoroutine(TimerRoutine(5));
    }

    private IEnumerator TimerRoutine(int time)
    {
        gUI.CallNotification("You have " + time + " seconds to sort your cards", resetText: false);
        while (time > 0)
        {
            yield return new WaitForSeconds(1);
            time--;
            gUI.CallNotification("You have " + time + " seconds to sort your cards", resetText: false);
        }
        string playerName = GameController.gc.players[randomPlayer].playerName;
        gUI.CallNotification(playerName + " Please discard a card.\r\nOtherwise a random card will be discarded!", resetText: false);
        gc.players[randomPlayer].Discard_a_Card();
    }
}
