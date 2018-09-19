using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace FbSdk.Internal
{
    internal class PlatformApi
    {
        private static readonly string _host = "https://test.api.fantablade.com";
        private static readonly string _version = "v0.1";

        public class WebRequest<TResponse> where TResponse : Response
        {
            private readonly Uri _uri;

            private WebRequest()
            {
            }

            private WebRequest(string path)
            {
                try
                {
                    _uri = new Uri(string.Join("/", new[] {_host, _version, path}));
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

            public IEnumerator Get(Action<string, TResponse> callback)
            {
                var request = UnityWebRequest.Get(_uri.AbsoluteUri);
                yield return SendRequest(request, callback);
            }

            public IEnumerator Post(Dictionary<string, string> form, Action<string, TResponse> callback)
            {
                var request = UnityWebRequest.Post(_uri.AbsoluteUri, form);
                yield return SendRequest(request, callback);
            }

            private IEnumerator SendRequest(UnityWebRequest request, Action<string, TResponse> callback)

            {
                request.SetRequestHeader("AccessKeyId", SdkManager.AccessKeyId);
                if (SdkManager.Auth.Token != null)
                {
                    request.SetRequestHeader("Authorization", SdkManager.Auth.Token);
                }

                yield return request.SendWebRequest();
                if (request.isNetworkError)
                {
                    Debug.LogError(request.error);
                    if (callback != null) callback(request.error, default(TResponse));
                }
                else if (request.isHttpError)
                {
                    Debug.LogError(string.Format("{0} {1}\n{2}", request.responseCode, request.error,
                        request.downloadHandler.text));
                    if (callback != null) callback(request.error, default(TResponse));
                }
                else
                {
                    var result = request.downloadHandler.text;
                    var response = JsonUtility.FromJson<TResponse>(result);
                    if (response.code != 0)
                    {
                        if (callback != null) callback(response.message, default(TResponse));
                    }
                    else
                    {
                        if (callback != null) callback(null, response);
                    }
                }
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

        [Serializable]
        public class Response
        {
            /// <summary>
            ///     http status
            /// </summary>
            public string status;

            /// <summary>
            ///     http error
            /// </summary>
            public string error;

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