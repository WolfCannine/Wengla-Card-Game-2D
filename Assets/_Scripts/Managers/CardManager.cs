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
        //_ = StartCoroutine(InstantiateAllCards());
        for (int i = 0; i < totalCards; i++)
        {
            cards[i].cardID = i;
        }
    }

    private IEnumerator InstantiateAllCards()
    {
        int i = 0;
        while (i < totalCards)
        {
            
        }
        yield return new WaitForSeconds(0.1f);
    }

    public void ShuffleCards()
    {
        Shuffle(playerCardsIDs);
        GameplayUI.gUI.ActivateDealCardButton();
    }

    public void DealCards()
    {
        GameplayUI.gUI.dealButtonGO.SetActive(false);
        _ = StartCoroutine(AssignCardsToHand());
    }

    public void AssignBeginnerCard() // the 13th card
    {
        int randomPlayer = Random.Range(0, 10);
        GameController.gc.players[randomPlayer].AssignBeginnerCard(playerCardsIDs[ID]);
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
                //yield return new WaitForSeconds(0.2f);
            }
            GameController.gc.players[currentPlayer].AssignCardID_List(cradID_List);
            currentPlayer++;
            //yield return new WaitForSeconds(0.2f);
        }
        AssignBeginnerCard();
        yield return null;
    }

    private void Shuffle<T>(List<T> list)
    {
        System.Random rand = new();
        int n = list.Count;
        while (n > 1)
        {
            int k = rand.Next(n);
            n--;
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}
