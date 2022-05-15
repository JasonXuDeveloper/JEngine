using System;

namespace JEngine.Core
{
    public static partial class Tools
    {
        /// <summary>
        /// 当前时间戳(ms)
        /// </summary>
        /// <returns></returns>
        public static long TimeStamp => (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;

        /// <summary>
        /// 获取下载速度
        /// </summary>
        /// <param name="downloadSpeed"></param>
        /// <returns></returns>
        public static string GetDisplaySpeed(float downloadSpeed)
        {
            if (downloadSpeed >= 1024 * 1024)
            {
                return $"{downloadSpeed * ConstMgr.Bytes2Mb:f2}MB/s";
            }
            if (downloadSpeed >= 1024)
            {
                return $"{downloadSpeed / 1024:f2}KB/s";
            }
            return $"{downloadSpeed:f2}B/s";
        }

        /// <summary>
        /// 确保字符串以某字符串结尾
        /// </summary>
        /// <param name="source"></param>
        /// <param name="endWith"></param>
        /// <returns></returns>
        public static void EnsureEndWith(ref string source, string endWith)
        {
            if (!source.EndsWith(endWith))
            {
                source += endWith;
            }
        }
        
        /// <summary>
        /// 获取显示大小
        /// </summary>
        /// <param name="downloadSize"></param>
        /// <returns></returns>
        public static string GetDisplaySize(long downloadSize)
        {
            if (downloadSize >= 1024 * 1024)
            {
                return $"{downloadSize * ConstMgr.Bytes2Mb:f2}MB";
            }
            if (downloadSize >= 1024)
            {
                return $"{downloadSize / 1024:f2}KB";
            }
            return $"{downloadSize:f2}B";
        }
    }
}