using System;
using System.Collections.Generic;
using FantaBlade.Internal.Jwt;
using UnityEngine;

namespace FantaBlade.Internal
{
    internal class AuthManager
    {
        public bool IsTourist
        {
            get { return _jwt.Payload.type == 1; }
        }

        public string Username
        {
            get { return _jwt.Audiences; }
        }

        private SecurityToken _jwt;

        public string Token
        {
            get
            {
                if (_jwt != null)
                {
                    return _jwt.RawData;
                }

                var token = PlayerPrefs.GetString("FantaBladeSDK_Platform_Token_" + SdkManager.AccessKeyId);
                if (!string.IsNullOrEmpty(token))
                {
                    _jwt = new SecurityToken(token);
                }

                return string.IsNullOrEmpty(token) ? null : token;
            }
            private set
            {
                if (value == null)
                {
                    _jwt = null;
                    PlayerPrefs.DeleteKey("FantaBladeSDK_Platform_Token_" + SdkManager.AccessKeyId);
                }
                else
                {
                    _jwt = new SecurityToken(value);
                    PlayerPrefs.SetString("FantaBladeSDK_Platform_Token_" + SdkManager.AccessKeyId, value);
                    PlayerPrefs.Save();
                }
            }
        }

        public bool IsLoggingIn;
        public bool IsInActivation;

        public void OnLoginCallback(string err, PlatformApi.ResponseMetaInfo meta, PlatformApi.TokenResponse resp)
        {
            SdkManager.Ui.Dialog.HideLoading();
            IsLoggingIn = false;

            if (err != null)
            {
                SdkManager.Ui.Dialog.Show(err, "ok");
            }
            else
            {
                LoginSuccess(resp.token);
            }
        }

        public void QuickLogin()
        {
            var deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
            if (deviceUniqueIdentifier == SystemInfo.unsupportedIdentifier)
            {
                SdkManager.Ui.Dialog.Show("Sorry，the device is temporarily unable to use the quick login feature，please log in after register.", "ok");
            }

            IsLoggingIn = true;

            var form = new Dictionary<string, string>
            {
                {"uniqueDeviceId", deviceUniqueIdentifier}
            };
            SdkManager.Ui.Dialog.ShowLoading();
            PlatformApi.User.QuickLogin.Post(form, OnLoginCallback);
        }

        public void LoginByCache()
        {
            if (string.IsNullOrEmpty(Token))
            {
                SdkManager.Ui.ShowLogin();
            }
            else
            {
                PlatformApi.User.RefreshToken.Get((err, meta, resp) =>
                {
                    if (err != null)
                    {
                        SdkManager.Ui.ShowLogin();
                    }
                    else
                    {
                        LoginSuccess(resp.token);
                    }
                });
            }
        }

        public void Login(string identifier, string password, string countryCode = "")
        {
            IsLoggingIn = true;
            var form = new Dictionary<string, string>
            {
                {"countryCode", countryCode},
                {"identifier", identifier},
                {"password", password}
            };
            SdkManager.Ui.Dialog.ShowLoading();
            PlatformApi.User.Login.Post(form, OnLoginCallback);
        }

        public void CheckIsActication(string activationCode, Action<bool> activatedCallback)
        {
            if (SdkManager.NeedActivation)
            {
                PlatformApi.User.RequestActicationValidate.Get(((err, info, resp) =>
                {
                    if (resp.code != 0)
                    {
                        SdkManager.Ui.ShowActivation();
                    }

                    if (null != activatedCallback)
                    {
                        activatedCallback(resp.code == 0);
                    }
                }));
            }
            else
            {
                if (null != activatedCallback)
                {
                    activatedCallback(true);
                }
            }
        }
        
        public void Activation(string activationCode, Action<bool> callback)
        {
            if (IsInActivation)
            {
                return;
            }
            IsInActivation = true;
            var form = new Dictionary<string, string>
            {
                {"code", activationCode},
            };
            PlatformApi.User.RequestActivation.Post(form, (err, metaInfo, resp)=>
            {
                IsInActivation = false;
                if (! string.IsNullOrEmpty(err))
                {
                    SdkManager.Ui.Dialog.Show(err, "ok");
                }

                if (resp.code == 0)
                {
                    SdkManager.Ui.HideAll();
                    OnLoginSucceed();
                }
                if (null != callback)
                {
                    callback(resp.code == 0);
                }
            });
        }
        
        private void LoginSuccess(string token)
        {
            Token = token;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (SdkManager.UseAndroidNativeApi)
            {
                ((Native.AndroidNativeApi)SdkManager.NativeApi).SetToken(token);
            }
#endif
                CheckIsActication(Token, (ret) =>
                {
                    if (ret)
                    {
                        OnLoginSucceed();
                    }
                });
        }

