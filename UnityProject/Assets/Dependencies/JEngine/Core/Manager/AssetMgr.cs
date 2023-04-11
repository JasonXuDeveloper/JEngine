//
// AssetMgr.cs
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

using System;
using YooAsset;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using UpdateMode = JEngine.Core.Updater.UpdateMode;

namespace JEngine.Core
{
    public static partial class AssetMgr
    {
        /// <summary>
        /// 真机模式
        /// </summary>
        public static bool RuntimeMode => Updater.Mode != UpdateMode.Simulate;

        /// <summary>
        /// 构造函数
        /// </summary>
        static AssetMgr()
        {
            // 初始化资源系统
            YooAssets.Initialize();
        }

        /// <summary>
        /// 获取平台
        /// </summary>
        public static string GetPlatform
        {
            get
            {
#if UNITY_ANDROID
                return "Android";
#elif UNITY_IOS
                return "IOS";
#elif UNITY_STANDALONE_OSX
                return "Mac";
#elif UNITY_STANDALONE_WIN
                return "Windows";
#elif UNITY_WEBGL
                return "WebGL";
#else
                return Application.platform.ToString();
#endif
            }
        }

        /// <summary>
        /// 初始化资源包
        /// </summary>
        /// <param name="packageName"></param>
        private static async Task SetUpPackage(string packageName)
        {
            string resourceUrl = Updater.ResourceUrl;
            string fallbackUrl = Updater.FallbackUrl;
            // 更新URL
            string end = $"/{GetPlatform}/{packageName}";
            if (!resourceUrl.EndsWith(end)) resourceUrl = $"{resourceUrl}{end}";
            if (!fallbackUrl.EndsWith(end)) fallbackUrl = $"{fallbackUrl}{end}";
            // 创建默认的资源包
            var package = YooAssets.TryGetPackage(packageName) ?? YooAssets.CreatePackage(packageName);
            if (packageName == Updater.MainPackageName)
            {
                // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
                YooAssets.SetDefaultPackage(package);
            }

            if (package.InitializeStatus == EOperationStatus.Succeed)
            {
                return;
            }

            // 初始化
            InitializeParameters initParameters = Updater.Mode switch
            {
                UpdateMode.Simulate => new EditorSimulateModeParameters()
                {
                    SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(packageName)
                },
                UpdateMode.Standalone => new OfflinePlayModeParameters(),
                UpdateMode.Remote => new HostPlayModeParameters()
                {
                    QueryServices = new QueryStreamingAssetsFileServices(),
                    DefaultHostServer = resourceUrl,
                    FallbackHostServer = fallbackUrl
                },
                _ => null
            };

            await package.InitializeAsync(initParameters).Task;
        }

