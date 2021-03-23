using System;
using UnityEngine;

namespace FantaBlade.Internal.Native
{
    public class StubNativeApi : INativeApi
    {
        public void RegisterChannel(int loginChannel, string appId, string weiboRedirectUrl)
        {
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
    }
}