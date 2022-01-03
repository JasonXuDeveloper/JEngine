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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JEngine.Core
{
    public class ClassBindMgr : MonoBehaviour
    {
        public static void Instantiate()
        {
            var bindMgr = FindObjectOfType<ClassBindMgr>();
            if (bindMgr != null)
                return;

            _instance = new GameObject("ClassBindMgr").AddComponent<ClassBindMgr>();
            DontDestroyOnLoad(_instance);
        }

        private static ClassBindMgr _instance;
        public static List<Scene> LoadedScenes;
        private static List<ClassBind> _cbs;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(this);
            }

            LoadedScenes = new List<Scene>(0) {SceneManager.GetActiveScene()};
            _cbs = new List<ClassBind>(0);

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                LoadedScenes.Add(scene);
                DoBind();
            };
            
            SceneManager.sceneUnloaded+=scene =>
            {
                LoadedScenes.Remove(scene);
            };
            DoBind();
        }

        public static void DoBind(List<ClassBind> cbs)
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

            //激活
            foreach (var cb in cbs)
            {
                foreach (ClassData data in cb.scriptsToBind)
                {
                    if (data == null ||data.Activated)
                    {
                        continue;
                    }

                    cb.Active(data);
                }
            }
        }
        
        public static void DoBind(ClassBind cb)
        {
            if (_cbs != null && _cbs.Contains(cb)) return;
            DoBind(new List<ClassBind>{cb});
        }
        
        public static void DoBind()
        {
            _cbs = Tools.FindObjectsOfTypeAll<ClassBind>();
            DoBind(_cbs);
        }
    }
}