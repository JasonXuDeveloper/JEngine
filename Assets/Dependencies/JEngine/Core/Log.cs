using UnityEngine;

namespace JEngine.Core
{
    public class Log
    {
        public static void Print(object message)
        {
            Debug.Log("[JEngine] " + message);
        }

        public static void PrintWarning(object message)
        {
            Debug.LogWarning("[JEngine Warnning] " + message);
        }

        public static void PrintError(object message)
        {
            Debug.LogError("[JEngine Error] " + message);
        }
    }
}
