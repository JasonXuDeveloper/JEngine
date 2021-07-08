using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ILRuntime.Runtime.Enviorment;
using UnityEngine;
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
                returnValue = flag ? converter.ConvertFrom(null, null, value) : converter.ConvertTo(null, null, value, destinationType); 
            } 
            catch (Exception e)
            {
                Log.PrintError("类型转换出错：'" + value + "' ==> " + destinationType + "\n" + e.Message);
                returnValue = destinationType.IsValueType ? Activator.CreateInstance(destinationType) : null;
            } 
            return returnValue; 
        } 

        public static List<T> FindObjectsOfTypeAll<T>()
        {
            return  ClassBindMgr.LoadedScenes.SelectMany(scene=>scene.GetRootGameObjects())
                .SelectMany(g => g.GetComponentsInChildren<T>(true))
                .ToList();
        }
        
        public static object GetHotComponent(GameObject gameObject,string typeName)
        {
            var clrInstances = gameObject.GetComponents<CrossBindingAdaptorType>();
            return clrInstances.ToList()
                .FindAll(a => a.ILInstance != null && a.ILInstance.Type.ReflectionType.FullName == typeName)
                .Select(a => a.ILInstance).ToArray();
        }
        
        public static void DestroyHotComponent(GameObject gameObject,object hotObject)
        {
            var clrInstances = gameObject.GetComponents<CrossBindingAdaptorType>();
            var objs = clrInstances.ToList()
                .FindAll(a => a.ILInstance != null && a.ILInstance == hotObject);
            foreach (var obj in objs)
            {
                UnityEngine.Object.Destroy(obj as MonoBehaviour);
            }
        }

        
        public static object GetHotComponentInChildren(GameObject gameObject,string typeName)
        {
            var clrInstances = gameObject.GetComponentsInChildren<CrossBindingAdaptorType>();
            return clrInstances.ToList()
                .FindAll(a => a.ILInstance != null && a.ILInstance.Type.ReflectionType.FullName == typeName)
                .Select(a => a.ILInstance).ToArray();
        }
    }
}