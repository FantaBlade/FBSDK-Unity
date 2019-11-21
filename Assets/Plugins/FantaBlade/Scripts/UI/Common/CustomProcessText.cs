using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FantaBlade.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.UI.Common
{

    public class CustomProcessText : Text
    {

        public Text extText;
        private string _realText;

        public override string text
        {
            get { return base.text; }

            set
            {
                _realText = value;
                string[] strArrs = Regex.Split(_realText, "\\[Space\\]");
                base.text = SdkManager.Localize.GetText(strArrs[0]);
                extText.text = 2 <= strArrs.Length ? strArrs[1] : "";
            }
        }

    }
}
