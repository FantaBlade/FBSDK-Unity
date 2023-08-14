using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;


using UnityEngine;

namespace AnyThink.Scripts.IntegrationManager.Editor
{
    public static class ATNetInfo {
        public static string platformName = "AnyThink";
        public  static string ATDownloadDir = "Assets/AnyThinkAds/Dependencies/";
        public  static string ATDependencyDir = "Assets/AnyThinkAds/Plugins/";
        public  static string sdk = "sdk";
        public  static string Android = "Android";
        public  static string iOS = "iOS";
        public static string localConfig =  "Assets/AnyThinkPlugin/Editor/localConfig.json";
        private static string unityPluginConfigUrl = "https://topon-sdk-release.oss-cn-hangzhou.aliyuncs.com/Unity/unity_plugin_config.json";  //插件和nythink的版本号列表
        private static string unityPluginConfigUrlDebug = "https://topon-sdk-release.oss-cn-hangzhou.aliyuncs.com/Unity/unity_plugin_config_debug.json";  //插件和nythink的版本号列表
        public static string packagePath = "https://topon-sdk-release.oss-cn-hangzhou.aliyuncs.com/Unity/";
        public static int isGlobal=1; //0:国外 //1:国内

        public static string getPluginConfigUrl() 
        {
            return ATConfig.isDebug ? unityPluginConfigUrlDebug : unityPluginConfigUrl;
        }

        public static string getNetworkListUrl(String ver)
        {
            return "https://topon-sdk-release.oss-cn-hangzhou.aliyuncs.com/Unity/"+ver+"/network_new.json";
        }

        public static string getPluginFileName(string pluginVersion)
        {
            return "AnyThinkPlugin_" + pluginVersion + ".unitypackage";
        }
        //https://topon-sdk-release.oss-cn-hangzhou.aliyuncs.com/Unity/Plugin/2.0.0/AnyThinkPlugin_2.0.0.unitypackage
        public static string getPluginDownloadUrl(string pluginVersion)
        {
            return "https://topon-sdk-release.oss-cn-hangzhou.aliyuncs.com/Unity/Plugin/" + pluginVersion + "/" + getPluginFileName(pluginVersion);
        }
    }
    
}
