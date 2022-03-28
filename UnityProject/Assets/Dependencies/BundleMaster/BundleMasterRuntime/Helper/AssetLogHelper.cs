using UnityEngine;

namespace BM
{
    public static class AssetLogHelper
    {
        public static void Log(string message)
        {
            Debug.Log(message);
        }
        
        public static void LogError(string message)
        {
            Debug.LogError(message);
        }
    }
}