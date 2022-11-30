using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Text;
using System.Security.Cryptography;


namespace NetEase
{
    public interface InfoReceiver
    {
        void onReceive(int Type, string info);
    }

    public class NetHeartBeat
    {
        public const int INFO_TYPE_HEARTBEAT = 1;  /// 心跳
        public const int INFO_TYPE_ENC_HEARTBEAT = 2;  /// 心跳
        public const int INFO_TYPE_CHEATINFO = 3;

        private NetHeartBeat()
        {
        }
        private static volatile NetHeartBeat mInstance = null;
        private static readonly object mSingletonLock = new object();
        public static NetHeartBeat getInstance()
        {
            if (mInstance == null)
            {
                lock (mSingletonLock)
                {
                    if (mInstance == null)
                    {
                        mInstance = new NetHeartBeat();
                    }
                }

            }
            return mInstance;
        }

        private readonly object padlockReceiver = new object();
        private static List<InfoReceiver> mReceivers = new List<InfoReceiver>();
        private static Thread mInfoPublisherThread = null;
        private static volatile bool thread_running = false;
        public void registInfoReceiver(InfoReceiver receiver)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (receiver == null) return;
            if (!thread_running)
            {
                lock (padlockReceiver)
                {
                    if (!thread_running)
                    {
                        thread_running = true;
                        mInfoPublisherThread = new Thread(getInstance().recvDataThread);
                        mInfoPublisherThread.Start();
                    }
                }
            }
            lock (padlockReceiver)
            {
                mReceivers.Add(receiver);
            }
#endif
        }

        private void callResult(int id, string info)
        {
            lock (padlockReceiver)
            {
                foreach (InfoReceiver r in mReceivers)
                {
                    r.onReceive(id, info);
                }
            }
        }

        private void recvDataThread()
        {
            string ret = openPipe();
            if (!ret.Equals("true"))
            {
                thread_running = false;
                return;
            }

            try
            {
                while (true)
                {
                    try
                    {
                        Thread.Sleep(1000);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("recvDataThread Sleep" + e.ToString());
                    }
                    string info = recvPipe();
                    if (info == null) break;
                    int pos = info.IndexOf('&');
                    if (pos == -1) break;
                    int id = Int32.Parse(info.Substring(0, pos));
                    string msg = info.Substring(pos + 1);
                    callResult(id, msg);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("recvDataThread" + e.ToString());
            }
            finally
            {
                closePipe();
                thread_running = false;
            }
        }

        private static string openPipe()
        {
            string info = NetSecProtect.exportHeartBeat(HeartBeatCmdID.Hb_Cmd_OpenPipe);
            return info;
        }

        private static string recvPipe()
        {
            string info = NetSecProtect.exportHeartBeat(HeartBeatCmdID.Hb_Cmd_RecvPipe);
            return info;
        }

        private static string closePipe()
        {
            string info = NetSecProtect.exportHeartBeat(HeartBeatCmdID.Hb_Cmd_ClosePipe);
            return info;
        }
    }

    public class SafeCommResult
    {
        public static int SC_CODE_OK = 0;
        public static int SC_FILE_NOTEXIST = -1;   // apk包文件无文件
        public static int SC_FILE_IS_BROKEN = -2;   // apk包文件损坏
        public static int SC_PROTOCOL_VERSION_ERROR = -3; // 协议版本不匹配
        public static int SC_PARAM_ERROR = -4; // 参数错误
        public static int SC_DATA_TAMPERED = -5; // 数据被篡改
        public static int SC_DATA_DECRYPT_ERROR = -6; // 数据解密失败
        public static int SC_ALG_ERROR = -7; // 算法不匹配,指定算法解密
        public static int SC_TIMEOUT = -8; // 超时

        public static int SC_BUFF_MALLOC_ERROR = -98; // 这种情况一般不会出现
        public static int SC_INSIDE_ERROR = -99; //内部错误，一般不会出现

        public int ret;
        public byte[] encBytes;
        public String encResult;
        public byte[] decResult;

        public SafeCommResult()
        {
            ret = SC_INSIDE_ERROR;
            decResult = null;
            encBytes = null;
        }
    }

}

