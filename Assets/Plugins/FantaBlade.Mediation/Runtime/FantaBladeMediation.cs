using System;
using FantaBlade.Common;
using FantaBlade.Platform;
using quicksdk;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
#endif

namespace FantaBlade.Mediation
{
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

    public class FantaBladeMediation
    {
        private static bool _isInitialized;
        
        private static GameRoleInfo _gameRoleInfo = new GameRoleInfo();

        private static readonly string DefinedChannel;

        static FantaBladeMediation()
        {
#if SOFUNNY
        DefinedChannel = "SoFunny";
#elif UNITY_IOS
        DefinedChannel = "App Store";
#elif UNITY_ANDROID
        #if QUICK
            DefinedChannel = "Quick";
        #elif DOUYIN
            DefinedChannel = "Douyin";
        #elif KUAISHOU
            DefinedChannel = "Kuaishou";
        #elif TAPTAP
            DefinedChannel = "TapTap";
        #elif GOOGLE_PLAY
            DefinedChannel = "Google Play";
        #else
            DefinedChannel = "Official";
        #endif
#else
            DefinedChannel = "Official";
#endif
        }

        #region API

        public static string Channel
        {
            get
            {
                if (DefinedChannel == "Quick")
                {
                    return QuickSDK.getInstance().channelName();
                }
#if UNITY_ANDROID && !UNITY_EDITOR
#if DOUYIN
            try {
                AndroidJavaObject context =
 new AndroidJavaClass ("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject> ("currentActivity"); //获得Context
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
#else
            try {
                AndroidJavaObject context =
 new AndroidJavaClass ("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject> ("currentActivity"); //获得Context
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
                return DefinedChannel;
            }
        }

        /// <summary>
        /// 是否需要验证激活
        /// </summary>
        public static bool NeedActivation;


        public static bool IsVerify()
        {
            if (Channel.Equals("Quick"))
            {
                var userInfo = EventHandle.Instance._UserInfo;
                return userInfo.realName;
            }

            if (Channel.Equals("SoFunny"))
            {
                return true; //上海外不触发实名制
            }

            return FantaBladePlatform.IsVerify;
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
                return 18; //上海外不触发实名制
            }

            return FantaBladePlatform.Age;
        }

        /// <param name="json">IAP 商品目录</param>
        public static void SetProductCatalog(string json)
        {
            FantaBladePlatform.SetProductCatalog(json);
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
            if (_isInitialized) return;
// #if UNITY_IOS && !UNITY_EDITOR
            // Unity.Advertisement.IosSupport.ATTrackingStatusBinding.RequestAuthorizationTracking();
// #endif
            if (Channel == "Quick")
            {
                var go = new GameObject("QuickSDK");
                go.AddComponent<EventHandle>();
                Object.DontDestroyOnLoad(go);
            }
            else if (Channel.Equals("SoFunny"))
            {
                SoFunnyController.Init();
            }
            else
            {
                FantaBladePlatform.Init(accessKey, showFloatingWindow, publishRegion);
                FantaBladePlatform.InitializeSuccess += OnInitializeSuccess;
                FantaBladePlatform.InitializeFailure += OnInitializeFailure;
                FantaBladePlatform.PaymentInitializeSuccess += OnPaymentInitializeSuccess;
                FantaBladePlatform.PaymentInitializeFailure += OnPaymentInitializeFailure;
                FantaBladePlatform.SwitchAccountSuccess += OnSwitchAccountSuccess;
                // FantaBladePlatform.PrivacyAgree += OnPrivacyAgree;
                // FantaBladePlatform.PrivacyRefuse += OnPrivacyRefuse;
                FantaBladePlatform.LoginSuccess += OnLoginSuccess;
                FantaBladePlatform.LoginFailure += OnLoginFailure;
                FantaBladePlatform.LoginCancel += OnLoginCancel;
                FantaBladePlatform.LogoutSuccess += OnLogoutSuccess;
                FantaBladePlatform.PaySuccess += OnPaySuccess;
                FantaBladePlatform.PayFailure += OnPayFailure;
                FantaBladePlatform.PayCancel += OnPayCancel;
                FantaBladeShare.ShareSucceed += OnShareSucceed;
                FantaBladeShare.ShareFailure += OnShareFailure;
                FantaBladeShare.ShareCancel += OnShareCancel;
            }

            _isInitialized = true;
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

        public static void ShowPrivace()
        {
            if (Channel.Equals("Quick"))
            {
                if (EventHandle.isInit)
                {
                    Log.Info("QuickSDK.getInstance().login(");
                    QuickSDK.getInstance().login();
                }
                else
                {
                    QuickSDK.getInstance().showPrivace();
                }

                return;
            }
        }

        /// <summary>
        ///     登陆账号，获取token
        /// </summary>
        /// <param name="forceShowUi">强制显示UI，不使用缓存token</param>
        public static void Login(bool forceShowUi = false, bool useQuickLoginFirstTime = false)
        {
            Log.Debug("onlogin");
            if (!FantaBladePlatform.IsInitialized) return;
            if (Channel.Equals("Quick"))
            {
                if (EventHandle.isInit)
                {
                    Log.Info("QuickSDK.getInstance().login(");
                    QuickSDK.getInstance().login();
                }
                else
                {
                    QuickSDK.getInstance().init();
                }

                return;
            }

            if (Channel.Equals("SoFunny"))
            {
                SoFunnyController.Login();
                return;
            }

            FantaBladePlatform.Login(forceShowUi, useQuickLoginFirstTime);
        }

        /// <summary>
        ///     退出登陆，需要返回游戏首页重新调用<see cref="Login" />
        /// </summary>
        public static void Logout()
        {
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

            FantaBladePlatform.Logout();
        }

        public static void CreateRole()
        {
            if (Channel.Equals("Quick"))
            {
                QuickSDK.getInstance().createRole(_gameRoleInfo);
            }
        }

        public static void EnterGame()
        {
            if (Channel.Equals("Quick"))
            {
                QuickSDK.getInstance().enterGame(_gameRoleInfo);
            }
        }

        public static void UpdateRole()
        {
            if (Channel.Equals("Quick"))
            {
                QuickSDK.getInstance().updateRole(_gameRoleInfo);
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

            FantaBladePlatform.OpenUserCenter();
        }

        public static void OpenVerifyAge()
        {
            if (Channel.Equals("Quick"))
            {
                return;
            }

            if (Channel.Equals("SoFunny"))
            {
                return;
            }

            FantaBladePlatform.OpenVerifyAge();
        }

        /// <summary>
        ///     支付
        /// </summary>
        public static void Pay(OrderInfo order)
        {
            Log.Debug(order.id);
            if (Channel.Equals("Quick"))
            {
                quicksdk.OrderInfo orderInfo = new quicksdk.OrderInfo();
                orderInfo.goodsID = order.id;
                orderInfo.goodsName = order.name;
                if (QuickSDK.getInstance().channelType() == 24) //华为传商品类型：0消耗型
                    orderInfo.goodsDesc = "0";
                else
                    orderInfo.goodsDesc = order.name;
                orderInfo.quantifier = "个";
                orderInfo.extrasParams = order.id;
                orderInfo.count = order.count;
                orderInfo.amount = order.price;
                orderInfo.price = order.price;
                orderInfo.callbackUrl = order.callbackUrl;
                orderInfo.cpOrderID = "1";
                QuickSDK.getInstance().pay(orderInfo, _gameRoleInfo);
                return;
            }

            FantaBladePlatform.Pay(order.id);
        }

        /// <summary>
        ///     退出游戏
        /// </summary>
        public static void ExitGame()
        {
            if (Channel.Equals("Quick") && QuickSDK.getInstance().isChannelHasExitDialog())
            {
                QuickSDK.getInstance().exit();
                return;
            }

            FantaBladePlatform.ExitGame();
        }

        #endregion

        #region Event invoker

        internal static void OnInitializeSuccess()
        {
            Log.Info("OnInitializeSuccess");
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

        internal static void OnPrivacyAgree()
        {
            Log.Info("OnPrivacyAgree: ");
            var handler = PrivacyAgree;
            if (handler != null) handler();
        }

        internal static void OnPrivacyRefuse()
        {
            Log.Info("OnPrivacyRefuse: ");
            var handler = PrivacyRefuse;
            if (handler != null) handler();
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
        ///     隐私协议通过
        /// </summary>
        public static event Action PrivacyAgree;


        /// <summary>
        ///     隐私协议不通过
        /// </summary>
        public static event Action PrivacyRefuse;

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

        public static event Action<string> ShareSucceed;
        public static event Action<string> ShareFailure;
        public static event Action<string> ShareCancel;

        #endregion
    }
}