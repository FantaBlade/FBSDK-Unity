using FantaBlade.Platform.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.Platform.UI
{
    internal class UserLicenseController : MonoBehaviour, IController
    {
        [SerializeField] private Button _btnReject;
        [SerializeField] private Button _btnAccept;
        [SerializeField] private Button _btnConfirm;
        private Window _window;

        public void Init()
        {
            _window = GetComponent<Window>();
        }
        
        public void OnEnable()
        {
            if (SdkManager.IsUserAcceptLisense())
            {
                _btnReject.gameObject.SetActive(false);
                _btnAccept.gameObject.SetActive(false);
                _btnConfirm.gameObject.SetActive(true);
            }else{
                _btnReject.gameObject.SetActive(true);
                _btnAccept.gameObject.SetActive(true);
                _btnConfirm.gameObject.SetActive(false);
            }
        }

        public void OnClickLisenseReject()
        {
            _window.Disappear();
            FantaBladePlatform.Logout();
        }

        public void OnClickLisenseAccept()
        {
            SdkManager.UserAcceptLisense();
            _window.Disappear();
        }
    }
}