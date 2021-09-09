//
// AntiCheatDemo.cs
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2021 JEngine
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
using JEngine.AntiCheat;
namespace JEngine.Examples
{
    public class AntiCheatDemo
    {
        JInt i;
        public void Awake()
        {
            Log.Print("========== AntiCheat Test ==========");

            Log.Print(">> JInt Test Starts");
            Log.Print("用抓内存的工具来看i在内存中的值就能知道效果了（安卓真机可以用GameGuardian，即GG修改器测试）");
            i = new JInt(0);
            i = new JInt("0");
            i = 0;
            for(int x = 0; x < 10; x++)
            {
                i += x;
                Log.Print($"i = {i}");
            }
            Log.Print("<< JInt Test Ends");
        }
    }
}
