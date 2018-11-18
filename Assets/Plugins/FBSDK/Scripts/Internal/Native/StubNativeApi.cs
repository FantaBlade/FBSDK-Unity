namespace FbSdk.Internal.Native
{
    internal class StubNativeApi : INativeApi
    {
        public void Init()
        {
            Sdk.OnInitializeSuccess();
        }

        public void Pay(string productId, string name, int price)
        {
        }
    }
}