        /// <summary>
        /// 下载包
        /// </summary>
        /// <param name="packageName">包名</param>
        /// <param name="updater">回调事件</param>
        public static async Task UpdatePackage(string packageName, IUpdater updater = null)
        {
            // 检查资源包
            await SetUpPackage(packageName);
            // 释放UI
            MessageBox.Dispose();
            // 版本信息
            string version = $"v{Application.version}";

            //初始化热更
            void Init()
            {
                updater?.OnProgress(1);
                updater?.OnMessage("下载完成");
                // 直接调用热更完成
                updater?.OnUpdateFinish(true);
            }

            //远程资源
            if (Updater.Mode == UpdateMode.Remote)
            {
                // 获取资源包
                var package = YooAssets.GetPackage(packageName);
                // 检查版本
                var updatePackageVersionOp = package.UpdatePackageVersionAsync();
                await updatePackageVersionOp.Task;
                bool updatedManifest = false;
                string err = null;

                // 成功就检查资源清单
                if (updatePackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    version = updatePackageVersionOp.PackageVersion;
                    var updatePackageManifestOp = package.UpdatePackageManifestAsync(version);
                    await updatePackageManifestOp.Task;
                    updatedManifest = updatePackageManifestOp.Status == EOperationStatus.Succeed;
                    err = updatePackageManifestOp.Error;
                }

                // 有错误
                if (updatePackageVersionOp.Status != EOperationStatus.Succeed || !updatedManifest)
                {
                    err ??= updatePackageVersionOp.Error;
                    TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                    var mb = MessageBox.Show("错误", $"无法获取服务器资源信息：{err}", "返回", "退出");
                    mb.onComplete = ok =>
                    {
                        if (ok == MessageBox.EventId.Ok)
                        {
                            updater?.OnUpdateFinish(false);
                            tcs.SetResult(true);
                        }
                        else
                        {
                            Updater.Quit();
                        }
                    };
                    await tcs.Task;
                    return;
                }
            }

            // 资源版本UI
            updater?.OnVersion(version);


            if (Updater.Mode == UpdateMode.Remote)
            {
                int downloadingMaxNum = 10;
                int failedTryAgain = 3;
                int timeout = 60;
                var package = YooAssets.GetPackage(packageName);
                var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);

                //没有需要下载的资源
                if (downloader.TotalDownloadCount == 0)
                {
                    Init();
                }
                else
                {
                    //需要下载的文件总数和总大小
                    int totalDownloadCount = downloader.TotalDownloadCount;
                    long totalDownloadBytes = downloader.TotalDownloadBytes;

                    updater?.OnMessage($"需要更新, 大小: {Tools.GetDisplaySize(totalDownloadBytes)}");
                    var tips =
                        $"发现{totalDownloadCount}个资源有更新，总计需要下载 {Tools.GetDisplaySize(totalDownloadBytes)}";
                    TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                    var mb = MessageBox.Show("提示", tips, "下载", "返回");
                    mb.onComplete = async ok =>
                    {
                        if (ok == MessageBox.EventId.Ok)
                        {
                            downloader.OnDownloadProgressCallback = (_, currentDownloadCount,
                                __, currentDownloadBytes) =>
                            {
                                updater?.OnMessage(
                                    $"正在下载第{currentDownloadCount}/{totalDownloadCount}个文件，总进度{Tools.GetDisplaySize(currentDownloadBytes)}/{Tools.GetDisplaySize(totalDownloadBytes)}");
                                updater?.OnProgress(
                                    (float)Math.Round((double)currentDownloadBytes / totalDownloadBytes, 2));
                            };
                            downloader.OnDownloadErrorCallback = (file, err) =>
                            {
                                updater?.OnMessage($"下载{file}失败：{err}");
                                Log.PrintError($"下载{file}失败：{err}");
                            };

                            //开启下载
                            downloader.BeginDownload();
                            await downloader.Task;

                            //检测下载结果
                            if (downloader.Status == EOperationStatus.Succeed)
                            {
                                //下载成功
                                Init();
                                tcs.SetResult(true);
                            }
                            else
                            {
                                //下载失败
                                var mb2 = MessageBox.Show("错误", $"下载失败：{downloader.Error}", "返回", "退出");
                                updater?.OnUpdateFinish(false);
                                mb2.onComplete = okk =>
                                {
                                    if (okk == MessageBox.EventId.Ok)
                                    {
                                        updater?.OnUpdateFinish(false);
                                        tcs.SetResult(true);
                                    }
                                    else
                                    {
                                        Updater.Quit();
                                    }
                                };
                            }
                        }
                        else
                        {
                            updater?.OnUpdateFinish(false);
                            tcs.SetResult(true);
                        }
                    };
                    await tcs.Task;
                }
            }
            else
            {
                Init();
            }
        }

        private static ResourcePackage GetPackage(string packageName)
        {
            if (packageName == null) packageName = Updater.MainPackageName;
            return YooAssets.GetPackage(packageName);
        }

        public static Object Load(string path, Type type) => Load(path, Updater.MainPackageName, type, out _);

