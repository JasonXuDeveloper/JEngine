using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Mono.Cecil.Pdb;
using ILRuntime.Reflection;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using UnityEngine;
using UnityEngine.SceneManagement;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using Component = UnityEngine.Component;
using Object = UnityEngine.Object;

namespace JEngine.Core
{
    public static class Tools
    {
        public static readonly object[] Param0 = Array.Empty<object>();
        private const float Bytes2Mb = 1f / (1024 * 1024);
        
        /// <summary>
        /// 当前时间戳(ms)
        /// </summary>
        /// <returns></returns>
        public static long TimeStamp => (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;

        /// <summary>
        /// 获取下载速度
        /// </summary>
        /// <param name="downloadSpeed"></param>
        /// <returns></returns>
        public static string GetDisplaySpeed(float downloadSpeed)
        {
            if (downloadSpeed >= 1024 * 1024)
            {
                return $"{downloadSpeed * Bytes2Mb:f2}MB/s";
            }
            if (downloadSpeed >= 1024)
            {
                return $"{downloadSpeed / 1024:f2}KB/s";
            }
            return $"{downloadSpeed:f2}B/s";
        }
        
        /// <summary>
        /// 获取显示大小
        /// </summary>
        /// <param name="downloadSize"></param>
        /// <returns></returns>
        public static string GetDisplaySize(long downloadSize)
        {
            if (downloadSize >= 1024 * 1024)
            {
                return $"{downloadSize * Bytes2Mb:f2}MB";
            }
            if (downloadSize >= 1024)
            {
                return $"{downloadSize / 1024:f2}KB";
            }
            return $"{downloadSize:f2}B";
        }

        /// <summary>
        /// 缓存domain
        /// </summary>
        private static AppDomain _cacheDomain;
        
        /// <summary>
        /// ILRuntime的Appdomain
        /// </summary>
        public static AppDomain Domain
        {
            get
            {
                if (_cacheDomain != null)
                {
                    _cacheDomain.Dispose();
                }
                
                if (Application.isPlaying && InitJEngine.Appdomain != null)
                {
                    return InitJEngine.Appdomain;
                }
                _cacheDomain = new AppDomain();
                _cacheDomain.LoadAssembly(new MemoryStream(DLLMgr.FileToByte(DLLMgr.DllPath)),null, new PdbReaderProvider());
                LoadILRuntime.InitializeILRuntime(_cacheDomain);
                return _cacheDomain;
            }
        }

        /// <summary>
        /// 通过字符串获取热更类型
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        public static Type GetHotType(string typename)
        {
            AppDomain ad = Domain;
            var t = ad.GetType(typename);
            ad.Dispose();
            return t?.ReflectionType;
        }
        
        /// <summary>
        /// 通过字符串获取热更IL类型
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        public static IType GetHotILType(string typename)
        {
            AppDomain ad = Domain;
            var t = ad.GetType(typename);
            ad.Dispose();
            return t;
        }
        
        /// <summary>
        /// 通过字符串获取热更类型实例
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        public static ILTypeInstance GetHotInstance(string typename)
        {
            AppDomain ad = Domain;
            var t = ad.GetType(typename);
            ad.Dispose();
            if (t == null) return null;
            return ad.Instantiate(typename);
        }
        
        /// <summary>
        /// 判断是否包含热更类型
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        public static bool HasHotType(string typename)
        {
            AppDomain ad = Domain;
            bool ret = ad.LoadedTypes.ContainsKey(typename);
            ad.Dispose();
            return ret;
        }
        
        /// <summary>
        /// 判断是否继承了JBehaviour
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsJBehaviourType(Type type)
        {
            Type jType = GetHotType("JEngine.Core.JBehaviour");
            if (jType == null)
            {
                return false;
            }
            return type.IsSubclassOf(jType);
        }
        
        /// <summary>
        /// 判断是不是继承了JBehaviour
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        public static bool IsJBehaviourType(string typename)
        {
            AppDomain ad = Domain;
            var t = ad.GetType(typename);
            var jb = ad.GetType("JEngine.Core.JBehaviour");
            bool ret = t.CanAssignTo(jb);
            ad.Dispose();
            return ret;
        }
        
        /// <summary>
        /// 调用热更方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="method"></param>
        public static void InvokeHotMethod(string type, string method)
        {
            InitJEngine.Appdomain.Invoke(type, method, Param0, Param0);
        }

        /// <summary>
        /// 调用热更方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <param name="instance"></param>
        /// <param name="param"></param>
        public static void InvokeHotMethod(string type, string method, object instance, params object[] param)
        {
            InitJEngine.Appdomain.Invoke(type, method, instance, param);
        }

        /// <summary>
        /// 获取热更方法的全部参数
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static ParameterInfo[] GetHotMethodParams(Type type,string methodName)
        {
            return type.GetMethod(methodName)?.GetParameters();
        }
        
        /// <summary>
        /// 获取对象的gameObject
        /// </summary>
        /// <param name="ins"></param>
        /// <returns></returns>
        public static GameObject GetGameObject(this object ins)
        {
            GameObject instance;
            if (ins is GameObject g)
            {
                instance = g;
            }
            else if (ins is ILTypeInstance ilt)
            {
                instance = FindGOForHotClass(ilt);
            }
            else if(ins is Transform t)
            {
                instance = t.gameObject;
            }
            else if (ins is Component c)
            {
                instance = c.gameObject;
            }
            else
            {
                instance = null;
            }

            return instance;
        }

        /// <summary>
        /// 找到热更对象的gameObject
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static GameObject FindGOForHotClass(this ILTypeInstance instance)
        {
            var returnType = instance.Type;
            PropertyInfo pi = null;
            if (returnType.ReflectionType == typeof(MonoBehaviour))
            {
                pi = returnType.ReflectionType.GetProperty("gameObject");
            }

            if (returnType.ReflectionType.IsSubclassOf(typeof(MonoBehaviour)))
            {
                if (returnType.ReflectionType.BaseType != null)
                {
                    pi = returnType.ReflectionType.BaseType.GetProperty("gameObject");
                }
            }

            return pi?.GetValue(instance.CLRInstance) as GameObject;
        }

        /// <summary>
        /// 获取全部Type对象（包含隐藏的）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> FindObjectsOfTypeAll<T>()
        {
            if (!Application.isPlaying)
            {
                return SceneManager.GetActiveScene().GetRootGameObjects()
                    .SelectMany(g => g.GetComponentsInChildren<T>(true))
                    .ToList();
            }
            return ClassBindMgr.LoadedScenes.SelectMany(scene => scene.GetRootGameObjects())
                .SelectMany(g => g.GetComponentsInChildren<T>(true))
                .ToList();
        }
        
        /// <summary>
        /// Get a class instance from a gameObject, can be either hot or local
        /// </summary>
        /// <param name="fieldType"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetInstanceFromGO(this Type fieldType, Object obj)
        {
            if (obj.GetType() == fieldType || obj.GetType().FullName == fieldType.FullName)
            {
                return obj;
            }

            GameObject go;
            if (obj is GameObject o)
            {
                go = o;
            }
            else
            {
                go = (obj as Component)?.gameObject;
            }

            if (go is null) return null;

            if (fieldType == typeof(Transform))
            {
                var rect = go.GetComponent<RectTransform>();
                if (rect != null)
                {
                    return rect;
                }
                return go.transform;
            }

            if (fieldType is ILRuntimeType ilType) //如果在热更中
            {
                var components = go.GetComponents<CrossBindingAdaptorType>();
                foreach (var c in components)
                {
                    if (c.ILInstance.Type.CanAssignTo(ilType.ILType))
                    {
                        return c.ILInstance;
                    }
                }
            }
            else
            {
                if (fieldType is ILRuntimeWrapperType type)
                {
                    fieldType = type.RealType;
                }

                return go.GetComponent(fieldType);
            }

            return null;
        }

        /// <summary>
        /// 判断是否可以被分配到类型
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="type"></param>
        public static bool CanAssignTo(this object instance, Type type)
        {
            return ((ILTypeInstance)instance).Type.CanAssignTo(InitJEngine.Appdomain.GetType(type.FullName));
        }
        
        /// <summary>
        /// 获取场景内全部MonoBehaviour适配器
        /// </summary>
        /// <returns></returns>
        public static List<CrossBindingAdaptorType> GetAllMonoAdapters()
        {
            return Object.FindObjectsOfType<MonoBehaviour>().ToList()
                .FindAll(x => x.GetType().GetInterfaces().Contains(typeof(CrossBindingAdaptorType))).Select(x => (CrossBindingAdaptorType)x)
                .ToList();
        }

        /// <summary>
        /// 获取热更对象（注意：这个会返回一个ILTypeInstance数组，需要把object转ILTypeInstance[]后判断长度然后取第一个元素
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static object GetHotComponent(this GameObject gameObject, string typeName)
        {
            var clrInstances = gameObject.GetComponents<CrossBindingAdaptorType>();
            return clrInstances.ToList()
                .FindAll(a =>
                    a.ILInstance != null && a.ILInstance.Type.CanAssignTo(InitJEngine.Appdomain.GetType(typeName)))
                .Select(a => a.ILInstance).ToArray();
        }

        /// <summary>
        /// 获取热更对象（注意：这个会返回一个ILTypeInstance数组，需要把object转ILTypeInstance[]后判断长度然后取第一个元素
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetHotComponent(this GameObject gameObject, ILType type)
        {
            var clrInstances = gameObject.GetComponents<CrossBindingAdaptorType>();
            return clrInstances.ToList()
                .FindAll(a => a.ILInstance != null && a.ILInstance.Type.CanAssignTo(type))
                .Select(a => a.ILInstance).ToArray();
        }
        
        /// <summary>
        /// 获取热更对象（注意：这个会返回一个ILTypeInstance数组，需要把object转ILTypeInstance[]后判断长度然后取第一个元素
        /// </summary>
        /// <param name="adapters"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetHotComponent(this CrossBindingAdaptorType[] adapters, ILType type)
        {
            return adapters.ToList()
                .FindAll(a => a.ILInstance != null && a.ILInstance.Type.CanAssignTo(type))
                .Select(a => a.ILInstance).ToArray();
        }
        
        /// <summary>
        /// 获取热更对象（注意：这个会返回一个ILTypeInstance数组，需要把object转ILTypeInstance[]后判断长度然后取第一个元素
        /// </summary>
        /// <param name="adapters"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetHotComponent(this List<CrossBindingAdaptorType> adapters, ILType type)
        {
            return adapters
                .FindAll(a => a.ILInstance != null && a.ILInstance.Type.CanAssignTo(type))
                .Select(a => a.ILInstance).ToArray();
        }

        public static void DestroyHotComponent(this GameObject gameObject, object hotObject)
        {
            var clrInstances = gameObject.GetComponents<CrossBindingAdaptorType>();
            var objs = clrInstances.ToList()
                .FindAll(a => a.ILInstance != null && Equals(a.ILInstance, hotObject));
            foreach (var obj in objs)
            {
                Object.Destroy(obj as MonoBehaviour);
            }
        }

        public static object GetHotComponentInChildren(this GameObject gameObject, string typeName)
        {
            var clrInstances = gameObject.GetComponentsInChildren<CrossBindingAdaptorType>(true);
            return clrInstances.ToList()
                .FindAll(a => a.ILInstance != null && a.ILInstance.Type.CanAssignTo(InitJEngine.Appdomain.GetType(typeName)))
                .Select(a => a.ILInstance).ToArray();
        }
        
        
        /// <summary>
        /// 将对象转换为特定类型
        /// </summary>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public static object ConvertSimpleType(this object value, Type destinationType)
        {
            object returnValue;
            if (value == null || destinationType.IsInstanceOfType(value))
            {
                return value;
            }

            if (value is string str && str.Length == 0)
            {
                return destinationType.IsValueType ? Activator.CreateInstance(destinationType) : null;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(destinationType);
            bool flag = converter.CanConvertFrom(value.GetType());
            if (!flag)
            {
                converter = TypeDescriptor.GetConverter(value.GetType());
            }

            if (!flag && !converter.CanConvertTo(destinationType))
            {
                Log.PrintError("无法转换成类型：'" + value + "' ==> " + destinationType);
            }

            try
            {
                returnValue = flag
                    ? converter.ConvertFrom(null, null, value)
                    : converter.ConvertTo(null, null, value, destinationType);
            }
            catch (Exception e)
            {
                Log.PrintError("类型转换出错：'" + value + "' ==> " + destinationType + "\n" + e.Message);
                returnValue = destinationType.IsValueType ? Activator.CreateInstance(destinationType) : null;
            }

            return returnValue;
        }
    }
}