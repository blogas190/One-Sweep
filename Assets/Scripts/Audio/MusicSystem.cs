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

    public void PlayIfDifferent(AudioClip clip)
    {
        if (clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        StopFade();

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void FadeToIfDifferent(AudioClip clip, float duration = 1f)
    {
        if (clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        StopFade();
        currentFade = coroutineHost.StartCoroutine(FadeToClip(clip, duration));
    }

    private IEnumerator FadeToClip(AudioClip newClip, float duration)
    {
        float startVolume = musicSource.volume;

        // Fade out
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.Play();

        // Fade in
        t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, startVolume, t / duration);
            yield return null;
        }

        musicSource.volume = startVolume;
    }
}
