using FantaBlade.Internal;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace FantaBlade.UI
{
    internal class CancelAccountController : MonoBehaviour, IController
    {
        [SerializeField] private GameObject cancel_go;
        [SerializeField] private GameObject cancel_panel;
        [SerializeField] private GameObject cancel_confirm;
         [SerializeField] private InputField _name;
         [SerializeField] private InputField _idcard;
         [SerializeField] private InputField _mobile;
         [SerializeField] private InputField _code;
         [SerializeField] private InputField _confirm;

         private Window _window;
         
         public void Init()
         {
             _window = GetComponent<Window>();
             cancel_go.SetActive(true);
             cancel_panel.SetActive(false);
             cancel_confirm.SetActive(false);
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
        
        public void OnCancelAccountCallback(string err,
            PlatformApi.ResponseMetaInfo meta,
            PlatformApi.TokenResponse resp)
        {
            SdkManager.Ui.Dialog.HideLoading();

            if (err != null)
            {
                SdkManager.Ui.Dialog.Show(err, "ok");
            }
            else
            {
                cancel_go.SetActive(false);
                cancel_panel.SetActive(false);
                cancel_confirm.SetActive(true);
            }
        }

        public void CancelAccountGo()
        {
            var show_panel = false;
            if (SdkManager.Auth.Age > 0)
            {
                _name.gameObject.SetActive(true);
                _idcard.gameObject.SetActive(true);
                show_panel = true;
            }else
            {
                _name.gameObject.SetActive(false);
                _idcard.gameObject.SetActive(false);
            }

            if (PlayerPrefs.GetInt("loginChannel", 0) == 0 && !SdkManager.Auth.IsTourist)
            {
                _code.gameObject.transform.parent.gameObject.SetActive(true);
                _mobile.gameObject.transform.parent.gameObject.SetActive(true);
                show_panel = true;
            }else
            {
                _code.gameObject.transform.parent.gameObject.SetActive(false);
                _mobile.gameObject.transform.parent.gameObject.SetActive(false);
            }
            cancel_go.SetActive(false);
            cancel_panel.SetActive(show_panel);
            cancel_confirm.SetActive(!show_panel);
            if (!show_panel)
            {
                CancelAccount();
            }
        }

        public void CancelAccount()
        {
            if (SdkManager.Auth.IsLoggingIn)
            {
                return;
            }

            var name = _name.text;
            var idcard = _idcard.text;
            var mobile = _mobile.text;
            var code = _code.text;

            if (_name.gameObject.activeInHierarchy && string.IsNullOrEmpty(name))
            {
                SdkManager.Ui.Dialog.Show("please_input_your_name", "ok");
                return;
            }

            idcard = string.IsNullOrEmpty(idcard) ? "" : idcard.ToUpper();
//            string pattern = @"^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$|^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}([0-9]|X)$";
            string pattern = @"^[A-Za-z0-9]+$";
            if (_idcard.gameObject.activeInHierarchy && (string.IsNullOrEmpty(idcard) || !Regex.IsMatch(idcard,pattern)))
            {
                SdkManager.Ui.Dialog.Show("please_input_id_card", "ok");
                return;
            }
            if (_code.gameObject.activeInHierarchy && string.IsNullOrEmpty(code))
            {
                SdkManager.Ui.Dialog.Show("please_input_validate_code", "ok");
                return;
            }
            SdkManager.Auth.CancelAccount(name, idcard, mobile, code, OnCancelAccountCallback);
        }

        public void CancelAccountConfirm()
        {
            if (_confirm.text != SdkManager.Localize.GetText("confirm_cancel_account_content2"))
            {
                SdkManager.Ui.Dialog.Show("error_diff_text", "ok");
                return;
            }
            Hide();
            SdkManager.Ui.Dialog.Show("cancel_account_result", "ok");
            SdkManager.Auth.Logout();
            SdkManager.Ui.HideGameCenter();
        }
    }
}