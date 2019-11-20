﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace FantaBlade.Internal
{
    public class LocalizeManager
    {
        private Dictionary<SystemLanguage, LanguageSet> _setting;
        private SystemLanguage _currentLanguage;

        public void Init(SystemLanguage language)
        {
            _setting = new Dictionary<SystemLanguage, LanguageSet>();
            SetLanguage(language);
        }

        public void SetLanguage(SystemLanguage language)
        {
            _currentLanguage = language;
            if (!_setting.ContainsKey(_currentLanguage))
            {
                _setting.Add(_currentLanguage, LanguageSet.CreateLanguageSet(GetFileNameByLanguage(_currentLanguage)));
            }
        }

        public string GetText(string key)
        {
            return _setting[_currentLanguage].GetText(key);
        }

        public string GetFileNameByLanguage(SystemLanguage language)
        {
            const string basePath = "fantablade_sdk/config/";
            switch (language)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    return basePath + "message_zh_cn";
                case SystemLanguage.ChineseTraditional:
                    return basePath + "message_zh_tw";
                default:
                    return basePath + "message_en";
            }
        }

        class LanguageSet
        {
            private readonly Dictionary<string, string> _localizeDict = new Dictionary<string, string>();

            public static LanguageSet CreateLanguageSet(string configFilePath)
            {
                TextAsset textAsset = Resources.Load<TextAsset>(configFilePath);
                LanguageSet langSet = new LanguageSet();
                langSet.ParseConfig(textAsset.text);
                return langSet;
            }

            public string GetText(string key)
            {
                if (!_localizeDict.ContainsKey(key))
                {
                    return key;
                }

                return _localizeDict[key];
            }

            private void ParseConfig(string configStr)
            {
                StringReader reader = new StringReader(configStr);
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] kvp = line.Split(',');
                    if (!_localizeDict.ContainsKey(kvp[0]))
                    {
                        _localizeDict.Add(kvp[0], kvp[1]);
                    }
                }
            }
        }

    }
}