public static class NetSecProtect
{
    private static bool isInitialized = false;
#if UNITY_ANDROID
    private const string strUUU_0 = "fZ434bpcoVuEg4egt6smXLGyYqq7mK9HKCv6+/9Rbe69IhXUaqaTI66/VCW1kN92L7md4h5J5u0x5z+l9S5+vQ==";
    private const string strCCC_0 = "zg92y1ai5CJ0PPUSp4qiiClkDK6wFLRU4XvvuQTfxSI=";
    private const string strHHH_0 = "PSGLjT3zlSDO9iaG/j7Pr+yQcvUwxs1FdfbuAeJPFTNTAEHw2YbQmPLR0siO1i87TXeIq8bFszG+MyiSnyUhTw==";
    private const string strIII_0 = "pObByb497KH0lJXQHeI7Cw==";
    private static string strUUU = "";
    private static string strCCC = "";
    private static string strHHH = "";
    private static string strIII = "";
#endif

    public static string exportHeartBeat(HeartBeatCmdID request)
    {

        var ptr = IntPtr.Zero;
        var size = OoO0oO0000Oo0OoO((int)request, ref ptr);
        if (size <= 0)
        {
            return "";
        }
        byte[] bytes = new byte[size];
        Marshal.Copy(ptr, bytes, 0, size);
        //释放native的结果
        O0O00ooo0o000OOo(ptr);
        string response_buf = System.Text.Encoding.ASCII.GetString(bytes);
        return response_buf;

    }

    public static string exportIoctl(RequestCmdID request, string data)
    {

        var ptr = IntPtr.Zero;
        var size = OoO0oo0o00Oo0OoO((int)request, data, ref ptr);
        if (size <= 0)
        {
            return "";
        }
        byte[] bytes = new byte[size];
        Marshal.Copy(ptr, bytes, 0, size);
        //释放native的结果
        O0O00ooo0o000OOo(ptr);
        string response_buf = System.Text.Encoding.ASCII.GetString(bytes);
        return response_buf;

    }

    // heartBeatData
    [DllImport("NetHTProtect")]
    private static extern int OoO0oO0000Oo0OoO(int request, ref IntPtr ptr);

    // iotcl
    [DllImport("NetHTProtect")]
    private static extern int OoO0oo0o00Oo0OoO(int request, string data, ref IntPtr ptr);

    [DllImport("NetHTProtect")]
    private static extern void O0O00ooo0o000OOo(IntPtr ptr);

    //logout
    [DllImport("NetHTProtect")]
    private static extern void oOOOoooo0000o00O();

