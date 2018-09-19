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
        }

        public void Pay(string commodityName, string commodityInfo, int orderAmount)
        {
            _nativeApi.Call("pay", commodityName, commodityInfo, orderAmount);
        }
    }
}
#endif