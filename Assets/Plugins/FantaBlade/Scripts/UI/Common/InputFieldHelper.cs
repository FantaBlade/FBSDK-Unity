using System;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.UI.Common
{
    /// <summary>
    /// 控制 InputField 辅助功能
    /// </summary>
    public class InputFieldHelper : MonoBehaviour
    {
        [SerializeField] private InputField _inputField;
        [SerializeField] private bool _isPassword;
        [SerializeField] private Button _btnClear;
        [SerializeField] private Image _imgDisplayPassword;
        [SerializeField] private Sprite _pwDisplaySprite;
        [SerializeField] private Sprite _pwHideSprite;

        private bool _isPwDisplaying = false;

        private void Awake()
        {
            _isPwDisplaying = false;

            if (null == _inputField) _inputField = GetComponent<InputField>();

            if (_isPassword)
            {
                UpdatePasswordType(_isPwDisplaying);
            }

            if (_btnClear) _btnClear.onClick.AddListener(OnClickClear);

            _inputField.onValueChanged.AddListener(OnInputValueChnage);
            OnInputValueChnage(_inputField.text);

            if (_imgDisplayPassword)
            {
                Button btnDisplayPassword = _imgDisplayPassword.GetComponent<Button>();
                if (btnDisplayPassword)
                {
                    btnDisplayPassword.onClick.AddListener(OnClickDisplayPassword);
                }
            }
        }

        private void OnEnable()
        {
            //由于能够看到密码,所以每次显示时会主动清空密码框
            if (_isPassword && _inputField) _inputField.text = string.Empty;
        }

        private void OnClickDisplayPassword()
        {
            _isPwDisplaying = !_isPwDisplaying;
            UpdatePasswordType(_isPwDisplaying);
        }

        private void OnClickClear()
        {
            if (_inputField) _inputField.text = string.Empty;
        }

        private void OnInputValueChnage(string val)
        {
            if (_btnClear) _btnClear.gameObject.SetActive(!string.IsNullOrEmpty(val));
        }

        private void UpdatePasswordType(bool displayPw)
        {
            if (_imgDisplayPassword) _imgDisplayPassword.sprite = displayPw ? _pwDisplaySprite : _pwHideSprite;
            if (_inputField)
            {
                _inputField.contentType = displayPw ? InputField.ContentType.Standard : InputField.ContentType.Password;
                _inputField.ForceLabelUpdate();
            }
        }
    }
}