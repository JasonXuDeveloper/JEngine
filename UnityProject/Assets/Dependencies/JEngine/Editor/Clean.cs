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
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using System.IO;
using JEngine.Core;

namespace JEngine.Editor
{
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

        if (!Directory.Exists("Assets/HotUpdateResources/Dll/Hidden~"))//DLL导入到隐藏文件夹，防止每次加载浪费时间
        {
            Directory.CreateDirectory("Assets/HotUpdateResources/Dll/Hidden~");
        }
        
        DirectoryInfo library = new DirectoryInfo(Application.dataPath+"/Library/ScriptAssemblies");
        DirectoryInfo di = new DirectoryInfo("Assets/HotUpdateResources/Dll/Hidden~");
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
                if (File.Exists(library.FullName+"/"+name))//重复依赖直接删除
                {
                    DLLMgr.Delete(file.FullName);
                    counts++;
                }
                else if(!name.Contains("HotUpdateScripts")&&(name.Contains(".pdb")||name.Contains(".dll")))//不存在就添加
                {
                    File.Move(file.FullName,file.FullName.Replace("/Hidden~",""));
                }
            }

            watch.Stop();
            if (counts > 0)//如果删除过东西，就代表DLL更新了，就需要生成文件
            {
                Log.Print("Cleaned: " + counts + " files in: " + watch.ElapsedMilliseconds + " ms.");
                DLLMgr.Delete("Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
                watch = new Stopwatch();
                DLLMgr.MakeBytes();
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
            DLLMgr.MakeBytes();
            watch.Stop();
            Log.Print("Convert DLL in: " + watch.ElapsedMilliseconds + " ms.");
            AssetDatabase.Refresh();
            isDone = true;
        }
    }

    
}
}