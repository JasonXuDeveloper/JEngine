//
// ILRuntimeCrossBinding.cs
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Enviorment;
using JEngine.Core;
using UnityEditor;
using UnityEngine;

namespace JEngine.Editor
{
    internal class ILRuntimeCrossBindingAdapterGenerator : EditorWindow
    {
        private static ILRuntimeCrossBindingAdapterGenerator window;
        private const string OUTPUT_PATH = "Assets/Scripts/Adapters";

        [MenuItem("JEngine/ILRuntime/Generate/Cross bind Adapter %#G", priority = 1001)]
        public static void ShowWindow()
        {
            window = GetWindow<ILRuntimeCrossBindingAdapterGenerator>();
            window.titleContent = new GUIContent("Generate Cross bind Adapter");
            window.minSize = new Vector2(300, 150);
            window.Show();
        }

        private string _assembly = "Assembly-CSharp";
        private string _class;
        private string _namespace = "ProjectAdapter";

        private void OnGUI()
        {
            //绘制标题
            GUILayout.Space(10);
            GUI.skin.label.fontSize = 24;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("ILRuntime适配器生成");
            GUI.skin.label.fontSize = 18;
            GUILayout.Label("ILRuntime Adapter Generator");

            //介绍
            EditorGUILayout.HelpBox("本地工程类（没生成asmdef的），Assembly一栏不需要改，Class name一栏写类名（有命名空间带上）；\n" +
                                    "有生成asmdef的工程类，Assembly一栏写asmdef里写的名字，Class name一栏写类名（有命名空间带上）；\n" +
                                    "最后的Namespace是生成的适配器的命名空间，随便写，只要在适配器Helper引用激活即可\n\n" +
                                    "如果要生成Unity类的适配器，请确定找到了对应的module，并添加进热更工程，HotUpdateScripts/Dlls文件夹内，不然无法获取",
                MessageType.Info);

            //程序集
            GUILayout.Space(50);
            Setting.MakeHorizontal(25, () => { EditorGUILayout.LabelField("Assembly 类的程序集"); });
            Setting.MakeHorizontal(25, () => { _assembly = EditorGUILayout.TextField("", _assembly); });

            //类名
            GUILayout.Space(10);
            Setting.MakeHorizontal(25, () => { _class = EditorGUILayout.TextField("Class name 类名", _class); });

            //命名空间
            GUILayout.Space(10);
            Setting.MakeHorizontal(25,
                () => { EditorGUILayout.LabelField("Namespace for generated adapter 生成适配器的命名空间"); });
            Setting.MakeHorizontal(25, () => { _namespace = EditorGUILayout.TextField("", _namespace); });

            //生成
            GUILayout.Space(10);
            Setting.MakeHorizontal(25, () =>
            {
                if (GUILayout.Button("Generate 生成"))
                {
                    GenAdapter();
                }
            });
        }

