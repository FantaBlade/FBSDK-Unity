using System;
using System.Collections.Generic;
using System.Threading;
using TapTap.TapDB.PC.Net;
using TapTap.TapDB.PC.Utils;
using UnityEngine;

namespace TapTap.TapDB.PC
{
    public class TapDBPC
    {
        public delegate Dictionary<string, object> DynamicPropertiesDelegate();

        private static readonly object Locker = new object();

        private static readonly Dictionary<string, object> StaticProperties = new Dictionary<string, object>();

        private static readonly Dictionary<string, object> BasicInfos = new Dictionary<string, object>();

        private static string _channel;

        private static volatile string _appId;

        private static volatile string _clientId;

        private static string _userId;

        private static string _loginType;

        private static string _gameVersion;

        private static volatile Thread _thread;

        private static DateTime _lastSavedPlayedDurationTime = DateTime.UtcNow;

        private static DynamicPropertiesDelegate _dynamicPropertiesDelegate;

        public static void InitWithAppId(string appId, string channel, string gameVersion)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                TapDBLogger.Error("Illegal App ID");
                return;
            }

            _appId = appId;
            InitInternal(channel, gameVersion);
        }

        public static void InitWithClientId(string clientId, string channel, string gameVersion)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                TapDBLogger.Error("Illegal Client ID");
                return;
            }

            _clientId = clientId;
            InitInternal(channel, gameVersion);
        }

        public static void SetUser(string userId, string loginType = "")
        {
            if (!CheckInitialized())
            {
                return;
            }

            UploadPlayedDuration();
            _userId = userId;
            _loginType = loginType;
            _userId = userId;
            TrackInternal("identify", "user_login");
            var properties = new Dictionary<string, object>
            {
                ["first_open_id"] = TapDBIdentity.GetTapOpenID(),
                ["first_login_type"] = _loginType
            };
            PropertiesInternal("initialise", true, false, properties);
            properties = new Dictionary<string, object>
            {
                ["has_user"] = true,
                ["current_open_id"] = TapDBIdentity.GetTapOpenID(),
                ["current_login_type"] = _loginType
            };
            PropertiesInternal("update", true, false, properties);
        }

        public static void SetName(string name)
        {
            if (!CheckInitialized())
            {
                return;
            }
            
            var properties = new Dictionary<string, object>
            {
                ["user_name"] = name
            };
            PropertiesInternal("update", false, true, properties);
        }

        public static void SetLevel(int level)
        {
            if (!CheckInitialized())
            {
                return;
            }

            var properties = new Dictionary<string, object>
            {
                ["level"] = level
            };
            PropertiesInternal("update", false, true, properties);
        }

        public static void SetServer(string server)
        {
            if (!CheckInitialized())
            {
                return;
            }

            var properties = new Dictionary<string, object>
            {
                ["first_server"] = server
            };
            PropertiesInternal("initialise", false, true, properties);
            properties = new Dictionary<string, object>
            {
                ["current_server"] = server
            };
            PropertiesInternal("update", false, true, properties);
        }

        public static void OnCharge(string orderId, string product, long amount, string currencyType, string payment)
        {
            if (!CheckInitialized())
            {
                return;
            }

            var properties = new Dictionary<string, object>
            {
                ["order_id"] = orderId,
                ["product"] = product,
                ["amount"] = amount,
                ["currency_type"] = currencyType,
                ["payment"] = payment
            };
            TrackInternal("event", "charge", properties);
        }

        public static void ClearUser()
        {
            if (!CheckInitialized())
            {
                return;
            }

            UploadPlayedDuration();
            _loginType = null;
            _userId = null;
        }

        public static void TrackEvent(string eventName, Dictionary<string, object> properties)
        {
            if (!CheckInitialized())
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(eventName))
            {
                TapDBLogger.Error("Illegal event name.");
            }

            TrackInternal("event", eventName, properties);
        }

        public static void RegisterStaticProperties(Dictionary<string, object> properties)
        {
            foreach (var keyValuePair in properties)
            {
                StaticProperties.Remove(keyValuePair.Key);
                StaticProperties.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public static void UnregisterStaticProperty(string propertyKey)
        {
            if (string.IsNullOrWhiteSpace(propertyKey))
            {
                TapDBLogger.Error("Illegal property key.");
                return;
            }
            
            StaticProperties.Remove(propertyKey);
        }

        public static void RegisterDynamicProperties(DynamicPropertiesDelegate dynamicPropertiesDelegate)
        {
            _dynamicPropertiesDelegate = dynamicPropertiesDelegate;
        }

        public static void ClearStaticProperties()
        {
            StaticProperties.Clear();
        }

        public static void DeviceInitialize(Dictionary<string, object> properties)
        {
            if (!CheckInitialized())
            {
                return;
            }
            PropertiesInternal("initialise", true , false, properties);
        }

        public static void DeviceUpdate(Dictionary<string, object> properties)
        {
            if (!CheckInitialized())
            {
                return;
            }
            PropertiesInternal("update", true , false, properties);
        }

        public static void DeviceAdd(Dictionary<string, object> properties)
        {
            if (!CheckInitialized())
            {
                return;
            }
            PropertiesInternal("add", true , false, properties);
        }

        public static void UserInitialize(Dictionary<string, object> properties)
        {
            if (!CheckInitialized())
            {
                return;
            }
            PropertiesInternal("initialise", false , true, properties);
        }

        public static void UserUpdate(Dictionary<string, object> properties)
        {
            if (!CheckInitialized())
            {
                return;
            }
            PropertiesInternal("update", false , true, properties);
        }

        public static void UserAdd(Dictionary<string, object> properties)
        {
            if (!CheckInitialized())
            {
                return;
            }
            PropertiesInternal("add", false , true, properties);
        }

        public static void TrackEvent(string eventName, string propertiesStr)
        {
            try
            {
                var properties =
                    (Dictionary<string, object>) TapDBJson.Deserialize(propertiesStr);
                TrackEvent(eventName, properties);
            }
            catch (Exception e)
            {
                TapDBLogger.Error("TrackEvent Failed:" + e.Message);
            }
        }

        public static void RegisterStaticProperties(string propertiesStr)
        {
            try
            {
                var properties =
                    (Dictionary<string, object>) TapDBJson.Deserialize(propertiesStr);
                RegisterStaticProperties(properties);
            }
            catch (Exception e)
            {
                TapDBLogger.Error("RegisterStaticProperties Failed:" + e.Message);
            }
        }

        public static void DeviceInitialize(string propertiesStr)
        {
            try
            {
                var properties =
                    (Dictionary<string, object>) TapDBJson.Deserialize(propertiesStr);
                DeviceInitialize(properties);
            }
            catch (Exception e)
            {
                TapDBLogger.Error("DeviceInitialize Failed:" + e.Message);
            }
        }

        public static void DeviceUpdate(string propertiesStr)
        {
            try
            {
                var properties =
                    (Dictionary<string, object>) TapDBJson.Deserialize(propertiesStr);
                DeviceUpdate(properties);
            }
            catch (Exception e)
            {
                TapDBLogger.Error("DeviceUpdate Failed:" + e.Message);
            }
        }

        public static void DeviceAdd(string propertiesStr)
        {
            try
            {
                var properties =
                    (Dictionary<string, object>) TapDBJson.Deserialize(propertiesStr);
                DeviceAdd(properties);
            }
            catch (Exception e)
            {
                TapDBLogger.Error("DeviceAdd Failed:" + e.Message);
            }
        }

        public static void UserInitialize(string propertiesStr)
        {
            try
            {
                var properties =
                    (Dictionary<string, object>) TapDBJson.Deserialize(propertiesStr);
                UserInitialize(properties);
            }
            catch (Exception e)
            {
                TapDBLogger.Error("UserInitialize Failed:" + e.Message);
            }
        }

        public static void UserUpdate(string propertiesStr)
        {
            try
            {
                var properties =
                    (Dictionary<string, object>) TapDBJson.Deserialize(propertiesStr);
                UserUpdate(properties);
            }
            catch (Exception e)
            {
                TapDBLogger.Error("UserUpdate Failed:" + e.Message);
            }
        }

        public static void UserAdd(string propertiesStr)
        {
            try
            {
                var properties =
                    (Dictionary<string, object>) TapDBJson.Deserialize(propertiesStr);
                UserAdd(properties);
            }
            catch (Exception e)
            {
                TapDBLogger.Error("UserAdd Failed:" + e.Message);
            }
        }

        public static void EnableLog(bool enableLog)
        {
            TapDBLogger.EnableLog = enableLog;
        }


        private static void InitInternal(string channel, string gameVersion)
        {
            lock (Locker)
            {
                _channel = channel;
                _gameVersion = gameVersion;
                InitBasicInfo();
                UploadPlayedDuration();
                if (_thread == null)
                {
                    _thread = new Thread(() =>
                    {
                        while (true)
                        {
                            Thread.Sleep( 2 * 1000);
                            SavePlayedDuration();
                        }
                    });
                    _thread.IsBackground = true;
                    _thread.Start();
                }

                var properties = new Dictionary<string, object>(BasicInfos);
                TrackInternal("identify", "device_login", properties);
            }
        }

        private static void InitBasicInfo()
        {
            switch (SystemInfo.operatingSystemFamily)
            {
                case OperatingSystemFamily.Windows:
                    BasicInfos["os"] = "Windows";
                    break;
                case OperatingSystemFamily.MacOSX:
                    BasicInfos["os"] = "Mac";
                    break;
                case OperatingSystemFamily.Linux:
                    BasicInfos["os"] = "Linux";
                    break;
                default:
                    BasicInfos["os"] = "Unknown";
                    break;
            }

            BasicInfos["device_model"] = SystemInfo.deviceModel;
            BasicInfos["os_version"] = SystemInfo.operatingSystem;
            BasicInfos["install_uuid"] = TapDBIdentity.GetInstallID();
            BasicInfos["persist_uuid"] = TapDBIdentity.GetPersistID();
            BasicInfos["width"] = Screen.currentResolution.width;
            BasicInfos["height"] = Screen.currentResolution.height;
            BasicInfos["provider"] = "unknown";
            BasicInfos["app_version"] = _gameVersion;
            BasicInfos["sdk_version"] = TapDBPCConstants.TapdbPCVersion;
            switch (Application.internetReachability)
            {
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    BasicInfos["network"] = "3";
                    break;
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    BasicInfos["network"] = "2";
                    break;
                case NetworkReachability.NotReachable:
                    BasicInfos["network"] = "Unknown";
                    break;
                default:
                    BasicInfos["network"] = "Unknown";
                    break;
            }

            BasicInfos["channel"] = _channel;
        }


        private static void TrackInternal(string uri,
            string name,
            Dictionary<string, object> properties = null,
            Dictionary<string, object> overrideField = null)
        {
            var theProperties = new Dictionary<string, object>(BasicInfos);

            foreach (var keyValuePair in StaticProperties)
            {
                theProperties.Remove(keyValuePair.Key);
                theProperties.Add(keyValuePair.Key, keyValuePair.Value);
            }

            var dynamicProperties = _dynamicPropertiesDelegate?.Invoke();
            if (dynamicProperties != null)
            {
                foreach (var keyValuePair in dynamicProperties)
                {
                    theProperties.Remove(keyValuePair.Key);
                    theProperties.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            if (properties != null)
            {
                foreach (var keyValuePair in properties)
                {
                    theProperties.Remove(keyValuePair.Key);
                    theProperties.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            
            var data = new Dictionary<string, object>()
            {
                ["type"] = "track",
                ["name"] = name,
                ["user_id"] = _userId,
                ["device_id"] = TapDBIdentity.GetDeviceID(),
                ["open_id"] = TapDBIdentity.GetTapOpenID(),
                ["properties"] = theProperties
            };
            if (!string.IsNullOrWhiteSpace(_appId))
            {
                data["index"] = _appId;
            }
            else if (!string.IsNullOrWhiteSpace(_clientId))
            {
                data["client_id"] = _clientId;
            }
            else
            {
                TapDBLogger.Error("TapDB SDK Error: Unknown AppID or ClientId");
                return;
            }

            if (overrideField != null)
            {
                foreach (var keyValuePair in overrideField)
                {
                    data.Remove(keyValuePair.Key);
                    data.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            TapDBNet.Instance.Send(TapDBPCConstants.BaseURL + uri, data);
        }

        private static void PropertiesInternal(
            string type,
            bool useDeviceId,
            bool useUserId,
            Dictionary<string, object> properties)
        {
            var data = new Dictionary<string, object>
            {
                ["type"] = type,
                ["properties"] = properties
            };

            if (!string.IsNullOrWhiteSpace(_appId))
            {
                data["index"] = _appId;
            }
            else if (!string.IsNullOrWhiteSpace(_clientId))
            {
                data["client_id"] = _clientId;
            }
            else
            {
                TapDBLogger.Error("Unknown AppID or ClientId");
                return;
            }

            if (useDeviceId)
            {
                data["device_id"] = TapDBIdentity.GetDeviceID();
            }

            if (useUserId)
            {
                data["user_id"] = _userId;
            }

            TapDBNet.Instance.Send(TapDBPCConstants.BaseURL + "event", data);
        }

        private static void SavePlayedDuration()
        {
            if (_userId == null)
            {
                return;
            }
            var durationStr = TapDBDataStorage.LoadString("tapdb_played_duration");
            long duration = 0;
            if (durationStr != null)
            {
                try
                {
                    duration = long.Parse(durationStr);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            var now = DateTime.UtcNow;
            duration += (long)(now - _lastSavedPlayedDurationTime).TotalSeconds;
            _lastSavedPlayedDurationTime = now;
            TapDBDataStorage.SaveString("tapdb_played_duration", duration.ToString());
            if (_userId != null)
            {
                TapDBDataStorage.SaveString("tapdb_played_duration_user_id", _userId.ToString());
            }
        }

        private static void UploadPlayedDuration()
        {
            var durationStr = TapDBDataStorage.LoadString("tapdb_played_duration");
            var userId = TapDBDataStorage.LoadString("tapdb_played_duration_user_id");
            long duration = 0;
            if (durationStr != null)
            {
                try
                {
                    duration = long.Parse(durationStr);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (duration <= 0 || userId == null)
            {
                return;
            }
            
            TapDBDataStorage.SaveString("tapdb_played_duration", "0");
            TapDBDataStorage.SaveString("tapdb_played_duration_user_id", null);
            var overrideField = new Dictionary<string, object>()
            {
                ["user_id"] = userId
            };

            var properties = new Dictionary<string, object>(BasicInfos)
            {
                ["duration"] = duration
            };
            TrackInternal("event", "play_game", properties, overrideField);
        }

        private static bool CheckInitialized()
        {
            if (!string.IsNullOrWhiteSpace(_clientId) || !string.IsNullOrWhiteSpace(_appId)) return true;
            TapDBLogger.Error("Must be initialized.");
            return false;
        }
    }
}
