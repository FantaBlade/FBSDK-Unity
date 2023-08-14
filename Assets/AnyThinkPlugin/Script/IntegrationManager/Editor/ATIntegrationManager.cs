

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;

namespace AnyThink.Scripts.IntegrationManager.Editor
{
    public class ATIntegrationManager
    {
        public static ATIntegrationManager Instance = new ATIntegrationManager();

        private UnityWebRequest downloadPluginRequest;

        private const string AnyThinkAds = "AnyThinkAds";
        //AnyThink的unity插件
        public static string AnyThinkNetworkName = "Core";
        private const string ATSdkAssetExportPath = "AnyThinkAds/Script/";
        public static readonly string GradleTemplatePath = Path.Combine("Assets/Plugins/Android", "mainTemplate.gradle");
        public static readonly string DefaultPluginExportPath = Path.Combine("Assets", AnyThinkAds);


        private ATIntegrationManager()
        {

        }

        public void CancelDownload()
        {
            if (downloadPluginRequest == null) return;

            downloadPluginRequest.Abort();
        }

        public IEnumerator loadNetworksData(PluginData pluginData, Action<PluginData> callback)
        {
            if (pluginData == null)
            {
                callback(null);
            }
            else
            {
                var networksRequest = UnityWebRequest.Get(ATNetInfo.getNetworkListUrl(pluginData.networkUrlVersion));
                var webRequest = networksRequest.SendWebRequest();
                while (!webRequest.isDone)
                {
                    yield return new WaitForSeconds(0.1f);
                }

#if UNITY_2020_1_OR_NEWER
            if (networksRequest.result != UnityWebRequest.Result.Success)
#elif UNITY_2017_2_OR_NEWER
            if (networksRequest.isNetworkError || networksRequest.isHttpError)
#else
                if (networksRequest.isError)
#endif
                {
                    Debug.Log("loadNetworksData failed.");
                    callback(pluginData);
                }
                else
                {
                    //解析network列表的版本数据
                    string netowrksJson = networksRequest.downloadHandler.text;
                    pluginData.mediatedNetworks = ATDataUtil.parseNetworksJson(pluginData, netowrksJson);
                    //初始化AnyThink的core network
                    Network coreNetwork = ATPluginSetting.Instance.CoreNetwork;
                    if (coreNetwork != null)
                    {
                        pluginData.anyThink = coreNetwork;
                    }
                    ATLog.log("loadNetworksData() >>> mediatedNetworks: " + pluginData.mediatedNetworks);
                    callback(pluginData);
                }
            }
        }

        public IEnumerator loadPluginData(Action<PluginData> callback)
        {
            var anythinkVersionRequest = UnityWebRequest.Get(ATNetInfo.getPluginConfigUrl());
            var webRequest = anythinkVersionRequest.SendWebRequest();
            while (!webRequest.isDone)
            {
                yield return new WaitForSeconds(0.1f);
            }

#if UNITY_2020_1_OR_NEWER
            if (anythinkVersionRequest.result != UnityWebRequest.Result.Success)
#elif UNITY_2017_2_OR_NEWER
            if (anythinkVersionRequest.isNetworkError || anythinkVersionRequest.isHttpError)
#else
            if (anythinkVersionRequest.isError)
#endif
            {
                Debug.Log("loadPluginData failed.");
                callback(null);
            }
            else
            {
                //解析Anythink的版本数据
                string anythinkVersionJson = anythinkVersionRequest.downloadHandler.text;
                PluginData pluginData = ATDataUtil.parsePluginDataJson(anythinkVersionJson);
                Debug.Log("loadPluginData succeed.");
                callback(pluginData);
            }
        }

        //默认下载core包，在下载完network的数据时。
        public void downloadCorePlugin(PluginData pluginData)
        {
            Network network = pluginData.anyThink;
            int country = pluginData.country;
            ATLog.log("downloadCorePlugin() >>> network: " + network);
            if (network == null)
            {
                return;
            }
            bool isInstalled = ATConfig.isNetworkInstalled(network.Name, country);
            ATLog.log("downloadCorePlugin() >>> isInstalled: " + isInstalled);
            if (!isInstalled)
            {
                downloadPlugin(network, false);
                // ATEditorCoroutine.startCoroutine(downloadPluginWithEnumerator(network, false));
            } else {
                handlerVersionAdapter(pluginData);
            }
        }

