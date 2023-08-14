using System;
using FantaBlade.Common;
using FantaBlade.Platform.Internal;

namespace FantaBlade.Platform
{
    public class FantaBladeShare
    {
        // 第三方分享
        public static class ShareChannel
        {
            public const int WECHAT_SESSION = 1;
            public const int WECHAT_TIMELINE = 2;
            public const int WECHAT_FAVORITE = 3;
            public const int QQ_SESSION = 4;
            public const int WEIBO = 5;
        }

        private static readonly ShareManager ShareManager = new ShareManager();
        
        public static void Share(int shareChannel, string imagePath, string title, string desc)
        {
            ShareManager.ShareImage(shareChannel, imagePath, title, desc);
        }
        
        internal static void OnShareSucceed(string msg)
        {
            Log.Debug("OnShareSucceed" + msg);
            var handler = ShareSucceed;
            if (handler != null) handler(msg);
        }

        internal static void OnShareFailure(string msg)
        {
            Log.Debug("OnShareFailure" + msg);
            var handler = ShareFailure;
            if (handler != null) handler(msg);
        }

        internal static void OnShareCancel(string msg)
        {
            Log.Debug("OnShareCancel" + msg);
            var handler = ShareCancel;
            if (handler != null) handler(msg);
        }
        
        public static event Action<string> ShareSucceed;
        public static event Action<string> ShareFailure;
        public static event Action<string> ShareCancel;
    }
}