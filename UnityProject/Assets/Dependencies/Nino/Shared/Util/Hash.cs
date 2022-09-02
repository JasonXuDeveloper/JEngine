using System;

namespace Nino.Shared.Util
{
    public static class Hash
    {
        /// <summary>
        /// Get hash code of a string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static unsafe int GetStringHashCode(this string key) {
            int i;
            int len = key.Length;
            int hash = 1315423911;
            fixed (char* ptr = key)
            {
                for(i = 0; i < len; i++)
                {
                    hash ^= ((hash << 5) + *(ptr + i) + (hash >> 2));
                }

                return hash;
            }
        }
        
        public static int GetTypeHashCode(this Type type)
        {
#if ILRuntime
            return type.GetHashCode();
#else
            //TODO a faster way to get the hash code of a type
            return type.TypeHandle.Value.GetHashCode();
#endif
        }
    }
}