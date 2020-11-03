using FantaBlade.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.UI
{
    internal class LoginController : MonoBehaviour, IController
    {
        [SerializeField] private InputField _identifier;
        [SerializeField] private InputField _password;

        public void OnLoginClick()
        {
            if (SdkManager.Auth.IsLoggingIn)
            {
                return;
            }

            var identifier = _identifier.text;
            var password = _password.text;
            if (string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(password))
            {
                SdkManager.Ui.Dialog.Show("Please input username and password！", "ok");
                return;
            }

            SaveCache();
            SdkManager.Auth.Login(identifier, password);
        }

        public void Init()
        {
            if (!string.IsNullOrEmpty(SdkManager.CacheAccountId))
            {
                _identifier.text = SdkManager.CacheAccountId;
            }
        }

        public void SaveCache()
        {
            SdkManager.CacheAccountId = _identifier.text;
        }
    }
}