using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        ApplySavedSettings();
    }

    /// <summary>
    /// Call this when the player lands on a level scene.
    /// Pass in the active scene name, e.g. SceneManager.GetActiveScene().name
    /// </summary>
    public void SaveGame(string scene = null)
    {
        // Load existing data so we don't overwrite a further save
        PlayerData data = GetPlayerData() ?? new PlayerData();

        if (scene == null)
        {
            scene = SceneManager.GetActiveScene().name;
        }
        data.UpdateFurthest(scene);

        string json = JsonUtility.ToJson(data);
        SaveSystem.SaveData(json);
    }

    /// <summary>
    /// Returns the saved PlayerData, or null if no save file exists.
    /// </summary>
    public PlayerData GetPlayerData()
    {
        string json = SaveSystem.LoadData();
        if (json == null) return null;
        return JsonUtility.FromJson<PlayerData>(json);
    }

    public void LoadGame()
    {
        PlayerData data = GetPlayerData();
        if (data == null)
        {
            Debug.LogWarning("No save file found");
            return;
        }
        SceneManager.LoadScene(data.sceneName);
    }

    public void ApplySavedSettings()
    {
        // screen mode
        int savedMode = PlayerPrefs.GetInt("ScreenMode", 1);
        FullScreenMode mode = savedMode switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.FullScreenWindow,
            2 => FullScreenMode.Windowed,
            _ => FullScreenMode.FullScreenWindow
        };
        Screen.fullScreenMode = mode;

        // audio
        AudioManager.Instance.SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1f));
        AudioManager.Instance.SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
        AudioManager.Instance.SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
        AudioManager.Instance.SetUIVolume(PlayerPrefs.GetFloat("UIVolume", 1f));
    }
}