using FbSdk.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FbSdk.UI
{
    public class TouristUpgradeController : MonoBehaviour, IController
    {
        [SerializeField] private InputField _username;
        [SerializeField] private InputField _password;
        [SerializeField] private InputField _mobileNumber;
        [SerializeField] private InputField _validateCode;
        [SerializeField] private Button _requestValidateCodeButton;
        private Text _requestValidateCodeText;

        private bool _isRequestingMobileValidate;
        private string _mobileNumberCache;

        private Countdown _countdown;

        public void Init()
        {
            _countdown = new Countdown(SdkManager.Instance);
            _requestValidateCodeText = _requestValidateCodeButton.GetComponentInChildren<Text>(true);
        }

        public void OnRequestMobileValidate()
        {
            if (_isRequestingMobileValidate)
            {
                return;
            }

            _mobileNumberCache = _mobileNumber.text;
            if (_mobileNumberCache.Length == 11)
            {
                _isRequestingMobileValidate = true;
                SdkManager.Auth.RequestValidateCode(_mobileNumberCache, err =>
                {
                    _isRequestingMobileValidate = false;
                    if (err != null)
                    {
                        SdkManager.Ui.Dialog.Show(err, "好的");
                    }
                    else
                    {
                        _validateCode.interactable = true;
                        _requestValidateCodeButton.interactable = false;
                        _countdown.Start(60, s =>
                        {
                            if (s == 0)
                            {
                                ResetValidateCodeButton();
                            }
                            else
                            {
                                _requestValidateCodeText.text = s.ToString();
                            }
                        });
                    }
                }, false);
            }
        }

        private void ResetValidateCodeButton()
        {
            _requestValidateCodeButton.interactable = true;
            _requestValidateCodeText.text = "获取验证码";
        }

        public void OnRequestUpgrade()
        {
            if (SdkManager.Auth.IsLoggingIn)
            {
                return;
            }

            var username = _username.text;
            var password = _password.text;
            var validateCode = _validateCode.text;

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

            if (string.IsNullOrEmpty(_mobileNumberCache))
            {
                SdkManager.Ui.Dialog.Show("请输入手机号码，并获取验证码", "好的");
                return;
            }

            if (validateCode.Length != 4)
            {
                SdkManager.Ui.Dialog.Show("请正确输入验证码", "好的");
                return;
            }

            SdkManager.Auth.TouristUpgrade(username, password, _mobileNumberCache, validateCode);
        }

        private void OnEnable()
        {
            _validateCode.interactable = false;
            _validateCode.text = null;
        }

        private void OnDisable()
        {
            ResetValidateCodeButton();
            _countdown.Reset();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                _countdown.UpdateTime();
            }
        }
    }
}