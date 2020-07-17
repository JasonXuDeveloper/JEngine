//
// JUIBehaviour.cs
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
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using System.IO;
using JEngine.Core;

namespace JEngine.Editor
{
 #if UNITY_EDITOR
[InitializeOnLoad]
class Clean
{
    static Clean()
    {
        isDone = true;
        EditorApplication.update += Update;
    }

    private static bool isDone;

    static void Update()
    {
        if (!isDone)
        {
            return;
        }

        DirectoryInfo di = new DirectoryInfo("Assets/HotUpdateResources/Dll");
        var files = di.GetFiles();
        if (files.Length > 1)
        {
            isDone = false;
            int counts = 0;
            var watch = new Stopwatch();
            watch.Start();
            foreach (var file in files)
            {
                var name = file.Name;
                if (!file.Name.Contains("HotUpdateScripts"))
                {
                    File.Delete(file.FullName);
                    counts++;
                }
            }

            watch.Stop();
            if (counts > 0)
            {
                Log.Print("Cleaned: " + counts + " files in: " + watch.ElapsedMilliseconds + " ms.");
                Delete("Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
                watch = new Stopwatch();
                MakeBytes();
                watch.Stop();
                Log.Print("Convert DLL in: " + watch.ElapsedMilliseconds + " ms.");
                AssetDatabase.Refresh();
            }

            isDone = true;
        }

        if (!File.Exists("Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes"))
        {
            isDone = false;
            var watch = new Stopwatch();
            MakeBytes();
            watch.Stop();
            Log.Print("Convert DLL in: " + watch.ElapsedMilliseconds + " ms.");
            AssetDatabase.Refresh();
            isDone = true;
        }
    }

    [MenuItem("JEngine/XAsset/Bundles/ConvertDLL")]
    public static void MakeBytes()
    {
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
    }

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
}
#endif   
}