using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace AnyThink.Scripts.IntegrationManager.Editor
{

    // public class ATConfig : ScriptableObject
    public class ATConfig
    {
public static string PLUGIN_VERSION = "2.0.1";
public static bool isDebug = false;
        private static string LAST_SELECT_COUNTRY_KEY = "country_key";  //国家
        public static int CHINA_COUNTRY = 1;
        public static int NONCHINA_COUNTRY = 2;
        public static string ANYTHINK_SDK_FILES_PATH = "Assets/AnyThinkAds";
        //国内core aar包的父目录
        public static string CHINA_ANDROID_CORE_FILES_PATH = "Assets/AnyThinkAds/Plugins/Android/China/anythink_base/";
        public static string NONCHINA_ANDROID_CORE_FILES_PATH = "Assets/AnyThinkAds/Plugins/Android/NonChina/anythink_base/";
        //国内Android network aar包的父目录
        public static string CHINA_ANDROID_NETWORK_FILES_PARENT_PATH = "Assets/AnyThinkAds/Plugins/Android/China/mediation/";
        //海外Android network 依赖文件的目录
        public static string NONCHINA_ANDROID_NETWORK_FILES_PARENT_PATH = "Assets/AnyThinkAds/Plugins/Android/NonChina/mediation/";
        //iOS network依赖文件的目录，不区分国家
        public static string IOS_NETWORK_FILES_PARENT_PATH = "Assets/AnyThinkAds/Plugins/iOS/";

        // 保存已安装的network到本地
        public static void saveInstalledNetworkVersion(string networkName, Versions v)
        {
            Versions versions = v.clone();
            int country = getLocalCountry();
           
            ATLog.log("saveInstalledNetworkVersion() >>> networkName: " + networkName + " unity: " + versions.Unity);
            versions = initUnityForVerions(versions, networkName, country);
            // ATPluginSetting.Instance.saveInstalledNetwork(networkName, country, versions);
            string jsonStr = JsonUtility.ToJson(versions);
            PlayerPrefs.SetString(networkName + "_" + country, jsonStr);
            PlayerPrefs.Save();
        }

        public static void saveLocalCountry(int country)
        {
            PlayerPrefs.SetInt(LAST_SELECT_COUNTRY_KEY, country);
            PlayerPrefs.Save();
        }
        //获取已安装的network版本，包括core network
        public static Versions getInstalledNetworkVersion(string networkName, int country)
        {
            string key = networkName + "_" + country;
            string sdkJson = PlayerPrefs.GetString(key);
            ATLog.log("getInstalledNetworkVersion() >>>  networkName: " + networkName + " sdkJson: " + sdkJson + " country: " + country);
            if (sdkJson == null || sdkJson.Length == 0)
            {
                return null;
            }

            Versions versions = JsonUtility.FromJson<Versions>(sdkJson);
            versions = initUnityForVerions(versions, networkName, country);
            return versions;
        }

        private static Versions initUnityForVerions(Versions versions, string networkName, int country)
        {
            if (versions == null)
            {
                return null;
            }
            //查找本地的android、iOS是否都已安装
            string androidPath = getAndroidNetworkPath(networkName, country);
            bool androidInstalled = isInstalledByPath(androidPath);
            string iosPath = getIosNetworkPath(networkName);
            bool iosInstalled = isInstalledByPath(iosPath);
            if (!androidInstalled && iosInstalled)
            {
                // versions.Android = "";
                versions.Unity = string.Format("ios_{0}", versions.Ios);
            }
            else if (androidInstalled && !iosInstalled)
            {
                // versions.Ios = "";
                versions.Unity = string.Format("android_{0}", versions.Android);
            }
            else if (!androidInstalled && !iosInstalled)
            {
                versions.Unity = "";
            }
            //core network的unity版本是插件版本
            if (networkName.Equals(ATIntegrationManager.AnyThinkNetworkName))
            {
                versions.Unity = ATConfig.PLUGIN_VERSION;
            }
            ATLog.log("initUnityForVerions() >>> networkName: " + networkName + " androidInstalled: " + androidInstalled + " iosInstalled: " + iosInstalled + " unity: " + versions.Unity);
            return versions;
        }

        public static bool isNetworkInstalled(string networkName, int country)
        {
            Versions versions = getInstalledNetworkVersion(networkName, country);
            if (versions == null)
            {
                return false;
            }
            if (networkName.Equals(ATIntegrationManager.AnyThinkNetworkName))    //core network判断是否已安装
            {
                string androidPath = getAndroidNetworkPath(networkName, country);
                bool androidInstalled = isInstalledByPath(androidPath);
                string iosPath = getIosNetworkPath(networkName);
                bool iosInstalled = isInstalledByPath(iosPath);

                return androidInstalled && iosInstalled;
            } else {    //network判断是否已安装
                return !string.IsNullOrEmpty(versions.Unity);
            }
        }

        public static bool isAndroidNetworkInstalled(string networkName, int country)
        {
            Versions versions = getInstalledNetworkVersion(networkName, country);
            if (versions == null)
            {
                return false;
            }
            string androidPath = getAndroidNetworkPath(networkName, country);
            bool androidInstalled = isInstalledByPath(androidPath);
            return androidInstalled;
        }

        public static bool isIOSNetworkInstalled(string networkName, int country)
        {
            Versions versions = getInstalledNetworkVersion(networkName, country);
            if (versions == null)
            {
                return false;
            }
            string iosPath = getIosNetworkPath(networkName);
            bool iosInstalled = isInstalledByPath(iosPath);
            return iosInstalled;
        }

        private static string getAndroidNetworkPath(string networkName, int country)
        {
            if (networkName.Equals(ATIntegrationManager.AnyThinkNetworkName))
            {
                return country == CHINA_COUNTRY ? CHINA_ANDROID_CORE_FILES_PATH : NONCHINA_ANDROID_CORE_FILES_PATH;
            }
            else
            {
                return country == CHINA_COUNTRY ? CHINA_ANDROID_NETWORK_FILES_PARENT_PATH + networkName.ToLower() : NONCHINA_ANDROID_NETWORK_FILES_PARENT_PATH + networkName.ToLower();
            }
        }

        private static string getIosNetworkPath(string networkName)
        {
            string defaultResult = IOS_NETWORK_FILES_PARENT_PATH + networkName.ToLower();
            if (networkName.Equals(ATIntegrationManager.AnyThinkNetworkName))
            {
                return IOS_NETWORK_FILES_PARENT_PATH + networkName; //Core network在本地的目录名称首字母是大写
            }
            // else if (networkName.Equals("Pangle"))
            // {
            //     //获取SDK版本
            //     // Network coreNework = ATPluginSetting.Instance.CoreNetwork;
            //     // if (coreNework != null && coreNework.CurrentVersions != null)
            //     // {
            //     //     string iosSdkVersion = coreNework.CurrentVersions.Ios;
            //     //     string compareVersion = "6.1.78";
            //     //     ATLog.log("getIosNetworkPath() >>> iosSdkVersion: " + iosSdkVersion);
            //     //     VersionComparisonResult comparisonResult = ATDataUtil.CompareVersions(iosSdkVersion, compareVersion);
            //     //     if (comparisonResult == VersionComparisonResult.Lesser) //小于6.1.78
            //     //     {
            //     //         string pangleName = isSelectedChina() ? "pangle_China" : "pangle_nonChina";
            //     //         ATLog.log("getIosNetworkPath() >>> pangleName: " + pangleName);
            //     //         return IOS_NETWORK_FILES_PARENT_PATH + pangleName;
            //     //     }
            //     // }
            //     string pangleName = isSelectedChina() ? "pangle_China" : "pangle_nonChina";
            //      // ATLog.log("getIosNetworkPath() >>> pangleName: " + pangleName);
            //     return IOS_NETWORK_FILES_PARENT_PATH + pangleName;
            // }
            else if (networkName.Equals("MyTarget"))
            {
                return IOS_NETWORK_FILES_PARENT_PATH + networkName;
            }
            return defaultResult;
        }


        public static int getLocalCountry()
        {
            return PlayerPrefs.GetInt(LAST_SELECT_COUNTRY_KEY, CHINA_COUNTRY); //默认是国内
            // return CHINA_COUNTRY;
        }

        public static bool isSelectedChina()
        {
            return getLocalCountry() == CHINA_COUNTRY;
        }

        public static void removeInstalledNetworkVersion(string networkName, int country)
        {
            // int country = getLocalCountry();
            PlayerPrefs.DeleteKey(networkName + "_" + country);
        }

        public static string[] getNetworkFilesPath(string networkName, int country)
        {
            // if (networkName.Equals(ATIntegrationManager.AnyThinkNetworkName)) {
            //     return new string[]{ANYTHINK_SDK_FILES_PATH};
            // }
            string[] filesPath = new string[2];
            filesPath[0] = getAndroidNetworkPath(networkName, country);
            filesPath[1] = getIosNetworkPath(networkName);
            return filesPath;
        }

        private static bool isInstalledByPath(string path)
        {
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);
                return files != null && files.Length > 0;
            }
            return false;
        }

        // private void OnEnable()
        // {
        //     EditorApplication.quitting += Save;
        // }

        // private void OnDisable()
        // {
        //     EditorApplication.quitting -= Save;
        // }

        // private void Save()
        // {
        //     // EditorPrefs.SetInt("MySetting", 1); // 保存您的EditorPrefs数据
        //     PlayerPrefs.Save(); // 确保数据已被保存到磁盘上
        // }
    }

}