using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public string nextScene;
    public GameObject viewMediaLogoPanel;
    public GameObject wenglaLogoPanel;
    public GameObject loginPanel;

    private void Start()
    {
        nextScene = GameManager.Instance.nextScene;
        if (string.IsNullOrEmpty(nextScene)) { nextScene = "Main Menu"; }
        _ = StartCoroutine(Loading());
    }

    private IEnumerator Loading()
    {
        AsyncOperation scene = SceneManager.LoadSceneAsync(nextScene);
        scene.allowSceneActivation = false;
        yield return new WaitForSecondsRealtime(2f);
        wenglaLogoPanel.SetActive(true);
        viewMediaLogoPanel.SetActive(false);
        yield return new WaitForSecondsRealtime(2f);
        scene.allowSceneActivation = true;
    }
}
