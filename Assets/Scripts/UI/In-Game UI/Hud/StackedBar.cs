using UnityEngine;
using UnityEngine.UI;

public class StackedBar : MonoBehaviour
{
    [SerializeField] Image whiteFill;
    [SerializeField] Image overfill;

    // Call this with any value 0–200
    public void SetValue(float value)
    {
        value = Mathf.Clamp(value, 0f, 200f);

        // White bar always fills fully once value hits 100
        whiteFill.fillAmount = Mathf.Clamp01(value / 100f);

        // Overfill only starts after 100
        overfill.fillAmount = value > 100f ? (value - 100f) / 100f : 0f;
    }
}