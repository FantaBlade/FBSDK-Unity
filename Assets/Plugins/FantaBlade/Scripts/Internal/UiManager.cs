using System.Collections.Generic;
using FantaBlade.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FantaBlade.Internal
{

    public enum ENormalUIID
    {
        eNone = 0,
        eLogin,
        eUserCenter,
        eWelcomeBack,
    }

    public class NormalUIPath
    {
        public static string GetPath(ENormalUIID uiId)
        {
            switch (uiId)
            {
                case ENormalUIID.eNone:
                    break;
                case ENormalUIID.eLogin:
                    return "fantablade_sdk/prefab/login";
                case ENormalUIID.eUserCenter:
                    return "fantablade_sdk/prefab/user_center";
                case ENormalUIID.eWelcomeBack:
                    return "fantablade_sdk/prefab/welcome_back";
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

        private GameObject _login;
        private GameObject _userCenter;

        private Dictionary<int, GameObject> mActiveUIs = new Dictionary<int, GameObject>();
        private Stack<int> mActiveUIStack = new Stack<int>();
        private Dictionary<string, GameObject> mCachedUIs = new Dictionary<string, GameObject>();

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

        public void ShowNormalUI(ENormalUIID uiId)
        {
            ShowNormalUI((int)uiId, NormalUIPath.GetPath(uiId));
        }

        /// <summary>
        /// 按照id加载并显示对应界面
        /// </summary>
        /// <param name="uiId">对应  ENormalUIID</param>
        public void ShowNormalUI(int uiId, string resPath)
        {
            //TODO use mActiveUIStack find opened ui with uiId
            GameObject go = null;
            if (! mActiveUIs.ContainsKey(uiId))
            {
                go = GetResource(resPath);
                mActiveUIs.Add(uiId, go);
            }

            go = mActiveUIs[uiId];
            SetLayer(_defaultLayer, go.transform);
            ControllerInit(go);
            go.SetActive(true);

            mActiveUIStack.Push(uiId);
        }

        public void Pop()
        {
            while(0 < mActiveUIStack.Count)
            {
                int id = mActiveUIStack.Pop();
                if (HideNormalUI(id))
                {
                    break;
                }
            }
        }

        public bool HideNormalUI(int uiId)
        {
            if(! mActiveUIs.ContainsKey(uiId))
            {
                return false;
            }
            GameObject go = mActiveUIs[uiId];
            if(null != go)
            {
                go.SetActive(false);
            }
            return true;
            //not remove from stack
        }

        public void HideAll()
        {
            Dictionary<int,GameObject>.KeyCollection keys =  mActiveUIs.Keys;
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

        }

        public void ShowLogin()
        {

            SdkManager.Ui.ShowNormalUI(ENormalUIID.eLogin);


            //if (_login == null)
            //{
            //    var login = Resources.Load<GameObject>("fantablade_sdk/prefab/login");
            //    _login = Object.Instantiate(login);
            //    SetLayer(_defaultLayer, _login.transform);
            //    ControllerInit(_login);
            //}

            //_login.SetActive(true);
        }

        public void HideLogin()
        {
            SdkManager.Ui.HideNormalUI((int)ENormalUIID.eLogin);
            //_login.SetActive(false);
        }

        public void ShowGameCenter()
        {
            SdkManager.Ui.ShowNormalUI(ENormalUIID.eUserCenter);
            //if (_userCenter == null)
            //{
            //    var userCenter = Resources.Load<GameObject>("fantablade_sdk/prefab/user_center");
            //    _userCenter = Object.Instantiate(userCenter);
            //    SetLayer(_defaultLayer, _userCenter.transform);
            //    ControllerInit(_userCenter);
            //}

            //_userCenter.SetActive(true);
        }

        public void HideGameCenter()
        {
            SdkManager.Ui.HideNormalUI((int)ENormalUIID.eUserCenter);
            //_userCenter.SetActive(false);
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
            if(! mCachedUIs.ContainsKey(path))
            {
                GameObject obj = Resources.Load<GameObject>(path);
                GameObject go = Object.Instantiate(obj);
                mCachedUIs.Add(path, go);
            }
            return mCachedUIs[path];
        }

        private void ClearCache()
        {
            if(null != mCachedUIs && 0 < mCachedUIs.Count)
            {
                foreach(KeyValuePair<string,GameObject> kvp in mCachedUIs)
                {
                    GameObject.Destroy(kvp.Value);
                }
            }
            mCachedUIs.Clear();
        }

    }
}