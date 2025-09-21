//
// MultiInheritenceDemo.cs
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2022 JEngine
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
using JEngine.Core;
using UnityEngine;

namespace JEngine.Examples
{
    public class MultiInheritenceDemo
    {
        public void Awake()
        {
            //测试纯热更的多重继承
            Debug.Log("先测试纯热更内（无跨域）的多重继承（热更工程内new）");
            new AAClass();
            Debug.Log("先测试纯热更内（无跨域）的多重继承（通过classbind创建）");
            var go = new GameObject("AAClassImp");
            var cb = go.AddComponent<ClassBind>();
            var cd = new ClassData()
            {
                classNamespace = "JEngine.Examples",
                className = "AAClass",
                activeAfter = false
            };
            cb.scriptsToBind = new ClassData[] { cd };
            cb.BindSelf();
            Debug.Log("不出意外的话两次实例化AAClass应该会有相同的log");

            //测试跨域多重继承
            Debug.Log("测试跨域的多重继承（热更工程内AddComponent）");
            go = new GameObject("AAMonoImp");
            go.AddComponent<AAMono>();
            Debug.Log("测试跨域的多重继承（通过classbind创建）");
            cb = go.AddComponent<ClassBind>();
            cd = new ClassData()
            {
                classNamespace = "JEngine.Examples",
                className = "AAMono",
                activeAfter = true
            };
            cb.scriptsToBind = new ClassData[] { cd };
            cb.BindSelf();
            Debug.Log("不出意外的话两次实例化AAMono应该会有相同的log");
        }
    }

    public class MonoBase : MonoBehaviour
    {
        public MonoBase()
        {
            Debug.Log("MonoBase.ctor");
        }
        public int f1 = 10;
    }

    public class AMono : MonoBase
    {
        public AMono()
        {
            Debug.Log("AMono.ctor");
        }
        public float f2 = 6.34f;
    }

    public class AAMono: AMono
    {
        public AAMono()
        {
            Debug.Log("AAMono.ctor");
            Debug.Log(f1);
            Debug.Log(f2);
            Debug.Log(f3);
            Debug.Log(this.GetInstanceID());
        }
        public bool f3 = true;
    }

    public class ABase
    {
        public ABase()
        {
            Debug.Log("ABase.ctor");
        }
        public int f1 = 2;
        public bool f2 = true;
    }

    public class AClass : ABase
    {
        public AClass()
        {
            Debug.Log("AClass.ctor");
        }
        public float f3 = 3f;
    }

    public class AAClass : AClass
    {
        public AAClass()
        {
            Debug.Log("AAClass.ctor");
            Debug.Log(f1);
            Debug.Log(f2);
            Debug.Log(f3);
            Debug.Log(f4);
        }
        public double f4 = 5d;
    }
}