        private void OnLoginSucceed()
        {
            Api.OnLoginSuccess(Token);
            //效果不好，先屏蔽
            // SdkManager.Ui.ShowNormalUI(NormalUIID.WelcomeBack);
            SdkManager.Ui.ShowFloatingWindow();
            SdkManager.Ui.HideLogin();
        }

        public void Logout()
        {
            Token = null;
            Api.OnLogoutSuccess();
#if UNITY_ANDROID && !UNITY_EDITOR
            if (SdkManager.UseAndroidNativeApi)
            {
                ((Native.AndroidNativeApi)SdkManager.NativeApi).Logout();
            }
#endif
            SdkManager.Ui.HideFloatingWindow();
        }

        public void RequestValidateCode(string countryCode, string mobileNumber, Action<string> callback,
            bool? registered = null)
        {
            var form = new Dictionary<string, string>
            {
                {"countryCode", countryCode},
                {"mobile", mobileNumber},
                {"lang", SdkManager.LanguageString}
            };
            PlatformApi.WebRequest<PlatformApi.Response> request;
            if (!registered.HasValue)
            {
                request = PlatformApi.User.RequestValidateCode;
            }
            else if (registered.Value)
            {
                request = PlatformApi.User.RequestValidateCodeForLogin;
            }
            else
            {
                request = PlatformApi.User.RequestValidateCodeForRigister;
            }

            SdkManager.Ui.Dialog.ShowLoading();
            request.Post(form, (err, meta, resp) =>
            {
                SdkManager.Ui.Dialog.HideLoading();
                if (callback != null) callback(err);
            });
        }

        public void Register(string username, string password, string countryCode, string mobileNumber,
            string vacode)
        {
            IsLoggingIn = true;
            var form = new Dictionary<string, string>
            {
                {"countryCode", countryCode},
                {"mobile", mobileNumber},
                {"password", password},
                {"username", username},
                {"vacode", vacode}
            };
            SdkManager.Ui.Dialog.ShowLoading();
            PlatformApi.User.Register.Post(form, (err, meta, resp) =>
            {
                if (resp.code == 0)
                {
                    SdkManager.Ui.HideLogin();
                }
                OnLoginCallback(err, meta, resp);
            });
        }

        public void CheckValidateCode(string countryCode, string mobileNumber, string vacode, Action<string> callback)
        {
            IsLoggingIn = true;
            var form = new Dictionary<string, string>
            {
                {"countryCode", countryCode},
                {"mobile", mobileNumber},
                {"usage", "reset_password"},
                {"vacode", vacode}
            };
            SdkManager.Ui.Dialog.ShowLoading();
            PlatformApi.User.RequestVacodeValidate.Post(form, (err, metaInfo, resp)=>
            {
                SdkManager.Ui.Dialog.HideLoading();
                IsLoggingIn = false;
                if (! string.IsNullOrEmpty(err))
                {
                    SdkManager.Ui.Dialog.Show(err, "ok");
                }
                else
                {
                    callback(resp.tempTicket);
                }
            });
        }

        public void ResetPassword(string countryCode, string mobileNumber, string newPassword, string tempTicket, Action successCalback)
        {
            IsLoggingIn = true;
            var form = new Dictionary<string, string>
            {
                {"countryCode", countryCode},
                {"mobile", mobileNumber},
                {"newPassword", newPassword},
                {"tempTicket", tempTicket},
            };
            SdkManager.Ui.Dialog.ShowLoading();
            PlatformApi.User.RequestResetPassword.Post(form, (err, metaInfo, resp)=>
            {
                SdkManager.Ui.Dialog.HideLoading();
                IsLoggingIn = false;
                if (! string.IsNullOrEmpty(err))
                {
                    SdkManager.Ui.Dialog.Show(err, "ok");
                }
                else
                {
                    successCalback();
                }
            });
        }
        
        public void TouristUpgrade(string username, string password, string countryCode, string mobileNumber,
            string vacode)
        {
            IsLoggingIn = true;
            var form = new Dictionary<string, string>
            {
                {"countryCode", countryCode},
                {"mobile", mobileNumber},
                {"password", password},
                {"username", username},
                {"vacode", vacode}
            };
            SdkManager.Ui.Dialog.ShowLoading();
            PlatformApi.User.TouristUpgrade.Post(form, (err, meta, resp) =>
            {
                SdkManager.Ui.Dialog.HideLoading();
                IsLoggingIn = false;

                if (err != null)
                {
                    SdkManager.Ui.Dialog.Show(err, "ok");
                }
                else
                {
                    Token = resp.token;
                    SdkManager.Ui.Dialog.Show("Congratulations! You Has been successfully upgraded to an official account!", "ok");
                    SdkManager.Ui.HideGameCenter();
                }
            });
        }

        private static string Base64Encode(string str)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
        }

        private static string Base64Decode(string str)
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }
    }
}