using UnityEngine;
using UnityEngine.UI;

public class SettingsController : BaseMenu
{
    public SaveManager saveManager;

    [Header("Audio Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;

    public bool isSettingsOpen = false;

    public void SyncSliders()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }
        masterSlider.SetValueWithoutNotify(AudioManager.Instance.masterVolume);
        musicSlider.SetValueWithoutNotify(AudioManager.Instance.musicVolume);
        sfxSlider.SetValueWithoutNotify(AudioManager.Instance.sfxVolume);
        uiSlider.SetValueWithoutNotify(AudioManager.Instance.uiVolume);
    }

    public void OnMasterVolumeChanged()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }
        AudioManager.Instance.SetMasterVolume(masterSlider.value);
    }

    public void OnMusicVolumeChanged()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }
        AudioManager.Instance.SetMusicVolume(musicSlider.value);
    }

    public void OnSFXVolumeChanged()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }
        AudioManager.Instance.SetSFXVolume(sfxSlider.value);
    }

    public void OnUIVolumeChanged()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }
        AudioManager.Instance.SetUIVolume(uiSlider.value);
    }
    public void OnBack()
    {
        UIManager.instance.SettingsBack();
        PlayerPrefs.Save();
        //saveManager.SaveGame();
    }
}
