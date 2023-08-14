using FantaBlade.Platform.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.Platform.UI
{
    [RequireComponent(typeof(MobileValidateController))]
    internal class TouristUpgradeController : MonoBehaviour, IController
    {
        [SerializeField] private InputField _username;
        [SerializeField] private InputField _password;

        private Window _window;
        
        private MobileValidateController _mobileValidate;

        public void Init()
        {
            _window = GetComponent<Window>();
            _mobileValidate = GetComponent<MobileValidateController>();
        }

        public void Hide()
        {
            _window.gameObject.SetActive(false);
            SdkManager.Ui.HideGameCenter();
        }

        public void OnRequestUpgrade()
        {
            if (SdkManager.Auth.IsLoggingIn)
            {
                return;
            }

            var username = _username.text;
            var password = _password.text;
            var countryCode = _mobileValidate.CountryCode;
            var mobileNumber = _mobileValidate.MobileNumber;
            var validateCode = _mobileValidate.ValidateCode;

            if (string.IsNullOrEmpty(username))
            {
                SdkManager.Ui.Dialog.Show("please_input_username", "ok");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                SdkManager.Ui.Dialog.Show("please_input_password", "ok");
                return;
            }

            if (string.IsNullOrEmpty(mobileNumber))
            {
                SdkManager.Ui.Dialog.Show("error_mobile_number_empty", "ok");
                return;
            }

            if (validateCode.Length != 4)
            {
                SdkManager.Ui.Dialog.Show("error_validate_code_empty", "ok");
                return;
            }
            SdkManager.Auth.TouristUpgrade(username, password, countryCode, mobileNumber, validateCode, this.Hide);
        }
    }
}