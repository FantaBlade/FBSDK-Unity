using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantaBlade.Internal.Native
{
    public class StubNativeApi : INativeApi
    {
        private List<int> registerChannel = new List<int>();

        public void Init()
        {
        }

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
        
        public bool IsSupportAuth(int loginChannel)
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