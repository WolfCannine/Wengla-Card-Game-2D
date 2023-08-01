using TMPro;
using UnityEngine;

public class GameplayUI : MonoBehaviour
{
    public static GameplayUI gUI;

    public GameObject shuffleButtonGO;
    public GameObject dealButtonGO;
    public GameObject notificationTextPrefab;
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
        GameObject notificationGO = Instantiate(notificationTextPrefab, notificationParentGO.transform);
        notificationGO.GetComponent<TextMeshProUGUI>().text = notification;
        Destroy(notificationGO, 3f);
    }
}
