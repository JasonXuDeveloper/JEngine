//
// ComplexLitJsonDemo.cs
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
using LitJson;
using JEngine.Core;
using JEngine.Examples.Data;

namespace JEngine.Examples
{
    public class ComplexLitJsonDemo
    {
        public void Awake()
        {
            Log.Print("这是一个复杂的LitJson序列化的测试，别的ILRuntime版本的Litjson并不支持（除非我把这个提交到ILRuntime）");
            Log.Print("首先实例化一个本地泛型<热更类型>，GTest<HotData>");
            GTest<HotData> d = new GTest<HotData>()
            {
                a = 1,
                b = new HotData()
                {
                    v = 1,
                    k = "第一个"
                }
            };
            var json = JsonMapper.ToJson(d);
            Log.Print($"序列化后的JSON: {json}");

            Log.Print("现在反序列化json，转成一个新的GTest<HotData>");
            var newD = JsonMapper.ToObject<GTest<HotData>>(json);
            Log.Print(newD);
        }
    }
}
