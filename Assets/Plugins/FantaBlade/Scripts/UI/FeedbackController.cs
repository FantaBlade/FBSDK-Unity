﻿using System;
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
            SdkManager.Ui.Dialog.Show("请输入内容", "好的");
            return;
        }

        _submitButton.interactable = false;
        var form = new Dictionary<string, string>
        {
            {"content", _content.text},
        };
        SdkManager.Ui.Dialog.Show("确认提交", "好的",
            () =>
            {
                PlatformApi.Feedback.Submit.Post(form, (err, info, response) =>
                {
                    _submitButton.interactable = true;
                    if (err == null && response.code == 0)
                    {
                        SdkManager.Ui.Dialog.Show("发送成功！", "好的");
                        _content.text = String.Empty;
                        _window.Disappear();
                    }
                    else
                    {
                        SdkManager.Ui.Dialog.Show("发送失败，请重试", "好的");
                    }
                });
            }, "取消");
    }
}