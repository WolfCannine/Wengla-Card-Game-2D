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

    [SerializeField]
    private int previousExposeCardID;
    private bool rightToBuzzer;
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
            return;
        }
        this.cardID_List.Sort();
        SetReadyText();
        ic.CheckAllCombinations();
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
            return;
        }
        ic.FindCardToDiscard();
    }

    public void ExposeCard(bool random = false, bool isAI = false)
    {
        if (random && !haveBeginnerCard)
        {
            exposeCardID = Tm.selectedCard != null ? Tm.selectedCard.GetComponent<Card>().cardID :
                exposeCardID = cardID_List[Random.Range(0, cardID_List.Count)];
        }
        else if (random && haveBeginnerCard)
        {
            exposeCardID = cardID_List[Random.Range(0, cardID_List.Count)];
        }
        else if (playerNumber == 0 && exposeCardID == -1)
        {
            Tm.selectedCard.GetComponent<SpriteRenderer>().color = Color.white;
            exposeCardID = Tm.selectedCard.GetComponent<Card>().cardID;
        }
        Gc.SetCardPickable(exposeCardID);
        if (playerNumber == 0)
        {
            Gui.SetSellectedCard();
            Gui.exposeButtonGO.SetActive(false);
            if (!haveBeginnerCard)
            {
                _ = StartCoroutine(ExposeDiscardCardRoutine(Gc.cards[exposeCardID].transform));
                Tm.selectedCard = null;
            }
        }
        else
        {
            _ = StartCoroutine(DealDiscardAICardRoutine(Gc.cards[exposeCardID].transform, false));
        }
        Gc.ExposeCardID(exposeCardID);
        RemoveCardIDFromHand(exposeCardID);
        StopTimerRoutine();
        Gc.SetBuzzerCallerID(allowBuzzer: true);
        exposeCardID = -1;
        haveBeginnerCard = false;
        if (rightToBuzzer) { Gc.SetPlayerTurn(playerNumber); }
        ExposeCardNotify(playerName, Gc.players[Gc.playerTurnNumber].playerName);
        Gc.SetTurnText();
        if (isAI) { ic.CheckAllCombinations(); }
    }

    public void BuzzerCall()
    {
        rightToBuzzer = Gc.playerTurnNumber == playerNumber;
        Gc.ResetTurnText(Gc.playerTurnNumber);
        AddCardIDToHand(Gc.exposeCardID);
        if (Gc.buzzerCallerID == -1 && Gc.allowBuzzer)
        {
            Gc.SetBuzzerCallerID(playerNumber);
            Gui.exposeButtonGO.SetActive(playerNumber == 0);
            if (playerNumber == 0)
            {
                StopTurnRoutine();
                Gui.buzzerCountText.text = "0";
                _ = StartCoroutine(SetDealtCardRoutine(Gc.cards[Gc.exposeCardID].transform));
            }
            else
            {
                cardID_List.Sort();
                Gc.cards[Gc.exposeCardID].pickable = false;
                _ = StartCoroutine(DealDiscardAICardRoutine(Gc.cards[Gc.exposeCardID].transform));
            }
            DiscardCallNotify();
            timerRoutine = StartCoroutine(TimerRoutine(15));
            previousExposeCardID = Gc.exposeCardID;
        }
    }

    private void StopTurnRoutine()
    {
        //if (Gc.turnRoutine != null) { StopCoroutine(Gc.turnRoutine); }
        //Gc.turnRoutine = null;
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

    #region Routines
    private IEnumerator ReadyRoutine()
    {
        int randomReady = Random.Range(1, Gc.sortingTime / 2);
        while (randomReady > 0)
        {
            yield return Helper.GetWait(1f);
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
            yield return Helper.GetWait(0f);
        }
    }

    private IEnumerator TimerRoutine(int time)
    {
        int cardExposeTime = playerNumber != 0 ? UnityEngine.Random.Range(0, time - 3) : 0;
        if (playerNumber != 0 && !haveBeginnerCard)
        {
            ic.haveCardToExpose = false;
            ic.FindCardToDiscard();
        }
        SetTimerText(time);
        SetTimerGlow(time % 2 != 0);
        while (time > 0)
        {
            yield return Helper.GetWait(1f);
            time--;
            SetTimerText(time);
            SetTimerGlow(time % 2 != 0);
            if (playerNumber != 0 && cardExposeTime >= time && ic.haveCardToExpose)
            {
                ExposeCard(isAI: true);
                yield break;
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
        if (playerNumber == 0) { Gui.SetSkipButton(true); }
        int randomBuzzerTime = playerNumber != 0 ? Random.Range(1, time - 1) : 0; // for ic(intelligent card)
        bool isImportant = false; // for ic(intelligent card)
        if (playerNumber != 0)
        {
            isImportant = ic.IsCardImp();
            if (!isImportant)
            {
                Gui.CallNotification("Turn Skip!");
                yield return Helper.GetWait(1.5f);
                SkipTurn();
                yield break;
            }
        }
        while (time > 0)
        {
            if (!Gc.allowBuzzer)
            {
                ResetTurnText();
                yield break;
            }
            if (time == 5) { NotifyTurnPlayer(); }
            if (haveBuzzerOption && randomBuzzerTime == time && isImportant)
            {
                haveBuzzerOption = false;
                BuzzerCall();
                Gc.turnRoutine = null;
                yield break;
            }
            yield return Helper.GetWait(1f);
            time--;
            SetTurnText(time % 2 == 0, time);
        }
        SkipTurn();
    }

    private void SkipTurn()
    {
        ResetTurnText();
        Gc.turnRoutine = null;
        Gc.SetPlayerTurn(playerNumber);
        TurnSkipNotify(Gc.players[Gc.playerTurnNumber].playerName);
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

    public void SkipPlayerTurn() { SkipTurn(); }

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

    private void ExposeCardNotify(string exposer, string nextExposer)
    {
        Gui.CallNotification(exposer + " have discarded a card!\r\nNow its " + nextExposer + " turn", resetText: false);
    }

    private void NotifyTurnPlayer() { Gui.CallNotification("In 5 Seconds or if someone else call Buzzer\r\n Your turn will be skiped!", resetText: false); }

    private void DiscardCallNotify() { Gui.CallNotification("Please discard a card!\r\nOtherwise a random card will be discarded!", resetText: false); }

    private void TurnSkipNotify(string name) { Gui.CallNotification("Turn skiped!\r\nNow its " + name + " turn", resetText: false); }
    #endregion
}
