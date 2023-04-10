using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JEngine.Editor
{
    internal static class SetData
    {
        public static bool hasAdded;
        private static string path = "JEngine.lock";

        public static string GetPrefix()
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
                if (Setting.Language == JEngineLanguage.English)
                {
                    Application.OpenURL("https://docs.xgamedev.net/documents/0.8/");
                }
                else
                {
                    Application.OpenURL("https://docs.xgamedev.net/zh/documents/0.8/");
                }
                InjectDefineSymbol();
            }
            else
            {
                prefix = File.ReadAllText(fPath);
            }

            return prefix;
        }
        
        public static void Update()
        {
            string prefix = GetPrefix();
            InjectDefineSymbol();
            hasAdded = true;
            Setting.SetPrefix(prefix);
        }

        private static void InjectDefineSymbol()
        {
            //注入宏
            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);
            var org = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var d = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            string[] Symbols = new string[] { "ILRuntime" };
            List<string> dfList = d.Split(';').ToList();
            dfList.AddRange(Symbols.Except(dfList));
            d = string.Join(";", dfList.Distinct().ToArray());
            if (org != d)
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, d);
        }
    }
}