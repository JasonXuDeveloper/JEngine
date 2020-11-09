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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JEngine.Core
{
    public class ClassBindMgr:MonoBehaviour
    {
        public static void Instantiate()
        {
            _instance = new GameObject("ClassBindMgr").AddComponent<ClassBindMgr>();
            DontDestroyOnLoad(_instance);
        }

        private static ClassBindMgr _instance;

        public static List<ClassBind> cbs = new List<ClassBind>(0);

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(this);
            }
            StartCoroutine(CheckClassBind());
        }

        IEnumerator CheckClassBind()
        {
            while (true)
            {
                for(int i = 0; i < cbs.Count; i++)
                {
                    var cb = cbs.ElementAt(i);
                    var done = true;
                    //先添加
                    foreach (_ClassBind _class in cb.ScriptsToBind)
                    {
                        if (_class == null)
                        {
                            done = false;
                            continue;
                        }
                        cb.AddClass(_class);
                    }
                    //再赋值
                    foreach (_ClassBind _class in cb.ScriptsToBind)
                    {
                        if (_class == null)
                        {
                            done = false;
                            continue;
                        }
                        cb.SetVal(_class);
                    }
                    //激活
                    foreach (_ClassBind _class in cb.ScriptsToBind)
                    {
                        if (_class == null)
                        {
                            done = false;
                            continue;
                        }
                        cb.Active(_class);
                    }
                    
                    if (done)
                    {
                        cb.Remove();
                        i--;
                    }
                }
                yield return null;
            }
        }

        public static List<T> FindObjectsOfTypeAll<T>()
        {
            return SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<T>(true))
                .ToList();
        }
    }
}