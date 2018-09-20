using UnityEngine;
using UnityEngine.Purchasing;

#if UNITY_IOS
namespace FbSdk.Internal.Native
{
    public class IosNativeApi : INativeApi, IStoreListener
    {
        private IStoreController _controller;
        private IExtensionProvider _extensions;

        public void Init()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            UnityPurchasing.Initialize(this, builder);
        }

        public void Pay(string commodityName, string commodityInfo, int orderAmount)
        {
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            foreach (var product in controller.products.all)
            {
                Debug.Log(product.metadata.localizedTitle);
                Debug.Log(product.metadata.localizedDescription);
                Debug.Log(product.metadata.localizedPriceString);
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
        {
        }
    }
}
#endif