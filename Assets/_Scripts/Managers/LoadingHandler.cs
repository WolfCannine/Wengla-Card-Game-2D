using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LoadingHandler : MonoBehaviour
{
    public string nextScene;
    public Slider slider;


    private void Start()
    {
        slider.value = 0;
        nextScene = GameManager.Instance.nextScene;
        if (string.IsNullOrEmpty(nextScene)) { nextScene = "Main Menu"; }
        //_ = slider.DOValue(1, 4);
        _ = StartCoroutine(Loading());
    }

    private IEnumerator Loading()
    {
        AsyncOperation scene = SceneManager.LoadSceneAsync(nextScene);
        scene.allowSceneActivation = false;
        yield return new WaitForSecondsRealtime(5f);
        scene.allowSceneActivation = true;
    }
}
