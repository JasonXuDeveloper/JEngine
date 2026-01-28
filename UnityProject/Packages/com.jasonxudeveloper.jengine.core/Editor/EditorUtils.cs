// EditorUtils.cs
//
//  Author:
//        JasonXuDeveloper <jason@xgamedev.net>
//
//  Copyright (c) 2025 JEngine
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

namespace JEngine.Core.Editor
{
    public static class EditorUtils
    {
        public static List<string> GetAvailableYooAssetPackages()
        {
            var packages = new List<string>();

            try
            {
                if (AssetBundleCollectorSettingData.Setting != null &&
                    AssetBundleCollectorSettingData.Setting.Packages != null)
                {
                    foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
                    {
                        if (!string.IsNullOrEmpty(package.PackageName))
                        {
                            packages.Add(package.PackageName);
                        }
                    }
                }

                // Add default if no packages found
                if (packages.Count == 0)
                {
                    packages.Add("main");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get YooAsset packages: {ex.Message}");
                packages.Add("main");
            }

            return packages;
        }

        /// <summary>
        /// Gets the YooAsset package name that contains the specified asset path.
        /// </summary>
        /// <param name="assetPath">The asset path to search for</param>
        /// <param name="fallbackPackageName">Fallback package name if not found (default: "main")</param>
        /// <returns>The package name containing the asset, or fallback if not found</returns>
        public static string GetPackageNameForAsset(string assetPath, string fallbackPackageName = "main")
        {
            try
            {
                if (AssetBundleCollectorSettingData.Setting != null &&
                    AssetBundleCollectorSettingData.Setting.Packages != null)
                {
                    foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
                    {
                        if (package.Groups == null) continue;

                        foreach (var group in package.Groups)
                        {
                            if (group == null || group.Collectors == null) continue;

                            foreach (var collector in group.Collectors)
                            {
                                if (collector == null || string.IsNullOrEmpty(collector.CollectPath)) continue;

                                // Check if asset path matches collector path
                                var collectPath = collector.CollectPath;
                                if (assetPath.StartsWith(collectPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    return package.PackageName;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get package for asset {assetPath}: {ex.Message}");
            }

            return fallbackPackageName;
        }

        /// <summary>
        /// Gets available .asmdef files from Assets/HotUpdate directory.
        /// </summary>
        public static List<string> GetAvailableAsmdefFiles()
        {
            var asmdefGuids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { "Assets/HotUpdate" });
            return asmdefGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(System.IO.Path.GetFileNameWithoutExtension)
                .Select(asmdefName => asmdefName + ".dll")
                .ToList();
        }

        /// <summary>
        /// Gets available scene files from Assets/HotUpdate directory.
        /// </summary>
        public static List<string> GetAvailableHotScenes()
        {
            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/HotUpdate" });
            return sceneGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToList();
        }

        /// <summary>
        /// Gets available public classes from the specified hot code assembly.
        /// </summary>
        /// <param name="hotCodeName">The hot code assembly name (e.g., "HotUpdate.Code.dll").</param>
        public static List<string> GetAvailableHotClasses(string hotCodeName)
        {
            try
            {
                var assemblyName = System.IO.Path.GetFileNameWithoutExtension(hotCodeName);
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == assemblyName);

                if (assembly != null)
                {
                    return assembly.GetTypes()
                        .Where(t => t.IsClass && t.IsPublic)
                        .Where(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static).Any())
                        .Select(t => t.FullName)
                        .OrderBy(n => n)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get hot classes: {ex.Message}");
            }

            return new List<string>();
        }

        /// <summary>
        /// Gets available static methods from the specified hot update entry class.
        /// </summary>
        /// <param name="hotCodeName">The hot code assembly name.</param>
        /// <param name="hotUpdateClassName">The full class name.</param>
        public static List<string> GetAvailableHotMethods(string hotCodeName, string hotUpdateClassName)
        {
            try
            {
                if (!string.IsNullOrEmpty(hotUpdateClassName))
                {
                    var type = Type.GetType(hotUpdateClassName);
                    if (type == null)
                    {
                        var assemblyName = System.IO.Path.GetFileNameWithoutExtension(hotCodeName);
                        var assembly = AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name == assemblyName);
                        if (assembly != null)
                        {
                            type = assembly.GetType(hotUpdateClassName);
                        }
                    }

                    if (type != null)
                    {
                        return type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .Where(m => m.ReturnType == typeof(void) ||
                                        m.ReturnType == typeof(UniTask) ||
                                        m.ReturnType == typeof(System.Threading.Tasks.Task))
                            .Where(m => m.GetParameters().Length == 0)
                            .Select(m => m.Name)
                            .OrderBy(n => n)
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get hot methods: {ex.Message}");
            }

            return new List<string>();
        }

        /// <summary>
        /// Gets available dynamic secret key files (.bytes) from the project.
        /// </summary>
        public static List<string> GetAvailableDynamicSecretKeys()
        {
            var secretKeys = new List<string>();

            var bytesGuids = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets" });
            var secretKeyFiles = bytesGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".bytes") &&
                               (path.Contains("Secret") || path.Contains("Obfuz") || path.Contains("Key")))
                .ToList();

            if (secretKeyFiles.Any())
            {
                secretKeys.AddRange(secretKeyFiles);
            }

            if (secretKeys.Count == 0)
            {
                secretKeys.Add("Assets/HotUpdate/Obfuz/DynamicSecretKey.bytes");
            }

            return secretKeys;
        }

        /// <summary>
        /// Gets available AOT DLL list files from Assets/HotUpdate/Compiled directory.
        /// </summary>
        public static List<string> GetAvailableAOTDataFiles()
        {
            var aotDataGuids = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/HotUpdate/Compiled" });
            return aotDataGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".bytes"))
                .OrderBy(path => path)
                .ToList();
        }
    }
}