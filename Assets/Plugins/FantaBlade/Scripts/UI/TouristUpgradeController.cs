using FantaBlade.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.UI
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
                SdkManager.Ui.Dialog.Show("请输入用户名", "好的");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                SdkManager.Ui.Dialog.Show("请输入密码", "好的");
                return;
            }

            if (string.IsNullOrEmpty(mobileNumber))
            {
                SdkManager.Ui.Dialog.Show("请输入手机号码，并获取验证码", "好的");
                return;
            }

            if (validateCode.Length != 4)
            {
                SdkManager.Ui.Dialog.Show("请正确输入验证码", "好的");
                return;
            }

            _window.Disappear();
            SdkManager.Auth.Register(username, password, countryCode, mobileNumber, validateCode);
        }
    }
}