//
// MenuItems.cs
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
#if UNITY_EDITOR
using System.Diagnostics;
using System.Reflection;
using JEngine.Core;
using libx;
using UnityEditor;
using UnityEngine;

namespace JEngine.Editor
{
    [Obfuscation(Exclude = true)]
    internal class MenuItems
    {
        [MenuItem("JEngine/Open Documents",priority = 1999)]
        public static void OpenDocument()
        {
            Application.OpenURL("https://xgamedev.uoyou.com");
        }
        
        [MenuItem("JEngine/Open on Github",priority = 2000)]
        public static void OpenGithub()
        {
            Application.OpenURL("https://github.com/JasonXuDeveloper/JEngine");
        }
      
        // Xasset Pro,打包需采用AES加密
#if XASSET_PRO
        [MenuItem("JEngine/DLL加密 （XASSET PRO请先执行这个再去打Bundles）")]
        private static void BuildAssetBundles()
        {
            DLLMgr.Delete("Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
            CryptoWindow.ShowWindow();
            CryptoWindow.Build = s =>
            {
                var watch = new Stopwatch();
                watch.Start();
                var bytes = DLLMgr.FileToByte(DLLMgr.DllPath);
                var result = DLLMgr.ByteToFile(CryptoHelper.AesEncrypt(bytes, s),
                    "Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
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
#endif
    }
}
#endif
