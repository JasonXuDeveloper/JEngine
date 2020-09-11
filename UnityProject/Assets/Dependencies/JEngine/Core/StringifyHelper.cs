using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using libx;
using LitJson;
using ProtoBuf;
using UnityEngine;
using Object = System.Object;

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
        public static byte[] ProtoSerialize(object obj)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, obj);
                    byte[] result = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(result, 0, result.Length);
                    return result;
                }
            }
            catch (IOException e)
            {
                Log.PrintError(e);
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
        public static T ProtoDeSerialize<T>(string path) where T : class
        {
            try
            {
                var res = Assets.LoadAsset(path, typeof(TextAsset));
                using (Stream file =
                    new MemoryStream(((TextAsset) res.asset).bytes))
                {
                    res.Release();
                    return Serializer.Deserialize<T>(file);
                }
            }
            catch (IOException e)
            {
                Log.PrintError(e);
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
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(msg, 0, msg.Length);
                    ms.Position = 0;
                    return Serializer.Deserialize<T>(ms);
                }
            }
            catch (IOException e)
            {
                Log.PrintError(e);
                return null;
            }
        }
        
        #endregion


        #region BinaryFormatter
        /// <summary>
        /// 类转换成二进制
        /// Convert object to byte[]
        /// </summary>
        /// <param name="path"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] BinarySerialize(System.Object obj)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream memory = new MemoryStream();
                bf.Serialize(memory, obj);
                byte[] bytes = memory.GetBuffer();
                memory.Close();
                return bytes;
            }
            catch (Exception e)
            {
                Log.PrintError("此类无法转换成二进制 " + obj.GetType() + "," + e);
            }

            return null;
        }
        
        /// <summary>
        /// 通过二进制转类
        /// Use byte[] to get class from BinarySerialize
        /// </summary>
        /// <param name="bytes"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T BinaryDeSerialize<T>(byte[] bytes) where T : class
        {
            T t = default(T);
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream memory = new MemoryStream(bytes);
            t = (T)bf.Deserialize(memory);
            memory.Close();
            return t;
        }


        /// <summary>
        /// 通过文件转类（仅限热更资源）
        /// Use file to load class (only hot update resource)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T BinaryDeSerialize<T>(string path) where T : class
        {
            T t = default(T);
            var res = Assets.LoadAsset(path, typeof(TextAsset));
            TextAsset textAsset = (TextAsset) res.asset;

            if (textAsset == null)
            {
                Log.PrintError("cant load TextAsset: " + path);
                return null;
            }

            try
            {
                using (MemoryStream stream = new MemoryStream(textAsset.bytes))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    t = (T) bf.Deserialize(stream);
                }

                res.Release();
            }
            catch (Exception e)
            {
                Log.PrintError("load TextAsset exception: " + path + "," + e);
            }

            return t;
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
            catch (IOException e)
            {
                Log.PrintError(e);
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
            catch (IOException e)
            {
                Log.PrintError(e);
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
                TextAsset textAsset = (TextAsset) res.asset;

                if (textAsset == null)
                {
                    Log.PrintError("cant load TextAsset: " + path);
                    return default(T);
                }
                
                var jsonObj = JsonMapper.ToObject<T>(textAsset.text);
                res.Release();
                return jsonObj;
            }
            catch (IOException e)
            {
                Log.PrintError(e);
                return default(T);
            }
        }
        #endregion
    }
}