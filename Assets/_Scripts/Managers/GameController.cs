using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController gc;
    public int exposeCardID;
    public Transform centerPlace;
    public List<Card> cards;
    public List<Transform> cardPlaces;
    public List<PlayerController> players;

    private void Awake()
    {
        gc = this;
    }

    public void ExposeCardID(int cardID = -1)
    {
        exposeCardID = cardID;
    }
}
