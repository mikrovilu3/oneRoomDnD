using System;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public static SaveData CurrentSave { get; private set; } = new SaveData();

    private static string SavePath => Path.Combine(Application.persistentDataPath, "savegame.dat");

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadGame();
    }

    public void SaveGame()
    {
        try
        {
            using FileStream stream = new FileStream(SavePath, FileMode.Create);
            new BinaryFormatter().Serialize(stream, CurrentSave);
            Debug.Log($"[SaveManager] Saved to {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Save failed: {e.Message}");
        }
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            CurrentSave = new SaveData();
            return;
        }

        try
        {
            using FileStream stream = new FileStream(SavePath, FileMode.Open);
            CurrentSave = (SaveData)new BinaryFormatter().Deserialize(stream);
            Debug.Log("[SaveManager] Loaded successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Load failed: {e.Message}");
            CurrentSave = new SaveData();
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
        CurrentSave = new SaveData();
    }

    public static bool SaveExists() => File.Exists(SavePath);
}