//
// CrossDomainDemo.cs
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
using UnityEngine;
using JEngine.Core;

namespace JEngine.Examples
{
    public class CrossDomainDemo : ExampleAPI
    {
        public override void ExampleMethod()
        {
            Log.Print("override ExampleMethod from Hot Update Scripts");
        }

        public void Awake()
        {
            Log.Print("跨域继承Demo，这个基类是ExampleAPI，" +
                "继承了Mono，所以可以在CrossDomainDemo这个GameObject上看到，" +
                "ClassBind自动挂了个ExampleAPIAdapter/Adapter");

            Log.Print($"这个对象用GetComponent获取是null嘛？{gameObject.GetComponent<CrossDomainDemo>() == null}");

            Log.Print("再来一个跨域继承泛型的Demo吧，ILRuntime2.0似乎可以生成这个了，后续JEngine会提供可视化工具");
            Log.Print("参考UnityProject/Assets/Scripts/Adapters/GenericExampleAdapter.cs这个文件，提供了泛型跨域继承的2个demo");
            Log.Print("首先是public class GenericTest1 : GenericExample<GenericTest1>这种，泛型参数是它本身的继承，本地工程需要写一个继承Adapter的泛型适配器，参考GenericExampleAdapter1，即可");
            Log.Print("现在挂一个这个类到该gameObject上");
            gameObject.AddComponent<GenericTest1>().LogTest();
            Log.Print("不出意外，来了一句Log，还顺带打印了T的类型，是Adapter");

            Log.Print("现在是public class GenericTest2 : GenericExample<JBehaviour>这种，泛型参数是其他的无跨域继承的热更类型，本地工程需要写一个继承ILTypeInstance的泛型适配器，参考GenericExampleAdapter2，即可");
            Log.Print("现在挂一个这个类到该gameObject上");
            gameObject.AddComponent<GenericTest2>().LogTest();
            Log.Print("不出意外，来了一句Log，还顺带打印了T的类型，是ILTypeInstance");

            Log.Print("那么泛型跨域适配器的演示到此为止了，记住一点：泛型参数是啥，适配器那边就注册啥");


            Log.Print("调用override方法，跨域继承");
            ExampleMethod();

            Log.Print("尝试跨域多层继承，热更的Base继承了MonoBehaviour，然后MultiInherit继承了Base，现在尝试AddComponent挂上去");
            var m = new GameObject("MultiInheit").AddComponent<MultiInherit>();
            Log.Print("现在开始测试他的方法能否正常调用");
            m.Test();
            Log.Print("现在尝试复制多层跨域继承的类");
            var m2 = GameObject.Instantiate(m);
            Log.Print($"m2.gameObject = {m2.gameObject}");
            //ILRuntime有个bug，不是泛型的FindObject(s)OfType似乎得存个变量，不然用String.Format拼接会报错
            var all = FindObjectsOfType(typeof(MultiInherit));
            Log.Print($"共有{all.Length}个MultiInherit");
            Log.Print($"泛型也可以用，FindObjectsOfType<MultiInherit>()[0].GetType()：{FindObjectsOfType<MultiInherit>()[0].GetType()}");
            var result = FindObjectOfType(typeof(MultiInherit));
            Log.Print($"FindObjectOfType(typeof(MultiInherit))：{result}");
            Log.Print($"泛型也可以用，FindObjectOfType<MultiInherit>()：{FindObjectOfType<MultiInherit>()}");

            Log.Print("尝试SendMessage方法，SendMessageUpward和BroadCast效果一样，就不测试了，可以自己写代码测试。CLR重定向后SendMessage等类似效果的代码已经可以使用，如果这里出现报错，请看控制台输出的Warning，里面有解释");
            SendMessage("MsgToSend", 200);

            Log.Print("尝试Invoke方法。CLR重定向后Invoke等类似效果的代码已经可以使用");
            Log.Print("1秒后会执行Do1");
            Invoke("Do1", 1);
            Log.Print($"Do1执行状态：{IsInvoking("Do1")}");

            Log.Print("尝试InvokeRepeating方法。");
            Log.Print("0秒后，会每秒重复执行Do");
            InvokeRepeating("Do", 0, 1);
            Log.Print($"Invoke状态：{IsInvoking("Do")}");

            //小彩蛋，试试更改TimeScale，看看会对Invoke出现什么影响
            //原生Unity的TimeScale就会影响Invoke，所以JEngine也对此作了处理，能保证百分百的原汁原味
        }

        public void MsgToSend(int code)
        {
            Log.Print($"code是：{code}");
        }

        public void Do1()
        {
            Log.Print($"Do1执行了");
        }

        int count = 0;

        public void Do()
        {
            count++;
            Log.Print($"Invoke了{count}次");

            if (count >= 10)
            {
                Log.Print("该取消了，CancelInvoke()和CancelInvoke(\"Do\")等效");
                CancelInvoke();
                CancelInvoke("Do");
            }
        }
    }

    public class Base : MonoBehaviour
    {

    }

    public class MultiInherit : Base
    {
        public void Awake()
        {
            Log.Print("Awake from Multi inherit");
        }

        public void Test()
        {
            Log.Print("Test from MultiInherit");
        }
    }

    public class GenericTest1 : GenericExample<GenericTest1>
    {

    }

    public class GenericTest2 : GenericExample<JBehaviour>
    {

    }
}
