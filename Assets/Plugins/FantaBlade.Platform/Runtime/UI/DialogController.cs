using System.Collections;
using FantaBlade.Platform.Internal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FantaBlade.Platform.UI
{
    internal class DialogController : MonoBehaviour, IController
    {
        [SerializeField] private Text _content;
        [SerializeField] private GameObject _loading;
        [SerializeField] private Button[] _buttons;
        private Window _dialogWindow;
        private Text[] _buttonTexts;

        public void Init()
        {
            _dialogWindow = GetComponent<Window>();
            _buttonTexts = new Text[_buttons.Length];
            for (var i = 0; i < _buttons.Length; i++)
            {
                _buttonTexts[i] = _buttons[i].GetComponentInChildren<Text>();
            }

            gameObject.SetActive(false);
        }
        
        private IEnumerator CoroutineShowLoading()
        {
            yield return new WaitForSeconds(0.5f);
            _loading.GetComponent<CanvasGroup>().alpha = 1;
        }
        public void ShowLoading()
        {
            _loading.SetActive(true);
            _loading.GetComponent<CanvasGroup>().alpha = 0;
            SdkManager.StartCoroutine(CoroutineShowLoading());
        }
        public void HideLoading()
        {
            _loading.SetActive(false);
        }

        private void DialogReset()
        {
            for (var i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].onClick.RemoveAllListeners();
            }
        }

        public void Hide()
        {
            Reset();
            _dialogWindow.Disappear();
        }

        public void Show(string content,
            string buttonName0, UnityAction callback0 = null,
            string buttonName1 = null, UnityAction callback1 = null,
            string buttonName2 = null, UnityAction callback2 = null)
        {
            content = SdkManager.Localize.GetText(content);
            buttonName0 = SdkManager.Localize.GetText(buttonName0);
            buttonName1 = SdkManager.Localize.GetText(buttonName1);
            buttonName2 = SdkManager.Localize.GetText(buttonName2);
            var buttonNames = new[]
            {
                buttonName0,
                buttonName1,
                buttonName2
            };

            var callbacks = new[]
            {
                callback0,
                callback1,
                callback2
            };

            for (var i = 0; i < 3; i++)
            {
                var buttonInUse = buttonNames[i] != null;
                _buttons[i].gameObject.SetActive(buttonInUse);
                if (buttonInUse)
                {
                    if (buttonNames[i] == null)
                    {
                        continue;
                    }

                    _buttonTexts[i].text = buttonNames[i];
                    _buttons[i].onClick.RemoveAllListeners();
                    if (callbacks[i] == null)
                    {
                        callbacks[i] = Hide;
                    }

                    _buttons[i].onClick.AddListener(callbacks[i]);
                }
            }

            _content.text = content;
            _dialogWindow.Appear();
        }

        private void Reset()
        {
            DialogReset();
        }
    }
}