using FantaBlade.Internal.Native;

namespace FantaBlade.Internal
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
            Api.OnShareSucceed(msg);
        }
        
        public void OnShareFailure(string msg)
        {
            if (msg == "UserCancel")
            {
                Api.OnShareCancel(msg);
            }
            else
            {
                Api.OnShareFailure(msg);
            }
        }
        
    }
}