using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TapTap.TapDB.PC.Utils
{
    public class TapDBPlayerPrefs
    {
        private static readonly string FileDirectory = Application.persistentDataPath;
        private const string FileName = "tapdb_storage";

        private static readonly object Locker = new object();
        private static ConcurrentDictionary<string, object> _dictionary;

        private static TapDBPlayerPrefs _instance;

        public static TapDBPlayerPrefs Instance => _instance ?? (_instance = new TapDBPlayerPrefs());

        private TapDBPlayerPrefs()
        {
            Load();
        }


        public string GetString(string key, string defaultValue = null)
        {
            return (string) _dictionary.GetOrAdd(key, defaultValue);
        }

        public void SetString(string key, string value)
        {
            _dictionary.AddOrUpdate(key, value, (_1, _2) => value);
            Save();
        }

        public void DeleteKey(string key)
        {
            _dictionary.TryRemove(key, out dynamic _);
            Save();
        }

        public void DeleteAll()
        {
            _dictionary.Clear();
            Save();
        }

        private static void Save()
        {
            lock (Locker)
            {
                var filePath = Path.Combine(FileDirectory, FileName);
                File.WriteAllText(filePath, TapDBJson.Serialize(_dictionary));
            }
        }

        private static void Load()
        {
            if (_dictionary != null)
            {
                return;
            }

            lock (Locker)
            {
                var filePath = Path.Combine(FileDirectory, FileName);
                if (!Directory.Exists(FileDirectory))
                {
                    Directory.CreateDirectory(FileDirectory);
                }

                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                }

                Dictionary<string, object> savedDictionary = null;
                var savedStr = File.ReadAllText(filePath);
                if (!string.IsNullOrWhiteSpace(savedStr))
                {
                    try
                    {
                        savedDictionary = (Dictionary<string, object>) TapDBJson.Deserialize(savedStr);
                    }
                    catch (Exception e)
                    {
                        TapDBLogger.Error("Load file error: " + e.Message);
                    }
                }

                _dictionary = savedDictionary != null
                    ? new ConcurrentDictionary<string, object>(savedDictionary)
                    : new ConcurrentDictionary<string, object>();
                File.WriteAllText(filePath, TapDBJson.Serialize(_dictionary));
            }
        }
    }
}