using FbSdk.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FbSdk.UI
{
    public class UserCenterController : MonoBehaviour
    {
        [SerializeField] private Text _username;
        [SerializeField] private GameObject _touristUpgradeBtn;

        private void OnEnable()
        {
            _touristUpgradeBtn.SetActive(SdkManager.Auth.IsTourist);
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
                dialog.Show("当前为游客账号，切换账号可能导致账号丢失，依然要切换吗？", "我偏要", () =>
                {
                    SwitchUser();
                    dialog.Hide();
                }, "不了不了");
            }
            else
            {
                dialog.Show("确认切换账号吗？", "确认", () =>
                {
                    SwitchUser();
                    dialog.Hide();
                }, "取消");
            }
        }

        private void SwitchUser()
        {
            SdkManager.Auth.Logout();
            SdkManager.Ui.HideGameCenter();
        }

        public void OnExitGame()
        {
            Sdk.ExitGame();
        }
    }
}