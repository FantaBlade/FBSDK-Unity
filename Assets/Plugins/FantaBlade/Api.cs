using System;
using FantaBlade.Internal;
using UnityEngine;
using UnityEngine.Purchasing;

namespace FantaBlade
{
    /// <summary>
    ///     Fantablade Platform SDK API
    /// </summary>
    public static class Api
    {
        #region API

        private static bool _isInitialized;
        private static bool _isPaymentInitialized;

        /// <summary>
        ///     是否初始化完成
        /// </summary>
        public static bool IsInitialized
        {
            get
            {
                if (!_isInitialized)
                {
                    Log.Warning("FantaBlade SDK is not initialized");
                }

                return _isInitialized;
            }
            private set { _isInitialized = value; }
        }

        /// <summary>
        ///     支付模块是否初始化完成
        /// </summary>
        public static bool IsPaymentInitialized
        {
            get
            {
                if (!_isPaymentInitialized)
                {
                    Log.Warning("Payment module of FantaBlade SDK is not initialized");
                }

                return _isPaymentInitialized;
            }
            private set { _isPaymentInitialized = value; }
        }

        /// <summary>
        ///     初始化SDK
        /// </summary>
        /// <param name="accessKey">AccessKey</param>
        /// <param name="showFloatingWindow">显示悬浮窗</param>
        /// <param name="publishRegion">发行区域</param>
        /// <param name="productCatalogJson">IAP 商品目录</param>
        public static void Init(string accessKey, bool showFloatingWindow = true,
            PublishRegion publishRegion = PublishRegion.China, string productCatalogJson = null)
        {
            SdkManager.Init(accessKey, showFloatingWindow, publishRegion, productCatalogJson);
        }

        /// <summary>
        ///     登陆账号，获取token
        /// </summary>
        /// <param name="forceShowUi">强制显示UI，不使用缓存token</param>
        public static void Login(bool forceShowUi = false)
        {
            if (!IsInitialized) return;

            if (forceShowUi)
            {
                SdkManager.Ui.ShowLogin();
            }
            else
            {
                SdkManager.Auth.LoginByCache();
            }
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
        ///     设置sdk语言,非立即刷新,界面在打开(OnEnable)时进行刷新
        ///     sdk内有缓存, 默认 缓存/初始 语言为 Application.systemLanguage
        /// </summary>
        /// <param name="language"></param>
        public static void SetLanguage(SystemLanguage language)
        {
            SdkManager.UpdateLanguage(language);
        }

        /// <summary>
        ///     支付
        /// </summary>
        public static void Pay(string productId)
        {
            if (!_isPaymentInitialized) return;

            SdkManager.Order.Pay(productId);
        }

        /// <summary>
        ///     根据 ID 获取商品信息
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product</returns>
        public static Product GetProductById(string id)
        {
            if (!_isPaymentInitialized) return null;

            return SdkManager.PaymentApi.GetProductById(id);
        }

        /// <summary>
        /// 获取全部商品信息
        /// </summary>
        /// <returns></returns>
        public static Product[] GetProducts()
        {
            if (!_isPaymentInitialized) return null;

            return SdkManager.PaymentApi.GetProducts();
        }

        /// <summary>
        ///     退出游戏
        /// </summary>
        public static void ExitGame()
        {
            if (!IsInitialized) return;

            var dialog = SdkManager.Ui.Dialog;
            dialog.Show("Are you sure to quit the game? ", "Yes", Application.Quit, "Not now.", dialog.Hide);
        }

        #endregion

        #region Event invoker

        internal static void OnInitializeSuccess()
        {
            Log.Info("OnInitializeSuccess");
            IsInitialized = true;
            var handler = InitializeSuccess;
            if (handler != null) handler();
        }

        internal static void OnInitializeFailure(string err)
        {
            Log.Info("OnInitializeFailure: " + err);
            var handler = InitializeFailure;
            if (handler != null) handler(err);
        }

        internal static void OnPaymentInitializeSuccess()
        {
            Log.Info("OnPaymentInitializeSuccess");
            IsPaymentInitialized = true;
            var handler = PaymentInitializeSuccess;
            if (handler != null) handler();
        }

        internal static void OnPaymentInitializeFailure(string err)
        {
            Log.Info("OnPaymentInitializeFailure: " + err);
            var handler = PaymentInitializeFailure;
            if (handler != null) handler(err);
        }

        internal static void OnLoginSuccess(string token)
        {
            Log.Info("OnLoginSuccess: " + token);
            var handler = LoginSuccess;
            if (handler != null) handler(token);
        }

        internal static void OnLoginCancel()
        {
            Log.Info("OnLoginCancel!");
            var handler = LoginCancel;
            if (handler != null) handler();
        }

        internal static void OnLogoutSuccess()
        {
            Log.Info("OnLogoutSuccess");
            var handler = LogoutSuccess;
            if (handler != null) handler();
        }

        internal static void OnPaySuccess()
        {
            Log.Info("OnPaySuccess");
            var handler = PaySuccess;
            if (handler != null) handler();
        }

        internal static void OnPayFailure(string err)
        {
            Log.Info("OnPayFailure: " + err);
            var handler = PayFailure;
            if (handler != null) handler(err);
        }

        internal static void OnPayCancel()
        {
            Log.Info("OnPayCancel");
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
        ///     支付模块初始化完成
        /// </summary>
        public static event Action PaymentInitializeSuccess;

        /// <summary>
        ///     支付模块初始化失败
        /// </summary>
        public static event Action<string> PaymentInitializeFailure;
        
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