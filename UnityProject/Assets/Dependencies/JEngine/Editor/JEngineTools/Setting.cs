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
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace JEngine.Editor
{
	internal class Setting : EditorWindow
	{
		private static string prefix;

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
		public const int CLASSBIND_AUTO_GET_FIELS = 18;
		public const int CLASSBIND_AUTO_SET_TYPES = 19;
		public const int LOCAL_JENGINE = 20;
		public const int HOT_JENGINE = 21;
		public const int CHOOSE_BTN = 22;
		public const int UPDATE_JENGINE = 23;
		public const int UPDATE_HELPBOX = 24;
		public const int XASSET_TITLE = 25;
		public const int XASSET_HELPBOX = 26;
		public const int XASSET_BUTTON = 27;
		public const int XASSET_ACCOUNT = 28;
		public const int XASSET_PASSWORD = 29;
		public const int LOGIN = 30;
		public const int SIGN_UP = 31;
		public const int LOG_OUT = 32;
		public const int XASSET_REMAIN = 33;
		public const int RECHARGE = 34;
		public const int CHARGE = 35;
		public const int XASSET_CHARGE_TXT = 36;

		#endregion

		public static Color RED_COLOR = new Color32(190, 61, 75, 255);
		public static Color CYAN_COLOR = new Color32(75, 226, 235, 255);
		public static Color GREEN_COLOR = new Color32(73, 225, 146, 255);
		public static Color PURPLE_COLOR = new Color32(141, 107, 225, 255);


		private static string[][] Titles =
		{
			new[] {"JEngine设置面板", "JEngine Setting Panel"},
			new[] {"显示语言", "Display Language"},
			new[] {"启动场景", "Start Up Scene"},
			new[] {"上次处理热更DLL时间", "Last Hot Update DLL clean up time"},
			new[] {"yyyy年MM月dd日 HH:mm:ss", "MM/dd/yyyy HH:mm:ss"}, //日期格式
			new[] {"运行后跳转启动场景", "Jump to start up scene when launch"},
			new[]
			{
				"修复Dependencies目录下\nType or namespace name\nof ILRuntime等Not found错误",
				"Fix under Dependencies directory,\nType or namespace name of ILRuntime, etc,\nNot found error"
			},
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
			new[]
			{
				"面板上的大部分设置是针对于Unity编辑器的，对打包后的客户端无效",
				"Most features in the setting panel is editor only, does not effect the built application"
			}, //面板提示
			new[] {"热更场景快捷操作", "Hot Update Scenes Shortcuts"},
			new[] {"打开", "Open"},
			new[] {"加载", "Load"},
			new[] {"卸载", "Unload"},
			new[] {"筛选场景", "Scene Filter"},
			new[] {"ClassBind助手", "ClassBind Tools"},
			new[] {"自动获取全部field", "Auto get fields for all"},
			new[] {"自动处理全部fieldType", "Auto get fieldTypes for all"},
			new[] {"本地工程JEngine框架路径", "Local JEngine Source Code"},
			new[] {"热更工程JEngine框架路径", "Hot Update JEngine Source Code"},
			new[] {"选择", "Choose"},
			new[] {"更新JEngine", "Update JEngine"},
			new[]
			{
				"更新JEngine后会删除热更工程的JEngine源码，如有修改请做好备份后再进行更新操作",
				"Update JEngine will delete JEngine's source code in hot update solution, " +
				"please make sure you did a back up before upgrading JEngine."
			},
			new[] {"XAsset Pro", "XAsset Pro"},
			new[]
			{
				"\nXAsset Pro升级需要以下步骤：\n" +
				"1）注册登入XAsset账号\n" +
				"2）确保已经在官网订阅过XAsset Pro\n" +
				"3）点击\"下载XAsset Pro\"按钮\n" +
				"4）删除本地Dependencies/XAsset文件夹\n" +
				"5）导入xasset-pro.unitypackage\n" +
				"6）将Init场景的相关脚本替换（Updater换为Initializer，UpgradeScene可能需要重新拖拽）\n" +
				"7）请熟悉XAsset Pro的规则配置，因为XAsset Pro的AB包规则需自定义，JEngine不再做特殊处理\n" +
				"8）理论上XAsset Pro支持热更启动场景，所以建议重构Init场景\n" +
				"9）Unity，ProjectSetting，Player内，在Scripting Define Symbols里面添加`XASSET_PRO`\n\n" +
				"请在升级XAsset Pro之前务必读一遍其文档\n",

				"\nUpgrade to XAsset Pro requires the following steps:\n" +
				"1）Register or Login to XAsset account\n" +
				"2）Make sure that you had subscribed XAsset Pro on the website\n" +
				"3）Click on \"Download XAsset Pro\" button\n" +
				"4）Delete Dependencies/XAsset directory\n" +
				"5）Import xasset-pro.unitypackage\n" +
				"6）Replace related scripts in Init scene (Updater becomes Initializer, UpgradeScene might need to redo)\n" +
				"7）Please be familiar with setting up XAsset Pro's build rules, " +
				"because XAsset Pro's build rules needs to be customize by you and JEngine can't do it for you this time\n" +
				"8）Theoretically XAsset Pro supports hot upgrade the init scene, so it is recommended to redesign your init scene\n" +
				"9）In Unity, ProjectSetting, Player, add `XASSET_PRO` into Scripting Define Symbols\n\n" +
				"Please get to read the documentation for XAsset Pro before you start updgrade\n"
			},
			new[] {"下载XAsset Pro", "Download XAsset Pro"},
			new[] {"XAsset账号", "XAsset Account"},
			new[] {"XAsset密码", "XAsset Password"},
			new[] {"登入", "Login"},
			new[] {"注册", "Sign Up"},
			new[] {"退出登入", "Log Out"},
			new[] {"Pro订阅剩余时间：{0}天", "Pro subscription remain: {0}days"},
			new[] {"续费", "Recharge"},
			new[] {"购买", "Buy"},
			new[] {"当前还没购买XAsset Pro", "Hasn't bought XAsset Pro yet"},
		};

		/// <summary>
		/// 语言
		/// </summary>
		public static JEngineLanguage Language
		{
			get => (JEngineLanguage) (int.Parse(PlayerPrefs.GetString($"{prefix}.PanelLanguage", "0")));
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
			set => PlayerPrefs.SetString($"{prefix}.JumpStartUpScene", value ? "1" : "0");
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
		/// 本地框架路径
		/// </summary>
		public static string LocalPath
		{
			get => PlayerPrefs.GetString($"{prefix}.LocalPath",
				new DirectoryInfo(Application.dataPath).FullName + "/Dependencies/JEngine");
			set => PlayerPrefs.SetString($"{prefix}.LocalPath", value);
		}

		/// <summary>
		/// 热更框架路径
		/// </summary>
		public static string HotPath
		{
			get => PlayerPrefs.GetString($"{prefix}.HotPath",
				new DirectoryInfo(Application.dataPath).Parent.FullName + "/HotUpdateScripts/JEngine");
			set => PlayerPrefs.SetString($"{prefix}.HotPath", value);
		}

		/// <summary>
		/// 是否已登入XAsset
		/// </summary>
		public static bool XAssetLoggedIn
		{
			get => PlayerPrefs.GetString($"{prefix}.XAssetLoggedIn", "0") == "1";
			set => PlayerPrefs.SetString($"{prefix}.XAssetLoggedIn", value ? "1" : "0");
		}
		
		/// <summary>
		/// XAsset账号
		/// </summary>
		public static string XAssetAccount
		{
			get => PlayerPrefs.GetString($"{prefix}.XAssetAccount","");
			set => PlayerPrefs.SetString($"{prefix}.XAssetAccount", value);
		}

		
		/// <summary>
		/// XAsset密码
		/// </summary>
		public static string XAssetPassword
		{
			get => PlayerPrefs.GetString($"{prefix}.XAssetPassword","");
			set => PlayerPrefs.SetString($"{prefix}.XAssetPassword", value);
		}
		
		/// <summary>
		/// XAsset剩余时间
		/// </summary>
		public static int XAssetRemain
		{
			get => PlayerPrefs.GetInt($"{prefix}.XAssetRemain",0);
			set => PlayerPrefs.SetInt($"{prefix}.XAssetRemain", value);
		}


		/// <summary>
		/// 是否展示热更场景
		/// </summary>
		private bool ShowScenes = true;

		/// <summary>
		/// 筛选场景搜索框
		/// </summary>
		[SerializeField] private AutocompleteSearchField sceneSearchField;

		/// <summary>
		/// 筛选场景字符串
		/// </summary>
		private string sceneSearchPattern = "";

		private Vector2 scrollPos;

		public static Setting Instance;

		[MenuItem("JEngine/Setting #&J", priority = 1998)]
		private static void ShowWindow()
		{
			var window = GetWindow<Setting>(GetString(JENGINE_SETTING));
			window.minSize = new Vector2(300, 500);
			window.Show();
			Instance = window;
		}

		public static void Refresh()
		{
			if (Instance != null)
			{
				Instance.Repaint();
			}
		}
		
		private void OnEnable()
		{
			prefix = $"JEngine.Editor.Setting.{Application.productName}";
			if (sceneSearchField == null) sceneSearchField = new AutocompleteSearchField();
			sceneSearchField.onInputChanged = s => { sceneSearchPattern = s; };
			sceneSearchField.onConfirm = s => { sceneSearchPattern = s; };
			scrollPos = new Vector2(position.width, position.height);
		}

		private int GetSpace(float percentage)
		{
			int result = (int) (position.width * percentage);
			return result;
		}

		private void OnGUI()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			
			//只滚动y轴
			var _scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			scrollPos = new Vector2(scrollPos.x, _scrollPos.y);

			#region 顶部JEngin相关

			//绘制标题
			GUILayout.Space(10);
			GUIStyle textStyle = new GUIStyle();
			textStyle.normal.textColor = GUI.skin.label.normal.textColor;
			textStyle.fontSize = 24;
			textStyle.alignment = TextAnchor.MiddleCenter;
			GUILayout.Label(GetString(JENGINE_SETTING), textStyle);
			GUILayout.Space(10);

			MakeHorizontal(GetSpace(0.1f), () => { EditorGUILayout.HelpBox(GetString(PANEL_INFO), MessageType.Info); });

			GUILayout.Space(10);

			//选择语言
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				Language = (JEngineLanguage) EditorGUILayout.EnumPopup(GetString(DISPLAY_LANGUAGE), Language);
				titleContent.text = GetString(JENGINE_SETTING);
			});

			//选择场景
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				var sceneObj = EditorGUILayout.ObjectField(GetString(START_UP_SCENE),
					AssetDatabase.LoadAssetAtPath<Object>(StartUpScenePath),
					typeof(Object), false);
				if (sceneObj == null || !AssetDatabase.GetAssetPath(sceneObj).EndsWith(".unity"))
				{
					ShowNotification(new GUIContent(GetString(INVALID_SCENE_OBJECT)), 3);
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

			GUILayout.Space(10);

			//本地路径
			MakeHorizontal(GetSpace(0.1f), () => { EditorGUILayout.LabelField(GetString(LOCAL_JENGINE)); });
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				GUI.enabled = false;
				LocalPath = EditorGUILayout.TextField(LocalPath);
				GUI.enabled = true;
				if (GUILayout.Button(GetString(CHOOSE_BTN), GUILayout.Width(70)))
				{
					var path = EditorUtility.OpenFolderPanel(GetString(LOCAL_JENGINE), LocalPath,
						GetString(LOCAL_JENGINE));
					if (!string.IsNullOrEmpty(path))
					{
						LocalPath = path;
					}
				}
			});
			//热更路径
			MakeHorizontal(GetSpace(0.1f), () => { EditorGUILayout.LabelField(GetString(HOT_JENGINE)); });
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				GUI.enabled = false;
				HotPath = EditorGUILayout.TextField(HotPath);
				GUI.enabled = true;
				if (GUILayout.Button(GetString(CHOOSE_BTN), GUILayout.Width(70)))
				{
					var path = EditorUtility.OpenFolderPanel(GetString(HOT_JENGINE), HotPath,
						GetString(HOT_JENGINE));
					if (!string.IsNullOrEmpty(path))
					{
						HotPath = path;
					}
				}
			});

			GUILayout.Space(10);

			//提示框
			MakeHorizontal(GetSpace(0.1f),
				() => { EditorGUILayout.HelpBox(GetString(UPDATE_HELPBOX), MessageType.Error); });

			//更新按钮
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				GUI.enabled = !Helpers.installing;
				
				if (GUILayout.Button(GetString(UPDATE_JENGINE), GUILayout.Height(30)))
				{
					Helpers.Update();
				}

				GUI.enabled = true;
			});

			#endregion

			#region XAsset相关

			//XAsset升级
			GUILayout.Space(30);
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				textStyle = new GUIStyle();
				textStyle.fontSize = 16;
				textStyle.normal.textColor = PURPLE_COLOR;
				textStyle.alignment = TextAnchor.MiddleCenter;
				GUILayout.Label(GetString(XASSET_TITLE), textStyle);
			});
			GUILayout.Space(10);

			//提示框
			MakeHorizontal(GetSpace(0.1f),
				() => { EditorGUILayout.HelpBox(GetString(XASSET_HELPBOX), MessageType.Warning); });

			//先登入，再出现按钮
			if (!XAssetLoggedIn)
			{
				MakeHorizontal(GetSpace(0.2f), () =>
				{
					EditorGUILayout.LabelField(GetString(XASSET_ACCOUNT));
				});
				MakeHorizontal(GetSpace(0.2f), () =>
				{
					XAssetAccount = EditorGUILayout.TextField(XAssetAccount);
				});
				MakeHorizontal(GetSpace(0.2f), () =>
				{
					EditorGUILayout.LabelField(GetString(XASSET_PASSWORD));
				});
				MakeHorizontal(GetSpace(0.2f), () =>
				{
					XAssetPassword = EditorGUILayout.PasswordField(XAssetPassword);
				});
				
				GUILayout.Space(10);
				
				MakeHorizontal(GetSpace(0.2f), () =>
				{
					if (GUILayout.Button(GetString(SIGN_UP), GUILayout.Height(30)))
					{
						Helpers.SignUpXAsset();
					}
					GUILayout.Space(50);
					
					GUI.enabled = !Helpers.loggingXAsset;
					
					if (GUILayout.Button(GetString(LOGIN), GUILayout.Height(30)))
					{
						_ = Helpers.LoginXAsset(true);
					}

					GUI.enabled = true;
				});
			}
			else
			{
				bool activated = XAssetRemain > 0;
				
				GUILayout.Space(10);
				
				//续费
				MakeHorizontal(GetSpace(0.1f), () =>
				{
					var style = new GUIStyle();
					style.fontSize = GUI.skin.textField.fontSize;
					style.normal.textColor = RED_COLOR;
					style.alignment = TextAnchor.MiddleCenter;
					style.fontStyle = FontStyle.Bold;
					EditorGUILayout.LabelField(
						activated
							? string.Format(GetString(XASSET_REMAIN), XAssetRemain)
							: GetString(XASSET_CHARGE_TXT), style);

					GUILayout.Space(10);

					if (GUILayout.Button(GetString(activated ? RECHARGE : CHARGE), GUILayout.Height(20)))
					{
						Helpers.RechargeXAsset();
					}
				});
				
				GUILayout.Space(10);

				if (activated)
				{
					//更新按钮
					MakeHorizontal(GetSpace(0.1f), () =>
					{
						GUI.enabled = !Helpers.installing;
					
						if (GUILayout.Button(GetString(XASSET_BUTTON), GUILayout.Height(30)))
						{
							Helpers.GetXAssetPro();
						}
					
						GUI.enabled = true;
					});
				}
				
				//退出登入
				MakeHorizontal(GetSpace(0.1f), () =>
				{
					if (GUILayout.Button(GetString(LOG_OUT), GUILayout.Height(30)))
					{
						Helpers.LogOutXAsset();
					}
				});
			}

			#endregion

			#region 热更场景相关
