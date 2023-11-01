using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IntelligentCard : MonoBehaviour
{
    #region Fields
    public bool haveCardToExpose;
    public bool haveStreet;
    public bool haveWengla;
    public int WenglaNumber;
    public int cardNumberWithMaxCount;
    public CardSuit cardSuitWithMaxCount;
    public IntelligentCardMode iCMode;
    public CardSuit streetSuit;
    public Street foundStreet;
    public Wengla foundWengla;
    public List<Twin> foundTwins = new();
    public List<Triplet> foundTriplets = new();
    public List<Quadruplet> foundQuadruplets = new();
    public List<int> lessImportantCards1 = new();
    public List<int> cardIDsWithMaxCounts = new();
    public List<int> cardIDsHavingNearStreet = new();
    public Queue<int> lessImportantCards = new();
    public Dictionary<CardSuit, List<int>> suitCardIDs = new();
    private Dictionary<CardSuit, List<int>> suitCardDifferentIDs = new();
    private CardController cc;
    private Coroutine checkCombinationRoutine;
    private GameController Gc => GameController.gc;
    //private int cardToExpose = -1;
    #endregion

    private void Awake()
    {
        cc = GetComponent<CardController>();
    }

    #region Public Methods
    public void FindCardToDiscard()
    {
        CheckAllCombinations();
    }

    public void CheckAllCombinations()
    {
        if (checkCombinationRoutine != null) { StopCoroutine(checkCombinationRoutine); }
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
        foreach (int id in cc.cardID_List)
        {
            Card card = Gc.cards[id];
            if (suitCardIDs.ContainsKey(card.cardSuit))
            {
                suitCardIDs[card.cardSuit].Add(id);
                continue;
            }
            suitCardIDs.Add(card.cardSuit, new List<int> { id });
        }
    }

    public bool IsCardImp() // check card importance
    {
        if (Gc.exposeCardID < 10) { return true; }

        int lessImpCardID = GetLessImpCardID;
        int xCardNumber = Gc.cards[Gc.exposeCardID].cardNumber;
        CardSuit xCardSuit = Gc.cards[Gc.exposeCardID].cardSuit;

        return lessImpCardID != -1 && CheckAllForImp(xCardNumber, xCardSuit);
    }

    public bool CheckAllForImp(int xCardNumber, CardSuit xCardSuit)
    {
        return MaxNumberExist(xCardNumber) || QuadExist(xCardNumber, xCardSuit) || TriExist(xCardNumber, xCardSuit) ||
            DuoExist(xCardNumber, xCardSuit) || SameExist(xCardNumber, xCardSuit) || SameNumberExist(xCardNumber);
    }

    public int GetCardIDToExpose()
    {
        int lessImpCardID = GetLessImpCardID;

        return lessImpCardID != -1 ? lessImpCardID : foundTwins.Count > 0 ? foundTwins[0].cardIDs[0] : foundTriplets.Count > 0 ?
            foundTriplets[0].cardIDs[0] : foundQuadruplets.Count > 0 ? foundQuadruplets[0].cardIDs[0] : foundQuadruplets[0].cardIDs[0];
    }

    public int GetLessImpCardID => lessImportantCards.Count != 0 ? lessImportantCards.Dequeue() : -1;

    public bool MaxNumberExist(int xCardNumber)
    {
        cardNumberWithMaxCount = cardNumberWithMaxCount == -1 ? FindCardNumberWithMaxCount() : cardNumberWithMaxCount;
        return xCardNumber == cardNumberWithMaxCount;
    }

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

    // for wengla formation
    public int FindCardNumberWithMaxCount()
    {
        cardIDsWithMaxCounts.Clear();
        Dictionary<int, List<int>> cardCounts = new();

        foreach (int id in cc.cardID_List)
        {
            int cardNum = Gc.cards[id].cardNumber;
            if (cardNum == 0) { continue; }
            if (cardCounts.ContainsKey(cardNum))
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

    public CardSuit FindCardSuitWithMaxCount(out List<int> numbers)
    {
        cardIDsHavingNearStreet.Clear();

        int maxcount = 0;
        foreach (CardSuit k in suitCardIDs.Keys)
        {
            if (suitCardIDs[k].Count > maxcount)
            {
                maxcount = suitCardIDs[k].Count;
                cardSuitWithMaxCount = k;
            }
            List<int> cardIDs = suitCardIDs[k];
            foreach (int id in cardIDs)
            {
                if (cardIDs.Contains(Gc.cards[id].cardNumber))
                {

                }
            }
        }

        numbers = new();
        return cardSuitWithMaxCount;
    }
    #endregion

    #region Private Methods
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

    private bool CheckQuad(int cardToCheck = -1)
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
    #endregion

    #region Routines
    private IEnumerator CheckCardsCombinations()
    {
        WaitForSeconds wait = Helper.GetWait(0.3f);
        SetSuitList();

        yield return wait;
        haveWengla = HaveWengla(out WenglaNumber);
        foundWengla = new Wengla { CardNumber = WenglaNumber };
        if (haveWengla)
        {
            Debug.Log(cc.playerName + " won!");
            yield break;
        }

        yield return wait;

        List<CardSuit> suitsToCheck = new() { CardSuit.Pentagone, CardSuit.Squre, CardSuit.Triangle, CardSuit.Circle };

        foreach (CardSuit suit in suitsToCheck)
        {
            yield return wait;

            if (HaveStreet(suit))
            {
                haveStreet = true;
                streetSuit = suit;
                foundStreet = new Street { CardSuit = suit };
            }

            yield return wait;

            if (HaveQuadruplets(out int number, suit))
            {
                AddCardIDsInCombination(suit, number, 2);
                yield return wait;
            }
            else if (HaveTriplets(out number, suit))
            {
                AddCardIDsInCombination(suit, number, 1);
                yield return wait;
            }
            else if (HaveTwins(out number, suit))
            {
                AddCardIDsInCombination(suit, number, 0);
                yield return wait;
            }
        }
        _ = StartCoroutine(SetLessImportantCards());
    }

    private IEnumerator SetLessImportantCards()
    {
        int maxCountCardNum = FindCardNumberWithMaxCount();
        lessImportantCards.Clear();
        lessImportantCards1.Clear();
        foreach (int id in cc.cardID_List)
        {
            if (id < 10 || CheckStreet(id) || CheckQuad(id) || CheckTriplet(id) || CheckTwin(id) ||
                Gc.cards[id].cardNumber == maxCountCardNum)
            {
                yield return Helper.GetWait(0.2f);
                continue;
            }
            lessImportantCards.Enqueue(id);
            lessImportantCards1 = lessImportantCards.ToList();
        }
        if (!haveCardToExpose)
        {
            int cardID = GetCardIDToExpose();
            yield return Helper.GetWait(0.2f);
            cc.exposeCardID = Gc.exposeCardID != cardID ? cardID : GetCardIDToExpose();
            haveCardToExpose = cardID != -1;
        }
        haveCardToExpose = true;
    }
    #endregion
}

//if (lessImpCardID != -1)
//{
//    return lessImpCardID;
//}
//else if (foundTwins.Count > 0)
//{
//    return foundTwins[0].cardIDs[0];
//}
//else if (foundTriplets.Count > 0)
//{
//    return foundTriplets[0].cardIDs[0];
//}
//else if (foundQuadruplets.Count > 0)
//{
//    return foundQuadruplets[0].cardIDs[0];
//}
//else
//{
//    return cc.cardID_List.LastOrDefault();
//}
/*
 * some act as making wengla
 * some act to make street
 * some collect quads and triplets and duo combination
 */
