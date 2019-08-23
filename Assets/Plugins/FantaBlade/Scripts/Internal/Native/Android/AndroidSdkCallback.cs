#undef UNITY_EDITOR
using UnityEngine;

namespace FantaBlade.Internal.Native
{
    #if UNITY_ANDROID && !UNITY_EDITOR
    
    internal class AndroidSdkCallback : AndroidJavaProxy, IAndroidSdkCallback
    {
        public AndroidSdkCallback() : base("com.fantablade.fbsdk.FbSdkListener")
        {
        }

        public void onLoginSuccess(string token)
        {
            
        }

        public void onLoginFailure(string msg)
        {
        }

        public void onLoginCancel()
        {
        }

        public void onLogoutSuccess()
        {
        }

        public void onPaySuccess()
        {
            Api.OnPaySuccess();
        }

        public void onPayFailure(string msg)
        {
            Api.OnPayFailure(msg);
        }

        public void onPayCancel()
        {
            Api.OnPayCancel();
        }
    }
    
    #endif
}