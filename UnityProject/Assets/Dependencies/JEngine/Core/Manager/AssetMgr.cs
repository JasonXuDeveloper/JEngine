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
using BM;
using ET;
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using JEngine.Interface;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace JEngine.Core
{
    public static partial class AssetMgr
    {
        public static bool RuntimeMode => AssetComponentConfig.AssetLoadMode != AssetLoadMode.Develop;

        public static Object Load(string path)
        {
            return Load(path, null, null);
        }

        public static Object Load(string path, string package)
        {
            return Load(path, package, null);
        }
        
        [Obsolete]
        public static Object Load(string path, Type type)
        {
            return Load(path, null, type);
        }

        private static Object Load(string path, string package, Type type)
        {
            
            var ret = AssetComponent.Load(out _, path, package);
            return ret;
        }

        public static T Load<T>(string path)
            where T : Object
        {
            return Load<T>(path, null, null);
        }

        public static T Load<T>(string path, string package)
            where T : Object
        {
            return Load<T>(path, package, null);
        }
        
        [Obsolete]
        public static T Load<T>(string path, Type type)
            where T : Object
        {
            return Load<T>(path, null, type);
        }

        private static T Load<T>(string path, string package, Type type)
            where T : Object
        {
            var ret = AssetComponent.Load<T>(out _, path, package);
            return ret;
        }

        public static async ETTask<Object> LoadAsync(string path)
        {
            return await LoadAsync(path, null, null);
        }

        public static async ETTask<Object> LoadAsync(string path, string package)
        {
            return await LoadAsync(path, package, null);
        }

        [Obsolete]
        public static async ETTask<Object> LoadAsync(string path, Type type)
        {
            return await LoadAsync(path, null, type);
        }

        private static async ETTask<Object> LoadAsync(string path, string package, Type type)
        {
            var ret = await AssetComponent.LoadAsync(out _, path, package);
            return ret;
        }

        public static async ETTask<T> LoadAsync<T>(string path)
            where T : Object
        {
            return await LoadAsync<T>(path, null, null);
        }

        public static async ETTask<T> LoadAsync<T>(string path, string package)
            where T : Object
        {
            return await LoadAsync<T>(path, package, null);
        }
        
        [Obsolete]
        public static async ETTask<T> LoadAsync<T>(string path, Type type)
            where T : Object
        {
            return await LoadAsync<T>(path, null, type);
        }

        private static async ETTask<T> LoadAsync<T>(string path, string package = null, Type type = null)
            where T : Object
        {
            var ret = await AssetComponent.LoadAsync<T>(out _, path, package);
            return ret;
        }

        /// <summary>
        /// 强制卸载所有该路径资源的引用
        /// </summary>
        /// <param name="path"></param>
        /// <param name="package"></param>
        public static void Unload(string path, string package = null)
        {
            package = string.IsNullOrEmpty(package) ? AssetComponentConfig.DefaultBundlePackageName : package;
            AssetComponent.UnLoadByPath(path, package);
        }

        public static void LoadScene(string path, bool additive = false, string package = null)
        {
            AssetComponent.LoadScene(path, package);
            if (additive)
                SceneManager.LoadScene(path, LoadSceneMode.Additive);
            else
                SceneManager.LoadScene(path);
            RemoveUnusedAssets();
        }

        public static async void LoadSceneAsync(string path, bool additive = false, string package = null,
            Action<float> loadingCallback = null,
            Action<AsyncOperation> finishedCallback = null)
        {
            await AssetComponent.LoadSceneAsync(path, package);
            AsyncOperation operation = additive
                ? SceneManager.LoadSceneAsync(path, LoadSceneMode.Additive)
                : SceneManager.LoadSceneAsync(path);
            operation.allowSceneActivation = false;
            while (!operation.isDone && operation.progress < 0.9f)
            {
                loadingCallback?.Invoke(operation.progress);
                await TimeMgr.Delay(1);
            }

            loadingCallback?.Invoke(1);
            operation.allowSceneActivation = true;
            operation.completed += asyncOperation =>
            {
                RemoveUnusedAssets();
                finishedCallback?.Invoke(asyncOperation);
            };
        }

        public static void RemoveUnusedAssets()
        {
            AssetComponent.ForceUnLoadAll();
        }
    }
}