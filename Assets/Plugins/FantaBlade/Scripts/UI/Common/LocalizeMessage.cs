using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FantaBlade.Internal;

# if USE_TMPro
using TMPro;
#endif

public class LocalizeMessage : MonoBehaviour
{
    public Text Text;
#if USE_TMPro
    public TextMeshProUGUI TextMPro;
#endif

    private void Awake()
    {
        if(!Text)
        {
            Text = GetComponent<Text>();
        }
#if USE_TMPro
        if (!TextMPro)
        {
            TextMPro = GetComponent<TextMeshProUGUI>();
        }
#endif
        SetText(GetText());
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
            Text.text = SdkManager.Localize.GetText(text.ToLower());
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
