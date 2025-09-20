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
                        addOnDemoButton.interactable = false; // 防止重复点击
                        Debug.Log("AddOnDemoButton clicked.");
                        var packageName = "addon1";

                        // 创建或获取AddOn1包
                        var package = Bootstrap.CreateOrGetPackage(packageName);

                        // 设置AddOn包的初始化回调
                        var callbacks = new PackageInitializationCallbacks
                        {
                            OnStatusUpdate = static status => Debug.Log($"[AddOn1] 状态: {GetStatusText(status)}"),
                            OnVersionUpdate = static version => Debug.Log($"[AddOn1] 版本: {version}"),
                            OnDownloadPrompt = static (count, size) => MessageBox.Show("提示",
                                $"[AddOn1] 需要下载 {count} 个文件，总大小 {size / 1024f / 1024f:F2}MB，是否继续？",
                                "是", "否"),
                            OnDownloadProgress = static data =>
                            {
                                Debug.Log(
                                    $"[AddOn1] 下载进度: {data.CurrentDownloadCount}/{data.TotalDownloadCount} ({Mathf.RoundToInt(data.Progress * 100)}%)");
                            },
                            OnDownloadStart = static () => Debug.Log("[AddOn1] 开始下载..."),
                            OnDownloadComplete = static () => Debug.Log("[AddOn1] 下载完成！"),
                            OnError = static async error =>
                            {
                                Debug.LogError($"[AddOn1] 错误: {error}");
                                await UniTask.CompletedTask; // 简单的错误处理
                            }
                        };

                        // 使用Bootstrap的通用初始化函数
                        Debug.Log("[AddOn1] 开始初始化AddOn1包...");
                        bool success = await Bootstrap.UpdatePackage(package, callbacks);

                        if (!success)
                        {
                            Debug.LogError("[AddOn1] AddOn1包初始化失败！");
                            return;
                        }

                        Debug.Log("[AddOn1] AddOn1包初始化成功！");

                        // 加载AddOn1场景
                        var sceneLoadCallbacks = new SceneLoadCallbacks
                        {
                            OnStatusUpdate = static status => Debug.Log($"[AddOn1] {GetSceneLoadStatusText(status)}"),
                            OnProgressUpdate = static progress => Debug.Log($"[AddOn1] 加载进度: {progress * 100:F0}%"),
                            OnError = static exception =>
                            {
                                Debug.LogError($"[AddOn1] 场景加载失败: {exception.Message}");
                                return UniTask.CompletedTask;
                            }
                        };

                        var handle = await Bootstrap.LoadHotUpdateScene(package,
                            "Assets/HotUpdate/AddOn1/Scene/test.unity",
                            sceneLoadCallbacks);
                        if (handle != null)
                        {
                            Debug.Log("进入分包场景");
                        }
                        else
                        {
                            Debug.LogError("[AddOn1] 场景加载异常");
                        }

                        // 加载AddOn1资源
                        var textHandle = package.LoadAssetAsync<TextAsset>("Assets/HotUpdate/AddOn1/Other/test.txt");
                        await textHandle.Task;
                        if (textHandle.Status == EOperationStatus.Succeed)
                        {
                            var textAsset = textHandle.GetAssetObject<TextAsset>();
                            Debug.Log($"[AddOn1] 加载的文本内容: {textAsset.text}");
                            textHandle.Release(); // 记得释放资源
                        }
                        else
                        {
                            Debug.LogWarning($"[AddOn1] 未能加载test.txt文件: {textHandle.LastError}");
                        }

                        // 清理AddOn1包的缓存（可选）
                        await Bootstrap.DeletePackageCache(package);
                        Debug.Log("[AddOn1] 清理AddOn1包的下载缓存完成。");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[AddOn1] 发生异常: {ex.Message}");
                    }
                    finally
                    {
                        try
                        {
                            addOnDemoButton.interactable = true; // 恢复按钮可点击
                        }
                        catch
                        {
                            // 忽略异常
                        }
                    }
                }).Forget();
            });
        }

        private static string GetStatusText(PackageInitializationStatus status)
        {
            return status switch
            {
                PackageInitializationStatus.InitializingPackage => "正在初始化资源包...",
                PackageInitializationStatus.GettingVersion => "正在获取资源包版本...",
                PackageInitializationStatus.UpdatingManifest => "正在更新资源清单...",
                PackageInitializationStatus.CheckingUpdate => "正在检查需要下载的资源...",
                PackageInitializationStatus.DownloadingResources => "正在下载资源...",
                PackageInitializationStatus.Completed => "资源包初始化完成",
                PackageInitializationStatus.Failed => "初始化失败",
                _ => status.ToString()
            };
        }

        private static string GetSceneLoadStatusText(SceneLoadStatus status)
        {
            return status switch
            {
                SceneLoadStatus.Loading => "正在加载场景...",
                SceneLoadStatus.Completed => "场景加载完成",
                SceneLoadStatus.Failed => "场景加载失败",
                _ => status.ToString()
            };
        }
    }
}