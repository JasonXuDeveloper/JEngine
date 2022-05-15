using System;
using UnityEngine;
using System.Linq;
using System.Reflection;
using ILRuntime.Reflection;
using ILRuntime.CLR.TypeSystem;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using Component = UnityEngine.Component;

namespace JEngine.Core
{
    public static partial class Tools
    {
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
#if INIT_JE
            return ClassBindMgr.LoadedScenes.SelectMany(scene => scene.GetRootGameObjects())
                .SelectMany(g => g.GetComponentsInChildren<T>(true))
                .ToList();
#endif
            return null;
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
    }
}