#if !XASSET_PRO
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
			MakeHorizontal(GetSpace(0.1f), () => { MakeFoldOut(ref ShowScenes, "热更场景列表", () => { }); });
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
							EditorSceneManager.CloseScene(SceneManager.GetSceneByPath(asset), true);
						}
					});
				}
			}
#endif
			#endregion

			#region ClassBind相关

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

			#endregion

			#region Bug相关

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
			MakeHorizontal(GetSpace(0.1f),
				() => { EditorGUILayout.HelpBox(GetString(ERROR_RESCUE_TOOLS_INFO), MessageType.Warning); });

			//修复Missing Type
			GUILayout.Space(10);
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				EditorGUILayout.LabelField(GetString(MISSING_ASSEMBLY), GUILayout.MinHeight(50));
				if (GUILayout.Button(GetString(MISSING_ASSEMBLY_BTN), GUILayout.MinHeight(50)))
				{
					PlayerSettings.allowUnsafeCode = false;
				}
			});

			#endregion

			try
			{
				EditorGUILayout.EndScrollView();
			}
			catch
			{
				// ignored
			}
		}


		public static void MakeFoldOut(ref bool fold, string name, Action act)
		{
			fold = EditorGUILayout.Foldout(fold, name);
			if (fold)
			{
				act();
			}
		}

		public static void MakeHorizontal(int space, Action act)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(space);
			act();
			GUILayout.Space(space);
			GUILayout.EndHorizontal();
		}

		public static string GetString(int index)
		{
			return Titles[index][(int) Language];
		}
	}

	public enum JEngineLanguage
	{
		中文 = 0,
		English = 1
	}
}