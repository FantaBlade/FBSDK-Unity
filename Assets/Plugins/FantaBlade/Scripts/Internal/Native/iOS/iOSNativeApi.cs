
using System.Collections.Generic;
#if UNITY_IOS
using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace FantaBlade.Internal.Native
{
    internal class iOSNativeApi : INativeApi
    {
        private List<int> registerChannel = new List<int>();

        public void RegisterChannel(int loginChannel, string appId, string weiboRedirectUrl)
        {
            registerChannel.Add(loginChannel);
            Debug.Log("StubNativeApi");
        }

        public void Login(int loginChannel)
        {
            Debug.Log("StubNativeApi");
        }

        public void Share(int shareChannel, string imagePath, string title, string desc)
        {
            Debug.Log("StubNativeApi");
        }

        public bool IsInstall(int loginChannel)
        {
            Debug.Log("StubNativeApi");
            return false;
        }

        public void Logout()
        {
            Debug.Log("StubNativeApi");
        }

        public bool IsChannelRegister(int loginChannel)
        {
            return registerChannel.Contains(loginChannel);
        }
    }
}
#endif