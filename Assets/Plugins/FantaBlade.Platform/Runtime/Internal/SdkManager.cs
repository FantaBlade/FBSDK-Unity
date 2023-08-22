using System;
using System.Collections;
using System.Collections.Generic;
using FantaBlade.Common;
using FantaBlade.Platform.Internal;
using FantaBlade.Platform.Internal.Native;
using UnityEngine;

namespace FantaBlade.Platform.Internal
{
    internal class SdkManager : MonoBehaviour
    {
        private static readonly List<FantaBladePlatform.LoginChannel> _loginThirdChannels =
            new List<FantaBladePlatform.LoginChannel>();

        /// <summary>
        ///     Debug 模式
        /// </summary>
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public static readonly bool DebugMode = true;
#else
        public static readonly bool DebugMode = false;
#endif


        /// <summary>
        ///     发行区域
        /// </summary>
        public static PublishRegion PublishRegion;

        /// <summary>
        ///     语言
        /// </summary>
        public static SystemLanguage Language = SystemLanguage.Unknown;

        public static string LanguageString
        {
            get
            {
                return Language == SystemLanguage.Chinese
                       || Language == SystemLanguage.ChineseSimplified
                       || Language == SystemLanguage.ChineseTraditional
                    ? "Chinese"
                    : "English";
            }
        }

        public static string Ip = "";

        /// <summary>
        ///     玩家定位
        /// </summary>
        public static string Location
        {
            get
            {
                const string locationKey = "FantaBladeSDK_User_Location";
                return PlayerPrefs.GetString(locationKey);
            }
            set
            {
                const string locationKey = "FantaBladeSDK_User_Location";
                PlayerPrefs.SetString(locationKey, value);
                PlayerPrefs.Save();
                if (LocationSuccess != null) LocationSuccess(value);
            }
        }

        public static string CacheAccountId
        {
            get
            {
                const string locationKey = "FantaBladeSDK_User_AccountId";
                return PlayerPrefs.GetString(locationKey);
            }
            set
            {
                const string locationKey = "FantaBladeSDK_User_AccountId";
                PlayerPrefs.SetString(locationKey, value);
                PlayerPrefs.Save();
            }
        }

        public static string CachePhoneNumber
        {
            get
            {
                const string locationKey = "FantaBladeSDK_User_PhoneNumber";
                return PlayerPrefs.GetString(locationKey);
            }
            set
            {
                const string locationKey = "FantaBladeSDK_User_PhoneNumber";
                PlayerPrefs.SetString(locationKey, value);
                PlayerPrefs.Save();
            }
        }

        public static event Action<string> LocationSuccess;

#if UNITY_ANDROID //&& !UNITY_EDITOR
        /// <summary>
        ///     是否使用 Android 原生接口
        /// </summary>
        public static bool UseAndroidNativeApi;
#endif

        /// <summary>
        ///     Platform Access Key ID
        /// </summary>
        public static string AccessKeyId { get; private set; }

        public static SdkManager Instance { get; private set; }

        public static readonly UiManager Ui = new UiManager();
        public static readonly AuthManager Auth = new AuthManager();
        public static readonly OrderManager Order = new OrderManager();
        public static readonly ShareManager Share = new ShareManager();
        public static readonly LocalizeManager Localize = new LocalizeManager();
        public static readonly MonoUpdateManager MonoUpdate = new MonoUpdateManager();

        /// <summary>
        ///     原生层 API
        /// </summary>
        internal static INativeApi NativeApi;

        /// <summary>
        ///     支付 API
        /// </summary>
        internal static IPaymentApi PaymentApi;

        public static void Init(string accessKeyId, bool showFloatingWindow,
            PublishRegion publishRegion)
        {
            try
            {
                Log.CurrentLevel = DebugMode ? Log.LogLevel.Debug : Log.LogLevel.Warning;

                PublishRegion = publishRegion;
                CountryInfo.SetDefaultCounty(publishRegion);
                PlatformApi.SetRegion(publishRegion);
                UpdateLanguage(Language);

                var isPaymentInitialized = PaymentApi != null && FantaBladePlatform.IsPaymentInitialized;
                if (!isPaymentInitialized)
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                UseAndroidNativeApi = PublishRegion != PublishRegion.SoutheastAsia;
                if (UseAndroidNativeApi)
                {
                    var androidNativeApi = new AndroidNativeApi();
                    NativeApi = androidNativeApi;
                    PaymentApi = androidNativeApi;
                }
                else
                {
                    PaymentApi = new UnityIapPaymentApi();
                }
#elif UNITY_IOS && !UNITY_EDITOR
                NativeApi = new iOSNativeApi();
                Debug.Log("NativeApi = new iOSNativeApi");
                Debug.Log(NativeApi != null);
                PaymentApi = new UnityIapPaymentApi();
                NativeApi?.Init();
#else
                    PaymentApi = new StubPaymentApi();
                    NativeApi = new StubNativeApi();
#endif
                }

                #region UI

                if (Instance != null) return;
                Instance = FindObjectOfType<SdkManager>();
                if (Instance != null) return;
                AccessKeyId = accessKeyId;

                var ui = Resources.Load<GameObject>("fantablade_sdk/prefab/fantablade_sdk");
                ui = Instantiate(ui);
                DontDestroyOnLoad(ui);
                ui.hideFlags = DebugMode ? HideFlags.None : HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                Instance = ui.AddComponent<SdkManager>();
                Ui.Init();
                Ui.FloatingWindow.IsActive = showFloatingWindow;
                if (!isPaymentInitialized)
                {
                    PaymentApi?.Init();
                }

                Share.SetNativeAPI(NativeApi);
                // 查询玩家位置
                if (!string.IsNullOrEmpty(Location))
                {
                    if (LocationSuccess != null) LocationSuccess(Location);
                }

                PlatformApi.Util.GetIpApi.Get((err, info, response) =>
                {
                    if (err == null)
                    {
                        Ip = response.query;
                        Debug.Log("ip:" + response.query);
                        if (string.IsNullOrEmpty(Location))
                        {
                            Location = response.countryCode;
                        }
                    }
                });
                PlatformApi.Util.GetIpCo.Get((err, info, response) =>
                {
                    if (err == null)
                    {
                        Ip = response.ip;
                        Debug.Log("ip:" + response.ip);
                        if (string.IsNullOrEmpty(Location))
                        {
                            Location = response.country_code;
                        }
                    }
                });

                #endregion

                FantaBladePlatform.OnInitializeSuccess();
            }
            catch (Exception e)
            {
                Debug.Log(e);
                FantaBladePlatform.OnInitializeFailure(e.Message);
            }
        }

