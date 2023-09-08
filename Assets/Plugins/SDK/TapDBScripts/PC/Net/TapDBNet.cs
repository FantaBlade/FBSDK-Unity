using System;
using System.Collections.Generic;
using System.Threading;
using TapTap.TapDB.PC.Utils;


namespace TapTap.TapDB.PC.Net
{
    class TapDBNet
    {
        private const string SuccessResponse = "1";
        private const int MaxWaitingRequests = 500;
        private const int MinRetryTime = 3000;
        
        private static TapDBNet _instance;
        
        private readonly object _locker = new object();

        private readonly List<TapDBRequest> _requests = new List<TapDBRequest>();
        private readonly EventWaitHandle _waitHandle = new AutoResetEvent(false);

        public static TapDBNet Instance => _instance ?? (_instance = new TapDBNet());

        private TapDBNet()
        {
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    if (_requests.Count > 0)
                    {
                        StartRequestSendData();
                    }
                    else
                    {
                        _waitHandle.WaitOne();
                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public void Send(string url, Dictionary<string, object> data)
        {
            lock(_locker)
            {
                if (_requests.Count > MaxWaitingRequests)
                {
                    TapDBLogger.Error("Too Many Requests.");
                    return;
                }
                _requests.Add(new TapDBRequest(
                    url,
                    data
                ));
            }
            _waitHandle.Set();
        }
        
        private void StartRequestSendData()
        {
            if (_requests.Count <= 0) return;
            TapDBRequest request;
            lock(_locker)
            {
                request = _requests[0];
            }
            if (request == null) return;
            var triedTimes = 0;
            while (!SuccessResponse.Equals(request.Send()))
            {
                Thread.Sleep((int) (Math.Pow(2, triedTimes) * MinRetryTime + new Random().Next(MinRetryTime)));
                triedTimes++;
            }
            lock(_locker)
            {
                _requests.RemoveAt(0);
            }
        }
    }
}
