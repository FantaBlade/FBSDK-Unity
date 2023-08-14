using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AnyThink.Scripts.IntegrationManager.Editor
{
    public class ATIntegrationManagerWindow : EditorWindow
    {

        private const string windowTitle = "AnyThink Integration Manager";
        private const string uninstallIconExportPath = "AnyThinkPlugin/Resources/Images/uninstall_icon.png";
        private const string alertIconExportPath = "AnyThinkPlugin/Resources/Images/alert_icon.png";
        private const string warningIconExportPath = "AnyThinkPlugin/Resources/Images/warning_icon.png";

        private static readonly Color darkModeTextColor = new Color(0.29f, 0.6f, 0.8f);
        private GUIStyle titleLabelStyle;
        private GUIStyle headerLabelStyle;
        private GUIStyle environmentValueStyle;
        private GUIStyle wrapTextLabelStyle;
        private GUIStyle linkLabelStyle;
        private GUIStyle iconStyle;
        private Texture2D uninstallIcon;
        private Texture2D alertIcon;
        private Texture2D warningIcon;
        private Vector2 scrollPosition;
        private static readonly Vector2 windowMinSize = new Vector2(750, 750);
        private const float actionFieldWidth = 60f;
        private const float upgradeAllButtonWidth = 80f;
        private const float networkFieldMinWidth = 100f;
        private const float versionFieldMinWidth = 190f;
        private const float privacySettingLabelWidth = 200f;
        private const float networkFieldWidthPercentage = 0.22f;
        private const float versionFieldWidthPercentage = 0.36f; // There are two version fields. Each take 40% of the width, network field takes the remaining 20%.
        private static float previousWindowWidth = windowMinSize.x;
        private static GUILayoutOption networkWidthOption = GUILayout.Width(networkFieldMinWidth);
        private static GUILayoutOption versionWidthOption = GUILayout.Width(versionFieldMinWidth);

        private static GUILayoutOption sdkKeyTextFieldWidthOption = GUILayout.Width(520);

        private static GUILayoutOption privacySettingFieldWidthOption = GUILayout.Width(400);
        private static readonly GUILayoutOption fieldWidth = GUILayout.Width(actionFieldWidth);
        private static readonly GUILayoutOption upgradeAllButtonFieldWidth = GUILayout.Width(upgradeAllButtonWidth);

        private ATEditorCoroutine loadDataCoroutine;
        private PluginData pluginData;
        private bool pluginDataLoadFailed;
        private bool networkButtonsEnabled = true;
        private bool shouldShowGoogleWarning;
        private int curSelectCountryInt;
        // private int dropdownIndex = 0;


        public static void ShowManager()
        {
            var manager = GetWindow<ATIntegrationManagerWindow>(utility: true, title: windowTitle, focus: true);
            manager.minSize = windowMinSize;
        }
        //定义UI的Style
        private void Awake()
        {
            titleLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                fixedHeight = 20
            };

            headerLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                fixedHeight = 18
            };

            environmentValueStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight
            };

            linkLabelStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                normal = { textColor = EditorGUIUtility.isProSkin ? darkModeTextColor : Color.blue }
            };

            wrapTextLabelStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true
            };

            iconStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedWidth = 18,
                fixedHeight = 18,
                padding = new RectOffset(1, 1, 1, 1)
            };

            // Load uninstall icon texture.
            var uninstallIconData = File.ReadAllBytes(ATSdkUtil.GetAssetPathForExportPath(uninstallIconExportPath));
            uninstallIcon = new Texture2D(0, 0, TextureFormat.RGBA32, false); // 1. Initial size doesn't matter here, will be automatically resized once the image asset is loaded. 2. Set mipChain to false, else the texture has a weird blurry effect.
            uninstallIcon.LoadImage(uninstallIconData);

            // Load alert icon texture.
            var alertIconData = File.ReadAllBytes(ATSdkUtil.GetAssetPathForExportPath(alertIconExportPath));
            alertIcon = new Texture2D(0, 0, TextureFormat.RGBA32, false);
            alertIcon.LoadImage(alertIconData);

            // Load warning icon texture.
            var warningIconData = File.ReadAllBytes(ATSdkUtil.GetAssetPathForExportPath(warningIconExportPath));
            warningIcon = new Texture2D(0, 0, TextureFormat.RGBA32, false);
            warningIcon.LoadImage(warningIconData);

            ATLog.log("Awake() >>> called");
            ATAssetDatabaseManager.Instance.downloadPluginProgressCallback = OnDownloadPluginProgress;
            ATAssetDatabaseManager.Instance.importPackageCompletedCallback = OnImportPackageCompleted;
            ATLog.log("OnEnable() >>> called");
            loadPluginData();
        }

        //这个方法在插件启动时会调用，然后脚本重新加载时也会重新调用，所以加载数据放在Awake中
        private void OnEnable()
        {
            if (ATAssetDatabaseManager.Instance.downloadPluginProgressCallback == null)
            {
                ATAssetDatabaseManager.Instance.downloadPluginProgressCallback = OnDownloadPluginProgress;
            }

            if (ATAssetDatabaseManager.Instance.importPackageCompletedCallback == null)
            {
                ATAssetDatabaseManager.Instance.importPackageCompletedCallback = OnImportPackageCompleted;
            }
        }

        private void OnDisable()
        {
            if (loadDataCoroutine != null)
            {
                loadDataCoroutine.Stop();
                loadDataCoroutine = null;
            }

            ATIntegrationManager.Instance.CancelDownload();
            EditorUtility.ClearProgressBar();

            // Saves the AppLovinSettings object if it has been changed.
            AssetDatabase.SaveAssets();
        }


        private void OnGUI()
        {
            // OnGUI is called on each frame draw, so we don't want to do any unnecessary calculation if we can avoid it. So only calculate it when the width actually changed.
            if (Math.Abs(previousWindowWidth - position.width) > 1)
            {
                previousWindowWidth = position.width;
                CalculateFieldWidth();
            }
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition, false, false))
            {
                scrollPosition = scrollView.scrollPosition;
                GUILayout.Space(5);
                EditorGUILayout.LabelField("Region (Only for Android, iOS is not affected by region)", titleLabelStyle);
                DrawCountryUI();
                EditorGUILayout.LabelField("AnyThink Plugin Details", titleLabelStyle);
                //显示插件版本号
                DrawPluginDetails();
                using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(false)))
                {
                    EditorGUILayout.LabelField("Mediated Networks", titleLabelStyle);
                    DrawUpgradeAllButton();
                }
                // Draw mediated networks
                DrawMediatedNetworks();
            }
            if (GUI.changed)
            {
                ATPluginSetting.Instance.SaveAsync();
            }
        }

        /// <summary>
        /// Callback method that will be called with progress updates when the plugin is being downloaded.
        /// </summary>
        public static void OnDownloadPluginProgress(string pluginName, float progress, bool done)
        {
            ATLog.logFormat("OnDownloadPluginProgress() >>> pluginName: {0}, progress: {1}, done: {2}", new object[] { pluginName, progress, done });
            // Download is complete. Clear progress bar.
            if (done || progress == 1)
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
            // Download is in progress, update progress bar.
            else
            {
                if (EditorUtility.DisplayCancelableProgressBar(windowTitle, string.Format("Downloading {0} plugin...", pluginName), progress))
                {
                    ATLog.log("OnDownloadPluginProgress() >>> click cancel download");
                    ATIntegrationManager.Instance.CancelDownload();
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.Refresh();
                }
            }
        }

        private void OnImportPackageCompleted(Network network)
        {
            // var parentDirectory = network.Name.Equals(ATIntegrationManager.AnyThinkNetworkName) ? ATIntegrationManager.PluginParentDirectory : ATIntegrationManager.MediationSpecificPluginParentDirectory;
            if (pluginData != null && network != null && network.Name.Equals(ATIntegrationManager.AnyThinkNetworkName))
            {
                pluginData.anyThink = network;
                ATPluginSetting.Instance.CoreNetwork = network;
                //更新了core包，则自动更新全部network
                if (NetworksRequireUpgrade())
                {
                    startUpgradeAllNetwork();
                }
            }
            ATIntegrationManager.Instance.UpdateCurrentVersions(network);
            // UpdateShouldShowGoogleWarningIfNeeded();
        }
        //获取插件和SDK的版本数据
        private void loadPluginData()
        {
            if (loadDataCoroutine != null)
            {
                loadDataCoroutine.Stop();
            }
            loadDataCoroutine = ATEditorCoroutine.startCoroutine(ATIntegrationManager.Instance.loadPluginData(data =>
            {
                if (data == null)
                {
                    pluginDataLoadFailed = true;
                }
                else
                {
                    ATLog.log("loadNetworksData() >>> pluginData: " + data);
                    pluginData = data;
                    pluginDataLoadFailed = false;
                    loadNetworksData(data);
                    // UpdateShouldShowGoogleWarningIfNeeded();
                }

                CalculateFieldWidth();
                Repaint();
            }));
        }
        //获取networks
        private void loadNetworksData(PluginData pluginData)
        {
            ATEditorCoroutine.startCoroutine(ATIntegrationManager.Instance.loadNetworksData(pluginData, data =>
            {
                pluginData = data;
                ATIntegrationManager.Instance.downloadCorePlugin(data);
                Repaint();
            }));
        }

        private void switchCountry(int country)
        {
            // ATIntegrationManager.Instance.switchCountry(pluginData, country);
            var originCountry = ATConfig.getLocalCountry();
            pluginData.country = country;
            ATConfig.removeInstalledNetworkVersion(ATIntegrationManager.AnyThinkNetworkName, originCountry);
            var networks = pluginData.mediatedNetworks;
            if (networks != null)
            {
                foreach(var network in networks)
                {
                     ATConfig.removeInstalledNetworkVersion(network.Name, originCountry);
                }
            }
            ATConfig.saveLocalCountry(country);
            //删除国内的core包和network
            string anythinkAdsPath = ATConfig.ANYTHINK_SDK_FILES_PATH;
            if (Directory.Exists(anythinkAdsPath))
            {
                FileUtil.DeleteFileOrDirectory(anythinkAdsPath);
            }
            string metaFile = anythinkAdsPath + ".meta";
            if (File.Exists(metaFile))
            {
                FileUtil.DeleteFileOrDirectory(metaFile);
            }
            // AssetDatabase.Refresh();
            //重新开始走network
            loadPluginData();
        }

        private void CalculateFieldWidth()
        {
            var currentWidth = position.width;
            var availableWidth = currentWidth - actionFieldWidth - 80; // NOTE: Magic number alert. This is the sum of all the spacing the fields and other UI elements.
            var networkLabelWidth = Math.Max(networkFieldMinWidth, availableWidth * networkFieldWidthPercentage);
            networkWidthOption = GUILayout.Width(networkLabelWidth);

            var versionLabelWidth = Math.Max(versionFieldMinWidth, availableWidth * versionFieldWidthPercentage);
            versionWidthOption = GUILayout.Width(versionLabelWidth);

            const int textFieldOtherUiElementsWidth = 45; // NOTE: Magic number alert. This is the sum of all the spacing the fields and other UI elements.
            var availableTextFieldWidth = currentWidth - networkLabelWidth - textFieldOtherUiElementsWidth;
            sdkKeyTextFieldWidthOption = GUILayout.Width(availableTextFieldWidth);

            var availableUserDescriptionTextFieldWidth = currentWidth - privacySettingLabelWidth - textFieldOtherUiElementsWidth;
            privacySettingFieldWidthOption = GUILayout.Width(availableUserDescriptionTextFieldWidth);
        }

        private void DrawCountryUI()
        {
            // GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope("box"))
            {
                GUILayout.Space(5);

                int countryInt = ATConfig.CHINA_COUNTRY; //默认是中国
                if (pluginData != null)
                {
                    countryInt = pluginData.country;
                }

                string[] options = new string[] { "ChinaMainland", "Overseas" };
                // 创建Dropdown组件
                int curDropdownIndex = ATDataUtil.isChina(countryInt) ? 0 : 1;
                int dropdownIndex = EditorGUILayout.Popup("Select Region:", curDropdownIndex, options);

                curSelectCountryInt = dropdownIndex == 0 ? ATConfig.CHINA_COUNTRY : ATConfig.NONCHINA_COUNTRY;
                //变化才设置
                if (pluginData != null && curSelectCountryInt != countryInt)
                {
                    ATLog.log("DrawCountryUI() >>> curSelectCountryInt: " + curSelectCountryInt + " countryInt: " + countryInt);
                    //Unity需要更换Network
                    switchCountry(curSelectCountryInt);
                }
                GUILayout.Space(5);
            }
            GUILayout.Space(4);
            DrawAndroidXUI();
            // GUILayout.EndHorizontal();
        }

        private void DrawAndroidXUI()
        {   
            if (!ATDataUtil.isChina(curSelectCountryInt)) {
                return;
            }
            EditorGUILayout.LabelField("AndroidX (Only for Android)", titleLabelStyle);
            GUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope("box"))
            {
                GUILayout.Space(5);

                bool enableAndroidX = ATPluginSetting.Instance.EnableAndroidX;

                string[] options = new string[] { "Disable", "Enable" };
                // 创建Dropdown组件
                int lastDropdownIndex = enableAndroidX ? 1 : 0;
                int curDropdownIndex = EditorGUILayout.Popup("Enable AndroidX:", lastDropdownIndex, options);

                //变化才设置
                if (curDropdownIndex != lastDropdownIndex)
                {
                    ATLog.log("DrawAndroidXUI() >>> curDropdownIndex: " + curDropdownIndex + " lastDropdownIndex: " + lastDropdownIndex);
                    ATPluginSetting.Instance.EnableAndroidX = curDropdownIndex == 1;
                }
                GUILayout.Space(5);
            }
            GUILayout.Space(4);
        }

        private void DrawPluginDetails()
        {
            // GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                // Draw plugin version details
                DrawHeaders("Platform", false);
                if (pluginData == null)
                {
                    DrawEmptyPluginData();
                    return;
                }
                var anythink = pluginData.anyThink;

                // Immediately after downloading and importing a plugin the entire IDE reloads and current versions can be null in that case. Will just show loading text in that case.
                if (pluginData.anyThink == null || pluginData.anyThink.CurrentVersions == null || pluginData.anyThink.LatestVersions == null)
                {
                    DrawEmptyPluginData();
                }
                else
                {
                    ATLog.log("DrawPluginDetails() >>> anythink.CurrentVersions: " + anythink.CurrentVersions + " LatestVersions: " + anythink.LatestVersions);
                    // Check if a newer version is available to enable the upgrade button.

                    DrawPluginDetailRow("Unity 3D", anythink.CurrentVersions.Unity, pluginData.pluginVersion);
                    DrawPluginDetailRow("Android", anythink.CurrentVersions.Android, anythink.LatestVersions.Android);
                    DrawPluginDetailRow("iOS", anythink.CurrentVersions.Ios, anythink.LatestVersions.Ios);

                    // BeginHorizontal combined with FlexibleSpace makes sure that the button is centered horizontally.
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    //检查插件和Android、iOS的原生sdk是否需要更新，如果其中之一需要都显示更新按钮
                    GUI.enabled = anythink.isReqiureUpdate();
                    if (GUILayout.Button(new GUIContent("Upgrade"), fieldWidth))
                    {
                        //点击更新按钮的操作
                        ATIntegrationManager.Instance.updateAnyThinkPlugin(pluginData);
                    }

                    GUI.enabled = true;
                    GUILayout.Space(5);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                }

#if !UNITY_2018_2_OR_NEWER
                EditorGUILayout.HelpBox("AnyThink Unity plugin will soon require Unity 2018.2 or newer to function. Please upgrade to a newer Unity version.", MessageType.Warning);
#endif
            }

            GUILayout.Space(5);
            // GUILayout.EndHorizontal();
        }

        private void DrawHeaders(string firstColumnTitle, bool drawAction)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField(firstColumnTitle, headerLabelStyle, networkWidthOption);
                EditorGUILayout.LabelField("Current Version", headerLabelStyle, versionWidthOption);
                GUILayout.Space(3);
                EditorGUILayout.LabelField("Latest Version", headerLabelStyle, versionWidthOption);
                GUILayout.Space(3);
                if (drawAction)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Button("Actions", headerLabelStyle, fieldWidth);
                    GUILayout.Space(5);
                }
            }

            GUILayout.Space(4);
        }
        private void DrawEmptyPluginData()
        {
            GUILayout.Space(5);

            // Plugin data failed to load. Show error and retry button.
            if (pluginDataLoadFailed)
            {
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                EditorGUILayout.LabelField("Failed to load plugin data. Please click retry or restart the integration manager.", titleLabelStyle);
                if (GUILayout.Button("Retry", fieldWidth))
                {
                    pluginDataLoadFailed = false;
                    loadPluginData();
                }

                GUILayout.Space(5);
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
            // Still loading, show loading label.
            else
            {
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Loading data...", titleLabelStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            GUILayout.Space(5);
        }

        private void DrawPluginDetailRow(string platform, string currentVersion, string latestVersion)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField(new GUIContent(platform), networkWidthOption);
                EditorGUILayout.LabelField(new GUIContent(currentVersion), versionWidthOption);
                GUILayout.Space(3);
                EditorGUILayout.LabelField(new GUIContent(latestVersion), versionWidthOption);
                GUILayout.Space(3);
            }

            GUILayout.Space(4);
        }

        private void DrawUpgradeAllButton()
        {
            GUI.enabled = NetworksRequireUpgrade();
            if (GUILayout.Button(new GUIContent("Upgrade All"), upgradeAllButtonFieldWidth))
            {
                startUpgradeAllNetwork();
            }

            GUI.enabled = true;
            GUILayout.Space(10);
        }

        private bool NetworksRequireUpgrade()
        {
            if (pluginData == null || pluginData.mediatedNetworks == null) return false;

            var networks = pluginData.mediatedNetworks;
            if (networks == null)
            {
                return false;
            }
            try
            {
                foreach (var network in networks)
                {
                    if (network.CurrentVersions != null && !string.IsNullOrEmpty(network.CurrentVersions.Unity))
                    {
                        ATLog.log("NetworksRequireUpgrade() >>> name: " + network.Name + " isReqiureUpdate: " + network.isReqiureUpdate() +
                        " curVersion: " + network.CurrentVersions.Unity + " latestVersion: " + network.LatestVersions.Unity);
                        network.CurrentToLatestVersionComparisonResult = ATDataUtil.getVersionComparisonResult(network.CurrentVersions, network.LatestVersions, true);
                        if (network.isReqiureUpdate())
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ATLog.logError("NetworksRequireUpgrade failed: " + e.Message);
            }
            // return networks.Any(network => network.CurrentVersions != null && ATConfig.isNetworkInstalled(network.Name, ATConfig.getLocalCountry()) && network.isReqiureUpdate());
            return false;
        }

        private void DrawMediatedNetworks()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                DrawHeaders("Network", true);

                // Immediately after downloading and importing a plugin the entire IDE reloads and current versions can be null in that case. Will just show loading text in that case.
                if (pluginData == null || pluginData.mediatedNetworks == null)
                {
                    ATLog.log("DrawMediatedNetworks failed.");
                    DrawEmptyPluginData();
                }
                else
                {
                    var networks = pluginData.mediatedNetworks;
                    foreach (var network in networks)
                    {
                        DrawNetworkDetailRow(network);
                    }

                    GUILayout.Space(10);
                }
            }

            GUILayout.Space(5);
            GUILayout.EndHorizontal();
        }

        private void DrawNetworkDetailRow(Network network)
        {
            if (network == null) return;

            string action;
            var currentVersion = "";
            if (network.CurrentVersions != null)
            {
                currentVersion = network.CurrentVersions.Unity;
            }
            var latestVersion = network.LatestVersions.Unity;
            bool isActionEnabled;
            bool isInstalled;

            if (string.IsNullOrEmpty(currentVersion))
            {
                action = "Install";
                currentVersion = "Not Installed";
                isActionEnabled = true;
                isInstalled = false;
            }
            else
            {
                isInstalled = true;
                // action = "Installed";
                // isActionEnabled = false;
                // A newer version is available
                var comparison = network.CurrentToLatestVersionComparisonResult;
                if (comparison == VersionComparisonResult.Lesser)
                {
                    action = "Upgrade";
                    isActionEnabled = true;
                }
                // Current installed version is newer than latest version from DB (beta version)
                else if (comparison == VersionComparisonResult.Greater)
                {
                    action = "Installed";
                    isActionEnabled = false;
                }
                // Already on the latest version
                else
                {
                    action = "Installed";
                    isActionEnabled = false;
                }
            }

            GUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(false)))
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField(new GUIContent(network.DisplayName), networkWidthOption);
                EditorGUILayout.LabelField(new GUIContent(currentVersion), versionWidthOption);
                GUILayout.Space(3);
                EditorGUILayout.LabelField(new GUIContent(latestVersion), versionWidthOption);
                GUILayout.Space(3);
                GUILayout.FlexibleSpace();

                if (network.isReqiureUpdate())
                {
                    GUILayout.Label(new GUIContent { image = alertIcon, tooltip = "Adapter not compatible, please update to the latest version." }, iconStyle);
                }
                // else if ((network.DisplayName.Equals("Admob") || network.DisplayName.Equals("GOOGLE_AD_MANAGER_NETWORK")) && shouldShowGoogleWarning)
                // {
                //     GUILayout.Label(new GUIContent { image = warningIcon, tooltip = "You may see unexpected errors if you use different versions of the AdMob and Google Ad Manager adapter SDKs." }, iconStyle);
                // }

                GUI.enabled = networkButtonsEnabled && isActionEnabled;
                if (GUILayout.Button(new GUIContent(action), fieldWidth))
                {
                    // Download the plugin.jkfjkfdani 
                    // AppLovinEditorCoroutine.StartCoroutine(AppLovinIntegrationManager.Instance.DownloadPlugin(network));
                    // ATIntegrationManager.Instance.downloadPlugin(network);
                    ATIntegrationManager.Instance.networkInstallOrUpdate(pluginData, network);
                }
                GUI.enabled = true;
                GUILayout.Space(2);

                GUI.enabled = networkButtonsEnabled && isInstalled;
                if (GUILayout.Button(new GUIContent { image = uninstallIcon, tooltip = "Uninstall" }, iconStyle))
                {
                    EditorUtility.DisplayProgressBar("Integration Manager", "Deleting " + network.Name + "...", 0.5f);
                    ATIntegrationManager.Instance.uninstallNetwork(network, pluginData.country);
                    //Refresh UI
                    AssetDatabase.Refresh();
                    EditorUtility.ClearProgressBar();
                }

                GUI.enabled = true;
                GUILayout.Space(5);
            }

            if (isInstalled)
            {
                // Custom integration for AdMob where the user can enter the Android and iOS App IDs.
                if (network.Name.Equals("Admob") && network.CurrentVersions != null)
                {
                    // Custom integration requires Google AdMob adapter version newer than android_19.0.1.0_ios_7.57.0.0.
                    if (ATDataUtil.CompareUnityMediationVersions(network.CurrentVersions.Unity, "android_19.0.1.0_ios_7.57.0.0") == VersionComparisonResult.Greater)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        using (new EditorGUILayout.VerticalScope("box"))
                        {
                            string requiredVersion = "android_19.2.0.0_ios_7.61.0.0";
                            string warningMessage = "The current version of AppLovin MAX plugin requires Google adapter version newer than " + requiredVersion + " to enable auto-export of AdMob App ID.";
                            // if (isPluginMoved)
                            // {
                            //     requiredVersion = "android_19.6.0.1_ios_7.69.0.0";
                            //     warningMessage = "Looks like the MAX plugin has been moved to a different directory. This requires Google adapter version newer than " + requiredVersion + " for auto-export of AdMob App ID to work correctly.";
                            // }
                            // else
                            // {
                            //     requiredVersion = "android_19.2.0.0_ios_7.61.0.0";
                            //     warningMessage = "The current version of AppLovin MAX plugin requires Google adapter version newer than " + requiredVersion + " to enable auto-export of AdMob App ID.";
                            // }

                            GUILayout.Space(2);
                            if (ATDataUtil.CompareUnityMediationVersions(network.CurrentVersions.Unity, requiredVersion) == VersionComparisonResult.Greater)
                            {
                                ATPluginSetting.Instance.AdMobAndroidAppId = DrawTextField("App ID (Android)", ATPluginSetting.Instance.AdMobAndroidAppId, networkWidthOption);
                                ATPluginSetting.Instance.AdMobIosAppId = DrawTextField("App ID (iOS)", ATPluginSetting.Instance.AdMobIosAppId, networkWidthOption);
                            }
                            else
                            {
                                EditorGUILayout.HelpBox(warningMessage, MessageType.Warning);
                            }
                        }

                        GUILayout.EndHorizontal();
                    }
                }
            }
        }

        private string DrawTextField(string fieldTitle, string text, GUILayoutOption labelWidth, GUILayoutOption textFieldWidthOption = null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            EditorGUILayout.LabelField(new GUIContent(fieldTitle), labelWidth);
            GUILayout.Space(4);
            text = (textFieldWidthOption == null) ? GUILayout.TextField(text) : GUILayout.TextField(text, textFieldWidthOption);
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            return text;
        }

        private bool DrawOtherSettingsToggle(bool value, string text)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(4);
                var toggleValue = GUILayout.Toggle(value, text);
                GUILayout.Space(4);
                return toggleValue;
            }
        }

        private void startUpgradeAllNetwork()
        {
            ATEditorCoroutine.startCoroutine(UpgradeAllNetworks());
        }

        private IEnumerator UpgradeAllNetworks()
        {
            networkButtonsEnabled = false;
            EditorApplication.LockReloadAssemblies();
            var networks = pluginData.mediatedNetworks;
            foreach (var network in networks)
            {
                // if (ATConfig.isNetworkInstalled(network.Name, ATConfig.getLocalCountry()))
                // {
                //     ATLog.log("UpgradeAllNetworks() >>> name: " + network.Name + " isReqiureUpdate: " + network.isReqiureUpdate() +
                //     " curVersion: " + network.CurrentVersions.Unity + " latestVersion: " + network.LatestVersions.Unity);
                //     network.CurrentToLatestVersionComparisonResult = ATDataUtil.getVersionComparisonResult(network.CurrentVersions, network.LatestVersions, true);
                //     if (network.isReqiureUpdate())
                //     {
                //         yield return ATIntegrationManager.Instance.downloadPluginWithEnumerator(network, false);
                //     }
                // }
                    if (network.CurrentVersions != null && !string.IsNullOrEmpty(network.CurrentVersions.Unity))
                    {
                        ATLog.log("UpgradeAllNetworks() >>> name: " + network.Name + " isReqiureUpdate: " + network.isReqiureUpdate() +
                        " curVersion: " + network.CurrentVersions.Unity + " latestVersion: " + network.LatestVersions.Unity);
                        network.CurrentToLatestVersionComparisonResult = ATDataUtil.getVersionComparisonResult(network.CurrentVersions, network.LatestVersions, true);
                        if (network.isReqiureUpdate())
                        {
                            yield return ATIntegrationManager.Instance.downloadPluginWithEnumerator(network, false);
                        }
                    }
            }

            EditorApplication.UnlockReloadAssemblies();
            networkButtonsEnabled = true;

            //The pluginData becomes stale after the networks have been updated, and we should re-load it.
            loadPluginData();
        }
    }
}