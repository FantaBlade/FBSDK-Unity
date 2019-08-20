using System.Collections;
using FbSdk.Internal.Native;
using UnityEngine;

namespace FbSdk.Internal
{
    internal class SdkManager : MonoBehaviour
    {
        public static bool DebugMode = false;
        public static Sdk.PublishRegion PublishRegion;
        public static bool UseAndroidNativeApi;

        public static SdkManager Instance { get; private set; }

        public static string AccessKeyId { get; private set; }

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


        public static void Init(string accessKeyId, bool showFloatingWindow, Sdk.PublishRegion publishRegion,
            string productCatalogJson)
        {
            Log.CurrentLevel = DebugMode ? Log.LogLevel.Debug : Log.LogLevel.Info;

            PublishRegion = publishRegion;

#if UNITY_ANDROID && !UNITY_EDITOR
            UseAndroidNativeApi = PublishRegion == Sdk.PublishRegion.China;
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

            var ui = Resources.Load<GameObject>("fbsdk/prefab/fbsdk");
            ui = Instantiate(ui);
            DontDestroyOnLoad(ui);
            ui.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            Instance = ui.AddComponent<SdkManager>();
            Ui.Init();
            Ui.FloatingWindow.IsActive = showFloatingWindow;

            Order.SetProductCatalog(productCatalogJson);
            PaymentApi.Init();
        }

        public new static Coroutine StartCoroutine(IEnumerator coroutine)
        {
            return ((MonoBehaviour) Instance).StartCoroutine(coroutine);
        }
    }
}