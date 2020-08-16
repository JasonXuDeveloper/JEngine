//
// AssetsMenuItem.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2020 fjy
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
using UnityEditor;
using UnityEngine;
using JEngine.Core;
using JEngine.Editor;
using Debug = UnityEngine.Debug;

namespace libx
{
    public static class MenuItems
    {
        private const string KBuildAssetBundles = "JEngine/XAsset/Bundles/Build Bundles %#&B";
        private const string KViewCachePath = "JEngine/XAsset/Bundles/View Caches";
        private const string KViewDataPath = "JEngine/XAsset/Bundles/View Bundles";
        private const string KCleanData = "JEngine/XAsset/Bundles/Clean Bundles";
        
        

        [MenuItem(KBuildAssetBundles)]
        private static void BuildAssetBundles()
        {
            DLLMgr.Delete("Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
            DLLMgr.Delete(Directory.GetParent(Application.dataPath)+"/Assets/XAsset/ScriptableObjects/Rules.asset");
            DLLMgr.Delete(Directory.GetParent(Application.dataPath)+"/Assets/XAsset/ScriptableObjects/Manifest.asset");


            CryptoWindow.ShowWindow();
            CryptoWindow.Build= s =>
            {
                var watch = new Stopwatch();
                watch.Start();
                var bytes = DLLMgr.FileToByte(DLLMgr.DllPath);
                var result = DLLMgr.ByteToFile(CryptoHelper.AesEncrypt(bytes,s), "Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
                watch.Stop();
                Log.Print("Convert Dlls in: " + watch.ElapsedMilliseconds + " ms.");
                if (!result)
                {
                    Log.PrintError("DLL转Byte[]出错！");
                }
            
                watch = new Stopwatch();
                watch.Start();
                BuildScript.ApplyBuildRules();
                watch.Stop();
                Log.Print("ApplyBuildRules in: " + watch.ElapsedMilliseconds + " ms.");
            
                watch = new Stopwatch();
                watch.Start();
                BuildScript.BuildAssetBundles();
                watch.Stop();
                Log.Print("BuildAssetBundles in: " + watch.ElapsedMilliseconds + " ms."); 
            };
        }
        
        [MenuItem(KCleanData)]
        private static void CleanBundles()
        {
            var watch = new Stopwatch();
            watch.Start();
            DLLMgr.Delete(Directory.GetParent(Application.dataPath)+"/DLC");
            watch.Stop();
            Log.Print("Clean bundles in: " + watch.ElapsedMilliseconds + " ms.");
        }
        

        [MenuItem(KViewDataPath)]
        private static void ViewDataPath()
        {
            if(Directory.Exists(Directory.GetParent(Application.dataPath).FullName + "/DLC"))
            {
                EditorUtility.OpenWithDefaultApp(Directory.GetParent(Application.dataPath).FullName + "/DLC");
            }
            else 
            {
                Log.PrintError("Unable to View Bundles: Please Build Bundles First");
            }
        }
        
        [MenuItem(KViewCachePath)]
        private static void ViewCachePath()
        {
            EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
        }

        #region Tools 
        [MenuItem("JEngine/XAsset/Tools/View CRC")]
        private static void GetCRC()
        {
            var path = EditorUtility.OpenFilePanel("OpenFile", Environment.CurrentDirectory, "");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            using (var fs = File.OpenRead(path))
            {
                var crc = Utility.GetCRC32Hash(fs);
                Log.Print(crc);
            }
        }

        [MenuItem("JEngine/XAsset/Tools/View MD5")]
        private static void GetMD5()
        {
            var path = EditorUtility.OpenFilePanel("OpenFile", Environment.CurrentDirectory, "");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            using (var fs = File.OpenRead(path))
            {
                var crc = Utility.GetMD5Hash(fs);
                Log.Print(crc);
            }
        }
        #endregion 
    }
}