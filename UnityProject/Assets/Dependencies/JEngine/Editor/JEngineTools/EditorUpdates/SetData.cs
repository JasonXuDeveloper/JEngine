using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace JEngine.Editor
{
    internal static class SetData
    {
        public static bool hasAdded;
        private static string path = "JEngine.lock";
        
        public static void Update()
        {
            string prefix = "";
            
            //看看文件存不存在，不存在就创建和提示
            string fPath = Path.Combine(Application.dataPath, path);
            if (!File.Exists(fPath))
            {
                prefix = Guid.NewGuid().ToString();
                File.WriteAllText(fPath, prefix);
                //提示看文档
                Debug.LogError(Setting.GetString(SettingString.NoticeText));
                EditorUtility.DisplayDialog(Setting.GetString(SettingString.Notice),
                    Setting.GetString(SettingString.NoticeText), Setting.GetString(SettingString.Done));
            }
            else
            {
                prefix = File.ReadAllText(fPath);
            }
            
            hasAdded = true;
            Setting.SetPrefix(prefix);
        }
    }
}