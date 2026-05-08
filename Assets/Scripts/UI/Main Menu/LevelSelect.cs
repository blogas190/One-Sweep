using UnityEngine;

public class LevelSelect : MonoBehaviour
{
    public void LoadLevel(string levelNumber)
    {
        string levelName = "Level " + levelNumber;

        SceneChanger.instance.LoadScene(levelName);
    }
}
