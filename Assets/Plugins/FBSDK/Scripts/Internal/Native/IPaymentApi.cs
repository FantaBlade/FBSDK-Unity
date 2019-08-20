using UnityEngine.Purchasing;

namespace FbSdk.Internal.Native
{
    internal interface IPaymentApi
    {
        void Init();
        
        Product GetProductById(string id);

        Product[] GetProducts();
        
        void Pay(string productId);
    }
}