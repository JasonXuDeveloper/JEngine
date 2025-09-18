// Bootstrap.cs
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

using HybridCLR;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JEngine.Core.Update;
using Nino.Core;
using Obfuz;
using Obfuz.EncryptionVM;
using UnityEngine;
using UnityEngine.SceneManagement;
// using Obfuz.EncryptionVM;
using YooAsset;
using Application = UnityEngine.Application;
using TMPro;
using UnityEngine.UI;

namespace JEngine.Core
{
    public class Bootstrap : MonoBehaviour
    {
        [Header("Server Settings")] public string defaultHostServer = "http://127.0.0.1/";

        [HideInInspector] public bool useDefaultAsFallback = true;

        public string fallbackHostServer = "http://127.0.0.1/";

        [Header("Asset Settings")] public string packageName = "main";

        public string hotCodeName = "HotUpdate.Code.dll";

        public string selectedHotScene = "Assets/HotUpdate/Main/Scene/Game.unity";

        public string hotUpdateClassName = "HotUpdate.Code.EntryPoint";

        public string hotUpdateMethodName = "RunGame";

        [Header("Security Settings")] public string staticSecretKeyPath = "Obfuz/StaticSecretKey";

        public string dynamicSecretKeyPath = "Assets/HotUpdate/Obfuz/DynamicSecretKey.bytes";

        public string aotDllListFilePath = "Assets/HotUpdate/Compiled/AOT.bytes";

        [Header("UI Settings")] public TextMeshProUGUI versionText;

        public TextMeshProUGUI updateStatusText;

        public TextMeshProUGUI downloadProgressText;

        public Slider downloadProgressBar;

#if UNITY_EDITOR
        [Header("Development Settings")] [HideInInspector]
        public bool useEditorDevMode = true;
#endif

        public static string GetPlatform()
        {
#if UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "iOS";
#elif UNITY_STANDALONE_WIN
            return "Standalone";
#elif UNITY_STANDALONE_OSX
            return "StandaloneOSX";
#else
            return "Unknown";
#endif
        }

        private static Bootstrap _instance;

        public static Bootstrap Instance => _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void SetUpStaticSecretKey()
        {
            Debug.Log("SetUpStaticSecret begin");
            // Find the Bootstrap instance in the scene to get the configured path
            _instance = FindObjectOfType<Bootstrap>();
            string keyPath = _instance != null ? _instance.staticSecretKeyPath : "Obfuz/StaticSecretKey";

            var secretKeyAsset = Resources.Load<TextAsset>(keyPath);
            if (secretKeyAsset != null)
            {
                EncryptionService<DefaultStaticEncryptionScope>.Encryptor =
                    new GeneratedEncryptionVirtualMachine(secretKeyAsset.bytes);
            }
            else
            {
                Debug.LogError($"Failed to load static secret key from Resources/{keyPath}");
            }

            Debug.Log("SetUpStaticSecret end");
        }

        private async UniTask SetUpDynamicSecret()
        {
            Debug.Log("SetUpDynamicSecret begin");
            var handle = YooAssets.LoadAssetAsync<TextAsset>(dynamicSecretKeyPath);
            await handle.Task;
            TextAsset dynamicSecretKeyAsset = (TextAsset)handle.AssetObject;
            EncryptionService<DefaultDynamicEncryptionScope>.Encryptor =
                new GeneratedEncryptionVirtualMachine(dynamicSecretKeyAsset.bytes);
            handle.Release();

            Debug.Log("SetUpDynamicSecret end");
        }

