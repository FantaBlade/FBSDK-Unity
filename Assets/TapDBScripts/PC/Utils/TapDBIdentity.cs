using System;
using UnityEngine;

namespace TapTap.TapDB.PC.Utils
{
    public class TapDBIdentity
    {
        
        private static readonly object Locker = new object();
        
        public static string GetDeviceID()
        {
            lock (Locker)
            {
                var uuid = TapDBDataStorage.LoadString("tapdb_unique_id");
                if (!string.IsNullOrEmpty(uuid))
                {
                    return uuid;
                }

                uuid = SystemInfo.deviceUniqueIdentifier;
                TapDBDataStorage.SaveString("tapdb_unique_id", uuid);
                return uuid;
            }
        }
        
        public static string GetPersistID()
        {
            lock (Locker)
            {
                var uuid = TapDBDataStorage.LoadString("tapdb_persist_id");
                if (!string.IsNullOrEmpty(uuid))
                {
                    return uuid;
                }

                uuid = Guid.NewGuid().ToString();
                TapDBDataStorage.SaveString("tapdb_persist_id", uuid);
                return uuid;
            }
        }
        
        public static string GetInstallID()
        {
            lock (Locker)
            {
                var uuid = TapDBDataStorage.LoadString("tapdb_install_id");
                if (!string.IsNullOrEmpty(uuid))
                {
                    return uuid;
                }

                uuid = Guid.NewGuid().ToString();
                TapDBDataStorage.SaveString("tapdb_install_id", uuid);
                return uuid;
            }
        }

        public static string GetTapOpenID()
        {
            return null;
        }
    }
}