        public static Object Load(string path, Type type, out AssetOperationHandle handle) =>
            Load(path, Updater.MainPackageName, type, out handle);

        public static Object Load(string path, string package, Type type) => Load(path, package, type, out _);

        public static Object Load(string path, string package, Type type, out AssetOperationHandle handle)
        {
            handle = GetPackage(package).LoadAssetSync(path, type);
            return handle.AssetObject;
        }

        public static T Load<T>(string path)
            where T : Object => Load<T>(path, Updater.MainPackageName, out _);

        public static T Load<T>(string path, out AssetOperationHandle handle)
            where T : Object => Load<T>(path, Updater.MainPackageName, out handle);

        public static T Load<T>(string path, string package)
            where T : Object => Load<T>(path, package, out _);

        public static T Load<T>(string path, string package, out AssetOperationHandle handle)
            where T : Object
        {
            handle = GetPackage(package).LoadAssetSync<T>(path);
            return handle.AssetObject as T;
        }

        public static async Task<T> LoadAsync<T>(string path)
            where T : Object => await LoadAsync<T>(path, Updater.MainPackageName);

        public static async Task<T> LoadAsync<T>(string path, string package)
            where T : Object
        {
            var handle = GetPackage(package).LoadAssetAsync<T>(path);
            await handle.Task;
            return handle.AssetObject as T;
        }

        public static async Task<Object> LoadAsync(string path, Type type) 
            => await LoadAsync(path, Updater.MainPackageName, type);

        public static async Task<Object> LoadAsync(string path, string package, Type type) 
        {
            var handle = GetPackage(package).LoadAssetAsync(path, type);
            await handle.Task;
            return handle.AssetObject;
        }

        public static async Task<(T, AssetOperationHandle)> LoadAsyncWithHandle<T>(string path)
            where T : Object => await LoadAsyncWithHandle<T>(path, Updater.MainPackageName);

        public static async Task<(T, AssetOperationHandle)> LoadAsyncWithHandle<T>(string path, string package)
            where T : Object
        {
            var handle = GetPackage(package).LoadAssetAsync<T>(path);
            await handle.Task;
            return (handle.AssetObject as T, handle);
        }
        
        public static async Task<(Object, AssetOperationHandle)> LoadAsyncWithHandle(string path, Type type) 
            => await LoadAsyncWithHandle(path, Updater.MainPackageName, type);
        
        public static async Task<(Object, AssetOperationHandle)> LoadAsyncWithHandle(string path, string package, Type type)
        {
            var handle = GetPackage(package).LoadAssetAsync(path, type);
            await handle.Task;
            return (handle.AssetObject, handle);
        }

        public static void LoadScene(string path, bool additive = false, string package = null)
        {
            SceneOperationHandle handle = GetPackage(package)
                .LoadSceneAsync(path, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            handle.Task.Wait();
            RemoveUnusedAssets();
        }

        public static async Task LoadSceneAsync(string path, bool additive = false, string package = null)
        {
            SceneOperationHandle handle = GetPackage(package)
                .LoadSceneAsync(path, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            await handle.Task;
            RemoveUnusedAssets();
        }

        public static RawFileOperationHandle LoadRaw(string path) => LoadRaw(path, Updater.MainPackageName);

        public static RawFileOperationHandle LoadRaw(string path, string package)
        {
            RawFileOperationHandle handle = GetPackage(package).LoadRawFileSync(path);
            return handle;
        }

        public static Task<RawFileOperationHandle> LoadRawAsync(string path) =>
            LoadRawAsync(path, Updater.MainPackageName);

        public static async Task<RawFileOperationHandle> LoadRawAsync(string path, string package)
        {
            RawFileOperationHandle handle = GetPackage(package).LoadRawFileAsync(path);
            await handle.Task;
            return handle;
        }

        public static void RemoveUnusedAssets(string package = null)
        {
            GetPackage(package)?.UnloadUnusedAssets();
        }
    }
}