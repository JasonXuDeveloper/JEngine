//
// StringifyHelper.cs
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
using System.IO;
using libx;
using LitJson;
using UnityEngine;

namespace JEngine.Core
{
    public class StringifyHelper
    {
        #region Protobuf
        /// <summary>
        /// 序列化并返回二进制
        /// Serialize an Object and return byte[]
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ProtoSerialize<T>(T obj) where T : class
        {
            try
            {
                using (var stream = new System.IO.MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize(stream, obj);
                    return stream.ToArray();
                }
            }
            catch (IOException ex)
            {
                Log.PrintError($"[StringifyHelper] 错误：{ex.Message}, {ex.Data["StackTrace"]}");
                return null;
            }
        }

        /// <summary>
        /// 获取文件来反序列化（只能是可热更资源）
        /// Use file to deserialize (only hot update resources)
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ProtoDeSerializeFromFile<T>(string path) where T : class
        {
            try
            {
                var res = Assets.LoadAsset(path, typeof(TextAsset));
                return ProtoBuf.Serializer.Deserialize(typeof(T), new System.IO.MemoryStream(res.bytes)) as T;
            }
            catch (IOException ex)
            {
                Log.PrintError($"[StringifyHelper] 错误：{ex.Message}, {ex.Data["StackTrace"]}");
                return null;
            }
        }

        /// <summary>
        /// 通过二进制反序列化
        /// Deserialize with byte[]
        /// </summary>
        /// <param name="msg"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ProtoDeSerialize<T>(byte[] msg) where T : class
        {
            try
            {
                return ProtoBuf.Serializer.Deserialize(typeof(T), new System.IO.MemoryStream(msg)) as T;
            }
            catch (IOException ex)
            {
                Log.PrintError($"[StringifyHelper] 错误：{ex.Message}, {ex.Data["StackTrace"]}");
                return null;
            }
        }

        #endregion

        #region JSON
        /// <summary>
        /// 将类转换至JSON字符串
        /// Convert object to JSON string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string JSONSerliaze(object value)
        {
            try
            {
                var json = JsonMapper.ToJson(value);
                return json;
            }
            catch (IOException ex)
            {
                Log.PrintError($"[StringifyHelper] 错误：{ex.Message}, {ex.Data["StackTrace"]}");
                return null;
            }
        }

        /// <summary>
        /// 将JSON字符串转类
        /// Convert JSON string to Class
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T JSONDeSerliaze<T>(string value)
        {
            try
            {
                var jsonObj = JsonMapper.ToObject<T>(value);
                return jsonObj;
            }
            catch (IOException ex)
            {
                Log.PrintError($"[StringifyHelper] 错误：{ex.Message}, {ex.Data["StackTrace"]}");
                return default(T);
            }
        }

        /// <summary>
        /// 将文件中的JSON字符串转类（仅限热更资源）
        /// Convert JSON string from file to class (only hot update files)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T JSONDeSerliazeFromFile<T>(string path)
        {
            try
            {
                var res = Assets.LoadAsset(path, typeof(TextAsset));
                TextAsset textAsset = (TextAsset)res.asset;

                if (textAsset == null)
                {
                    Log.PrintError("cant load TextAsset: " + path);
                    return default(T);
                }

                var jsonObj = JsonMapper.ToObject<T>(textAsset.text);
                res.Release();
                return jsonObj;
            }
            catch (IOException ex)
            {
                Log.PrintError($"[StringifyHelper] 错误：{ex.Message}, {ex.Data["StackTrace"]}");
                return default(T);
            }
        }
        #endregion
    }
}