using System;
using System.Collections.Generic;
using FantaBlade.Internal;
using FantaBlade.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.UI
{
	internal class MainController : MonoBehaviour, IController
	{
		[SerializeField] private MobileController _mobileController;
		[SerializeField] private InputField _password;
		[SerializeField] private List<Window> _windowsList;
		[SerializeField] private WindowType _initWindowType;
		[SerializeField] private GameObject wechatBtn;
		[SerializeField] private GameObject qqBtn;
		[SerializeField] private GameObject weiboBtn;
		[SerializeField] private GameObject douyinBtn;
		[SerializeField] private GameObject appleBtn;
        
		private bool _isLoggingIn;
		private readonly Dictionary<WindowType, Window> _windowsDict= new Dictionary<WindowType, Window>();
		private readonly Stack<WindowType> _windowStack = new Stack<WindowType>();

		public void Init()
		{
			_windowsDict.Clear();
			_windowStack.Clear();
			foreach (var window in _windowsList)
			{
				_windowsDict.Add(window.WindowType, window);
				if (window.isActiveAndEnabled)
				{
					CloseWindow(window);
				}
			}
			// init third login
			InitThirdLogin();
			OpenWindow(_initWindowType);
		}

		private void SetBtnActive(GameObject go, int loginChannel)
		{
			if (null == go)
			{
				return;
			}
			var active = SdkManager.NativeApi.IsChannelRegister(loginChannel);
			#if UNITY_IOS
			active = active && SdkManager.NativeApi.IsInstall(loginChannel);
			#endif
			go.SetActive(active);
		}
		
		public void InitThirdLogin()
		{
			SetBtnActive(appleBtn, Api.LoginChannel.CHANNEL_APPLE);
			SetBtnActive(wechatBtn, Api.LoginChannel.CHANNEL_WECHAT);
			SetBtnActive(qqBtn, Api.LoginChannel.CHANNEL_QQ);
			SetBtnActive(weiboBtn, Api.LoginChannel.CHANNEL_WEIBO);
			SetBtnActive(douyinBtn, Api.LoginChannel.CHANNEL_DOUYIN);
		}

		public void OnLoginClick()
		{
			if (SdkManager.Auth.IsLoggingIn)
			{
				return;
			}

			_mobileController.SaveCache();
			SdkManager.Auth.Login(_mobileController.MobileNumber, _password.text, _mobileController.CountryCode);
		}

		public void OnQuickLoginClick()
		{
			if (SdkManager.Auth.IsLoggingIn)
			{
				return;
			}

			SdkManager.Auth.QuickLogin();
		}

		public void OpenWndow(int windowTypeInt)
		{
			OpenWindow((WindowType)windowTypeInt);
		}
		
		public void OpenWindow(WindowType windowType)
		{
			if (0 < _windowStack.Count)
			{
				_windowsDict[_windowStack.Peek()].Disappear();
			}
			_windowStack.Push(windowType);
			_windowsDict[windowType].Appear();
		}

		public void WindowBack()
		{
			if (Utils.IsClickTooOften())
			{
				return;
			}
			if (1 >= _windowStack.Count)
			{
				return;
			}
			_windowsDict[_windowStack.Pop()].Disappear();
			if (0 < _windowStack.Count)
			{
				_windowsDict[_windowStack.Peek()].Appear();
			}
		}

		public void WechatLogin()
		{
			SdkManager.Auth.LoginThird(Api.LoginChannel.CHANNEL_WECHAT);
		}
		
		public void QQLogin()
		{
			SdkManager.Auth.LoginThird(Api.LoginChannel.CHANNEL_QQ);
		}
		
		public void SinaLogin()
		{
			SdkManager.Auth.LoginThird(Api.LoginChannel.CHANNEL_WEIBO);
		}

		public void DouYinLogin()
		{
			SdkManager.Auth.LoginThird(Api.LoginChannel.CHANNEL_DOUYIN);
		}

		private void CloseWindow(Window window)
		{
			if (window)
			{
				window.gameObject.SetActive(false);
			}
		}
	}
}