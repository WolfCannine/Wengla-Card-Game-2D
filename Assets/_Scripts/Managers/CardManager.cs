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
    private int randomPlayer;

    private void Awake()
    {
        cm = this;
    }

    private void Start()
    {
        Invoke(nameof(ShuffleCards), 0f);
    }

    public void ShuffleCards()
    {
        GameplayUI.gUI.CallNotification("Dealer is Shufling Cards!", resetText: false);
        Helper.Shuffle(playerCardsIDs);
        Invoke(nameof(DealCards), 0f);
    }

    public void DealCards()
    {
        GameplayUI.gUI.CallNotification("Dealer is Assigning Cards!", resetText: false);
        _ = StartCoroutine(AssignCardsToHand());
    }

    public void AssignBeginnerCard()
    {
        GameController.gc.firstPlayerNumber = randomPlayer = Random.Range(0, 10);
        GameController.gc.ResetBuzzerOption();
        GameController.gc.SetPlayerTurn(randomPlayer);
        CardController randomPlayerController = GameController.gc.players[randomPlayer];
        randomPlayerController.AssignBeginnerCard(playerCardsIDs[ID]);
        GameplayUI.gUI.CallNotification("Beginner Card is Assign to Player: " + randomPlayerController.playerName, resetText: false);
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
            GameController.gc.players[currentPlayer].AssignCardID_List(cradID_List);
            currentPlayer++;
        }
        yield return new WaitForSeconds(0f);
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
        GameplayUI.gUI.CallNotification("You have " + time + " seconds to sort your cards", resetText: false);
        while (time > 0)
        {
            yield return new WaitForSeconds(1);
            time--;
            GameplayUI.gUI.CallNotification("You have " + time + " seconds to sort your cards", resetText: false);
        }
        string playerName = GameController.gc.players[randomPlayer].playerName;
        GameplayUI.gUI.CallNotification(playerName + " Please discard a card.\r\nOtherwise a random card will be discarded!", resetText: false);
        GameController.gc.players[randomPlayer].Discard_a_Card();
    }
}
