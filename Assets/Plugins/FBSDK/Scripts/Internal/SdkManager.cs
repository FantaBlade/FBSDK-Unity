using System.Collections;
using FbSdk.Internal.Native;
using UnityEngine;

namespace FbSdk.Internal
{
    internal class SdkManager : MonoBehaviour
    {
        public static SdkManager Instance { get; private set; }

        public static bool IsInitialized { get; private set; }

        public static string AccessKeyId { get; private set; }

        public static readonly UiManager Ui = new UiManager();
        public static readonly AuthManager Auth = new AuthManager();
        public static readonly OrderManager Order = new OrderManager();

        internal static readonly INativeApi NativeApi =
#if UNITY_EDITOR
            new StubNativeApi();
#elif UNITY_ANDROID
            new AndroidNativeApi();
#elif UNITY_IOS
            new IosNativeApi();
#endif

        public static void Init(string accessKeyId)
        {
            if (Instance != null) return;
            Instance = FindObjectOfType<SdkManager>();
            if (Instance != null) return;
            AccessKeyId = accessKeyId;

            NativeApi.Init();

            var ui = Resources.Load<GameObject>("fbsdk/prefab/fbsdk");
            ui = Instantiate(ui);
            DontDestroyOnLoad(ui);
            Instance = ui.AddComponent<SdkManager>();
            Ui.Init();

            IsInitialized = true;
            Sdk.OnInitialize();
        }

        public new static Coroutine StartCoroutine(IEnumerator coroutine)
        {
            return ((MonoBehaviour) Instance).StartCoroutine(coroutine);
        }
    }
}