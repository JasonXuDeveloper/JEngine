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
using Debug = UnityEngine.Debug;

namespace libx
{
    public static class MenuItems
    {
        private const string KBuildAssetBundles = "JEngine/XAsset/Bundles/Build Bundles %#&B";
        private const string KViewCachePath = "JEngine/XAsset/Bundles/View Caches";
        private const string KViewDataPath = "JEngine/XAsset/Bundles/View Bundles";
        private const string KCleanData = "JEngine/XAsset/Bundles/Clean Bundles";
        
        /// <summary>
        /// 删除文件或目录
        /// </summary>
        /// <param name="path"></param>
        private static void Delete(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                di.Delete(true);
            }
        }
        
        /// <summary>
        /// 将文件转换成byte[]数组
        /// </summary>
        /// <param name="fileUrl">文件路径文件名称</param>
        /// <returns>byte[]数组</returns>
        public static byte[] FileToByte(string fileUrl)
        {
            try
            {
                using (FileStream fs = new FileStream(fileUrl, FileMode.Open, FileAccess.Read))
                {
                    byte[] byteArray = new byte[fs.Length];
                    fs.Read(byteArray, 0, byteArray.Length);
                    return byteArray;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 将byte[]数组保存成文件
        /// </summary>
        /// <param name="byteArray">byte[]数组</param>
        /// <param name="fileName">保存至硬盘的文件路径</param>
        /// <returns></returns>
        public static bool ByteToFile(byte[] byteArray, string fileName)
        {
            bool result = false;
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    result = true;
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        [MenuItem(KBuildAssetBundles)]
        private static void BuildAssetBundles()
        {
            Delete("Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
            Delete(Directory.GetParent(Application.dataPath)+"/Assets/XAsset/ScriptableObjects/Rules.asset");
            Delete(Directory.GetParent(Application.dataPath)+"/Assets/XAsset/ScriptableObjects/Manifest.asset");
            
            var watch = new Stopwatch();
            watch.Start();
            var bytes = FileToByte("Assets/HotUpdateResources/Dll/HotUpdateScripts.dll");
            var result = ByteToFile(bytes, "Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
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
        }
        
        [MenuItem(KCleanData)]
        private static void CleanBundles()
        {
            var watch = new Stopwatch();
            watch.Start();
            Delete(Directory.GetParent(Application.dataPath)+"/DLC");
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