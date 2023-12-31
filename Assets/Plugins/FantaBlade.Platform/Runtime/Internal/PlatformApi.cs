﻿using System;
using System.Collections;
using System.Collections.Generic;
using FantaBlade.Common;
using UnityEngine;
using UnityEngine.Networking;

namespace FantaBlade.Platform.Internal
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
        public static string UserCenterHost;
        private static string _version;
        private static string _apiUrl;
        private static bool _useBackup;
        private static string _apiUrl2;

        public class WebRequest<TResponse> where TResponse : Response
        {
            public delegate void WebResponseEventHandler(string err, ResponseMetaInfo metaInfo, TResponse response);

            private readonly string _path;
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
            }


            private IEnumerator PostCoroutine(Dictionary<string, string> form, WebResponseEventHandler callback)
            {
                yield return SendRequest(RequestMethod.Post, form, callback);
            }

            private IEnumerator SendRequest(RequestMethod method, Dictionary<string, string> form, WebResponseEventHandler callback)
            {
                var uri = GetUri();
                var request = method == RequestMethod.Post ? UnityWebRequest.Post(uri.AbsoluteUri, form):UnityWebRequest.Get(uri.AbsoluteUri);
                request.SetRequestHeader("AccessKeyId", SdkManager.AccessKeyId);
                if (SdkManager.Auth.Token != null) request.SetRequestHeader("Authorization", SdkManager.Auth.Token);
                Log.Debug(string.Format("{0} request url: {1} Authorization={2}", SdkManager.Ip, request.url, SdkManager.Auth.Token != null? SdkManager.Auth.Token : ""));
                request.timeout = 5;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    SwitchApiHost();
                    Log.Error(string.Format("{0} url: {1} {2}", SdkManager.Ip, request.url, request.error));
                    uri = GetUri();
                    request = method == RequestMethod.Post ? UnityWebRequest.Post(uri.AbsoluteUri, form):UnityWebRequest.Get(uri.AbsoluteUri);
                    request.SetRequestHeader("AccessKeyId", SdkManager.AccessKeyId);
                    if (SdkManager.Auth.Token != null) request.SetRequestHeader("Authorization", SdkManager.Auth.Token);
                    request.timeout = 5;
                    yield return request.SendWebRequest();
                }
                else if (request.result == UnityWebRequest.Result.ProtocolError && request.responseCode == 404)
                {
                    SwitchApiHost();
                    Log.Error(string.Format("url: {0} {1} {2}\n{3}\n Authorization:{4}", request.url, request.responseCode, request.error,
                        request.downloadHandler.text, SdkManager.Auth.Token));
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
                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    SwitchApiHost();
                    Log.Error(string.Format("{0} url: {1} {2}", SdkManager.Ip, request.url, request.error));
                    err = request.error;
                }
                else if (request.result == UnityWebRequest.Result.ProtocolError)
                {
                    SwitchApiHost();
                    Log.Error(string.Format("url: {0} {1} {2}\n{3}", request.url, request.responseCode, request.error,
                        request.downloadHandler.text));
                    err = request.error;
                }
                else
                {
                    var result = request.downloadHandler.text;
                    try
                    {
                        response = JsonUtility.FromJson<TResponse>(result);
                        if (response.code != 0)
                        {
                            err = response.message;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(string.Format("{0} url: {1} parse response error!", SdkManager.Ip, request.url));
                    }
                }

                if (callback != null) callback(err, metaInfo, response);
            }

            private string GetApiUrl()
            {
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
                    if (_path.StartsWith("http"))
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
                    if (_path.StartsWith("http"))
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
            public static readonly WebRequest<TokenResponse> MobileLogin = Prefix + "mobilelogin";
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
            public static readonly WebRequest<IpApiResponse> GetIpApi = "http://ip-api.com/json";
            public static readonly WebRequest<IpCoResponse> GetIpCo = "https://ipapi.co/json";
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

        public class IpApiResponse : Response
        {
            public string countryCode;
            public string query;

            public IpApiResponse()
            {
                code = 0;
            }
        }

        public class IpCoResponse : Response
        {
            public string country_code;
            public string ip;

            public IpCoResponse()
            {
                code = 0;
            }
        }
    }
}