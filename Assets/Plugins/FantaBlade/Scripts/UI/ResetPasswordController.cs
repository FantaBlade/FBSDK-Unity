using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using FantaBlade.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.UI
{
    public class ResetPasswordController : MonoBehaviour, IController
    {
        [SerializeField] private MainController _mainController;
        [SerializeField] private GameObject _step1;
        [SerializeField] private GameObject _step2;

        [SerializeField] private MobileValidateController _mobileValidateController;
        [SerializeField] private InputField _txtPW;
        [SerializeField] private InputField _txtPWConfirm;

        private string _tempTicket;

        public void Init()
        {
        }

        /// <summary>
        /// Reset Password 界面只要显示就肯定是从step1开始
        /// </summary>
        private void OnEnable()
        {
            _step1.SetActive(true);
            _step2.SetActive(false);
        }

        public void OnClickNext()
        {
            if(string.IsNullOrEmpty(_mobileValidateController.MobileNumber))
            {
                SdkManager.Ui.Dialog.Show("error_mobile_number_empty", "ok");
                return;
            }
            if(string.IsNullOrEmpty(_mobileValidateController.ValidateCode))
            {
                SdkManager.Ui.Dialog.Show("error_validate_code_empty", "ok");
                return;
            }
            SdkManager.Auth.CheckValidateCode(_mobileValidateController.CountryCode,
                _mobileValidateController.MobileNumber, _mobileValidateController.ValidateCode,
                (tempTicket) =>
                {
                    _tempTicket = tempTicket;
                    _step1.SetActive(false);
                    _step2.SetActive(true);
                });
        }

        public void OnClickConfirmReset()
        {
            if (string.IsNullOrEmpty(_txtPW.text) || string.IsNullOrEmpty(_txtPWConfirm.text))
            {
                SdkManager.Ui.Dialog.Show("please_input_password", "ok");
                return;
            }

            if (!_txtPW.text.Equals(_txtPWConfirm.text))
            {
                SdkManager.Ui.Dialog.Show("error_password_not_equal", "ok");
                return;
            }

            SdkManager.Auth.ResetPassword(_mobileValidateController.CountryCode, _mobileValidateController.MobileNumber,
                _txtPW.text, _tempTicket,
                () =>
                {
                    SdkManager.Ui.Dialog.Show("msg_reset_success", "ok");
                    _mainController.WindowBack();
                });
        }
    }
}