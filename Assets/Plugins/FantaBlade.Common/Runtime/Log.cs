using System;


namespace FantaBlade.Common
{
    /// <summary>
    ///     受控 Log 系统
    /// </summary>
    public static class Log
    {
        #region LOG控制

        public enum LogLevel
        {
            Emerg = 7, //system is unusable
            Alert = 6, //action must be taken immediately
            Crit = 5, //critical conditions
            Error = 4, //error conditions
            Warning = 3, //warning conditions
            Notice = 2, //normal, but significant, condition
            Info = 1, //informational message
            Debug = 0 //debug-level message
        }

        /// <summary>
        ///     日志等级
        /// </summary>
        public static LogLevel CurrentLevel { get; set; }

        private static string Now
        {
            get
            {
                var now = DateTime.Now;
                return string.Format("[{0:u} {1:D3}]", now, now.Millisecond);
            }
        }

        /// <summary>
        ///     Debug 时临时输出的Log
        /// </summary>
        /// <param name="obj"></param>
        public static void Debug(object obj)
        {
            if (CurrentLevel <= LogLevel.Debug)
            {
                UnityEngine.Debug.LogFormat("{0} [Debug] {1}", Now, obj);
            }
        }

        /// <summary>
        ///     系统运行过程中的常规日志
        /// </summary>
        /// <param name="obj"></param>
        public static void Info(object obj)
        {
            if (CurrentLevel <= LogLevel.Info)
            {
                UnityEngine.Debug.LogFormat("{0} [Info] {1}", Now, obj);
            }
        }

        /// <summary>
        ///     系统运行过程中的常规日志，但是应该引起注意
        /// </summary>
        /// <param name="obj"></param>
        public static void Notice(object obj)
        {
            if (CurrentLevel <= LogLevel.Notice)
            {
                UnityEngine.Debug.LogFormat("{0} [Notice] {1}", Now, obj);
            }
        }

        /// <summary>
        ///     警告信息
        /// </summary>
        /// <param name="obj"></param>
        public static void Warning(object obj)
        {
            if (CurrentLevel <= LogLevel.Warning)
            {
                UnityEngine.Debug.LogWarningFormat("{0} [Warning] {1}", Now, obj);
            }
        }

        /// <summary>
        ///     系统错误情况
        /// </summary>
        /// <param name="obj"></param>
        public static void Error(object obj)
        {
            if (CurrentLevel <= LogLevel.Error)
            {
                UnityEngine.Debug.LogErrorFormat("{0} [Error] {1}", Now, obj);
            }
        }

        /// <summary>
        ///     系统警报，在编辑器模式下运行时会弹出警报对话框
        /// </summary>
        /// <param name="obj"></param>
        public static void Alert(object obj)
        {
            if (CurrentLevel <= LogLevel.Alert)
            {
                UnityEngine.Debug.LogErrorFormat("{0} [Alert] {1}", Now, obj);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog("ERROR!!!", obj.ToString(), "OK");
#endif
            }
        }

        #endregion
    }
}