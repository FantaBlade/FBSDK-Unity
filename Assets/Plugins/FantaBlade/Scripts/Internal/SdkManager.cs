﻿using System;
using System.Collections;
using FantaBlade.Internal.Native;
using UnityEngine;

namespace FantaBlade.Internal
{
    internal class SdkManager : MonoBehaviour
    {
        /// <summary>
        ///     Debug 模式
        /// </summary>
        public static readonly bool DebugMode = false;


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
        public static readonly LocalizeManager Localize = new LocalizeManager();
        public static readonly ShareManager Share = new ShareManager();
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

                var isPaymentInitialized = PaymentApi != null && Api.IsPaymentInitialized;
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
                NativeApi  = new iOSNativeApi();
                PaymentApi = new UnityIapPaymentApi();
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
                else
                {
                    bool needRequestIpJson = PublishRegion == PublishRegion.SoutheastAsia;
                    if (needRequestIpJson)
                    {
                        PlatformApi.Util.GetIpJson.Get((err, info, response) =>
                        {
                            if (err == null)
                            {
                                if (Location != response.countryCode)
                                {
                                    Location = response.countryCode;
                                }
                            }
                        });
                    }
                    else
                    {
                        Location = "CN";
                    }
                }
                #endregion

                Api.OnInitializeSuccess();
            }
            catch (Exception e)
            {
                Api.OnInitializeFailure(e.Message);
            }
        }

        public static void UpdateLanguage(SystemLanguage language = SystemLanguage.Unknown)
        {
            if (language == SystemLanguage.Unknown)
            {
                SystemLanguage cachedLang = (SystemLanguage)PlayerPrefs.GetInt("fantablade_sdk_language", (int)SystemLanguage.Unknown);
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

        public new static Coroutine StartCoroutine(IEnumerator coroutine)
        {
            return ((MonoBehaviour) Instance).StartCoroutine(coroutine);
        }

        public void UserAcceptLisense()
        {
            PlayerPrefs.SetInt("user_accept_lisense", 1);
        }

        public static bool IsUserAcceptLisense()
        {
            return 1 == PlayerPrefs.GetInt("user_accept_lisense", 0);
        }

        void Update()
        {
            if (null != MonoUpdate)
            {
                MonoUpdate.Update(Time.deltaTime);
            }
        }
    }
}