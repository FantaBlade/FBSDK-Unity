using UnityEngine.Rendering;

namespace FantaBlade.Internal.Native
{
    internal interface INativeApi
    {
        void RegisterChannel(int loginChannel, string appId, string weiboRedirectUrl);

        void Login(int loginChannel);

        void Share(int shareChannel, string imagePath, string title, string desc);

        bool IsInstall(int loginChannel);

        void Logout();

        bool IsChannelRegister(int loginChannel);
    }
}