using FantaBlade.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.UI
{
    public class UserCenterController : MonoBehaviour
    {
        [SerializeField] private Text _username;
        [SerializeField] private GameObject _touristUpgradeBtn;
        [SerializeField] private GameObject _verifyBtn;

        private void OnEnable()
        {
            _touristUpgradeBtn.SetActive(SdkManager.Auth.IsTourist);
            _verifyBtn.SetActive(!SdkManager.Auth.IsTourist && !SdkManager.Auth.IsVerify);
            _username.text = SdkManager.Auth.Username;
        }

        public void OnHide()
        {
            SdkManager.Ui.HideGameCenter();
        }

        public void OnOpenProfile()
        {
            Application.OpenURL(PlatformApi.UserCenterHost + "auth?authToken=" + SdkManager.Auth.Token);
        }

        public void OnTouristUpgrade()
        {
        }

        public void OnSwitchUser()
        {
            var dialog = SdkManager.Ui.Dialog;
            if (SdkManager.Auth.IsTourist)
            {
                dialog.Show("Currently for the Tourist account，switching accounts may result in account loss，still have to switch?", "I want to", () =>
                {
                    SwitchUser();
                    dialog.Hide();
                }, "No!No!");
            }
            else
            {
                dialog.Show("Confirm switching account?", "confirm", () =>
                {
                    SwitchUser();
                    dialog.Hide();
                }, "cancel");
            }
        }

        private void SwitchUser()
        {
            SdkManager.Auth.Logout();
            SdkManager.Ui.HideGameCenter();
        }

        public void OnExitGame()
        {
            Api.ExitGame();
        }
    }
}