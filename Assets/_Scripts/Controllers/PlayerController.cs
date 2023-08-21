using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public bool currentPlayer;
    public bool haveBeginnerCard;
    public bool haveBuzzerOption;
    public bool haveFaceDownPileOption;
    public string playerName;
    public int playerNumber;
    public int exposeCardID;
    public SpriteRenderer redSprite;
    public TextMeshPro timerText;
    public TextMeshPro numberText;
    public Image playerGreenImage;
    public TextMeshProUGUI playerTimerText;
    public List<int> cardID_List;
    [SerializeField]
    private int previousExposeCardID;
    [SerializeField]
    private TransformSet discardCardTrans;
    private Coroutine timerRoutine;
    private GameController gc;
    private TouchManager tm;
    private const float DURATION = 0.3f;

    private void Start()
    {
        gc = GameController.gc;
        tm = TouchManager.tm;
        SetTimerText(enable: false);
        SetTimerGlow();
        previousExposeCardID = -1;
        if (numberText != null) { numberText.text = playerNumber.ToString(); }
    }

    public void AssignCardID_List(List<int> cardID_List)
    {
        this.cardID_List = cardID_List;
        if (playerNumber == 0) { _ = StartCoroutine(ActivateCardsRoutine()); }
    }

    public void AssignBeginnerCard(int cardID)
    {
        haveBuzzerOption = false;
        cardID_List.Add(cardID);
        haveBeginnerCard = true;
        exposeCardID = cardID;
        if (playerNumber == 0)
        {
            ExposeCardOnTable(cardID, pickable: true);
        }
    }

    public void ExposeCard()
    {
        AddCardIDToHand(previousExposeCardID);
        if (playerNumber == 0)
        {
            exposeCardID = haveBeginnerCard ? exposeCardID : tm.selectedCard.GetComponent<Card>().cardID;
            gc.cards[exposeCardID].pickable = false;
            gc.ExposeCardID(exposeCardID);
            if (!haveBeginnerCard)
            {
                _ = StartCoroutine(ExposeDiscardCardRoutine(tm.selectedCard.transform));
                tm.selectedCard = null;
            }
        }
        else
        {
            ExposeCardOnTable(exposeCardID);
            gc.ExposeCardID(exposeCardID);
        }
        RemoveCardIDFromHand(exposeCardID);
        StopTimerRoutine();
        gc.BuzzerCall();
        gc.allowBuzzer = true;
        exposeCardID = -1;
        if (haveBeginnerCard) { haveBeginnerCard = false; }
        GameplayUI.gUI.CallNotification(playerName + " have discarded a card!", 4);
    }

    public void BuzzerCall()
    {
        if (gc.buzzerCallerID == -1 && gc.allowBuzzer && haveBuzzerOption)
        {
            haveBuzzerOption = false;
            gc.BuzzerCall(playerNumber);
            gc.allowBuzzer = false;
            if (playerNumber == 0)
            {
                _ = StartCoroutine(SetDealtCardRoutine(gc.cards[gc.exposeCardID].gameObject.transform));
                GameplayUI.gUI.exposeButtonGO.SetActive(true);
                GameplayUI.gUI.CallNotification("Please discard a card!\r\nOtherwise a random card will be discarded!", 6);
            }
            timerRoutine = StartCoroutine(TimerRoutine(15));
            previousExposeCardID = gc.exposeCardID;
            if (gc.playerTurnNumber != playerNumber)
            {
                gc.players[gc.playerTurnNumber].haveFaceDownPileOption = true;
            }
        }
    }

    public void AddCardIDToHand(int cardID)
    {
        if (cardID != -1) { cardID_List.Add(cardID); }
    }

    public void RemoveCardIDFromHand(int cardID)
    {
        if (cardID_List.Contains(cardID)) { _ = cardID_List.Remove(cardID); }
    }

    public void SortCards() // for Ai
    {
        if (playerNumber != 0)
        {
            cardID_List.Sort();
        }
    }

    public void Discard_a_Card()
    {
        GameplayUI.gUI.exposeButtonGO.SetActive(true);
        timerRoutine = StartCoroutine(TimerRoutine(15));
    }

    private IEnumerator ActivateCardsRoutine()
    {
        int i = 0;
        int count = cardID_List.Count;
        while (i < count)
        {
            Transform place = gc.cardPlaces[i];
            place.GetComponent<SpriteRenderer>().enabled = false;
            Cell cell = place.GetComponent<Cell>();
            Card card = gc.cards[cardID_List[i]];
            card.pickable = true;
            card.gameObject.SetActive(true);
            card.transform.SetPositionAndRotation(place.position, place.rotation);
            card.transform.localScale = place.localScale;
            cell.card = card;
            cell.IsOccupide = true;
            i++;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator TimerRoutine(int time)
    {
        int cardExposeTime = 0;
        if (playerNumber != 0)
        {
            cardExposeTime = UnityEngine.Random.Range(0, time - 3);
        }
        SetTimerText(time);
        SetTimerGlow(time % 2 != 0);
        while (time > 0)
        {
            yield return new WaitForSeconds(1);
            time--;
            SetTimerText(time);
            SetTimerGlow(time % 2 != 0);
            if (playerNumber != 0 && cardExposeTime == time)
            {
                ExposeCard();
            }
        }
        SetTimerText(enable: false);
        SetTimerGlow();
        timerRoutine = null;
    }

    private IEnumerator SetDealtCardRoutine(Transform obj)
    {
        Vector2 startPos = new(obj.position.x, obj.position.y);
        Vector2 endPos = new(gc.dealtCardPlace.position.x, gc.dealtCardPlace.position.y);
        float progress = 0;
        float elapsedTime = 0;
        while (progress <= 1)
        {
            obj.position = Vector2.Lerp(startPos, endPos, progress);
            elapsedTime += Time.deltaTime;
            progress = elapsedTime / DURATION;
            yield return null;
        }
        obj.position = endPos;
    }

    private IEnumerator ExposeDiscardCardRoutine(Transform obj)
    {
        obj.GetComponent<SpriteRenderer>().color = Color.white;
        discardCardTrans = new(obj.position, obj.rotation, obj.localScale);
        TransformSet startTrans = discardCardTrans;
        TransformSet endTrans = new(new(gc.centerPlace.position.x, gc.centerPlace.position.y), gc.centerPlace.localRotation,
            gc.centerPlace.localScale);
        float progress = 0;
        float elapsedTime = 0;
        while (progress <= 1)
        {
            obj.SetPositionAndRotation(Vector2.Lerp(startTrans.position, endTrans.position, progress),
                Quaternion.Slerp(startTrans.rotation, endTrans.rotation, progress));
            obj.localScale = Vector3.Lerp(startTrans.scale, endTrans.scale, progress);
            elapsedTime += Time.deltaTime;
            progress = elapsedTime / DURATION;
            yield return null;
        }
        obj.SetPositionAndRotation(endTrans.position, endTrans.rotation);
        obj.localScale = endTrans.scale;
        _ = StartCoroutine(GetDealtCardRoutine(gc.cards[previousExposeCardID].transform));
    }

    private IEnumerator GetDealtCardRoutine(Transform obj)
    {
        TransformSet startTrans = new(obj.position, obj.rotation, obj.localScale);
        TransformSet endTrans = discardCardTrans;
        float progress = 0;
        float elapsedTime = 0;
        while (progress <= 1)
        {
            obj.SetPositionAndRotation(Vector2.Lerp(startTrans.position, endTrans.position, progress),
                Quaternion.Slerp(startTrans.rotation, endTrans.rotation, progress));
            obj.localScale = Vector3.Lerp(startTrans.scale, endTrans.scale, progress);
            elapsedTime += Time.deltaTime;
            progress = elapsedTime / DURATION;
            yield return null;
        }
        obj.SetPositionAndRotation(endTrans.position, endTrans.rotation);
        obj.localScale = endTrans.scale;
        obj.GetComponent<Card>().pickable = true;
        previousExposeCardID = -1;
    }

    private void SetTimerText(int time = 0, bool enable = true)
    {
        if (playerNumber != 0) { timerText.text = enable ? time.ToString() : ""; }
        else { playerTimerText.text = enable ? time.ToString() : ""; }
    }

    private void SetTimerGlow(bool enable = false)
    {
        if (playerNumber != 0) { redSprite.color = enable ? Color.red : Color.white; }
        else { playerGreenImage.color = enable ? Color.green : Color.white; }
    }

    private void StopTimerRoutine()
    {
        if (timerRoutine != null) { StopCoroutine(timerRoutine); }
        timerRoutine = null;
        SetTimerText(enable: false);
        SetTimerGlow();
    }

    private void ExposeCardOnTable(int cardID, bool pickable = false)
    {
        Transform place = gc.centerPlace;
        //Cell cell = place.GetComponent<Cell>();
        Card card = gc.cards[cardID];
        card.pickable = pickable;
        card.gameObject.SetActive(true);
        card.transform.SetPositionAndRotation(place.position, place.rotation);
        card.transform.localScale = place.localScale;
        //place.gameObject.SetActive(false);
    }
}
