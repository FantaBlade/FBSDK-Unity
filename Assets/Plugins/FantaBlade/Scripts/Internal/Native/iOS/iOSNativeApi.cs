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
        private static extern void setLoginDelegate(DelegateMessage callback);
        [DllImport("__Internal")]
        private static extern void setShareDelegate(DelegateMessage callback);
        [DllImport("__Internal")]
        private static extern void setLogoutDelegate(DelegateMessage callback);
        [DllImport("__Internal")]
        private static extern bool isInstalled(int channel);
        [DllImport("__Internal")]
        private static extern void login(int channel);
        [DllImport("__Internal")]
        private static extern void logout();
        [DllImport("__Internal")]
        private static extern void share(int channel,string imagePath, string title, string desc);
        [DllImport("__Internal")]
        private static extern void registerThirdApp(int channel, string appId, string weiboRedirectUrl);
        
        private List<int> registerChannel = new List<int>();

        public void Init()
        {
            setLoginDelegate(DelegateLoginMessageCallback);
            setShareDelegate(DelegateShareMessageCallback);
            setLogoutDelegate(DelegateLogoutMessageCallback);
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
            registerThirdApp(loginChannel, appId, weiboRedirectUrl);
        }

        public void Login(int loginChannel)
        {
            login(loginChannel);
        }

        public void Share(int shareChannel, string imagePath, string title, string desc)
        {
            share(shareChannel, imagePath, title, desc);
        }

        public bool IsInstall(int loginChannel)
        {
            return isInstalled(loginChannel);
        }

        public void Logout()
        {
            logout();
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