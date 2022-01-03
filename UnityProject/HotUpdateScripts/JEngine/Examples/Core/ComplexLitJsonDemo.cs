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
    public class JsonTestClass
    {
        public int intVal;
        public float floatVal;
        public DataClass hotVal;

        public override string ToString()
        {
            return $"a={intVal},b.isNull = {hotVal == null}";
        }
    }
    public class ComplexLitJsonDemo
    {
        public void Awake()
        {

            Test1();
            Test2();
            Test3();
        }

        private void Test1()
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

        private void Test2()
        {
            Log.Print("测试2，热更类型，内部分别有2个本地类型和1个热更类型字段");
            var t2 = new JsonTestClass()
            {
                intVal = 10,
                floatVal = 3.14159f,
                hotVal = new DataClass()
                {
                    gm = true,
                    Money = 100,
                    id = 1,
                    name = "测试2"
                }
            };
            var json = JsonMapper.ToJson(t2);
            Log.Print($"序列化后的JSON: {json}");
            var newT2 = JsonMapper.ToObject<JsonTestClass>(json);
            Log.Print("反序列化t2:");
            Log.Print(newT2);
        }

        private void Test3()
        {
            Log.Print("第3个测试，类似测试1，但这次是GTest<JsonTestClass>，这样会有3个字段，其中包含1个热更类型字段");
            GTest<JsonTestClass> t3 = new GTest<JsonTestClass>()
            {
                a = 3,
                b = new JsonTestClass()
                {
                    intVal = 999,
                    floatVal = 3.1415926f,
                    hotVal = new DataClass()
                    {
                        gm = false,
                        Money = 10000,
                        id = 299,
                        name = "测试3"
                    }
                }
            };
            var json = JsonMapper.ToJson(t3);
            Log.Print($"序列化后的JSON: {json}");

            Log.Print("现在反序列化json，转成一个新的GTest<HotData>");
            var newT3 = JsonMapper.ToObject<GTest<JsonTestClass>>(json);
            Log.Print(newT3);
        }
    }
}