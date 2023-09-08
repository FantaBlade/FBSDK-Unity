using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace TapTap.TapDB.PC.Utils
{
    public static class TapDBDataStorage
    {
        private static Dictionary<string, string> _dataCache;
        private static readonly byte[] Keys = {0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF};

        // private static readonly string SavedDirectory = Application.persistentDataPath;
        
        public static void SaveString(string key, string value)
        {
          
            SaveStringToCache(key, value);
            TapDBPlayerPrefs.Instance.SetString(key, EncodeString(value));
        }

        public static string LoadString(string key)
        {
            var value = LoadStringFromCache(key);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            value = DecodeString(TapDBPlayerPrefs.Instance.GetString(key));
            if (value != null)
            {
                SaveStringToCache(key, value);
            }

            return value;
        }

        private static void SaveStringToCache(string key, string value)
        {
            if (_dataCache == null)
            {
                _dataCache = new Dictionary<string, string>();
            }

            if (_dataCache.ContainsKey(key))
            {
                _dataCache[key] = value;
            }
            else
            {
                _dataCache.Add(key, value);
            }
        }

        private static string LoadStringFromCache(string key)
        {
            if (_dataCache == null)
            {
                _dataCache = new Dictionary<string, string>();
            }

            return _dataCache.ContainsKey(key) ? _dataCache[key] : null;
        }

        private static string EncodeString(string encryptString)
        {
            if (encryptString == null) return null;
            try
            {
                var rgbKey = Encoding.UTF8.GetBytes(GetMacAddress().Substring(0, 8));
                var rgbIv = Keys;
                var inputByteArray = Encoding.UTF8.GetBytes(encryptString);
                var dCsp = new DESCryptoServiceProvider();
                var mStream = new MemoryStream();
                var cStream =
                    new CryptoStream(mStream, dCsp.CreateEncryptor(rgbKey, rgbIv), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                cStream.Close();
                return Convert.ToBase64String(mStream.ToArray());
            }
            catch (Exception e)
            {
                TapDBLogger.Debug("EncodeString Failed: " + e.Message);
            }
            return encryptString;
        }

        private static string DecodeString(string decryptString)
        {
            if (decryptString == null) return null;
            try
            {
                var rgbKey = Encoding.UTF8.GetBytes(GetMacAddress().Substring(0, 8));
                var rgbIv = Keys;
                var inputByteArray = Convert.FromBase64String(decryptString);
                var cryptoServiceProvider = new DESCryptoServiceProvider();
                var mStream = new MemoryStream();
                var cStream =
                    new CryptoStream(mStream, cryptoServiceProvider.CreateDecryptor(rgbKey, rgbIv), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                cStream.Close();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch (Exception e)
            {
                TapDBLogger.Debug("EncodeString Failed: " + e.Message);
            }
            return decryptString;
        }


        private static string GetMacAddress()
        {
            var physicalAddress = "FFFFFFFFFFFF";
            var allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var networkInterface in allNetworkInterfaces)
            {
                physicalAddress = networkInterface.GetPhysicalAddress().ToString();
                if (!string.IsNullOrEmpty(physicalAddress))
                {
                    break;
                }
            }

            return physicalAddress;
        }
    }
}
