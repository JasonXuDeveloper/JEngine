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
using System.Collections.Generic;
using System.IO;
using JEngine.Core;
using libx;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Analytics;
using Object = UnityEngine.Object;

namespace JEngine.Editor
{
    internal class Setting : EditorWindow
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
        public const int SCENES_TITLE = 12;
        public const int LOAD_SCENE_BTN = 13;
        public const int LOAD_SCENE_ADDITIVE_BTN = 14;
        public const int UNLOAD_SCENE_BTN = 15;
        public const int SCENE_FILTER = 16;
        public const int CLASSBIND_TOOLS = 17;
        public const int CLASSBIND_AUTO_GET_FIELS= 18;
        public const int CLASSBIND_AUTO_SET_TYPES = 19;
        #endregion
        
        public static Color RED_COLOR = new Color32(245, 80, 98, 255);
        public static Color CYAN_COLOR = new Color32(144, 245, 229, 255);
        public static Color GREEN_COLOR = new Color32(98, 245, 131, 255);


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
            new[] {"错误修复工具", "Error Rescue Tools"}, //修复Bug工具的标题
            new[]
            {
                "一键修复可能会出现的特定错误\n" +
                "注意：修复中会造成一定时间的无反应，请耐心等待",
                "Fix errors that might occurs by one click\n" +
                "Warning: Unity might not be responding while rescuing, just be patient"
            }, //修复Bug工具的提示
            new[] {"选择的资源不是场景资源", "Invalid resource type, please choose scene resource"},
            new[] {"面板上的大部分设置是针对于Unity编辑器的，对打包后的客户端无效", "Most features in the setting panel is editor only, does not effect the built application"},//面板提示
            new[] {"热更场景快捷操作", "Hot Update Scenes Shortcuts"},
            new[] {"打开", "Open"},
            new[] {"加载", "Load"},
            new[] {"卸载", "Unload"},
            new[] {"筛选场景", "Scene Filter"},
            new[] {"ClassBind助手", "ClassBind Tools"},
            new[] {"自动获取全部field", "Auto get fields for all"},
            new[] {"自动处理全部fieldType", "Auto get fieldTypes for all"},
        };

        /// <summary>
        /// 语言
        /// </summary>
        public static JEngineLanguage Language
        {
	        get => (JEngineLanguage)(int.Parse(PlayerPrefs.GetString($"{prefix}.PanelLanguage", "0")));
	        set => PlayerPrefs.SetString($"{prefix}.PanelLanguage", value == JEngineLanguage.中文 ? "0" : "1");
        }

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

        /// <summary>
        /// 是否展示热更场景
        /// </summary>
        private bool ShowScenes = true;
        
        /// <summary>
        /// 筛选场景搜索框
        /// </summary>
        [SerializeField]
        private AutocompleteSearchField sceneSearchField;

        /// <summary>
        /// 筛选场景字符串
        /// </summary>
        private string sceneSearchPattern = "";

        private Vector2 scrollPos;

        [MenuItem("JEngine/Setting #&J",priority = 1998)]
        private static void ShowWindow()
        {
            var window = GetWindow<Setting>(GetString(JENGINE_SETTING));
            window.minSize = new Vector2(300, 500);
            window.Show();
        }

        private void OnEnable()
        {
	        if (sceneSearchField == null) sceneSearchField = new AutocompleteSearchField();
	        sceneSearchField.onInputChanged = (s) =>
	        {
		        sceneSearchPattern = s;
	        };
	        sceneSearchField.onConfirm = (s) =>
	        {
		        sceneSearchPattern = s;
	        };
	        scrollPos = new Vector2(position.width, position.height);
        }

        private int GetSpace(float percentage)
        {
            int result = (int)(this.position.width * percentage);
            return result;
        }

        private void OnGUI()
        {
	        //只滚动y轴
	        var _scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
	        scrollPos = new Vector2(scrollPos.x, _scrollPos.y);
	        
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
            
            //直接进热更场景
            GUILayout.Space(30);
            MakeHorizontal(GetSpace(0.1f), () =>
            {
                textStyle = new GUIStyle();
                textStyle.fontSize = 16;
                textStyle.normal.textColor = CYAN_COLOR;
                textStyle.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label(GetString(SCENES_TITLE), textStyle);
            });
            GUILayout.Space(10);
            MakeHorizontal(GetSpace(0.1f), () =>
            {
                MakeFoldOut(ref ShowScenes, "热更场景列表", () => { });
            });
            //如果场景
            if (ShowScenes)
            {
                //筛选框
                sceneSearchField.OnGUI(GetSpace(0.1f));
                
                //获取热更场景
                Assets.basePath = BuildScript.outputPath + Path.DirectorySeparatorChar;
                Assets.loadDelegate = AssetDatabase.LoadAssetAtPath; 

                var assets = new List<string>();
                var rules = BuildScript.GetBuildRules();
                foreach (var asset in rules.scenesInBuild)
                {
                    var path = AssetDatabase.GetAssetPath(asset);
                    if (string.IsNullOrEmpty(path))
                    {
                        continue;
                    }
                    assets.Add(path); 
                } 
                foreach (var rule in rules.rules)
                {
                    if (rule.searchPattern.Contains("*.unity"))
                    {
                        assets.AddRange(rule.GetAssets());
                    }
                }

                foreach (var asset in assets)
                {
                    MakeHorizontal(GetSpace(0.1f), () =>
                    {
                        GUI.enabled = false;
                        Object sceneObj = AssetDatabase.LoadAssetAtPath<Object>(asset);
                        
                        //筛选
                        if (!sceneObj.name.StartsWith(sceneSearchPattern))
                        {
	                        GUI.enabled = true;
	                        return;
                        }
                        
                        EditorGUILayout.ObjectField(sceneObj,
                            typeof(Object), false);
                        GUI.enabled = true;

                        GUILayout.Space(15);
                        
                        if (GUILayout.Button(GetString(LOAD_SCENE_BTN)))
                        {
                            EditorSceneManager.OpenScene(asset);
                        }
                        
                        GUILayout.Space(5);
                        
                        if (GUILayout.Button(GetString(LOAD_SCENE_ADDITIVE_BTN)))
                        {
                            EditorSceneManager.OpenScene(asset, OpenSceneMode.Additive);
                        }
                        
                        GUILayout.Space(5);
                        
                        if (GUILayout.Button(GetString(UNLOAD_SCENE_BTN)))
                        {
                            EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath(asset), true);
                        }
                    });
                }
            }
            
            //Classbind工具
            GUILayout.Space(30);
            MakeHorizontal(GetSpace(0.1f), () =>
            {
	            textStyle = new GUIStyle();
	            textStyle.fontSize = 16;
	            textStyle.normal.textColor = GREEN_COLOR;
	            textStyle.alignment = TextAnchor.MiddleCenter;
	            GUILayout.Label(GetString(CLASSBIND_TOOLS), textStyle);
            });
            GUILayout.Space(10);
            MakeHorizontal(GetSpace(0.1f), () =>
            {
	            if (GUILayout.Button(GetString(CLASSBIND_AUTO_GET_FIELS)))
	            {
		            foreach (var cb in FindObjectsOfType<ClassBind>())
		            {
			            ClassBindEditor.DoConvert(cb);
		            }
	            }
            });
            
            MakeHorizontal(GetSpace(0.1f), () =>
            {
	            if (GUILayout.Button(GetString(CLASSBIND_AUTO_SET_TYPES)))
	            {
		            foreach (var cb in FindObjectsOfType<ClassBind>())
		            {
			            ClassBindEditor.DoFieldType(cb);
		            }
	            }
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
            GUILayout.Space(10);
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

            try
            {
	            EditorGUILayout.EndScrollView();
            }
            catch
            {
	            // ignored
            }
        }


        public static void MakeFoldOut(ref bool fold,string name, Action act)
        {
            fold = EditorGUILayout.Foldout(fold, name);
            if(fold)
            {
                act();
            }
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
    
    [Serializable]
	public class AutocompleteSearchField
	{
		static class Styles
		{
			public const float resultHeight = 20f;
			public const float resultsBorderWidth = 2f;
			public const float resultsMargin = 15f;
			public const float resultsLabelOffset = 2f;

			public static readonly GUIStyle entryEven;
			public static readonly GUIStyle entryOdd;
			public static readonly GUIStyle labelStyle;
			public static readonly GUIStyle resultsBorderStyle;

			static Styles()
			{
				entryOdd = new GUIStyle("CN EntryBackOdd");
				entryEven = new GUIStyle("CN EntryBackEven");
				resultsBorderStyle = new GUIStyle("hostview");

				labelStyle = new GUIStyle(EditorStyles.label)
				{
					alignment = TextAnchor.MiddleLeft,
					richText = true
				};
			}
		}

		public Action<string> onInputChanged;
		public Action<string> onConfirm;
		public string searchString;
		public int maxResults = 15;

		[SerializeField]
		List<string> results = new List<string>();

		[SerializeField]
		int selectedIndex = -1;

		SearchField searchField;

		Vector2 previousMousePosition;
		bool selectedIndexByMouse;

		bool showResults;

		public void AddResult(string result)
		{
			if(!results.Contains(result))
				results.Add(result);
		}

		public void ClearResults()
		{
			results.Clear();
		}

		public void OnToolbarGUI(float space =0)
		{
			Draw(asToolbar:true,space);
		}

		public void OnGUI(float space = 0)
		{
			Draw(asToolbar:false,space);
		}

		void Draw(bool asToolbar,float space)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(space);
			var rect = GUILayoutUtility.GetRect(1, 1, 18, 18);
			DoSearchField(rect, asToolbar);
			GUILayout.Space(space);
			GUILayout.EndHorizontal();
			rect.y += 18;
			DoResults(rect);
		}

		void DoSearchField(Rect rect, bool asToolbar)
		{
			if(searchField == null)
			{
				searchField = new SearchField();
				searchField.downOrUpArrowKeyPressed += OnDownOrUpArrowKeyPressed;
			}

			if (searchString != "")
			{
				var result = asToolbar
					? searchField.OnToolbarGUI(rect, searchString)
					: searchField.OnGUI(rect, searchString);
				
				if (result != searchString && onInputChanged != null)
				{
					onInputChanged(result);
					selectedIndex = -1;
					showResults = true;
				}
			
				searchString = result;
			}
			else
			{
				var s = asToolbar
					? searchField.OnToolbarGUI(rect, Setting.GetString(Setting.SCENE_FILTER))
					: searchField.OnGUI(rect, Setting.GetString(Setting.SCENE_FILTER));
				searchString = "";
			}
			

			if(HasSearchbarFocused())
			{
				RepaintFocusedWindow();
			}
		}

		void OnDownOrUpArrowKeyPressed()
		{
			var current = Event.current;

			if (current.keyCode == KeyCode.UpArrow)
			{
				current.Use();
				selectedIndex--;
				selectedIndexByMouse = false;
			}
			else
			{
				current.Use();
				selectedIndex++;
				selectedIndexByMouse = false;
			}

			if (selectedIndex >= results.Count) selectedIndex = results.Count - 1;
			else if (selectedIndex < 0) selectedIndex = -1;
		}

		void DoResults(Rect rect)
		{
			if(results.Count <= 0 || !showResults) return;

			var current = Event.current;
			rect.height = Styles.resultHeight * Mathf.Min(maxResults, results.Count);
			rect.x = Styles.resultsMargin;
			rect.width -= Styles.resultsMargin * 2;

			var elementRect = rect;

			rect.height += Styles.resultsBorderWidth;
			GUI.Label(rect, "", Styles.resultsBorderStyle);

			var mouseIsInResultsRect = rect.Contains(current.mousePosition);

			if(mouseIsInResultsRect)
			{
				RepaintFocusedWindow();
			}

			var movedMouseInRect = previousMousePosition != current.mousePosition;

			elementRect.x += Styles.resultsBorderWidth;
			elementRect.width -= Styles.resultsBorderWidth * 2;
			elementRect.height = Styles.resultHeight;

			var didJustSelectIndex = false;

			for (var i = 0; i < results.Count && i < maxResults; i++)
			{
				if(current.type == EventType.Repaint)
				{
					var style = i % 2 == 0 ? Styles.entryOdd : Styles.entryEven;

					style.Draw(elementRect, false, false, i == selectedIndex, false);

					var labelRect = elementRect;
					labelRect.x += Styles.resultsLabelOffset;
					GUI.Label(labelRect, results[i], Styles.labelStyle);
				}
				if(elementRect.Contains(current.mousePosition))
				{
					if(movedMouseInRect)
					{
						selectedIndex = i;
						selectedIndexByMouse = true;
						didJustSelectIndex = true;
					}
					if(current.type == EventType.MouseDown)
					{
						OnConfirm(results[i]);
					}
				}
				elementRect.y += Styles.resultHeight;
			}

			if(current.type == EventType.Repaint && !didJustSelectIndex && !mouseIsInResultsRect && selectedIndexByMouse)
			{
				selectedIndex = -1;
			}

			if((GUIUtility.hotControl != searchField.searchFieldControlID && GUIUtility.hotControl > 0)
				|| (current.rawType == EventType.MouseDown && !mouseIsInResultsRect))
			{
				showResults = false;
			}

			if(current.type == EventType.KeyUp && current.keyCode == KeyCode.Return && selectedIndex >= 0)
			{
				OnConfirm(results[selectedIndex]);
			}

			if(current.type == EventType.Repaint)
			{
				previousMousePosition = current.mousePosition;
			}
		}

		void OnConfirm(string result)
		{
			searchString = result;
			if(onConfirm != null) onConfirm(result);
			if(onInputChanged != null) onInputChanged(result);
			RepaintFocusedWindow();
			GUIUtility.keyboardControl = 0; // To avoid Unity sometimes not updating the search field text
		}

		bool HasSearchbarFocused()
		{
			return GUIUtility.keyboardControl == searchField.searchFieldControlID;
		}

		static void RepaintFocusedWindow()
		{
			if(EditorWindow.focusedWindow != null)
			{
				EditorWindow.focusedWindow.Repaint();
			}
		}
	}
}