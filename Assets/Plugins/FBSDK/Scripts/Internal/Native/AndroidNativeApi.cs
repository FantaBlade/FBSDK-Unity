#if UNITY_ANDROID
using UnityEngine;

namespace FbSdk.Internal.Native
{
    internal class AndroidNativeApi : INativeApi
    {
        private static AndroidJavaObject _nativeApi;
        
        public void Init()
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            _nativeApi = new AndroidJavaObject("com.fantablade.fbsdk.Api", currentActivity, SdkManager.AccessKeyId);
            _nativeApi.Call("setListener", new AndroidSdkCallback());
            Sdk.OnInitializeSuccess();
        }

        public void SetToken(string token)
        {
            if (!Sdk.IsInitialized)
            {
                Debug.LogWarning("FBSDK is not initialized");
                return;
            }
            _nativeApi.Call("setToken", token);
        }

        public void Logout()
        {
            if (!Sdk.IsInitialized)
            {
                Debug.LogWarning("FBSDK is not initialized");
                return;
            }
            _nativeApi.Call("logout");
        }

        public void Pay(string productId, string name, int price)
        {
            if (!Sdk.IsInitialized)
            {
                Debug.LogWarning("FBSDK is not initialized");
                return;
            }
            _nativeApi.Call("pay", productId, name, price);
        }
    }
}
#endif