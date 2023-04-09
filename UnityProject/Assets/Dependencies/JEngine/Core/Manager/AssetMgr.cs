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

namespace JEngine.Core
{
    public static partial class AssetMgr
    {
        public static bool RuntimeMode => Updater.Mode != Updater.UpdateMode.Simulate;
        
        private static ResourcePackage GetPackage(string packageName)
        {
            if(packageName == null) packageName = Updater.MainPackageName;
            return YooAssets.GetPackage(packageName);
        }
        
        public static Object Load(string path, Type type) => Load(path, Updater.MainPackageName, type, out _);
        
        public static Object Load(string path, Type type, out AssetOperationHandle handle) => Load(path, Updater.MainPackageName, type, out handle);

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
        
        public static async Task<(T, AssetOperationHandle)> LoadAsyncWithHandle<T>(string path)
            where T : Object => await LoadAsyncWithHandle<T>(path, Updater.MainPackageName);

        public static async Task<(T, AssetOperationHandle)> LoadAsyncWithHandle<T>(string path, string package)
            where T : Object
        {
            var handle = GetPackage(package).LoadAssetAsync<T>(path);
            await handle.Task;
            return (handle.AssetObject as T, handle);
        }
        
        public static void LoadScene(string path, bool additive = false, string package = null)
        {
            SceneOperationHandle handle = GetPackage(package).LoadSceneAsync(path, additive? LoadSceneMode.Additive:LoadSceneMode.Single);
            handle.Task.Wait();
            RemoveUnusedAssets();
        }

        public static async Task LoadSceneAsync(string path, bool additive = false, string package = null)
        {
            SceneOperationHandle handle = GetPackage(package).LoadSceneAsync(path, additive? LoadSceneMode.Additive:LoadSceneMode.Single);
            await handle.Task;
            RemoveUnusedAssets();
        }

        public static RawFileOperationHandle LoadRaw(string path) => LoadRaw(path, Updater.MainPackageName);
        
        public static RawFileOperationHandle LoadRaw(string path, string package)
        {
            RawFileOperationHandle handle = GetPackage(package).LoadRawFileSync(path);
            return handle;
        }
        
        public static Task<RawFileOperationHandle> LoadRawAsync(string path) => LoadRawAsync(path, Updater.MainPackageName);
        
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