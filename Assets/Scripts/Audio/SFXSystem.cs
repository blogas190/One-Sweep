using UnityEngine;
using System.Collections.Generic;

public class SFXSystem
{
    private AudioSource[] sfxPool;

    private Dictionary<AudioClip, AudioSource> activeLoopingSFX = new Dictionary<AudioClip, AudioSource>();

    public SFXSystem(AudioSource[] pool)
    {
        sfxPool = pool;

    }

    public void Play(AudioClip clip)
    {
        if (clip == null) 
        { 
            return; 
        }

        AudioSource source = GetFreeSFXSource();
        
        if (source == null) 
        {
            return;
        } 
        
        source.clip = clip;
        source.Play();
    }

    public void PlayLoop(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (activeLoopingSFX.ContainsKey(clip))
        {
            return;
        }

        AudioSource source = GetFreeSFXSource();

        if (source == null)
        {
            return;
        }

        source.clip = clip;
        source.loop = true;
        source.Play();

        activeLoopingSFX.Add(clip, source);
    }

    public void StopLoop(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (!activeLoopingSFX.TryGetValue(clip, out AudioSource source))
        {
            return;
        }

        source.Stop();
        source.clip = null;
        source.loop = false;

        activeLoopingSFX.Remove(clip);
    }

    public void StopAll()
    {
        foreach (var k in activeLoopingSFX)
        {
            AudioSource source = k.Value;
            source.Stop();
            source.clip = null;
            source.loop = false;
        }

        activeLoopingSFX.Clear();

        for (int i = 0; i < sfxPool.Length; i++)
        {
            sfxPool[i].Stop();
            sfxPool[i].clip = null;
            sfxPool[i].loop = false;
        }
    }

    private AudioSource GetFreeSFXSource()
    {
        for (int i = 0; i < sfxPool.Length; i++)
        {
            if(!sfxPool[i].isPlaying)
            {
                return sfxPool[i];
            }
        }

        return null;
    }
}
