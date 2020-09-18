using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

namespace HotFix_Project
{
    public class TestJson
    {
        enum JsonTestEnum
        {
            Test1,
            Test2,
            Test3,
        }
        class JsonTestClass
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }
            public JsonTestEnum EnumProp { get; set; }
            public JsonTestSubClass SubClassProp { get; set; }
            public Dictionary<string, JsonTestSubClass> DicTest { get; set; }
            public Dictionary<string, int> DicTest2 { get; set; }
        }

        class JsonTestSubClass
        {
            public long LongProp { get; set; }
            public List<JsonTestSubClass> SubClassList { get; set; }
            public JsonTestSubClass[] ArrayProp { get; set; }
        }

        public static void RunTest()
        {
            JsonTestClass cls = new JsonTestClass();
            cls.IntProp = 1;
            cls.StringProp = "2";
            cls.EnumProp = JsonTestEnum.Test3;
            var sub = new JsonTestSubClass();
            sub.LongProp = 4;
            var sub2 = new JsonTestSubClass();
            sub2.LongProp = 5;
            var sub3 = new JsonTestSubClass();
            sub3.LongProp = 6;

            cls.SubClassProp = sub;
            sub.ArrayProp = new JsonTestSubClass[2];
            sub.ArrayProp[0] = sub2;
            sub.ArrayProp[1] = sub3;
            sub.SubClassList = new List<JsonTestSubClass>();
            sub.SubClassList.Add(sub2);
            sub.SubClassList.Add(sub3);

            cls.DicTest = new Dictionary<string, JsonTestSubClass>();
            cls.DicTest["11111"] = sub;
            cls.DicTest2 = new Dictionary<string, int>();
            cls.DicTest2["111222"] = 333444;

            var str = JsonMapper.ToJson(cls);
            Debug.Log("---------------");
            Debug.Log(str);
            Debug.Log("---------------");
            var cls2 = JsonMapper.ToObject<JsonTestClass>(str);
            Debug.Log(cls2.SubClassProp.ArrayProp[0].LongProp);
            Debug.Log(cls2.SubClassProp.ArrayProp[1].LongProp);
            Debug.Log(cls2.SubClassProp.SubClassList[0].LongProp);
            Debug.Log(cls2.SubClassProp.SubClassList[1].LongProp);
            Debug.Log(cls2.DicTest["11111"].LongProp);
            Debug.Log(cls2.DicTest2["111222"]);
        }
    }
}
