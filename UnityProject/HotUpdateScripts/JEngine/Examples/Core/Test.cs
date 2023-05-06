//
// Test.cs
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2023 JEngine
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
using ILRuntime.Runtime;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace HotUpdateScripts
{


    /// <summary>
    /// 测试dll优化
    /// </summary>
    public class Test
    {
        public void DoTest()
        {
            Debug.Log($"original(1) = {Original(1)}");
            Debug.Log($"jit(1) = {JIT(1)}");
            Debug.Log($"optimized(1) = {Optimized(1)}");
            Debug.Log($"optimizedJIT(1) = {OptimizedJIT(1)}");
            RunTest(10);
            RunTest(100);
            RunTest(1000);
            RunTest(10000);
            RunTest(100000);
        }

        public void RunTest(int cnt = 100000)
        {
            Stopwatch sw = new Stopwatch();
            Debug.Log($"{cnt}次计算");
            sw.Start();
            int a = 0;
            int i = cnt;
            while (i-- > 0)
            {
                a = Original(i);
            }

            sw.Stop();
            Debug.Log($"Original: {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            i = cnt;
            while (i-- > 0)
            {
                a = JIT(i);
            }

            sw.Stop();
            Debug.Log($"JIT: {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            i = cnt;
            while (i-- > 0)
            {
                a = Optimized(i);
            }

            sw.Stop();
            Debug.Log($"Optimized: {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            i = cnt;
            while (i-- > 0)
            {
                a = OptimizedJIT(i);
            }

            sw.Stop();
            Debug.Log($"OptimizedJIT: {sw.ElapsedMilliseconds}ms");
        }

        public int Original(int x)
        {
            int a = 5;
            int b = 20;
            int c = 10;
            c = a;
            int d = 5 * a;
            int e = d;
            int f = e / 2;
            int g = f + a + b + c * 6 - b / 4;
            int h = x * 2 + 10 + 3 - 6 - 8 - g;
            return h + 10 % 3;
        }

        [ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]
        public int JIT(int x)
        {
            int a = 5;
            int b = 20;
            int c = 10;
            c = a;
            int d = 5 * a;
            int e = d;
            int f = e / 2;
            int g = f + a + b + c * 6 - b / 4;
            int h = x * 2 + 10 + 3 - 6 - 8 - g;
            return h + 10 % 3;
        }

        public int Optimized(int x)
        {
            int a = 5;
            int b = 20;
            int c = 10;
            c = a;
            int d = 5 * a;
            int e = d;
            int f = e / 2;
            int g = f + a + b + c * 6 - b / 4;
            int h = x * 2 + 10 + 3 - 6 - 8 - g;
            return h + 10 % 3;
        }

        [ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]
        public int OptimizedJIT(int x)
        {
            int a = 5;
            int b = 20;
            int c = 10;
            c = a;
            int d = 5 * a;
            int e = d;
            int f = e / 2;
            int g = f + a + b + c * 6 - b / 4;
            int h = x * 2 + 10 + 3 - 6 - 8 - g;
            return h + 10 % 3;
        }
    }
}

