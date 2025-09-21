// EntryPoint.cs
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

using Cysharp.Threading.Tasks;
using JEngine.Core;
using JEngine.Core.Encrypt;
using JEngine.Core.Misc;
using JEngine.Core.Update;
using Obfuz;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using YooAsset;

namespace HotUpdate.Code
{
    [Preserve]
    [ObfuzIgnore(ObfuzScope.TypeName)]
    public static class EntryPoint
    {
        [Preserve]
        [ObfuzIgnore(ObfuzScope.MethodName)]
        public static void RunGame()
        {
            Debug.Log("HotUpdateScripts EntryPoint RunGame called.");
            // Your game logic goes here
            Button addOnDemoButton = GameObject.Find("Canvas/BtnList/AddOnDemoButton").GetComponent<Button>();
            addOnDemoButton.onClick.AddListener(() =>
            {
                UniTask.RunOnThreadPool(async () =>
                {
                    await UniTask.SwitchToMainThread();
                    try
                    {
                        addOnDemoButton.interactable = false; // Prevent duplicate clicks
                        Debug.Log("AddOnDemoButton clicked.");
                        var packageName = "addon1";

                        // Create or get AddOn1 package
                        var package = Bootstrap.CreateOrGetPackage(packageName);

                        // Set up AddOn package initialization callbacks
                        var callbacks = new PackageInitializationCallbacks
                        {
                            OnStatusUpdate = static status => Debug.Log($"[AddOn1] Status: {GetStatusText(status)}"),
                            OnVersionUpdate = static version => Debug.Log($"[AddOn1] Version: {version}"),
                            OnDownloadPrompt = static (count, size) => MessageBox.Show("Notice",
                                $"[AddOn1] Need to download {count} files, total size {size / 1024f / 1024f:F2}MB. Continue?",
                                "Yes", "No"),
                            OnDownloadProgress = static data =>
                            {
                                Debug.Log(
                                    $"[AddOn1] Download progress: {data.CurrentDownloadCount}/{data.TotalDownloadCount} ({Mathf.RoundToInt(data.Progress * 100)}%)");
                            },
                            OnDownloadStart = static () => Debug.Log("[AddOn1] Starting download..."),
                            OnDownloadComplete = static () => Debug.Log("[AddOn1] Download completed!"),
                            OnError = static async error =>
                            {
                                Debug.LogError($"[AddOn1] Error: {error}");
                                await UniTask.CompletedTask; // Simple error handling
                            }
                        };

                        // Use Bootstrap's common initialization function
                        Debug.Log("[AddOn1] Starting AddOn1 package initialization...");
                        bool success = await Bootstrap.UpdatePackage(package, callbacks, EncryptionOption.Xor);

                        if (!success)
                        {
                            Debug.LogError("[AddOn1] AddOn1 package initialization failed!");
                            return;
                        }

                        Debug.Log("[AddOn1] AddOn1 package initialization successful!");

                        // Load AddOn1 scene
                        var sceneLoadCallbacks = new SceneLoadCallbacks
                        {
                            OnStatusUpdate = static status => Debug.Log($"[AddOn1] {GetSceneLoadStatusText(status)}"),
                            OnProgressUpdate = static progress => Debug.Log($"[AddOn1] Loading progress: {progress * 100:F0}%"),
                            OnError = static exception =>
                            {
                                Debug.LogError($"[AddOn1] Scene loading failed: {exception.Message}");
                                return UniTask.CompletedTask;
                            }
                        };

                        var handle = await Bootstrap.LoadHotUpdateScene(package,
                            "Assets/HotUpdate/AddOn1/Scene/test.unity",
                            sceneLoadCallbacks);
                        if (handle != null)
                        {
                            Debug.Log("Entered addon scene");
                        }
                        else
                        {
                            Debug.LogError("[AddOn1] Scene loading exception");
                        }

                        // Load AddOn1 resources
                        var textHandle = package.LoadAssetAsync<TextAsset>("Assets/HotUpdate/AddOn1/Other/test.txt");
                        await textHandle.Task;
                        if (textHandle.Status == EOperationStatus.Succeed)
                        {
                            var textAsset = textHandle.GetAssetObject<TextAsset>();
                            Debug.Log($"[AddOn1] Loaded text content: {textAsset.text}");
                            textHandle.Release(); // Remember to release resources
                        }
                        else
                        {
                            Debug.LogWarning($"[AddOn1] Failed to load test.txt file: {textHandle.LastError}");
                        }

                        // Clear AddOn1 package cache (optional)
                        await Bootstrap.DeletePackageCache(package);
                        Debug.Log("[AddOn1] AddOn1 package download cache cleanup completed.");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[AddOn1] Exception occurred: {ex.Message}");
                    }
                    finally
                    {
                        try
                        {
                            addOnDemoButton.interactable = true; // Restore button clickability
                        }
                        catch
                        {
                            // Ignore exception
                        }
                    }
                }).Forget();
            });
        }

        private static string GetStatusText(PackageInitializationStatus status)
        {
            return status switch
            {
                PackageInitializationStatus.InitializingPackage => "Initializing resource package...",
                PackageInitializationStatus.GettingVersion => "Getting resource package version...",
                PackageInitializationStatus.UpdatingManifest => "Updating resource manifest...",
                PackageInitializationStatus.CheckingUpdate => "Checking resources to download...",
                PackageInitializationStatus.DownloadingResources => "Downloading resources...",
                PackageInitializationStatus.Completed => "Resource package initialization completed",
                PackageInitializationStatus.Failed => "Initialization failed",
                _ => "Unknown status"
            };
        }

        private static string GetSceneLoadStatusText(SceneLoadStatus status)
        {
            return status switch
            {
                SceneLoadStatus.Loading => "Loading scene...",
                SceneLoadStatus.Completed => "Scene loading completed",
                SceneLoadStatus.Failed => "Scene loading failed",
                _ => "Unknown status"
            };
        }
    }
}