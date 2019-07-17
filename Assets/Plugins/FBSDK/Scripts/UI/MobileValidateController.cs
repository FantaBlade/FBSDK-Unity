using FbSdk.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FbSdk.UI
{
    internal class MobileValidateController : MonoBehaviour, IController
    {
        enum ValidateMode
        {
            /// <summary>
            ///     不验证
            /// </summary>
            None,

            /// <summary>
            ///     必须是会员（号码注册过）
            /// </summary>
            Member,

            /// <summary>
            ///     必须是游客（号码没有注册过）
            /// </summary>
            Guest,
        }

        public string CountryCode
        {
            get { return _mobileController.CountryCode; }
        }

        public string MobileNumber
        {
            get { return _mobileNumberCache; }
        }

        public string ValidateCode
        {
            get { return _validateCode.text; }
        }

        [SerializeField] private ValidateMode _validateMode;
        [SerializeField] private MobileController _mobileController;
        [SerializeField] private InputField _validateCode;
        [SerializeField] private Button _requestValidateCodeButton;
        private Text _requestValidateCodeText;

        private bool _isRequestingMobileValidate;

        /// <summary>
        ///     缓存国别码，确保获取国别码与之前请求验证码的国别码一致
        /// </summary>
        private string _countryCodeCache; 
        
        /// <summary>
        ///     缓存手机号码，确保获取手机号与之前请求验证码的号码一致
        /// </summary>
        private string _mobileNumberCache;

        private Countdown _countdown;

        public void Init()
        {
            _countdown = new Countdown(SdkManager.Instance);
            _requestValidateCodeText = _requestValidateCodeButton.GetComponentInChildren<Text>(true);
            _requestValidateCodeButton.onClick.AddListener(OnRequestMobileValidate);
        }

        private void OnRequestMobileValidate()
        {
            if (_isRequestingMobileValidate)
            {
                return;
            }

            _countryCodeCache = _mobileController.CountryCode;
            _mobileNumberCache = _mobileController.MobileNumber;
            if (_mobileNumberCache.Length == 11)
            {
                _isRequestingMobileValidate = true;
                SdkManager.Auth.RequestValidateCode(_countryCodeCache, _mobileNumberCache, err =>
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
                    }, _validateMode == ValidateMode.None
                        ? null
                        : (bool?) (_validateMode == ValidateMode.Member));
            }
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
                _countdown.UpdateTime();
            }
        }
    }
}