        private async UniTask LoadMetadataForAOTAssemblies()
        {
            var aotListHandle = YooAssets.LoadAssetAsync<TextAsset>(aotDllListFilePath);
            await aotListHandle.Task;
            TextAsset aotDataAsset = (TextAsset)aotListHandle.AssetObject;
            var aotDllList = NinoDeserializer.Deserialize<List<string>>(aotDataAsset.bytes);
            aotListHandle.Release();

            foreach (var aotDllName in aotDllList)
            {
                if (!YooAssets.CheckLocationValid(aotDllName))
                {
                    Debug.LogError($"AOT DLL not found: {aotDllName}");
                    continue;
                }

                var handle = YooAssets.LoadAssetAsync<TextAsset>(aotDllName);
                await handle.Task;
                byte[] dllBytes = ((TextAsset)handle.AssetObject).bytes;
                var err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
                Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. ret:{err}");
                handle.Release();
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async void Initialize()
        {
            try
            {
                await InitializeGame();
            }
            catch (Exception e)
            {
                await MessageBox.Show("错误", $"初始化失败：{e.Message}", no: null);
                Application.Quit();
            }
        }

        private async UniTask InitializeGame()
        {
            versionText.text = string.Empty;
            while (true)
            {
                try
                {
                    updateStatusText.text = "初始化中...";
                    downloadProgressBar.gameObject.SetActive(false);

                    YooAssets.Destroy();

                    // 初始化资源系统
                    YooAssets.Initialize();

                    // 创建默认的资源包
                    // 如果资源包不存在，则创建一个新的资源包
                    var package = YooAssets.TryGetPackage(packageName) ?? YooAssets.CreatePackage(packageName);

                    // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
                    YooAssets.SetDefaultPackage(package);

                    // 使用提取出的通用初始化函数
                    var packageInitCallbacks = new PackageInitializationCallbacks
                    {
                        OnStatusUpdate = status => updateStatusText.text = status,
                        OnVersionUpdate = version => versionText.text = $"v{Application.version}.{version}",
                        OnDownloadPrompt = async (count, size) =>
                            await MessageBox.Show("提示",
                                $"需要下载{count}个文件，总大小{size / 1024 / 1024}MB，是否开始下载？",
                                "下载"),
                        OnDownloadProgress = data =>
                        {
                            if (updateStatusText != null)
                            {
                                updateStatusText.text =
                                    $"正在下载第{data.CurrentDownloadCount}/{data.TotalDownloadCount}个文件（{data.CurrentDownloadBytes / 1024 / 1024}MB/{data.TotalDownloadBytes / 1024 / 1024}MB）";
                            }

                            if (downloadProgressText != null)
                            {
                                downloadProgressText.text = $"{Mathf.RoundToInt(data.Progress * 100)}%";
                            }

                            if (downloadProgressBar != null)
                            {
                                downloadProgressBar.value = data.Progress;
                            }
                        },
                        OnDownloadStart = () =>
                        {
                            downloadProgressBar.gameObject.SetActive(true);
                            updateStatusText.text = "下载中...";
                            downloadProgressText.text = "";
                            downloadProgressBar.value = 0f;
                        },
                        OnDownloadComplete = () =>
                        {
                            if (updateStatusText != null)
                                updateStatusText.text = "下载完成，正在加载...";
                            if (downloadProgressText != null)
                                downloadProgressText.text = "100%";
                            if (downloadProgressBar != null)
                                downloadProgressBar.value = 1f;
                        },
                        OnError = async error => await MessageBox.Show("警告", error, no: null)
                    };

                    bool success = await InitializePackageWithCallbacks(package, packageInitCallbacks);
                    if (!success)
                    {
                        continue; // Retry the loop
                    }

                    // 先补充元数据
                    updateStatusText.text = "正在加载代码...";
                    await LoadMetadataForAOTAssemblies();
                    // 设置动态密钥
                    updateStatusText.text = "正在解密资源...";
                    await SetUpDynamicSecret();

                    //进热更场景
                    updateStatusText.text = "正在加载场景...";
                    await LoadSceneWithProgress(package, selectedHotScene);

                    // 加载热更DLL
                    updateStatusText.text = "正在加载代码...";
#if UNITY_EDITOR
                    // 编辑器下直接从文件系统加载，不能读加密的，加密的只有真机才可以运行
                    var hotUpdateDllBytes = await System.IO.File.ReadAllBytesAsync($"Library/ScriptAssemblies/{hotCodeName}");
#else
                    var dllHandle =
                        package.LoadAssetAsync<TextAsset>($"Assets/HotUpdate/Compiled/{hotCodeName}.bytes");
                    await dllHandle.Task;
                    TextAsset hotUpdateDllAsset = (TextAsset)dllHandle.AssetObject;
                    var hotUpdateDllBytes = hotUpdateDllAsset.bytes;
                    dllHandle.Release();
#endif


                    Assembly hotUpdateAss = Assembly.Load(hotUpdateDllBytes);
                    await LoadHotCode(hotUpdateAss);

                    // 热更代码执行成功后，卸载之前的场景
                    await UnloadPreviousScene();

                    // If we reach here, initialization was successful, break out of the retry loop
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Initialization failed with exception: {ex}");
                    await MessageBox.Show("错误", $"初始化过程中发生异常：{ex.Message}");
                    // Continue the loop to retry
                }
            }
        }

        private Scene? _previousScene; // 存储之前的场景用于后续卸载

        private async UniTask LoadSceneWithProgress(ResourcePackage package, string sceneName)
        {
            // 显示进度UI
            downloadProgressBar.gameObject.SetActive(true);
            updateStatusText.text = "正在加载场景...";
            downloadProgressText.text = "0%";
            downloadProgressBar.value = 0f;

            // 保存当前活动场景，用于后续卸载
            _previousScene = SceneManager.GetActiveScene();

            // 使用Additive模式加载新场景
            var handle = package.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            // 更新加载进度
            while (!handle.IsDone)
            {
                float progress = handle.Progress;

                // 更新UI显示
                downloadProgressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
                downloadProgressBar.value = progress;

                await UniTask.DelayFrame(1);
            }

            // 等待场景加载完成
            await handle.Task;

            if (handle.Status == EOperationStatus.Succeed)
            {
                updateStatusText.text = "场景加载完成";
                downloadProgressText.text = "100%";
                downloadProgressBar.value = 1f;

                // 设置新场景为活动场景
                var newScene = handle.SceneObject;
                SceneManager.SetActiveScene(newScene);

                // 等待一帧确保场景已经设置为活动状态
                await UniTask.DelayFrame(1);

                // 隐藏进度UI
                downloadProgressBar.gameObject.SetActive(false);
                downloadProgressText.text = "";
                updateStatusText.text = "";
            }
            else
            {
                // 加载失败处理
                await MessageBox.Show("错误", $"场景加载失败: {handle.LastError}", ok: "重试");
                throw new Exception($"Failed to load scene: {handle.LastError}");
            }
        }

        private async UniTask UnloadPreviousScene()
        {
            if (_previousScene.HasValue && !string.IsNullOrEmpty(_previousScene.Value.name))
            {
                // 静默卸载之前的场景，不显示进度
                var unloadOperation = SceneManager.UnloadSceneAsync(_previousScene.Value);
                if (unloadOperation != null)
                {
                    await unloadOperation;
                }

                _previousScene = null;
            }
        }

        private async UniTask LoadHotCode(Assembly hotUpdateAss)
        {
            Type type = hotUpdateAss.GetType(hotUpdateClassName);
            if (type == null)
            {
                await MessageBox.Show("错误", "代码异常，请联系客服", ok: null);
                Application.Quit();
                return;
            }

            var method = type.GetMethod(hotUpdateMethodName, BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                await MessageBox.Show("错误", "代码异常，请联系客服", ok: null);
                Application.Quit();
                return;
            }

            try
            {
                // Check if method returns UniTask or ValueTask or Task and handle accordingly
                if (method.ReturnType == typeof(UniTask))
                {
                    var task = (UniTask)method.Invoke(null, null);
                    await task;
                }
                else if (method.ReturnType == typeof(ValueTask))
                {
                    var task = (ValueTask)method.Invoke(null, null);
                    await task;
                }
                else if (method.ReturnType == typeof(Task))
                {
                    var task = (Task)method.Invoke(null, null);
                    await task;
                }
                else
                {
                    // Void method
                    method.Invoke(null, null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to invoke hot update method {hotUpdateMethodName}: {e}");
                await MessageBox.Show("错误", $"函数调用失败：{e.Message}", ok: "退出", no: null);
                Application.Quit();
            }
        }

        /// <summary>
        /// 资源包初始化回调接口
        /// </summary>
        public class PackageInitializationCallbacks
        {
            public Action<string> OnStatusUpdate { get; set; }
            public Action<string> OnVersionUpdate { get; set; }
            public Func<int, long, UniTask<bool>> OnDownloadPrompt { get; set; }
            public Action<DownloadStatus> OnDownloadProgress { get; set; }
            public Action OnDownloadStart { get; set; }
            public Action OnDownloadComplete { get; set; }
            public Func<string, UniTask> OnError { get; set; }
        }

        /// <summary>
        /// 下载状态信息
        /// </summary>
        public class DownloadStatus
        {
            public int CurrentDownloadCount { get; set; }
            public int TotalDownloadCount { get; set; }
            public long CurrentDownloadBytes { get; set; }
            public long TotalDownloadBytes { get; set; }
            public float Progress { get; set; }
        }

        /// <summary>
        /// 通用的资源包初始化函数
        /// </summary>
        /// <param name="package">要初始化的资源包</param>
        /// <param name="callbacks">各种回调函数</param>
        /// <returns>是否初始化成功</returns>
        public async UniTask<bool> InitializePackageWithCallbacks(ResourcePackage package,
            PackageInitializationCallbacks callbacks)
        {
            try
            {
                // 1. 初始化资源包
                callbacks.OnStatusUpdate?.Invoke("正在初始化资源包...");
                InitializationOperation initOperation;

#if UNITY_EDITOR
                if (useEditorDevMode)
                {
                    // 如果是编辑器模拟模式，使用模拟构建的资源包
                    var buildResult = EditorSimulateModeHelper.SimulateBuild(package.PackageName);
                    var packageRoot = buildResult.PackageRootDirectory;
                    var editorFileSystemParams =
                        FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                    var initParameters = new EditorSimulateModeParameters
                    {
                        EditorFileSystemParameters = editorFileSystemParams
                    };
                    initOperation = package.InitializeAsync(initParameters);
                }
                else
#endif
                {
                    var server =
                        $"{(defaultHostServer.EndsWith("/") ? defaultHostServer : defaultHostServer + "/")}{GetPlatform()}/{package.PackageName}";
                    var effectiveFallbackServer = useDefaultAsFallback ? server : fallbackHostServer;
                    var remoteServices = new RemoteServices(server, effectiveFallbackServer);
                    var decryption = new FileStreamDecryption();
                    var cacheFileSystemParams =
                        FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices, decryption);
                    var initParameters = new HostPlayModeParameters
                    {
                        BuildinFileSystemParameters = null,
                        CacheFileSystemParameters = cacheFileSystemParams
                    };
                    initOperation = package.InitializeAsync(initParameters);
                }

                await initOperation.Task;

                if (initOperation.Status != EOperationStatus.Succeed)
                {
                    if (callbacks.OnError != null)
                        await callbacks.OnError($"资源初始化失败：{initOperation.Error}");
                    return false;
                }

                // 2. 获取包版本
                callbacks.OnStatusUpdate?.Invoke("正在获取资源包版本...");
                var operation = package.RequestPackageVersionAsync();
                await operation.Task;

                string packageVersion;
                if (operation.Status == EOperationStatus.Succeed)
                {
                    packageVersion = operation.PackageVersion;
                    callbacks.OnVersionUpdate?.Invoke(packageVersion);
                }
                else
                {
                    if (callbacks.OnError != null)
                        await callbacks.OnError($"获取资源版本失败：{operation.Error}");
                    return false;
                }

                // 3. 更新资源清单
                callbacks.OnStatusUpdate?.Invoke("正在更新资源清单...");
                var updateOperation = package.UpdatePackageManifestAsync(packageVersion);
                await updateOperation.Task;

                if (updateOperation.Status != EOperationStatus.Succeed)
                {
                    if (callbacks.OnError != null)
                        await callbacks.OnError($"更新资源清单失败：{updateOperation.Error}");
                    return false;
                }

                await UniTask.WaitForSeconds(0.5f);

                // 4. 检查并下载资源
                callbacks.OnStatusUpdate?.Invoke("正在检查需要下载的资源...");
                int downloadingMaxNum = 10;
                int failedTryAgain = 3;
                var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

                if (downloader.TotalDownloadCount != 0)
                {
                    // 询问用户是否下载
                    bool shouldDownload = true;
                    if (callbacks.OnDownloadPrompt != null)
                    {
                        shouldDownload = await callbacks.OnDownloadPrompt(downloader.TotalDownloadCount,
                            downloader.TotalDownloadBytes);
                    }

                    if (!shouldDownload)
                    {
                        return false;
                    }

                    // 开始下载
                    callbacks.OnDownloadStart?.Invoke();

                    await UniTask.DelayFrame(1);

                    downloader.DownloadUpdateCallback += data =>
                    {
                        var status = new DownloadStatus
                        {
                            CurrentDownloadCount = data.CurrentDownloadCount,
                            TotalDownloadCount = downloader.TotalDownloadCount,
                            CurrentDownloadBytes = data.CurrentDownloadBytes,
                            TotalDownloadBytes = downloader.TotalDownloadBytes,
                            Progress = data.Progress
                        };
                        callbacks.OnDownloadProgress?.Invoke(status);
                    };

                    downloader.BeginDownload();
                    await downloader.Task;

                    if (downloader.Status != EOperationStatus.Succeed)
                    {
                        if (callbacks.OnError != null)
                            await callbacks.OnError($"下载资源失败：{downloader.Error}");
                        return false;
                    }

                    await UniTask.DelayFrame(1);
                    callbacks.OnDownloadComplete?.Invoke();
                }
                else
                {
                    callbacks.OnStatusUpdate?.Invoke("无需下载资源");
                }

                callbacks.OnStatusUpdate?.Invoke("资源包初始化完成");
                return true;
            }
            catch (Exception ex)
            {
                if (callbacks.OnError != null)
                    await callbacks.OnError($"初始化过程中发生异常：{ex.Message}");
                return false;
            }
        }

        private class RemoteServices : IRemoteServices
        {
            private readonly string _defaultHostServer;
            private readonly string _fallbackHostServer;

            public RemoteServices(string defaultHostServer, string fallbackHostServer)
            {
                _defaultHostServer = defaultHostServer;
                _fallbackHostServer = fallbackHostServer;
            }

            public string GetRemoteMainURL(string fileName)
            {
                return $"{_defaultHostServer}/{fileName}";
            }

            public string GetRemoteFallbackURL(string fileName)
            {
                return $"{_fallbackHostServer}/{fileName}";
            }
        }
    }
}