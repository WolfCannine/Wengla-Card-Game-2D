using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    #region Fields
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
    private GameController Gc => GameController.gc;
    private GameplayUI Gui => GameplayUI.gUI;
    #endregion

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
        Gui.CallNotification("Dealer is Shufling Cards!", resetText: false);
        Helper.Shuffle(playerCardsIDs);
        Invoke(nameof(DealCards), 0f);
    }

    public void DealCards()
    {
        Gui.CallNotification("Dealer is Assigning Cards!", resetText: false);
        _ = StartCoroutine(AssignCardsToHand());
    }

    public void AssignBeginnerCard()
    {
        Gc.firstPlayerNumber = randomPlayer = Random.Range(0, 10);
        Gc.ResetBuzzerOption();
        Gc.SetPlayerTurn(randomPlayer);
        CardController randomPlayerController = Gc.players[randomPlayer];
        randomPlayerController.AssignBeginnerCard(playerCardsIDs[ID]);
        ID++;
        Gc.sortingRoutine = StartCoroutine(SortingRoutine(Gc.sortingTime));
        SetFaceDownPile();
        PromptForReady();
    }

    public void AllPlayersReady()
    {
        if (Gc.sortingRoutine != null) { StopCoroutine(Gc.sortingRoutine); }
        Gc.sortingRoutine = null;
        string playerName = Gc.players[randomPlayer].playerName;
        Gui.CallNotification(playerName + " Please discard a card.\r\nOtherwise a random card will be discarded!", resetText: false);
        Gc.players[randomPlayer].Discard_a_Card();
    }

    private void PromptForReady()
    {
        foreach (CardController cc in Gc.players)
        {
            cc.StartReadyRoutine();
        }
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
            GameObject cardGO = Gc.cards[i].gameObject;
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

    private IEnumerator AssignCardsToHand()
    {
        int currentPlayer = 0;
        numberOfPlayers = Gc.players.Count;
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
            Gc.players[currentPlayer].AssignCardID_List(cradID_List);
            currentPlayer++;
        }
        yield return new WaitForSeconds(0f);
        AssignBeginnerCard();
    }

    private IEnumerator SortingRoutine(int time)// Time for sorting cards
    {
        string playerName = Gc.players[randomPlayer].playerName;
        Gui.CallNotification("Beginner Card is Assign to Player: " + playerName, resetText: false);
        yield return new WaitForSeconds(5);
        Gui.CallNotification("You have " + time + " seconds to sort your cards", resetText: false);
        while (time > 0)
        {
            yield return new WaitForSeconds(1);
            time--;
            Gui.CallNotification("You have " + time + " seconds to sort your cards", resetText: false, msg: false);
        }
        Gui.AreYouReadyButton();
    }
}
