namespace FbSdk.Internal.Native
{
    internal interface INativeApi
    {
        void Init();
        
        void Pay(string commodityName, string commodityInfo, int orderAmount);
    }
}