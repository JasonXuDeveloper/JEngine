//
// Clean.cs
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
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using JEngine.Core;
using Debug = UnityEngine.Debug;

namespace JEngine.Editor
{
    [InitializeOnLoad]
    class Clean
    {
        static Clean()
        {
            isDone = true;
            EditorApplication.update += Update;
            library = new DirectoryInfo(Application.dataPath + "/../Library/ScriptAssemblies");
            di = new DirectoryInfo("Assets/HotUpdateResources/Dll/Hidden~");
        }

        private static bool isDone;

        private static DirectoryInfo library;
        private static DirectoryInfo di;

        private const string updateTimeStr = "JEngine.Editor.Clean.UpdateTime";

        static void Update()
        {
            if (!isDone || EditorApplication.isPlaying)
            {
                return;
            }

            if (!Directory.Exists("Assets/HotUpdateResources/Dll/Hidden~")) //DLL导入到隐藏文件夹，防止每次加载浪费时间
            {
                Directory.CreateDirectory("Assets/HotUpdateResources/Dll/Hidden~");
            }

            if (!File.Exists(DLLMgr.DllPath)) //没热更dll就返回
            {
                return;
            }

            //有的话比较日期
            DateTime lastModified = File.GetLastWriteTime(DLLMgr.DllPath);
            string lastModifiedStr = lastModified.Ticks.ToString();
            if (PlayerPrefs.GetString(updateTimeStr) != lastModifiedStr) //不一样再处理
            {
                var files = di.GetFiles();
                var watch = new Stopwatch();
                int counts = 0;
                List<string> fileNames = Directory.GetFiles("Assets/",
                    "*.dll", SearchOption.AllDirectories).ToList();
                
                DLLMgr.Delete("Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
                watch = new Stopwatch();
                DLLMgr.MakeBytes();
                watch.Stop();
                if (watch.ElapsedMilliseconds > 0)
                {
                    Log.Print("Convert DLL in: " + watch.ElapsedMilliseconds + " ms.");
                }

                AssetDatabase.Refresh();
                
                PlayerPrefs.SetString(updateTimeStr, lastModifiedStr);
                
                isDone = false;
                fileNames = fileNames.FindAll((x) => !x.Contains("~"));
                
                watch.Start();
                foreach (var file in files)
                {
                    var name = file.Name;
                    if (!File.Exists(library.FullName + "/" + name) && !name.Contains("netstandard") &&
                        !name.Contains("HotUpdateScripts") && !name.Contains("Unity") && !name.Contains("System") &&
                        ((name.Contains(".pdb") || name.Contains(".dll"))))
                    {
                        if (fileNames.Find(x => x.Contains(name)) == null) //不存在就添加
                        {
                            DLLMgr.Delete(file.FullName.Replace("Hidden~", "../Dll"));
                            File.Move(file.FullName, file.FullName.Replace("Hidden~", "../Dll"));
                            Log.Print(
                                $"Find new referenced dll `{name}`, note that your hot update code may not be able " +
                                $"to run without rebuild application\n" +
                                $"发现新的引用DLL`{name}`，请注意，游戏可能需要重新打包，否则热更代码无法将有可能运行");
                        }
                        else //存在就删了
                        {
                            DLLMgr.Delete(file.FullName);
                            counts++;
                        }
                    }
                    else if (!name.Contains("HotUpdateScripts"))
                    {
                        DLLMgr.Delete(file.FullName);
                        counts++;
                    }
                    else
                    {
                        if (!name.Contains("HotUpdateScripts"))
                        {
                            Log.PrintError($"无法删除{file.FullName}，请手动删除");
                        }
                    }
                }

                watch.Stop();
                if (counts > 0) //如果删除过东西，就代表DLL更新了，就需要生成文件
                {
                    Log.Print("Cleaned: " + counts + " files in: " + watch.ElapsedMilliseconds + " ms.");
                }

                isDone = true;
            }

            if (!File.Exists("Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes"))
            {
                isDone = false;
                var watch = new Stopwatch();
                DLLMgr.MakeBytes();
                watch.Stop();
                Log.Print("Convert DLL in: " + watch.ElapsedMilliseconds + " ms.");
                AssetDatabase.Refresh();
                isDone = true;
            }
        }
    }
}