        private void GenAdapter()
        {
            //获取主工程DLL的类
            Type t = null;
            if (_class.Contains("UnityEngine") || _assembly.Contains("Unity"))
            {
                t = Type.GetType($"{_class},UnityEngine");
            }
            else
            {
                t = Type.GetType(_class);
            }

            if (t == null)
            {
                if (File.Exists(new DirectoryInfo(Application.dataPath).Parent?.FullName +
                                $"/Library/ScriptAssemblies/{_assembly}.dll"))
                {
                    t = Assembly
                        .LoadFile(new DirectoryInfo(Application.dataPath).Parent?.FullName +
                                  $"/Library/ScriptAssemblies/{_assembly}.dll").GetType(_class);

                    if (t == null)
                    {

                        if (File.Exists(new DirectoryInfo(Application.dataPath).Parent?.FullName +
                                        $"/HotUpdateScripts/Dlls/{_assembly}.dll"))
                        {
                            t = Assembly
                                .LoadFile(new DirectoryInfo(Application.dataPath).Parent?.FullName +
                                          $"/HotUpdateScripts/Dlls/{_assembly}.dll").GetType(_class);
                        }
                    }
                }
            }

            //判断空
            if (t == null)
            {
                EditorUtility.DisplayDialog("Error", $"Invalid Class {_class}!\r\n{_class}类不存在！", "Ok");
                return;
            }

            _class = t.FullName.Replace(".", "_");

            if (!Directory.Exists(OUTPUT_PATH))
            {
                Directory.CreateDirectory(OUTPUT_PATH);
            }

            //如果有先删除
            if (File.Exists($"{OUTPUT_PATH}/{_class}Adapter.cs"))
            {
                File.Delete($"{OUTPUT_PATH}/{_class}Adapter.cs");
                if (File.Exists($"{OUTPUT_PATH}/{_class}Adapter.cs.meta"))
                {
                    File.Delete($"{OUTPUT_PATH}/{_class}Adapter.cs.meta");
                }

                AssetDatabase.Refresh();
            }
            
            if (!Directory.Exists(OUTPUT_PATH + "/MonoMethods"))
            {
                Directory.CreateDirectory(OUTPUT_PATH + "/MonoMethods");
            }
            if (File.Exists($"{OUTPUT_PATH}/MonoMethods/{_class}Adapter.MonoMethods.cs"))
            {
                File.Delete($"{OUTPUT_PATH}/MonoMethods/{_class}Adapter.MonoMethods.cs");
                if (File.Exists($"{OUTPUT_PATH}/MonoMethods/{_class}Adapter.MonoMethods.cs.meta"))
                {
                    File.Delete($"{OUTPUT_PATH}/MonoMethods/{_class}Adapter.MonoMethods.cs.meta");
                }

                AssetDatabase.Refresh();
            }

            if (!Directory.Exists(OUTPUT_PATH + "/Editor"))
            {
                Directory.CreateDirectory(OUTPUT_PATH + "/Editor");
            }

            //如果有先删除
            if (File.Exists($"{OUTPUT_PATH}/Editor/{_class}AdapterEditor.cs"))
            {
                File.Delete($"{OUTPUT_PATH}/Editor/{_class}AdapterEditor.cs");
                if (File.Exists($"{OUTPUT_PATH}/Editor/{_class}AdapterEditor.cs.meta"))
                {
                    File.Delete($"{OUTPUT_PATH}/Editor/{_class}AdapterEditor.cs.meta");
                }

                AssetDatabase.Refresh();
            }


            //生成适配器
            FileStream stream = new FileStream($"{OUTPUT_PATH}/{_class}Adapter.cs", FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(stream);
            Stopwatch watch = new Stopwatch();
            sw.WriteLine(
                CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(t,
                    _namespace));
            watch.Stop();
            Log.Print($"Generated {OUTPUT_PATH}/{_class}Adapter.cs in: " +
                      watch.ElapsedMilliseconds + " ms.");
            sw.Dispose();
            
            stream = new FileStream($"{OUTPUT_PATH}/MonoMethods/{_class}Adapter.MonoMethods.cs", FileMode.Append, FileAccess.Write);
            sw = new StreamWriter(stream);
            watch = new Stopwatch();
            sw.WriteLine(
                GenerateCrossBindingAdapterMonoMethods(t,
                    _namespace));
            watch.Stop();
            Log.Print($"Generated {OUTPUT_PATH}/MonoMethods/{_class}Adapter.MonoMethods.cs in: " +
                      watch.ElapsedMilliseconds + " ms.");
            sw.Dispose();

            //生成编辑器
            string editorText = GenerateCrossBindingAdapterEditorCode(t,_namespace);
            if (editorText != null)
            {
                stream = new FileStream($"{OUTPUT_PATH}/Editor/{_class}AdapterEditor.cs", FileMode.Append,
                    FileAccess.Write);
                sw = new StreamWriter(stream);
                watch = new Stopwatch();
                sw.WriteLine(editorText);
                watch.Stop();
                Log.Print($"Generated {OUTPUT_PATH}/Editor/{_class}AdapterEditor.cs in: " +
                          watch.ElapsedMilliseconds + " ms.");
                sw.Dispose();
            }

            window.Close();

            AssetDatabase.Refresh();
        }

        private static bool IsMono(Type baseType)
        {
            return baseType == typeof(MonoBehaviour) || baseType.IsSubclassOf(typeof(MonoBehaviour));
        }

        private static string GenerateCrossBindingAdapterMonoMethods(Type baseType, string nameSpace)
        {
            if (!IsMono(baseType))
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();

            List<MethodInfo> methods = baseType
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).ToList();
            List<MethodInfo> virtMethods = new List<MethodInfo>();
            foreach (var i in methods)
            {
                if (i.IsVirtual || i.IsAbstract || baseType.IsInterface)
                    virtMethods.Add(i);
            }
            string clsName, realClsName;
            bool isByRef;
            baseType.GetClassName(out clsName, out realClsName, out isByRef, true);
            sb.Append(
                @"/*
 * JEngine自动生成的Mono方法脚本，作者已经代替你掉了头发，帮你写出了这个Mono适配器脚本，让你能够直接调用全部Mono类
 */
using System;
using UnityEngine;
using JEngine.Core;
using ILRuntime.CLR.Method;
using System.Threading.Tasks;

namespace " + nameSpace + @"
{");
            sb.Append(@"
    public partial class ");
            sb.Append(clsName);
            sb.Append(@"Adapter
    {
        public partial class Adapter
        {
");
            var lines = File.ReadAllLines("Assets/Dependencies/JEngine/Templates/MonoAdapter.txt");
            foreach (var line in lines)
            {
                sb.AppendLine(line);
            }

            sb.Append(@"        }");
            sb.Append(@"
    }
}");
            return sb.ToString();
        }

        private static string GenerateCrossBindingAdapterEditorCode(Type baseType,string nameSpace)
        {
            if (!IsMono(baseType))
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();

            string clsName, realClsName;
            bool isByRef;
            baseType.GetClassName(out clsName, out realClsName, out isByRef, true);
            sb.Append(
                @"/*
 * JEngine自动生成的编辑器脚本，作者已经代替你掉了头发，帮你写出了这个编辑器脚本，让你能够直接看对象序列化后的字段
 */
using UnityEditor;
using UnityEngine;
using JEngine.Editor;
using " + nameSpace + @";
using UnityEditor.AnimatedValues;

");
            sb.Append(
                @"[CustomEditor(typeof(");
            sb.Append(clsName + "Adapter.Adapter");
            sb.Append(@"), true)]
public class ");
            sb.Append(clsName);
            sb.Append(@"AdapterEditor : Editor
{
");
            var lines = File.ReadAllLines("Assets/Dependencies/JEngine/Templates/AdapterEditor.txt");
            foreach (var line in lines)
            {
                if (line.Contains("MonoBehaviourAdapter.Adaptor clr = target as MonoBehaviourAdapter.Adaptor;"))
                {
                    sb.AppendLine($"        {nameSpace}.{clsName}Adapter.Adapter clr = target as {nameSpace}.{clsName}Adapter.Adapter;");
                    continue;
                }
                sb.AppendLine(line);
            }

            sb.Append(@"}");
            return sb.ToString();
        }
    }
}