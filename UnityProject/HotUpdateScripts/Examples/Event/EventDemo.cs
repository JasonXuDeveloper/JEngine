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

namespace JEngine.Examples
{
    public class EventDemo : JBehaviour
    {
        public override void Init()
        {
            Stopwatch sw = new Stopwatch();

            //JEvent.ShowLog = true;//是否显示一些log

            
            //广播空参数方法
            sw.Start();
            JEvent.defaultEvent.Post();
            sw.Stop();
            Log.Print("<color=#db4259>[JEvent] 注册前，执行无参数方法的事件（共0个）通知耗时" + sw.ElapsedMilliseconds + "ms</color>");
            sw.Reset();

            JEvent.defaultEvent.Register(typeof(EventClass));//注册方法1
            JEvent.defaultEvent.Register(new EventClass2());//注册方法2

            //广播空参数方法
            sw.Start();
            JEvent.defaultEvent.Post();
            sw.Stop();
            Log.Print("<color=#db4259>[JEvent] 注册后，执行无参数方法的事件（demo里有5个）通知耗时" + sw.ElapsedMilliseconds + "ms</color>");
            sw.Reset();

            Log.Print("<color=#db4259>[JEvent] 现在取消注册EventClass，任何里面的方法都无法被广播到，我们来试试</color>");
            JEvent.defaultEvent.Unregister(typeof(EventClass));//取消一个类的监听

            //广播空参数方法
            sw.Start();
            JEvent.defaultEvent.Post();
            sw.Stop();
            Log.Print("<color=#db4259>[JEvent] 取消注册EventClass后，执行无参数方法的事件（共0个）通知耗时" + sw.ElapsedMilliseconds + "ms</color>");
            sw.Reset();


            Log.Print("<color=#db4259>[JEvent] EventClass里的方法没被触发，现在我们重新注册，然后Post</color>");
            JEvent.defaultEvent.Register(typeof(EventClass));//重新注册

            //广播666，全部void xxx(int)的都会被广播到
            JEvent.defaultEvent.Post(666);

            //广播DataClass，全部void xxx(DataClass)的都会被广播到
            JEvent.defaultEvent.Post(new DataClass() { Money = 123456,name="JEnvent测试"});
        }

        public override void Run()
        {
            //创建独立的JEvent
            JEvent e = new JEvent();
            //给独立的JEvent注册方法
            e.Register(this.GetType());
            //广播
            e.Post();
        }

        [Subscriber]
        private void TestMethod()
        {
            Log.Print("独立的JEvent的Test哦~");
        }
    }

    //继承了也能被监听
    [Subscriber(ThreadMode.Main)]
    public class InheritTest
    {
        public void Test()
        {
            Log.Print("InheritTest: Test运行在线程：" + System.Threading.Thread.CurrentThread.ManagedThreadId);
        }
    }

    //整个类的所有方法都被监听
    [Subscriber(ThreadMode.Other)]
    public class EventClass: InheritTest
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