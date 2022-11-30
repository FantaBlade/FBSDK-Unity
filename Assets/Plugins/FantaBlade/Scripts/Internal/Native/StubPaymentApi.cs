using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace FantaBlade.Internal.Native
{
    internal class StubPaymentApi : IPaymentApi
    {
        private ProductCollection _products;
        
        public void Init()
        {
            _products = SdkManager.Order.GetCustomProducts();
            Api.OnPaymentInitializeSuccess();
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var product = GetProductById(productId);
            var name = product.metadata.localizedTitle + "";
            var price = ((int) product.metadata.localizedPrice) * 100 + "";
            var form = new Dictionary<string, string>
            {
                {"commodityId", productId},
                {"commodityName", name},
                {"orderAmount", price},
                {"payMethod", "0"},
            };
            PlatformApi.User.FakePay.Post(form, (err, info, resp) =>
            {
                //fake pay
                Api.OnPaySuccess();
            });
#else
            Api.OnPaySuccess();
#endif
        }
    }
}