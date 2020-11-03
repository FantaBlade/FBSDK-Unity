using FantaBlade.Internal;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace FantaBlade.UI
{
    internal class VerifyAgeController : MonoBehaviour, IController
    {
        [SerializeField] private InputField _name;
        [SerializeField] private InputField _idcard;

        private Window _window;
        
        public void Init()
        {
            _window = GetComponent<Window>();
        }

        public void Hide()
        {
            _window.gameObject.SetActive(false);
            SdkManager.Ui.ShowGameCenter();
        }
        
        public void HideGameCenter()
        {
            _window.gameObject.SetActive(false);
            SdkManager.Ui.HideGameCenter();
        }

        public void OnRequestVerify()
        {
            if (SdkManager.Auth.IsLoggingIn)
            {
                return;
            }

            var name = _name.text;
            var idcard = _idcard.text;

            if (string.IsNullOrEmpty(name))
            {
                SdkManager.Ui.Dialog.Show("please_input_your_name", "ok");
                return;
            }

            idcard = string.IsNullOrEmpty(idcard) ? "" : idcard.ToUpper();
//            string pattern = @"^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$|^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}([0-9]|X)$";
            string pattern = @"^[A-Za-z0-9]+$";
            if (string.IsNullOrEmpty(idcard) || !Regex.IsMatch(idcard,pattern))
            {
                SdkManager.Ui.Dialog.Show("please_input_id_card", "ok");
                return;
            }
            SdkManager.Auth.VerifyAge(name, idcard, HideGameCenter);
        }
    }
}