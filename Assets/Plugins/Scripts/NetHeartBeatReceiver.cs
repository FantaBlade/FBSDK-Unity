using System;

namespace NetEase
{
    public class NetHeartBeatReceiver : NetEase.InfoReceiver
    {
        private static NetHeartBeatReceiver _instance;
        public static int version = 3;

        public static NetHeartBeatReceiver GetInstance()
        {
            if (_instance == null)
            {
                _instance = new NetHeartBeatReceiver();
                NetSecProtect.registInfoReceiver(_instance);
            }

            return _instance;
        }

        public static event Action<int, string> Receive;

        public void onReceive(int type, string info)
        {
            var handler = Receive;
            if (handler != null) handler(type, info);
        }
    }
}