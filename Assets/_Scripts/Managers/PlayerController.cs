using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool currentPlayer;
    public bool haveBeginnerCard;
    public string playerName;
    public int playerNumber;
    public int exposeCardID;
    public List<int> cardID_List;

    private void Start()
    {
        GameController.gc.players.Add(this);
        playerNumber = GameController.gc.players.Count - 1;
    }

    public void AssignCardID_List(List<int> cardID_List)
    {
        this.cardID_List = cardID_List;
        if (playerNumber == 0) { _ = StartCoroutine(ActivateCardsRoutine()); }
    }

    public void AssignBeginnerCard(int cardID)
    {
        cardID_List.Add(cardID);
        haveBeginnerCard = true;
        exposeCardID = cardID;
        if (playerNumber == 0)
        {
            Transform place = GameController.gc.centerPlace;
            Cell cell = place.GetComponent<Cell>();
            Card card = GameController.gc.cards[cardID];
            card.gameObject.SetActive(true);
            card.transform.SetPositionAndRotation(place.position, place.rotation);
            card.transform.localScale = place.localScale;
            place.gameObject.SetActive(false);
            GameplayUI.gUI.exposeButtonGO.SetActive(true);
        }
    }

    public void ExposeCard()
    {
        if (playerNumber == 0)
        {
            GameController.gc.ExposeCardID(exposeCardID);
        }
        else
        {
            Transform place = GameController.gc.centerPlace;
            Cell cell = place.GetComponent<Cell>();
            Card card = GameController.gc.cards[exposeCardID];
            card.gameObject.SetActive(true);
            card.transform.SetPositionAndRotation(place.position, place.rotation);
            card.transform.localScale = place.localScale;
            place.gameObject.SetActive(false);
            GameController.gc.ExposeCardID(exposeCardID);
        }
    }

    private IEnumerator ActivateCardsRoutine()
    {
        int i = 0;
        int count = cardID_List.Count;
        while (i < count)
        {
            Transform place = GameController.gc.cardPlaces[i];
            place.GetComponent<SpriteRenderer>().enabled = false;
            Cell cell = place.GetComponent<Cell>();
            Card card = GameController.gc.cards[cardID_List[i]];
            card.gameObject.SetActive(true);
            card.transform.SetPositionAndRotation(place.position, place.rotation);
            card.transform.localScale = place.localScale;
            cell.card = card;
            cell.IsOccupide = true;
            i++;
            yield return new WaitForSeconds(0f);
        }
    }
}
