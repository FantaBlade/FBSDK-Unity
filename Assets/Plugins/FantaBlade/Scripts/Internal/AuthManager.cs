using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FantaBlade.Internal.Jwt;
using UnityEngine;

namespace FantaBlade.Internal
{
    internal class AuthManager
    {
        public bool IsTourist
        {
            get { return _jwt == null || _jwt.Payload.type == 1; }
        }

        public string Username
        {
            get { return _jwt != null ? _jwt.Audiences : ""; }
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
                // Debug.Log("get?  "+PlayerPrefs.GetString("FantaBladeSDK_Platform_Token_" ,""));
                if (!string.IsNullOrEmpty(token))
                {
                    _jwt = new SecurityToken(token);
                }

                // Debug.Log("get "+token);
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
                    // PlayerPrefs.SetString("FantaBladeSDK_Platform_Token_" , PlayerPrefs.GetString("FantaBladeSDK_Platform_Token_" + SdkManager.AccessKeyId, ""));
                    PlayerPrefs.SetString("FantaBladeSDK_Platform_Token_" + SdkManager.AccessKeyId, value);
                    // Debug.Log("set "+value);
                    PlayerPrefs.Save();
                }
            }
        }

        public bool IsVerify;
        public int Age = 0;
        public bool IsLoggingIn;
        public bool IsInActivation;
        public int curLoginChannel;

        public void OnLoginCallback(string err, PlatformApi.ResponseMetaInfo meta, PlatformApi.TokenResponse resp)
        {
            SdkManager.Ui.Dialog.HideLoading();
            IsLoggingIn = false;

            if (err != null)
            {
                mobileAuthTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                SdkManager.Ui.ShowLogin();
                SdkManager.Ui.Dialog.Show(err, "ok");
            }
            else
            {
                LoginSuccess(resp.token);
            }
        }

        public void QuickLogin()
        {
            if (SdkManager.Auth.IsLoggingIn)
            {
                return;
            }
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
            PlayerPrefs.SetInt("loginChannel", Api.LoginChannel.CHANNEL_OFFICIAL);
            SdkManager.Ui.Dialog.ShowLoading();
            PlatformApi.User.QuickLogin.Post(form, OnLoginCallback);
        }

        public void LoginByCache(bool useQuickLoginFirst = false)
        {
            if (string.IsNullOrEmpty(Token))
            {
                if (useQuickLoginFirst)
                {
                    QuickLogin();
                }
                else
                {
                    SdkManager.Ui.ShowLogin();
                }
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
                        // Debug.Log("refresh token:"+resp.token);
                        LoginSuccess(resp.token);
                    }
                });
            }
        }

        public void Login(string identifier, string password, string countryCode = "")
        {
            IsLoggingIn = true;
            PlayerPrefs.SetInt("loginChannel", Api.LoginChannel.CHANNEL_OFFICIAL);
            var form = new Dictionary<string, string>
            {
                {"countryCode", countryCode},
                {"identifier", identifier},
                {"password", password}
            };
            SdkManager.Ui.Dialog.ShowLoading();
            PlatformApi.User.Login.Post(form, OnLoginCallback);
        }

        public void LoginByThird(string authCode, int loginChannel)
        {
            IsLoggingIn = true;
            var form = new Dictionary<string, string>
            {
                {"code", authCode},
                {"type", loginChannel + ""}
            };
            SdkManager.Ui.Dialog.ShowLoading();
            PlatformApi.User.LoginThird.Post(form, OnThirdLoginCallback);
        }
        
        public void MobileLogin(string authCode)
        {
            IsLoggingIn = true;
            var form = new Dictionary<string, string>
            {
                {"code", authCode}
            };
            SdkManager.Ui.Dialog.ShowLoading();
            PlatformApi.User.MobileLogin.Post(form, OnLoginCallback);
        }
        public void CancelAccount(string name, string idcard, string mobile, string code, PlatformApi.WebRequest<PlatformApi.TokenResponse>.WebResponseEventHandler callback)
        {
            var form = new Dictionary<string, string>
            {
                {"mobile", mobile},
                {"vacode", code},
                {"realName", name},
                {"idCard", idcard},
            };
            SdkManager.Ui.Dialog.ShowLoading();
            PlatformApi.User.CancelAccount.Post(form, callback);
        }

        public void OnThirdLoginCallback(string err,
            PlatformApi.ResponseMetaInfo meta,
            PlatformApi.TokenResponse resp)
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
        
        public void LoginThird(int channel)
        {
            // 获取第三方access_token/auth_code
            curLoginChannel = channel;
            PlayerPrefs.SetInt("loginChannel", channel);
            SdkManager.NativeApi.Login(curLoginChannel);
        }

        public void CheckIsActication(string activationCode, Action<bool> activatedCallback)
        {
            if (Api.NeedActivation)
            {
                SdkManager.Ui.Dialog.ShowLoading();
                PlatformApi.User.RequestActicationValidate.Get(((err, info, resp) =>
                {
                    SdkManager.Ui.Dialog.HideLoading();
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
            Age = 0;
            IsVerify = false;
            GetCertification();
        }

        private void GetCertification()
        {
            PlatformApi.User.GetCertification.Get((err, meta, resp) =>
            {
                IsVerify = (err == null && resp.code == 0 && !resp.antiIndulgence);
                if (IsVerify)
                {
                    Age = resp.age;
                }
                // else
                // {
                //     SdkManager.Ui.ShowNormalUI(NormalUIID.VerifyAge);
                // }
            });
        }

        public void Logout()
        {
            Token = null;
            IsVerify = false;
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
                if (resp != null && resp.code == 0)
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
            string vacode, Action successCallback)
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
                    successCallback();
                }
            });
        }
        
        public void VerifyAge(string name, string idcard, Action successCallback)
        {
            var form = new Dictionary<string, string>
            {
                {"realName", name},
                {"identificationCard", idcard}
            };
            SdkManager.Ui.Dialog.ShowLoading();
            PlatformApi.User.VerifyAge.Post(form, (err, meta, resp) =>
            {
                SdkManager.Ui.Dialog.HideLoading();

                if (err != null)
                {
                    SdkManager.Ui.Dialog.Show(err, "ok");
                }
                else
                {
                    IsVerify = true;
                    SdkManager.Ui.Dialog.Show("verify_name_success", "ok");
                    GetCertification();
                    successCallback();
                }
            });
        }

        public long mobileAuthTimestamp = 0;
        public void OnSDKLoginFinish(bool success, string authCode)
        {
            mobileAuthTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            if (success)
            {
                if (curLoginChannel == Api.LoginChannel.CHANNEL_MOBILE)
                {
                    MobileLogin(authCode);
                }
                else
                {
                    // 使用authcode换取平台服token
                    LoginByThird(authCode, curLoginChannel);
                }
            }else if (curLoginChannel == Api.LoginChannel.CHANNEL_MOBILE)
            {
                SdkManager.Ui.ShowLogin();
            }
            else if (authCode == "UserCancel")
            {
                Api.OnLoginCancel();
            }
            else if (authCode == "iOSNotSupport")
            {
                SdkManager.Ui.Dialog.Show("系统版本不支持,请使用其他方式或升级系统", "ok");
                Api.OnLoginFailure(authCode);
            }
            else
            {
                Api.OnLoginFailure(authCode);
            }
        }

        public void OnSDKLogoutFinish(bool success)
        {
            if (success)
            {
                Api.OnLogoutSuccess();
            }
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