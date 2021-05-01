//
// ConvertDemo.cs
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
using System.Collections.Generic;
using JEngine.Core;
namespace JEngine.Examples
{
    public interface IDemo { }

    public class OtherClass { }

    public class Demo1 : JBehaviour, IDemo { }

    public class Demo2 : OtherClass, IDemo { }

    public class ConvertDemo : JBehaviour
    {
        public override void Init()
        {
            Log.Print($"[ConvertDemo] 本Demo用于示例多继承并至少有一个相同继承的类型如何存入数组");

            List<IDemo> list = new List<IDemo>(0);
            Log.Print($"[ConvertDemo] 创建了IDemo列表list");

            var go = new UnityEngine.GameObject("demo1");
            go.transform.SetParent(UnityEngine.GameObject.Find("ConvertDemo").transform);

            Demo1 d1 = go.CreateJBehaviour<Demo1>();
            Log.Print($"[ConvertDemo] 创建了一个{d1.GetType().FullName}对象d1，" +
                $"继承了JBehaviour和IDemo，挂载在ConvertDemo/demo1下");

            Demo2 d2 = new Demo2();
            Log.Print($"[ConvertDemo] 创建了一个{d2.GetType().FullName}对象d2，" +
                $"继承了OtherClass和IDemo");

            list.Add(d1);
            list.Add(d2);

            Log.Print($"[ConvertDemo] list 添加了d1和d2");

            Log.Print($"[ConvertDemo] list 有{list.Count}个元素");
        }
    }
}
