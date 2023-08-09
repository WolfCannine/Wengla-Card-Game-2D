using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    public static GameplayUI gUI;
    public GameObject startGamePanel;
    public GameObject shuffleButtonGO;
    public GameObject dealButtonGO;
    public GameObject finishButtonGO;
    public GameObject exposeButtonGO;
    public GameObject notificationParentGO;
    public TextMeshProUGUI notificationText;
    public Sprite selectedCardDefaultSprite;
    public Image selectedCardImage;

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

    public void CallNotification(string notification)
    {
        notificationText.text = notification;
        Invoke(nameof(ResetText), 3f);
    }

    public void ExposeCard()
    {
        GameController.gc.players[0].ExposeCard();
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
        notificationText.text = "";
    }
}
