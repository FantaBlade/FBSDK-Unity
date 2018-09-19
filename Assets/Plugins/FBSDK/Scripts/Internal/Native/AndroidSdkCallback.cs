#undef UNITY_EDITOR
using UnityEngine;

namespace FbSdk.Internal.Native
{
    #if UNITY_ANDROID && !UNITY_EDITOR
    
    public class AndroidSdkCallback : AndroidJavaProxy, IAndroidSdkCallback
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
            Sdk.OnPaySuccess();
        }

        public void onPayFailure(string msg)
        {
            Sdk.OnPayFailure(msg);
        }

        public void onPayCancel()
        {
            Sdk.OnPayCancel();
        }
    }
    
    #endif
}