//
// JPrefab.cs
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
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JEngine.Core
{
    public class JPrefab: IDisposable
    {
        /// <summary>
        /// Load a prefab from hot update resources
        /// 从热更资源里读取prefab
        /// </summary>
        /// <param name="path"></param>
        public JPrefab(string path, bool async = false) : this(path, async, null)
        {

        }

        /// <summary>
        /// Load a prefab from hot update resources (async)
        /// 从热更资源里读取prefab （异步）
        /// </summary>
        /// <param name="path"></param>
        /// <param name="complete">Action<bool,JPrefab>, success 与 JPrefab</param>
        public JPrefab(string path, Action<bool, JPrefab> complete = null) : this(path, true, complete)
        {

        }


        private JPrefab(string path,bool async,Action<bool,JPrefab> complete)
        {
            if (!path.Contains(".prefab"))
            {
                path = new StringBuilder(path).Append(".prefab").ToString();
            }
            if (async)
            {
                _ = LoadPrefabAsync(path, complete);
            }
            else
            {
                var obj = AssetMgr.Load(path, typeof(GameObject));
                Instance = obj != null ? obj as GameObject : null;
                Loaded = true;
            }
            this.path = path;
        }

        /// <summary>
        /// Wait for async
        /// 如果是异步加载JPrefab，则可以使用此方法等待
        /// </summary>
        public async Task WaitForAsyncLoading()
        {
            while(!Loaded)
            {
                await Task.Delay(1);
            }
        }


        private async Task LoadPrefabAsync(string path, Action<bool, JPrefab> callback)
        {
            var obj = await AssetMgr.LoadAsync(path, typeof(GameObject));
            Instance = obj != null ? obj as GameObject : null;
            Loaded = true;
            callback?.Invoke(!Error, this);
        }

        private string path;

        /// <summary>
        /// If the prefab has loaded or not (if it has error, it will still be loaded)
        /// Prefab是否加载（如果有错误，这里也会是加载）
        /// </summary>
        public bool Loaded;

        /// <summary>
        /// If has error while loading or not
        /// 加载时是否有错
        /// </summary>
        public bool Error => !String.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Error message when error
        /// 错误时的错误信息
        /// </summary>
        public string ErrorMessage => AssetMgr.Error(path);

        /// <summary>
        /// Progress of loading a prefab
        /// 加载prefab的进度
        /// </summary>
        public float Progress => AssetMgr.Progress(path);

        /// <summary>
        /// State of loading a prefab
        /// 加载prefab的状态
        /// </summary>
        public libx.LoadState State => AssetMgr.State(path);

        /// <summary>
        /// Prefab GameObject (this is not in scene and it has not been instantiated)
        /// Prefab游戏对象（这个并不在场景中，也没被生成）
        /// </summary>
        public GameObject Instance;

        /// <summary>
        /// All GameObjects that has been instantiated to scene
        /// 全部被生成到场景中的游戏对象
        /// </summary>
        public List<GameObject> InstantiatedGameObjects
        {
            get
            {
                List<GameObject> ret = new List<GameObject>();
                for(int i = 0, cnt = _instantiatedGameObjects.Count; i < cnt; i++)
                {
                    var gameObject = _instantiatedGameObjects[i];
                    if (gameObject == null || ReferenceEquals(gameObject, null)) continue;
                    ret.Add(gameObject);
                }
                _instantiatedGameObjects = ret;
                return ret;
            }
        }
        private List<GameObject> _instantiatedGameObjects = new List<GameObject>(0);

        /// <summary>
        /// Instantiate a prefab
        /// 生成预制体
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameObject Instantiate(string name = "")
        {
            while (!Loaded)
            {
                throw new Exception($"{path} has not been loaded yet");
            }
            if (Error)
            {
                throw new Exception($"{path} has an error: {ErrorMessage}");
            }
            var go = UnityEngine.Object.Instantiate(Instance);
            if (!String.IsNullOrEmpty(name))
            {
                go.name = name;
            }
            InstantiatedGameObjects.Add(go);
            return go;
        }

        /// <summary>
        /// Instantiate a prefab
        /// 生成预制体
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameObject Instantiate(Transform parent, string name = "")
        {
            if (!Loaded)
            {
                throw new Exception($"{path} has not been loaded yet");
            }
            if (Error)
            {
                throw new Exception($"{path} has an error: {ErrorMessage}");
            }
            var go = UnityEngine.Object.Instantiate(Instance, parent);
            if (!String.IsNullOrEmpty(name))
            {
                go.name = name;
            }
            InstantiatedGameObjects.Add(go);
            return go;
        }

        /// <summary>
        /// Instantiate a prefab
        /// 生成预制体
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="instantiateInWorldSpace"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameObject Instantiate(Transform parent, bool instantiateInWorldSpace, string name = "")
        {
            if (!Loaded)
            {
                throw new Exception($"{path} has not been loaded yet");
            }
            if (Error)
            {
                throw new Exception($"{path} has an error: {ErrorMessage}");
            }
            var go = UnityEngine.Object.Instantiate(Instance, parent, instantiateInWorldSpace);
            if (!String.IsNullOrEmpty(name))
            {
                go.name = name;
            }
            InstantiatedGameObjects.Add(go);
            return go;
        }

        /// <summary>
        /// Instantiate a prefab
        /// 生成预制体
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameObject Instantiate(Vector3 position, Quaternion rotation, string name = "")
        {
            if (!Loaded)
            {
                throw new Exception($"{path} has not been loaded yet");
            }
            if (Error)
            {
                throw new Exception($"{path} has an error: {ErrorMessage}");
            }
            var go = UnityEngine.Object.Instantiate(Instance, position, rotation);
            if (!String.IsNullOrEmpty(name))
            {
                go.name = name;
            }
            InstantiatedGameObjects.Add(go);
            return go;
        }

        /// <summary>
        /// Instantiate a prefab
        /// 生成预制体
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameObject Instantiate(Vector3 position, Quaternion rotation,Transform parent, string name = "")
        {
            if (!Loaded)
            {
                throw new Exception($"{path} has not been loaded yet");
            }
            if (Error)
            {
                throw new Exception($"{path} has an error: {ErrorMessage}");
            }
            var go = UnityEngine.Object.Instantiate(Instance, position, rotation, parent);
            if (!String.IsNullOrEmpty(name))
            {
                go.name = name;
            }
            InstantiatedGameObjects.Add(go);
            return go;
        }

        /// <summary>
        /// Destory all instantiated gameObjects from this prefab
        /// 删除该prefab生成的全部gameObject
        /// </summary>
        public void DestroyAllInstantiatedObjects()
        {
            for (int i = 0, cnt = InstantiatedGameObjects.Count; i < cnt; i++)
            {
                UnityEngine.Object.DestroyImmediate(InstantiatedGameObjects[i]);
            }
        }

        public override string ToString()
        {
            return $"JPrefab: {path}";
        }

        ~JPrefab()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            DestroyAllInstantiatedObjects();
            _instantiatedGameObjects.Clear();
            AssetMgr.Unload(path);
            Instance = null;
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
