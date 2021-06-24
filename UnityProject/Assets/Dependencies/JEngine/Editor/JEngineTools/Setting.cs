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
	internal enum SettingString
	{
		JEngineSetting,
		DisplayLanguage,
		StartUpScene,
		LastDLLCleanTime,
		DateFormat,
		JumpToStartUpScene,
		MissingAssembly,
		MissingAssemblyBtn,
		ErrorRescueTools,
		ErrorRescueToolsInfo,
		InvalidSceneObject,
		PanelInfo,
		ScenesTitle,
		LoadSceneBtn,
		LoadSceneAdditiveBtn,
		UnloadSceneBtn,
		ClassBindTools,
		LocalJEngine,
		HotJEngine,
		ChooseBtn,
		UpdateJEngine,
		UpdateHelpBox,
		XAssetTitle,
		XAssetHelpBox,
		XAssetButton,
		XAssetAccount,
		XAssetPassword,
		Login,
		SignUp,
		LogOut,
		XAssetRemain,
		Recharge,
		Charge,
		XAssetChargeTxt,
		DLLConvertLog,
		DLLCleanLog,
		DLLNewReferenceLog,
		DeleteErrorLog,
		ClassBindErrorTitle,
		ClassBindErrorContent,
		ClassBindInvalidField,
		ClassBindGetAllField,
		ClassBindGetAllType,
		ClassBindProgress,
		ClassBindProgressContentForGetField,
		ClassBindProgressContentForGetType,
		Success,
		Fail,
		ClassBindResultTitle,
		ClassBindResultContentForGetType,
		Done,
		HotSceneList,
		ClassBindResultContentForSetField,
		ClassBindUnableSetFieldValue,
		MemberVariables,
		ClassBindIgnorePrivate,
		ClassBindIgnoreHideInInspector,
	}

	internal class Setting : EditorWindow
	{
		private static string _prefix;

		private static DirectoryInfo _dataPath;

		private static readonly Color RedColor = new Color32(190, 61, 75, 255);
		private static readonly Color CyanColor = new Color32(75, 226, 235, 255);
		private static readonly Color GreenColor = new Color32(73, 225, 146, 255);
		private static readonly Color PurpleColor = new Color32(141, 107, 225, 255);


		private static readonly string[][] Texts =
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
			new[] {"ClassBind助手", "ClassBind Tools"},
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
				"Please get to read the documentation for XAsset Pro before you start upgrade\n"
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
			new[] {"转换热更DLL耗时{0}ms", "Convert DLL in: {0} ms"},
			new[] {"清理热更工程编译的{0}个文件耗时{1}ms", "Cleaned: {0} files in: {1} ms"},
			new[]
			{
				"发现新的引用DLL`{0}`，请注意，游戏可能需要重新打包，否则热更代码将有可能无法运行",
				"Find new referenced dll `{0}`, note that your hot update code may not be able " +
				"to run without rebuild application"
			},
			new[] {"无法删除{0}，请手动删除", "Unable to delete {0}, please delete it manually"},
			new[] {"ClassBind错误", "ClassBind Error"},
			new[]
			{
				"'{0}'类在热更工程中不存在", "Class {0} does not exist " +
				                   "in hot update scripts solution!"
			},
			new[] {"{0}不存在{1}，已跳过", "{0} does not contain field: {1}, skipped assigning this field"},
			new[] {"自动匹配全部fields", "Get all fields for ClassBind"},
			new[] {"自动矫正field的type", "Get all types for ClassBind"},
			new[] {"ClassBind转换进度", "ClassBind convert progress"},
			new[] {"正在获取{0}的字段：{1}/{2}", "Getting Field for {0} {1}/{2}"},
			new[] {"正在获取{0}的字段类型：{1}/{2}", "Getting Field Type for {0} {1}/{2}"},
			new[] {"成功", "Succeeded"},
			new[] {"失败", "Failed"},
			new[] {"ClassBind结果", "ClassBind Result"},
			new[] {"<{1}>:ClassBind中{0}个fields已自动设置FieldType", "Set {0} fieldTypes into ClassBind: {1}"},
			new[] {"完成", "Done"},
			new[] {"热更场景列表", "Hot Update Scene List"},
			new[] {"<{1}>:ClassBind新增了{0}个fields", "Add {0} fields into ClassBind: {1}"},
			new[] {"无法对<{0}>ClassBind上{2}({1})进行自动赋值构造值", "Unable to set value for field {1}:{2} on ClassBind:<{0}>"},
			new[] {"{0}的成员变量", "{0} variables"},
			new[] {"不匹配Private成员变量", "Banned getting private fields"},
			new[] {"不匹配带有标签\n[HideInInspector]的变量", "Banned getting fields with\n attribute [HideInInspector]"},

		};

		/// <summary>
		/// 语言
		/// </summary>
		private static JEngineLanguage Language
		{
			get => (JEngineLanguage) (int.Parse(PlayerPrefs.GetString($"{_prefix}.PanelLanguage", "0")));
			set => PlayerPrefs.SetString($"{_prefix}.PanelLanguage", value == JEngineLanguage.中文 ? "0" : "1");
		}

		/// <summary>
		/// 跳转场景路径
		/// </summary>
		public static string StartUpScenePath
		{
			get => PlayerPrefs.GetString($"{_prefix}.StartUpScenePath", "Assets/Init.unity");
			private set => PlayerPrefs.SetString($"{_prefix}.StartUpScenePath", value);
		}

		/// <summary>
		/// 启动后是否跳转场景
		/// </summary>
		public static bool JumpStartUp
		{
			get => PlayerPrefs.GetString($"{_prefix}.JumpStartUpScene", "1") == "1";
			private set => PlayerPrefs.SetString($"{_prefix}.JumpStartUpScene", value ? "1" : "0");
		}

		/// <summary>
		/// 上次处理热更DLL时间
		/// </summary>
		public static string LastDLLCleanUpTime
		{
			get => PlayerPrefs.GetString($"{_prefix}.LastDLLCleanUpTime");
			set => PlayerPrefs.SetString($"{_prefix}.LastDLLCleanUpTime", value);
		}

		/// <summary>
		/// 本地框架路径
		/// </summary>
		public static string LocalPath
		{
			get => PlayerPrefs.GetString($"{_prefix}.LocalPath",
				_dataPath.FullName + "/Dependencies/JEngine");
			private set => PlayerPrefs.SetString($"{_prefix}.LocalPath", value);
		}

		/// <summary>
		/// 热更框架路径
		/// </summary>
		public static string HotPath
		{
			get => PlayerPrefs.GetString($"{_prefix}.HotPath",
				_dataPath.Parent?.FullName + "/HotUpdateScripts/JEngine");
			private set => PlayerPrefs.SetString($"{_prefix}.HotPath", value);
		}

		/// <summary>
		/// 是否已登入XAsset
		/// </summary>
		public static bool XAssetLoggedIn
		{
			get => PlayerPrefs.GetString($"{_prefix}.XAssetLoggedIn", "0") == "1";
			set => PlayerPrefs.SetString($"{_prefix}.XAssetLoggedIn", value ? "1" : "0");
		}

		/// <summary>
		/// XAsset账号
		/// </summary>
		public static string XAssetAccount
		{
			get => PlayerPrefs.GetString($"{_prefix}.XAssetAccount", "");
			private set => PlayerPrefs.SetString($"{_prefix}.XAssetAccount", value);
		}


		/// <summary>
		/// XAsset密码
		/// </summary>
		public static string XAssetPassword
		{
			get => PlayerPrefs.GetString($"{_prefix}.XAssetPassword", "");
			private set => PlayerPrefs.SetString($"{_prefix}.XAssetPassword", value);
		}

		/// <summary>
		/// XAsset剩余时间
		/// </summary>
		public static int XAssetRemainTime
		{
			get => PlayerPrefs.GetInt($"{_prefix}.XAssetRemain", 0);
			set => PlayerPrefs.SetInt($"{_prefix}.XAssetRemain", value);
		}
		
		/// <summary>
		/// ClassBind不获取private
		/// </summary>
		public static bool ClassBindIgnorePrivate
		{
			get => PlayerPrefs.GetString($"{_prefix}.ClassBindIgnorePrivate", "0") == "1";
			private set => PlayerPrefs.SetString($"{_prefix}.ClassBindIgnorePrivate", value ? "1" : "0");
		}
		
		/// <summary>
		/// ClassBind不获取HideInInspector
		/// </summary>
		public static bool ClassBindIgnoreHideInInspector
		{
			get => PlayerPrefs.GetString($"{_prefix}.ClassBindIgnoreHideInInspector", "0") == "1";
			private set => PlayerPrefs.SetString($"{_prefix}.ClassBindIgnoreHideInInspector", value ? "1" : "0");
		}


		/// <summary>
		/// 是否展示热更场景
		/// </summary>
		private bool _showScenes = true;

		private Vector2 _scrollPos;

		private static Setting _instance;

		[MenuItem("JEngine/Setting #&J", priority = 1998)]
		private static void ShowWindow()
		{
			var window = GetWindow<Setting>(GetString(SettingString.JEngineSetting));
			window.minSize = new Vector2(300, 500);
			window.Show();
			_instance = window;
		}

		public static void Refresh()
		{
			if (_instance != null)
			{
				_instance.Repaint();
			}
		}

		private void OnEnable()
		{
			_dataPath = new DirectoryInfo(Application.dataPath);
			_scrollPos = new Vector2(position.width, position.height);
		}

		private int GetSpace(float percentage)
		{
			int result = (int) (position.width * percentage);
			return result;
		}

		public static void SetPrefix()
		{
			if (string.IsNullOrEmpty(_prefix))
			{
				_prefix = $"JEngine.Editor.Setting.{Application.productName}";
			}
		}

		private void OnGUI()
		{
			SetPrefix();
			
			if (_instance == null)
			{
				_instance = this;
			}

			//只滚动y轴
			var scrollPos = EditorGUILayout.BeginScrollView(this._scrollPos);
			_scrollPos = new Vector2(this._scrollPos.x, scrollPos.y);

			#region 顶部JEngin相关

			//绘制标题
			GUILayout.Space(10);
			GUIStyle textStyle = new GUIStyle
			{
				normal = {textColor = GUI.skin.label.normal.textColor},
				fontSize = 24,
				alignment = TextAnchor.MiddleCenter
			};
			GUILayout.Label(GetString(SettingString.JEngineSetting), textStyle);
			GUILayout.Space(10);

			MakeHorizontal(GetSpace(0.1f),
				() => { EditorGUILayout.HelpBox(GetString(SettingString.PanelInfo), MessageType.Info); });

			GUILayout.Space(10);

			//选择语言
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				Language = (JEngineLanguage) EditorGUILayout.EnumPopup(GetString(SettingString.DisplayLanguage),
					Language);
				titleContent.text = GetString(SettingString.JEngineSetting);
			});

			//选择场景
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				var sceneObj = EditorGUILayout.ObjectField(GetString(SettingString.StartUpScene),
					AssetDatabase.LoadAssetAtPath<Object>(StartUpScenePath),
					typeof(Object), false);
				if (sceneObj == null || !AssetDatabase.GetAssetPath(sceneObj).EndsWith(".unity"))
				{
					ShowNotification(new GUIContent(GetString(SettingString.InvalidSceneObject)), 3);
				}
				else
				{
					StartUpScenePath = AssetDatabase.GetAssetPath(sceneObj);
				}
			});

			//是否跳转
			MakeHorizontal(GetSpace(0.1f),
				() =>
				{
					JumpStartUp = EditorGUILayout.Toggle(GetString(SettingString.JumpToStartUpScene), JumpStartUp);
				});

			//上次处理热更DLL时间
			MakeHorizontal(GetSpace(0.1f),
				() => { EditorGUILayout.LabelField(GetString(SettingString.LastDLLCleanTime)); });
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				GUI.enabled = false;
				textStyle = new GUIStyle(EditorStyles.textField.name) {alignment = TextAnchor.MiddleCenter};
				EditorGUILayout.TextField(LastDLLCleanUpTime, textStyle);
				GUI.enabled = true;
			});

			GUILayout.Space(10);

			//本地路径
			MakeHorizontal(GetSpace(0.1f),
				() => { EditorGUILayout.LabelField(GetString(SettingString.LocalJEngine)); });
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				GUI.enabled = false;
				LocalPath = EditorGUILayout.TextField(LocalPath);
				GUI.enabled = true;
				if (GUILayout.Button(GetString(SettingString.ChooseBtn), GUILayout.Width(70)))
				{
					var path = EditorUtility.OpenFolderPanel(GetString(SettingString.LocalJEngine), LocalPath,
						GetString(SettingString.LocalJEngine));
					if (!string.IsNullOrEmpty(path))
					{
						LocalPath = path;
					}

					GUIUtility.ExitGUI();
				}
			});
			//热更路径
			MakeHorizontal(GetSpace(0.1f), () => { EditorGUILayout.LabelField(GetString(SettingString.HotJEngine)); });
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				GUI.enabled = false;
				HotPath = EditorGUILayout.TextField(HotPath);
				GUI.enabled = true;
				if (GUILayout.Button(GetString(SettingString.ChooseBtn), GUILayout.Width(70)))
				{
					var path = EditorUtility.OpenFolderPanel(GetString(SettingString.HotJEngine), HotPath,
						GetString(SettingString.HotJEngine));
					if (!string.IsNullOrEmpty(path))
					{
						HotPath = path;
					}

					GUIUtility.ExitGUI();
				}
			});

			GUILayout.Space(10);

			//提示框
			MakeHorizontal(GetSpace(0.1f),
				() => { EditorGUILayout.HelpBox(GetString(SettingString.UpdateHelpBox), MessageType.Error); });

			//更新按钮
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				GUI.enabled = !XAssetHelper.installing;

				if (GUILayout.Button(GetString(SettingString.UpdateJEngine), GUILayout.Height(30)))
				{
					XAssetHelper.Update();
					GUIUtility.ExitGUI();
				}

				GUI.enabled = true;
			});

			#endregion

			// #region XAsset相关
			//
			// //XAsset升级
			// GUILayout.Space(30);
			// MakeHorizontal(GetSpace(0.1f), () =>
			// {
			// 	textStyle = new GUIStyle
			// 	{
			// 		fontSize = 16, normal = {textColor = PurpleColor}, alignment = TextAnchor.MiddleCenter
			// 	};
			// 	GUILayout.Label(GetString(SettingString.XAssetTitle), textStyle);
			// });
			// GUILayout.Space(10);
			//
			// //提示框
			// MakeHorizontal(GetSpace(0.1f),
			// 	() => { EditorGUILayout.HelpBox(GetString(SettingString.XAssetHelpBox), MessageType.Warning); });
			//
			// //先登入，再出现按钮
			// if (!XAssetLoggedIn)
			// {
			// 	MakeHorizontal(GetSpace(0.2f),
			// 		() => { EditorGUILayout.LabelField(GetString(SettingString.XAssetAccount)); });
			// 	MakeHorizontal(GetSpace(0.2f), () => { XAssetAccount = EditorGUILayout.TextField(XAssetAccount); });
			// 	MakeHorizontal(GetSpace(0.2f),
			// 		() => { EditorGUILayout.LabelField(GetString(SettingString.XAssetPassword)); });
			// 	MakeHorizontal(GetSpace(0.2f),
			// 		() => { XAssetPassword = EditorGUILayout.PasswordField(XAssetPassword); });
			//
			// 	GUILayout.Space(10);
			//
			// 	MakeHorizontal(GetSpace(0.2f), () =>
			// 	{
			// 		if (GUILayout.Button(GetString(SettingString.SignUp), GUILayout.Height(30)))
			// 		{
			// 			XAssetHelper.SignUpXAsset();
			// 			GUIUtility.ExitGUI();
			// 		}
			//
			// 		GUILayout.Space(50);
			//
			// 		GUI.enabled = !XAssetHelper.loggingXAsset;
			//
			// 		if (GUILayout.Button(GetString(SettingString.Login), GUILayout.Height(30)))
			// 		{
			// 			_ = XAssetHelper.LoginXAsset(true);
			// 			GUIUtility.ExitGUI();
			// 		}
			//
			// 		GUI.enabled = true;
			// 	});
			// }
			// else
			// {
			// 	bool activated = XAssetRemainTime > 0;
			//
			// 	GUILayout.Space(10);
			//
			// 	//续费
			// 	MakeHorizontal(GetSpace(0.1f), () =>
			// 	{
			// 		var style = new GUIStyle
			// 		{
			// 			fontSize = GUI.skin.textField.fontSize,
			// 			normal = {textColor = RedColor},
			// 			alignment = TextAnchor.MiddleCenter,
			// 			fontStyle = FontStyle.Bold
			// 		};
			// 		EditorGUILayout.LabelField(
			// 			activated
			// 				? string.Format(GetString(SettingString.XAssetRemain), XAssetRemainTime)
			// 				: GetString(SettingString.XAssetChargeTxt), style);
			//
			// 		GUILayout.Space(10);
			//
			// 		if (GUILayout.Button(GetString(activated ? SettingString.Recharge : SettingString.Charge),
			// 			GUILayout.Height(20)))
			// 		{
			// 			XAssetHelper.RechargeXAsset();
			// 		}
			// 	});
			//
			// 	GUILayout.Space(10);
			//
			// 	if (activated)
			// 	{
			// 		//更新按钮
			// 		MakeHorizontal(GetSpace(0.1f), () =>
			// 		{
			// 			GUI.enabled = !XAssetHelper.installing;
			//
			// 			if (GUILayout.Button(GetString(SettingString.XAssetButton), GUILayout.Height(30)))
			// 			{
			// 				XAssetHelper.GetXAssetPro();
			// 				GUIUtility.ExitGUI();
			// 			}
			//
			// 			GUI.enabled = true;
			// 		});
			// 	}
			//
			// 	//退出登入
			// 	MakeHorizontal(GetSpace(0.1f), () =>
			// 	{
			// 		if (GUILayout.Button(GetString(SettingString.LogOut), GUILayout.Height(30)))
			// 		{
			// 			XAssetHelper.LogOutXAsset();
			// 			GUIUtility.ExitGUI();
			// 		}
			// 	});
			// }
			//
			// #endregion

			#region 热更场景相关

