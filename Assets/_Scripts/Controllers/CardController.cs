using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    #region Fields
    public bool ready;
    public bool currentPlayer;
    public bool haveBeginnerCard;
    public bool haveBuzzerOption;
    public bool haveFaceDownPileOption;
    public int exposeCardID;
    public int playerNumber;
    public string playerName;
    public SpriteRenderer redSprite; // for Intelligent Card
    public TextMeshPro timerText; // for Intelligent Card
    public TextMeshPro numberText; // for Intelligent Card
    public Transform aiCardPlace; // for Intelligent Card
    public Image playerGreenImage; // for Player
    public TextMeshProUGUI playerTimerText; // for Player
    public List<int> cardID_List;

    private int aiExposeCardID; // for Intelligent Card
    [SerializeField]
    private int previousExposeCardID;
    private Coroutine timerRoutine;
    private TransformSet discardCardTrans;

    private IntelligentCard ic; // for Intelligent Card
    private GameController Gc => GameController.gc;
    private TouchManager Tm => TouchManager.tm;
    private GameplayUI Gui => GameplayUI.gUI;

    private const float DURATION = 0.3f;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        ic = GetComponent<IntelligentCard>();
    }

    private void Start()
    {
        SetTimerText(enable: false);
        SetTimerGlow();
        previousExposeCardID = -1;
        aiExposeCardID = -1;
        if (numberText != null) { numberText.text = playerNumber.ToString(); }
    }
    #endregion

    #region Public Methods
    public void StartReadyRoutine()
    {
        if (playerNumber == 0) { return; }
        _ = StartCoroutine(ReadyRoutine());
    }

    public void ResetReadyText()
    {
        if (playerNumber == 0) { return; }
        timerText.text = "";
    }

    public void AssignCardID_List(List<int> cardID_List)
    {
        this.cardID_List = cardID_List;
        if (playerNumber == 0)
        {
            _ = StartCoroutine(ActivateCardsRoutine());
            Gui.readyButtonGO.SetActive(true);
        }
        else
        {
            /*
             * Start Thinking of AI
             */
            this.cardID_List.Sort();
            SetReadyText();
        }
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
            Gc.ExposeCardOnTable(cardID, pickable: true);
            Gui.readyButtonGO.SetActive(true);
        }
        else
        {
            /*
             * Start Thinking of AI
             * Check if beginner card is important
             * if not then discard the beginner card
             * otherwise check the other card which one is not forming
             * any kind of combination then discard that card
             */
            ic.FindCardToDiscard();
        }
    }

    public void ExposeCard(bool random = false, bool isAI = false)
    {
        if (random && !haveBeginnerCard)
        {
            exposeCardID = Tm.selectedCard != null ? Tm.selectedCard.GetComponent<Card>().cardID :
                exposeCardID = cardID_List[UnityEngine.Random.Range(0, cardID_List.Count)];
        }
        else if (random && haveBeginnerCard)
        {
            exposeCardID = cardID_List[UnityEngine.Random.Range(0, cardID_List.Count)];
        }
        else if (playerNumber == 0 && exposeCardID == -1)
        {
            exposeCardID = Tm.selectedCard.GetComponent<Card>().cardID;
        }
        exposeCardID = isAI && aiExposeCardID != -1 ? aiExposeCardID : exposeCardID;
        Gc.SetCardPickable(exposeCardID);
        if (playerNumber == 0)
        {
            Gui.SetSellectedCard();
            Gui.exposeButtonGO.SetActive(false);
            Gc.ExposeCardID(exposeCardID);
            if (!haveBeginnerCard)
            {
                _ = StartCoroutine(ExposeDiscardCardRoutine(Gc.cards[exposeCardID].transform));
                Tm.selectedCard = null;
            }
        }
        else
        {
            //Gc.ExposeCardOnTable(exposeCardID);
            _ = StartCoroutine(DealDiscardAICardRoutine(Gc.cards[exposeCardID].transform, false));
            Gc.ExposeCardID(exposeCardID);
        }
        RemoveCardIDFromHand(exposeCardID);
        StopTimerRoutine();
        Gc.BuzzerCall();
        Gc.allowBuzzer = true;
        exposeCardID = -1;
        aiExposeCardID = -1;
        if (haveBeginnerCard) { haveBeginnerCard = false; }
        Gc.SetPlayerTurn(playerNumber);
        //if (gc.previousTurnNumber == (gc.playerTurnNumber - 1) % gc.players.Count)
        //{
        //    gc.SetPlayerTurn(playerNumber);
        //    gc.previousTurnNumber = playerNumber;
        //}
        //else
        //{
        //    gc.playerTurnNumber = gc.previousTurnNumber;
        //}
        Gui.CallNotification(playerName + " have discarded a card!\r\nNow its " + Gc.players[Gc.playerTurnNumber].playerName
            + " turn", resetText: false);
        Gc.SetTurnText();
    }

    public void BuzzerCall()
    {
        Gc.ResetTurnText(Gc.playerTurnNumber);
        AddCardIDToHand(Gc.exposeCardID);
        if (Gc.buzzerCallerID == -1 && Gc.allowBuzzer && haveBuzzerOption)
        {
            haveBuzzerOption = false;
            Gc.BuzzerCall(playerNumber);
            Gc.allowBuzzer = false;
            Gui.exposeButtonGO.SetActive(playerNumber == 0);
            if (playerNumber == 0)
            {
                if (Gc.turnRoutine != null) { StopCoroutine(Gc.turnRoutine); }
                Gc.turnRoutine = null;
                Gui.buzzerCountText.text = "0";
                _ = StartCoroutine(SetDealtCardRoutine(Gc.cards[Gc.exposeCardID].gameObject.transform));
                Gui.CallNotification("Please discard a card!\r\nOtherwise a random card will be discarded!", resetText: false);
            }
            else
            {
                cardID_List.Sort();
                Gc.cards[Gc.exposeCardID].pickable = false;
                //Gc.cards[Gc.exposeCardID].gameObject.SetActive(false);
                _ = StartCoroutine(DealDiscardAICardRoutine(Gc.cards[Gc.exposeCardID].transform));
                Gui.CallNotification("Please discard a card!\r\nOtherwise a random card will be discarded!", resetText: false);
            }
            timerRoutine = StartCoroutine(TimerRoutine(15));
            previousExposeCardID = Gc.exposeCardID;
            if (Gc.playerTurnNumber != playerNumber)
            {
                Gc.players[Gc.playerTurnNumber].haveFaceDownPileOption = true;
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
        Gui.exposeButtonGO.SetActive(playerNumber == 0);
        timerRoutine = StartCoroutine(TimerRoutine(15));
    }
    #endregion

    #region Coroutine
    private IEnumerator ReadyRoutine()
    {
        int randomReady = Random.Range(1, Gc.sortingTime / 2);
        while (randomReady > 0)
        {
            yield return new WaitForSeconds(1f);
            randomReady--;
        }
        ready = true;
        SetReadyText(true);
        Gc.CheckIfAllPlayersReady();
    }

    private IEnumerator ActivateCardsRoutine()
    {
        int i = 0;
        int count = cardID_List.Count;
        while (i < count)
        {
            Transform place = Gc.cardPlaces[i];
            place.GetComponent<SpriteRenderer>().enabled = false;
            Cell cell = place.GetComponent<Cell>();
            Card card = Gc.cards[cardID_List[i]];
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
        Vector2 endPos = new(Gc.dealtCardPlace.position.x, Gc.dealtCardPlace.position.y);
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
        TransformSet endTrans = new(new(Gc.centerPlace.position.x, Gc.centerPlace.position.y), Gc.centerPlace.localRotation,
            Gc.centerPlace.localScale);
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
        _ = StartCoroutine(GetDealtCardRoutine(Gc.cards[previousExposeCardID].transform));
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

    private IEnumerator TurnSetRoutine(int time = 20)
    {
        int randomBuzzerTime = playerNumber != 0 ? UnityEngine.Random.Range(1, time - 1) : 0;
        if (playerNumber != 0)
        {

        }
        while (time > 0)
        {
            if (time == 5) { NotifyTurnPlayer(); }
            if (randomBuzzerTime == time && ic.IsImportant(out aiExposeCardID))
            {
                BuzzerCall();
                Gc.turnRoutine = null;
                yield break;
            }
            yield return new WaitForSeconds(1);
            time--;
            SetTurnText(time % 2 == 0, time);
        }
        ResetTurnText();
        Gui.CallNotification("Turn skiped!\r\nNow its " + Gc.players[Gc.playerTurnNumber].playerName
            + " turn", resetText: false);
        Gc.turnRoutine = null;
        Gc.SetPlayerTurn(playerNumber);
        Gc.SetTurnText();
    }

    private IEnumerator DealDiscardAICardRoutine(Transform obj, bool deal = true)
    {
        obj.gameObject.SetActive(true);
        Transform centerPlace = Gc.centerPlace;
        TransformSet startTrans = new(centerPlace.position, centerPlace.rotation, centerPlace.localScale);
        TransformSet endTrans = new(aiCardPlace.position, aiCardPlace.rotation, aiCardPlace.localScale);
        if (!deal) { (startTrans, endTrans) = (endTrans, startTrans); }
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
        obj.gameObject.SetActive(!deal);
    }
    #endregion

    #region UI Methods
    private void SetReadyText(bool ready = false)
    {
        if (!ready)
        {
            timerText.text = "Ready?";
            timerText.color = Color.red;
        }
        else
        {
            timerText.color = Color.white;
            timerText.text = "Ready!";
        }
    }

    public void SetTurnText()
    {
        if (playerNumber != 0) { timerText.text = "Turn"; }
        else { playerTimerText.text = "Turn"; }
        Gc.turnRoutine = StartCoroutine(TurnSetRoutine(20));
    }

    public void ResetTurnText()
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

    private void SetTurnText(bool enable, int time = 0)
    {
        if (playerNumber != 0)
        {
            timerText.color = enable ? Color.red : Color.green;
            timerText.text = "Turn, " + time.ToString() + "s";
        }
        else
        {
            playerTimerText.color = enable ? Color.red : Color.green;
            playerTimerText.text = "Turn, " + time.ToString() + "s";
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
    #endregion

    #region Private Methods
    private void StopTimerRoutine()
    {
        if (timerRoutine != null) { StopCoroutine(timerRoutine); }
        timerRoutine = null;
        SetTimerText(enable: false);
        SetTimerGlow();
    }

    private void NotifyTurnPlayer()
    {
        Gui.CallNotification("In 5 Seconds or if someone else call Buzzer\r\n Your turn will be skiped!", resetText: false);
    }
    #endregion
}
