using UnityEngine;
using System.Collections;
using FantaBlade;
using FantaBlade.Internal;

namespace quicksdk
{
    // QuickSDKListener
    public abstract class QuickSDKListener : MonoBehaviour
    {
        //callback
		public abstract void onInitSuccess();

		public abstract void onInitFailed(ErrorMsg message);

        public abstract void onLoginSuccess(UserInfo userInfo);

		public abstract void onSwitchAccountSuccess(UserInfo userInfo);

		public abstract void onLoginFailed(ErrorMsg errMsg);

        public abstract void onLogoutSuccess();

        public abstract void onSucceed(string infos);
        public abstract void onFailed(string message);



        public abstract void onPaySuccess(PayResult payResult);

        public abstract void onPayFailed(PayResult payResult);

        public abstract void onPayCancel(PayResult payResult);

        public abstract void onExitSuccess();


        //callback end


		public void onInitSuccess(string msg)
		{
			onInitSuccess();
		}

		public void onInitFailed(string msg)
		{
			var data = SimpleJSON.JSONNode.Parse(msg);
			ErrorMsg errMsg = new ErrorMsg();
			errMsg.errMsg =  data["msg"].Value;
			
			onInitFailed(errMsg);
		}

        public void onLoginSuccess(string msg)
        {
	        EventHandle.showLog("QuickSdk onLoginSuccess ", msg);
            var data = SimpleJSON.JSONNode.Parse(msg);
            UserInfo userInfo = new UserInfo();
            userInfo.uid = data["userId"].Value;
            userInfo.token = data["userToken"].Value;
            userInfo.userName = data["userName"].Value;
            userInfo.errMsg = data["msg"].Value;

            onLoginSuccess(userInfo);
        }

		public void onSwitchAccountSuccess(string msg)
		{
			EventHandle.showLog("QuickSdk onSwitchAccountSuccess ", msg);
			var data = SimpleJSON.JSONNode.Parse(msg);
			UserInfo userInfo = new UserInfo();
			userInfo.uid = data["userId"].Value;
			userInfo.token = data["userToken"].Value;
			userInfo.userName = data["userName"].Value;
			userInfo.errMsg = data["msg"].Value;

			onSwitchAccountSuccess(userInfo);
		}

        public void onLoginFailed(string msg)
        {
	        EventHandle.showLog("QuickSdk onLoginFailed ", msg);
            var data = SimpleJSON.JSONNode.Parse(msg);
			ErrorMsg errMsg = new ErrorMsg();
			errMsg.errMsg = data["msg"].Value;
			
			onLoginFailed(errMsg);
        }

        public void onLogoutSuccess(string msg)
        {
            onLogoutSuccess();
        }



        public void onPaySuccess(string msg)
        {
            var data = SimpleJSON.JSONNode.Parse(msg);
            PayResult result = new PayResult();
            result.cpOrderId = data["cpOrderId"].Value;
            result.orderId = data["orderId"].Value;
            result.extraParam = data["extraParam"].Value;

            onPaySuccess(result);
        }

        public void onPayFailed(string msg)
        {
            var data = SimpleJSON.JSONNode.Parse(msg);
            PayResult result = new PayResult();
            result.cpOrderId = data["cpOrderId"].Value;
            result.orderId = data["orderId"].Value;
            result.extraParam = data["extraParam"].Value;

            onPayFailed(result);
        }

        public void onPayCancel(string msg)
        {
            var data = SimpleJSON.JSONNode.Parse(msg);
            PayResult result = new PayResult();
            result.cpOrderId = data["cpOrderId"].Value;
            result.orderId = data["orderId"].Value;
            result.extraParam = data["extraParam"].Value;

            onPayCancel(result);
        }

        public void onExitSuccess(string msg)
        {
            onExitSuccess();
        }
		
        public void onSuccess(string infos)
        {
	        EventHandle.showLog("QuickSdk onSuccess ", infos);
            onSucceed(infos);
        }

        public void onFail(string msg)
        {
            onFailed(msg);
        }

    }
}
