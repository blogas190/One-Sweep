using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public PlayerMovement player;

    public static SaveManager instance;

    public void SaveGame()
    {
        PlayerData data = new PlayerData();
        data.sceneName = SceneManager.GetActiveScene().name;

        string json = JsonUtility.ToJson(data);
        SaveSystem.SaveData(json);
    }

    public void LoadGame()
    {
        string json = SaveSystem.LoadData();
        if(json == null)
        {
            Debug.LogWarning("No save file found");
            return;
        }

        PlayerData data = JsonUtility.FromJson<PlayerData>(json);

        SceneManager.LoadScene(data.sceneName);
    }

}
