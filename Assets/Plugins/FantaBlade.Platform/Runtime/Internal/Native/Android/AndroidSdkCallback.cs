#undef UNITY_EDITOR
using System;
using UnityEngine;

namespace FantaBlade.Platform.Internal.Native
{
    #if UNITY_ANDROID && !UNITY_EDITOR
    
    internal class AndroidSdkCallback : AndroidJavaProxy, IAndroidSdkCallback
    {
        public AndroidSdkCallback() : base("com.fantablade.fbsdk.FbSdkListener")
        {
        }

        public void onLoginSuccess(string token)
        {
            RunOnMonoThread(() =>
            {
                SdkManager.Auth.OnSDKLoginFinish(true, token);
            });
        }

        public void onLoginFailure(string msg)
        {
            RunOnMonoThread(() =>
            {
                SdkManager.Auth.OnSDKLoginFinish(false, msg);
            });
        }

        public void onLoginCancel()
        {
            RunOnMonoThread(() =>
            {
                SdkManager.Auth.OnSDKLoginFinish(false, "UserCancel");
            });
        }

        public void onLogoutSuccess()
        {
            RunOnMonoThread(() =>
            {
                SdkManager.Auth.OnSDKLogoutFinish(true);
            });
        }

        public void onPaySuccess()
        {
            RunOnMonoThread(() => { FantaBladePlatform.OnPaySuccess(); });
        }

        public void onPayFailure(string msg)
        {
            RunOnMonoThread(() => { FantaBladePlatform.OnPayFailure(msg); });
        }

        public void onPayCancel()
        {
            RunOnMonoThread(() => { FantaBladePlatform.OnPayCancel(); });
        }

        public void onShareSucceed(string msg)
        {
            RunOnMonoThread(() => { SdkManager.Share.OnShareSucceed(msg); });
        }

        public void onShareFailure(string msg)
        {
            RunOnMonoThread(() =>
            {
                SdkManager.Share.OnShareFailure(msg);
            });
        }

        public void RunOnMonoThread(Action act)
        {
            SdkManager.MonoUpdate.RunOnMonoThread(act);
        }
    }
    
    #endif
}