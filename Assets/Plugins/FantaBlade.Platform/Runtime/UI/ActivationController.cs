using FantaBlade.Platform.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.Platform.UI
{
    internal class ActivationController : MonoBehaviour
    {
        [SerializeField] private InputField _activationCode;

        public void OnActivationClick()
        {
            if (SdkManager.Auth.IsLoggingIn)
            {
                return;
            }

            var activationCode = _activationCode.text;
            if (string.IsNullOrEmpty(activationCode))
            {
                SdkManager.Ui.Dialog.Show("Please input activation code", "ok");
                return;
            }

            SdkManager.Auth.Activation(activationCode, (succeed) =>
            {
                //
            });
        }

        public void OnBack()
        {
            SdkManager.Ui.ShowLogin();
            SdkManager.Ui.HideActivation();
        }
    }
}