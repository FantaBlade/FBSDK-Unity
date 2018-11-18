namespace FbSdk.Internal
{
    internal class OrderManager
    {
        public void Pay(string productId, string name, int price)
        {
            SdkManager.NativeApi.Pay(productId, name, price);
        }
    }
}