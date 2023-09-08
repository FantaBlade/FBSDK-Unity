namespace TapTap.TapDB.PC.Utils
{
    public class TapDBLogger
    {
        public static bool EnableLog
        {
            get;
            set;
        } = true;


        public static void Debug(string log)
        {
            if (EnableLog)
            { 
                UnityEngine.Debug.Log("TapDB SDK Debug: " + log);
            }
            
        }

        public static void Error(string log)
        {
            UnityEngine.Debug.LogError("TapDB SDK Error: " + log);
        }
    }
}