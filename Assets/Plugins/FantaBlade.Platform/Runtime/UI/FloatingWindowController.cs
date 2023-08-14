using FantaBlade.Platform.Internal;
using UnityEngine;

namespace FantaBlade.Platform.UI
{
    public class FloatingWindowController : MonoBehaviour, IController
    {
        private FloatingWindow _floatingWindow;

        public bool IsActive;

        public void Init()
        {
            _floatingWindow = GetComponentInChildren<FloatingWindow>(true);
            _floatingWindow.Click += () => FantaBladePlatform.OpenUserCenter();
        }
    }
}