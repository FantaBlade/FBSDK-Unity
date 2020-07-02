using System;
using System.Collections.Generic;
using FantaBlade.Internal;
using FantaBlade.UI;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackController : MonoBehaviour, IController
{
    [SerializeField] private InputField _content;
    [SerializeField] private Button _submitButton;

    private Window _window;

    public void Init()
    {
        _window = GetComponent<Window>();
    }

    public void OnSubmitClick()
    {
        if (string.IsNullOrEmpty(_content.text))
        {
            SdkManager.Ui.Dialog.Show("Please Input Message", "ok");
            return;
        }

        _submitButton.interactable = false;
        var form = new Dictionary<string, string>
        {
            {"content", _content.text},
        };
        SdkManager.Ui.Dialog.Show("Confirm Submit", "ok",
            () =>
            {
                SdkManager.Ui.Dialog.ShowLoading();
                PlatformApi.Feedback.Submit.Post(form, (err, info, response) =>
                {
                    SdkManager.Ui.Dialog.HideLoading();
                    _submitButton.interactable = true;
                    if (err == null && response.code == 0)
                    {
                        SdkManager.Ui.Dialog.Show("Submit Success!", "ok");
                        _content.text = String.Empty;
                        _window.Disappear();
                    }
                    else
                    {
                        SdkManager.Ui.Dialog.Show("Submit Failed. Please Retry.", "ok");
                    }
                });
            }, "cancel");
    }
}