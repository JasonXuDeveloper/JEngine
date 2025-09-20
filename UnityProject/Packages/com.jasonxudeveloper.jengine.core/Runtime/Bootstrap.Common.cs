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

using System;
using Cysharp.Threading.Tasks;
using JEngine.Core.Update;
using UnityEngine.SceneManagement;
using YooAsset;

namespace JEngine.Core
{
    public partial class Bootstrap
    {
        /// <summary>
        /// 加载热更新场景的通用函数
        /// </summary>
        /// <param name="package"></param>
        /// <param name="sceneName"></param>
        /// <param name="callbacks"></param>
        /// <param name="loadMode"></param>
        /// <exception cref="Exception"></exception>
        public static async UniTask<SceneHandle> LoadHotUpdateScene(ResourcePackage package, string sceneName,
            SceneLoadCallbacks callbacks,
            LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            // 调用状态更新回调
            callbacks.OnStatusUpdate?.Invoke("正在加载场景...");
            callbacks.OnProgressUpdate?.Invoke(0f);

            // 保存当前活动场景，用于后续卸载
            var previousSceneName = SceneManager.GetActiveScene().name;

            try
            {
                // 加载新场景
                var handle = package.LoadSceneAsync(sceneName, loadMode, suspendLoad: true);

                // 更新加载进度
                while (handle.Progress < 0.9f)
                {
                    float progress = handle.Progress;

                    // 调用进度更新回调
                    callbacks.OnProgressUpdate?.Invoke(progress);

                    await UniTask.DelayFrame(1);
                }

                // 调用完成状态回调
                callbacks.OnStatusUpdate?.Invoke("场景加载完成");
                callbacks.OnProgressUpdate?.Invoke(1f);

                await UniTask.DelayFrame(1); // 等待一帧，确保场景切换完成
                handle.UnSuspend();
                await handle.Task;

                if (handle.Status != EOperationStatus.Succeed)
                {
                    throw new Exception(handle.LastError);
                }

                // 调用完成回调
                return handle;
            }
            catch (Exception e)
            {
                // 调用错误回调
                await callbacks.OnError(e);
                // 切换回上一个场景
                await SceneManager.LoadSceneAsync(previousSceneName);

                return null;
            }
        }

        /// <summary>
        /// 创建或获取一个资源包
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static ResourcePackage CreateOrGetPackage(string packageName)
        {
            return YooAssets.TryGetPackage(packageName) ?? YooAssets.CreatePackage(packageName);
        }

        /// <summary>
        /// 通用的资源包更新函数
        /// </summary>
        /// <param name="package">要初始化的资源包</param>
        /// <param name="callbacks">各种回调函数</param>
        /// <returns>是否初始化成功</returns>
        public static async UniTask<bool> UpdatePackage(ResourcePackage package,
            PackageInitializationCallbacks callbacks)
        {
            if (_instance == null)
            {
                throw new Exception("Bootstrap instance not found in the scene.");
            }

            var ret = await _instance.UpdatePackageImpl(package, callbacks);
            if (!ret)
            {
                await package.DestroyAsync();
            }

            return ret;
        }

        /// <summary>
        /// 清理资源包缓存
        /// </summary>
        /// <param name="package"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static async UniTask<bool> DeletePackageCache(ResourcePackage package,
            Func<Exception, UniTask> onError = null)
        {
            var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearAllBundleFiles);
            await operation.ToUniTask();

            if (operation.Status == EOperationStatus.Succeed)
            {
                //清理成功
                return true;
            }

            //清理失败
            if (onError != null) await onError(new Exception(operation.Error));
            return false;
        }
    }
}