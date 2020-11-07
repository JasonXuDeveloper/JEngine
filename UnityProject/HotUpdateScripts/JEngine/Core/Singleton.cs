//
// Singleton.cs
//
// Author:
//       L-Fone <275757115@qq.com>
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
namespace JEngine.Core
{
    /// <summary>
    /// 单例基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : new()
    {
        protected Singleton() { }

        protected static T _inst = new T();
        public static T Instance
        {
            get
            {
                if (null == _inst)
                    _inst = new T();
                return _inst;
            }
        }
    }

    /// <summary>
    /// 单例Mono基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T> where T : UnityEngine.Component
    {
        protected MonoSingleton() { }

        protected static T _inst = null;
        public static T Instance
        {
            get
            {
                if (null == _inst)
                    _inst = new GameObject(typeof(T).Name + "GO").AddComponent<T>();
                return _inst;
            }
        }
    }
}