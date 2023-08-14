using UnityEngine.Purchasing;

namespace FantaBlade.Platform.Internal.Native
{
    internal interface IPaymentApi
    {
        void Init();
        
        Product GetProductById(string id);

        Product[] GetProducts();
        
        void Pay(string productId);
    }
}