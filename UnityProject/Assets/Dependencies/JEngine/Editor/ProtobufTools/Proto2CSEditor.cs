//
// Proto2CSEditor.cs
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

using System.IO;
using Google.Protobuf.Reflection;
using JEngine.Core;
using ProtoBuf.Reflection;
using UnityEditor;
using UnityEngine;

namespace JEngine.Editor
{
	internal class Proto2CSEditor : EditorWindow
	{
		private static Proto2CSEditor win;

		[MenuItem("JEngine/Protobuf/Proto2CS/Generate")]
		public static void BuildAssetBundle()
		{
			win = GetWindow<Proto2CSEditor>("Proto2CS Generator");
			win.folder = EditorUtility.OpenFolderPanel("Select proto files directory 请选择proto文件路径",
				Application.dataPath, "");
			win.minSize = new Vector2(500, 500);
			win.Show();
		}


		[MenuItem("JEngine/Protobuf/Proto2CS/View Files")]
		private static void ViewDataPath()
		{
			if (!Directory.Exists(Application.dataPath + "/../HotUpdateScripts/Proto2cs"))
			{
				Directory.CreateDirectory(Application.dataPath + "/../HotUpdateScripts/Proto2cs");
			}

			EditorUtility.OpenWithDefaultApp(Application.dataPath + "/../HotUpdateScripts/Proto2cs");
		}

		protected string folder;

		[SerializeField] protected string[] _fileList = new string[0];

		protected SerializedObject _serializedObject;

		protected SerializedProperty _fileListProperty;


		protected void OnEnable()
		{
			//使用当前类初始化
			_serializedObject = new SerializedObject(this);
			//获取当前类中可序列话的属性
			_fileListProperty = _serializedObject.FindProperty("_fileList");
		}

		protected void OnGUI()
		{
			//绘制标题
			GUILayout.Space(10);
			GUIStyle textStyle = new GUIStyle();
			textStyle.fontSize = 24;
			textStyle.normal.textColor = Color.white;
			textStyle.alignment = TextAnchor.MiddleCenter;
			GUILayout.Label("Proto文件转CS文件", textStyle);
			textStyle.fontSize = 18;
			GUILayout.Label("Proto file to CS file", textStyle);
			GUILayout.Space(10);

			/*
			 * 路径
			 */
			GUILayout.Label("Proto file folder Proto文件路径");
			GUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(true);
			folder = EditorGUILayout.TextField(folder);
			EditorGUI.EndDisabledGroup();

			GUILayout.Space(10);
			if (GUILayout.Button("Select Path 选择路径", GUILayout.ExpandWidth(false)))
			{
				folder = EditorUtility.OpenFolderPanel("Select proto files destination 请选择proto文件路径",
					Application.dataPath, "");
			}

			GUILayout.EndHorizontal();

			/*
			 * 文件
			 */
			GUILayout.Space(10);
			GUILayout.Label("Files to convert 需转换文件");
			//更新
			_serializedObject.Update();
			//开始检查是否有修改
			EditorGUI.BeginChangeCheck();
			//显示属性
			EditorGUILayout.PropertyField(_fileListProperty, true);

			//结束检查是否有修改
			if (EditorGUI.EndChangeCheck())
			{
				//提交修改
				_serializedObject.ApplyModifiedProperties();
			}

			/*
			 * 按钮
			 */
			GUILayout.Space(50);
			if (GUILayout.Button("Match all files from folder 从文件夹中匹配全部文件"))
			{
				_fileList = Directory.GetFiles(folder,
					"*.proto", SearchOption.AllDirectories);
				var _fileListInstance = new string[_fileList.Length];
				for (int i = 0; i < _fileList.Length; i++)
					_fileListInstance[i] = Path.GetFileName(_fileList[i]);
				_fileList = _fileListInstance;
				_serializedObject.Update();
			}

			GUILayout.Space(10);
			if (GUILayout.Button("Generate 生成"))
			{
				Generate(folder, _fileList, Application.dataPath + "/../HotUpdateScripts/Proto2cs");
			}
		}

		private static void Generate(string inpath, string[] inprotos, string outpath)
		{
			if (!Directory.Exists(outpath))
			{
				Directory.CreateDirectory(outpath);
			}
			
			var set = new FileDescriptorSet();

			set.AddImportPath(inpath);
			foreach (var inproto in inprotos)
			{
				var s = inproto;
				if (!inproto.Contains(".proto"))
				{
					s += ".proto";
				}

				set.Add(s, true);
			}

			set.Process();
			var errors = set.GetErrors();
			CSharpCodeGenerator.ClearTypeNames();
			var files = CSharpCodeGenerator.Default.Generate(set);

			foreach (var file in files)
			{
				CSharpCodeGenerator.ClearTypeNames();
				var path = Path.Combine(outpath, file.Name);
				File.WriteAllText(path, file.Text);
				
				Log.Print($"Generated cs file for {file.Name.Replace(".cs",".proto")} successfully to: {path}");
			}

			EditorUtility.DisplayDialog("Complete",
				"Proto文件已转CS，详细请看控制台输出" +
				"\n" +
				"Proto files has been convert into CS files, please go to console and view details",
				"Close window");
			win.Close();
		}
	}

}