#if !XASSET_PRO
			//直接进热更场景
			GUILayout.Space(30);
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				textStyle = new GUIStyle
				{
					fontSize = 16, normal = {textColor = CyanColor}, alignment = TextAnchor.MiddleCenter
				};
				GUILayout.Label(GetString(SettingString.ScenesTitle), textStyle);
			});
			GUILayout.Space(10);
			MakeHorizontal(GetSpace(0.1f),
				() => { MakeFoldOut(ref _showScenes, GetString(SettingString.HotSceneList), () => { }); });
			//如果场景
			if (_showScenes)
			{
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

						EditorGUILayout.ObjectField(sceneObj,
							typeof(Object), false);
						GUI.enabled = true;

						GUILayout.Space(15);

						if (GUILayout.Button(GetString(SettingString.LoadSceneBtn)))
						{
							EditorSceneManager.OpenScene(asset);
							GUIUtility.ExitGUI();
						}

						GUILayout.Space(5);

						if (GUILayout.Button(GetString(SettingString.LoadSceneAdditiveBtn)))
						{
							EditorSceneManager.OpenScene(asset, OpenSceneMode.Additive);
							GUIUtility.ExitGUI();
						}

						GUILayout.Space(5);

						if (GUILayout.Button(GetString(SettingString.UnloadSceneBtn)))
						{
							EditorSceneManager.CloseScene(SceneManager.GetSceneByPath(asset), true);
							GUIUtility.ExitGUI();
						}
					});
				}
			}
