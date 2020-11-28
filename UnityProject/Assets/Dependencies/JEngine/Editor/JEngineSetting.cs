//
// JEngineSetting
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2020 JEngine
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace JEngine.Editor
{
    internal class JEngineSetting : EditorWindow
    {
        private const string prefix = "JEngine.Editor.Setting";

        #region 代表了不同字符串的序号的常量
        public const int JENGINE_SETTING = 0;
        public const int DISPLAY_LANGUAGE = 1;
        public const int START_UP_SCENE = 2;
        public const int LAST_DLL_CLEAN_TIME = 3;
        public const int DATE_FORMAT = 4;
        public const int JUMP_TO_START_UP_SCENE = 5;
        public const int MISSING_ASSEMBLY = 6;
        public const int MISSING_ASSEMBLY_BTN = 7;
        public const int ERROR_RESCUE_TOOLS = 8;
        public const int ERROR_RESCUE_TOOLS_INFO = 9;
        public const int INVALID_SCENE_OBJECT = 10;
        public const int PANEL_INFO = 11;
        #endregion
        
        public static Color RED_COLOR = new Color32(245, 80, 98, 255);


        private static string[][] Titles =
        {
            new[] {"JEngine设置面板", "JEngine Setting Panel"},
            new[] {"显示语言", "Display Language"},
            new[] {"启动场景", "Start Up Scene"},
            new[] {"上次处理热更DLL时间", "Last Hot Update DLL clean up time"},
            new[] {"yyyy年MM月dd日 HH:mm:ss", "MM/dd/yyyy HH:mm:ss"}, //日期格式
            new[] {"运行后跳转启动场景", "Jump to start up scene when launch"},
            new[] {"修复Dependencies目录下\nType or namespace name\nof ILRuntime等Not found错误", "Fix under Dependencies directory,\nType or namespace name of ILRuntime, etc,\nNot found error"},
            new[] {"修复", "Fix"},
            new[] {"错误修复工具", "Error rescue tools"}, //修复Bug工具的标题
            new[]
            {
                "一键修复可能会出现的特定错误\n" +
                "注意：修复中会造成一定时间的无反应，请耐心等待",
                "Fix errors that might occurs by one click\n" +
                "Warning: Unity might not be responding while rescuing, just be patient"
            }, //修复Bug工具的提示
            new[] {"选择的资源不是场景资源", "Invalid resource type, please choose scene resource"},
            new[] {"面板上的设置仅对编辑器有效", "All settings in the panel only work on editor"},//面板提示
        };

        /// <summary>
        /// 语言
        /// </summary>
        public static JEngineLanguage Language = JEngineLanguage.中文;

        /// <summary>
        /// 跳转场景路径
        /// </summary>
        public static string StartUpScenePath
        {
            get => PlayerPrefs.GetString($"{prefix}.StartUpScenePath", "Assets/Init.unity");
            set => PlayerPrefs.SetString($"{prefix}.StartUpScenePath", value);
        }
        
        /// <summary>
        /// 启动后是否跳转场景
        /// </summary>
        public static bool JumpStartUp
        {
            get => PlayerPrefs.GetString($"{prefix}.JumpStartUpScene", "1") == "1";
            set => PlayerPrefs.SetString($"{prefix}.JumpStartUpScene", value?"1":"0");
        }
        
        /// <summary>
        /// 上次处理热更DLL时间
        /// </summary>
        public static string LastDLLCleanUpTime
        {
            get => PlayerPrefs.GetString($"{prefix}.LastDLLCleanUpTime");
            set => PlayerPrefs.SetString($"{prefix}.LastDLLCleanUpTime", value);
        }

        [MenuItem("JEngine/Setting",priority = 1998)]
        private static void ShowWindow()
        {
            var window = GetWindow<JEngineSetting>(GetString(JENGINE_SETTING));
            window.minSize = new Vector2(300, 500);
            window.Show();
        }

        private int GetSpace(float percentage)
        {
            int result = (int)(this.position.width * percentage);
            return result;
        }

        private void OnGUI()
        {
            //绘制标题
            GUILayout.Space(10);
            GUIStyle textStyle = new GUIStyle();
            textStyle.fontSize = 24;
            textStyle.normal.textColor = Color.white;
            textStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(GetString(JENGINE_SETTING), textStyle);
            
            GUILayout.Space(10);
            
            MakeHorizontal(GetSpace(0.1f), () =>
            {
                EditorGUILayout.HelpBox(GetString(PANEL_INFO),MessageType.Info);
            });
            
            GUILayout.Space(10);

            //选择语言
            MakeHorizontal(GetSpace(0.1f), () =>
            {
                Language = (JEngineLanguage) EditorGUILayout.EnumPopup(GetString(DISPLAY_LANGUAGE), Language);
                this.titleContent.text = GetString(JENGINE_SETTING);
            });

            //选择场景
            MakeHorizontal(GetSpace(0.1f), () =>
            {
                var sceneObj = EditorGUILayout.ObjectField(GetString(START_UP_SCENE),
                    AssetDatabase.LoadAssetAtPath<Object>(StartUpScenePath),
                    typeof(Object), false);
                if (sceneObj == null || !AssetDatabase.GetAssetPath(sceneObj).EndsWith(".unity"))
                {
                    ShowNotification(new GUIContent(GetString(INVALID_SCENE_OBJECT)),3);
                }
                else
                {
                    StartUpScenePath = AssetDatabase.GetAssetPath(sceneObj);   
                }
            });

            //是否跳转
            MakeHorizontal(GetSpace(0.1f),
                () => { JumpStartUp = EditorGUILayout.Toggle(GetString(JUMP_TO_START_UP_SCENE), JumpStartUp); });

            //上次处理热更DLL时间
            MakeHorizontal(GetSpace(0.1f), () => { EditorGUILayout.LabelField(GetString(LAST_DLL_CLEAN_TIME)); });
            MakeHorizontal(GetSpace(0.1f), () =>
            {
                GUI.enabled = false;
                textStyle = new GUIStyle(EditorStyles.textField.name);
                textStyle.alignment = TextAnchor.MiddleCenter;
                EditorGUILayout.TextField(LastDLLCleanUpTime, textStyle);
                GUI.enabled = true;
            });

            //bug修复
            GUILayout.Space(30);
            MakeHorizontal(GetSpace(0.1f), () =>
            {
                textStyle = new GUIStyle();
                textStyle.fontSize = 16;
                textStyle.normal.textColor = RED_COLOR;
                textStyle.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label(GetString(ERROR_RESCUE_TOOLS), textStyle);
            });
            MakeHorizontal(GetSpace(0.1f), () =>
            {
                EditorGUILayout.HelpBox(GetString(ERROR_RESCUE_TOOLS_INFO),MessageType.Warning);
            });
            
            //修复Missing Type
            GUILayout.Space(10);
            MakeHorizontal(GetSpace(0.1f), () =>
            {
                EditorGUILayout.LabelField(GetString(MISSING_ASSEMBLY),GUILayout.MinHeight(50));
                if (GUILayout.Button(GetString(MISSING_ASSEMBLY_BTN),GUILayout.MinHeight(50)))
                {
                    UnityEditor.PlayerSettings.allowUnsafeCode = false;
                    UnityEditor.PlayerSettings.allowUnsafeCode = true;
                }
            });
        }

        public static void MakeHorizontal(int space,Action act)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(space);
            act();
            GUILayout.Space(space);
            GUILayout.EndHorizontal();
        }

        public static string GetString(int index)
        {
            return Titles[index][(int)Language];
        }
    }

    public enum JEngineLanguage
    {
        中文 = 0,
        English = 1
    }
}