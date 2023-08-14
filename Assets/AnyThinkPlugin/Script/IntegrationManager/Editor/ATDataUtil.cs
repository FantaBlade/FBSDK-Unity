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
    public class ATDataUtil
    {
        // private static string LAST_SELECT_COUNTRY_KEY = "country_key"; //当前选择的country
        // public static Network[] chinaNetworks;
        // public static Network[] nonChinaNetworks;

        public static Network coreNetwork;

        public static Network[] parseNetworksJson(PluginData pluginData, string netowrksJson)
        {
            try
            {
                int country = pluginData.country;
                bool isChinaCountry = isChina(country);

                ServerNetworks serverNetworks = JsonUtility.FromJson<ServerNetworks>(netowrksJson);
                ServerNetworkInfo[] serverNetworkInfoList;
                Network[] networks;
                if (isChinaCountry)
                {
                    serverNetworkInfoList = serverNetworks.ChinaSdk;
                    networks = formatServerNetworks(pluginData, country, serverNetworkInfoList);
                    // networks = chinaNetworks;
                }
                else
                {
                    serverNetworkInfoList = serverNetworks.GlobalSdk;
                    networks = formatServerNetworks(pluginData, country, serverNetworkInfoList);
                    // networks = nonChinaNetworks;
                }
                return networks;
            }
            catch (Exception e)
            {
                // 错误处理代码
                ATLog.log("parseNetworksJson() >>> failed: " + e);
            }

            return null;
        }

        public static PluginData parsePluginDataJson(string serverPluginVersionJson)
        {
            ATLog.log("plugin version data: " + serverPluginVersionJson);

            try
            {
                var pluginData = new PluginData();
                // var anythink = new Network();
                ServerPluginVersion serverPluginVersion = JsonUtility.FromJson<ServerPluginVersion>(serverPluginVersionJson);
                pluginData.networkUrlVersion = serverPluginVersion.networkUrlVersion;
                pluginData.country = ATConfig.getLocalCountry();
                pluginData.pluginVersion = serverPluginVersion.pluginVersion;
                return pluginData;
            }
            catch (Exception e)
            {
                // 错误处理代码
                ATLog.log("parse version data failed: " + e);
            }

            return null;
        }

        public static Network[] formatServerNetworks(PluginData pluginData, int country, ServerNetworkInfo[] networkInfoList)
        {
            if (networkInfoList == null || networkInfoList.Length == 0)
            {
                return null;
            }
            int length = networkInfoList.Length;
            List<Network> networkList = new List<Network>();

            for (int i = 0; i < length; i++)
            {
                Network network = new Network();
                ServerNetworkInfo serverInfo = networkInfoList[i];
                network.Name = serverInfo.name;
                network.DisplayName = serverInfo.displayName;
                network.DownloadUrl = serverInfo.downloadUrl;
                network.PluginFileName = serverInfo.pluginFileName;
                Versions latestVersion = new Versions();
                ServerNetworkVersion serverVersion = serverInfo.versions;
                if (serverVersion != null)
                {
                    latestVersion.Android = serverVersion.android;
                    latestVersion.Ios = serverVersion.ios;
                    latestVersion.Unity = serverVersion.unity;
                }
                network.LatestVersions = latestVersion;

                networkList.Add(network);
            }
            Network coreNetwork = networkList.First<Network>(n => n.Name.Equals(ATIntegrationManager.AnyThinkNetworkName));
            //先初始化core network
            if (coreNetwork != null)
            {
                // 获取当前已安装network的版本号
                Versions curVerions = ATConfig.getInstalledNetworkVersion(coreNetwork.Name, country);
                Versions latestVersion = coreNetwork.LatestVersions;
                if (latestVersion == null)
                {
                    latestVersion = new Versions();
                }
                latestVersion.Unity = pluginData.pluginVersion;
                coreNetwork.LatestVersions = latestVersion;
                ATLog.log("coreNetwork latestVersion: Android=" + latestVersion.Android);
                if (curVerions != null)
                {
                    coreNetwork.CurrentVersions = curVerions;
                    coreNetwork.CurrentToLatestVersionComparisonResult = getVersionComparisonResult(curVerions, latestVersion, false);
                }
                
                ATPluginSetting.Instance.CoreNetwork = coreNetwork;
            }
            networkList.Remove(coreNetwork);
            //后初始化network
            foreach (var network in networkList)
            {
                Versions curVerion = ATConfig.getInstalledNetworkVersion(network.Name, country);
                network.CurrentVersions = curVerion;
                VersionComparisonResult result = getVersionComparisonResult(curVerion, network.LatestVersions, true);
                ATLog.log("formatServerNetworks() >>> compareResult: " + result);
                network.CurrentToLatestVersionComparisonResult = result;
            }

            Network[] networks = (Network[])networkList.ToArray();
            //排序
            Array.Sort(networks);
            return networks;
        }

        // public bool isChina()
        // {
        //     int result = EditorPrefs.GetInt(LAST_SELECT_COUNTRY_KEY, 1);
        //     return result == 1;
        // }

        public static bool isChina(int country)
        {
            return country == ATConfig.CHINA_COUNTRY;
        }

        public static VersionComparisonResult getVersionComparisonResult(Versions curVersion, Versions latestVersion, bool isNetwork = false)
        {
            if (curVersion == null || latestVersion == null)
            {
                return VersionComparisonResult.Equal;
            }
            string curUnity = curVersion.Unity;
            string latestUnity = latestVersion.Unity;
            string curAndroid = curVersion.Android;
            string latestAndroid = latestVersion.Android;
            string curIos = curVersion.Ios;
            string latestIos = latestVersion.Ios;

            VersionComparisonResult compareVersionResult = VersionComparisonResult.Equal;
            if (isNetwork)
            {
                if (string.IsNullOrEmpty(curUnity) || string.IsNullOrEmpty(latestUnity))
                {
                    return VersionComparisonResult.Equal;
                }
                compareVersionResult = CompareUnityMediationVersions(curVersion.Unity, latestVersion.Unity);
            }
            else
            {
                var unityVersionComparison = CompareVersions(curUnity, latestUnity);
                var androidVersionComparison = CompareVersions(curAndroid, latestAndroid);
                var iosVersionComparison = CompareVersions(curIos, latestIos);

                if (unityVersionComparison == VersionComparisonResult.Equal &&
                 androidVersionComparison == VersionComparisonResult.Equal &&
                 iosVersionComparison == VersionComparisonResult.Equal)
                {
                    compareVersionResult = VersionComparisonResult.Equal;
                }
                else if (unityVersionComparison == VersionComparisonResult.Lesser ||
                     androidVersionComparison == VersionComparisonResult.Lesser ||
                     iosVersionComparison == VersionComparisonResult.Lesser)
                {
                    compareVersionResult = VersionComparisonResult.Lesser;
                }
                else
                {
                    compareVersionResult = VersionComparisonResult.Greater;
                }
            }
            return compareVersionResult;
        }

        // public static long compareVersionToInt(string currentVersion, string currentNewVersion)
        // {
        //     if (currentVersion == null || currentVersion.Length == 0
        //      || currentNewVersion == null || currentNewVersion.Length == 0)
        //     {
        //         return 0;
        //     }

        //     ATLog.log("compareVersionToInt() >>> before curVersion: " + currentVersion + " curNewVersion: " + currentNewVersion);
        //     string curVersion = currentVersion.Replace(".", "").Replace("_", "").Replace("android", "").Replace("ios", "").Replace(" ", "");
        //     string curNewVersion = currentNewVersion.Replace(".", "").Replace("_", "").Replace("android", "").Replace("ios", "").Replace(" ", "");
        //     ATLog.log("compareVersionToInt() >>> after curVersion: " + curVersion + " curNewVersion: " + curNewVersion);
        //     long intCurVersion = long.Parse(curVersion);
        //     long intNewVersion = long.Parse(curNewVersion);
        //     return intNewVersion - intCurVersion;
        // }

        //比较Unity集成的版本
        public static VersionComparisonResult CompareUnityMediationVersions(string versionA, string versionB)
        {
            if (versionA.Equals(versionB)) return VersionComparisonResult.Equal;

            // Unity version would be of format:      android_w.x.y.z_ios_a.b.c.d
            // For Android only versions it would be: android_w.x.y.z
            // For iOS only version it would be:      ios_a.b.c.d

            // After splitting into their respective components, the versions would be at the odd indices.
            var versionAComponents = versionA.Split('_').ToList();
            var versionBComponents = versionB.Split('_').ToList();

            var androidComparison = VersionComparisonResult.Equal;
            if (versionA.Contains("android") && versionB.Contains("android"))
            {
                var androidVersionA = versionAComponents[1];
                var androidVersionB = versionBComponents[1];
                androidComparison = CompareVersions(androidVersionA, androidVersionB);

                // Remove the Android version component so that iOS versions can be processed.
                versionAComponents.RemoveRange(0, 2);
                versionBComponents.RemoveRange(0, 2);
            }
            else if (versionA.Contains("android"))
            {
                androidComparison = VersionComparisonResult.Greater;

                // Remove the Android version component so that iOS versions can be processed.
                versionAComponents.RemoveRange(0, 2);
            }
            else if (versionB.Contains("android"))
            {
                androidComparison = VersionComparisonResult.Lesser;

                // Remove the Android version component so that iOS version can be processed.
                versionBComponents.RemoveRange(0, 2);
            }

            var iosComparison = VersionComparisonResult.Equal;
            if (versionA.Contains("ios") && versionB.Contains("ios"))
            {
                var iosVersionA = versionAComponents[1];
                var iosVersionB = versionBComponents[1];
                iosComparison = CompareVersions(iosVersionA, iosVersionB);
            }
            else if (versionA.Contains("ios"))
            {
                iosComparison = VersionComparisonResult.Greater;
            }
            else if (versionB.Contains("ios"))
            {
                iosComparison = VersionComparisonResult.Lesser;
            }


            // If either one of the Android or iOS version is greater, the entire version should be greater.
            return (androidComparison == VersionComparisonResult.Greater || iosComparison == VersionComparisonResult.Greater) ? VersionComparisonResult.Greater : VersionComparisonResult.Lesser;
        }
        //只比较Android、iOS
        public static VersionComparisonResult CompareVersions(string versionA, string versionB)
        {
            if (string.IsNullOrEmpty(versionA) || string.IsNullOrEmpty(versionB) || versionA.Equals(versionB))
            {
                return VersionComparisonResult.Equal;
            }

            // var versionAComponents = versionA.Replace(".", "");
            // var versionBComponents = versionB.Replace(".", "");
            // var charArrayA = versionAComponents.ToCharArray();
            // var charArrayB = versionBComponents.ToCharArray();

            // for(var i = 0; i < charArrayA.Length; i++)
            // {
            //     var aVersion = 0; 
            //     int.TryParse(charArrayA[i].ToString(), out aVersion);
            //     var bVersion = 0;
            //     int.TryParse(charArrayB[i].ToString(), out bVersion);

            //     if (aVersion < bVersion) return VersionComparisonResult.Lesser;
            //     if (aVersion > bVersion) return VersionComparisonResult.Greater;
            // }

            try
            {
                var aVersionArrays = versionA.Split('.');
                var bVersionArrays = versionB.Split('.');

                var arrayLength = Mathf.Min(aVersionArrays.Length, bVersionArrays.Length);
                for (var i = 0; i < arrayLength; i++)
                {
                    var aVersionStr = aVersionArrays[i];
                    var bVersionStr = bVersionArrays[i];

                    var aVersionInt = int.Parse(aVersionStr);
                    var bVersionInt = int.Parse(bVersionStr);

                    if (i == arrayLength - 1) //末尾最后一个
                    {
                        if (aVersionStr.Length > bVersionStr.Length)
                        {
                            int gapLength = aVersionStr.Length - bVersionStr.Length;
                            bVersionInt = bVersionInt * (gapLength * 10);
                        }
                        else if (aVersionStr.Length < bVersionStr.Length)
                        {
                            int gapLength = bVersionStr.Length - aVersionStr.Length;
                            aVersionInt = aVersionInt * (gapLength * 10);
                        }
                    }
                    if (aVersionInt < bVersionInt) return VersionComparisonResult.Lesser;
                    if (aVersionInt > bVersionInt) return VersionComparisonResult.Greater;
                }
            }
            catch (Exception e)
            {
                ATLog.logError("CompareVersions failed: " + e.Message);
            }

            // // Split the version string into beta component and the underlying version.
            // int piece;

            // // Compare the non beta component of the version string.
            // var versionAComponents = versionA.Split('.').Select(version => int.TryParse(version, out piece) ? piece : 0).ToArray();
            // var versionBComponents = versionB.Split('.').Select(version => int.TryParse(version, out piece) ? piece : 0).ToArray();
            // var length = Mathf.Max(versionAComponents.Length, versionBComponents.Length);
            // for (var i = 0; i < length; i++)
            // {
            //     var aComponent = i < versionAComponents.Length ? versionAComponents[i] : 0;
            //     var bComponent = i < versionBComponents.Length ? versionBComponents[i] : 0;

            //     if (aComponent < bComponent) return VersionComparisonResult.Lesser;

            //     if (aComponent > bComponent) return VersionComparisonResult.Greater;
            // }

            return VersionComparisonResult.Equal;
        }
    }

    //后端下发的Plugin、android、iOS版本号
    [Serializable]
    public class ServerPluginVersion
    {
        public string platformName;
        public string networkUrlVersion;
        public string pluginVersion;
        public string androidVersion;
        public string iosVersion;
        public string[] versionList;
    }

    [Serializable]
    public class ServerNetworks
    {
        public ServerNetworkInfo[] GlobalSdk;
        public ServerNetworkInfo[] ChinaSdk;
        public string platformName;
        public string version;
    }
    [Serializable]
    public class ServerNetworkInfo
    {
        public string name;
        public string displayName;
        public string downloadUrl;
        public string pluginFileName;
        public ServerNetworkVersion versions;
    }
    [Serializable]
    public class ServerNetworkVersion
    {
        public string android;
        public string ios;
        public string unity;
    }
}