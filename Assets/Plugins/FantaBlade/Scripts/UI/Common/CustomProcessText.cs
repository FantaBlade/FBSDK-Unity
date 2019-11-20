using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.UI.Common
{

    public class CustomProcessText : Text
    {

        public Text extText;
        private string realText;

        public override string text
        {
            get { return base.text; }

            set
            {
                realText = value;
                string[] strArrs = Regex.Split(realText, "\\[Space\\]");
                base.text = strArrs[0];
                extText.text = 2 <= strArrs.Length ? strArrs[1] : "";
            }
        }

    }
}
