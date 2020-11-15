//
// EventDemo.cs
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
using JEngine.Core;
using JEngine.Event;
using System.Diagnostics;

namespace JEngine.Examples.Event
{
    public class EventDemo : JBehaviour
    {
        public override void Init()
        {
            Stopwatch sw = new Stopwatch();

            //JEvent.ShowLog = true;//是否显示一些log

            JEvent.Register(typeof(EventClass));//注册方法1
            JEvent.Register(new EventClass2());//注册方法2
            
            //广播数据
            sw.Start();
            JEvent.Post();
            sw.Stop();
            Log.Print("执行无参数方法的事件通知耗时" + sw.ElapsedMilliseconds + "ms");

            JEvent.Unregister(typeof(EventClass));//取消一个类的监听

            JEvent.Register(typeof(EventClass));//重新注册

            //广播666，全部void xxx(int)的都会被广播到
            JEvent.Post(666);

            //广播DataClass，全部void xxx(DataClass)的都会被广播到
            JEvent.Post(new DataClass() { Money = 123456,name="JEnvent测试"});
        }
    }

    //整个类的所有方法都被监听
    [Subscriber(ThreadMode.Other)]
    public class EventClass
    {

        public void MethodA()
        {
            Log.Print("Class1: MethodA运行在线程：" + System.Threading.Thread.CurrentThread.ManagedThreadId);
        }

        public void MethodB(int val)
        {
            Log.Print("Class1: MethodB：int数字是" + val);
        }

        public void MethodC(DataClass d)
        {
            Log.Print("Class1: MethodC：dataClass的Json字符串是：" + StringifyHelper.JSONSerliaze(d));
        }
    }

    //个别方法被监听
    public class EventClass2
    {
        [Subscriber(ThreadMode.Main)]//跑主线程
        public void MethodA()
        {
            Log.Print("Class2: MethodA运行在线程：" + System.Threading.Thread.CurrentThread.ManagedThreadId);
        }

        //不打标签不会被通知
        public void MethodB(int val)
        {
            Log.Print("Class2: MethodB：" + val);
        }

        [Subscriber]//默认就是跑主线程
        public void MethodC()
        {
            Log.Print("Class2: MethodC运行在线程：" + System.Threading.Thread.CurrentThread.ManagedThreadId);
        }

        [Subscriber(ThreadMode.Other)]
        public void MethodD()
        {
            Log.Print("Class2: MethodD运行在线程：" + System.Threading.Thread.CurrentThread.ManagedThreadId);
        }
    }
}