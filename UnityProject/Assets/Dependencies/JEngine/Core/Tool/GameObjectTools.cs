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

#if PLUGIN_NINO
using Nino.Shared.IO;
#endif

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
        public static List<T> FindObjectsOfTypeAll<T>() where T: Object
        {
            if (!Application.isPlaying)
            {
                return SceneManager.GetActiveScene().GetRootGameObjects()
                    .SelectMany(g => g.GetComponentsInChildren<T>(true))
                    .ToList();
            }
#if PLUGIN_NINO
            List<GameObject> all = null;
            List<GameObject> temp = null;
            foreach (var scene in ClassBindMgr.LoadedScenes)
            {
                if (all == null)
                {
                    all = ObjectPool<List<GameObject>>.Peak() != null ? ObjectPool<List<GameObject>>.Request() : new List<GameObject>(scene.rootCount);
                    all.Clear();
                }
                if (temp == null)
                {
                    temp = ObjectPool<List<GameObject>>.Peak() != null ? ObjectPool<List<GameObject>>.Request() : new List<GameObject>(scene.rootCount);
                }
                scene.GetRootGameObjects(temp);
                all.AddRange(temp);
            }
            if (all == null)
            {
                return new List<T>();
            }
            temp.Clear();
            ObjectPool<List<GameObject>>.Return(temp);
            List<T> lst = ObjectPool<List<T>>.Peak() != null ? ObjectPool<List<T>>.Request() : new List<T>(all.Count);
            List<T> tempT = ObjectPool<List<T>>.Peak() != null ? ObjectPool<List<T>>.Request() : new List<T>(all.Count);
            lst.Clear();
            tempT.Clear();
            foreach (var gameObject in all)
            {
                gameObject.GetComponentsInChildren(true, tempT);
                lst.AddRange(tempT);
                tempT.Clear();
            }
            all.Clear();
            ObjectPool<List<GameObject>>.Return(all);
            ObjectPool<List<T>>.Return(tempT);
            return lst;
#endif
            return ClassBindMgr.LoadedScenes.SelectMany(scene => scene.GetRootGameObjects())
                .SelectMany(g => g.GetComponentsInChildren<T>(true))
                .ToList();
            return UnityEngine.Object.FindObjectsOfType<T>().ToList();
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
        /// 获取场景内全部MonoBehaviour适配器（CrossBindingAdaptorType）
        /// </summary>
        /// <returns></returns>
        public static List<CrossBindingAdaptorType> GetAllMonoAdapters()
        {
            List<CrossBindingAdaptorType> adapters = new List<CrossBindingAdaptorType>();
            List<CrossBindingAdaptorType> searchResult = new List<CrossBindingAdaptorType>();
            foreach (var scene in ClassBindMgr.LoadedScenes)
            {
                var rootGameObjects = scene.GetRootGameObjects();
                foreach (var rootGameObject in rootGameObjects)
                {
                    searchResult.Clear();
                    rootGameObject.GetComponentsInChildren(true, searchResult);
                    foreach (var crossBindingAdaptorType in searchResult)
                    {
                        if (crossBindingAdaptorType?.ILInstance != null)
                        {
                            adapters.Add(crossBindingAdaptorType);
                        }
                    }
                }
            }
            
            return adapters;
        }

        /// <summary>
        /// 获取热更对象
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static object GetHotComponent(this GameObject gameObject, string typeName)
        {
            var clrInstances = gameObject.GetComponents<CrossBindingAdaptorType>();
            var type = InitJEngine.Appdomain.GetType(typeName);
            foreach (var clrInstance in clrInstances)
            {
                if (clrInstance.ILInstance != null && clrInstance.ILInstance.Type.CanAssignTo(type))
                {
                    return clrInstance.ILInstance;
                }
            }

            return null;
        }
        
        /// <summary>
        /// 获取热更对象
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static object[] GetHotComponents(this GameObject gameObject, string typeName)
        {
            var clrInstances = gameObject.GetComponents<CrossBindingAdaptorType>();
            var type = InitJEngine.Appdomain.GetType(typeName);
            object[] ret = new object[clrInstances.Length];
            int i = 0;
            foreach (var clrInstance in clrInstances)
            {
                if (clrInstance.ILInstance != null && clrInstance.ILInstance.Type.CanAssignTo(type))
                {
                    ret[i] = clrInstance.ILInstance;
                    i++;
                }
            }
            Array.Resize(ref ret, i);
            
            return ret;
        }

        /// <summary>
        /// 获取热更对象
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetHotComponent(this GameObject gameObject, ILType type)
        {
            var clrInstances = gameObject.GetComponents<CrossBindingAdaptorType>();
            foreach (var clrInstance in clrInstances)
            {
                if (clrInstance.ILInstance != null && clrInstance.ILInstance.Type.CanAssignTo(type))
                {
                    return clrInstance.ILInstance;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 获取热更对象
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object[] GetHotComponents(this GameObject gameObject, ILType type)
        {
            var clrInstances = gameObject.GetComponents<CrossBindingAdaptorType>();
            object[] ret = new object[clrInstances.Length];
            int i = 0;
            foreach (var clrInstance in clrInstances)
            {
                if (clrInstance.ILInstance != null && clrInstance.ILInstance.Type.CanAssignTo(type))
                {
                    ret[i] = clrInstance.ILInstance;
                    i++;
                }
            }
            Array.Resize(ref ret, i);

            return ret;
        }
        
        /// <summary>
        /// 获取热更对象
        /// </summary>
        /// <param name="adapters"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetHotComponent(this CrossBindingAdaptorType[] adapters, ILType type)
        {
            foreach (var adapter in adapters)
            {
                if (adapter.ILInstance != null && adapter.ILInstance.Type.CanAssignTo(type))
                {
                    return adapter.ILInstance;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 获取热更对象
        /// </summary>
        /// <param name="adapters"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object[] GetHotComponents(this CrossBindingAdaptorType[] adapters, ILType type)
        {
            object[] ret = new object[adapters.Length];
            int i = 0;
            foreach (var adapter in adapters)
            {
                if (adapter.ILInstance != null && adapter.ILInstance.Type.CanAssignTo(type))
                {
                    ret[i] = adapter.ILInstance;
                    i++;
                }
            }
            Array.Resize(ref ret, i);

            return ret;
        }

        /// <summary>
        /// 获取热更对象
        /// </summary>
        /// <param name="adapters"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetHotComponent(this List<CrossBindingAdaptorType> adapters, ILType type)
        {
            foreach (var adapter in adapters)
            {
                if (adapter.ILInstance != null && adapter.ILInstance.Type.CanAssignTo(type))
                {
                    return adapter.ILInstance;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 获取热更对象
        /// </summary>
        /// <param name="adapters"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object[] GetHotComponents(this List<CrossBindingAdaptorType> adapters, ILType type)
        {
            object[] ret = new object[adapters.Count];
            int i = 0;
            foreach (var adapter in adapters)
            {
                if (adapter.ILInstance != null && adapter.ILInstance.Type.CanAssignTo(type))
                {
                    ret[i] = adapter.ILInstance;
                    i++;
                }
            }
            Array.Resize(ref ret, i);

            return ret;
        }

        /// <summary>
        /// 删除挂载GameObject上的热更脚本
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="hotObject"></param>
        public static void DestroyHotComponent(this GameObject gameObject, object hotObject)
        {
            var clrInstances = gameObject.GetComponents<CrossBindingAdaptorType>();
            foreach (var clrInstance in clrInstances)
            {
                if (clrInstance.ILInstance != null && Equals(clrInstance.ILInstance, hotObject))
                {
                    Object.Destroy(clrInstance as MonoBehaviour);
                }
            }
        }

        /// <summary>
        /// 获取子对象上的热更对象
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static object[] GetHotComponentInChildren(this GameObject gameObject, string typeName)
        {
            var clrInstances = gameObject.GetComponentsInChildren<CrossBindingAdaptorType>(true);
            var type = InitJEngine.Appdomain.GetType(typeName);
            object[] ret = new object[clrInstances.Length];
            int i = 0;
            foreach (var clrInstance in clrInstances)
            {
                if (clrInstance.ILInstance != null && clrInstance.ILInstance.Type.CanAssignTo(type))
                {
                    ret[i] = clrInstance.ILInstance;
                    i++;
                }
            }
            Array.Resize(ref ret, i);
            
            return ret;
        }
    }
}