        public static void EnableThirdChannel(FantaBladePlatform.LoginChannel[] loginChannels,
            Dictionary<FantaBladePlatform.LoginChannel, string> appIds = null)
        {
            for (int i = 0, max = loginChannels.Length; i < max; ++i)
            {
                string appId = "";
                string param = "";
                FantaBladePlatform.LoginChannel channel = loginChannels[i];
                switch (channel)
                {
                    case FantaBladePlatform.LoginChannel.CHANNEL_WECHAT:
                        appId = Config.WECHAT_APPID;
                        param = Config.WECHAT_UNIVERSAL_LINK;
                        break;
                    case FantaBladePlatform.LoginChannel.CHANNEL_QQ:
                        appId = Config.QQ_APPID;
                        param = Application.identifier + ".fileprovider";
                        break;
                    case FantaBladePlatform.LoginChannel.CHANNEL_WEIBO:
                        appId = Config.WEIBO_APPID;
#if UNITY_ANDROID
                        param = Config.WEIBO_REDIRECTURL;
#else
                        param = Config.WECHAT_UNIVERSAL_LINK;
#endif
                        break;
                    case FantaBladePlatform.LoginChannel.CHANNEL_DOUYIN:
                        appId = Config.DOUYIN_CLIENTKEY;
                        break;
                    case FantaBladePlatform.LoginChannel.CHANNEL_MOBILE:
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

        public static void RegisterChannel(FantaBladePlatform.LoginChannel loginChannel, string appId,
            string weiboRedirectUrl = "")
        {
            if (!_loginThirdChannels.Contains(loginChannel))
            {
                NativeApi?.RegisterChannel((int)loginChannel, appId, weiboRedirectUrl);
                _loginThirdChannels.Add(loginChannel);
            }
        }

        public static void UpdateLanguage(SystemLanguage language = SystemLanguage.Unknown)
        {
            if (language == SystemLanguage.Unknown)
            {
                SystemLanguage cachedLang =
                    (SystemLanguage)PlayerPrefs.GetInt("fantablade_sdk_language", (int)SystemLanguage.Unknown);
                if (cachedLang == SystemLanguage.Unknown)
                {
                    cachedLang = Application.systemLanguage;
                }

                language = cachedLang;
            }

            if (Language != language)
            {
                Language = language;
                PlayerPrefs.SetInt("fantablade_sdk_language", (int)Language);
                PlayerPrefs.Save();
            }

            Localize.Init(Language);
        }

        public static Coroutine StartCoroutine(IEnumerator coroutine)
        {
            return ((MonoBehaviour)Instance).StartCoroutine(coroutine);
        }

        public static void UserAcceptLisense()
        {
            PlayerPrefs.SetInt("user_accept_lisense", 1);
        }

        public static bool IsUserAcceptLisense()
        {
            return 1 == PlayerPrefs.GetInt("user_accept_lisense", 0);
        }

        public static void ActiveLoginChannel(FantaBladePlatform.LoginChannel loginChannel, bool enable)
        {
            if (enable)
            {
                _loginThirdChannels.Add(loginChannel);
            }
            else
            {
                _loginThirdChannels.Remove(loginChannel);
            }
        }

        /// <summary>
        ///     登录渠道是否设定开启
        /// </summary>
        /// <param name="loginChannel"></param>
        /// <returns></returns>
        public static bool IsLoginChannelEnable(FantaBladePlatform.LoginChannel loginChannel)
        {
            return _loginThirdChannels.Contains(loginChannel) && NativeApi != null &&
                   NativeApi.IsChannelRegister((int)loginChannel);
        }

        /// <summary>
        ///     登录渠道是否可以使用
        /// </summary>
        /// <returns></returns>
        public static bool IsLoginChannelAvailable(FantaBladePlatform.LoginChannel loginChannel)
        {
            return IsLoginChannelEnable(loginChannel) && FantaBladePlatform.IsSupportAuth(loginChannel) &&
                   FantaBladePlatform.IsInstalled((int)loginChannel);
        }

        private void Update()
        {
            if (null != MonoUpdate)
            {
                MonoUpdate.Update(Time.deltaTime);
            }
        }
    }
}