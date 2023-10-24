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
    public GameObject notificationParentGO;
    public GameObject notificationPrefab;
    public GameObject notificationContentGO;
    public TextMeshProUGUI buzzerCountText;
    public TextMeshProUGUI notificationText;
    public Sprite selectedCardDefaultSprite;
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
        }
        else
        {
            Gc.players[0].ExposeCard();
        }
    }

    public void BuzzerButton()
    {
        CardController player = Gc.players[0];
        if (player.haveBuzzerOption)
        {
            player.haveBuzzerOption = false;
            player.BuzzerCall();
        }
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
        }
        else
        {
            selectedCardImage.sprite = selectedCardDefaultSprite;
        }
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

    private void ResetText()
    {
        //notificationText.text = "";
    }

    private void CreateNotificationInContent(in string text)
    {
        GameObject obj = Instantiate(notificationPrefab, notificationContentGO.transform);
        obj.transform.SetParent(notificationContentGO.transform);
        obj.GetComponent<TextMeshProUGUI>().text = text;
        Destroy(obj, 120f);
    }
}
