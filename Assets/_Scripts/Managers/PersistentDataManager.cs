using Newtonsoft.Json;
using System.IO;
using UnityEngine;


public class PersistentDataManager : MonoBehaviour
{
    public static PersistentDataManager Instance { get; private set; }

    public GameData gameData;
    [SerializeField]
    private string filePath;
    [SerializeField]
    private bool resetData;


    private void Awake()
    {
        Instance = this;

        filePath = Application.persistentDataPath + "/custom_data.json";

        if (!resetData) { LoadData(); } else { ResetData(); }
    }

    private void OnApplicationQuit() { SaveData(); }

    public void SaveData()
    {
        string json = JsonConvert.SerializeObject(gameData);
        File.WriteAllText(filePath, json);
        Debug.Log("Data saved");
    }

    private void LoadData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            GameData gameDataFromJson = JsonConvert.DeserializeObject<GameData>(json);

            gameData.coins = gameDataFromJson.coins;
            gameData.userName = gameDataFromJson.userName;
            gameData.isSoundOn = gameDataFromJson.isSoundOn;
            gameData.isMusicOn = gameDataFromJson.isMusicOn;
            gameData.removeADs = gameDataFromJson.removeADs;
            gameData.emailAddress = gameDataFromJson.emailAddress;
            gameData.userPassword = gameDataFromJson.userPassword;
            gameData.unlockedLevel = gameDataFromJson.unlockedLevel;
            gameData.unlockedAllLevel = gameDataFromJson.unlockedAllLevel;

            Debug.Log("Data loaded");
        }
        else
        {
            Debug.Log("No data found");
        }
    }

    private void ResetData()
    {
        gameData.coins = 0;
        gameData.unlockedLevel = 0;
        gameData.isSoundOn = true;
        gameData.isMusicOn = true;
        gameData.removeADs = false;
        gameData.unlockedAllLevel = false;
        gameData.userName = string.Empty;
        gameData.emailAddress = string.Empty;
        gameData.userPassword = string.Empty;
        SaveData();
    }
}

//gameData.themeBuy.ToList().ForEach(t => t = false);
