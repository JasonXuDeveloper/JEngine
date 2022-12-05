//
// NinoDemo.cs
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2022 JEngine
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
using JEngine.Core;
using Nino.Serialization;

namespace JEngine.Examples
{
    [NinoSerialize]
    public class NinoTestData
    {
        public enum Sex
        {
            Male,
            Female
        }
        [NinoMember(1)]
        public string name;
        [NinoMember(2)]
        public int id;
        [NinoMember(3)]
        public bool isHasPet;
        [NinoMember(4)]
        public Sex sex;

    }

    public class NinoDemo
	{
		public void Awake()
        {
            var _list = new List<NinoTestData>();
            _list.Add(new NinoTestData
            {
                sex = NinoTestData.Sex.Male,
                name = "A",
                id = 0,
                isHasPet = false
            });
            _list.Add(new NinoTestData
            {
                sex = NinoTestData.Sex.Female,
                name = "B",
                id = 1,
                isHasPet = true
            });

            var buf = Serializer.Serialize(_list);
            Log.Print($"Nino 将_list数据序列化为了 {buf.Length} bytes");

            var _list2 = Deserializer.Deserialize<List<NinoTestData>>(buf);
            Log.Print($"Nino 反序列化了一个_list2，内部有{_list2.Count}个元素");
            Log.Print($"_list2[0].sex = {_list2[0].sex}\n" +
                $"_list2[0].name = {_list2[0].name}\n" +
                $"_list2[0].id = {_list2[0].id}\n" +
                $"_list2[0].isHasPet = {_list2[0].isHasPet}\n");
            Log.Print($"_list2[1].sex = {_list2[1].sex}\n" +
                $"_list2[1].name = {_list2[1].name}\n" +
                $"_list2[1].id = {_list2[1].id}\n" +
                $"_list2[1].isHasPet = {_list2[1].isHasPet}\n");

            var _arr = new NinoTestData[2];
            _arr[0] = new NinoTestData
            {
                sex = NinoTestData.Sex.Male,
                name = "C",
                id = 2,
                isHasPet = true
            };
            _arr[1] = new NinoTestData
            {
                sex = NinoTestData.Sex.Female,
                name = "D",
                id = 3,
                isHasPet = false
            };

            buf = Serializer.Serialize(_arr);
            Log.Print($"Nino 将_arr数据序列化为了 {buf.Length} bytes");

            var _arr2 = Deserializer.Deserialize<NinoTestData[]> (buf);
            Log.Print($"Nino 反序列化了一个_arr2，内部有{_arr2.Length}个元素");
            Log.Print($"_arr2[0].sex = {_arr2[0].sex}\n" +
                $"_arr2[0].name = {_arr2[0].name}\n" +
                $"_arr2[0].id = {_arr2[0].id}\n" +
                $"_arr2[0].isHasPet = {_arr2[0].isHasPet}\n");
            Log.Print($"_arr2[1].sex = {_arr2[1].sex}\n" +
                $"_arr2[1].name = {_arr2[1].name}\n" +
                $"_arr2[1].id = {_arr2[1].id}\n" +
                $"_arr2[1].isHasPet = {_arr2[1].isHasPet}\n");
        }
	}
}

