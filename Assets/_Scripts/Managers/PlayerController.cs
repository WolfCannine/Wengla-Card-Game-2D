using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string playerName;
    public int playerNumber;
    public List<Card> hand;
    public List<int> cardID_List;
    public int beginnerCardID;
    public bool haveBeginnerCard;

    private void Start()
    {
        GameController.gc.players.Add(this);
        playerNumber = GameController.gc.players.Count - 1;
    }

    public void AssignCardID_List(List<int> cardID_List)
    {
        this.cardID_List = cardID_List;
        if (playerNumber == 0) ActivateCards();
    }

    public void AssignBeginnerCard(int cardID)
    {
        beginnerCardID = cardID;
        haveBeginnerCard = true;
        Card card = GameController.gc.cards[cardID];
        Transform place = GameController.gc.centerPlace;
        card.gameObject.SetActive(true);
        card.gameObject.SetActive(true);
        card.transform.SetPositionAndRotation(place.position, place.rotation);
        card.transform.localScale = place.localScale;
        place.gameObject.SetActive(false);
        GameplayUI.gUI.CallNotification("Beginner Card is Assign to Player: " + playerNumber);
    }

    private void ActivateCards()
    {
        int count = cardID_List.Count;
        for (int i = 0; i < count; i++)
        {
            Transform place = GameController.gc.cardPlaces[i];
            Card card = GameController.gc.cards[cardID_List[i]];
            card.gameObject.SetActive(true);
            card.transform.SetPositionAndRotation(place.position, place.rotation);
            card.transform.localScale = place.localScale;
            place.gameObject.SetActive(false);
        }
    }
}
