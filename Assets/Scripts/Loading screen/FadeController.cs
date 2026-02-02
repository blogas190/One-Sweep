using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    [SerializeField] private Image fadeImage;

    public IEnumerator Fade(float from, float to, float duration)
    {
        Color c = fadeImage.color;
        float t = 0f;

        c.a = from;
        fadeImage.color = c;

        while(t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t/duration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage. color = c;
    }
}
