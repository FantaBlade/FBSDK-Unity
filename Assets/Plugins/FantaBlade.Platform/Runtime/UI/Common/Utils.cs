using UnityEngine;

namespace FantaBlade.Platform.UI.Common
{
    public class Utils
    {
        private static float _preClickTime = 0;
        private const float ClickInterval = 0.2f;
        
        /// <summary>
        /// 阻止连续点击
        /// </summary>
        /// <param name="customInterval">自定义点击间隔</param>
        /// <returns></returns>
        public static bool IsClickTooOften(float customInterval = ClickInterval)
        {
            if (_preClickTime + customInterval > Time.time)
            {
                return true;
            }

            _preClickTime = Time.time;
            return false;
        }
    }
}