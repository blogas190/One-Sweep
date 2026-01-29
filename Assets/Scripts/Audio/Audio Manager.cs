using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private SFXSystem sfx;
    private MusicSystem music;
    private UISystem ui;

    [Header("SFX Sources")]
    [SerializeField] private AudioSource[] sfxPool;

    [Header("Music Source")]
    [SerializeField] private AudioSource musicSource;

    [Header("UI Source")]
    [SerializeField] private AudioSource uiSource;

    [Header("UI Clips")]
    [SerializeField] private AudioClip uiHover;
    [SerializeField] private AudioClip uiClick;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfx = new SFXSystem(sfxPool);
        music = new MusicSystem(musicSource, this);
        ui = new UISystem(uiSource, uiHover, uiClick);
    }

    //--------------------------------------------
    //SFX Exposers
    //--------------------------------------------

    public void PlaySFX(AudioClip clip)
    {
        sfx.Play(clip);
    }

    public void PlaySFXLoop(AudioClip clip)
    {
        sfx.PlayLoop(clip);
    }

    public void StopSFXLoop(AudioClip clip)
    {
        sfx.StopLoop(clip);
    }

    public void StopAllSFX()
    {
        sfx.StopAll();
    }

    //--------------------------------------------
    //Music Exposers
    //--------------------------------------------

    public void PlayMusic(AudioClip clip)
    {
        music.Play(clip);
    }

    public void StopMusic()
    {
        music.Stop();
    }

    public void ChangeMusic(AudioClip newClip)
    {
        music.Change(newClip);
    }

    public void FadeInMusic(AudioClip clip, float duration = 1f)
    {
        music.FadeIn(clip, duration);
    }

    public void FadeOutMusic(float duration = 1f)
    {
        music.FadeOut(duration);
    }

    //--------------------------------------------
    //UI Exposers
    //--------------------------------------------

    public void PlayUIHover()
    {
        ui.Play(UISystem.UISound.Hover);
    }

    public void PlayUIClick()
    {
        ui.Play(UISystem.UISound.Click);
    }

    //--------------------------------------------
    //Volume Control (Mixer)
    //--------------------------------------------

    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("masterVolume", LinearToDecibel(value));
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("musicVolume", LinearToDecibel(value));
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("sfxVolume", LinearToDecibel(value));
    }

    public void SetUIVolume(float value)
    {
        audioMixer.SetFloat("uiVolume", LinearToDecibel(value));
    }

    private float LinearToDecibel(float value)
    {
        return Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
    }
}
