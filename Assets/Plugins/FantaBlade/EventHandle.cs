using System;
using System.Threading;
using FantaBlade.Internal;
using UnityEngine;
using quicksdk;
using UnityEngine.Analytics;

namespace FantaBlade
{
    public class EventHandle : QuickSDKListener
    {
        public Api.UserInfo _UserInfo = new Api.UserInfo();
        public static EventHandle Instance;

        public void Start()
        {
            if (Api.Channel.Equals("Quick"))
            {
                Instance = this;
                QuickSDK.getInstance().setListener(this);
            }
        }

        public override void onInitSuccess()
        {
            //初始化成功的回调
            // Api.OnInitializeSuccess();
        }

        public override void onInitFailed(ErrorMsg err)
        {
            //初始化失败的回调
            Debug.Log(err);
            Api.OnInitializeFailure(err.ToString());
        }

        public override void onLoginSuccess(quicksdk.UserInfo userInfo)
        {
            _UserInfo.switchAccount = false;
            _UserInfo.uid = userInfo.uid;
            _UserInfo.token = userInfo.token;
            if (QuickSDK.getInstance().isFunctionSupported(FuncType.QUICK_SDK_FUNC_TYPE_REAL_NAME_REGISTER))
            {
                QuickSDK.getInstance().callFunction(FuncType.QUICK_SDK_FUNC_TYPE_REAL_NAME_REGISTER);
            }
            else
            {
                onSucceed("{\"uid\":\"1178471402501092\",\"age\":20,\"realName\":true,\"resumeGame\":true,\"other\":\"\",\"FunctionType\":105}");
            }
        }

        public override void onLoginFailed(ErrorMsg errMsg)
        {
            //登录失败的回调
            //如果游戏没有登录按钮，应在这里再次调用登录接口
            Api.OnLoginCancel();
        }

        public override void onSwitchAccountSuccess(quicksdk.UserInfo userInfo)
        {
            //切换账号成功的回调
            //一些渠道在悬浮框有切换账号的功能，此回调即切换成功后的回调。游戏应清除当前的游戏角色信息。在切换账号成功后回到选择服务器界面，请不要再次调用登录接口。
            _UserInfo.switchAccount = true;
            _UserInfo.uid = userInfo.uid;
            _UserInfo.token = userInfo.token;
            if (QuickSDK.getInstance().isFunctionSupported(FuncType.QUICK_SDK_FUNC_TYPE_REAL_NAME_REGISTER))
            {
                QuickSDK.getInstance().callFunction(FuncType.QUICK_SDK_FUNC_TYPE_REAL_NAME_REGISTER);
            }
            else
            {
                onSucceed("{\"uid\":\"1178471402501092\",\"age\":20,\"realName\":true,\"resumeGame\":true,\"other\":\"\",\"FunctionType\":105}");
            }
        }

        public override void onLogoutSuccess()
        {
            //注销成功的回调
            //游戏应该清除当前角色信息，回到登陆界面，并自动调用一次登录接口
            Api.OnLogoutSuccess();
        }

        public override void onSucceed(string infos)
        {
            // onSuccess将返回封装的json实体；uid(表示用户id)，age (表示年龄, 如果渠道没返回默认为-1)，realName (是否已实名： true表示已实名， false表示未实名；如果渠道没返回默认为 false)，resumeGame (渠道实名认证失败之后是否可以继续游戏 ：true表示可以， false表示不可以；如果渠道没返回默认为 true)，other (预留字段，如果渠道没返回默认为""的字符串); FunctionType(表示调用的功能, FuncType.QUICK_SDK_FUNC_TYPE_REAL_NAME_REGISTER的值105为,表示实名认证):
            // QuickSDK.getInstance().callFunction(FuncType.QUICK_SDK_FUNC_TYPE_REAL_NAME_REGISTER);
            // 例如,测试渠道的回调infos结果形式为:
            // {"uid":"1178471402501092","age":20,"realName":true,"resumeGame":true,"other":"","FunctionType":105}
            Api.UserInfo info = JsonUtility.FromJson<Api.UserInfo>(infos);
            switch (info.FunctionType)
            {
                case FuncType.QUICK_SDK_FUNC_TYPE_REAL_NAME_REGISTER:
                    _UserInfo.age = info.age;
                    _UserInfo.realName = info.realName;
                    _UserInfo.resumeGame = info.resumeGame;
                    int real = !_UserInfo.realName || (_UserInfo.age > 0 && _UserInfo.age < 18) ? 1 : 0;
                    //最后一位1是防沉迷
                    if (_UserInfo.switchAccount)
                    {
                        Api.OnSwitchAccountSuccess(QuickSDK.getInstance().channelType()
                                                   + "|" + _UserInfo.uid + "|" + _UserInfo.token + "|" + real);
                    }
                    else
                    {
                        Api.OnLoginSuccess(QuickSDK.getInstance().channelType()
                                           + "|" + _UserInfo.uid + "|" + _UserInfo.token + "|" + real);
                    }

                    break;
            }
        }

        public override void onFailed(string infos)
        {
            try
            {
                Api.UserInfo info = JsonUtility.FromJson<Api.UserInfo>(infos);
                switch (info.FunctionType)
                {
                    case FuncType.QUICK_SDK_FUNC_TYPE_REAL_NAME_REGISTER:
                        Log.Error("REAL_NAME_REGISTER Failed:"+info.msg);
                        onSucceed("{\"uid\":\"1178471402501092\",\"age\":20,\"realName\":true,\"resumeGame\":true,\"other\":\"\",\"FunctionType\":105}");
                        break;
                }
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        public override void onPaySuccess(PayResult payResult)
        {
            //支付成功的回调
            //一些渠道支付成功的通知并不准确，因此客户端的通知仅供参考，游戏发货请以服务端通知为准，不能以客户端的通知为准
            Api.OnPaySuccess();
        }

        public override void onPayCancel(PayResult payResult)
        {
            //支付取消的回调
            Api.OnPayCancel();
        }

        public override void onPayFailed(PayResult payResult)
        {
            //支付失败的回调
            Api.OnPayFailure(payResult.extraParam);
        }

        public override void onExitSuccess()
        {
            //SDK退出成功的回调
            //在此处调用QuickSDK.getInstance().exitGame()函数即可实现退出游戏，杀进程。为避免与渠道发生冲突，请不要使用Application.Quit()函数
            QuickSDK.getInstance().exitGame();
        }

        public void onPauseGame()
        {
            // Time.timeScale = 0;
            QuickSDK.getInstance().callFunction(FuncType.QUICK_SDK_FUNC_TYPE_PAUSED_GAME);
        }

        public void onResumeGame()
        {
            // Time.timeScale = 1;
        }
    }
}
