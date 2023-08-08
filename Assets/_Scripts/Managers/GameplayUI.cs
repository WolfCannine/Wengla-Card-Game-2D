using TMPro;
using UnityEngine;

public class GameplayUI : MonoBehaviour
{
    public static GameplayUI gUI;

    public GameObject shuffleButtonGO;
    public GameObject dealButtonGO;
    public TextMeshProUGUI notificationText;
    public GameObject notificationParentGO;

    private void Awake()
    {
        gUI = this;
        dealButtonGO.SetActive(false);
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

    private void ResetText()
    {
        notificationText.text = "";
    }
}
