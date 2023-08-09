using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int gameLevel;
    public int gameMode;
    public string nextScene;
    public Mode mode;

    private void Awake() { Instance = this; }
}
