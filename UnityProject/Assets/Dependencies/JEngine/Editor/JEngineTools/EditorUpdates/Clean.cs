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
using System.Threading.Tasks;
using JEngine.Core;
using UnityEditor;
using UnityEngine;

namespace JEngine.Editor
{
    internal static class Clean
    {
        public static bool hasAdded;

        private static bool _isDone = true;

        private static readonly string HotProjectName = ConstMgr.MainHotDLLName;

        private static readonly DirectoryInfo LibraryDirectory =
            new DirectoryInfo(Application.dataPath + "/../Library/ScriptAssemblies");
        
        private static readonly DirectoryInfo PackageDirectory =
            new DirectoryInfo(Application.dataPath + "/../Library/PackageCache");

        private static readonly DirectoryInfo HiddenDirectory =
            new DirectoryInfo(ConstMgr.DLLSourceFolder);

        public delegate void PostCleanEvent(int count);

        public static event PostCleanEvent onPostClean;

        static Clean()
        {
            onPostClean += cnt => MakeBytes();
        }

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

            if (!FileMgr.HasFile(DllMgr.GetDllInEditorPath(ConstMgr.MainHotDLLName))) //没热更dll就返回
            {
                return;
            }

            //有的话比较日期
            DateTime lastModified = File.GetLastWriteTime(DllMgr.GetDllInEditorPath(ConstMgr.MainHotDLLName));
            string lastModifiedStr = lastModified.ToString(Setting.GetString(SettingString.DateFormat));
            if (Setting.LastDLLCleanUpTime != lastModifiedStr) //不一样再处理
            {
                var files = HiddenDirectory.GetFiles();
                int counts = 0;
                List<string> fileNames = Directory.GetFiles("Assets/",
                    "*.dll", SearchOption.AllDirectories).ToList();//白名单DLL
                //ScriptAssemblies和PackageCache的Dll也应该进白名单
                fileNames.AddRange(Directory.GetFiles(LibraryDirectory.FullName,
                    "*.dll", SearchOption.AllDirectories));
                fileNames.AddRange(Directory.GetFiles(PackageDirectory.FullName,
                    "*.dll", SearchOption.AllDirectories));

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
                            FileMgr.Delete(file.FullName);
                            counts++;
                        }
                    }
                    else if (!name.Contains(HotProjectName))
                    {
                        try
                        {
                            FileMgr.Delete(file.FullName);
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
                            DirectoryInfo newPath = new DirectoryInfo(file.Directory.FullName).Parent?.Parent?.Parent;
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
                    onPostClean?.Invoke(counts);
                }
                
                _isDone = true;
            }
        }

        private static void MakeBytes()
        {
            FileMgr.Delete(DllMgr.GetDllInRuntimePath(ConstMgr.MainHotDLLName));
            FileMgr.Delete(DllMgr.GetPdbInRuntimePath(ConstMgr.MainHotDLLName));

            Action<string> buildAct = async s =>
            {
                var watch = new Stopwatch();
                watch.Start();
                string dllPath = DllMgr.GetDllInEditorPath(ConstMgr.MainHotDLLName);
                var bytes = FileMgr.FileToBytes(dllPath);
                var result = FileMgr.BytesToFile(CryptoMgr.AesEncrypt(bytes, s),
                    DllMgr.GetDllInRuntimePath(ConstMgr.MainHotDLLName));
                watch.Stop();
                Log.Print("Convert Dlls in: " + watch.ElapsedMilliseconds + " ms.");
                if (!result)
                {
                    Log.PrintError("DLL转Byte[]出错！");
                    return;
                }

                watch.Reset();
                watch.Start();
                string pdbPath = DllMgr.GetPdbInEditorPath(ConstMgr.MainHotDLLName);
                if (File.Exists(pdbPath))
                {
                    bytes = FileMgr.FileToBytes(pdbPath);
                    result = FileMgr.BytesToFile(bytes,
                        DllMgr.GetPdbInRuntimePath(ConstMgr.MainHotDLLName));
                    watch.Stop();
                    Log.Print("Convert PDBs in: " + watch.ElapsedMilliseconds + " ms.");
                    if (!result)
                    {
                        Log.PrintError("PDB转Byte[]出错！");
                        return;
                    }   
                }

                Setting.EncryptPassword = s;

                await Task.Delay(3);
                AssetDatabase.Refresh();
            };

            if (string.IsNullOrEmpty(Setting.EncryptPassword))
            {
                CryptoWindow.ShowWindow();
                CryptoWindow.Build = buildAct;
            }
            else
            {
                buildAct.Invoke(Setting.EncryptPassword);
            }
        }
    }
}