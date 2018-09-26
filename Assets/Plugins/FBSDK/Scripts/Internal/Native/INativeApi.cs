namespace FbSdk.Internal.Native
{
    internal interface INativeApi
    {
        void Init();
        
        void Pay(string productId, string name, int price);
    }
}