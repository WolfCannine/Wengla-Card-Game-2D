using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private int aiExposeCardID;
    [SerializeField]
    private TransformSet discardCardTrans;
    private Coroutine timerRoutine;
    //private Coroutine turnRoutine;
    private GameController gc;
    private TouchManager tm;
    private GameplayUI gui;
    private const float DURATION = 0.3f;

    private void Start()
    {
        gc = GameController.gc;
        tm = TouchManager.tm;
        gui = GameplayUI.gUI;
        SetTimerText(enable: false);
        SetTimerGlow();
        previousExposeCardID = -1;
        aiExposeCardID = -1;
        if (numberText != null) { numberText.text = playerNumber.ToString(); }
    }

    public void AssignCardID_List(List<int> cardID_List)
    {
        this.cardID_List = cardID_List;
        if (playerNumber == 0) { _ = StartCoroutine(ActivateCardsRoutine()); }
        else { this.cardID_List.Sort(); }
    }

    public void AssignBeginnerCard(int cardID)
    {
        haveBuzzerOption = false;
        cardID_List.Add(cardID);
        if (playerNumber != 0) { cardID_List.Sort(); }
        haveBeginnerCard = true;
        exposeCardID = cardID;
        if (playerNumber == 0)
        {
            ExposeCardOnTable(cardID, pickable: true);
        }
        else
        {
            exposeCardID = exposeCardID != -1 && exposeCardID < 10 ? cardID_List.LastOrDefault() : exposeCardID;
        }
    }

    public void ExposeCard(bool random = false, bool isAI = false)
    {
        if (random && !haveBeginnerCard)
        {
            if (tm.selectedCard != null)
            {
                exposeCardID = tm.selectedCard.GetComponent<Card>().cardID;
            }
            else
            {
                exposeCardID = cardID_List[UnityEngine.Random.Range(0, cardID_List.Count)];
            }
        }
        else if (random && haveBeginnerCard)
        {
            exposeCardID = cardID_List[UnityEngine.Random.Range(0, cardID_List.Count)];
        }
        else if (playerNumber == 0 && exposeCardID == -1)
        {
            exposeCardID = tm.selectedCard.GetComponent<Card>().cardID;
        }
        AddCardIDToHand(previousExposeCardID);
        exposeCardID = isAI && aiExposeCardID != -1 ? aiExposeCardID : exposeCardID;
        gc.cards[exposeCardID].pickable = false;
        if (playerNumber == 0)
        {
            gui.exposeButtonGO.SetActive(false);
            gc.cards[exposeCardID].pickable = false;
            gc.ExposeCardID(exposeCardID);
            if (!haveBeginnerCard)
            {
                _ = StartCoroutine(ExposeDiscardCardRoutine(gc.cards[exposeCardID].transform));
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
        aiExposeCardID = -1;
        haveBeginnerCard = haveBeginnerCard ? false : haveBeginnerCard;
        gc.SetPlayerTurn(playerNumber);
        gui.CallNotification(playerName + " have discarded a card!\r\nNow its " + gc.players[gc.playerTurnNumber].playerName
            + " turn", resetText: false);
        gc.players[gc.playerTurnNumber].SetTurnText();
        Invoke(nameof(NotifyTurnPlayer), 5f);
    }

    public void BuzzerCall()
    {
        if (gc.buzzerCallerID == -1 && gc.allowBuzzer && haveBuzzerOption)
        {
            haveBuzzerOption = false;
            gc.BuzzerCall(playerNumber);
            gc.allowBuzzer = false;
            gui.exposeButtonGO.SetActive(playerNumber == 0);
            if (playerNumber == 0)
            {
                if (gc.turnRoutine != null) { StopCoroutine(gc.turnRoutine); }
                gc.turnRoutine = null;
                gui.buzzerCountText.text = "0";
                _ = StartCoroutine(SetDealtCardRoutine(gc.cards[gc.exposeCardID].gameObject.transform));
                gui.CallNotification("Please discard a card!\r\nOtherwise a random card will be discarded!", 6);
            }
            else
            {
                gc.cards[gc.exposeCardID].pickable = false;
                gc.cards[gc.exposeCardID].gameObject.SetActive(false);
                gui.CallNotification("Please discard a card!\r\nOtherwise a random card will be discarded!", resetText: false);
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

    public void SortCards()
    {
        cardID_List.Sort();
        _ = StartCoroutine(ActivateCardsRoutine());
    }

    public void Discard_a_Card()
    {
        gui.exposeButtonGO.SetActive(playerNumber == 0);
        timerRoutine = StartCoroutine(TimerRoutine(15));
    }

    public void SetTurnText()
    {
        if (playerNumber != 0) { timerText.text = "Turn"; }
        else { playerTimerText.text = "Turn"; }
        gc.turnRoutine = StartCoroutine(TurnSetRoutine(10));
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
            yield return new WaitForSeconds(0.0f);
        }
    }

    private IEnumerator TimerRoutine(int time)
    {
        int cardExposeTime = playerNumber != 0 ? UnityEngine.Random.Range(0, time - 3) : 0;
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
                ExposeCard(isAI: true);
            }
        }
        SetTimerText(enable: false);
        SetTimerGlow();
        timerRoutine = null;
        ExposeCard(random: true);
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

    private IEnumerator CheckExposeCard() // for AI
    {
        if (exposeCardID < 10)
        {
            exposeCardID = cardID_List.LastOrDefault();
        }
        yield return null;
    }

    private IEnumerator TurnSetRoutine(int time = 10)
    {
        int randomBuzzerTime = playerNumber != 0 ? UnityEngine.Random.Range(1, time - 1) : 0;
        while (time > 0)
        {
            if (randomBuzzerTime == time && IsImportant)
            {
                aiExposeCardID = gc.exposeCardID;
                ResetTurnText();
                BuzzerCall();
                gc.turnRoutine = null;
                yield break;
            }
            yield return new WaitForSeconds(1);
            time--;
            SetTurnText(time % 2 == 0);
        }
        ResetTurnText();
        gc.turnRoutine = null;
        gc.SetPlayerTurn(playerNumber);
        gui.CallNotification("Turn skiped!\r\nNow its " + gc.players[gc.playerTurnNumber].playerName
            + " turn", resetText: false);
        gc.players[gc.playerTurnNumber].SetTurnText();
        Invoke(nameof(NotifyTurnPlayer), 5f);
    }

    private bool IsImportant => gc.exposeCardID < cardID_List.LastOrDefault();

    private void SetTurnText(bool enable)
    {
        if (playerNumber != 0)
        {
            timerText.color = enable ? Color.red : Color.green;
        }
        else
        {
            playerTimerText.color = enable ? Color.red : Color.green;
        }
    }

    private void ResetTurnText()
    {
        if (playerNumber != 0)
        {
            timerText.color = Color.white;
            timerText.text = "";
        }
        else
        {
            playerTimerText.color = Color.black;
            playerTimerText.text = "";
        }
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
        Card card = gc.cards[cardID];
        card.pickable = pickable;
        card.gameObject.SetActive(true);
        card.transform.SetPositionAndRotation(place.position, place.rotation);
        card.transform.localScale = place.localScale;
    }

    private void NotifyTurnPlayer()
    {
        gui.CallNotification("In 5 Seconds or if someone else call Buzzer\r\n Your turn will be skiped!", resetText: false);
    }
}
