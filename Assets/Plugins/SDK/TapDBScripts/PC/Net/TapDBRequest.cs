using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using TapTap.TapDB.PC.Utils;
using UnityEngine.Networking;

namespace TapTap.TapDB.PC.Net
{
    class TapDBRequest
    {
        private const string DefaultMethod = "POST";

        private const string DefaultContentType = "application/x-www-form-urlencoded";

        private const int DefaultReadWriteTimeout = 5 * 1000;

        private const int DefaultTimeout = 5 * 1000;

        private readonly string _url;

        private readonly Dictionary<string, object> _data;

        public TapDBRequest(string url, Dictionary<string, object> data)
        {
            _url = url;
            _data = data;
        }

        public string Send()
        {
            var request = (HttpWebRequest) WebRequest.Create(this._url);
            request.Method = DefaultMethod;
            request.ContentType = DefaultContentType;
            request.ReadWriteTimeout = DefaultReadWriteTimeout;
            request.Timeout = DefaultTimeout;
            var dataStr = TapDBJson.Serialize(this._data);
            var dataEncodedStr = UnityWebRequest.EscapeURL(dataStr);
            var dataBytes = Encoding.UTF8.GetBytes(dataEncodedStr);
            Stream requestStream = null;
            HttpWebResponse response = null;
            Stream responseStream = null;
            string responseResult = null;
            try
            {
                using (requestStream = request.GetRequestStream())
                {
                    requestStream.Write(dataBytes, 0, dataBytes.Length);
                    response = (HttpWebResponse) request.GetResponse();
                    responseStream = response.GetResponseStream();
                    responseResult =
                        new StreamReader(responseStream ?? throw new InvalidOperationException()).ReadToEnd();
                    TapDBLogger.Debug(_url
                                           + "\n Before Encode: " + dataStr
                                           + "\n After Encode: " + dataEncodedStr
                                           + "\n Response:" + responseResult);
                }
            }
            catch (Exception ex)
            {
                TapDBLogger.Error("Network Error:" + ex.Message);
            }
            finally
            {
                requestStream?.Close();
                responseStream?.Close();
                response?.Close();
                request.Abort();
            }

            return responseResult;
        }
    }
}
