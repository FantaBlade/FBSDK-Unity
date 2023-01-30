using System;
using FantaBlade.Internal;
using UnityEngine;
using UnityEngine.Purchasing;
using quicksdk;

namespace FantaBlade
{
    /// <summary>
    ///     Fantablade Platform SDK API
    /// </summary>
    public static class Api
    {
        public class UserInfo
        {
            public string uid;
            public string token;
            public int age;
            public bool realName;
            public bool switchAccount;
            public bool resumeGame;
            public string msg;
            public FuncType FunctionType;
        }

        public class OrderInfo
        {
            public string id;
            public string name;
            public double price;
            public int count;
            public string orderId;
            public string callbackUrl;
            public string extra;
        }
        
                
        // 第三方登陆
        public static class LoginChannel {
            public const int CHANNEL_OFFICIAL = 0;
            public const int CHANNEL_WECHAT = 1;
            public const int CHANNEL_QQ = 2;
            public const int CHANNEL_WEIBO = 3;
            public const int CHANNEL_DOUYIN = 4;
            public const int CHANNEL_ALIPAY = 5;
            public const int CHANNEL_APPLE = 6;
            public const int CHANNEL_GOOGLE = 7;
            public const int CHANNEL_FACEBOOK = 8;
            public const int CHANNEL_MOBILE = 9;
        }

        // 第三方分享
        public static class ShareChannel {
            public const int WECHAT_SESSION = 1;
            public const int WECHAT_TIMELINE = 2;
            public const int WECHAT_FAVORITE = 3;
            public const int QQ_SESSION = 4;
            public const int WEIBO = 5;
        }
        
        #region API

#if SOFUNNY
        public static readonly string Channel = "SoFunny";
#elif UNITY_IOS
    public static readonly string Channel = "App Store";
#elif UNITY_ANDROID
#if TAPTAP
    public static readonly string Channel = "TapTap";
#elif TEST1
    public static readonly string Channel = "Test1";
#elif TEST2
    public static readonly string Channel = "Test2";
#elif TEST3
    public static readonly string Channel = "Test3";
#elif QUICK
    public static readonly string Channel = "Quick";
#elif DOUYIN
    public static readonly string Channel = "Douyin";
#elif OFFICIALSEA
    public static readonly string Channel = "Google Play";
#else
        public static readonly string Channel = "Official";
#endif
#else
    public static readonly string Channel = "Unknown";
#endif
        public static int GetApiVersion()
        {
            return 1;
        }

