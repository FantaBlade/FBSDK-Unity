using System;
using System.Collections.Generic;
using FantaBlade.Common;
using FantaBlade.Platform.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FantaBlade.Platform.Internal
{
    public enum NormalUIID
    {
        None = 0,
        Login,
        UserCenter,
        WelcomeBack,
        Activation,
        VerifyAge,
        Feedback,
        UserLicense,
        CancelAccount,
    }

    public class NormalUIPath
    {
        public static string GetPath(NormalUIID uiId)
        {
            switch (uiId)
            {
                case NormalUIID.None:
                    break;
                case NormalUIID.Login:
                    return "fantablade_sdk/prefab/login";
                case NormalUIID.UserCenter:
                    return "fantablade_sdk/prefab/user_center";
                case NormalUIID.WelcomeBack:
                    return "fantablade_sdk/prefab/welcome_back";
                case NormalUIID.Activation:
                    return "fantablade_sdk/prefab/activation_code";
                case NormalUIID.VerifyAge:
                    return "fantablade_sdk/prefab/verify_age";
                case NormalUIID.Feedback:
                    return "fantablade_sdk/prefab/feedback";
                case NormalUIID.CancelAccount:
                    return "fantablade_sdk/prefab/cancel_account";
                case NormalUIID.UserLicense:
                    return "fantablade_sdk/prefab/user_license";
                default:
                    break;
            }

            return string.Empty;
        }
    }

    internal class UiManager
    {
        public DialogController Dialog;
        public FloatingWindowController FloatingWindow;

        private readonly List<IController> _controllers = new List<IController>();

        private Transform _uiRoot;
        private Transform _defaultLayer;
        private Transform _dialogLayer;
        private Transform _floatingLayer;

        // private GameObject _login;
        // private GameObject _userCenter;

        private Dictionary<int, GameObject> mActiveUIs = new Dictionary<int, GameObject>();
        private Stack<int> mActiveUIStack = new Stack<int>();
        private Dictionary<string, GameObject> mCachedUIs = new Dictionary<string, GameObject>();

        public void Init()
        {
            if (_uiRoot != null) return;
            DestroyAll();
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
        

        public void ShowFloatingWindow()
        {
            if(FloatingWindow.IsActive){_uiRoot.gameObject.SetActive(true);}
            FloatingWindow.gameObject.SetActive(FloatingWindow.IsActive);
        }

        public void HideFloatingWindow()
        {
            FloatingWindow.gameObject.SetActive(false);
        }

        public void ShowNormalUI(NormalUIID uiId)
        {
            ShowNormalUI((int) uiId, NormalUIPath.GetPath(uiId));
        }

        /// <summary>
        /// 按照id加载并显示对应界面
        /// </summary>
        /// <param name="uiId">对应  NormalUIID</param>
        public void ShowNormalUI(int uiId, string resPath)
        {
            _uiRoot.gameObject.SetActive(true);
            var scaler = _uiRoot.gameObject.GetComponent<CanvasScaler>();
            if (Screen.orientation == UnityEngine.ScreenOrientation.Portrait ||
                Screen.orientation == UnityEngine.ScreenOrientation.PortraitUpsideDown)
            {
                scaler.referenceResolution = new Vector2(1080, 1920);
            }
            else
            {
                scaler.referenceResolution = new Vector2(1920, 1080);
            }
            // use mActiveUIStack find opened ui with uiId
            GameObject go = null;
            if (!mActiveUIs.ContainsKey(uiId))
            {
                go = GetResource(resPath);
                mActiveUIs.Add(uiId, go);
            }

            go = mActiveUIs[uiId];
            SetLayer(_defaultLayer, go.transform);
            if (go.GetComponent<Window>() != null)
            {
                go.GetComponent<Window>().Appear();
            }
            else
            {
                go.SetActive(true);
            }

            ControllerInit(go);

            mActiveUIStack.Push(uiId);
            go.transform.SetAsLastSibling();
        }

        public void Pop()
        {
            while (0 < mActiveUIStack.Count)
            {
                int id = mActiveUIStack.Pop();
                if (HideNormalUI(id))
                {
                    break;
                }
            }
        }
        
        public void HideRoot()
        {
            // if (FloatingWindow.gameObject.activeSelf)
            //     return;
            // if (Dialog.gameObject.activeSelf)
            //     return;
            // foreach (var gameObject in mActiveUIs.Values)
            // {
            //     if (gameObject.activeSelf)
            //         return;
            // }
            // _uiRoot.gameObject.SetActive(false);
        }

        public bool HideNormalUI(int uiId)
        {
            if (!mActiveUIs.ContainsKey(uiId))
            {
                return false;
            }

            GameObject go = mActiveUIs[uiId];
            if (null != go)
            {
                go.SetActive(false);
            }

            return true;
            //not remove from stack
        }

        public void HideAll()
        {
            Dictionary<int, GameObject>.KeyCollection keys = mActiveUIs.Keys;
            Dictionary<int, GameObject>.KeyCollection.Enumerator enumerator = keys.GetEnumerator();
            while (enumerator.MoveNext())
            {
                HideNormalUI(enumerator.Current);
            }
        }

        public void DestroyAll()
        {
            HideAll();
            mActiveUIs = new Dictionary<int, GameObject>();
            mActiveUIStack = new Stack<int>();
            mCachedUIs.Clear();
        }

        public void ShowLogin()
        {
            if (SdkManager.IsLoginChannelAvailable(FantaBladePlatform.LoginChannel.CHANNEL_MOBILE)
                && DateTimeOffset.Now.ToUnixTimeSeconds() - SdkManager.Auth.mobileAuthTimestamp > 3)
            {
                SdkManager.Auth.mobileAuthTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                Log.Debug("调起手机认证");
                SdkManager.Auth.LoginThird(FantaBladePlatform.LoginChannel.CHANNEL_MOBILE);
            }
            else
            {
                Log.Debug("手机认证不支持，调起常规登录");
                ShowNormalUI(NormalUIID.Login);
            }
        }

        public void HideLogin()
        {
            HideNormalUI((int) NormalUIID.Login);
            HideRoot();
        }

        public void ShowActivation()
        {
            ShowNormalUI(NormalUIID.Activation);
        }
        
        public void HideActivation()
        {
            HideNormalUI((int)NormalUIID.Activation);
        }

        public void ShowGameCenter(NormalUIID ui = NormalUIID.None)
        {
            ShowNormalUI(NormalUIID.UserCenter);
        }

        public void HideGameCenter()
        {
            HideNormalUI((int) NormalUIID.UserCenter);
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

        private GameObject GetResource(string path)
        {
            if (!mCachedUIs.ContainsKey(path))
            {
                GameObject obj = Resources.Load<GameObject>(path);
                GameObject go = Object.Instantiate(obj);
                mCachedUIs.Add(path, go);
            }

            return mCachedUIs[path];
        }

        private void ClearCache()
        {
            if (null != mCachedUIs && 0 < mCachedUIs.Count)
            {
                foreach (KeyValuePair<string, GameObject> kvp in mCachedUIs)
                {
                    GameObject.Destroy(kvp.Value);
                }
            }

            mCachedUIs.Clear();
        }
    }
}