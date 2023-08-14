#if UNITY_IOS

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace FantaBlade.Internal.Native
{
    internal class iOSNativeApi : INativeApi
    {
        private delegate void DelegateMessage(string token, bool success);
        
        [DllImport("__Internal")]
        private static extern void fbsdk_setLoginDelegate(DelegateMessage callback);
        [DllImport("__Internal")]
        private static extern void fbsdk_setShareDelegate(DelegateMessage callback);
        [DllImport("__Internal")]
        private static extern void fbsdk_setLogoutDelegate(DelegateMessage callback);
        [DllImport("__Internal")]
        private static extern bool fbsdk_isInstalled(int channel);
        [DllImport("__Internal")]
        private static extern bool fbsdk_isSupportAuth(int channel);
        [DllImport("__Internal")]
        private static extern void fbsdk_login(int channel);
        [DllImport("__Internal")]
        private static extern void fbsdk_logout();
        [DllImport("__Internal")]
        private static extern void fbsdk_share(int channel,string imagePath, string title, string desc);
        [DllImport("__Internal")]
        private static extern void fbsdk_registerThirdApp(int channel, string appId, string weiboRedirectUrl);
        
        private List<int> registerChannel = new List<int>();

        public void Init()
        {
            fbsdk_setLoginDelegate(DelegateLoginMessageCallback);
            fbsdk_setShareDelegate(DelegateShareMessageCallback);
            fbsdk_setLogoutDelegate(DelegateLogoutMessageCallback);
        }

        [MonoPInvokeCallback(typeof(DelegateMessage))]
        private static void DelegateLoginMessageCallback(string token, bool success)
        {
            RunOnMonoThread(() =>
            {
                SdkManager.Auth.OnSDKLoginFinish(success, token);
            });
        }
        
        [MonoPInvokeCallback(typeof(DelegateMessage))]
        private static void DelegateShareMessageCallback(string token, bool success)
        {
            if (success)
            {
                RunOnMonoThread(() => { SdkManager.Share.OnShareSucceed(token); });
            }
            else
            {
                RunOnMonoThread(() => { SdkManager.Share.OnShareFailure(token); });
            }
        }
        
        [MonoPInvokeCallback(typeof(DelegateMessage))]
        private static void DelegateLogoutMessageCallback(string token, bool success)
        {
            RunOnMonoThread(() =>
            {
                SdkManager.Auth.OnSDKLogoutFinish(true);
            });
        }
        
        public void RegisterChannel(int loginChannel, string appId, string weiboRedirectUrl)
        {
            registerChannel.Add(loginChannel);
            fbsdk_registerThirdApp(loginChannel, appId, weiboRedirectUrl);
        }

        public void Login(int loginChannel)
        {
            fbsdk_login(loginChannel);
        }

        public void Share(int shareChannel, string imagePath, string title, string desc)
        {
            fbsdk_share(shareChannel, imagePath, title, desc);
        }

        public bool IsInstall(int loginChannel)
        {
            return fbsdk_isInstalled(loginChannel);
        }

        public bool IsSupportAuth(int loginChannel)
        {
            return fbsdk_isSupportAuth(loginChannel);
        }

        public void Logout()
        {
            fbsdk_logout();
        }

        public bool IsChannelRegister(int loginChannel)
        {
            return registerChannel.Contains(loginChannel);
        }

        private static void RunOnMonoThread(Action act)
        {
            SdkManager.MonoUpdate.RunOnMonoThread(act);
        }
    }
}
#endif