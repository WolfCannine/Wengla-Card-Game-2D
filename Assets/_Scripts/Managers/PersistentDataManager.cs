using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

public class PersistentDataManager : MonoBehaviour
{
    public static PersistentDataManager Instance { get; private set; }
    [SerializeField]
    private bool resetData;
    public GameData gameData;
    [SerializeField]
    private string filePath;

    private void Awake()
    {
        Instance = this;

        filePath = Application.persistentDataPath + "/custom_data.json";
        (!resetData ? (Action)LoadData : ResetData)();
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

            gameData.wengCoins = gameDataFromJson.wengCoins;

            Debug.Log("Data loaded");
        }
        else
        {
            Debug.Log("No data found");
        }
    }

    private void ResetData()
    {
        gameData.wengCoins = 0;
        SaveData();
    }
}