        public static string GetChannel()
        {
            if (Channel.Equals("Quick"))
            {
                return QuickSDK.getInstance().channelName();
            }
#if UNITY_ANDROID
#if DOUYIN
            try {
                AndroidJavaObject context = new AndroidJavaClass ("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject> ("currentActivity"); //获得Context
                using (var actClass = new AndroidJavaClass("com.bytedance.hume.readapk.HumeSDK")) {
                    String channel = actClass.CallStatic<String>("getChannel",context);
                    if (!string.IsNullOrEmpty(channel))
                        return channel;
                }
            }
            catch (Exception e)
            {
                Log.Warning("GetChannel " + e.ToString());
            }
#elif !UNITY_EDITOR
            try {
                AndroidJavaObject context = new AndroidJavaClass ("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject> ("currentActivity"); //获得Context
                using (var actClass = new AndroidJavaClass("com.mcxiaoke.packer.helper.PackerNg")) {
                    String channel = actClass.CallStatic<String>("getChannel",context);
                    if (!string.IsNullOrEmpty(channel))
                        return channel;
                }
            }
            catch (Exception e)
            {
                Log.Warning("GetChannel " + e.ToString());
                return Channel;
            }
#endif
#endif
            return Channel;
        }
        public static int GetChannelType()
        {
            if (Channel.Equals("Quick"))
            {
                return QuickSDK.getInstance().channelType();
            }
            return 0;
        }
        private static bool _isInitialized;
        private static bool _isPaymentInitialized;
        public static GameRoleInfo gameRoleInfo = new GameRoleInfo();
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
        
        public static bool IsVerify()
        {
            if (Channel.Equals("Quick"))
            {
                var userInfo = EventHandle.Instance._UserInfo;
                return userInfo.realName;
            }
            if (Channel.Equals("SoFunny"))
            {
                return true;//上海外不触发实名制
            }
            return SdkManager.Auth.IsVerify;
        }
        
        public static int Age()
        {
            if (Channel.Equals("Quick"))
            {
                var userInfo = EventHandle.Instance._UserInfo;
                return userInfo.age;
            }
            if (Channel.Equals("SoFunny"))
            {
                return 18;//上海外不触发实名制
            }
            return SdkManager.Auth.Age;
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
            if (Channel.Equals("SoFunny"))
            {
                SoFunnyController.Init();
            }
        }

        public static void EnableThirdChannel(int[] loginChannels, string[] appIds)
        {
            for (int i = 0, max = loginChannels.Length; i < max; ++i)
            {
                string appId = "";
                string weiboUrl = "";
                int channel = loginChannels[i];
                switch (channel)
                {
                    case LoginChannel.CHANNEL_WECHAT:
                        appId = FantaBlade.Config.WECHAT_APPID;
                        weiboUrl = FantaBlade.Config.WECHAT_UNIVERSAL_LINK;
                        break;
                    case LoginChannel.CHANNEL_QQ:
                        appId = FantaBlade.Config.QQ_APPID;
                        weiboUrl = Application.identifier+".fileprovider";
                        break;
                    case LoginChannel.CHANNEL_WEIBO:
                        appId = FantaBlade.Config.WEIBO_APPID;
#if UNITY_ANDROID
                        weiboUrl = FantaBlade.Config.WEIBO_REDIRECTURL;
#else
                        weiboUrl = FantaBlade.Config.WECHAT_UNIVERSAL_LINK;
#endif
                        break;
                    case LoginChannel.CHANNEL_DOUYIN:
                        appId = FantaBlade.Config.DOUYIN_CLIENTKEY;
                        break;
                    case LoginChannel.CHANNEL_MOBILE:
                        appId = FantaBlade.Config.MOBILE_SECRETINFO;
                        break;
                }
                appId = string.IsNullOrEmpty(appIds[i]) ? appId : appIds[i];
                RegisterChannel(channel, appId, weiboUrl);
            }
        }

        public static void HideLoginChannel(int loginChannel, bool enable)
        {
            SdkManager.Instance.HideLoginChannel(loginChannel, enable);
        }

        public static void OnStop()
        {
            if (Channel.Equals("Quick") && EventHandle.Instance)
            {
                EventHandle.Instance.onPauseGame();
            }
        }
        
        public static void OnResume()
        {
            if (Channel.Equals("Quick") && EventHandle.Instance)
            {
                EventHandle.Instance.onResumeGame();
            }
        }

        /// <summary>
        ///     登陆账号，获取token
        /// </summary>
        /// <param name="forceShowUi">强制显示UI，不使用缓存token</param>
        public static void Login(bool forceShowUi = false, bool useQuickLoginFirstTime = false)
        {
            Log.Debug("onlogin");
            if (!IsInitialized) return;
            if (Channel.Equals("Quick"))
            {
                QuickSDK.getInstance().login();
                return;
            }

            if (Channel.Equals("SoFunny"))
            {
                SoFunnyController.Login();
                return;
            }

            if (forceShowUi)
            {
                SdkManager.Ui.ShowLogin();
            }
            else
            {
                SdkManager.Auth.LoginByCache(useQuickLoginFirstTime);
            }
        }
        
        public static void Share(int shareChannel, string imagePath, string title, string desc)
        {
            SdkManager.Share.ShareImage(shareChannel, imagePath, title, desc);
        }

        public static bool IsInstalled(int loginChannel)
        {
            return SdkManager.NativeApi.IsInstall(loginChannel);
        }
        
        public static bool IsSupportAuth(int loginChannel)
        {
            return SdkManager.NativeApi.IsSupportAuth(loginChannel);
        }

        public static void RegisterChannel(int loginChannel, string appId, string weiboRedirectUrl = "")
        {
            SdkManager.NativeApi?.RegisterChannel(loginChannel, appId, weiboRedirectUrl);
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
            if (Channel.Equals("Quick"))
            {
                QuickSDK.getInstance().logout();
                return;
            }
            if (Channel.Equals("SoFunny"))
            {
                SoFunnyController.Logout();
                return;
            }

            SdkManager.Auth.Logout();
        }
        public static void CreateRole()
        {
            if (Channel.Equals("Quick"))
            {
                QuickSDK.getInstance().createRole(gameRoleInfo);
            }
        }
        public static void EnterGame()
        {
            if (Channel.Equals("Quick"))
            {
                QuickSDK.getInstance().enterGame(gameRoleInfo);
            }
        }
        public static void UpdateRole()
        {
            if (Channel.Equals("Quick"))
            {
                QuickSDK.getInstance().updateRole(gameRoleInfo);
            }
        }
        
        // public static bool IsSupportUserCenter()
        // {
        //     if (Channel.Equals("Quick"))
        //     {
        //         return QuickSDK.getInstance().isFunctionSupported(FuncType.QUICK_SDK_FUNC_TYPE_ENTER_USER_CENTER);
        //     }
        //
        //     return true;
        // }

        /// <summary>
        ///     打开用户中心
        /// </summary>
        public static void OpenUserCenter()
        {
            if (!IsInitialized) return;
            if (Channel.Equals("Quick"))
            {
                // if (QuickSDK.getInstance().isFunctionSupported(FuncType.QUICK_SDK_FUNC_TYPE_ENTER_USER_CENTER))
                // {
                //     QuickSDK.getInstance().callFunction(FuncType.QUICK_SDK_FUNC_TYPE_ENTER_USER_CENTER);
                // }
                QuickSDK.getInstance().logout();
                return;
            }

            if (Channel.Equals("SoFunny"))
            {
                SoFunnyController.OpenUserCenter();
                return;
            }
            
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

            if (Channel.Equals("Quick"))
            {
                return;
            }
            if (Channel.Equals("SoFunny"))
            {
                return;
            }
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
        public static void Pay(OrderInfo order)
        {
            if (Channel.Equals("Quick"))
            {
                quicksdk.OrderInfo orderInfo = new quicksdk.OrderInfo();
                orderInfo.goodsID = order.id;
                orderInfo.goodsName = order.name;
                orderInfo.goodsDesc = order.name;
                orderInfo.quantifier = "个";
                orderInfo.extrasParams = order.id;
                orderInfo.count = order.count;
                orderInfo.amount = order.price;
                orderInfo.price = order.price;
                orderInfo.callbackUrl = order.callbackUrl;
                orderInfo.cpOrderID = "1";
                QuickSDK.getInstance ().pay (orderInfo, gameRoleInfo);
                return;
            }
            if (!_isPaymentInitialized) return;

            SdkManager.Order.Pay(order.id);
        }

        /// <summary>
        ///     根据 ID 获取商品信息
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product</returns>
        public static Product GetProductById(string id)
        {
            if (Channel.Equals("Quick"))
            {
                return null;
            }
            if (!_isPaymentInitialized) return null;

            return SdkManager.PaymentApi.GetProductById(id);
        }

        /// <summary>
        /// 获取全部商品信息
        /// </summary>
        /// <returns></returns>
        public static Product[] GetProducts()
        {
            if (Channel.Equals("Quick"))
            {
                return new Product[0];
            }
            if (!_isPaymentInitialized) return null;

            return SdkManager.PaymentApi.GetProducts();
        }

        /// <summary>
        ///     退出游戏
        /// </summary>
        public static void ExitGame()
        {
            if (!IsInitialized) return;

            if (Channel.Equals("Quick") && QuickSDK.getInstance().isChannelHasExitDialog()){
                    QuickSDK.getInstance().exit();
                    return;
            }
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

        internal static void OnShareSucceed(string msg)
        {
            Log.Debug("OnShareSucceed" + msg);
            var handler = ShareSucceed;
            if (handler != null) handler(msg);
        }
        
        internal static void OnShareFailure(string msg)
        {
            Log.Debug("OnShareFailure" + msg);
            var handler = ShareFailure;
            if (handler != null) handler(msg);
        }
        
        internal static void OnShareCancel(string msg)
        {
            Log.Debug("OnShareCancel" + msg);
            var handler = ShareCancel;
            if (handler != null) handler(msg);
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

        public static event Action<string> ShareSucceed;
        public static event Action<string> ShareFailure;
        public static event Action<string> ShareCancel;

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
