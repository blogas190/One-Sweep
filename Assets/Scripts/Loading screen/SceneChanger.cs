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
        }

        loadingScreenInstance.SetActive(true);

        FadeController fade = loadingScreenInstance.GetComponentInChildren<FadeController>();

        if(fade != null)
        {
            yield return fade.Fade(0f, 1f, 0.4f);
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while(op.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        op.allowSceneActivation = true;

        // Wait for scene activation to fully complete
        while (!op.isDone)
        {
            yield return null;
        }

        // Extra safety frame (covers Unity hitches)
        yield return null;

        // Fade out AFTER everything is ready
        if (fade != null)
        {
            yield return fade.Fade(1f, 0f, 0.4f);
        }

        loadingScreenInstance.SetActive(false);
        isLoading = false;
    }
}
