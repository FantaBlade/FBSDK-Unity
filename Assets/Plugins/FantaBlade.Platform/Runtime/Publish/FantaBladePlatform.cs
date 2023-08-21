using System;
using System.Collections.Generic;
using FantaBlade.Common;
using FantaBlade.Platform.Internal;
using UnityEngine;
using UnityEngine.Purchasing;

namespace FantaBlade.Platform
{
    /// <summary>
    ///     Fantablade Platform SDK API
    /// </summary>
    public static class FantaBladePlatform
    {
        private static readonly string DefinedChannel;
        
        static FantaBladePlatform()
        {
#if UNITY_IOS
            DefinedChannel = "AppStore";
#elif UNITY_ANDROID
            #if TAPTAP
                DefinedChannel = "TapTap";
            #elif GOOGLE_PLAY
                DefinedChannel = "GooglePlay";
            #else
                DefinedChannel = "Official";
            #endif
#else
            DefinedChannel = "Official";
#endif
        }
        
        // 第三方登陆
        public enum LoginChannel
        {
            CHANNEL_OFFICIAL = 0,
            CHANNEL_WECHAT = 1,
            CHANNEL_QQ = 2,
            CHANNEL_WEIBO = 3,
            CHANNEL_DOUYIN = 4,
            CHANNEL_ALIPAY = 5,
            CHANNEL_APPLE = 6,
            CHANNEL_GOOGLE = 7,
            CHANNEL_FACEBOOK = 8,
            CHANNEL_MOBILE = 9,
        }

        #region API

        public static string Channel
        {
            get { return DefinedChannel; }
        }

        private static bool _isInitialized;
        private static bool _isPaymentInitialized;

        /// <summary>
        /// 是否需要验证激活
        /// </summary>
        public static bool NeedActivation;

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

        public static bool IsVerify
        {
            get { return SdkManager.Auth.IsVerify; }
        }

        public static int Age
        {
            get { return SdkManager.Auth.Age; }
        }

        /// <param name="json">IAP 商品目录</param>
        public static void SetProductCatalog(string json)
        {
            SdkManager.Order.SetProductCatalog(json);
        }

        /// <summary>
        ///     初始化SDK
        /// </summary>
        /// <param name="accessKey">AccessKey</param>
        /// <param name="showFloatingWindow">显示悬浮窗</param>
        /// <param name="publishRegion">发行区域</param>
        public static void Init(string accessKey, bool showFloatingWindow = true,
            PublishRegion publishRegion = PublishRegion.China)
        {
// #if UNITY_IOS && !UNITY_EDITOR
            // Unity.Advertisement.IosSupport.ATTrackingStatusBinding.RequestAuthorizationTracking();
// #endif
            SdkManager.Init(accessKey, showFloatingWindow, publishRegion);
        }

        public static void EnableThirdChannel(LoginChannel[] loginChannels, Dictionary<LoginChannel, string> appIds = null)
        {
            for (int i = 0, max = loginChannels.Length; i < max; ++i)
            {
                string appId = "";
                string param = "";
                LoginChannel channel = loginChannels[i];
                switch (channel)
                {
                    case LoginChannel.CHANNEL_WECHAT:
                        appId = Config.WECHAT_APPID;
                        param = Config.WECHAT_UNIVERSAL_LINK;
                        break;
                    case LoginChannel.CHANNEL_QQ:
                        appId = Config.QQ_APPID;
                        param = Application.identifier + ".fileprovider";
                        break;
                    case LoginChannel.CHANNEL_WEIBO:
                        appId = Config.WEIBO_APPID;
#if UNITY_ANDROID
                        param = Config.WEIBO_REDIRECTURL;
#else
                        param = Config.WECHAT_UNIVERSAL_LINK;
#endif
                        break;
                    case LoginChannel.CHANNEL_DOUYIN:
                        appId = Config.DOUYIN_CLIENTKEY;
                        break;
                    case LoginChannel.CHANNEL_MOBILE:
                        appId = Config.MOBILE_SECRETINFO;
                        break;
                }

                if (appIds != null && appIds.TryGetValue(channel, out var customAppId))
                {
                    appId = customAppId;
                }

                RegisterChannel(channel, appId, param);
            }
        }

        public static void HideLoginChannel(LoginChannel loginChannel, bool enable)
        {
            SdkManager.Instance.HideLoginChannel(loginChannel, enable);
        }

        /// <summary>
        ///     登陆账号，获取token
        /// </summary>
        /// <param name="forceShowUi">强制显示UI，不使用缓存token</param>
        public static void Login(bool forceShowUi = false, bool useQuickLoginFirstTime = false)
        {
            Log.Debug("onlogin");
            if (!IsInitialized) return;

            if (forceShowUi)
            {
                SdkManager.Ui.ShowLogin();
            }
            else
            {
                SdkManager.Auth.LoginByCache(useQuickLoginFirstTime);
            }
        }

        public static bool IsInstalled(int loginChannel)
        {
            return SdkManager.NativeApi.IsInstall(loginChannel);
        }

        public static bool IsSupportAuth(LoginChannel loginChannel)
        {
            return SdkManager.NativeApi.IsSupportAuth((int)loginChannel);
        }

        public static void RegisterChannel(LoginChannel loginChannel, string appId, string weiboRedirectUrl = "")
        {
            SdkManager.NativeApi?.RegisterChannel((int)loginChannel, appId, weiboRedirectUrl);
        }

        public static void Feedback()
        {
            SdkManager.Ui.ShowNormalUI(NormalUIID.Feedback);
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

            if (SdkManager.Auth.Token == null)
            {
                Login(true);
            }
            else
            {
                SdkManager.Ui.ShowGameCenter();
            }
        }

        public static void OpenVerifyAge()
        {
            if (!IsInitialized) return;

            if (SdkManager.Auth.Token == null)
            {
                Login(true);
            }
            else
            {
                SdkManager.Ui.ShowNormalUI(NormalUIID.VerifyAge);
            }
        }

        public static void OpenUserLicense()
        {
            SdkManager.Instance.UserAcceptLisense();
            SdkManager.Ui.ShowNormalUI(NormalUIID.UserLicense);
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
        public static void Pay(string orderId)
        {
            Log.Debug(orderId);

            if (!_isPaymentInitialized) return;

            SdkManager.Order.Pay(orderId);
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

        internal static void OnSwitchAccountSuccess(string token)
        {
            Log.Info("OnSwitchAccountSuccess: " + token);
            var handler = SwitchAccountSuccess;
            if (handler != null) handler(token);
        }

        internal static void OnLoginSuccess(string token)
        {
            Log.Info("OnLoginSuccess: " + token);
            var handler = LoginSuccess;
            if (handler != null) handler(token);
        }

        internal static void OnLoginFailure(string msg)
        {
            Log.Info("OnLoginFailure: " + msg);
            var handler = LoginFailure;
            if (handler != null) handler(msg);
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
            Log.Debug("OnPayCancel");
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

        public static event Action<string> SwitchAccountSuccess;

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