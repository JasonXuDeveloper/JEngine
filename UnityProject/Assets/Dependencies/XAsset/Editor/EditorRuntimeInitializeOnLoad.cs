﻿//
// EditorRuntimeInitializeOnLoad.cs
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using JEngine.Core;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;

namespace libx
{
    public static class EditorRuntimeInitializeOnLoad
    {
        [RuntimeInitializeOnLoadMethod]
        private static void OnInitialize()
        {
            Assets.basePath = BuildScript.outputPath + Path.DirectorySeparatorChar;
            Assets.loadDelegate = AssetDatabase.LoadAssetAtPath; 

            var assets = new List<string>();
            var rules = BuildScript.GetBuildRules();
            foreach (var asset in rules.scenesInBuild)
            {
                var path = AssetDatabase.GetAssetPath(asset);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }
                assets.Add(path); 
            } 
            foreach (var rule in rules.rules)
            {
                if (rule.searchPattern.Contains("*.unity"))
                {
                    assets.AddRange(rule.GetAssets());
                }
            }
            
            List<EditorBuildSettingsScene> _scenes =new List<EditorBuildSettingsScene>(0);
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.path.Contains("Init.unity") || assets.Contains(scene.path))
                {
                    continue;
                }

                _scenes.Add(scene);
                Log.Print(scene.path);
            }

            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[assets.Count + _scenes.Count + 1];
            
            scenes[0] = new EditorBuildSettingsScene("Assets/Init.unity", true);//添加启动场景
            int _index = 1;
            for (var index = 0; index < _scenes.Count; index++)//添加其他场景（用户自己加的）
            {
                scenes[index + 1] = new EditorBuildSettingsScene(_scenes[index].path, true);
                _index++;
            }
            for (var index = _index; index < scenes.Length; index++)//添加热更场景（用于编译测试）
            {
                var asset = assets[index - _index]; 
                scenes[index] = new EditorBuildSettingsScene(asset, true);
            }

            EditorBuildSettings.scenes = scenes;
        }

        [InitializeOnLoadMethod]
        private static void OnEditorInitialize()
        {
            EditorUtility.ClearProgressBar();
            // BuildScript.GetManifest();
            // BuildScript.GetBuildRules();
        }
    }
}