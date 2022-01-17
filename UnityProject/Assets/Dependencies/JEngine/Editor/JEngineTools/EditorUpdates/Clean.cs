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
using System.IO;
using System.Linq;
using JEngine.Core;
using UnityEditor;
using UnityEngine;

namespace JEngine.Editor
{
    internal static class Clean
         {
             public static bool hasAdded;
             
             private static bool _isDone = true;
     
             private static readonly string HotProjectName = "HotUpdateScripts";
             
             private static readonly DirectoryInfo LibraryDirectory = new DirectoryInfo(Application.dataPath + "/../Library/ScriptAssemblies");
             private static readonly DirectoryInfo HiddenDirectory = new DirectoryInfo("Assets/HotUpdateResources/Dll/Hidden~");

             public static void Update()
             {
                 hasAdded = true;
                 
                 if (!_isDone || EditorApplication.isPlaying)
                 {
                     return;
                 }
     
                 if (!HiddenDirectory.Exists) //DLL导入到隐藏文件夹，防止每次加载浪费时间
                 {
                     HiddenDirectory.Create();
                 }
     
                 if (!File.Exists(DLLMgr.DllPath)) //没热更dll就返回
                 {
                     return;
                 }
     
                 //有的话比较日期
                 DateTime lastModified = File.GetLastWriteTime(DLLMgr.DllPath);
                 string lastModifiedStr = lastModified.ToString(Setting.GetString(SettingString.DateFormat));
                 if (Setting.LastDLLCleanUpTime != lastModifiedStr) //不一样再处理
                 {
                     var files = HiddenDirectory.GetFiles();
                     int counts = 0;
                     List<string> fileNames = Directory.GetFiles("Assets/",
                         "*.dll", SearchOption.AllDirectories).ToList();
                     
                     var watch = new Stopwatch();
                     AssetDatabase.Refresh();
     
                     Setting.LastDLLCleanUpTime = lastModifiedStr;
     
                     _isDone = false;
                     fileNames = fileNames.FindAll(x => !x.Contains("~"));
                     
                     watch.Start();
                     foreach (var file in files)
                     {
                         var name = file.Name;
                         var success = true;
                         if (!File.Exists(LibraryDirectory.FullName + "/" + name) && !name.Contains("netstandard") &&
                             !name.Contains(HotProjectName) && !name.Contains("Unity") && !name.Contains("System") &&
                             (name.EndsWith(".pdb") || name.EndsWith(".dll")))
                         {
                             if (fileNames.Find(x => x.Contains(name)) == null) //不存在就添加
                             {
                                 success = false;
                             }
                             else //存在就删了
                             {
                                 DLLMgr.Delete(file.FullName);
                                 counts++;
                             }
                         }
                         else if (!name.Contains(HotProjectName))
                         {
                             try
                             {
                                 DLLMgr.Delete(file.FullName);
                                 counts++;
                             }
                             catch
                             {
                                 Log.Print(String.Format(
                                     Setting.GetString(SettingString.DeleteErrorLog),
                                     file.Name));
                                 success = false;
                             }
                         }

                         if (!success)
                         {
                             if (file.Directory != null)
                             {
                                 DirectoryInfo newPath = new DirectoryInfo(file.Directory.FullName)?.Parent?.Parent?.Parent;
                                 if (newPath != null)
                                 {
                                     File.Move(file.FullName, newPath.FullName + "/" + file.Name);
                                     Log.Print(String.Format(
                                         Setting.GetString(SettingString.DLLNewReferenceLog),
                                         newPath.FullName + "/" + file.Name));
                                 }
                             }
                         }
                     }
     
                     watch.Stop();
                     if (counts > 0) //如果删除过东西，就代表DLL更新了，就需要生成文件
                     {
                         Log.Print(String.Format(Setting.GetString(SettingString.DLLCleanLog),
                             counts,
                             watch.ElapsedMilliseconds));
                     }
     
                     _isDone = true;
                 }
             }
         }
}
