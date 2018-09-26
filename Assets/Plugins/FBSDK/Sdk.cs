using System;
using FbSdk.Internal;
using UnityEngine;

namespace FbSdk
{
    /// <summary>
    ///     Fantablade Platform SDK API
    /// </summary>
    public class Sdk
    {
        #region API

        public static bool IsInitialized { get; private set; }

        /// <summary>
        ///     初始化SDK
        /// </summary>
        /// <param name="accessKey">AccessKey</param>
        public static void Init(string accessKey)
        {
            SdkManager.Init(accessKey);
        }

        /// <summary>
        ///     登陆账号，获取token
        /// </summary>
        /// <param name="forceShowUi">强制显示UI，不使用缓存token</param>
        public static void Login(bool forceShowUi = false)
        {
            if (!IsInitialized) return;

            if (forceShowUi)
                SdkManager.Ui.ShowLogin();
            else
                SdkManager.Auth.LoginByCache();
        }

        /// <summary>
        ///     退出登陆，需要返回游戏首页重新调用<see cref="Login" />
        /// </summary>
        public static void Logout()
        {
            if (!IsInitialized) return;

            SdkManager.Auth.Logout();
        }

        /// <summary>
        ///     打开用户中心
        /// </summary>
        public static void OpenUserCenter()
        {
            if (!IsInitialized) return;

            SdkManager.Ui.ShowGameCenter();
        }

        /// <summary>
        ///     支付
        /// </summary>
        public static void Pay(string productId, string name, int price)
        {
            SdkManager.Order.Pay(productId, name, price);
        }

        /// <summary>
        ///     退出游戏
        /// </summary>
        public static void ExitGame()
        {
            var dialog = SdkManager.Ui.Dialog;
            dialog.Show("确定退出游戏嘛？", "再玩一会儿", dialog.Hide, "就此别过", Application.Quit);
        }

        #endregion

        #region Event invoker

        internal static void OnInitializeSuccess()
        {
            Debug.Log("OnInitialized");
            IsInitialized = true;
            var handler = InitializeSuccess;
            if (handler != null) handler();
        }        
        
        internal static void OnInitializeFailure(string err)
        {
            Debug.Log("OnInitializeFailure: " + err);
            var handler = InitializeFailure;
            if (handler != null) handler(err);
        }

        internal static void OnLoginSuccess(string token)
        {
            Debug.Log("OnLoginSuccess: " + token);
            var handler = LoginSuccess;
            if (handler != null) handler(token);
        }

        internal static void OnLogoutSuccess()
        {
            Debug.Log("OnLogoutSuccess");
            var handler = LogoutSuccess;
            if (handler != null) handler();
        }

        internal static void OnPaySuccess()
        {Debug.Log("OnPaySuccess");
            var handler = PaySuccess;
            if (handler != null) handler();
        }

        internal static void OnPayFailure(string err)
        {Debug.Log("OnPayFailure: " + err);
            var handler = PayFailure;
            if (handler != null) handler(err);
        }

        internal static void OnPayCancel()
        {Debug.Log("OnPayCancel");
            var handler = PayCancel;
            if (handler != null) handler();
        }

        #endregion

        #region Event

        /// <summary>
        ///     初始化完成
        /// </summary>
        public static event Action InitializeSuccess;

        /// <summary>
        ///     初始化失败
        /// </summary>
        public static event Action<string> InitializeFailure;

        /// <summary>
        ///     登陆成功
        /// </summary>
        public static event Action<string> LoginSuccess;

        /// <summary>
        ///     登陆失败
        /// </summary>
        /// <param name="msg">error message</param>
        public static event Action<string> LoginFailure;

        /// <summary>
        ///     登陆取消
        /// </summary>
        public static event Action LoginCancel;

        /// <summary>
        ///     登出成功
        /// </summary>
        public static event Action LogoutSuccess;

        /// <summary>
        ///     支付成功
        /// </summary>
        public static event Action PaySuccess;

        /// <summary>
        ///     支付失败
        /// </summary>
        /// <param name="msg">msg error message</param>
        public static event Action<string> PayFailure;

        /// <summary>
        ///     支付取消
        /// </summary>
        public static event Action PayCancel;

        #endregion
    }
}