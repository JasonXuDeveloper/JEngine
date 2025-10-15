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
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JEngine.Core.Encrypt;
using JEngine.Core.Misc;
using JEngine.Core.Update;
using Nino.Core;
using Obfuz;
using Obfuz.EncryptionVM;
using UnityEngine;
using YooAsset;
using Application = UnityEngine.Application;
using TMPro;
using UnityEngine.UI;

namespace JEngine.Core
{
    public partial class Bootstrap : MonoBehaviour
    {
        [Header("Server Settings")] public string defaultHostServer = "http://127.0.0.1/";

        [HideInInspector] public bool useDefaultAsFallback = true;

        public string fallbackHostServer = "http://127.0.0.1/";

        public bool appendTimeTicks = true;

        [Header("Asset Settings")] public TargetPlatform targetPlatform = TargetPlatform.Regular;

        public string packageName = "main";

        public string hotCodeName = "HotUpdate.Code.dll";

        public string selectedHotScene = "Assets/HotUpdate/Main/Scene/Game.unity";

        public string hotUpdateClassName = "HotUpdate.Code.EntryPoint";

        public string hotUpdateMethodName = "RunGame";

        [Header("Security Settings")]
        public string dynamicSecretKeyPath = "Assets/HotUpdate/Obfuz/DynamicSecretKey.bytes";

        public string aotDllListFilePath = "Assets/HotUpdate/Compiled/AOT.bytes";

        public EncryptionOption encryptionOption = EncryptionOption.Xor;

        [Header("UI Settings")] public TextMeshProUGUI versionText;

        public TextMeshProUGUI updateStatusText;

        public TextMeshProUGUI downloadProgressText;

        public Slider downloadProgressBar;

        public Button startButton;

#if UNITY_EDITOR
        [Header("Development Settings")] [HideInInspector]
        public bool useEditorDevMode = true;
#endif

        public static string GetPlatform()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
#else
            return Application.platform switch
            {
                RuntimePlatform.WindowsPlayer => "StandaloneWindows64",
                RuntimePlatform.OSXPlayer => "StandaloneOSX",
                RuntimePlatform.LinuxPlayer => "StandaloneLinux64",
                RuntimePlatform.Android => "Android",
                RuntimePlatform.IPhonePlayer => "iOS",
                RuntimePlatform.WebGLPlayer => "WebGL",
                RuntimePlatform.WSAPlayerX64 or RuntimePlatform.WSAPlayerX86 or RuntimePlatform.WSAPlayerARM => "WSAPlayer",
                RuntimePlatform.tvOS => "tvOS",
                RuntimePlatform.PS4 => "PS4",
                RuntimePlatform.PS5 => "PS5",
                RuntimePlatform.XboxOne => "XboxOne",
                RuntimePlatform.GameCoreXboxSeries => "GameCoreXboxSeries",
                RuntimePlatform.Switch => "Switch",
                RuntimePlatform.VisionOS => "VisionOS",
                _ => Application.platform.ToString()
            };
#endif
        }

        private static Bootstrap _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void SetUpStaticSecretKey()
        {
            Debug.Log("SetUpStaticSecret begin");
            string keyPath = "Obfuz/StaticSecretKey";

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
                DestroyImmediate(_instance);
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            startButton?.gameObject.SetActive(true);
            downloadProgressBar?.gameObject.SetActive(false);
            startButton?.onClick.AddListener(() =>
            {
                startButton?.gameObject.SetActive(false);
                downloadProgressBar?.gameObject.SetActive(true);
                Initialize();
            });
        }

