//
// Program.cs
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
using JEngine.Core;
using UnityEngine;

namespace HotUpdateScripts
{
    public static class Program
    {
        public static void SetupGame()
        {
            Debug.Log("<color=cyan>[SetupGame] 这个周期在ClassBind初始化之前，可以对游戏数据进行一些初始化</color>");
            //防止Task内的报错找不到堆栈，不建议删下面的代码
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                foreach (var innerEx in e.Exception.InnerExceptions)
                {
                    Debug.LogError($"{innerEx.Message}\n" +
                                   $"ILRuntime StackTrace: {innerEx.Data["StackTrace"]}\n\n" +
                                   $"Full Stacktrace: {innerEx.StackTrace}");
                }
            };
        }

        public static void RunGame()
        {
            Debug.Log("<color=yellow>[RunGame] 这个周期在ClassBind初始化后，可以激活游戏相关逻辑</color>");
            //如果生成热更解决方案跳过，参考https://docs.xgamedev.net/zh/documents/0.8/FAQ.html#生成热更工程dll跳过
            //的方法一，把生成的平台改成Any CPU（默认是小写的，windows下无法生成）
        }
    }
}