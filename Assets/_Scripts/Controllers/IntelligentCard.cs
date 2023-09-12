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

    public bool haveCardToExpose;

    public bool haveStreet;
    public bool haveWengla;
    public int WenglaNumber;
    public int cardNumberWithMaxCount;
    public CardSuit streetSuit;
    public List<Twin> foundTwins = new();
    public List<Triplet> foundTriplets = new();
    public List<Quadruplet> foundQuadruplets = new();
    public Street foundStreet;
    public Wengla foundWengla;
    public Dictionary<CardSuit, List<int>> suitCardIDs = new();

    public Queue<int> lessImportantCards = new();
    public List<int> lessImportantCards1 = new();
    public List<int> cardIDsWithMaxCounts = new();
    //private int cardToExpose = -1;

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
        CheckAllCombinations();
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
        haveStreet = false;
        haveWengla = false;
        haveCardToExpose = false;
        foundStreet = null;
        foundWengla = null;
        suitCardIDs.Clear();
        foundTwins.Clear();
        foundTriplets.Clear();
        foundQuadruplets.Clear();
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

    public bool IsCardImp()
    {
        if (Gc.exposeCardID < 10) { return true; }

        int lessImpCardID = GetLessImpCardID;
        int xCardNumber = Gc.cards[Gc.exposeCardID].cardNumber;
        CardSuit xCardSuit = Gc.cards[Gc.exposeCardID].cardSuit;

        return lessImpCardID != -1 && CheckAllForImp(xCardNumber, xCardSuit);
    }

    public bool CheckAllForImp(int xCardNumber, CardSuit xCardSuit)
    {
        return QuadExist(xCardNumber, xCardSuit) || TriExist(xCardNumber, xCardSuit) || DuoExist(xCardNumber, xCardSuit)
            || SameExist(xCardNumber, xCardSuit) || SameNumberExist(xCardNumber) || SameSuitExist(xCardSuit);
    }

    public bool IsImportant(out int cardID)
    {
        int lessImpCardID = GetLessImpCardID;
        if (Gc.exposeCardID < 10)
        {
            cardID = lessImpCardID;
            return true;
        }

        int xCardNumber = Gc.cards[Gc.exposeCardID].cardNumber;
        CardSuit xCardSuit = Gc.cards[Gc.exposeCardID].cardSuit;

        if (lessImpCardID != -1 && (QuadExist(xCardNumber, xCardSuit) || TriExist(xCardNumber, xCardSuit) ||
            DuoExist(xCardNumber, xCardSuit) || SameExist(xCardNumber, xCardSuit) || SameNumberExist(xCardNumber)
            || SameSuitExist(xCardSuit)))
        {
            cardID = lessImpCardID;
            return true;
        }

        cardID = -1;
        return false;
    }

    public int GetCardIDToExpose()
    {
        int lessImpCardID = GetLessImpCardID;

        if (lessImpCardID != -1)
        {
            return lessImpCardID;
        }
        else if (foundTwins.Count > 0)
        {
            return foundTwins[0].cardIDs[0];
        }
        else if (foundTriplets.Count > 0)
        {
            return foundTriplets[0].cardIDs[0];
        }
        else if (foundQuadruplets.Count > 0)
        {
            return foundQuadruplets[0].cardIDs[0];
        }
        //else
        //{
        //    return cc.cardID_List.LastOrDefault();
        //}
        return lessImpCardID != -1 ? lessImpCardID : foundTwins.Count > 0 ? foundTwins[0].cardIDs[0] : foundTriplets.Count > 0 ?
            foundTriplets[0].cardIDs[0] : foundQuadruplets.Count > 0 ? foundQuadruplets[0].cardIDs[0] : foundQuadruplets[0].cardIDs[0];
    }

    public int GetLessImpCardID => lessImportantCards.Count != 0 ? lessImportantCards.Dequeue() : -1;

    public bool QuadExist(int xCardNumber, CardSuit xCardSuit)
    {
        return foundQuadruplets.Any(quad => quad.CardNumber == xCardNumber && quad.CardSuit == xCardSuit);
    }

    public bool TriExist(int xCardNumber, CardSuit xCardSuit)
    {
        return foundTriplets.Any(triple => triple.CardNumber == xCardNumber && triple.CardSuit == xCardSuit);
    }

    public bool DuoExist(int xCardNumber, CardSuit xCardSuit)
    {
        return foundTwins.Any(twin => twin.CardNumber == xCardNumber && twin.CardSuit == xCardSuit);
    }

    public bool SameExist(int xCardNumber, CardSuit xCardSuit)
    {
        return cc.cardID_List.Any(cardID => Gc.cards[cardID].cardNumber == xCardNumber && Gc.cards[cardID].cardSuit == xCardSuit);
    }

    public bool SameNumberExist(int xCardNumber)
    {
        return cc.cardID_List.Any(cardID => Gc.cards[cardID].cardNumber == xCardNumber);
    }

    public bool SameSuitExist(CardSuit xCardSuit)
    {
        return cc.cardID_List.Any(cardID => Gc.cards[cardID].cardSuit == xCardSuit);
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
                continue;
            }
            cardIDs[0] = ID;
            cardCounts.Add(cardNum, 1);
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
                continue;
            }
            cardCounts.Add(cardNum, 1);
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
            cardNumbers[cardNum] = cardNum >= 1 && cardNum <= 8;
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

    // near street combination
    public bool HaveNearStreet(CardSuit targetCardSuit)
    {
        int count = suitCardIDs[targetCardSuit].Count;
        if (!suitCardIDs.ContainsKey(targetCardSuit) || count < 4)
        {
            return false;
        }

        bool[] cardNumbers = new bool[count + 1];

        foreach (int ID in suitCardIDs[targetCardSuit])
        {
            int cardNum = Gc.cards[ID].cardNumber;
            cardNumbers[cardNum] = cardNum >= 1 && cardNum <= count;
        }

        for (int i = 1; i <= count; i++)
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

    // Near wengla combination
    public bool HaveNearWengla(out int cardNumber)
    {
        Dictionary<int, int> cardCounts = new();

        foreach (int ID in cc.cardID_List)
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
                continue;
            }
            cardCounts.Add(cardNum, 1);
        }

        cardNumber = -1;
        return true;
    }

    public int FindCardNumberWithMaxCount() // for wengla formation
    {
        cardIDsWithMaxCounts.Clear();
        Dictionary<int, List<int>> cardCounts = new();

        foreach (int id in cc.cardID_List)
        {
            int cardNum = Gc.cards[id].cardNumber;
            if (cardNum == 0) { continue; }
            if (cardCounts.Any(x => Gc.cards[x.Key].cardNumber == cardNum))
            {
                cardCounts[cardNum].Add(id);
                continue;
            }
            cardCounts.Add(cardNum, new List<int> { id });
        }

        cardNumberWithMaxCount = cardCounts.OrderByDescending(pair => pair.Value.Count).First().Key;
        _ = cardCounts.TryGetValue(cardNumberWithMaxCount, out cardIDsWithMaxCounts);

        return cardNumberWithMaxCount;
    }

    private IEnumerator CheckCardsCombinations()
    {
        float wait = 0.3f;
        SetSuitList();

        yield return new WaitForSeconds(wait);// For Wengla!!!!!!!!!!!!
        haveWengla = HaveWengla(out WenglaNumber);
        foundWengla = new Wengla { CardNumber = WenglaNumber };
        if (haveWengla)
        {
            Debug.Log(cc.playerName + " won!");
            yield break;
        }

        yield return new WaitForSeconds(wait);

        List<CardSuit> suitsToCheck = new() { CardSuit.Pentagone, CardSuit.Squre, CardSuit.Triangle, CardSuit.Circle };

        foreach (CardSuit suit in suitsToCheck)
        {
            yield return new WaitForSeconds(wait);

            if (HaveStreet(suit)) // For Street!!!!!!!!!!!!
            {
                haveStreet = true;
                streetSuit = suit;
                foundStreet = new Street { CardSuit = suit };
            }

            yield return new WaitForSeconds(wait);

            if (HaveQuadruplets(out int number, suit)) // For Quadruplets!!!!!!!!!!!!
            {
                //foundQuadruplets.Add(new Quadruplet { CardNumber = number, CardSuit = suit });
                AddCardIDsInCombination(suit, number, 2);
                yield return new WaitForSeconds(wait);
            }
            else if (HaveTriplets(out number, suit)) // For Triplets!!!!!!!!!!!!
            {
                //foundTriplets.Add(new Triplet { CardNumber = number, CardSuit = suit });
                AddCardIDsInCombination(suit, number, 1);
                yield return new WaitForSeconds(wait);
            }
            else if (HaveTwins(out number, suit)) // For Twins!!!!!!!!!!!!
            {
                //foundTwins.Add(new Twin { CardNumber = number, CardSuit = suit });
                AddCardIDsInCombination(suit, number, 0);
                yield return new WaitForSeconds(wait);
            }
        }
        _ = StartCoroutine(SetLessImportantCards());
    }

    private void AddCardIDsInCombination(CardSuit suit, int number, int combType)
    {
        if (combType == 0) foundTwins.Add(new Twin { CardNumber = number, CardSuit = suit });
        else if (combType == 1) foundTriplets.Add(new Triplet { CardNumber = number, CardSuit = suit });
        else foundQuadruplets.Add(new Quadruplet { CardNumber = number, CardSuit = suit });
        int j = 0;
        int jMax = combType == 0 ? 2 : combType == 1 ? 3 : 4;
        foreach (int i in cc.cardID_List)
        {
            if (j < jMax && Gc.cards[i].cardSuit == suit && Gc.cards[i].cardNumber == number)
            {
                if (combType == 0)
                {
                    foundTwins[^1].cardIDs[j] = i;
                    j++;
                    continue;
                }
                else if (combType == 1)
                {
                    foundTriplets[^1].cardIDs[j] = i;
                    j++;
                    continue;
                }
                foundQuadruplets[^1].cardIDs[j] = i;
                j++;
            }
        }
    }

    private IEnumerator SetLessImportantCards()
    {
        const float wait = 0.2f;
        lessImportantCards.Clear();
        lessImportantCards1.Clear();
        foreach (int id in cc.cardID_List)
        {
            if (CheckStreet(id) || CheckQuadruplet(id) || CheckTriplet(id) || CheckTwin(id))
            {
                yield return new WaitForSeconds(wait);
                continue;
            }
            if (id > 9)
            {
                lessImportantCards.Enqueue(id);
                lessImportantCards1.Add(id);
            }
        }
        int cardID = GetCardIDToExpose();
        yield return new WaitForSeconds(wait);
        cc.exposeCardID = cardID;
        haveCardToExpose = cardID != -1;
    }

    private bool CheckStreet(int cardToCheck = -1)
    {
        if (haveStreet)
        {
            if (streetSuit == Gc.cards[cardToCheck].cardSuit)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckQuadruplet(int cardToCheck = -1)
    {
        if (foundQuadruplets.Count == 0)
        {
            return false;
        }
        foreach (Quadruplet quad in foundQuadruplets)
        {
            if (quad.CardSuit == Gc.cards[cardToCheck].cardSuit && quad.CardNumber == Gc.cards[cardToCheck].cardNumber)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckTriplet(int cardToCheck = -1)
    {
        if (foundTriplets.Count == 0)
        {
            return false;
        }
        foreach (Triplet triple in foundTriplets)
        {
            if (triple.CardSuit == Gc.cards[cardToCheck].cardSuit && triple.CardNumber == Gc.cards[cardToCheck].cardNumber)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckTwin(int cardToCheck = -1)
    {
        if (foundTwins.Count == 0)
        {
            return false;
        }
        foreach (Twin twin in foundTwins)
        {
            if (twin.CardSuit == Gc.cards[cardToCheck].cardSuit && twin.CardNumber == Gc.cards[cardToCheck].cardNumber)
            {
                return true;
            }
        }
        return false;
    }
}