        private async void Initialize()
        {
            try
            {
                await InitializeGame();
            }
            catch (Exception e)
            {
                await MessageBox.Show("Error", $"Initialization failed: {e.Message}", no: null);
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
                    updateStatusText.text = "Initializing...";
                    downloadProgressBar.gameObject.SetActive(false);

                    YooAssets.Destroy();

                    // Initialize asset system
                    YooAssets.Initialize();

                    // Create default resource package
                    var package = CreateOrGetPackage(packageName);

                    // Set this resource package as the default resource package. You can use YooAssets related loading interfaces to load the contents of this resource package.
                    YooAssets.SetDefaultPackage(package);

                    // Use the extracted common initialization function
                    var packageInitCallbacks = new PackageInitializationCallbacks
                    {
                        OnStatusUpdate = status => updateStatusText.text = GetStatusText(status),
                        OnVersionUpdate = version => versionText.text = $"v{Application.version}.{version}",
                        OnDownloadPrompt = async (count, size) =>
                            await MessageBox.Show("Notice",
                                $"Need to download {count} files, total size {size / 1024f / 1024f:F2}MB. Start download?",
                                "Download"),
                        OnDownloadProgress = data =>
                        {
                            if (updateStatusText != null)
                            {
                                updateStatusText.text =
                                    $"Downloading file {data.CurrentDownloadCount}/{data.TotalDownloadCount} ({data.CurrentDownloadBytes / 1024f / 1024f:F2}MB/{data.TotalDownloadBytes / 1024f / 1024f:F2}MB)";
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
                            updateStatusText.text = "Downloading...";
                            downloadProgressText.text = "";
                            downloadProgressBar.value = 0f;
                        },
                        OnDownloadComplete = () =>
                        {
                            if (updateStatusText != null)
                                updateStatusText.text = "Download completed, loading...";
                            if (downloadProgressText != null)
                                downloadProgressText.text = "100%";
                            if (downloadProgressBar != null)
                                downloadProgressBar.value = 1f;
                        },
                        OnError = async error => await MessageBox.Show("Warning", error.Message, no: null)
                    };

                    bool success = await UpdatePackage(package, packageInitCallbacks, encryptionOption);
                    if (!success)
                    {
                        continue; // Retry the loop
                    }

                    // First supplement metadata
                    updateStatusText.text = "Loading code...";
                    await LoadMetadataForAOTAssemblies();
                    // Set dynamic key
                    updateStatusText.text = "Decrypting resources...";
                    await SetUpDynamicSecret();

                    // Enter hot update scene
                    updateStatusText.text = "Loading scene...";
                    var sceneLoadCallbacks = new SceneLoadCallbacks
                    {
                        OnStatusUpdate = status => updateStatusText.text = GetSceneLoadStatusText(status),
                        OnProgressUpdate = progress =>
                        {
                            downloadProgressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
                            downloadProgressBar.value = progress;
                        },
                        OnError = async exception =>
                        {
                            await MessageBox.Show("Error", $"Scene loading failed: {exception.Message}",
                                ok: "Retry");
                        }
                    };
                    downloadProgressBar.gameObject.SetActive(true);
                    await LoadHotUpdateScene(package, selectedHotScene, sceneLoadCallbacks);

                    // Load hot update DLL
                    updateStatusText.text = "Loading code...";
#if UNITY_EDITOR
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    Assembly hotUpdateAss = null;
                    foreach (var assembly in assemblies)
                    {
                        if (assembly.GetName().Name == Path.GetFileNameWithoutExtension(hotCodeName))
                        {
                            hotUpdateAss = assembly;
                            break;
                        }
                    }
                    
                    if (hotUpdateAss == null)
                    {
                        throw new Exception($"Hot update assembly {hotCodeName} not found in editor mode.");
                    }
#else
                    var dllHandle =
                        package.LoadAssetAsync<TextAsset>($"Assets/HotUpdate/Compiled/{hotCodeName}.bytes");
                    await dllHandle.Task;
                    TextAsset hotUpdateDllAsset = dllHandle.GetAssetObject<TextAsset>();
                    var hotUpdateDllBytes = hotUpdateDllAsset.bytes;
                    dllHandle.Release();
                    Assembly hotUpdateAss = Assembly.Load(hotUpdateDllBytes);
#endif

                    await LoadHotCode(hotUpdateAss);

                    // If we reach here, initialization was successful, break out of the retry loop
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Initialization failed with exception: {ex}");
                    await MessageBox.Show("Error", $"Exception occurred during initialization: {ex.Message}");
                    // Continue the loop to retry
                }
            }
        }

