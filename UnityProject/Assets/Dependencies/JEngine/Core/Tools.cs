using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Utils;
using ILRuntime.Reflection;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using Object = System.Object;

namespace JEngine.Core
{
    public class Tools
    {
        public static object ConvertSimpleType(object value, Type destinationType)
        {
            object returnValue;
            if ((value == null) || destinationType.IsInstanceOfType(value))
            {
                return value;
            }

            string str = value as string;
            if ((str != null) && (str.Length == 0))
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


        /// <summary>
        /// 找到热更对象的gameObject
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static GameObject FindGOForHotClass(ILTypeInstance instance)
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

            return pi.GetValue(instance.CLRInstance) as GameObject;
        }

        public static List<T> FindObjectsOfTypeAll<T>()
        {
            return ClassBindMgr.LoadedScenes.SelectMany(scene => scene.GetRootGameObjects())
                .SelectMany(g => g.GetComponentsInChildren<T>(true))
                .ToList();
        }

        public static object GetHotComponent(GameObject gameObject, string typeName)
        {
            var clrInstances = gameObject.GetComponents<CrossBindingAdaptorType>();
            return clrInstances.ToList()
                .FindAll(a => a.ILInstance != null && (a.ILInstance.Type.ReflectionType.FullName == typeName ||
                                                       a.ILInstance.Type.ReflectionType.Name == typeName))
                .Select(a => a.ILInstance).ToArray();
        }

        public static object GetHotComponent(GameObject gameObject, ILType type)
        {
            var clrInstances = gameObject.GetComponents<CrossBindingAdaptorType>();
            return clrInstances.ToList()
                .FindAll(a => a.ILInstance != null && a.ILInstance.Type == type)
                .Select(a => a.ILInstance).ToArray();
        }
        
        public static object GetHotComponent(CrossBindingAdaptorType[] adapters, ILType type)
        {
            return adapters.ToList()
                .FindAll(a => a.ILInstance != null && a.ILInstance.Type == type)
                .Select(a => a.ILInstance).ToArray();
        }
        
        public static object GetHotComponent(List<CrossBindingAdaptorType> adapters, ILType type)
        {
            return adapters
                .FindAll(a => a.ILInstance != null && a.ILInstance.Type == type)
                .Select(a => a.ILInstance).ToArray();
        }

        public static void DestroyHotComponent(GameObject gameObject, object hotObject)
        {
            var clrInstances = gameObject.GetComponents<CrossBindingAdaptorType>();
            var objs = clrInstances.ToList()
                .FindAll(a => a.ILInstance != null && Equals(a.ILInstance, hotObject));
            foreach (var obj in objs)
            {
                UnityEngine.Object.Destroy(obj as MonoBehaviour);
            }
        }

        public static object GetHotComponentInChildren(GameObject gameObject, string typeName)
        {
            var clrInstances = gameObject.GetComponentsInChildren<CrossBindingAdaptorType>(true);
            return clrInstances.ToList()
                .FindAll(a => a.ILInstance != null && (a.ILInstance.Type.ReflectionType.FullName == typeName ||
                                                       a.ILInstance.Type.ReflectionType.Name == typeName))
                .Select(a => a.ILInstance).ToArray();
        }
    }
}