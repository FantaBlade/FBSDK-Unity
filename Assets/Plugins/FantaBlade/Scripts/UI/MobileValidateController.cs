using FantaBlade.Internal;
using FantaBlade.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.UI
{
    internal class MobileValidateController : MonoBehaviour, IController
    {
        private enum ValidateMode
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
            if (_mobileNumberCache.Length > 0)
            {
                if (Utils.IsClickTooOften(3f))
                {
                    return;
                }
                _isRequestingMobileValidate = true;
                SdkManager.Auth.RequestValidateCode(_countryCodeCache, _mobileNumberCache, err =>
                    {
                        _isRequestingMobileValidate = false;
                        if (err != null)
                        {
                            SdkManager.Ui.Dialog.Show(err, "ok");
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
                                    _requestValidateCodeText.text = s.ToString() + SdkManager.Localize.GetText("x_second_to_retry");
                                }
                            });
                        }
                    }, _validateMode == ValidateMode.None
                        ? null
                        : (bool?) (_validateMode == ValidateMode.Member));
            }
            else
            {
                SdkManager.Ui.Dialog.Show("please_input_mobile_number", "ok");
            }
        }

        private void ResetValidateCodeButton()
        {
            _requestValidateCodeButton.interactable = true;
            _requestValidateCodeText.text = SdkManager.Localize.GetText("get_verify_code");
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