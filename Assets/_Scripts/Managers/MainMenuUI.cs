using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public GameObject mainMenuPanel;


    public void StartGame()
    {
        SceneManager.LoadScene("Gameplay");
    }
}
