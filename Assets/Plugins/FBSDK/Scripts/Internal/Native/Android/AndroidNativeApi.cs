#if UNITY_ANDROID
using UnityEngine;
using UnityEngine.Purchasing;

namespace FbSdk.Internal.Native
{
    internal class AndroidNativeApi : INativeApi, IPaymentApi
    {
        private static AndroidJavaObject _nativeApi;
        private ProductCollection _products;

        public void Init()
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            _nativeApi = new AndroidJavaObject("com.fantablade.fbsdk.Api", currentActivity, SdkManager.AccessKeyId);
            _nativeApi.Call("setListener", new AndroidSdkCallback());

            _products = SdkManager.Order.GetCustomProducts();

            Sdk.OnInitializeSuccess();
        }

        public Product GetProductById(string id)
        {
            return _products.WithID(id);
        }

        public Product[] GetProducts()
        {
            return _products.all;
        }

        public void SetToken(string token)
        {
            if (!Sdk.IsInitialized)
            {
                Log.Warning("FBSDK is not initialized");
                return;
            }

            _nativeApi.Call("setToken", token);
        }

        public void Logout()
        {
            if (!Sdk.IsInitialized)
            {
                Log.Warning("FBSDK is not initialized");
                return;
            }

            _nativeApi.Call("logout");
        }

        public void Pay(string productId)
        {
            if (!Sdk.IsInitialized)
            {
                Log.Warning("FBSDK is not initialized");
                return;
            }

            var product = SdkManager.PaymentApi.GetProductById(productId);
            var name = product.metadata.localizedTitle;
            var price = ((int) product.metadata.localizedPrice) * 100;
            _nativeApi.Call("pay", productId, name, price);
        }
    }
}
#endif