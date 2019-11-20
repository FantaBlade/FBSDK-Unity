using System;
using System.Collections.Generic;
using FantaBlade.Internal;
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

			OpenWindow(_initWindowType);
		}

		public void OnLoginClick()
		{
			if (SdkManager.Auth.IsLoggingIn)
			{
				return;
			}
            
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
			_windowsDict[_windowStack.Pop()].Disappear();
			if (0 < _windowStack.Count)
			{
				_windowsDict[_windowStack.Peek()].Appear();
			}
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