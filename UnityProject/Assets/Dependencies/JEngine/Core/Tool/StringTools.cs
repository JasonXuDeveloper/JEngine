using System;

namespace JEngine.Core
{
    public static partial class Tools
    {
        /// <summary>
        /// 当前时间戳(s)
        /// </summary>
        /// <returns></returns>
        public static long TimeStamp => (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;

        /// <summary>
        /// 字节转显示的文本
        /// </summary>
        /// <param name="downloadSpeed"></param>
        /// <returns></returns>
        public static string GetDisplaySize(float downloadSpeed)
        {
            if (downloadSpeed >= 1024 * 1024)
            {
                return $"{downloadSpeed * ConstMgr.Bytes2Mb:f2}MB";
            }
            if (downloadSpeed >= 1024)
            {
                return $"{downloadSpeed / 1024:f2}KB";
            }
            return $"{downloadSpeed:f2}B";
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
    }
}