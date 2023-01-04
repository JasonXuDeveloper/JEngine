using System;
using System.Collections.Generic;
using System.Reflection;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Reflection;
using ILRuntime.Runtime.Intepreter;
using JEngine.Misc;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace JEngine.Editor
{
    public static partial class SerializeILTypeInstance
    {
        [SerializeTypeMethod(2)]
        public static bool SerializeBindablePropertyType(AnimBool[] fadeGroup, Type cType, IType type,
            ILTypeInstance instance, KeyValuePair<string, int> i,
            string name)
        {
            var objValue = i.Value;
            var objName = i.Key;
            var obj = instance[objValue];
            //可绑定值，可以尝试更改
            if (type.ReflectionType.IsGenericType && type.ReflectionType.GetGenericTypeDefinition() == typeof(BindableProperty<>) && obj != null)
            {
                PropertyInfo fi = type.ReflectionType.GetProperty("Value");
                object val = fi?.GetValue(obj);

                Type genericType = type.ReflectionType.GenericTypeArguments[0];
                genericType = genericType is ILRuntimeWrapperType wt ? wt.RealType : genericType;
                if (genericType == null ||
                    (!genericType.IsPrimitive && genericType != typeof(string))) //不是基础类型或字符串
                {
                    EditorGUILayout.LabelField(objName, val?.ToString()); //只显示字符串
                }
                else
                {
                    //可更改
                    var data = JEngine.Core.Tools.ConvertSimpleType(EditorGUILayout.TextField(objName, val?.ToString()),
                        genericType);
                    if (data != null) //尝试更改
                    {
                        fi?.SetValue(obj, data);
                    }
                }

                return true;
            }

            return false;
        }
    }
}