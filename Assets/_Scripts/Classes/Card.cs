using UnityEngine;

public class Card : MonoBehaviour
{
    public bool pickable;

    public bool joker;
    public int cardID;
    public int cardNumber;
    public Cell cell;
    public CardSuit cardSuit;
    public CardCorners cardCorners;

    public int Rank { get; set; }
}