    private static string initDecrypt(string cipherText)
    {
        string encryptionKey = "906f4ef72ca2048251";
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0xa8, 0x79, 0x61, 0x6e, 0x20, 0xa5, 0x38, 0x64, 0x76, 0x56, 0x54, 0x65, 0x40 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                cipherText = Encoding.Unicode.GetString(ms.ToArray());
            }
        }
        return cipherText;
    }

    public static void init(string appId, string gameKey)
    {
        if (isInitialized)
        {
            return;
        }
#if UNITY_ANDROID
        strUUU = initDecrypt(strUUU_0);
        strCCC = initDecrypt(strCCC_0);
        strHHH = initDecrypt(strHHH_0);
        strIII = initDecrypt(strIII_0);
        initSDK(appId, gameKey, 1);
#endif
        isInitialized = true;
    }

    public static void init(string appId, string gameKey, int serverType)
    {
        if (isInitialized)
        {
            return;
        }
#if UNITY_ANDROID
        strUUU = initDecrypt(strUUU_0);
        strCCC = initDecrypt(strCCC_0);
        strHHH = initDecrypt(strHHH_0);
        strIII = initDecrypt(strIII_0);
        initSDK(appId, gameKey, serverType);
#endif
        isInitialized = true;
    }

    private static void initSDK(string appId, string gameKey, int serverType)
    {
#if UNITY_ANDROID
        try
        {
            using (var actClass = new AndroidJavaClass(strUUU))
            {
                AndroidJavaObject curAC = actClass.GetStatic<AndroidJavaObject>(strCCC);
                if (curAC != null)
                {
                    AndroidJavaClass HTPClass = new AndroidJavaClass(strHHH);
                    if (HTPClass != null)
                    {
                        HTPClass.CallStatic(strIII, curAC, appId, gameKey, serverType);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("InitSDK" + e.ToString());
        }
#endif
    }

    public static string getDataSign(string inputData, int algIndex)
    {
        string sign = "";
#if UNITY_ANDROID
        try
        {

            using (var actClass = new AndroidJavaClass(strUUU))
            {
                AndroidJavaClass HTPClass = new AndroidJavaClass(strHHH);
                if (HTPClass != null)
                {
                    string encedKey = "5gS14O4oV73TDX/RFvTQYiFrchts4GoOyErXXMv8bvyERoXTX2Kg4BFcuoG2yPeqbayIiPw71O0bZbZnWAst+uSnD977HY2g1+Y3orjWhK8=";

                    string encryptionKey = "906f4ef72ca2048251";
                    byte[] cipherBytes = Convert.FromBase64String(encedKey);
                    using (Aes encryptor = Aes.Create())
                    {
                        Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0xa8, 0x79, 0x61, 0x6e, 0x20, 0xa5, 0x38, 0x64, 0x76, 0x56, 0x54, 0x65, 0x40 });
                        encryptor.Key = pdb.GetBytes(32);
                        encryptor.IV = pdb.GetBytes(16);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(cipherBytes, 0, cipherBytes.Length);
                                cs.Close();
                            }
                            string decedKey = Encoding.Unicode.GetString(ms.ToArray());
                            string nativeSign = HTPClass.CallStatic<string>("getDataSign", inputData, algIndex);
                            nativeSign = nativeSign + decedKey;
                            MD5 md5 = MD5.Create();
                            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(nativeSign));
                            for (int i = 0; i < s.Length; i++)
                            {
                                sign = sign + s[i].ToString("x2");
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("getDataSign" + e.ToString());
        }
#endif
        return sign;
    }

    public static void setRoleInfo(string roleId, string roleName, string roleAccount, string roleServer, int serverId, string gameJson)
    {
#if UNITY_ANDROID
        try
        {
            using (var actClass = new AndroidJavaClass(strUUU))
            {
                AndroidJavaClass HTPClass = new AndroidJavaClass(strHHH);
                if (HTPClass != null)
                {
                    HTPClass.CallStatic("setRoleInfo", roleId, roleName, roleAccount, roleServer, serverId, gameJson);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("setRoleInfo:\n" + e.ToString());
        }
#endif
    }

    public static void logOut()
    {
#if UNITY_ANDROID
        try
        {
            oOOOoooo0000o00O();
        }
        catch (System.Exception e)
        {
            Debug.LogError("logOut" + e.ToString());
        }
#endif
    }

    public static void setTransInfo(string transHost, string transIp, int transPort)
    {
#if UNITY_ANDROID
        try
        {
            using (var actClass = new AndroidJavaClass(strUUU))
            {
                AndroidJavaClass HTPClass = new AndroidJavaClass(strHHH);
                if (HTPClass != null)
                {
                    HTPClass.CallStatic("setTransInfo", transHost, transIp, transPort);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("setTransInfo:\n" + e.ToString());
        }
#endif
    }

    public static string localSaveEncode(string inputData, int algIndex)
    {
        string ret = "";
#if UNITY_ANDROID
        try
        {
            using (var actClass = new AndroidJavaClass(strUUU))
            {
                AndroidJavaClass HTPClass = new AndroidJavaClass(strHHH);
                if (HTPClass != null)
                {
                    ret = HTPClass.CallStatic<string>("localSaveEncode", inputData, algIndex);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("EncodeLocal" + e.ToString());
        }
#endif
        return ret;
    }

    public static string localSaveDecode(string inputData, int algIndex)
    {
        string ret = "";
#if UNITY_ANDROID
        try
        {
            using (var actClass = new AndroidJavaClass(strUUU))
            {
                AndroidJavaClass HTPClass = new AndroidJavaClass(strHHH);
                if (HTPClass != null)
                {
                    ret = HTPClass.CallStatic<string>("localSaveDecode", inputData, algIndex);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("DecodeLocal" + e.ToString());
        }
#endif
        return ret;
    }

    public static string localSaveBytesEncode(byte[] inputData, int algIndex)
    {
        string ret = "";
#if UNITY_ANDROID
        string tmp = Convert.ToBase64String(inputData);
        ret = localSaveEncode(tmp, algIndex);
#endif
        return ret;
    }

    public static byte[] localSaveBytesDecode(string inputData, int algIndex)
    {
        byte[] ret = { 0 };
#if UNITY_ANDROID
        string tmp = localSaveDecode(inputData, algIndex);
        ret = Convert.FromBase64String(tmp);
#endif
        return ret;
    }

    public static string ioctl(RequestCmdID request, string data)
    {
        string ret = "";
#if UNITY_ANDROID
        ret = NetSecProtect.exportIoctl(request, data);
#endif
        return ret;
    }

    public static string getToken()
    {
        string ret = "";
#if UNITY_ANDROID
        try
        {
            using (var actClass = new AndroidJavaClass(strUUU))
            {
                AndroidJavaClass HTPClass = new AndroidJavaClass(strHHH);
                if (HTPClass != null)
                {
                    ret = HTPClass.CallStatic<string>("ioctl", 11, "");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("htpIoctl:\n" + e.ToString());
        }
#endif
        return ret;
    }

    public static void registInfoReceiver(NetEase.InfoReceiver receiver)
    {
        NetEase.NetHeartBeat.getInstance().registInfoReceiver(receiver);
    }


    public static void registerTouchEvent(int gameplayId, int sceneId)
    {
#if UNITY_ANDROID
        try
        {
            using (var actClass = new AndroidJavaClass(strUUU))
            {
                AndroidJavaClass HTPClass = new AndroidJavaClass(strHHH);
                if (HTPClass != null)
                {
                    HTPClass.CallStatic("registerTouchEvent", gameplayId, sceneId);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("registerTouchEvent" + e.ToString());
        }
#endif
    }

    public static void unregisterTouchEvent()
    {
#if UNITY_ANDROID
        try
        {
            using (var actClass = new AndroidJavaClass(strUUU))
            {
                AndroidJavaClass HTPClass = new AndroidJavaClass(strHHH);
                if (HTPClass != null)
                {
                    HTPClass.CallStatic("unregisterTouchEvent");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("unregisterTouchEvent" + e.ToString());
        }
#endif
    }

    public static NetEase.SafeCommResult safeCommToServerByte(int alg, byte[] inputData)
    {
        NetEase.SafeCommResult safeCommResult = new NetEase.SafeCommResult();
        if (inputData == null || inputData.Length == 0)
        {
            safeCommResult.ret = NetEase.SafeCommResult.SC_PARAM_ERROR;
            return safeCommResult;
        }
        string inputString = Convert.ToBase64String(inputData);
#if UNITY_ANDROID
        try
        {
            using (var actClass = new AndroidJavaClass(strUUU))
            {
                AndroidJavaClass HTPClass = new AndroidJavaClass(strHHH);
                if (HTPClass != null)
                {
                    AndroidJavaObject scClass = HTPClass.CallStatic<AndroidJavaObject>("safeCommToServerCSharp", alg, inputString);
                    int ret = scClass.Get<int>("ret");
                    safeCommResult.ret = ret;
                    if (ret == NetEase.SafeCommResult.SC_CODE_OK)
                    {
                        string encResult = scClass.Get<string>("encResult");
                        safeCommResult.encBytes = Convert.FromBase64String(encResult);
                        safeCommResult.ret = ret;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("safeCommToServer" + e.ToString());
        }
#endif
        return safeCommResult;
    }

    public static NetEase.SafeCommResult safeCommToServer(int alg, byte[] inputData)
    {
        NetEase.SafeCommResult safeCommResult = new NetEase.SafeCommResult();
        if (inputData == null || inputData.Length == 0)
        {
            safeCommResult.ret = NetEase.SafeCommResult.SC_PARAM_ERROR;
            return safeCommResult;
        }
        string inputString = Convert.ToBase64String(inputData);
#if UNITY_ANDROID
        try
        {
            using (var actClass = new AndroidJavaClass(strUUU))
            {
                AndroidJavaClass HTPClass = new AndroidJavaClass(strHHH);
                if (HTPClass != null)
                {
                    AndroidJavaObject scClass = HTPClass.CallStatic<AndroidJavaObject>("safeCommToServerCSharp", alg, inputString);
                    int ret = scClass.Get<int>("ret");
                    safeCommResult.ret = ret;
                    if (ret == NetEase.SafeCommResult.SC_CODE_OK)
                    {
                        safeCommResult.encResult = scClass.Get<string>("encResult");
                        safeCommResult.ret = ret;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("safeCommToServer" + e.ToString());
        }
#endif
        return safeCommResult;
    }

    public static NetEase.SafeCommResult safeCommFromServerByte(int alg, int timeout, byte[] inputData)
    {
        NetEase.SafeCommResult safeCommResult = new NetEase.SafeCommResult();
        if (inputData == null || inputData.Length == 0)
        {
            safeCommResult.ret = NetEase.SafeCommResult.SC_PARAM_ERROR;
            return safeCommResult;
        }
#if UNITY_ANDROID
        try
        {
            using (var actClass = new AndroidJavaClass(strUUU))
            {
                AndroidJavaClass HTPClass = new AndroidJavaClass(strHHH);
                if (HTPClass != null)
                {
                    string input = Convert.ToBase64String(inputData);
                    AndroidJavaObject scClass = HTPClass.CallStatic<AndroidJavaObject>("safeCommFromServerCSharp", alg, timeout, input);
                    int ret = scClass.Get<int>("ret");
                    safeCommResult.ret = ret;
                    if (ret == NetEase.SafeCommResult.SC_CODE_OK)
                    {
                        string encResult = scClass.Get<string>("encResult");
                        safeCommResult.decResult = Convert.FromBase64String(encResult);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("safeCommToServer" + e.ToString());
        }
#endif
        return safeCommResult;
    }

    public static NetEase.SafeCommResult safeCommFromServer(int alg, int timeout, string inputData)
    {
        NetEase.SafeCommResult safeCommResult = new NetEase.SafeCommResult();
        if (inputData == null || inputData.Length == 0)
        {
            safeCommResult.ret = NetEase.SafeCommResult.SC_PARAM_ERROR;
            return safeCommResult;
        }
#if UNITY_ANDROID
        try
        {
            using (var actClass = new AndroidJavaClass(strUUU))
            {
                AndroidJavaClass HTPClass = new AndroidJavaClass(strHHH);
                if (HTPClass != null)
                {
                    AndroidJavaObject scClass = HTPClass.CallStatic<AndroidJavaObject>("safeCommFromServerCSharp", alg, timeout, inputData);
                    int ret = scClass.Get<int>("ret");
                    safeCommResult.ret = ret;
                    if (ret == NetEase.SafeCommResult.SC_CODE_OK)
                    {
                        string encResult = scClass.Get<string>("encResult");
                        safeCommResult.decResult = Convert.FromBase64String(encResult);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("safeCommToServer" + e.ToString());
        }
#endif
        return safeCommResult;
    }

    public static string safeComm(string inputData, int algType, bool dec)
    {
        string ret = "";
#if UNITY_ANDROID
        try
        {
            using (var actClass = new AndroidJavaClass(strUUU))
            {
                AndroidJavaClass HTPClass = new AndroidJavaClass(strHHH);
                if (HTPClass != null)
                {
                    ret = HTPClass.CallStatic<string>("safeComm", inputData, algType, dec);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("htpIoctl" + e.ToString());
        }
#endif
        return ret;
    }
}

public enum HeartBeatCmdID
{
    Hb_Cmd_OpenPipe = 1,
    Hb_Cmd_RecvPipe,
    Hb_Cmd_ClosePipe
};

public enum RequestCmdID
{
    Cmd_GetEmulatorName = 1,
    Cmd_IsRootDevice,
    Cmd_DeviceID,
    Cmd_OpenPipe,
    Cmd_RecvPipe,
    Cmd_ClosePipe,
    Cmd_GetHTPVersion,
    Cmd_GetCollectData,
    Cmd_GetEncHTPVersion,
    Cmd_GetToken
};