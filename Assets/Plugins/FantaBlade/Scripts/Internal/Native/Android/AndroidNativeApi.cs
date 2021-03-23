#if UNITY_ANDROID
using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace FantaBlade.Internal.Native
{
    internal class AndroidNativeApi : INativeApi, IPaymentApi
    {
        private static AndroidJavaObject _nativeApi;
        private ProductCollection _products;

        public void Init()
        {
            try
            {
                var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                _nativeApi = new AndroidJavaObject("com.fantablade.fbsdk.Api", currentActivity, SdkManager.AccessKeyId);
                _nativeApi.Call("setListener", new AndroidSdkCallback());

                _products = SdkManager.Order.GetCustomProducts();

                Api.OnPaymentInitializeSuccess();
            }
            catch (Exception e)
            {
                Api.OnPaymentInitializeFailure(e.Message);
            }
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
            if (!Api.IsInitialized)
            {
                Log.Warning("FBSDK is not initialized");
                return;
            }

            _nativeApi.Call("setToken", token);
        }

        public void RegisterChannel(int loginChannel, string appId, string weiboRedirectUrl = "")
        {
            _nativeApi.Call("registerChannel", loginChannel, appId, weiboRedirectUrl);
        }

        public bool IsInstall(int loginChannel)
        {
            return _nativeApi.Call<bool>("isInstalled", loginChannel);
        }
        
        public void Login(int loginChannel)
        {
            _nativeApi.Call("login", loginChannel);
        }

        public void Share(int shareChannel, string imagePath, string title, string desc)
        {
            _nativeApi.Call("shareImage", shareChannel, imagePath, title, desc);
        }

        public void Logout()
        {
            if (!Api.IsInitialized)
            {
                Log.Warning("FBSDK is not initialized");
                return;
            }

            _nativeApi.Call("logout");
        }

        public void Pay(string productId)
        {
            if (!Api.IsInitialized)
            {
                Log.Warning("FBSDK is not initialized");
                return;
            }

            var product = GetProductById(productId);
            var name = product.metadata.localizedTitle;
            var price = ((int) product.metadata.localizedPrice) * 100;
            _nativeApi.Call("pay", productId, name, price);
        }
    }
}
#endif