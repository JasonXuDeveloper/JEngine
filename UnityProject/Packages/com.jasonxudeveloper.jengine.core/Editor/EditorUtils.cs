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
    }
}