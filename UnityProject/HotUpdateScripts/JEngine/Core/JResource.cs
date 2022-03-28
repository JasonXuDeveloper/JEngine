//
// JResource.cs
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

namespace JEngine.Core
{
    public class JResource
    {
        /// <summary>
        /// 加载热更资源（请使用全路径）
        /// Load hot update resource
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        public static T LoadRes<T>(string path, string package) where T : UnityEngine.Object
        {
            return AssetMgr.Load<T>(path, package, typeof(T));
        }

        /// <summary>
        /// 卸载热更资源
        /// Load hot update resource
        /// </summary>
        /// <param name="path"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        public static void UnloadRes(string path, string package)
        {
            AssetMgr.Unload(path, package);
        }

        /// <summary>
        /// 异步并行加载热更资源（可加回调）
        /// Load hot update resource async but parallel (can add callback)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <param name="mode"></param>
        public static async void LoadResAsync<T>(string path, Action<T> callback) where T : UnityEngine.Object
        {
            var asset = await AssetMgr.LoadAsync<T>(path, typeof(T));
            callback?.Invoke(asset);
        }

        /// <summary>
        /// 场景加载的进度
        /// Progress of loading a scene
        /// </summary>
        public static float LoadSceneProgress;

        /// <summary>
        /// 异步并行加载场景（可加回调）
        /// Load hot update scene async but parallel (can add callback)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="package"></param>
        /// <param name="loading"></param>
        /// <param name="finished"></param>
        /// <param name="additive"></param>
        public static void LoadSceneAsync(string path, string package = null, Action<float> loading = null, Action finished = null, bool additive = false)
        {
            AssetMgr.LoadSceneAsync(path, additive, package, (p) =>
            {
                loading?.Invoke(p);
                LoadSceneProgress = p;
             }, (b) =>
             {
                 finished?.Invoke();
                 LoadSceneProgress = 1;
             });
        }
    }
}
