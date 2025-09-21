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
using System.IO;
using JEngine.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace JEngine.Editor
{
	internal enum SettingString
	{
		JEngineSetting = 0,
		DisplayLanguage = 1,
		StartUpScene = 2,
		LastDLLCleanTime = 3,
		DateFormat = 4,
		JumpToStartUpScene = 5,
		MissingAssembly = 6,
		MissingAssemblyBtn = 7,
		ErrorRescueTools = 8,
		ErrorRescueToolsInfo = 9,
		InvalidSceneObject = 10,
		PanelInfo = 11,
		ScenesTitle = 12,
		LoadSceneBtn = 13,
		LoadSceneAdditiveBtn = 14,
		UnloadSceneBtn = 15,
		ClassBindTools = 16,
		LocalJEngine = 17,
		HotJEngine = 18,
		ChooseBtn = 19,
		UpdateJEngine = 20,
		UpdateHelpBox = 21,
		DLLConvertLog = 22,
		DLLCleanLog = 23,
		DLLNewReferenceLog = 24,
		DeleteErrorLog = 25,
		ClassBindErrorTitle = 26,
		ClassBindErrorContent = 27,
		ClassBindInvalidField = 28,
		ClassBindGetAllField = 29,
		ClassBindGetAllType = 30,
		ClassBindProgress = 31,
		ClassBindProgressContentForGetField = 32,
		ClassBindProgressContentForGetType = 33,
		Success = 34,
		Fail = 35,
		ClassBindResultTitle = 36,
		ClassBindResultContentForGetType = 37,
		Done = 38,
		HotSceneList = 39,
		ClassBindResultContentForSetField = 40,
		ClassBindUnableSetFieldValue = 41,
		MemberVariables = 42,
		ClassBindIgnorePrivate = 43,
		ClassBindIgnoreHideInInspector = 44,
		ClassBindInvalidFieldDeleted = 45,
		ClassBindRearrange = 46,
		ClassBindRearrangeResult = 47,
		ClassBindRearrangeTitle = 48,
		EncryptDLLPassword = 49,
		ShortCuts = 50,
		BuildBundles = 51,
		GenerateClrBind = 52,
		GenerateCrossDomainAdapter = 53,
		OpenJEngineSetting = 54,
		ClassBindInfo = 55,
		Notice = 56,
		NoticeText = 57,
		MismatchDLLKeyTitle = 58,
		MismatchDLLKeyContext = 59,
		Ok = 60,
		Ignore = 61,
		ClassBindGetFromBase = 62,
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
			new[] { "JEngine设置面板", "JEngine Setting Panel" },//JEngineSetting
			new[] { "显示语言", "Display Language" },//DisplayLanguage
			new[] { "启动场景", "Start Up Scene" },//StartUpScene
			new[] { "上次处理热更DLL时间", "Last Hot Update DLL clean up time" },//LastDLLCleanTime
			new[] { "yyyy年MM月dd日 HH:mm:ss", "MM/dd/yyyy HH:mm:ss" }, //日期格式//DateFormat
			new[] { "运行后跳转启动场景", "Jump to start up scene when launch" },//JumpToStartUpScene
			new[]
			{
				"修复Dependencies目录下\nType or namespace name\nof ILRuntime等Not found错误",
				"Fix under Dependencies directory,\nType or namespace name of ILRuntime, etc,\nNot found error"
			},//MissingAssembly
			new[] { "修复", "Fix" },//MissingAssemblyBtn
			new[] { "错误修复工具", "Error Rescue Tools" }, //修复Bug工具的标题//ErrorRescueTools
			new[]
			{
				"一键修复可能会出现的特定错误\n" +
				"注意：修复中会造成一定时间的无反应，请耐心等待",
				"Fix errors that might occurs by one click\n" +
				"Warning: Unity might not be responding while rescuing, just be patient"
			}, //修复Bug工具的提示//ErrorRescueToolsInfo
			new[] { "选择的资源不是场景资源", "Invalid resource type, please choose scene resource" },//InvalidSceneObject
			new[]
			{
				"面板上的大部分设置是针对于Unity编辑器的，对打包后的客户端无效",
				"Most features in the setting panel is editor only, does not effect the built application"
			}, //面板提示//PanelInfo
			new[] { "热更场景快捷操作", "Hot Update Scenes Shortcuts" },//ScenesTitle
			new[] { "打开", "Open" },//LoadSceneBtn
			new[] { "加载", "Load" },//LoadSceneAdditiveBtn
			new[] { "卸载", "Unload" },//UnloadSceneBtn
			new[] { "ClassBind助手", "ClassBind Tools" },//ClassBindTools
			new[] { "本地工程JEngine框架路径", "Local JEngine Source Code" },//LocalJEngine
			new[] { "热更工程JEngine框架路径", "Hot Update JEngine Source Code" },//HotJEngine
			new[] { "选择", "Choose" },//ChooseBtn
			new[] { "更新JEngine", "Update JEngine" },//UpdateJEngine
			new[]
			{
				"更新JEngine后会删除热更工程的JEngine源码，如有修改请做好备份后再进行更新操作",
				"Update JEngine will delete JEngine's source code in hot update solution, " +
				"please make sure you did a back up before upgrading JEngine."
			},//UpdateHelpBox
			new[] { "转换热更DLL耗时{0}ms", "Convert DLL in: {0} ms" },//DLLConvertLog
			new[] { "清理热更工程编译的{0}个文件耗时{1}ms", "Cleaned: {0} files in: {1} ms" },//DLLCleanLog
			new[]
			{
				"发现新的引用DLL`{0}`，请注意，游戏可能需要重新打包，否则热更代码将有可能无法运行",
				"Find new referenced dll `{0}`, note that your hot update code may not be able " +
				"to run without rebuild application"
			},//DLLNewReferenceLog
			new[] { "无法删除{0}，请手动删除", "Unable to delete {0}, please delete it manually" },//DeleteErrorLog
			new[] { "ClassBind错误", "ClassBind Error" },//ClassBindErrorTitle
			new[]
			{
				"'{0}'类在热更工程中不存在", "Class {0} does not exist " +
				                   "in hot update scripts solution!"
			},//ClassBindErrorContent
			new[] { "{0}不存在{1}，已跳过", "{0} does not contain field: {1}, skipped assigning this field" },//ClassBindInvalidField
			new[] { "自动匹配全部fields", "Get all fields for ClassBind" },//ClassBindGetAllField
			new[] { "自动矫正field的type", "Get all types for ClassBind" },//ClassBindGetAllType
			new[] { "ClassBind转换进度", "ClassBind convert progress" },//ClassBindProgress
			new[] { "正在获取{0}的字段：{1}/{2}", "Getting Field for {0} {1}/{2}" },//ClassBindProgressContentForGetField
			new[] { "正在获取{0}的字段类型：{1}/{2}", "Getting Field Type for {0} {1}/{2}" },//ClassBindProgressContentForGetType
			new[] { "成功", "Succeeded" },//Success
			new[] { "失败", "Failed" },//Fail
			new[] { "ClassBind结果", "ClassBind Result" },//ClassBindResultTitle
			new[] { "<{1}>:ClassBind中{0}个fields已自动设置FieldType", "Set {0} fieldTypes into ClassBind: {1}" },//ClassBindResultContentForGetType
			new[] { "完成", "Done" },//Done
			new[] { "热更场景列表", "Hot Update Scene List" },//HotSceneList
			new[] { "<{1}>:ClassBind新增了{0}个fields", "Add {0} fields into ClassBind: {1}" },//ClassBindResultContentForSetField
			new[] { "无法对<{0}>ClassBind上{2}({1})进行自动赋值构造值", "Unable to set value for field {1}:{2} on ClassBind:<{0}>" },//ClassBindUnableSetFieldValue
			new[] { "{0}的成员变量", "{0} variables" },//MemberVariables
			new[] { "不匹配Private成员变量", "Banned getting private fields" },//ClassBindIgnorePrivate
			new[] { "不匹配带有标签\n[HideInInspector]的变量", "Banned getting fields with\n attribute [HideInInspector]" },//ClassBindIgnoreHideInInspector
			new[] { "{0}不存在{1}，已删除该字段", "{0} does not contain field: {1}, deleted this field" },//ClassBindInvalidFieldDeleted
			new[] { "正在重新排序字段", "Automatically rearranging fields" },//ClassBindRearrange
			new[] { "<{1}>:ClassBind中{0}个fields已重新排序", "Rearranged {0} fieldTypes from ClassBind: {1}" },//ClassBindRearrangeResult
			new[] { "重新排序全部fields", "Rearrange all fields for ClassBind" },//ClassBindRearrangeTitle
			new[] { "加密DLL秘钥", "Encrypt dll password" },//EncryptDLLPassword
			new[] { "快捷键", "Shortcuts" },//ShortCuts
			new[] { "生成热更Bundles", "Build Bundles" },//BuildBundles
			new[] { "生成CLR绑定", "Generate CLR Binding" },//GenerateClrBind
			new[] { "生成跨域适配器", "Generate Cross Domain Adapter" },//GenerateCrossDomainAdapter
			new[] { "打开JEngine面板", "Open JEngine Setting" },//OpenJEngineSetting
			new[]
			{
				"\n自动匹配全部fields会匹配对应类型全部没有打[ClassBindIgnore]的字段及属性，同时可以在JEngine面板内选择是否匹配private和打了[HideInInspector]标签的字段及属性，\n" +
				"自动校正field的type只会针对已经在ClassBind面板内添加的field进行其type的矫正，\n" +
				"重新排序全部fields会根据field的名称排序fields，同时会删除不存在的字段及属性（不包含打了HideInInspector或ClassBindIgnore标签的字段及属性）\n",
				"\nGet all fields for ClassBind will match all fields and properties from a type except it has [ClassBindIgnore] attribute, " +
				"and it is optional to ignore private fields and properties, or ignore fields and properties with [HideInInspector] attribute, you need to set it up in JEngine Setting Panel,\n" +
				"Get all types for ClassBind will adjust all types for current fields in the ClassBind Inspector,\n" +
				"Rearrange fields will rearrange all fields by its name, and will remove non-exist fields and properties in the specific type (not including fields and properties with ClassBindIgnore or HideInInspector attribute)\n"
			}, //ClassBindInfo
			new[]
			{
				"温馨提示",
				"Kindly Notice"
			}, //Notice
			new[]
			{
				"[JEngine] 第一次使用请看文档！文档网站：docs.xgamedev.net",
				"[JEngine] First time to use JEngine please read the document first! URL: docs.xgamedev.net"
			}, //NoticeText
			new []
			{
				"DLL解密秘钥异常",
				"DLL decrypt key exception",
			},//mismatchDLLKeyTitle
			new []
			{
				"面板里配置的加密密码是：{0}\n" +
				"游戏场景里配置的解密密码是：{1}\n" +
				"点击确定使用面板配置的密码，点击忽略则继续使用当前密码'{1}'",
				"DLL Encryption password on JEngine Setting during building Assetbundle is: {0}\n" +
				"DLL Encryption password in Updater during runtime is: {1}\n" +
				"Press ok to use the password from JEngine setting, press ignore to continue using the current password '{1}'"
			},//mismatchDLLKeyContext
			new []
			{
				"确定",
				"Ok"
			},//OK
			new []
			{
				"忽略",
				"Ignore"
			},//Ignore
			new[] { "匹配基类成员变量", "Get fields from base type" },//ClassBindGetFromBase
		};

		/// <summary>
		/// 语言
		/// </summary>
		public static JEngineLanguage Language
		{
			get => (JEngineLanguage)(int.Parse(PlayerPrefs.GetString($"{_prefix}.PanelLanguage", "0")));
			set => PlayerPrefs.SetString($"{_prefix}.PanelLanguage", value == JEngineLanguage.中文 ? "0" : "1");
		}

		/// <summary>
		/// 加密秘钥
		/// </summary>
		public static string EncryptPassword
		{
			get => PlayerPrefs.GetString($"{_prefix}.EncryptPassword", "");
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					return;
				}
				PlayerPrefs.SetString($"{_prefix}.EncryptPassword", value);
				SetData.UpdateData(data=>data.EncryptPassword = value);
			}
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
		/// ClassBind获取基类
		/// </summary>
		public static bool ClassBindGetFromBase
		{
			get => PlayerPrefs.GetString($"{_prefix}.ClassBindGetFromBase", "0") == "1";
			private set => PlayerPrefs.SetString($"{_prefix}.ClassBindGetFromBase", value ? "1" : "0");
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
			int result = (int)(position.width * percentage);
			return result;
		}

		public static void SetPrefix(string prefix)
		{
			if (string.IsNullOrEmpty(_prefix))
			{
				_prefix = $"JEngine.Editor.Setting.{Application.productName}.{prefix}.";
			}
		}

		private void OnGUI()
		{
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
				normal = { textColor = GUI.skin.label.normal.textColor },
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
				Language = (JEngineLanguage)EditorGUILayout.EnumPopup(GetString(SettingString.DisplayLanguage),
					Language);
				titleContent.text = GetString(SettingString.JEngineSetting);
			});

			//加密密钥
			MakeHorizontal(GetSpace(0.1f),
				() =>
				{
					EncryptPassword =
						EditorGUILayout.TextField(GetString(SettingString.EncryptDLLPassword), EncryptPassword);
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
				textStyle = new GUIStyle(EditorStyles.textField.name) { alignment = TextAnchor.MiddleCenter };
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
				GUI.enabled = false;

				if (GUILayout.Button(GetString(SettingString.UpdateJEngine) + " - 暂不可用", GUILayout.Height(30)))
				{
					UpdateJEngine.Update();
					GUIUtility.ExitGUI();
				}

				GUI.enabled = true;
			});

			#endregion

			#region 热更场景相关
			
			//直接进热更场景
			GUILayout.Space(30);
			MakeHorizontal(GetSpace(0.1f), () =>
			{
				textStyle = new GUIStyle
				{
					fontSize = 16, normal = { textColor = CyanColor }, alignment = TextAnchor.MiddleCenter
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
				//load all scene files from HotUpdateResources
				var assets = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/HotUpdateResources" });
				foreach (var guid in assets)
				{
					MakeHorizontal(GetSpace(0.1f), () =>
					{
						//get asset from guid
						var asset = AssetDatabase.GUIDToAssetPath(guid);
						GUI.enabled = false;
						EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<SceneAsset>(asset),
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
			//是否包含父类
			MakeHorizontal(GetSpace(0.1f),
				() =>
				{
					EditorGUILayout.LabelField(GetString(SettingString.ClassBindGetFromBase),
						GUILayout.MinHeight(20));
					ClassBindGetFromBase = EditorGUILayout.Toggle(ClassBindGetFromBase, GUILayout.MinHeight(20));
				});
			GUILayout.Space(10);
			//是否跳过private
			MakeHorizontal(GetSpace(0.1f),
				() =>
				{
					EditorGUILayout.LabelField(GetString(SettingString.ClassBindIgnorePrivate),
						GUILayout.MinHeight(20));
					ClassBindIgnorePrivate = EditorGUILayout.Toggle(ClassBindIgnorePrivate, GUILayout.MinHeight(20));
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
				if (GUILayout.Button(GetString(SettingString.ClassBindGetAllField), GUILayout.Height(30)))
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
				if (GUILayout.Button(GetString(SettingString.ClassBindGetAllType), GUILayout.Height(30)))
				{
					foreach (var cb in Tools.FindObjectsOfTypeAll<ClassBind>())
					{
						ClassBindEditor.DoFieldType(cb, false);
					}

					GUIUtility.ExitGUI();
				}
			});

			MakeHorizontal(GetSpace(0.1f), () =>
			{
				if (GUILayout.Button(GetString(SettingString.ClassBindRearrangeTitle), GUILayout.Height(30)))
				{
					foreach (var cb in Tools.FindObjectsOfTypeAll<ClassBind>())
					{
						ClassBindEditor.CleanFields(cb, false);
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
					fontSize = 16, normal = { textColor = RedColor }, alignment = TextAnchor.MiddleCenter
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

			#region 快捷键

			MakeHorizontal(GetSpace(0.1f), () =>
			{
				textStyle = new GUIStyle
				{
					fontSize = 16, normal = { textColor = PurpleColor }, alignment = TextAnchor.MiddleCenter
				};
				GUILayout.Label(GetString(SettingString.ShortCuts), textStyle);
			});
			GUILayout.Space(10);
			textStyle = new GUIStyle(EditorStyles.textField.name) { alignment = TextAnchor.MiddleCenter };
			MakeHorizontal(GetSpace(0.1f),
				() =>
				{
					EditorGUILayout.LabelField(GetString(SettingString.BuildBundles), "Command/Control Shift Alt B",
						textStyle);
				});
			MakeHorizontal(GetSpace(0.1f),
				() => { EditorGUILayout.LabelField(GetString(SettingString.GenerateClrBind), "Shift B", textStyle); });
			MakeHorizontal(GetSpace(0.1f),
				() =>
				{
					EditorGUILayout.LabelField(GetString(SettingString.OpenJEngineSetting), "Alt Shift J", textStyle);
				});
			MakeHorizontal(GetSpace(0.1f),
				() =>
				{
					EditorGUILayout.LabelField(GetString(SettingString.GenerateCrossDomainAdapter),
						"Command/Control Shift G", textStyle);
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

		public static string GetString(SettingString index)
		{
			return Texts[(int)index][(int)Language];
		}
	}

	public enum JEngineLanguage
	{
		中文 = 0,
		English = 1
	}
}