// Settings.cs
//
//  Author:
//        JasonXuDeveloper <jason@xgamedev.net>
//
//  Copyright (c) 2025 JEngine
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.

using JEngine.Editor;
using UnityEditor;
using UnityEngine;

namespace JEngine.Core.Editor
{
    public class Settings : ScriptableObject
    {
        [Header("Package Settings")]
        public string packageName = "DefaultPackage";

        public BuildTarget buildTarget = BuildTarget.Android;

        [Header("Build Options")]
        [Tooltip("Clear build cache before building. Uncheck to enable incremental builds (faster)")]
        public bool clearBuildCache;

        [Tooltip("Use asset dependency database to improve build speed")]
        public bool useAssetDependDB = true;
        
        private static Settings _instance;
        private static readonly string SettingsPath = "Assets/Editor/JEngineSettings.asset";

        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<Settings>(SettingsPath);

                    if (_instance == null)
                    {
                        _instance = CreateInstance<Settings>();

                        // Ensure the Editor directory exists
                        if (!AssetDatabase.IsValidFolder("Assets/Editor"))
                        {
                            AssetDatabase.CreateFolder("Assets", "Editor");
                        }

                        AssetDatabase.CreateAsset(_instance, SettingsPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        
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
                }
                return _instance;
            }
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}