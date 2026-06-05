using Michsky.UI.Reach;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to the level select screen.
///
/// Setup in Inspector:
/// - Create one LevelStageTab entry per stage tab in your menu.
/// - Each entry holds the level button GameObjects in order (index 0 = level 1, etc.).
/// - Locked levels have their GameObject disabled entirely.
/// </summary>
public class LevelSelectUI : MonoBehaviour
{
    [System.Serializable]
    public class LevelStageTab
    {
        public int stageNumber;          // Which stage this tab represents (1, 2, 3...)
        public GameObject[] levelButtons; // GameObjects in order: index 0 = level 1, index 1 = level 2, etc.
    }

    [Header("Stage Tabs")]
    public LevelStageTab[] stageTabs;
    public PanelManager panelManager;

    void Start()
    {
        RefreshLevelSelect();
        panelManager.InitializePanels();
    }


    public void RefreshLevelSelect()
    {
        PlayerData data = null;

        if (SaveManager.instance != null)
            data = SaveManager.instance.GetPlayerData();

        foreach (LevelStageTab tab in stageTabs)
        {
            for (int i = 0; i < tab.levelButtons.Length; i++)
            {
                int levelNumber = i + 1; // index 0 = level 1
                GameObject btn = tab.levelButtons[i];

                if (btn == null) continue;

                bool unlocked = data != null && data.IsUnlocked(tab.stageNumber, levelNumber);
                btn.SetActive(unlocked);
            }
        }
    }

    public void LoadLevel(int stage, int level)
    {
        string sceneName = $"Level {stage}-{level}";
        SceneManager.LoadScene(sceneName);
    }
}