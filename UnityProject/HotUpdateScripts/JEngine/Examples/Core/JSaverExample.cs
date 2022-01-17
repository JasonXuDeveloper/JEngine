//
// JSaverExample.cs
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2020 
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
using UnityEngine.UI;
using JEngine.Core;

namespace JEngine.Examples
{
    /// <summary>
    /// JSaver的Demo
    /// JSaver Demo Showcase
    /// </summary>
    public class JSaverExample: JBehaviour
    {
        public Text Value;//显示的Text | Text which shows result

        /*
         * 3个不同存储方法的按钮
         * 3 ways to store data
         */
        public Button SaveAsStrBtn;
        public Button SaveAsJSONBtn;
        public Button SaveAsProtobufBtn;

        /// <summary>
        /// 给按钮绑定代码
        /// Bind actions to buttons
        /// </summary>
        public override void Init()
        {
            SaveAsStrBtn.onClick.AddListener(SaveAsString);
            SaveAsJSONBtn.onClick.AddListener(SaveAsJSON);
            SaveAsProtobufBtn.onClick.AddListener(SaveAsProtobuf);

            Log.Print("这里测试一个加密转JSON保存一个本地类型");
            JSONTest test = new JSONTest
            {
                a = 10,
                b = 1.2f,
                c = 2.4d,
                d = true,
                e = "JEngine"
            };
            var json = JSaver.SaveAsJSON("testCase", test);
            Log.Print($"转JSON结果：{LitJson.JsonMapper.ToJson(test)}");
            Log.Print($"加密后将这个存本地了：{json}");
            Log.Print("现在开始尝试解密获取");
            var result = JSaver.GetObjectFromJSON<JSONTest>("testCase");
            Log.Print("解密结果：");
            Log.Print($"a: {result.a}");
            Log.Print($"b: {result.b}");
            Log.Print($"c: {result.c}");
            Log.Print($"d: {result.d}");
            Log.Print($"e: {result.e}");
        }

        /// <summary>
        /// 通过字符串存本地
        /// Store value as string
        /// </summary>
        public void SaveAsString()
        {
            JSaver.SaveAsString("牛皮语句", "ToString()存本地，存了JEngine牛皮");
            Value.text = $"字符串已以字符串存到本地，\n\n" +
                $"本地存储文件内数据为：\n{PlayerPrefs.GetString("牛皮语句")}\n\n" +
                $"JSaver获取后，数据为：\n{JSaver.GetString("牛皮语句")}";
        }

        /// <summary>
        /// 通过JSON存数据
        /// Store value as JSON
        /// </summary>
        public void SaveAsJSON()
        {
            DataClass data = new DataClass
            {
                id = 666,
                name = "JSaver - JSON",
                data = new System.Collections.Generic.Dictionary<string, string>()
                {
                    {"test","112233" }
                }
            };
            JSaver.SaveAsJSON("数据存JSON", data);
            Value.text = $"测试数据已以JSON存到本地，\n\n" +
                $"本地存储文件内数据为：\n{PlayerPrefs.GetString("数据存JSON")}\n\n" +
                $"JSaver获取后，数据为：\n{JSaver.GetString("数据存JSON")}\n\n" +
                $"JSON转对象：\n"+
                $"data.id = {JSaver.GetObjectFromJSON<DataClass>("数据存JSON").id}\n" +
                $"data.name = {JSaver.GetObjectFromJSON<DataClass>("数据存JSON").name}" +
                $"data.data['test'] = {JSaver.GetObjectFromJSON<DataClass>("数据存JSON").data["test"]}";
        }

        /// <summary>
        /// 通过Protobuf存数据
        /// Store value as Protobuf
        /// </summary>
        public void SaveAsProtobuf()
        {
            DataClass data = new DataClass
            {
                id = 666666,
                name = "JSaver - Protobuf",
                data = new System.Collections.Generic.Dictionary<string, string>()
                {
                    {"test-proto","112233" }
                }
            };
            JSaver.SaveAsProtobufBytes("数据存Protobuf", data);
            Value.text = $"测试数据已以Protobuf存到本地，\n\n" +
                $"本地存储文件内数据为：\n{PlayerPrefs.GetString("数据存Protobuf")}\n\n" +
                $"JSaver获取后，数据为（二进制转Base64）：\n{JSaver.GetString("数据存Protobuf")}\n\n" +
                $"Protobuf转对象：\n" +
                $"data.id = {JSaver.GetObjectFromProtobuf<DataClass>("数据存Protobuf").id}\n" +
                $"data.name = {JSaver.GetObjectFromProtobuf<DataClass>("数据存Protobuf").name}"+
                $"data.data['test-proto'] = {JSaver.GetObjectFromProtobuf<DataClass>("数据存Protobuf").data["test-proto"]}";
        }

    }
}
