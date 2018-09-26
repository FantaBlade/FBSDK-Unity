namespace FbSdk.Internal
{
    public class OrderManager
    {
        public void Pay(string productId, string name, int price)
        {
            SdkManager.NativeApi.Pay(productId, name, price);
        }
    }
}