using System.Collections;
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
            return;
        }

        if (SaveSystem.LoadData() == null)
        {
            PlayerData defaultData = new PlayerData();
            defaultData.furthestStage = 1;
            defaultData.furthestLevel = 1;
            defaultData.sceneName = "Level 1-1";
            SaveSystem.SaveData(JsonUtility.ToJson(defaultData));
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplySavedSettings();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySavedSettings();
    }

    public void SaveGame(string scene = null)
    {
        PlayerData data = GetPlayerData() ?? new PlayerData();
        if (scene == null)
        {
            scene = SceneManager.GetActiveScene().name;
        }
        data.UpdateFurthest(scene);
        string json = JsonUtility.ToJson(data);
        SaveSystem.SaveData(json);
    }

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
        int savedMode = PlayerPrefs.GetInt("ScreenMode", 1);
        FullScreenMode fsMode = savedMode switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.FullScreenWindow,
            2 => FullScreenMode.Windowed,
            _ => FullScreenMode.FullScreenWindow
        };
        Screen.fullScreenMode = fsMode;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(PlayerPrefs.GetFloat("masterVolume", 1f));
            AudioManager.Instance.SetMusicVolume(PlayerPrefs.GetFloat("musicVolume", 1f));
            AudioManager.Instance.SetSFXVolume(PlayerPrefs.GetFloat("sfxVolume", 1f));
            AudioManager.Instance.SetUIVolume(PlayerPrefs.GetFloat("uiVolume", 1f));
        }

        StartCoroutine(LogScreenModeNextFrame());
    }

    private IEnumerator LogScreenModeNextFrame()
    {
        yield return null; // wait one frame
        Debug.Log($"Screen mode after apply: {Screen.fullScreenMode}");
    }
}