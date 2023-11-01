using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    public static GameplayUI gUI;
    public GameObject startGamePanel;
    public GameObject pauseGamePanel;
    public Image selectedCardImage;
    public GameObject dealButtonGO;
    public GameObject readyButtonGO;
    public GameObject finishButtonGO;
    public GameObject exposeButtonGO;
    public GameObject shuffleButtonGO;
    public GameObject skipTurnButtonGO;
    public GameObject notificationParentGO;
    public GameObject notificationPrefab;
    public GameObject notificationContentGO;
    public TextMeshProUGUI buzzerCountText;
    public TextMeshProUGUI notificationText;
    public Sprite selectedCardDefaultSprite;
    public ObjectPool objectPool;
    public float notificationDuration = 120f;
    private GameController Gc => GameController.gc;

    private void Awake()
    {
        gUI = this;
        dealButtonGO.SetActive(false);
    }

    public void StartGameButton()
    {
        startGamePanel.SetActive(false);
    }

    public void ActivateDealCardButton()
    {
        shuffleButtonGO.SetActive(false);
        dealButtonGO.SetActive(true);
    }

    public void CallNotification(in string notification, in float notificationDisableTime = 3, in bool resetText = true, in bool msg = true)
    {
        notificationText.text = notification;
        if (resetText) { Invoke(nameof(ResetText), notificationDisableTime); }
        if (msg) { CreateNotificationInContent(notification); }
    }

    public void AreYouReadyButton()
    {
        Gc.players[0].ready = true;
        Gc.CheckIfAllPlayersReady();
        readyButtonGO.SetActive(false);
    }

    public void ExposeCard()
    {
        if (TouchManager.tm.selectedCard == null && !Gc.players[0].haveBeginnerCard)
        {
            CallNotification("Plaese select a Card then press Expose Button!", msg: false);
            return;
        }
        Gc.players[0].ExposeCard();
    }

    public void BuzzerButton()
    {
        if (!Gc.allowBuzzer) { return; }
        SetSkipButton();
        CardController player = Gc.players[0];
        if (player.haveBuzzerOption)
        {
            player.haveBuzzerOption = false;
            player.BuzzerCall();
        }
    }

    public void SkipTurnButton()
    {
        Gc.players[0].SkipPlayerTurn();
        SetSkipButton();
    }

    public void SetSkipButton(bool value = false)
    {
        skipTurnButtonGO.SetActive(value);
    }

    public void RestartButton()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void SetSellectedCard(Sprite sprite = null)
    {
        if (sprite != null)
        {
            selectedCardImage.sprite = sprite;
            return;
        }
        selectedCardImage.sprite = selectedCardDefaultSprite;
    }

    public void PauseGame()
    {
        pauseGamePanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseGamePanel.SetActive(false);
    }

    public void ResetPlayerBuzzerCountText()
    {
        buzzerCountText.text = "1";
    }

    private void ResetText()
    {
        //notificationText.text = "";
    }

    private void CreateNotificationInContent(in string text)
    {
        GameObject obj = objectPool.GetPoolObject();
        obj.transform.SetParent(notificationContentGO.transform);
        obj.GetComponent<TextMeshProUGUI>().text = text;
        _ = StartCoroutine(ReturnNotificationAfterDelay(obj, notificationDuration));
    }

    private IEnumerator ReturnNotificationAfterDelay(GameObject obj, float delay)
    {
        yield return Helper.GetWait(delay);
        objectPool.ReturnPoolObject(obj);
    }
}
