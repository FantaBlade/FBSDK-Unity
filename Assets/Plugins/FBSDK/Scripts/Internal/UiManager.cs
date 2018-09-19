using System.Collections.Generic;
using FbSdk.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FbSdk.Internal
{
    internal class UiManager
    {
        public DialogController Dialog;
        public FloatingWindowController FloatingWindow;

        private readonly List<IController> _controllers = new List<IController>();

        private Transform _uiRoot;
        private Transform _defaultLayer;
        private Transform _dialogLayer;
        private Transform _floatingLayer;

        private GameObject _login;
        private GameObject _userCenter;


        public void Init()
        {
            if (_uiRoot != null) return;
            _uiRoot = SdkManager.Instance.transform;
            _defaultLayer = _uiRoot.Find("default_layer");
            _dialogLayer = _uiRoot.Find("dialog_layer");
            _floatingLayer = _uiRoot.Find("floating_layer");
            Dialog = GetController<DialogController>(_dialogLayer, "dialog");
            FloatingWindow = GetController<FloatingWindowController>(_floatingLayer, "floating_window");

            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                SetLayer(eventSystem.transform, _uiRoot);
            }
        }

        public void ShowLogin()
        {
            if (_login == null)
            {
                var login = Resources.Load<GameObject>("fbsdk/prefab/login");
                _login = Object.Instantiate(login);
                SetLayer(_defaultLayer, _login.transform);
                ControllerInit(_login);
            }

            _login.SetActive(true);
        }

        public void HideLogin()
        {
            _login.SetActive(false);
        }

        public void ShowGameCenter()
        {
            if (_userCenter == null)
            {
                var userCenter = Resources.Load<GameObject>("fbsdk/prefab/user_center");
                _userCenter = Object.Instantiate(userCenter);
                SetLayer(_defaultLayer, _userCenter.transform);
                ControllerInit(_userCenter);
            }

            _userCenter.SetActive(true);
        }

        public void HideGameCenter()
        {
            _userCenter.SetActive(false);
        }

        private void ControllerInit(GameObject root)
        {
            foreach (var controller in root.GetComponentsInChildren<IController>(true))
            {
                controller.Init();
            }
        }

        private void SetLayer(Transform layer, Transform transform)
        {
            transform.SetParent(layer, false);
        }

        private T GetController<T>(Transform layer, string name) where T : IController
        {
            var controller = layer.Find(name).GetComponent<T>();
            _controllers.Add(controller);
            controller.Init();
            return controller;
        }
    }
}