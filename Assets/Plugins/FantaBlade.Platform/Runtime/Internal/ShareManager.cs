using FantaBlade.Platform.Internal.Native;

namespace FantaBlade.Platform.Internal
{
    internal class ShareManager
    {
        private INativeApi _nativeApi;

        public void SetNativeAPI(INativeApi api)
        {
            _nativeApi = api;
        }

        public void ShareImage(int shareChannel, string imagePath, string title, string desc)
        {
            _nativeApi.Share(shareChannel, imagePath, title, desc);
        }
        
        public void OnShareSucceed(string msg)
        {
            FantaBladeShare.OnShareSucceed(msg);
        }
        
        public void OnShareFailure(string msg)
        {
            if (msg == "UserCancel")
            {
                FantaBladeShare.OnShareCancel(msg);
            }
            else
            {
                FantaBladeShare.OnShareFailure(msg);
            }
        }
        
    }
}