        public void networkInstallOrUpdate(PluginData pluginData, Network network)
        {
            bool uninstall = true;
            if (pluginData != null)
            {
                Network coreNetwork = pluginData.anyThink;
                if (coreNetwork != null)
                {
                    if (coreNetwork.isReqiureUpdate())  //network有更新，并且core包也有更新，则更新core包，然后core包更新完，会自动去更新全部network
                    {
                        uninstall = false;
                        network = coreNetwork;
                    }
                }
            }
            int country = ATConfig.getLocalCountry();
            if (ATConfig.isNetworkInstalled(network.Name, country))
            {
                if (uninstall)
                {
                    uninstallNetwork(network, country);
                }
                downloadPlugin(network, false);
            }
            else
            {
                downloadPlugin(network, true);
            }
        }

        /// <summary>
        /// Downloads the plugin file for a given network.
        /// </summary>
        /// <param name="network">Network for which to download the current version.</param>
        /// <param name="showImport">Whether or not to show the import window when downloading. Defaults to <c>true</c>.</param>
        /// <returns></returns>
        public void downloadPlugin(Network network, bool showImport = true)
        {
            ATEditorCoroutine.startCoroutine(downloadPluginWithEnumerator(network, showImport));
        }

        public IEnumerator downloadPluginWithEnumerator(Network network, bool showImport = true)
        {
            ATLog.log("downloadPluginWithEnumerator() >>> network: " + network);
            if (downloadPluginRequest != null)
            {
                downloadPluginRequest.Dispose();
            }
            var path = Path.Combine(Application.temporaryCachePath, ATAssetDatabaseManager.Instance.GetPluginFileName(network));
            ATLog.log("downloadPluginWithEnumerator() >>> path: " + path);
#if UNITY_2017_2_OR_NEWER
            var downloadHandler = new DownloadHandlerFile(path);
#else
            var downloadHandler = new ATDownloadHandler(path);
#endif
            downloadPluginRequest = new UnityWebRequest(network.DownloadUrl)
            {
                method = UnityWebRequest.kHttpVerbGET,
                downloadHandler = downloadHandler
            };

#if UNITY_2017_2_OR_NEWER
            var operation = downloadPluginRequest.SendWebRequest();
#else
            var operation = downloadPluginRequest.Send();
#endif
            while (!operation.isDone)
            {
                yield return new WaitForSeconds(0.1f); // Just wait till downloadPluginRequest is completed. Our coroutine is pretty rudimentary.
                if (operation.progress != 1 && operation.isDone)
                {
                    CallDownloadPluginProgressCallback(network.DisplayName, operation.progress, operation.isDone);
                }
            }

#if UNITY_2020_1_OR_NEWER
            if (downloadPluginRequest.result != UnityWebRequest.Result.Success)
#elif UNITY_2017_2_OR_NEWER
            if (downloadPluginRequest.isNetworkError || downloadPluginRequest.isHttpError)
#else
            if (downloadPluginRequest.isError)
#endif
            {
                ATLog.logError(downloadPluginRequest.error);
            }
            else
            {
                CallDownloadPluginProgressCallback(network.DisplayName, 1, true);
                // importingNetwork = network;
                ATAssetDatabaseManager.Instance.importingNetwork = network;
                AssetDatabase.Refresh();
                AssetDatabase.ImportPackage(path, showImport);
            }
            downloadPluginRequest.Dispose();
            downloadPluginRequest = null;
        }

        private static void CallDownloadPluginProgressCallback(string pluginName, float progress, bool isDone)
        {
            var callback = ATAssetDatabaseManager.Instance.downloadPluginProgressCallback;
            if (callback == null) return;

            callback(pluginName, progress, isDone);
        }

        // private static void CallImportPackageCompletedCallback(Network network)
        // {
        //     ATLog.log("CallImportPackageCompletedCallback() >>> network: " + network);
        //     if (network != null)
        //     {
        //         ATConfig.saveInstalledNetworkVersion(network.Name, network.LatestVersions);
        //     }
        //     var callback = ATAssetDatabaseManager.Instance.importPackageCompletedCallback;
        //     if (callback == null) return;
        //     callback(network);
        // }

