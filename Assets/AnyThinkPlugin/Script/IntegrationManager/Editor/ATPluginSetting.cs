using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using AnyThink.Scripts.IntegrationManager.Editor;
using Network = AnyThink.Scripts.IntegrationManager.Editor.Network;

public class ATPluginSetting : ScriptableObject
{
    public const string SettingsExportPath = "Assets/AnyThinkPlugin/Resources/Assets/ATPluginSetting.asset";

    private static ATPluginSetting instance;
    [SerializeField] private string adMobAndroidAppId = string.Empty;
    [SerializeField] private string adMobIosAppId = string.Empty;

    [SerializeField] private AnyThink.Scripts.IntegrationManager.Editor.Network coreNetwork = null;

    [SerializeField] private bool enableAndroidX = false;

    // [SerializeField] private int country = ATConfig.CHINA_COUNTRY;

    // [SerializeField] private Dictionary<string, Versions> installedNetwork = new Dictionary<string, Versions>();

    public static ATPluginSetting Instance
    {
        get
        {
            if (instance == null)
            {
                var settingsDir = Path.GetDirectoryName(SettingsExportPath);
                if (!Directory.Exists(settingsDir))
                {
                    Directory.CreateDirectory(settingsDir);
                }
                string settingsFilePath = SettingsExportPath;

                instance = AssetDatabase.LoadAssetAtPath<ATPluginSetting>(settingsFilePath);
                if (instance != null) return instance;

                instance = CreateInstance<ATPluginSetting>();
                AssetDatabase.CreateAsset(instance, settingsFilePath);
                // initCoreNetwork();
            }

            return instance;
        }
    }

    // private static void initCoreNetwork()
    // {
    //     Versions curVerions = ATConfig.getInstalledNetworkVersion(ATIntegrationManager.AnyThinkNetworkName, ATConfig.getLocalCountry());
    //     if (curVerions != null)
    //     {
    //         AnyThink.Scripts.IntegrationManager.Editor.Network coreNetwork = new AnyThink.Scripts.IntegrationManager.Editor.Network();
    //         coreNetwork.CurrentVersions = curVerions;
    //         ATPluginSetting.Instance.CoreNetwork = coreNetwork;
    //     }
    // }

    /// <summary>
    /// AdMob Android App ID.
    /// </summary>
    public string AdMobAndroidAppId
    {
        get { return Instance.adMobAndroidAppId; }
        set { Instance.adMobAndroidAppId = value; }
    }

    /// <summary>
    /// AdMob iOS App ID.
    /// </summary>
    public string AdMobIosAppId
    {
        get { return Instance.adMobIosAppId; }
        set { Instance.adMobIosAppId = value; }
    }

    public AnyThink.Scripts.IntegrationManager.Editor.Network CoreNetwork
    {
        get { return Instance.coreNetwork; }
        set { Instance.coreNetwork = value; }
    }
    // public void saveInstalledNetwork(string networkName, int country, Versions versions)
    // {
    //     string key = networkName + "_" + country;
    //     if (installedNetwork.ContainsKey(key))
    //     {
    //         installedNetwork.Remove(key);
    //     }
    //     installedNetwork.Add(key, versions);
    //     SaveAsync();
    // }

    // public Versions getInstalledNetwork(string networkName, int country)
    // {
    //     Versions versions;
    //     installedNetwork.TryGetValue(networkName + "_" + country, out versions);
    //     return versions;
    // }

    /// <summary>
    /// Saves the instance of the settings.
    /// </summary>
    public void SaveAsync()
    {
        EditorUtility.SetDirty(instance);
    }

    public bool EnableAndroidX
    {
        get { return Instance.enableAndroidX; }
        set { Instance.enableAndroidX = value; }
    }
}
