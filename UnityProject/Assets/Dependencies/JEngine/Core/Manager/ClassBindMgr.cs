//
// ClassBindMgr.cs
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

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace JEngine.Core
{
    public partial class ClassBindMgr : MonoBehaviour
    {
        public static void Instantiate()
        {
            if (_instance != null)
                return;

            _instance = new GameObject("ClassBindMgr").AddComponent<ClassBindMgr>();
            DontDestroyOnLoad(_instance);
            SceneManager.sceneLoaded += _instance.OnSceneLoaded;
            SceneManager.sceneUnloaded += _instance.OnSceneUnloaded;
            LoadedScenes.Add(SceneManager.GetActiveScene());
            DoBind();
        }

        private static ClassBindMgr _instance;
        public static readonly HashSet<Scene> LoadedScenes = new HashSet<Scene>();
        private static readonly List<ClassBind> Cbs = new List<ClassBind>(30);

        private void Awake()
        {
            if (_instance != null)
            {
                DestroyImmediate(this);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LoadedScenes.Add(scene);
            DoBind();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            LoadedScenes.Remove(scene);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                SceneManager.sceneLoaded -= _instance.OnSceneLoaded;
                SceneManager.sceneUnloaded -= _instance.OnSceneUnloaded;
                LoadedScenes.Clear();
                Cbs.Clear();
                _instance = null;
            }
        }

        public static void DoBind(ICollection<ClassBind> cbs)
        {
            foreach (var cb in cbs)
            {
                //先添加
                foreach (ClassData data in cb.scriptsToBind)
                {
                    if (data == null || data.Added)
                    {
                        continue;
                    }

                    cb.AddClass(data);
                }
            }

            foreach (var cb in cbs)
            {
                //再赋值
                foreach (ClassData data in cb.scriptsToBind)
                {
                    if (data == null || data.BoundData)
                    {
                        continue;
                    }

                    cb.SetVal(data);
                }
            }

            //确保任务全执行了
            LifeCycleMgr.Instance.ExecuteOnceTask();

            //激活
            foreach (var cb in cbs)
            {
                foreach (ClassData data in cb.scriptsToBind)
                {
                    if (data == null || data.Activated)
                    {
                        continue;
                    }

                    cb.Active(data);
                }
                
                Object.Destroy(cb);
            }

            //确保任务全执行了
            LifeCycleMgr.Instance.ExecuteOnceTask();
        }

        private static readonly List<ClassBind> Temp = new List<ClassBind>(1);

        public static void DoBind(ClassBind cb)
        {
            if (Cbs.Contains(cb)) return;
            Cbs.Add(cb);
            if (Temp.Count == 1)
            {
                if (Temp[0] == null)
                {
                    Temp[0] = cb;
                    DoBind(Temp);
                }
                else
                {
                    DoBind(new List<ClassBind>(1)
                    {
                        cb
                    });
                }
            }
            else
            {
                Temp.Add(cb);
                DoBind(Temp);
            }
        }

        public static void DoBind()
        {
            var c = Tools.FindObjectsOfTypeAll<ClassBind>();
            Cbs.AddRange(c);
            DoBind(c);
        }
    }
}