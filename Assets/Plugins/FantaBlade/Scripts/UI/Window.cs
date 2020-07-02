using UnityEngine;

namespace FantaBlade.UI
{

    public enum WindowType
    {
        None = 0,
        PhoneLogin = 1,
        AccountLogin = 2,
        Register = 3,
        ResetPassword = 4,
        UserLicense = 5,
        CommonBg = 6,
        GuestTip = 7,
        Activation = 8,
    }
    
    /// <summary>
    ///     UI View 的内容
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    internal class Window : MonoBehaviour
    {
        public WindowType WindowType;

        private CanvasGroup _canvasGroup;

        private CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
                }

                return _canvasGroup;
            }
        }

        private Tweener _tweener;

        private bool _isAppear;

        /// <summary>
        ///     For UnityEvent use
        /// </summary>
        /// <returns></returns>
        public void Appear()
        {
            Appear(true);
        }

        /// <summary>
        ///     For UnityEvent use
        /// </summary>
        /// <returns></returns>
        public void Disappear()
        {
            Disappear(true);
        }

        /// <summary>
        ///     For UnityEvent use
        /// </summary>
        /// <returns></returns>
        public void Switch()
        {
            Switch(true);
        }

        private YieldInstruction Appear(bool withAnim)
        {
            _isAppear = true;
            gameObject.SetActive(true);
            KillTweenerIfPlaying();
            // if (withAnim)
            // {
            //     _tweener = CanvasGroup.DOFade(this, 1, 0.2f);
            //     return _tweener.WaitForCompletion();
            // }
            // else
            // {
                CanvasGroup.alpha = 1;
                return null;
            // }
        }

        private YieldInstruction Disappear(bool withAnim)
        {
            _isAppear = false;
            KillTweenerIfPlaying();
            if (withAnim)
            {
                _tweener = CanvasGroup.DOFade(this, 0, 0.2f);
                _tweener.OnComplete(() => gameObject.SetActive(false));
                return _tweener.WaitForCompletion();
            }
            else
            {
                CanvasGroup.alpha = 0;
                gameObject.SetActive(false);
                return null;
            }
        }

        private YieldInstruction Switch(bool withAnim)
        {
            return _isAppear ? Disappear(withAnim) : Appear(withAnim);
        }

        private void KillTweenerIfPlaying()
        {
            if (_tweener != null && _tweener.IsPlaying())
            {
                _tweener.Kill();
            }
        }
    }
}