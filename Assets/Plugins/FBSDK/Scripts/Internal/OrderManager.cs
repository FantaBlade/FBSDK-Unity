namespace FbSdk.Internal
{
    public class OrderManager
    {
        public void Pay(string commodityName, string commodityInfo, int orderAmount)
        {
            SdkManager.NativeApi.Pay(commodityInfo, commodityInfo, orderAmount);
        }
    }
}