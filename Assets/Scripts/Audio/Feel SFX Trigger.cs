using UnityEngine;

public class FeelSFXTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip sfxClip;

    public void Play()
    {
        AudioManager.Instance.PlaySFX(sfxClip);
    }

    public void PlayLoop()
    {
        AudioManager.Instance.PlaySFXLoop(sfxClip);
    }

    public void StopLoop()
    {
        AudioManager.Instance.StopSFXLoop(sfxClip);
    }
}
