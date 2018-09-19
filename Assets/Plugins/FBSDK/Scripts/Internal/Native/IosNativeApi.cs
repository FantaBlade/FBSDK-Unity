#if UNITY_IOS
namespace FbSdk.Internal.Native
{
    public class IosNativeApi : INativeApi
    {
        public void Init()
        {
        }

        public void Pay(string commodityName, string commodityInfo, int orderAmount)
        {
        }
    }
}
#endif