        private async UniTask LoadHotCode(Assembly hotUpdateAss)
        {
            Type type = hotUpdateAss.GetType(hotUpdateClassName);
            if (type == null)
            {
                await MessageBox.Show("Error", "Code exception, please contact customer service", ok: null);
                Application.Quit();
                return;
            }

            var method = type.GetMethod(hotUpdateMethodName, BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                await MessageBox.Show("Error", "Code exception, please contact customer service", ok: null);
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
                await MessageBox.Show("Error", $"Function call failed: {e.Message}", ok: "Exit", no: null);
                Application.Quit();
            }
        }

        private async UniTask<bool> UpdatePackageImpl(ResourcePackage package,
            PackageInitializationCallbacks callbacks, EncryptionOption option)
        {
            try
            {
                // 1. Initialize resource package
                callbacks.OnStatusUpdate?.Invoke(PackageInitializationStatus.InitializingPackage);
                InitializationOperation initOperation;

#if UNITY_EDITOR
                if (useEditorDevMode)
                {
                    // If it's editor simulation mode, use simulated built resource package
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
                        $"{defaultHostServer}{(defaultHostServer.EndsWith("/") ? "" : "/")}{GetPlatform()}/{package.PackageName}";
                    var effectiveFallbackServer = useDefaultAsFallback ? server : fallbackHostServer;
                    var remoteServices = new RemoteServices(server, effectiveFallbackServer);
                    var bundleConfig = EncryptionMapping.GetBundleConfig(option);
                    var manifestRestoration = bundleConfig.ManifestEncryptionConfig.Decryption;
                    InitializeParameters initParameters = null;
                    switch (targetPlatform)
                    {
                        case TargetPlatform.Regular:
                        {
#if UNITY_WEBGL
                            YooAssets.SetOperationSystemMaxTimeSlice(100);
                            var webRemoteFileSystem =
                                FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(remoteServices,
                                    bundleConfig.WebDecryption);
                            webRemoteFileSystem.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES,
                                manifestRestoration);

                            FileSystemParameters webServerFileSystem =
                                FileSystemParameters.CreateDefaultWebServerFileSystemParameters(bundleConfig
                                    .WebDecryption);
                            webServerFileSystem.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES,
                                manifestRestoration);
                            if (package.PackageName != packageName)
                            {
                                webServerFileSystem = null;
                            }

                            initParameters = new WebPlayModeParameters
                            {
                                WebRemoteFileSystemParameters = webRemoteFileSystem,
                                WebServerFileSystemParameters = webServerFileSystem,
                                WebGLForceSyncLoadAsset = true
                            };

#else
                            var cacheFileSystemParams =
                                FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices,
                                    bundleConfig.Decryption);
                            cacheFileSystemParams.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES,
                                manifestRestoration);

                            var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(
                                bundleConfig.Decryption);
                            buildinFileSystemParams?.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES,
                                manifestRestoration);
                            if (package.PackageName != packageName)
                            {
                                buildinFileSystemParams = null;
                            }

                            initParameters = new HostPlayModeParameters
                            {
                                BuildinFileSystemParameters = buildinFileSystemParams,
                                CacheFileSystemParameters = cacheFileSystemParams
                            };
#endif
                            break;
                        }
                        case TargetPlatform.WeChat:
                        {
                            YooAssets.SetOperationSystemMaxTimeSlice(100);
#if UNITY_WEBGL && WEIXINMINIGAME
                            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE/yoo";
                            var wechatFileSystemParams =
                                WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices,
                                    bundleConfig.WebDecryption);
                            wechatFileSystemParams.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES,
                                manifestRestoration);

                            initParameters = new WebPlayModeParameters
                            {
                                WebServerFileSystemParameters = wechatFileSystemParams,
                                WebGLForceSyncLoadAsset = true
                            };
#endif

                            break;
                        }
                        case TargetPlatform.Douyin:
                        {
                            YooAssets.SetOperationSystemMaxTimeSlice(100);
#if UNITY_WEBGL && DOUYINMINIGAME
                            var webRemoteFileSystem = TiktokFileSystemCreater.CreateFileSystemParameters("yoo",
                                remoteServices, bundleConfig.WebDecryption);
                            webRemoteFileSystem.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES,
                                manifestRestoration);
                            
                            initParameters = new WebPlayModeParameters
                            {
                                WebServerFileSystemParameters = webRemoteFileSystem,
                                WebGLForceSyncLoadAsset = true
                            };
