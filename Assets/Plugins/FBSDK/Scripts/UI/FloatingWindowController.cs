using FbSdk.Internal;
using UnityEngine;

namespace FbSdk.UI
{
    public class FloatingWindowController : MonoBehaviour, IController
    {
        private FloatingWindow _floatingWindow;

        public bool IsActive;

        public void Init()
        {
            _floatingWindow = GetComponentInChildren<FloatingWindow>(true);
            _floatingWindow.Click += () => Sdk.OpenUserCenter();
        }

        public void Show()
        {
            if (!IsActive)
            {
                return;
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}