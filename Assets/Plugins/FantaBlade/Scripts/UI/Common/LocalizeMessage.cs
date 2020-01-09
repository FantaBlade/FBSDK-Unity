using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FantaBlade.Internal;

# if USE_TMPro
using TMPro;
#endif
namespace FantaBlade.UI.Common
{
    public class LocalizeMessage : MonoBehaviour
    {
        public Text Text;
#if USE_TMPro
    public TextMeshProUGUI TextMPro;
#endif
        private string _originalText;

        private void Awake()
        {
            if (!Text)
            {
                Text = GetComponent<Text>();
            }
#if USE_TMPro
        if (!TextMPro)
        {
            TextMPro = GetComponent<TextMeshProUGUI>();
        }
#endif
            _originalText = GetText();
            SetText(_originalText);
        }

        private void OnEnable()
        {
            SetText(_originalText);
        }

        public void SetText(string text)
        {
            SetTextInternal(text);
        }

        public string GetText()
        {
            return GetTextInternal();
        }

        private void SetTextInternal(string text)
        {
            if (Text)
            {
                Text.text = SdkManager.Localize.GetText(text);
            }
#if USE_TMPro
        if (TextMPro)
        {
            TextMPro.text = Boot.Localize.GetText(text.ToLower());
        }
#endif
        }

        private string GetTextInternal()
        {
            if (Text)
            {
                return Text.text;
            }
#if USE_TMPro
        if (TextMPro)
        {
            return TextMPro.text;
        }
#endif
            return null;
        }
    }
}
