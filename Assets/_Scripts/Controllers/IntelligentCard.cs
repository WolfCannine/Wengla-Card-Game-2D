using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/*
 * Twins amount: 2, number: same, suit: same            2 cards with same number and suit
 * Triplets amount: 3, number: same, suit: same         3 cards with same number and suit
 * quadruplets amount: 4, number: same, suit: same      4 cards with same number and suit
 * street amount: 8, number: 1 to 8 All, suit: same     8 cards from 1 to 8 with same suit
 * wengla amount: 12, number: same, suit: different     12 cards of same number
 * 
 * take linear approch
 * 
 * 
 */
public class IntelligentCard : MonoBehaviour
{
    private CardController cc;
    private Coroutine checkCombinationRoutine;
    private GameController Gc => GameController.gc;

    public bool haveStreet;
    public bool haveWengla;
    public int WenglaNumber;
    public CardSuit streetSuit;
    public List<Twin> foundTwins = new();
    public List<Triplet> foundTriplets = new();
    public List<Quadruplet> foundQuadruplets = new();
    public Street foundStreet;
    public Wengla foundWengla;
    public Dictionary<CardSuit, List<int>> suitCardIDs = new();

    private bool findCard;
    private int cardToExpose = -1;

    private void Awake()
    {
        cc = GetComponent<CardController>();
    }

    /*
     * Start Thinking of AI
     * 
     * Check if beginner card is important
     * 
     * if not then discard the beginner card
     * 
     * otherwise check the other card which one is not forming
     * any kind of combination then discard that card
     * 
     * 
     * 
     * 
     */

    public void FindCardToDiscard()
    {
        findCard = true;
        CheckAllCombinations();
    }

    public void CheckImportanceOfCard()
    {
        _ = StartCoroutine(FindCardToDiscardRoutine());
    }

    public void CheckAllCombinations()
    {
        if (checkCombinationRoutine != null)
        {
            StopCoroutine(checkCombinationRoutine);
        }
        checkCombinationRoutine = StartCoroutine(CheckCardsCombinations());
    }

    public void ClearSuitCardIDs()
    {
        suitCardIDs.Clear();
        foundTwins.Clear();
        foundTriplets.Clear();
        foundQuadruplets.Clear();
        foundStreet = null;
        foundWengla = null;
    }

    public void SetSuitList()
    {
        ClearSuitCardIDs();
        foreach (int ID in cc.cardID_List)
        {
            Card card = Gc.cards[ID];
            if (suitCardIDs.ContainsKey(card.cardSuit))
            {
                suitCardIDs[card.cardSuit].Add(ID);
            }
            else
            {
                suitCardIDs.Add(card.cardSuit, new List<int> { ID });
            }
        }
    }

    public bool IsImportant(out int cardID)
    {
        int require = Gc.cards[Gc.exposeCardID].cardNumber;
        bool quadrupletExist = foundQuadruplets.Any(quad => quad.CardNumber == require);
        bool tripletExist = foundTriplets.Any(triple => triple.CardNumber == require);
        bool twinExist = foundTwins.Any(twin => twin.CardNumber == require);
        cardID = cardToExpose;
        return (twinExist || tripletExist || quadrupletExist) && cardID != -1;
    }

    //2 cards with same number and suit
    public bool HaveTwins(out int cardNumber, CardSuit targetCardSuit)
    {
        if (!suitCardIDs.ContainsKey(targetCardSuit) || suitCardIDs[targetCardSuit].Count < 2)
        {
            cardNumber = -1;
            return false;
        }

        int[] cardIDs = new int[2];
        HashSet<int> seenNumbers = new();

        foreach (int ID in suitCardIDs[targetCardSuit])
        {
            if (seenNumbers.Contains(Gc.cards[ID].cardNumber))
            {
                cardIDs[1] = ID;
                cardNumber = Gc.cards[ID].cardNumber;
                return true;
            }
            cardIDs[0] = ID;
            _ = seenNumbers.Add(Gc.cards[ID].cardNumber);
        }
        cardNumber = -1;
        return false;
    }

    //3 cards with same number and suit
    public bool HaveTriplets(out int cardNumber, CardSuit targetCardSuit)
    {
        if (!suitCardIDs.ContainsKey(targetCardSuit) || suitCardIDs[targetCardSuit].Count < 3)
        {
            cardNumber = -1;
            return false;
        }

        int[] cardIDs = new int[3];
        Dictionary<int, int> cardCounts = new();

        foreach (int ID in suitCardIDs[targetCardSuit])
        {
            int cardNum = Gc.cards[ID].cardNumber;

            if (cardCounts.ContainsKey(cardNum))
            {
                cardCounts[cardNum]++;
                if (cardCounts[cardNum] >= 3)
                {
                    cardIDs[2] = ID;
                    cardNumber = cardNum;
                    return true;
                }
                cardIDs[1] = ID;
            }
            else
            {
                cardIDs[0] = ID;
                cardCounts.Add(cardNum, 1);
            }
        }
        cardNumber = -1;
        return false;
    }

