//
// CS2ProtoEditor.cs
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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using JEngine.Core;
using ProtoBuf;
using UnityEditor;
using UnityEngine;

namespace JEngine.Editor
{
	internal class CS2ProtoEditor : EditorWindow
	{
		private static CS2ProtoEditor win;

		public string _class;
		
		[MenuItem("JEngine/Protobuf/CS2Proto/Generate")]
		public static void BuildAssetBundle()
		{
			win = GetWindow<CS2ProtoEditor>("CS2Proto Generator");
			win.minSize = new Vector2(300, 250);
			win.Show();
		}


		[MenuItem("JEngine/Protobuf/CS2Proto/View Files")]
		private static void ViewDataPath()
		{
			if (!Directory.Exists(OUTPUT_PATH))
			{
				Directory.CreateDirectory(OUTPUT_PATH);
			}
			
			EditorUtility.OpenWithDefaultApp(OUTPUT_PATH);
		}
		protected void OnGUI()
		{
			//绘制标题
			GUILayout.Space(10);
			GUIStyle textStyle = new GUIStyle();
			textStyle.fontSize = 24;
			textStyle.normal.textColor = Color.white;
			textStyle.alignment = TextAnchor.MiddleCenter;
			GUILayout.Label("CS类转Proto协议", textStyle);
			textStyle.fontSize = 18;
			GUILayout.Label("CS class to Proto file", textStyle);
			GUILayout.Space(10);
			
			//类名
			GUILayout.Space(10);
			EditorGUILayout.LabelField("Class name (Contains namespace) 类名（包含命名空间）");
			GUILayout.Space(25);
			_class = EditorGUILayout.TextField(_class);

			//按钮
			GUILayout.Space(30);
			if (GUILayout.Button("Generate 生成"))
			{
				Generate();
			}
		}

		private const string OUTPUT_PATH =  "Assets/../CS2Proto";
		private static void Generate()
		{
			if (!Directory.Exists(OUTPUT_PATH))
			{
				Directory.CreateDirectory(OUTPUT_PATH);
			}

			var _class = win._class;
			
			//先热更里找
			Type t = Assembly
				.LoadFile("Assets/HotUpdateResources/Dll/Hidden~/HotUpdateScripts.dll").GetType(_class);

			if (t == null)
			{
				//本地找
				t = Assembly
					.LoadFile(new DirectoryInfo(Application.dataPath).Parent?.FullName +
					          "/Library/ScriptAssemblies/Assembly-CSharp.dll").GetType(_class);
			}

			//都没
			if (t == null)
			{
				EditorUtility.DisplayDialog("Error", $"Invalid Class {_class}!\r\n{_class}类不存在！", "Ok");
				return;
			}

			var mi = typeof(Serializer).GetMethod("GetProto",new Type[] {})?.MakeGenericMethod(t);
			string proto = (string)mi?.Invoke(null,null);
			
			//如果有先删除
			if (File.Exists($"{OUTPUT_PATH}/{_class.Replace('.','_')}.proto"))
			{
				File.Delete($"{OUTPUT_PATH}/{_class.Replace('.','_')}.proto");
			}

			//生成
			FileStream stream = new FileStream($"{OUTPUT_PATH}/{_class.Replace('.','_')}.proto", FileMode.Append, FileAccess.Write);
			StreamWriter sw = new StreamWriter(stream);
			Stopwatch watch = new Stopwatch();
			sw.WriteLine(proto);
			watch.Stop();
			Log.Print($"Generated proto file for {_class} successfully to: {OUTPUT_PATH}/{_class.Replace('.','_')}.proto in {watch.ElapsedMilliseconds} ms");
			sw.Dispose();
			
			EditorUtility.DisplayDialog("Complete",
				"CS类已转Proto文件，详细请看控制台输出" +
				"\n" +
				" CS class has been convert into proto files, please go to console and view details",
				"Close window");
			win.Close();
		}
	}

}