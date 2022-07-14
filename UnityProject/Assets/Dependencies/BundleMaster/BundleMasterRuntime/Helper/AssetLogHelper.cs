using System;
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
        
        public static void LogError(Exception e)
        {
            if (e.Data.Contains("StackTrace"))
            {
                Debug.LogError($"{e.Data["StackTrace"]}\n{e}");
                return;
            }
            string str = e.ToString();
            Debug.LogError(str);
        }
    }
}