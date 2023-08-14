using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
//using UnityEditor.iOS_I2Loc.Xcode;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using UnityEngine;
public static class PlayerPackager
{
    [PostProcessBuild(10001)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {

        if (buildTarget == BuildTarget.iOS)
        {
#if UNITY_IOS
            string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
             
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
#if UNITY_2019_3_OR_NEWER
            string tmpMainTarget = proj.GetUnityMainTargetGuid();
            proj.SetBuildProperty(tmpMainTarget, "ENABLE_BITCODE", "false");
            string phaseId = proj.GetResourcesBuildPhaseByTarget(tmpMainTarget);
            proj.AddFileToBuild(tmpMainTarget, proj.AddFile("Frameworks/Plugins/FantaBlade/Libs/iOS/ATAuthSDK.bundle",
                "Frameworks/Plugins/FantaBlade/Libs/iOS/ATAuthSDK.bundle", PBXSourceTree.Source));
            proj.AddFileToBuild(tmpMainTarget, proj.AddFile("Frameworks/Plugins/FantaBlade/Libs/iOS/libWeiboSDK/WeiboSDK.bundle",
                "Frameworks/Plugins/FantaBlade/Libs/iOS/libWeiboSDK/WeiboSDK.bundle", PBXSourceTree.Source));
            string target = proj.GetUnityFrameworkTargetGuid();
#else
            string target = proj.TargetGuidByName("Unity-iPhone");
#endif

            // TapDB不支持bitcode
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "false");
            proj.SetBuildProperty(target, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
            proj.SetBuildProperty(target, "GCC_C_LANGUAGE_STANDARD", "gnu99");
            // proj.SetBuildProperty(target, "USYM_UPLOAD_AUTH_TOKEN", "FakeToken");
            proj.RemoveFrameworkFromProject(target, "CoreLocation.framework");
            // weixin
            proj.AddFrameworkToProject(target, "Webkit.framework", false);
            // sign with apple
            proj.AddFrameworkToProject(target, "AuthenticationServices.framework", false);
            // douyin
            proj.AddFrameworkToProject(target, "Photos.framework", false);
            // weibo
            // proj.AddFrameworkToProject(target, "CoreTelephony.framework", false);

            // TapDB 3.0.2
            proj.AddFrameworkToProject(target, "AdSupport.framework", false);
            proj.AddFrameworkToProject(target, "AdServices.framework", true);
            proj.AddFrameworkToProject(target, "AppTrackingTransparency.framework", true);
            proj.AddFrameworkToProject(target, "CoreMotion.framework", true);
            proj.AddFrameworkToProject(target, "SystemConfiguration.framework", false);
            proj.AddFrameworkToProject(target, "Security.framework", false);
            proj.AddFrameworkToProject(target, "Accelerate.framework", false);
            proj.AddFrameworkToProject(target, "MediaPlayer.framework", false);
            proj.AddFrameworkToProject(target, "MobileCoreServices.framework", false);
            proj.AddFileToBuild(target, proj.AddFile("usr/lib/libresolv.tbd", "Frameworks/libresolv.tbd", PBXSourceTree.Sdk));
            proj.AddFileToBuild(target, proj.AddFile("usr/lib/libz.tbd", "Frameworks/libz.tbd", PBXSourceTree.Sdk));
            proj.AddFileToBuild(target, proj.AddFile("usr/lib/libbz2.tbd", "Frameworks/libbz2.tbd", PBXSourceTree.Sdk));
            proj.AddFileToBuild(target, proj.AddFile("usr/lib/libc++.tbd", "Frameworks/libc++.tbd", PBXSourceTree.Sdk));
            proj.AddFileToBuild(target, proj.AddFile("usr/lib/libxml2.tbd", "Libraries/libxml2.tbd", PBXSourceTree.Sdk));
            proj.AddFileToBuild(target, proj.AddFile("usr/lib/libresolv.9.tbd", "Libraries/libresolv.9.tbd", PBXSourceTree.Sdk));
#if UNITY_2019_3_OR_NEWER
            // proj.AddFileToBuild(target, proj.AddFile("Frameworks/AnyThinkAds/Plugins/iOS/Core/AnyThinkSDK.bundle", "Frameworks/AnyThinkAds/Plugins/iOS/Core/AnyThinkSDK.bundle", PBXSourceTree.Sdk));
#else
#endif

            // 微信编译
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");
            //proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-all_load");
            // qq
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-fobjc-arc");
            //
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-lc++");
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-lc++abi");
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-lsqlite3");
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-lz");
            
            File.WriteAllText(projPath, proj.WriteToString());
            Debug.Log("Save project.pbxproj.");
            
            var preprocessorPath = path + "/Classes/Preprocessor.h";
            var preprocessor = File.ReadAllText(preprocessorPath);
            if (preprocessor.Contains("UNITY_USES_LOCATION"))
                preprocessor = preprocessor.Replace("UNITY_USES_LOCATION 1", "UNITY_USES_LOCATION 0");

            string mainTarget;

            var unityMainTargetGuidMethod = proj.GetType().GetMethod("GetUnityMainTargetGuid");
            var unityFrameworkTargetGuidMethod = proj.GetType().GetMethod("GetUnityFrameworkTargetGuid");

            if (unityMainTargetGuidMethod != null && unityFrameworkTargetGuidMethod != null)
            {
                mainTarget = (string)unityMainTargetGuidMethod.Invoke(proj, null);
            }
            else
            {
#if UNITY_2019_3_OR_NEWER
                mainTarget = proj.GetUnityFrameworkTargetGuid();
#else
                mainTarget = proj.TargetGuidByName("Unity-iPhone");
#endif
            }
            var entitlementsFileName = proj.GetBuildPropertyForAnyConfig(mainTarget, "CODE_SIGN_ENTITLEMENTS");
            if (entitlementsFileName == null)
            {
                var bundleIdentifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);
                entitlementsFileName =
 string.Format("{0}.entitlements", bundleIdentifier.Substring(bundleIdentifier.LastIndexOf(".") + 1));
            }

            // var capManager = new ProjectCapabilityManager(projPath, entitlementsFileName, targetGuid: mainTarget);
            // capManager.AddAssociatedDomains(new []{
            //     "applinks:watergun.hotfix.huanrengame.com"
            // });
            // capManager.AddSignInWithApple();
            // capManager.WriteToFile();

            File.WriteAllText(preprocessorPath, preprocessor);
             
            // Add url schema to plist file
            // string plistPath = path + "/Info.plist";
            // PlistDocument plist = new PlistDocument();
            // plist.ReadFromString(File.ReadAllText(plistPath));
            //  
            // // Get root
            // PlistElementDict rootDict = plist.root;
            // rootDict.SetBoolean("UIRequiresFullScreen",true);
            // plist.WriteToFile(plistPath);
#endif
        }
        else if (buildTarget == BuildTarget.Android)
        {
            string manifestPath = string.Format("{0}/src/main/AndroidManifest.xml", path);
            // if (!File.Exists(manifestPath))
            //     return;
            if (FantaBlade.Mediation.FantaBladeMediation.Channel.Equals("Quick"))
            {
                // File.Copy("Assets/Plugins/FantaBlade/AndroidManifest.xml",manifestPath,true);
                // manifestDoc = AppendAndroidPermissionField(manifestDoc,"android.permission.GET_TASKS");
                // manifestDoc = AppendAndroidPermissionField(manifestDoc,"android.permission.WRITE_EXTERNAL_STORAGE");
                // manifestDoc = AppendAndroidPermissionField(manifestDoc,"android.permission.CHANGE_WIFI_STATE");
                // manifestDoc = AppendAndroidPermissionField(manifestDoc,"android.permission.INTERNET");
                // manifestDoc = AppendAndroidPermissionField(manifestDoc,"android.permission.ACCESS_NETWORK_STATE");
                // manifestDoc = AppendAndroidPermissionField(manifestDoc,"android.permission.READ_PHONE_STATE");
                // manifestDoc = AppendAndroidPermissionField(manifestDoc,"android.permission.BLUETOOTH");
                // manifestDoc = AppendAndroidPermissionField(manifestDoc,"android.permission.REQUEST_INSTALL_PACKAGES");
                // manifestDoc = AppendAndroidPermissionField(manifestDoc,"android.permission.SYSTEM_ALERT_WINDOW");
            }
            else
            {
                // XmlDocument manifestDoc = new XmlDocument();
                // manifestDoc.Load(manifestPath);
                // manifestDoc = AppendAndroidPermissionField(manifestDoc,"android.permission.INTERNET");
                // manifestDoc = AppendAndroidPermissionField(manifestDoc,"android.permission.ACCESS_NETWORK_STATE");
                // manifestDoc = AppendAndroidPermissionField(manifestDoc,"android.permission.ACCESS_WIFI_STATE");
                // manifestDoc = AppendAndroidPermissionField(manifestDoc,"android.permission.READ_PHONE_STATE");
                // manifestDoc = AppendAndroidPermissionField(manifestDoc,"android.permission.READ_LOGS");
                // manifestDoc.Save(manifestPath);
            }
        }
    }
    
    public static string IOS_TEAM_ID = "8SZC2M5QS4";
    [MenuItem("FB Publish/Build iOS", false, 207)]
    public static void BuildiOS()
    {
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.appleDeveloperTeamID = IOS_TEAM_ID;

        BuildPlayer("out", BuildTarget.iOS,
            BuildOptions.CompressWithLz4HC);
    }
    
    [MenuItem("FB Publish/Build Apk", false, 108)]
    public static void BuildSnapshotApk()
    {
        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.Android.keystoreName = "";
        PlayerSettings.Android.keyaliasName = "";
        BuildPlayer("out", BuildTarget.Android,
            BuildOptions.Development | BuildOptions.CompressWithLz4HC);
    }
    
    private static void BuildPlayer(string locationPathName, BuildTarget target, BuildOptions options)
    {
        var time = DateTime.Now;
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        string[] scenes = EditorBuildSettings.scenes.Where(e => e != null && e.enabled).Select(e => e.path).ToArray();
        BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(target);
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = locationPathName,
            targetGroup = buildTargetGroup,
            target = target,
            options = options
        };
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("BuildPlayer finished! "+(DateTime.Now-time).TotalSeconds+"s");
        if (Application.isBatchMode && report.summary.result != BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
    }
}