    //4 cards with same number and suit
    public bool HaveQuadruplets(out int cardNumber, CardSuit targetCardSuit)
    {
        if (!suitCardIDs.ContainsKey(targetCardSuit) || suitCardIDs[targetCardSuit].Count < 4)
        {
            cardNumber = -1;
            return false;
        }

        Dictionary<int, int> cardCounts = new();

        foreach (int ID in suitCardIDs[targetCardSuit])
        {
            int cardNum = Gc.cards[ID].cardNumber;

            if (cardCounts.ContainsKey(cardNum))
            {
                cardCounts[cardNum]++;
                if (cardCounts[cardNum] >= 4)
                {
                    cardNumber = cardNum;
                    return true;
                }
            }
            else
            {
                cardCounts.Add(cardNum, 1);
            }
        }

        cardNumber = -1;
        return false;
    }

    //8 cards from 1 to 8 with same suit
    public bool HaveStreet(CardSuit targetCardSuit)
    {
        if (!suitCardIDs.ContainsKey(targetCardSuit) || suitCardIDs[targetCardSuit].Count < 8)
        {
            return false;
        }

        bool[] cardNumbers = new bool[9];

        foreach (int ID in suitCardIDs[targetCardSuit])
        {
            int cardNum = Gc.cards[ID].cardNumber;

            if (cardNum >= 1 && cardNum <= 8)
            {
                cardNumbers[cardNum] = true;
            }
        }

        for (int i = 1; i <= 8; i++)
        {
            if (!cardNumbers[i])
            {
                return false;
            }
        }

        return true;
    }

    //12 cards of same number
    public bool HaveWengla(out int cardNumber)
    {
        int firstCardNum = SetFirstCardNumber(out int count);

        for (int i = count; i < 12; i++)
        {
            if (firstCardNum != Gc.cards[cc.cardID_List[i]].cardNumber)
            {
                cardNumber = -1;
                return false;
            }
        }

        cardNumber = firstCardNum;
        return true;
    }

    private int SetFirstCardNumber(out int count) // for HaveWengla func
    {
        int j = 0;
        foreach (int i in cc.cardID_List)
        {
            if (Gc.cards[i].cardNumber != 0)
            {
                count = j;
                return Gc.cards[i].cardNumber;
            }
            j++;
        }
        count = -1;
        return 0;
    }

    private IEnumerator CheckCardsCombinations()
    {
        SetSuitList();

        yield return new WaitForSeconds(0.3f);// For Wengla!!!!!!!!!!!!
        haveWengla = HaveWengla(out WenglaNumber);
        foundWengla = new Wengla { CardNumber = WenglaNumber };
        if (haveWengla)
        {
            Debug.Log(cc.playerName + " won!");
            yield break;
        }

        yield return new WaitForSeconds(0.3f);

        List<CardSuit> suitsToCheck = new() { CardSuit.Pentagone, CardSuit.Squre, CardSuit.Triangle, CardSuit.Circle };

        foreach (CardSuit suit in suitsToCheck)
        {
            yield return new WaitForSeconds(0.1f);// For Street!!!!!!!!!!!!

            if (HaveStreet(suit))
            {
                haveStreet = true;
                streetSuit = suit;
                foundStreet = new Street { CardSuit = suit };
            }

            yield return new WaitForSeconds(0.3f);// For Quadruplets!!!!!!!!!!!!

            if (HaveQuadruplets(out int number, suit))
            {
                foundQuadruplets.Add(new Quadruplet { CardNumber = number, CardSuit = suit });
            }

            yield return new WaitForSeconds(0.3f);// For Triplets!!!!!!!!!!!!

            if (HaveTriplets(out number, suit))
            {
                foundTriplets.Add(new Triplet { CardNumber = number, CardSuit = suit });
            }

            yield return new WaitForSeconds(0.3f);// For Twins!!!!!!!!!!!!

            if (HaveTwins(out number, suit))
            {
                foundTwins.Add(new Twin { CardNumber = number, CardSuit = suit });
            }

            yield return new WaitForSeconds(0.3f);
        }
        if (findCard)
        {
            FindCardToDiscard();
        }
    }

    private IEnumerator FindCardToDiscardRoutine()
    {
        int cardId = -1;
        foreach (int id in cc.cardID_List)
        {
            if (haveStreet)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }
            else if (CheckQuadruplet(id))
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }
            else if (CheckTriplet(id))
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }
            else if (CheckTwin(id))
            {
                cardId = id;
                yield return new WaitForSeconds(0.2f);
                continue;
            }
            else
            {
                cardToExpose = id;
                cc.exposeCardID = id;
                yield break;
            }
        }
        cardToExpose = cardId;
        cc.exposeCardID = cardId;
        yield return new WaitForSeconds(1);
    }

    private bool CheckQuadruplet(int cardToCheck = -1)
    {
        if (foundQuadruplets.Count == 0)
        {
            return false;
        }
        foreach (Quadruplet qc in foundQuadruplets)
        {
            if (qc.CardSuit == Gc.cards[cardToCheck].cardSuit && qc.CardNumber == Gc.cards[cardToCheck].cardNumber)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckTriplet(int cardToCheck = -1)
    {
        if (foundQuadruplets.Count == 0)
        {
            return false;
        }
        foreach (Triplet qc in foundTriplets)
        {
            if (qc.CardSuit == Gc.cards[cardToCheck].cardSuit && qc.CardNumber == Gc.cards[cardToCheck].cardNumber)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckTwin(int cardToCheck = -1)
    {
        if (foundQuadruplets.Count == 0)
        {
            return false;
        }
        foreach (Twin qc in foundTwins)
        {
            if (qc.CardSuit == Gc.cards[cardToCheck].cardSuit && qc.CardNumber == Gc.cards[cardToCheck].cardNumber)
            {
                return true;
            }
        }
        return false;
    }
}
