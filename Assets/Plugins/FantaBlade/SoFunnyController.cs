using System.Collections;
using System.Collections.Generic;
using FantaBlade.Internal;
using UnityEngine;
#if SOFUNNY
using SoFunny;
using SoFunny.Utils;
#endif

namespace FantaBlade
{
    public class SoFunnyController
    {
        public static string birthday;
        public static string sex;

        // public Image userIconImage;
        // public Text displayNameText;
        // public Text statusMessageText;
        // public Text rawJsonText;
        // public VerticalLayoutGroup layoutGroup;
        private static bool isInitialized;

#if SOFUNNY
        public static void Init()
        {
            if (isInitialized)
                return;
            /// 初始化 SDK 
            //FunnySDK.InitializeSDK("900000000");
            FunnySDK.Initialize();
            isInitialized = true;
            /// SDK 事件监听
            FunnySDK.OnLogoutEvent += OnLogoutEvent;

            FunnySDK.OnLoginEvent += OnLoginEvent;

            FunnySDK.OnOpenUserCenterEvent += OnOpenUserCenterEvent;
            FunnySDK.OnCloseUserCenterEvent += OnCloseUserCenterEvent;

            FunnySDK.OnGuestDidBindEvent += OnGuestDidBindEvent;
            FunnySDK.OnSwitchAccountEvent += OnSwitchAccountEvent;

        }
        public static async void AuthPrivacyProfile() {
        try {
            var profile = await FunnySDK.AuthPrivacyProfile();
            Log.Debug("AuthPrivacyProfile");
            birthday = profile.UserBirthday;
            sex = profile.UserSex;
            // profile.AuthPlatform
             Log.Debug("已获取到授权信息");
             Log.Debug(birthday);
             Log.Debug(sex);
        }
        catch (PrivacyProfileCancelledException) {
             Log.Debug("用户已取消授权");
        }
        catch (PrivacyProfileDisableException) {
             Log.Debug("授权功能未开启");
        }
        catch (FunnySDKException ex) {
            Log.Debug($"发生错误 - {ex.Message} : {ex.Code}");
        }
        }


        private static void OnSwitchAccountEvent(AccessToken accessToken)
        {
            Log.Debug("切换到新账号了");
            AuthPrivacyProfile();
            Api.OnSwitchAccountSuccess("sofunny|"+accessToken.Value);
            // GetProfile();
        }

        private static void OnLogoutEvent()
        {
            Log.Debug("账号登出了");
            birthday = "";
            sex = "";
            Api.OnLogoutSuccess();
            // ResetProfile();
        }

        private static void OnGuestDidBindEvent(AccessToken token)
        {
            Log.Debug("当前游客用户已绑定至新账号");
        }

        private static void OnOpenUserCenterEvent()
        {
            Log.Debug("用户中心被打开了");
        }

        private static void OnCloseUserCenterEvent()
        {
            Log.Debug("用户中心被关闭了");
        }

        private static void OnLoginEvent(AccessToken token)
        {
            Log.Debug("账号已登陆");
            Log.Debug("call AuthPrivacyProfile");
            AuthPrivacyProfile();
            // GetProfile();
        }

//     private void Start() {
// #if UNITY_IOS
//         layoutGroup.padding.top = (int)(Screen.safeArea.y / 2.5);
// #else
//         layoutGroup.padding.top = 20;
// #endif
//     }

        public static async void Login()
        {
            try
            {
                AccessToken accessToken = await FunnySDK.Login();
                Api.OnLoginSuccess("sofunny|"+accessToken.Value);
            }
            catch (LoginCancelledException)
            {
                Log.Debug("登录被取消了");
            }
            catch (FunnySDKException error)
            {
                Log.Debug("登录出错：" + error.Message);
                FunnyUtils.ShowTipsAlert("提示", "登录出错");
                // rawJsonText.text = error.Message;
            }
        }

        public static void OpenUserCenter()
        {
            FunnySDK.OpenUserCenterUI();
        }

        public static string GetCurrentToken()
        {
            return FunnySDK.GetCurrentAccessToken().Value;
            // UpdateRawSection(accessToken);
        }

        public static async void Logout()
        {
            var success = await FunnySDK.Logout();
            if (success)
            {
                Api.OnLogoutSuccess();
            }
        }

        public static void CopyTokenValue()
        {
            var token = FunnySDK.GetCurrentAccessToken();
            if (token != null)
            {
                GUIUtility.systemCopyBuffer = token.Value;
                FunnyUtils.ShowTipsAlert("提示", "已复制到粘贴板");
            }
            else
            {
                FunnyUtils.ShowTipsAlert("提示", "未登录，无法复制");
            }
        }

