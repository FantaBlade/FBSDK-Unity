using UnityEngine.Purchasing;

namespace FbSdk.Internal.Native
{
    internal class StubPaymentApi : IPaymentApi
    {
        private ProductCollection _products;
        
        public void Init()
        {
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

        public void Pay(string productId)
        {
            Sdk.OnPaySuccess();
        }
    }
}