#endif

			#endregion

			#region ClassBind相关

			//ClassBind工具
			GUILayout.Space(30);
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				textStyle = new GUIStyle
				{
					fontSize = 16, normal = {textColor = GreenColor}, alignment = TextAnchor.MiddleCenter
				};
				GUILayout.Label(GetString(SettingString.ClassBindTools), textStyle);
			});
			GUILayout.Space(10);
			//是否跳过private
			MakeHorizontal(GetSpace(0.1f),
				() =>
				{
					EditorGUILayout.LabelField(GetString(SettingString.ClassBindIgnorePrivate), GUILayout.MinHeight(20));
					ClassBindIgnorePrivate = EditorGUILayout.Toggle(ClassBindIgnorePrivate,GUILayout.MinHeight(20));
				});
			//是否跳过标签
			MakeHorizontal(GetSpace(0.1f),
				() =>
				{
					EditorGUILayout.LabelField(GetString(SettingString.ClassBindIgnoreHideInInspector),
						GUILayout.MinHeight(30));
					ClassBindIgnoreHideInInspector =
						EditorGUILayout.Toggle(ClassBindIgnoreHideInInspector, GUILayout.MinHeight(30));
				});
			GUILayout.Space(10);
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				if (GUILayout.Button(GetString(SettingString.ClassBindGetAllField),GUILayout.Height(30)))
				{
					foreach (var cb in Tools.FindObjectsOfTypeAll<ClassBind>())
					{
						ClassBindEditor.DoConvert(cb, false);
					}

					GUIUtility.ExitGUI();
				}
			});

			MakeHorizontal(GetSpace(0.1f), () =>
			{
				if (GUILayout.Button(GetString(SettingString.ClassBindGetAllType),GUILayout.Height(30)))
				{
					foreach (var cb in Tools.FindObjectsOfTypeAll<ClassBind>())
					{
						ClassBindEditor.DoFieldType(cb, false);
					}

					GUIUtility.ExitGUI();
				}
			});

			#endregion

			#region Bug相关

			//bug修复
			GUILayout.Space(30);
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				textStyle = new GUIStyle
				{
					fontSize = 16, normal = {textColor = RedColor}, alignment = TextAnchor.MiddleCenter
				};
				GUILayout.Label(GetString(SettingString.ErrorRescueTools), textStyle);
			});
			GUILayout.Space(10);
			MakeHorizontal(GetSpace(0.1f),
				() => { EditorGUILayout.HelpBox(GetString(SettingString.ErrorRescueToolsInfo), MessageType.Warning); });

			//修复Missing Type
			GUILayout.Space(10);
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				EditorGUILayout.LabelField(GetString(SettingString.MissingAssembly), GUILayout.MinHeight(50));
				if (GUILayout.Button(GetString(SettingString.MissingAssemblyBtn), GUILayout.MinHeight(50)))
				{
					PlayerSettings.allowUnsafeCode = false;
				}
			});
			GUILayout.Space(10);
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

		public static string GetString(SettingString index)
		{
			return Texts[(int) index][(int) Language];
		}
	}

	public enum JEngineLanguage
	{
		中文 = 0,
		English = 1
	}
}