using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController gc;

    public List<Card> cards;
    public List<Transform> cardPlaces;
    public Transform centerPlace;
    public List<PlayerController> players;

    private void Awake()
    {
        gc = this;
    }
}
