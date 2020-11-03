using FantaBlade.Internal;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FantaBlade.UI
{
    [RequireComponent(typeof(MobileValidateController))]
    internal class RegisterController : MonoBehaviour, IController
    {
        private static string _defaultCountryCode;

        [FormerlySerializedAs("_userLisense")] [SerializeField] private Window _userLicense;
        [SerializeField] private InputField _username;
        [SerializeField] private InputField _password;

        private MobileValidateController _mobileValidate;

        public void Init()
        {
            _mobileValidate = GetComponent<MobileValidateController>();
        }

        public void OnRequestRegister()
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
            
            // if(SdkManager.Instance.IsUserAcceptLisense())
            // {
            //     SdkManager.Auth.Register(username, password, countryCode, mobileNumber, validateCode);
            // }
            // else
            // {
            //     _userLicense.Appear();
            // }
            OnClickLisenseAccept();
        }

        public void OnClickLisenseReject()
        {
            _userLicense.Disappear();
        }

        public void OnClickLisenseAccept()
        {
            SdkManager.Instance.UserAcceptLisense();
//            _userLicense.Disappear();
            var username = _username.text;
            var password = _password.text;
            var countryCode = _mobileValidate.CountryCode;
            var mobileNumber = _mobileValidate.MobileNumber;
            var validateCode = _mobileValidate.ValidateCode;
            SdkManager.Auth.Register(username, password, countryCode, mobileNumber, validateCode);
        }
    }
}