        public static async void GetProfile()
        {
            try
            {
                var profile = await FunnySDK.GetProfile();
                // StartCoroutine(UpdateProfile(profile));
                // UpdateRawSection(profile);
            }
            catch (NotLoggedInException)
            {
                // FunnyUtils.ShowTipsAlert("", "未登陆账号");
            }
            catch (AccessTokenInvalidException)
            {
                // FunnyUtils.ShowTipsAlert("", "Token 已失效，请重新登陆");
            }
        }

        // public void GetIPAddress() {
        //     try {
        //         IPAddress[] iPs;
        //         iPs = Dns.GetHostAddresses(Dns.GetHostName());
        //         var local_ip_list = iPs.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        //                                .Select(ip => ip.ToString()).ToArray();
        //         rawJsonText.text = string.Join(" | ", local_ip_list);
        //     }
        //     catch (System.Exception) {
        //         rawJsonText.text = "获取 IP 失败";
        //     }
        // }
        //
        // public void CleanDatasView() {
        //     rawJsonText.text = "";
        // }

        // IEnumerator UpdateProfile(UserProfile profile) {
        //     if (profile.PictureUrl != null) {
        //         var www = UnityWebRequestTexture.GetTexture(profile.PictureUrl);
        //         yield return www.SendWebRequest();
        //         if (www.isDone && !www.isNetworkError) {
        //             var texture = DownloadHandlerTexture.GetContent(www);
        //             userIconImage.color = Color.white;
        //             userIconImage.sprite = Sprite.Create(
        //                 texture,
        //                 new Rect(0, 0, texture.width, texture.height),
        //                 new Vector2(0, 0));
        //         }
        //         else {
        //             Log.DebugError(www.error);
        //         }
        //         //switch (www.result) {
        //         //    case UnityWebRequest.Result.Success:
        //         //        var texture = DownloadHandlerTexture.GetContent(www);
        //         //        userIconImage.color = Color.white;
        //         //        userIconImage.sprite = Sprite.Create(
        //         //            texture,
        //         //            new Rect(0, 0, texture.width, texture.height),
        //         //            new Vector2(0, 0));
        //         //        break;
        //         //    default:
        //         //        Log.DebugError(www.error);
        //         //        break;
        //         //}
        //     } else {
        //         yield return null;
        //     }
        //
        //     displayNameText.text = profile.DisplayName;
        //     statusMessageText.text = profile.StatusMessage;
        //
        //     switch (profile.loginChannel) {
        //         case LoginType.SoFunny:
        //             displayNameText.text += " (真有趣)";
        //             break;
        //         case LoginType.Guest:
        //             displayNameText.text += " (游客)";
        //             break;
        //         case LoginType.Apple:
        //             displayNameText.text += " (Apple)";
        //             break;
        //         case LoginType.Facebook:
        //             displayNameText.text += " (Facebook)";
        //             break;
        //         case LoginType.Twitter:
        //             displayNameText.text += " (Twitter)";
        //             break;
        //         case LoginType.GooglePlay:
        //             displayNameText.text += " (GooglePlay)";
        //             break;
        //         case LoginType.QQ:
        //             displayNameText.text += " (QQ)";
        //             break;
        //         case LoginType.WeChat:
        //             displayNameText.text += " (WeChat)";
        //             break;
        //     }
        // }

        // void ResetProfile() {
        //     userIconImage.color = Color.gray;
        //     userIconImage.sprite = null;
        //     displayNameText.text = "Display Name";
        //     statusMessageText.text = "Status Message";
        //     rawJsonText.text = "";
        // }

        // void UpdateRawSection(object obj) {
        //     if (obj == null) {
        //         rawJsonText.text = "null";
        //         return;
        //     }
        //     
        //     var text = JsonUtility.ToJson(obj, true);
        //     if (text == null) {
        //         rawJsonText.text = "Invalid Object";
        //         return;
        //     }
        //     rawJsonText.text = text;
        //     var scrollContentTransform = (RectTransform)rawJsonText.gameObject.transform.parent;
        //     scrollContentTransform.localPosition = Vector3.zero;
        // }
#else
        public static void Init() {}
        public static async void Login(){}
        public static async void Logout(){}
        public static void OpenUserCenter(){}
        public static async void AuthPrivacyProfile(){}
        public static string GetCurrentToken() {return "";}
        public static void CopyTokenValue(){ }
        public static async void GetProfile(){}

#endif
    }
}