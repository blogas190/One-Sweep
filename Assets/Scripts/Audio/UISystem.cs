using UnityEngine;

public class UISystem
{
    private AudioSource uiSource;

    public enum UISound
    {
        Hover,
        Click
    }

    private AudioClip hoverClip;
    private AudioClip clickClip;

    public UISystem(AudioSource source, AudioClip hover, AudioClip click)
    {
        uiSource = source;
        hoverClip = hover;
        clickClip = click;
    }

    public void Play(UISound sound)
    {
        if (uiSource == null)
            return;

        switch (sound)
        {
            case UISound.Hover:
                if (hoverClip != null)
                    uiSource.PlayOneShot(hoverClip);
                break;

            case UISound.Click:
                if (clickClip != null)
                    uiSource.PlayOneShot(clickClip);
                break;
        }
    }
}
