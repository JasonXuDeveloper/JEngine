//
// ProtobufTest.cs
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
using System.Collections.Generic;
using JEngine.Examples.Data;
using JEngine.Core;

namespace JEngine.Examples
{
    [System.Serializable]
    [global::ProtoBuf.ProtoContract()]
    public class PBTest
    {
        /*
         * Fields to serialize
         */
        [global::ProtoBuf.ProtoMember(1)]
        public List<HotData> a;

        [global::ProtoBuf.ProtoMember(2)]
        public Label label;

       [global::ProtoBuf.ProtoContract()]
        public enum Label
        {
            LABEL_OPTIONAL = 1,
            LABEL_REQUIRED = 2,
            LABEL_REPEATED = 3,
        }
    }

    public class ProtobufDemo
    {
        public void Awake()
        {
            Log.Print("JEngine顺便解决了，无法pb序列化List<热更类型>和序列化反序列化热更enum的问题");
            var pbtest = new PBTest()
            {
                a = new List<HotData>()
                {
                    new HotData()
                    {
                        v=1,
                        k="第一个"
                    },
                    new HotData()
                    {
                        v=2,
                        k="第二个"
                    },
                },
                label = PBTest.Label.LABEL_OPTIONAL
            };
            var bytes = StringifyHelper.ProtoSerialize(pbtest);
            Log.Print($"序列化后的byte[]长度：{bytes.Length}");
            Log.Print("开始反序列化");
            var result = StringifyHelper.ProtoDeSerialize<PBTest>(bytes);
            Log.Print("反序列化成功，现在我们打印一下反序列化后的类型的a字段，也就是List<HotData>字段");
            Log.Print(string.Join(",", result.a));
            Log.Print("反序列化成功，现在我们打印一下反序列化后的类型的label字段，也就是热更的enum");
            Log.Print(result.label);
        }
    }
}
