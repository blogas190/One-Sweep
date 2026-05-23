using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI screens")]
    public HudController hud;
    public PauseController pauseMenu;
    public LevelCompleteController levelComplete;
    public SettingsController settings;

    public RectTransform deathPanel;
    public float dropDuration = 0.6f;
    public float offscreenY;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        offscreenY = Screen.height * 0.6f + 800f;

        pauseMenu.Hide();
        levelComplete.Hide();
        settings.Hide();
        hud.Show();
        if (deathPanel == null)
        {
            deathPanel = GameObject.FindGameObjectWithTag("DeathPanel").GetComponent<RectTransform>();
        }
        deathPanel.anchoredPosition = new Vector2(0, offscreenY);
    }

    void Update()
    {
        if (GameManager.instance.currentState != GameState.playing)
        {
            hud.Hide();
        }
        else if (GameManager.instance.currentState == GameState.playing)
        {
            hud.Show();
        }
    }

    public void TogglePause()
    {
        if (GameManager.instance.currentState == GameState.paused)
        {
            if(settings.isSettingsOpen == true)
            {
                settings.Hide();
                settings.isSettingsOpen = false;
            }
            else
            {
                ResumeGame();
            }
        }
        else if (GameManager.instance.currentState == GameState.playing)
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenu.Show();
        hud.Hide();
    }

    public void ShowDeathScreen()
    {
        StartCoroutine(DropIn());
    }

    IEnumerator DropIn()
    {
        Vector2 startPos = new Vector2(0, offscreenY);
        Vector2 endPos = Vector2.zero;   // center of screen
        float elapsed = 0f;

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;

            // ease-out: decelerates as it lands
            t = 1f - Mathf.Pow(1f - t, 3f);

            deathPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        deathPanel.anchoredPosition = endPos;
    }

    public void ResumeGame()
    {
        pauseMenu.Hide();
        hud.Show();
        GameManager.instance.ResumeGame();
    }

    public void ShowLevelComplete()
    {
        hud.Hide();
        levelComplete.Show();
    }

    public void Settings()
    {
        pauseMenu.Hide();
        settings.Show();
        settings.isSettingsOpen = true;
    }

    public void SettingsBack()
    {
        settings.Hide();
        pauseMenu.Show();
    }
}
