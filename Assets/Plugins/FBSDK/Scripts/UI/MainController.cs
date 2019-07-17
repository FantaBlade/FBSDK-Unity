using System.Collections.Generic;
using FbSdk.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FbSdk.UI
{
	internal class MainController : MonoBehaviour, IController
	{
		[SerializeField] private MobileController _mobileController;
		[SerializeField] private InputField _password;
        
		private bool _isLoggingIn;

		public void Init()
		{
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
	}
}