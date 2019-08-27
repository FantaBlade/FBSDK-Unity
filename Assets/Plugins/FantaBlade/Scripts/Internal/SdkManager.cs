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
        public static SystemLanguage Language;
        
        /// <summary>
        ///     玩家定位
        /// </summary>
        public static string Location;

        public static event Action<string> LocationSuccess;

#if UNITY_ANDROID && !UNITY_EDITOR
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

        /// <summary>
        ///     原生层 API
        /// </summary>
        internal static INativeApi NativeApi;

        /// <summary>
        ///     支付 API
        /// </summary>
        internal static IPaymentApi PaymentApi;


        public static void Init(string accessKeyId, bool showFloatingWindow, PublishRegion publishRegion,
            string productCatalogJson)
        {
            Log.CurrentLevel = DebugMode ? Log.LogLevel.Debug : Log.LogLevel.Info;

            PublishRegion = publishRegion;
            PlatformApi.SetRegion(publishRegion);

            Language = Application.systemLanguage;

#if UNITY_ANDROID && !UNITY_EDITOR
            UseAndroidNativeApi = PublishRegion == PublishRegion.China;
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
            PaymentApi = new UnityIapPaymentApi();
#else
            PaymentApi = new StubPaymentApi();
#endif

            if (Instance != null) return;
            Instance = FindObjectOfType<SdkManager>();
            if (Instance != null) return;
            AccessKeyId = accessKeyId;

            var ui = Resources.Load<GameObject>("fantablade_sdk/prefab/fantablade_sdk");
            ui = Instantiate(ui);
            DontDestroyOnLoad(ui);
            ui.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            Instance = ui.AddComponent<SdkManager>();
            Ui.Init();
            Ui.FloatingWindow.IsActive = showFloatingWindow;

            Order.SetProductCatalog(productCatalogJson);
            PaymentApi.Init();

            // 查询玩家位置
            const string locationKey = "FbSdk_Location";
            if (PlayerPrefs.HasKey(locationKey))
            {
                Location = PlayerPrefs.GetString(locationKey);
                if (LocationSuccess != null) LocationSuccess(Location);
            }

            PlatformApi.Util.GetIpInfo.Get((err, info, response) =>
            {
                if (err == null)
                {
                    if (Location != response.countryCode)
                    {
                        Location = response.countryCode;
                        PlayerPrefs.SetString(locationKey, Location);
                        PlayerPrefs.Save();
                        if (LocationSuccess != null) LocationSuccess(Location);
                    }
                }
            });
        }

        public new static Coroutine StartCoroutine(IEnumerator coroutine)
        {
            return ((MonoBehaviour) Instance).StartCoroutine(coroutine);
        }
    }
}