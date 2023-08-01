using System;
using UnityEngine;

public class Card : MonoBehaviour
{
    public bool joker;
    public int cardID;
    public int cardNumber;
    public CardSuit cardSuit;
    public CardCorners cardCorners;

    //private Vector3 offset;

    //public void Start()
    //{

    //}

    //private void OnMouseDown()
    //{
    //    offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(x: Input.mousePosition.x, y: Input.mousePosition.y));
    //}

    //private void OnMouseDrag()
    //{
    //    Vector2 newPosition = Camera.main.ScreenToWorldPoint(new Vector3(x: Input.mousePosition.x, y: Input.mousePosition.y)) + offset;
    //    transform.position = newPosition;
    //}
}

[Serializable]
public enum CardSuit { None, Red, Green, Blue, Brown }

[Serializable]
public enum CardCorners { None, Zero, Three, Four, Five }
