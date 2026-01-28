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
using JEngine.Core.Encrypt;
using JEngine.Core.Update;
using UnityEngine.SceneManagement;
using YooAsset;
using SceneHandle = YooAsset.SceneHandle;

namespace JEngine.Core
{
    public partial class Bootstrap
    {
        /// <summary>
        /// Common function for loading hot update scenes
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
            // Call status update callback
            callbacks.OnStatusUpdate?.Invoke(SceneLoadStatus.Loading);
            callbacks.OnProgressUpdate?.Invoke(0f);

            // Save current active scene for later unloading
            var previousSceneName = SceneManager.GetActiveScene().name;

            try
            {
                // Load new scene
                var handle = package.LoadSceneAsync(sceneName, loadMode, suspendLoad: true);

                // Update loading progress
                while (handle.Progress < 0.9f)
                {
                    float progress = handle.Progress;

                    // Call progress update callback
                    callbacks.OnProgressUpdate?.Invoke(progress);

                    await UniTask.DelayFrame(1);
                }

                // Call completion status callback
                callbacks.OnStatusUpdate?.Invoke(SceneLoadStatus.Completed);
                callbacks.OnProgressUpdate?.Invoke(1f);

                await UniTask.DelayFrame(1); // Wait one frame to ensure scene transition is complete
                handle.UnSuspend();
                await handle.Task;

                if (handle.Status != EOperationStatus.Succeed)
                {
                    throw new Exception(handle.LastError);
                }

                // Call completion callback
                return handle;
            }
            catch (Exception e)
            {
                callbacks.OnStatusUpdate?.Invoke(SceneLoadStatus.Failed);
                // Call error callback
                await callbacks.OnError(e);
                // Switch back to previous scene
                await SceneManager.LoadSceneAsync(previousSceneName).ToUniTask();

                return null;
            }
        }

        /// <summary>
        /// Create or get a resource package
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static ResourcePackage CreateOrGetPackage(string packageName)
        {
            return YooAssets.TryGetPackage(packageName) ?? YooAssets.CreatePackage(packageName);
        }

        /// <summary>
        /// Common resource package update function
        /// </summary>
        /// <param name="package">Resource package to initialize</param>
        /// <param name="callbacks">Various callback functions</param>
        /// <param name="encryptionOption">Package encryption method</param>
        /// <returns>Whether initialization was successful</returns>
        public static async UniTask<bool> UpdatePackage(ResourcePackage package,
            PackageInitializationCallbacks callbacks, EncryptionOption encryptionOption)
        {
            if (_instance == null)
            {
                throw new Exception("Bootstrap instance not found in the scene.");
            }

            var ret = await _instance.UpdatePackageImpl(package, callbacks, encryptionOption);
            if (!ret)
            {
                await package.DestroyAsync();
            }

            return ret;
        }

        /// <summary>
        /// Clear resource package cache
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
                // Cleanup successful
                return true;
            }

            // Cleanup failed
            if (onError != null) await onError(new Exception(operation.Error));
            return false;
        }
    }
}