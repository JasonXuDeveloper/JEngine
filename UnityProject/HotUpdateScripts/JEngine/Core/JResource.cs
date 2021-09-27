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
        /// 加载热更资源
        /// Load hot update resource
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static T LoadRes<T>(string path, MatchMode mode = MatchMode.AutoMatch) where T : UnityEngine.Object
        {
            return AssetMgr.Load(ResPath(path, mode), typeof(T)) as T;
        }

        /// <summary>
        /// 卸载热更资源
        /// Load hot update resource
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static void UnloadRes(string path, MatchMode mode = MatchMode.AutoMatch)
        {
            AssetMgr.Unload(ResPath(path, mode));
        }

        /// <summary>
        /// 异步并行加载热更资源（可加回调）
        /// Load hot update resource async but parallel (can add callback)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <param name="mode"></param>
        public static async void LoadResAsync<T>(string path, Action<T> callback, MatchMode mode = MatchMode.AutoMatch) where T : UnityEngine.Object
        {
            var asset = await AssetMgr.LoadAsync(path, typeof(T));
            callback?.Invoke(asset as T);
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
        /// <param name="callback"></param>
        /// <param name="additive"></param>
        public static void LoadSceneAsync(string path, Action callback = null, bool additive = false)
        {
            AssetMgr.LoadSceneAsync(path, additive, (p) =>
            {
                LoadSceneProgress = p;
            }, (b) =>
            {
                callback?.Invoke();
                LoadSceneProgress = 1;
            });
        }


        private static string ResPath(string path, MatchMode mode)
        {
            if (path.Contains("Assets/HotUpdateResources/"))
            {
                path = path.Replace("Assets/HotUpdateResources/", "");
                path = path.Substring(path.IndexOf("/") + 1);
            }
            switch (mode)
            {
                case MatchMode.AutoMatch:
                    return path;
                case MatchMode.Animation:
                    return "Assets/HotUpdateResources/Controller/" + path;
                case MatchMode.Material:
                    return "Assets/HotUpdateResources/Material/" + path;
                case MatchMode.Prefab:
                    return "Assets/HotUpdateResources/Prefab/" + path;
                case MatchMode.Scene:
                    return "Assets/HotUpdateResources/Scene/" + path;
                case MatchMode.ScriptableObject:
                    return "Assets/HotUpdateResources/ScriptableObject/" + path;
                case MatchMode.TextAsset:
                    return "Assets/HotUpdateResources/TextAsset/" + path;
                case MatchMode.UI:
                    return "Assets/HotUpdateResources/UI/" + path;
                case MatchMode.Other:
                    return "Assets/HotUpdateResources/Other/" + path;
                default:
                    return path;
            }
        }

        public enum MatchMode
        {
            AutoMatch = 1,
            Animation = 2,
            Material = 3,
            Prefab = 4,
            Scene = 5,
            ScriptableObject = 6,
            TextAsset = 7,
            UI = 8,
            Other = 9
        }
    }
}
