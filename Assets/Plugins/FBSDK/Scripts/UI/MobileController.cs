using FbSdk.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FbSdk.UI
{
    internal class MobileController : MonoBehaviour, IController
    {
        private bool _isRequestingMobileValidate;
        private bool _isLoggingIn;
        private string _mobileNumberCache;

        [SerializeField] private InputField _mobile;
        [SerializeField] private InputField _validateCode;
        [SerializeField] private Button _requestValidateCodeButton;
        private Text _requestValidateCodeText;

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

            _mobileNumberCache = _mobile.text;
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
                });
            }
        }

        public void OnValidateCodeChange(string validateCode)
        {
            if (SdkManager.Auth.IsLoggingIn)
            {
                return;
            }

            if (validateCode.Length == 4)
            {
                SdkManager.Auth.Login(_mobileNumberCache, validateCode);
            }
        }

        public void OnQuickLoginClick()
        {
            if (SdkManager.Auth.IsLoggingIn)
            {
                return;
            }

            SdkManager.Auth.QuickLogin();
        }

        private void ResetValidateCodeButton()
        {
            _requestValidateCodeButton.interactable = true;
            _requestValidateCodeText.text = "获取验证码";
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
                if (_countdown != null) _countdown.UpdateTime();
            }
        }
    }
}