#endif
                            break;
                        }
                        case TargetPlatform.Alipay:
                        {
                            YooAssets.SetOperationSystemMaxTimeSlice(100);
#if UNITY_WEBGL && UNITY_ALIMINIGAME
                            var webRemoteFileSystem = AlipayFileSystemCreater.CreateFileSystemParameters(
                                "yoo", remoteServices, bundleConfig.WebDecryption);
                            webRemoteFileSystem.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES,
                                manifestRestoration);
                            
                            initParameters = new WebPlayModeParameters
                            {
                                WebServerFileSystemParameters = webRemoteFileSystem,
                                WebGLForceSyncLoadAsset = true
                            };
#endif
                            break;
                        }
                        case TargetPlatform.TapTap:
                        {
                            YooAssets.SetOperationSystemMaxTimeSlice(100);
#if UNITY_WEBGL && TAPMINIGAME
                            var webRemoteFileSystem = TaptapFileSystemCreater.CreateFileSystemParameters(
                                "yoo", remoteServices, bundleConfig.WebDecryption);
                            webRemoteFileSystem.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES,
                                manifestRestoration);

                            initParameters = new WebPlayModeParameters
                            {
                                WebServerFileSystemParameters = webRemoteFileSystem,
                                WebGLForceSyncLoadAsset = true
                            };
#endif
                            break;
                        }
                        default:
                            throw new NotSupportedException($"Target platform {targetPlatform} is not supported yet.");
                    }

                    if (initParameters == null)
                        throw new InvalidOperationException(
                            $"Failed to create initialization parameters for package platform {targetPlatform} (current build target platform: {Application.platform})");

                    initOperation = package.InitializeAsync(initParameters);
                }

                await initOperation.Task;

                if (initOperation.Status != EOperationStatus.Succeed)
                {
                    if (callbacks.OnError != null)
                        await callbacks.OnError(new Exception(initOperation.Error));
                    return false;
                }

                // 2. Get package version
                callbacks.OnStatusUpdate?.Invoke(PackageInitializationStatus.GettingVersion);
                var operation = package.RequestPackageVersionAsync(appendTimeTicks: appendTimeTicks);
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
                        await callbacks.OnError(new Exception(operation.Error));
                    return false;
                }

                // 3. Update resource manifest
                callbacks.OnStatusUpdate?.Invoke(PackageInitializationStatus.UpdatingManifest);
                var updateOperation = package.UpdatePackageManifestAsync(packageVersion);
                await updateOperation.Task;

                if (updateOperation.Status != EOperationStatus.Succeed)
                {
                    if (callbacks.OnError != null)
                        await callbacks.OnError(new Exception(updateOperation.Error));
                    return false;
                }

                await UniTask.WaitForSeconds(0.5f);

                // 4. Check and download resources
                callbacks.OnStatusUpdate?.Invoke(PackageInitializationStatus.CheckingUpdate);
                int downloadingMaxNum = 10;
                int failedTryAgain = 3;
                var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

                if (downloader.TotalDownloadCount != 0)
                {
                    // Ask user whether to download
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

                    // Start download
                    callbacks.OnStatusUpdate?.Invoke(PackageInitializationStatus.DownloadingResources);
                    callbacks.OnDownloadStart?.Invoke();

                    await UniTask.DelayFrame(1);

                    downloader.DownloadUpdateCallback += callbacks.OnDownloadProgress;
                    downloader.BeginDownload();
                    await downloader.Task;

                    if (downloader.Status != EOperationStatus.Succeed)
                    {
                        if (callbacks.OnError != null)
                            await callbacks.OnError(new Exception(downloader.Error));
                        return false;
                    }

                    await UniTask.DelayFrame(1);
                    callbacks.OnDownloadComplete?.Invoke();
                }
                else
                {
                    callbacks.OnStatusUpdate?.Invoke(PackageInitializationStatus.Completed);
                }

                callbacks.OnStatusUpdate?.Invoke(PackageInitializationStatus.Completed);
                return true;
            }
            catch (Exception ex)
            {
                if (callbacks.OnError != null)
                    await callbacks.OnError(ex);
                return false;
            }
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