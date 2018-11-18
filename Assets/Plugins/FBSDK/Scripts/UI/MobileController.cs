using FbSdk.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FbSdk.UI
{
    [RequireComponent(typeof(MobileValidateController))]
    internal class MobileController : MonoBehaviour, IController
    {
        private MobileValidateController _mobileValidate;
        
        private bool _isLoggingIn;

        public void Init()
        {
            _mobileValidate = GetComponent<MobileValidateController>();
        }

        public void OnValidateCodeChange(string validateCode)
        {
            if (SdkManager.Auth.IsLoggingIn)
            {
                return;
            }

            if (validateCode.Length == 4)
            {
                SdkManager.Auth.Login(_mobileValidate.MobileNumber, validateCode);
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
    }
}