using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace FantaBlade.Internal
{
    internal class PlatformApi
    {
        private static readonly Dictionary<PublishRegion, Dictionary<string, string>> UrlConfigs =
            new Dictionary<PublishRegion, Dictionary<string, string>>
            {
                {
                    PublishRegion.LocalDev, new Dictionary<string, string>
                    {
                        {"UserCenterHost", "http://www.fantablade.cn/"},
                        {"ApiHost", "api.fantablade.cn"},
                        {"ApiHost2", "api-protect.fantablade.cn"},
                        {"ApiVersion", "v0.1"},
                        {"Protocol", "http"},
                        {"ApiIp", "172.26.194.121"}
                    }
                },
                {
                    PublishRegion.China, new Dictionary<string, string>
                    {
                        {"UserCenterHost", "https://cn.fantablade.com/"},
                        {"ApiHost", "api.cn.fantablade.com"},
                        {"ApiHost2", "api.protect.fantablade.com"},
                        {"ApiVersion", "v1"},
                        {"Protocol", "https"},
                        {"ApiIp", "127.0.0.1"}
                    }
                },
                {
                    PublishRegion.TW, new Dictionary<string, string>
                    {
                        {"UserCenterHost", "https://tw.fantablade.com/"},
                        {"ApiHost", "api.tw.fantablade.com"},
                        {"ApiHost2", "api.tw.fantablade.com"},
                        {"ApiVersion", "v1"},
                        {"Protocol", "https"},
                        {"ApiIp", "127.0.0.1"}
                    }
                },
                {
                    PublishRegion.SoutheastAsia, new Dictionary<string, string>
                    {
                        {"UserCenterHost", "https://sea.fantablade.com/"},
                        {"ApiHost", "api.sea.fantablade.com"},
                        {"ApiHost2", "api.sea.fantablade.com"},
                        {"ApiVersion", "v1"},
                        {"Protocol", "https"},
                        {"ApiIp", "127.0.0.1"}
                    }
                }
            };

        public static void SetRegion(PublishRegion region)
        {
            var config = UrlConfigs[region];
            UserCenterHost = config["UserCenterHost"];
            _protocol = config["Protocol"];
            _apiHost = config["ApiHost"];
            _apiHost2 = config["ApiHost2"];
            _apiIp = config["ApiIp"];
            _version = config["ApiVersion"];
            _apiUrl = _protocol + "://" + _apiHost + "/" + _version;
            _apiUrl2 = _protocol + "://" + _apiHost2 + "/" + _version;
        }

        public static void SwitchApiHost()
        {
            _useBackup = !_useBackup;
        }


        private static string _protocol;
        private static string _apiHost;
        private static string _apiHost2;
        private static string _apiIp;
        public static string UserCenterHost;
        private static string _version;
        private static string _apiUrl;
        private static bool _useBackup;
        private static string _apiUrl2;

        public class WebRequest<TResponse> where TResponse : Response
        {
            public delegate void WebResponseEventHandler(string err, ResponseMetaInfo metaInfo, TResponse response);

            private readonly string _path;
            //private bool _useIp;
            private Uri _uri;
            private Uri _uri2;

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
            internal enum RequestMethod
            {
                Get,
                Post
            }
            private IEnumerator GetCoroutine(WebResponseEventHandler callback)
            {
                yield return SendRequest(RequestMethod.Get, null, callback);
                /*
                var request = (HttpWebRequest) WebRequest.Create(uri.AbsoluteUri);
                request.Method = "GET";
                SetRequest(request);
                SendRequest(request, callback);
                yield break;
                */
            }


            private IEnumerator PostCoroutine(Dictionary<string, string> form, WebResponseEventHandler callback)
            {
                yield return SendRequest(RequestMethod.Post, form, callback);
                /*
                var request = (HttpWebRequest) WebRequest.Create(uri.AbsoluteUri);
                request.Method = "POST";
                SetRequest(request);
                //set all Headers before GetRequestStream()
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] postBytes = UnityWebRequest.SerializeSimpleForm(form);
                request.ContentLength = postBytes.Length;
                Stream postStream = null;
                try
                {
                    postStream = request.GetRequestStream();
                    postStream.Write(postBytes, 0, postBytes.Length);
                    postStream.Close();
                    SendRequest(request, callback);
                }
                catch (Exception e)
                {
                    Log.Warning(e);
                    if (postStream != null)
                    {
                        postStream.Close();
                    }
                    var err = e.Message;
                    var metaInfo = new ResponseMetaInfo(0, err);
                    if (callback != null) callback(err, metaInfo, null);
                }
                yield break;
                */
            }

            // private void SetRequest(HttpWebRequest request)
            // {
            //     //if (_useIp)
            //     //{
            //     //    request.Host = _apiHost;
            //     //}
            //     request.Headers.Add("AccessKeyId", SdkManager.AccessKeyId);
            //     if (SdkManager.Auth.Token != null) request.Headers.Add("Authorization", SdkManager.Auth.Token);
            // }

            // private static void SendRequest(HttpWebRequest request, WebResponseEventHandler callback)
            // {
            //     string err = null;
            //     var metaInfo = new ResponseMetaInfo(0, "");
            //     TResponse tResponse = null;
            //     try
            //     {
            //         var response = (HttpWebResponse)request.GetResponse();
            //         TextReader reader = new StreamReader(response.GetResponseStream());
            //         var result = reader.ReadToEnd();
            //         tResponse = JsonUtility.FromJson<TResponse>(result);
            //         if (tResponse.code != 0)
            //         {
            //             err = tResponse.message;
            //         }
            //         metaInfo.Status = Convert.ToInt64(response.StatusCode);
            //         metaInfo.Error = err;
            //     }
            //     catch (Exception e)
            //     {
            //         Log.Warning(e);
            //         err = e.Message;
            //     }
            //     if (callback != null) callback(err, metaInfo, tResponse);
            //     // var uri = GetUri(_path);
            //     // var request = UnityWebRequest.Post(uri.AbsoluteUri, form);
            //     // yield return SendRequest(RequestMethod.Post, form, callback);
            // }

            private IEnumerator SendRequest(RequestMethod method, Dictionary<string, string> form, WebResponseEventHandler callback)
            {
                var uri = GetUri();
                var request = method == RequestMethod.Post ? UnityWebRequest.Post(uri.AbsoluteUri, form):UnityWebRequest.Get(uri.AbsoluteUri);
                // Log.Debug("AccessKeyId:"+SdkManager.AccessKeyId);
                // Log.Debug("Authorization:"+SdkManager.Auth.Token);
                request.SetRequestHeader("AccessKeyId", SdkManager.AccessKeyId);
                if (SdkManager.Auth.Token != null) request.SetRequestHeader("Authorization", SdkManager.Auth.Token);
                request.timeout = 5;
                yield return request.SendWebRequest();
                if (request.isNetworkError)
                {
                    SwitchApiHost();
#if UNITY_EDITOR
                    Log.Error(string.Format("url: {0} {1}", request.url, request.error));
#else
                    Log.Warning(string.Format("url: {0} {1}", request.url, request.error));
#endif
                    uri = GetUri();
                    request = method == RequestMethod.Post ? UnityWebRequest.Post(uri.AbsoluteUri, form):UnityWebRequest.Get(uri.AbsoluteUri);
                    request.SetRequestHeader("AccessKeyId", SdkManager.AccessKeyId);
                    if (SdkManager.Auth.Token != null) request.SetRequestHeader("Authorization", SdkManager.Auth.Token);
                    request.timeout = 5;
                    yield return request.SendWebRequest();
                }
                else if (request.isHttpError && request.responseCode == 404)
                {
                    SwitchApiHost();
#if UNITY_EDITOR
                    Log.Error(string.Format("url: {0} {1} {2}\n{3}\nAuthorization:{4}", request.url, request.responseCode, request.error,
                        request.downloadHandler.text, SdkManager.Auth.Token));
#else
                    Log.Warning(string.Format("url: {0} {1} {2}\n{3}", request.url, request.responseCode, request.error,
                        request.downloadHandler.text));
#endif
                    uri = GetUri();
                    request = method == RequestMethod.Post ? UnityWebRequest.Post(uri.AbsoluteUri, form):UnityWebRequest.Get(uri.AbsoluteUri);
                    request.SetRequestHeader("AccessKeyId", SdkManager.AccessKeyId);
                    if (SdkManager.Auth.Token != null) request.SetRequestHeader("Authorization", SdkManager.Auth.Token);
                    request.timeout = 5;
                    yield return request.SendWebRequest();
                }
                string err = null;
                var metaInfo = new ResponseMetaInfo(request.responseCode, request.error);
                TResponse response = null;
                if (request.isNetworkError)
                {
                    SwitchApiHost();
#if UNITY_EDITOR
                    Log.Error(string.Format("url: {0} {1}", request.url, request.error));
#else
                    Log.Warning(string.Format("url: {0} {1}", request.url, request.error));
#endif
                    err = request.error;
                }
                else if (request.isHttpError)
                {
                    SwitchApiHost();
#if UNITY_EDITOR
                    Log.Error(string.Format("url: {0} {1} {2}\n{3}", request.url, request.responseCode, request.error,
                        request.downloadHandler.text));
#else
                    Log.Warning(string.Format("url: {0} {1} {2}\n{3}", request.url, request.responseCode, request.error,
                        request.downloadHandler.text));
#endif
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

            private string GetApiUrl()
            {
                /*
                try
                {
                    Dns.GetHostAddresses(_apiHost);
                }
                catch (SocketException e)
                {
                    Log.Warning(e);
                    _useIp = true;
                    return _protocol + "://" + _apiIp + "/" + _version;
                }

                _useIp = false;
                */
                return _protocol + "://" + _apiHost + "/" + _version;
            }
            
            private string GetCurrentPlatform()
            {
#if UNITY_ANDROID
                return "Android";
#elif UNITY_IOS
                return "IOS";
#elif UNITY_PS4
                return "PS4";
#elif UNITY_XBOXONE
                return "XBox";
#elif UNITY_STANDALONE || UNITY_WEBGL
                return "PC";
#else
                return (Input.touchSupported ? "Touch" : "PC");
#endif
            }

            private Uri GetUri()
            {
                if (_uri == null)
                {
                    if (_path.StartsWith("http://"))
                    {
                        _uri = new Uri(_path);
                    }
                    else
                    {
                        _uri = new Uri(string.Join("/", new[] {_apiUrl, _path})+"?lang="+SdkManager.Localize.GetLanguageName()+"&ref="+GetCurrentPlatform());
                    }
                }
                if (_uri2 == null)
                {
                    if (_path.StartsWith("http://"))
                    {
                        _uri2 = new Uri(_path);
                    }
                    else
                    {
                        _uri2 = new Uri(string.Join("/", new[] {_apiUrl2, _path})+"?lang="+SdkManager.Localize.GetLanguageName()+"&ref="+GetCurrentPlatform());
                    }
                }
                return _useBackup?_uri2:_uri;
            }
        }

        public static class User
        {
            private const string Server = "account";
            private const string Prefix = Server + "/user/";

            public static readonly WebRequest<TokenResponse> Register = Prefix + "register";
            public static readonly WebRequest<TokenResponse> TouristUpgrade = Prefix + "bind/tourist";
            public static readonly WebRequest<CertificationInfoResponse> GetCertification = Prefix + "certification/fetch";
            public static readonly WebRequest<Response> VerifyAge = Prefix + "certification/update";
            // public static readonly WebRequest<AntiIndulgenceInfoResponse> GetAntiIndulgence = Prefix + "anti_indulgence";
            public static readonly WebRequest<TokenResponse> Login = Prefix + "login";
            public static readonly WebRequest<TokenResponse> QuickLogin = Prefix + "quicklogin";
            public static readonly WebRequest<TokenResponse> RefreshToken = Prefix + "refresh/token";
            public static readonly WebRequest<Response> RequestValidateCode = Prefix + "vacode";
            public static readonly WebRequest<Response> RequestValidateCodeForRigister = Prefix + "vacode/register";
            public static readonly WebRequest<Response> RequestValidateCodeForLogin = Prefix + "vacode/login";
            public static readonly WebRequest<Response> RequestResetPassword = Prefix + "reset/password";
            public static readonly WebRequest<TempTicketResponse> RequestVacodeValidate = Prefix + "vacode/validate";
            public static readonly WebRequest<Response> RequestActivation = Prefix + "activation";
            public static readonly WebRequest<Response> RequestActicationValidate = Prefix + "activation/validate";
            public static readonly WebRequest<TokenResponse> LoginThird = Prefix + "thirdlogin";
            public static readonly WebRequest<TokenResponse> CancelAccount = Prefix + "deleteUser";
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            public static readonly WebRequest<Response> FakePay = "sandbox/order/app/pay";
#endif
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
            public static readonly WebRequest<IpJsonResponse> GetIpJson = "http://ip-api.com/json";
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
        public class AntiIndulgenceInfoResponse : Response
        {
            public bool antiIndulgence;
        }

        [Serializable]
        public class Certification
        {
            public long id;
            public string createTime;
            public string updateTime;
            public long userId;
            public string realName;
            public string identificationCard;
        }
        
        [Serializable]
        public class CertificationInfoResponse : Response
        {
            public int code;
            public bool antiIndulgence;
            public int age;
            public Certification certification;
        }

        [Serializable]
        public class IpInfoResponse : Response
        {
            public string countryCode;
        }

        public class TempTicketResponse : Response
        {
            public string tempTicket;
        }

        public class IpJsonResponse : Response
        {
            public string countryCode;

            public IpJsonResponse()
            {
                code = 0;
            }
        }
    }
}