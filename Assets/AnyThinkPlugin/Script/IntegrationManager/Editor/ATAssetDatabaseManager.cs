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
using IntegrationManager = AnyThink.Scripts.IntegrationManager.Editor.ATIntegrationManager;

public class ATAssetDatabaseManager : ScriptableObject
{
    public const string SettingsExportPath = "Assets/AnyThinkPlugin/Resources/Assets/ATAssetDatabaseManager.asset";

    private static ATAssetDatabaseManager instance;
    public Network importingNetwork = null;

    public delegate void DownloadPluginProgressCallback(string pluginName, float progress, bool done);
    public delegate void ImportPackageCompletedCallback(Network network);

    public DownloadPluginProgressCallback downloadPluginProgressCallback;
    public ImportPackageCompletedCallback importPackageCompletedCallback;

    // public Network importingNetwork;

    public static ATAssetDatabaseManager Instance
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

                instance = AssetDatabase.LoadAssetAtPath<ATAssetDatabaseManager>(settingsFilePath);
                if (instance != null) return instance;

                instance = CreateInstance<ATAssetDatabaseManager>();
                AssetDatabase.CreateAsset(instance, settingsFilePath);
                // init();
            }

            return instance;
        }
    }

    private ATAssetDatabaseManager()
    {
        init();
    }

    private void init()
    {
        ATLog.log("ATAssetDatabaseManager inited");

        AssetDatabase.importPackageCompleted += packageName =>
       {
           ATLog.logFormat("importPackageCompleted() >>> packageName: {0} importingNetwork: {1}", new object[] { packageName, importingNetwork });
           if (!IsImportingNetwork(packageName)) return;
           ATLog.log("importPackageCompleted() >>> import succeed.");
           //    ATConfig.saveInstalledNetworkVersion(packageName, )
           //    var pluginParentDir = PluginParentDirectory;
           //    var isPluginOutsideAssetsDir = IsPluginOutsideAssetsDirectory;
           //    MovePluginFilesIfNeeded(pluginParentDir, isPluginOutsideAssetsDir);
           //    AddLabelsToAssetsIfNeeded(pluginParentDir, isPluginOutsideAssetsDir);
           AssetDatabase.Refresh();

           CallImportPackageCompletedCallback(importingNetwork);
           //    importingNetwork = null;
           setImportingNetworkNull();
       };

        AssetDatabase.importPackageCancelled += packageName =>
        {
            ATLog.logFormat("importPackageCancelled() >>> packageName: {0}", new object[] { packageName });
            if (!IsImportingNetwork(packageName)) return;

            // MaxSdkLogger.UserDebug("Package import cancelled.");
            // importingNetwork = null;
            setImportingNetworkNull();
        };

        AssetDatabase.importPackageFailed += (packageName, errorMessage) =>
        {
            ATLog.logFormat("importPackageFailed() >>> packageName: {0}, errorMsg: {1}", new object[] { packageName, errorMessage });
            if (!IsImportingNetwork(packageName)) return;

            // MaxSdkLogger.UserError(errorMessage);
            // importingNetwork = null;
            setImportingNetworkNull();
        };
    }

    private void setImportingNetworkNull()
    {
        // ATPluginSetting.Instance.ImportingNetwork = null;
        importingNetwork = null;
    }

    public void CallDownloadPluginProgressCallback(string pluginName, float progress, bool isDone)
    {
        if (downloadPluginProgressCallback == null) return;

        downloadPluginProgressCallback(pluginName, progress, isDone);
    }

    public void CallImportPackageCompletedCallback(Network network)
    {
        ATLog.log("CallImportPackageCompletedCallback() >>> network: " + network + " importPackageCompletedCallback: " + importPackageCompletedCallback);
        if (network != null)
        {
            ATConfig.saveInstalledNetworkVersion(network.Name, network.LatestVersions);
            // ATIntegrationManager.Instance.UpdateCurrentVersions(network);
        }
        if (importPackageCompletedCallback == null) return;
        importPackageCompletedCallback(network);
    }

    public string GetPluginFileName(Network network)
    {
        // return network.Name.ToLowerInvariant() + "_" + network.LatestVersions.Unity + ".unitypackage";
        return network.PluginFileName;
    }

    private bool IsImportingNetwork(string packageName)
    {
        // Note: The pluginName doesn't have the '.unitypacakge' extension included in its name but the pluginFileName does. So using Contains instead of Equals.
        //window系统packageName是：项目路径/AnyThinkCore，Mac系统是: AnyThinkCore
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            if (packageName.Contains("\\"))
            {
                string[] packageNameArray = packageName.Split('\\');
                if (packageNameArray != null && packageNameArray.Length > 0)
                {
                    packageName = packageNameArray[packageNameArray.Length -1];
                }
            }
        #endif
        return importingNetwork != null && GetPluginFileName(importingNetwork).Contains(packageName);
    }

}
