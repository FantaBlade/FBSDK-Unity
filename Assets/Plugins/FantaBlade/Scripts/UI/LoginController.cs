using FantaBlade.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.UI
{
    internal class LoginController : MonoBehaviour
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
                SdkManager.Ui.Dialog.Show("用户名和密码不能为空！", "好的");
                return;
            }

            SdkManager.Auth.Login(identifier, password);
        }
    }
}