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
                //注入宏
                var target = EditorUserBuildSettings.activeBuildTarget;
                var group = BuildPipeline.GetBuildTargetGroup(target);
                var d = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                if (!d.Contains("INIT_JE"))
                {
                    if (!d.EndsWith(";"))
                    {
                        d += ";";
                    }
                }
                else
                {
                    d = d.Replace("INIT_JE;", "").Replace("INIT_JE", "");
                }

                d += "INIT_JE;";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group,d);
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