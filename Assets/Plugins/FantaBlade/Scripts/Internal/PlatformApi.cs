using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace FantaBlade.Internal
{
    internal class PlatformApi
    {
        public static readonly Dictionary<PublishRegion, Dictionary<string, string>> UrlConfigs =
            new Dictionary<PublishRegion, Dictionary<string, string>>
            {
                {
                    PublishRegion.China, new Dictionary<string, string>
                    {
                        {"UserCenterHost", "https://test.fantablade.com/"},
                        {"ApiHost", "https://api.test.fantablade.com/"},
                        {"ApiVersion", "v0.1"}
                    }
                },
                {
                    PublishRegion.SoutheastAsia, new Dictionary<string, string>
                    {
                        {"UserCenterHost", "https://sea.fantablade.com/"},
                        {"ApiHost", "https://api.sea.fantablade.com/"},
                        {"ApiVersion", "v1"}
                    }
                }
            };

        public static void SetRegion(PublishRegion region)
        {
            var config = UrlConfigs[region];
            UserCenterHost = config["UserCenterHost"];
            _apiHost = config["ApiHost"];
            _version = config["ApiVersion"];
            _apiUrl = _apiHost + _version;
        }


        public static string UserCenterHost;

        private static string _apiHost;
        private static string _version;
        private static string _apiUrl;


        public class WebRequest<TResponse> where TResponse : Response
        {
            public delegate void WebResponseEventHandler(string err, ResponseMetaInfo metaInfo, TResponse response);

            private readonly string _path;
            private Uri _uri;

            private WebRequest()
            {
                // 直接使用 string 隐式转换实例化
            }

            private WebRequest(string path)
            {
                _path = path;
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
                var uri = GetUri(_path);
                var request = UnityWebRequest.Get(uri.AbsoluteUri);
                yield return SendRequest(request, callback);
            }


            private IEnumerator PostCoroutine(Dictionary<string, string> form, WebResponseEventHandler callback)
            {
                var uri = GetUri(_path);
                var request = UnityWebRequest.Post(uri.AbsoluteUri, form);
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
                    Log.Error(string.Format("url: {0} {1}", request.url, request.error));
                    err = request.error;
                }
                else if (request.isHttpError)
                {
                    Log.Error(string.Format("url: {0} {1} {2}\n{3}", request.url, request.responseCode, request.error,
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

            private Uri GetUri(string path)
            {
                if (_uri == null)
                {
                    _uri = new Uri(string.Join("/", new[] {_apiUrl, path}));
                }

                return _uri;
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

        public static class Feedback
        {
            private const string Server = "account";
            private const string Prefix = Server + "/feedback/";

            public static readonly WebRequest<Response> Submit = Prefix + "submit";
        }

        public static class Util
        {
            private const string Server = "account";
            private const string Prefix = Server + "/util/";

            public static readonly WebRequest<IpInfoResponse> GetIpInfo = Prefix + "getIpInfo";
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

        [Serializable]
        public class IpInfoResponse : Response
        {
            public string countryCode;
        }
    }
}