        //更新network已安装的版本
        public void UpdateCurrentVersions(Network network)
        {
            if (network == null)
            {
                return;
            }
            bool isCoreNetwork = network.Name.Equals(AnyThinkNetworkName);
            Versions curVerions = ATConfig.getInstalledNetworkVersion(network.Name, ATConfig.getLocalCountry());
            network.CurrentVersions = curVerions;
            network.CurrentToLatestVersionComparisonResult = ATDataUtil.getVersionComparisonResult(curVerions, network.LatestVersions, !isCoreNetwork);
            // if (isCoreNetwork)
            // {
            //     network.CurrentVersions.Unity = ATConfig.PLUGIN_VERSION;
            // }
            ATLog.log("UpdateCurrentVersions() >>> Name: " + network.Name + " latest Unity Version: " + network.LatestVersions.Unity);
        }
        public void uninstallNetwork(Network network, int country)
        {
            foreach (var filePath in ATConfig.getNetworkFilesPath(network.Name, country))
            {
                if (Directory.Exists(filePath))
                {
                    FileUtil.DeleteFileOrDirectory(filePath);
                }
                if (File.Exists(filePath + ".meta"))
                {
                    FileUtil.DeleteFileOrDirectory(filePath + ".meta");
                }
            }
            ATConfig.removeInstalledNetworkVersion(network.Name, country);
            UpdateCurrentVersions(network);
        }

        public void updateAnyThinkPlugin(PluginData pluginData)
        {
            Network coreNetwork = pluginData.anyThink;
            bool unityUpdate = ATDataUtil.CompareVersions(coreNetwork.CurrentVersions.Unity, pluginData.pluginVersion) == VersionComparisonResult.Lesser;
            if (unityUpdate)
            {
                Application.OpenURL(ATNetInfo.getPluginDownloadUrl(pluginData.pluginVersion));
                // ATEditorCoroutine.startCoroutine(downloadAnyThinkPlugin(pluginData.pluginVersion));
            }
            else
            {
                downloadPlugin(coreNetwork);
            }
        }

        public IEnumerator downloadAnyThinkPlugin(string pluginVersion)
        {
            Network network = new Network();
            network.Name = "AnyThinkPlugin";
            network.PluginFileName = ATNetInfo.getPluginFileName(pluginVersion);
            network.DownloadUrl = ATNetInfo.getPluginDownloadUrl(pluginVersion);
            //https://topon-sdk-release.oss-cn-hangzhou.aliyuncs.com/Unity/Plugin/2.0.0/AnyThinkPlugin_2.0.0.unitypackage
            var path = Path.Combine(Application.temporaryCachePath, network.PluginFileName);
            ATLog.log("downloadPluginWithEnumerator() >>> path: " + path);
#if UNITY_2017_2_OR_NEWER
            var downloadHandler = new DownloadHandlerFile(path);
#else
            var downloadHandler = new ATDownloadHandler(path);
#endif
            var downloadPluginRequest = new UnityWebRequest(network.DownloadUrl)
            {
                method = UnityWebRequest.kHttpVerbGET,
                downloadHandler = downloadHandler
            };

#if UNITY_2017_2_OR_NEWER
            var operation = downloadPluginRequest.SendWebRequest();
#else
            var operation = downloadPluginRequest.Send();
#endif
            while (!operation.isDone)
            {
                yield return new WaitForSeconds(0.1f); // Just wait till downloadPluginRequest is completed. Our coroutine is pretty rudimentary.
                if (operation.progress != 1 && operation.isDone)
                {
                    CallDownloadPluginProgressCallback(network.Name, operation.progress, operation.isDone);
                }
            }

#if UNITY_2020_1_OR_NEWER
            if (downloadPluginRequest.result != UnityWebRequest.Result.Success)
#elif UNITY_2017_2_OR_NEWER
            if (downloadPluginRequest.isNetworkError || downloadPluginRequest.isHttpError)
#else
            if (downloadPluginRequest.isError)
#endif
            {
                ATLog.logError(downloadPluginRequest.error);
            }
            else
            {
                CallDownloadPluginProgressCallback(network.Name, 1, true);
                // importingNetwork = network;
                ATAssetDatabaseManager.Instance.importingNetwork = network;
                AssetDatabase.Refresh();
                AssetDatabase.ImportPackage(path, false);
            }
        }
        //处理插件版本,pangle、csj兼容问题
        public void handlerVersionAdapter(PluginData pluginData) {
            string comparePluginVersion = "2.0.1";
            //插件版本>=2.0.1
            VersionComparisonResult comparisonResult = ATDataUtil.CompareVersions(pluginData.pluginVersion, comparePluginVersion);
            if (comparisonResult == VersionComparisonResult.Equal || comparisonResult == VersionComparisonResult.Greater) {
                Network coreNetwork = pluginData.anyThink;
                bool isChina = ATConfig.isSelectedChina();
                if (isChina) { 
                    //国内，Android需要把原来已安装的pangle目录删除掉，2.0.1插件后用csj作为目录
                    string androidPanglePath = ATConfig.CHINA_ANDROID_NETWORK_FILES_PARENT_PATH + "pangle";
                    deleteFilePath(androidPanglePath);  
                }
                //海外，iOS需要把原来已安装的pangle_nonChina目录删除掉，2.0.1插件后用pangle作为目录
                string iOSNonChinaPanglePath = ATConfig.IOS_NETWORK_FILES_PARENT_PATH + "pangle_nonChina";
                //国内，iOS需要把原来已安装的pangle_China目录删除掉，2.0.1插件后用csj作为目录
                string iOSPanglePath = ATConfig.IOS_NETWORK_FILES_PARENT_PATH + "pangle_China";
                deleteFilePath(iOSNonChinaPanglePath);
                deleteFilePath(iOSPanglePath);
            }
        }

