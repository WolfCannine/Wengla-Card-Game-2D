using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    public static GameplayUI gUI;
    public GameObject startGamePanel;
    public Image selectedCardImage;
    public GameObject dealButtonGO;
    public GameObject readyButtonGO;
    public GameObject finishButtonGO;
    public GameObject exposeButtonGO;
    public GameObject shuffleButtonGO;
    public GameObject notificationParentGO;
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

    public void CallNotification(string notification, float notificationDisableTime = 3, bool resetText = true)
    {
        notificationText.text = notification;
        if (resetText) { Invoke(nameof(ResetText), notificationDisableTime); }
    }

    public void AreYouReadyButton()
    {
        readyButtonGO.SetActive(false);
        Gc.players[0].ready = true;
        Gc.CheckIfAllPlayersReady();
    }

    public void ExposeCard()
    {
        if (TouchManager.tm.selectedCard == null && !Gc.players[0].haveBeginnerCard)
        {
            CallNotification("Plaese select a Card then press Expose Button!", 4);
        }
        else
        {
            Gc.players[0].ExposeCard();
        }
    }

    public void BuzzerButton()
    {
        Gc.players[0].BuzzerCall();
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

    private void ResetText()
    {
        //notificationText.text = "";
    }
}
