using UnityEngine;
using System.Collections;
using TMPro;

namespace Dossamer.Dialogue
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextIterator : MonoBehaviour
    {
        TextMeshProUGUI _text;

        [SerializeField]
        float _secondsToWait = 0.02f; // 20 milliseconds

        IEnumerator _coroutine;

        [SerializeField]
        bool _shouldIterate = true;

        public delegate void TextDoneIterating();
        public TextDoneIterating OnTextDoneIterating;

        // Use this for initialization
        void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        public void TriggerNewText(string text)
        {
            StopAllCoroutines();
            if (_shouldIterate)
            {
                _coroutine = IterateLetters(text);
                StartCoroutine(_coroutine);
            }
            else
            {
                _text.text = text;
                _text.maxVisibleCharacters = int.MaxValue; // show all
            }
        }

        IEnumerator IterateLetters(string text)
        {
            _text.text = text;           // set full rich text immediately
            _text.maxVisibleCharacters = 0;
            _text.ForceMeshUpdate();     // ensure TMP parses the tags

            int totalVisible = _text.textInfo.characterCount; // actual visible chars (tags excluded)

            for (int i = 0; i <= totalVisible; i++)
            {
                _text.maxVisibleCharacters = i;

                float endTime = Time.time + _secondsToWait;
                while (Time.time < endTime)
                {
                    yield return null;
                }
            }

            _text.maxVisibleCharacters = totalVisible;
            OnTextDoneIterating?.Invoke();
        }
    }
}
