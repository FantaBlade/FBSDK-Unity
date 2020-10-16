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
        
        #region API

#if UNITY_IOS
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
#elif GOOGLE_PLAY
    public static readonly string Channel = "Google Play";
#else
        public static readonly string Channel = "Official";
#endif
#else
    public static readonly string Channel = "Unknown";
#endif
        public static string GetChannel()
        {
            if (Channel.Equals("Quick"))
            {
                return QuickSDK.getInstance().channelName();
            }
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
                bool real = !userInfo.realName || (userInfo.age > 0 && userInfo.age < 18);
                return !real;
            }
            return SdkManager.Auth.IsVerify;
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
            // if (Channel.Equals("Quick"))
            // {
            //     OnInitializeSuccess();
            // }
            // else
            // {
                SdkManager.Init(accessKey, showFloatingWindow, publishRegion);
            // }
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
        public static void Login(bool forceShowUi = false)
        {
            Debug.Log("onlogin");
            if (!IsInitialized) return;
            if (Channel.Equals("Quick"))
            {
                QuickSDK.getInstance().login();
                return;
            }

            if (forceShowUi)
            {
                SdkManager.Ui.ShowLogin();
            }
            else
            {
                SdkManager.Auth.LoginByCache();
            }
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
            Debug.Log("OnPayCancel");
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