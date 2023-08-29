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
 */
public class CardAI : MonoBehaviour
{
    private GameController Gc => GameController.gc;
    private CardController pc;
    private Coroutine checkCombinationRoutine;

    public bool haveStreetCombination;
    public bool haveWenglaCombination;
    public int WenglaCombinationNumber;
    public CardSuit streetCombinationSuit;
    public Dictionary<CardSuit, List<int>> suitCardIDs = new();
    public List<TwinCombination> foundTwins = new();
    public List<TripletCombination> foundTriplets = new();
    public List<QuadrupletCombination> foundQuadruplets = new();

    private void Awake()
    {
        pc = GetComponent<CardController>();
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
    }

    public void SetSuitList()
    {
        ClearSuitCardIDs();
        foreach (int ID in pc.cardID_List)
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
        cardID = pc.cardID_List.LastOrDefault();
        return Gc.exposeCardID < cardID;
    }

    //2 cards with same number and suit
    public bool HaveTwins(out int cardNumber, CardSuit targetCardSuit)
    {
        if (!suitCardIDs.ContainsKey(targetCardSuit) || suitCardIDs[targetCardSuit].Count < 2)
        {
            cardNumber = -1;
            return false;
        }

        HashSet<int> seenNumbers = new();

        foreach (int ID in suitCardIDs[targetCardSuit])
        {
            if (seenNumbers.Contains(Gc.cards[ID].cardNumber))
            {
                cardNumber = Gc.cards[ID].cardNumber;
                return true;
            }
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

        Dictionary<int, int> cardCounts = new();

        foreach (int ID in suitCardIDs[targetCardSuit])
        {
            int cardNum = Gc.cards[ID].cardNumber;

            if (cardCounts.ContainsKey(cardNum))
            {
                cardCounts[cardNum]++;
                if (cardCounts[cardNum] >= 3)
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
            if (firstCardNum != Gc.cards[pc.cardID_List[i]].cardNumber)
            {
                cardNumber = -1;
                return false;
            }
        }

        cardNumber = firstCardNum;
        return true;
    }

    private int SetFirstCardNumber(out int count)
    {
        int j = 0;
        foreach (int i in pc.cardID_List)
        {
            if (Gc.cards[pc.cardID_List[i]].cardNumber != 0)
            {
                count = j;
                return Gc.cards[pc.cardID_List[i]].cardNumber;
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
        haveWenglaCombination = HaveWengla(out WenglaCombinationNumber);
        if (haveWenglaCombination) { yield break; }

        yield return new WaitForSeconds(0.3f);

        List<CardSuit> suitsToCheck = new() { CardSuit.Pentagone, CardSuit.Squre, CardSuit.Triangle, CardSuit.Circle };

        foreach (CardSuit suit in suitsToCheck)
        {
            yield return new WaitForSeconds(0.1f);// For Street!!!!!!!!!!!!

            if (HaveStreet(suit))
            {
                haveStreetCombination = true;
                streetCombinationSuit = suit;
            }

            yield return new WaitForSeconds(0.3f);// For Quadruplets!!!!!!!!!!!!

            if (HaveQuadruplets(out int number, suit))
            {
                foundQuadruplets.Add(new QuadrupletCombination { CardNumber = number, CardSuit = suit });
            }

            yield return new WaitForSeconds(0.3f);// For Triplets!!!!!!!!!!!!

            if (HaveTriplets(out number, suit))
            {
                foundTriplets.Add(new TripletCombination { CardNumber = number, CardSuit = suit });
            }

            yield return new WaitForSeconds(0.3f);// For Twins!!!!!!!!!!!!

            if (HaveTwins(out number, suit))
            {
                foundTwins.Add(new TwinCombination { CardNumber = number, CardSuit = suit });
            }

            yield return new WaitForSeconds(0.3f);
        }
    }
}

// 234 -- 324
// 234 -- 275

// priorities
// unique
// only
// rearest

/*
yield return new WaitForSeconds(0.3f);// For Street!!!!!!!!!!!!
if (HaveStreet(CardSuit.Pentagone))
{
    haveStreetCombination = true;
    streetCombinationSuit = CardSuit.Pentagone;
    yield return new WaitForSeconds(0.1f);
}
else if (HaveStreet(CardSuit.Squre))
{
    haveStreetCombination = true;
    streetCombinationSuit = CardSuit.Squre;
    yield return new WaitForSeconds(0.1f);
}
else if (HaveStreet(CardSuit.Triangle))
{
    haveStreetCombination = true;
    streetCombinationSuit = CardSuit.Triangle;
    yield return new WaitForSeconds(0.1f);
}
else if (HaveStreet(CardSuit.Circle))
{
    haveStreetCombination = true;
    streetCombinationSuit = CardSuit.Circle;
    yield return new WaitForSeconds(0.1f);
}
yield return new WaitForSeconds(0.3f);// For Quadruplets!!!!!!!!!!!!
if (HaveQuadruplets(out int number, CardSuit.Pentagone))
{
    foundQuadruplets.Add(new QuadrupletCombination { CardNumber = number, CardSuit = CardSuit.Pentagone });
}
yield return new WaitForSeconds(0.3f);
if (HaveQuadruplets(out number, CardSuit.Squre))
{
    foundQuadruplets.Add(new QuadrupletCombination { CardNumber = number, CardSuit = CardSuit.Squre });
}
yield return new WaitForSeconds(0.3f);
if (HaveQuadruplets(out number, CardSuit.Triangle))
{
    foundQuadruplets.Add(new QuadrupletCombination { CardNumber = number, CardSuit = CardSuit.Triangle });
}
yield return new WaitForSeconds(0.3f);
if (HaveQuadruplets(out number, CardSuit.Circle))
{
    foundQuadruplets.Add(new QuadrupletCombination { CardNumber = number, CardSuit = CardSuit.Circle });
}
yield return new WaitForSeconds(0.3f);// For Triplets!!!!!!!!!!!!
if (HaveTriplets(out number, CardSuit.Pentagone))
{
    foundTriplets.Add(new TripletCombination { CardNumber = number, CardSuit = CardSuit.Pentagone });
}
yield return new WaitForSeconds(0.3f);
if (HaveTriplets(out number, CardSuit.Squre))
{
    foundTriplets.Add(new TripletCombination { CardNumber = number, CardSuit = CardSuit.Squre });
}
yield return new WaitForSeconds(0.3f);
if (HaveTriplets(out number, CardSuit.Triangle))
{
    foundTriplets.Add(new TripletCombination { CardNumber = number, CardSuit = CardSuit.Triangle });
}
yield return new WaitForSeconds(0.3f);
if (HaveTriplets(out number, CardSuit.Circle))
{
    foundTriplets.Add(new TripletCombination { CardNumber = number, CardSuit = CardSuit.Circle });
}
yield return new WaitForSeconds(0.3f);// For Twins!!!!!!!!!!!!
if (HaveTwins(out number, CardSuit.Pentagone))
{
    foundTwins.Add(new TwinCombination { CardNumber = number, CardSuit = CardSuit.Pentagone });
}
yield return new WaitForSeconds(0.3f);
if (HaveTwins(out number, CardSuit.Squre))
{
    foundTwins.Add(new TwinCombination { CardNumber = number, CardSuit = CardSuit.Squre });
}
yield return new WaitForSeconds(0.3f);
if (HaveTwins(out number, CardSuit.Triangle))
{
    foundTwins.Add(new TwinCombination { CardNumber = number, CardSuit = CardSuit.Triangle });
}
yield return new WaitForSeconds(0.3f);
if (HaveTwins(out number, CardSuit.Circle))
{
    foundTwins.Add(new TwinCombination { CardNumber = number, CardSuit = CardSuit.Circle });
}
yield return new WaitForSeconds(0.3f);
*/
