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

            Log.Print("调用override方法，跨域继承");
            ExampleMethod();

            Log.Print("尝试SendMessage方法，SendMessageUpward和BroadCast效果一样，就不测试了，可以自己写代码测试。CLR重定向后SendMessage等类似效果的代码已经可以使用，如果这里出现报错，请看控制台输出的Warning，里面有解释");
            SendMessage("MsgToSend", 200);

            Log.Print("尝试Invoke方法。CLR重定向后Invoke等类似效果的代码已经可以使用");
            Log.Print("1秒后会执行Do");
            Invoke("Do", 1);
            Log.Print($"Do执行状态：{IsInvoking("Do")}");

            Log.Print("尝试InvokeRepeating方法。");
            Log.Print("0秒后，会每秒重复执行Do");
            InvokeRepeating("Do", 0, 1);
            Log.Print($"Invoke状态：{IsInvoking()}");
            
        }

        public void MsgToSend(int code)
        {
            Log.Print($"code是：{code}");
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
}
