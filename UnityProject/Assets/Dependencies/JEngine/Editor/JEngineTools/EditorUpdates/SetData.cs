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
        public static bool HasAdded;
        private static string _path = "JEngine.proj";
        private static JEngineProjData _data;

        public static void UpdateData(Action<JEngineProjData> func)
        {
            func.Invoke(_data);
            Span<byte> data = stackalloc byte[_data.Size()];
            _data.AsBinary(ref data);
            File.WriteAllBytes(Path.Combine(Application.dataPath, _path), data.ToArray());
        }

        public static string GetPrefix()
        {
            if (_data == null)
            {
                _data = new JEngineProjData();
                //看看文件存不存在，不存在就创建和提示
                string fPath = Path.Combine(Application.dataPath, _path);
                if (!File.Exists(fPath))
                {
                    //兼容老版本
                    bool flag = false;
                    if (File.Exists(Path.Combine(Application.dataPath, "JEngine.lock")))
                    {
                        _data.Prefix = File.ReadAllText(Path.Combine(Application.dataPath, "JEngine.lock"));
                        _data.EncryptPassword = PlayerPrefs.GetString($"{_data.Prefix}.EncryptPassword", "");
                        File.Delete(Path.Combine(Application.dataPath, "JEngine.lock"));
                    }
                    else
                    {
                        _data.Prefix = Guid.NewGuid().ToString();
                        flag = true;
                    }

                    Span<byte> data = stackalloc byte[_data.Size()];
                    _data.AsBinary(ref data);
                    File.WriteAllBytes(fPath, data.ToArray());
                    if (flag)
                    {
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
                    }

                    InjectDefineSymbol();
                }
                else
                {
                    //读取文件
                    Span<byte> data = File.ReadAllBytes(fPath);
                    _data.FromBinary(ref data);
                }
            }

            return _data.Prefix;
        }

        public static void Update()
        {
            string prefix = GetPrefix();
            InjectDefineSymbol();
            HasAdded = true;
            Setting.SetPrefix(prefix);
            Setting.EncryptPassword = _data.EncryptPassword;
        }

        private static void InjectDefineSymbol()
        {
            //注入宏
            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);
            var org = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var d = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            string[] symbols = new string[] { "ILRuntime" };
            List<string> dfList = d.Split(';').ToList();
            dfList.AddRange(symbols.Except(dfList));
            d = string.Join(";", dfList.Distinct().ToArray());
            if (org != d)
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, d);
        }
    }
}