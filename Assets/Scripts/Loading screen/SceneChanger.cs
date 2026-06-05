using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance;
    [SerializeField] private GameObject loadingScreenPrefab;
    private GameObject loadingScreenInstance;

    private bool isLoading = false;

    void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        if(isLoading) return;

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;

        if(loadingScreenInstance == null)
        {
            loadingScreenInstance = Instantiate(loadingScreenPrefab);
            DontDestroyOnLoad(loadingScreenInstance);
        }

        loadingScreenInstance.SetActive(true);

        if(Time.timeScale == 0)
        {
            Time.timeScale = 1;
        }

        FadeController fade = loadingScreenInstance.GetComponentInChildren<FadeController>();

        if(fade != null)
        {
            yield return fade.Fade(0f, 1f, 0.4f);
        }

        Debug.Log($"Starting to load scene: {sceneName}");
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        Debug.Log($"Async operation started for scene: {sceneName}");

        while(op.progress < 0.9f)
        {
            Debug.Log($"Scene loading progress: {op.progress}");
            yield return null;
        }

        Debug.Log($"Scene loading reached 90% for scene: {sceneName}");
        yield return new WaitForSeconds(0.3f);


        Debug.Log($"Allowing scene activation for scene: {sceneName}");
        op.allowSceneActivation = true;

        // Wait for scene activation to fully complete
        while (!op.isDone)
        {
            yield return null;
        }
        Debug.Log($"Scene loading completed for scene: {sceneName}");
        yield return null;

        OnSceneLoaded(sceneName);

        if (fade != null)
        {
            yield return fade.Fade(1f, 0f, 0.4f);
        }

        loadingScreenInstance.SetActive(false);
        isLoading = false;
        Destroy(loadingScreenInstance);
        loadingScreenInstance = null;
    }

    private void OnSceneLoaded(string sceneName)
    {
        if(GameManager.instance == null) return;

        switch(sceneName)
        {
            case "Main Menu":
                GameManager.instance.SetState(GameState.mainMenu);
                break;
        }

        AudioManager.Instance.StopAllSFX();
    }
}
