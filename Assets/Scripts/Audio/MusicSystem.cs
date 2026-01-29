using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MusicSystem
{
    private AudioSource musicSource;
    private MonoBehaviour coroutineHost;

    private Coroutine currentFade;

    public MusicSystem(AudioSource source, MonoBehaviour host)
    {
        musicSource = source;
        coroutineHost = host;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void Stop()
    {
        musicSource.Stop();
        musicSource.clip = null;
        musicSource.loop = false;
    }

    public void FadeIn(AudioClip clip, float duration = 1f)
    {
        if(clip == null) return;

        StopFade();
        musicSource.clip = clip;
        musicSource.volume = 0f;
        musicSource.loop = true;
        musicSource.Play();

        currentFade = coroutineHost.StartCoroutine(FadeVolume(1f, duration));
    }

    public void FadeOut(float duration = 1f)
    {
        if(!musicSource.isPlaying) return;

        StopFade();
        currentFade = coroutineHost.StartCoroutine(FadeVolume(0f, duration, stopAfter: true));
    }

    public void Change(AudioClip newClip)
    {
        if (newClip == null || musicSource.clip == newClip)
        {
            return;
        }

        musicSource.clip = newClip;
        musicSource.Play();
    }

    private void StopFade()
    {
        if(currentFade != null)
        {
            coroutineHost.StopCoroutine(currentFade);
            currentFade = null;
        }
    }

    private IEnumerator FadeVolume(float target, float duration, bool stopAfter = false)
    {
        float start = musicSource.volume;
        float time = 0f;

        while(time < duration)
        {
            time+= Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(start, target, time/duration);
            yield return null;
        }

        musicSource.volume = target;

        if(stopAfter)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
    }
}
