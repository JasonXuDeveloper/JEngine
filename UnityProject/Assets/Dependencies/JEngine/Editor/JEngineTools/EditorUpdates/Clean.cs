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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using JEngine.Core;
using UnityEditor;

namespace JEngine.Editor
{
    internal static class Clean
    {
        public static bool initialised;

        private static readonly DirectoryInfo HiddenDirectory =
            new DirectoryInfo(ConstMgr.DLLSourceFolder);


        public static void Initialise()
        {
            var watcher = new FileSystemWatcher(HiddenDirectory.FullName);
            watcher.NotifyFilter = NotifyFilters.Attributes
                                   | NotifyFilters.CreationTime
                                   | NotifyFilters.DirectoryName
                                   | NotifyFilters.FileName
                                   | NotifyFilters.LastAccess
                                   | NotifyFilters.LastWrite
                                   | NotifyFilters.Security
                                   | NotifyFilters.Size;

            watcher.Changed += (_, _) => { EditorApplication.delayCall += MakeBytes; };

            watcher.Filter = $"{ConstMgr.MainHotDLLName}.dll";
            watcher.EnableRaisingEvents = true;
            initialised = true;
        }

        private static void MakeBytes()
        {
            Action<string> buildAct = async s =>
            {
                var watch = new Stopwatch();
                watch.Start();
                string dllPath = DllMgr.GetDllInEditorPath(ConstMgr.MainHotDLLName);
                if (!File.Exists(dllPath))
                {
                    Log.PrintError("DLL文件不存在！");
                    return;
                }

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