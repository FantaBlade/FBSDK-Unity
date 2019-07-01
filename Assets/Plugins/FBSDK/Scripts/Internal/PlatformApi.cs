using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace FbSdk.Internal
{
    internal class PlatformApi
    {
        public static readonly string UserCenterHost = "https://test.fantablade.com/";

        private static readonly string _apiHost = "https://api.test.fantablade.com/";
        private static readonly string _version = "v0.1";
        private static readonly string _apiUrl = _apiHost + _version;

        public class WebRequest<TResponse> where TResponse : Response
        {
            public delegate void WebResponseEventHandler(string err, ResponseMetaInfo metaInfo, TResponse response);

            private readonly Uri _uri;

            private WebRequest()
            {
            }

            private WebRequest(string path)
            {
                try
                {
                    _uri = new Uri(string.Join("/", new[] {_apiUrl, path}));
                }
                catch (UriFormatException e)
                {
                    Debug.LogError(e);
                    throw;
                }
            }

            public static implicit operator WebRequest<TResponse>(string path)
            {
                return new WebRequest<TResponse>(path);
            }

            public Coroutine Get(WebResponseEventHandler callback)
            {
                return SdkManager.StartCoroutine(GetCoroutine(callback));
            }

            public Coroutine Post(Dictionary<string, string> form, WebResponseEventHandler callback)
            {
                return SdkManager.StartCoroutine(PostCoroutine(form, callback));
            }

            private IEnumerator GetCoroutine(WebResponseEventHandler callback)
            {
                var request = UnityWebRequest.Get(_uri.AbsoluteUri);
                yield return SendRequest(request, callback);
            }


            private IEnumerator PostCoroutine(Dictionary<string, string> form, WebResponseEventHandler callback)
            {
                var request = UnityWebRequest.Post(_uri.AbsoluteUri, form);
                yield return SendRequest(request, callback);
            }

            private static IEnumerator SendRequest(UnityWebRequest request, WebResponseEventHandler callback)
            {
                request.SetRequestHeader("AccessKeyId", SdkManager.AccessKeyId);
                if (SdkManager.Auth.Token != null) request.SetRequestHeader("Authorization", SdkManager.Auth.Token);

                yield return request.SendWebRequest();
                string err = null;
                var metaInfo = new ResponseMetaInfo(request.responseCode, request.error);
                TResponse response = null;
                if (request.isNetworkError)
                {
                    Debug.LogError(request.error);
                    err = request.error;
                }
                else if (request.isHttpError)
                {
                    Debug.LogError(string.Format("{0} {1}\n{2}", request.responseCode, request.error,
                        request.downloadHandler.text));
                    err = request.error;
                }
                else
                {
                    var result = request.downloadHandler.text;
                    response = JsonUtility.FromJson<TResponse>(result);
                    if (response.code != 0)
                    {
                        err = response.message;
                    }
                }

                if (callback != null) callback(err, metaInfo, response);
            }
        }

        public static class User
        {
            private const string Server = "account";
            private const string Prefix = Server + "/user/";

            public static readonly WebRequest<TokenResponse> Register = Prefix + "register";
            public static readonly WebRequest<TokenResponse> TouristUpgrade = Prefix + "bind/tourist";
            public static readonly WebRequest<TokenResponse> Login = Prefix + "login";
            public static readonly WebRequest<TokenResponse> QuickLogin = Prefix + "quicklogin";
            public static readonly WebRequest<TokenResponse> RefreshToken = Prefix + "refresh/token";
            public static readonly WebRequest<Response> RequestValidateCode = Prefix + "vacode";
            public static readonly WebRequest<Response> RequestValidateCodeForRigister = Prefix + "vacode/register";
            public static readonly WebRequest<Response> RequestValidateCodeForLogin = Prefix + "vacode/login";
        }

        public static class Iap
        {
            private const string Server = "order";
            private const string Prefix = Server + "/iap/";

            public static readonly WebRequest<Response> PurchaseNotify = Prefix + "notify";
        }

        [Serializable]
        public struct ResponseMetaInfo
        {
            /// <summary>
            ///     http status
            /// </summary>
            public long Status;

            /// <summary>
            ///     http error
            /// </summary>
            public string Error;

            public ResponseMetaInfo(long status, string error)
            {
                Status = status;
                Error = error;
            }
        }

        [Serializable]
        public class Response
        {
            /// <summary>
            ///     error code
            /// </summary>
            public int code;

            /// <summary>
            ///     error message
            /// </summary>
            public string message;
        }

        [Serializable]
        public class TokenResponse : Response
        {
            public string token;
        }
    }
}