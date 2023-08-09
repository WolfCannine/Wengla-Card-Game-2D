//Implement a CardManager script to manage the deck, shuffling, dealing, and drawing cards.
// Dealer
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager cm;

    public GameObject cardsParentGO;

    public int colorCount = 4;
    public int cornerType = 4;
    public int numberType = 8;
    public int jokerCount = 10;
    public int totalCards = 138;
    public int numberOfPlayers = 10;
    public int cardsPerPlayer = 12;
    public Card[] cards;
    public List<Card> deck;
    public List<List<Card>> playerHands = new();
    [SerializeField]
    private List<int> playerCardsIDs = new();
    private int ID = 0;


    private void Awake()
    {
        cm = this;
        for (int i = 0; i < totalCards; i++)
        {
            cards[i].cardID = i;
        }
    }

    private void Start()
    {
        Invoke(nameof(ShuffleCards), 0f);
    }

    public void ShuffleCards()
    {
        GameplayUI.gUI.CallNotification("Dealer is Shufling Cards!");
        Helper.Shuffle(playerCardsIDs);
        Invoke(nameof(DealCards), 0f);
        //GameplayUI.gUI.ActivateDealCardButton();
    }

    public void DealCards()
    {
        GameplayUI.gUI.CallNotification("Dealer is Assigning Cards!");
        //GameplayUI.gUI.dealButtonGO.SetActive(false);
        _ = StartCoroutine(AssignCardsToHand());
    }

    public void AssignBeginnerCard()
    {
        int randomPlayer = Random.Range(0, 10);
        GameController.gc.players[randomPlayer].AssignBeginnerCard(playerCardsIDs[ID]);
        GameplayUI.gUI.CallNotification("Beginner Card is Assign to Player: " + randomPlayer);
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
}
