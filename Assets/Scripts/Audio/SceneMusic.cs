using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [SerializeField] private AudioClip sceneMusic;
    [SerializeField] private bool useFade = true;
    [SerializeField] private float fadeDuration = 1f;

    private void Start()
    {
        if (useFade)
        {
            AudioManager.Instance.FadeToMusicIfDifferent(sceneMusic, fadeDuration);
        }
        else
        {
            AudioManager.Instance.PlayMusicIfDifferent(sceneMusic);
        }
    }
}