        private void deleteFilePath(string filePath) {
            if (Directory.Exists(filePath))
            {
                FileUtil.DeleteFileOrDirectory(filePath);
            }
            if (File.Exists(filePath + ".meta"))
            {
                FileUtil.DeleteFileOrDirectory(filePath + ".meta");
            }
        }
    }


    [Serializable]
    public class PluginData
    {
        public string networkUrlVersion;
        public string pluginVersion;    //插件版本
        public int country = ATConfig.CHINA_COUNTRY; //默认是1=china
        public Network anyThink;
        public Network[] mediatedNetworks;
    }

    [Serializable]
    public class Network : IComparable<Network>
    {
        //
        // Sample network data:
        //
        // {
        //   "Name": "adcolony",
        //   "DisplayName": "AdColony",
        //   "DownloadUrl": "https://bintray.com/applovin/Unity-Mediation-Packages/download_file?file_path=AppLovin-AdColony-Adapters-Android-3.3.10.1-iOS-3.3.7.2.unitypackage",
        //   "PluginFileName": "AppLovin-AdColony-Adapters-Android-3.3.10.1-iOS-3.3.7.2.unitypackage",
        //   "DependenciesFilePath": "MaxSdk/Mediation/AdColony/Editor/Dependencies.xml",
        //   "LatestVersions" : {
        //     "Unity": "android_3.3.10.1_ios_3.3.7.2",
        //     "Android": "3.3.10.1",
        //     "Ios": "3.3.7.2"
        //   }
        // }
        //

        public string Name;
        public string DisplayName;
        public string DownloadUrl;
        public string DependenciesFilePath;
        public string PluginFileName;
        public string[] PluginFilePaths;
        public Versions LatestVersions; //最新版本
        public Versions CurrentVersions;    //当前版本
        [NonSerialized] public VersionComparisonResult CurrentToLatestVersionComparisonResult = VersionComparisonResult.Equal;
        // [NonSerialized] public bool RequiresUpdate = CurrentToLatestVersionComparisonResult == VersionComparisonResult.Lesser;

        public bool isReqiureUpdate()
        {
            return CurrentToLatestVersionComparisonResult == VersionComparisonResult.Lesser;
        }

        public int CompareTo(Network other)
        {
            return this.DisplayName.CompareTo(other.DisplayName);
        }
    }

    /// <summary>
    /// A helper data class used to get current versions from Dependency.xml files.
    /// </summary>
    [Serializable]
    public class Versions
    {

        public string Unity;

        public string Android;

        public string Ios;

        public override bool Equals(object value)
        {
            var versions = value as Versions;

            return versions != null
                   && (Unity == null || Unity.Equals(versions.Unity))
                   && (Android == null || Android.Equals(versions.Android))
                   && (Ios == null || Ios.Equals(versions.Ios));
        }

        public bool HasEqualSdkVersions(Versions versions)
        {
            return versions != null && versions.Android == Android && versions.Ios == Ios;
        }

        public override int GetHashCode()
        {
            return new { Unity, Android, Ios }.GetHashCode();
        }

        public Versions clone()
        {
            Versions cloneObj = new Versions();
            cloneObj.Android = Android;
            cloneObj.Ios = Ios;
            cloneObj.Unity = Unity;

            return cloneObj;
        }
    }

    public enum VersionComparisonResult
    {
        Lesser = -1,
        Equal = 0,
        Greater = 1
    }
}
