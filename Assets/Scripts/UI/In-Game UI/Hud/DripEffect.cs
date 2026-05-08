using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DripEffect : MonoBehaviour
{
    [SerializeField] TMP_Text percentageText;
    [SerializeField] RawImage dripImage;
    [SerializeField] float dripSpeed = 0.08f;
    [SerializeField] Material dripMaterial;

    [SerializeField] float offsetX = 0f;
    [SerializeField] float offsetY = 0f;
    [SerializeField] float widthOffset = 0f;

    RectTransform textRect;
    RectTransform dripRect;
    Material dripMat;
    float maxTextWidth;

    void Start()
    {
        textRect = percentageText.GetComponent<RectTransform>();
        dripRect = dripImage.GetComponent<RectTransform>();
        dripMat = new Material(dripMaterial);
        dripImage.material = dripMat;

        // measure max width at 100%
        percentageText.text = "100%";
        percentageText.ForceMeshUpdate();
        var lastChar = percentageText.textInfo.characterInfo[percentageText.textInfo.characterCount - 1];
        maxTextWidth = lastChar.bottomRight.x - percentageText.textInfo.characterInfo[0].bottomLeft.x;

        // restore original text
        percentageText.text = "0%";
    }

    void LateUpdate()
    {
        percentageText.ForceMeshUpdate();

        var textInfo = percentageText.textInfo;
        if (textInfo.characterCount == 0) return;

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (char.IsWhiteSpace(charInfo.character)) continue;

            minX = Mathf.Min(minX, charInfo.bottomLeft.x);
            maxX = Mathf.Max(maxX, charInfo.bottomRight.x);
            minY = Mathf.Min(minY, charInfo.bottomLeft.y);
        }

        // force include last character (% sign)
        var lastChar = textInfo.characterInfo[textInfo.characterCount - 1];
        maxX = lastChar.bottomRight.x;

        float textWidth = maxX - minX + widthOffset;
        float textHeight = percentageText.preferredHeight;
        float centerX = (minX + maxX) * 0.5f;

        dripRect.sizeDelta = new Vector2(textWidth, textHeight * 1.5f);
        dripRect.anchoredPosition = new Vector2(
            textRect.anchoredPosition.x + centerX + offsetX,
            textRect.anchoredPosition.y - (textHeight * 0.5f) - (dripRect.sizeDelta.y * 0.5f) + offsetY
        );

        // normalize width against max (100%) and drive shader
        float fill = Mathf.Clamp01(textWidth / maxTextWidth);
        dripMat.SetFloat("_FillAmount", fill);
    }
}