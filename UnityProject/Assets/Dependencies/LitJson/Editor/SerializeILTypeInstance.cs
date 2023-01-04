using System;
using System.Collections.Generic;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter;
using LitJson;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace JEngine.Editor
{
    public static partial class SerializeILTypeInstance
    {
        [SerializeTypeMethod(4)]
        public static bool SerializeJsonDataType(AnimBool[] fadeGroup, Type cType, IType type, ILTypeInstance instance,
            KeyValuePair<string, int> i,
            string name)
        {
            if (cType == typeof(JsonData)) //可以折叠显示Json数据
            {
                var objValue = i.Value;
                if (instance[objValue] != null)
                {
                    fadeGroup[1].target = EditorGUILayout.Foldout(fadeGroup[1].target, name, true);
                    if (EditorGUILayout.BeginFadeGroup(fadeGroup[1].faded))
                    {
                        instance[objValue] = EditorGUILayout.TextArea(
                            ((JsonData) instance[objValue]).ToString()
                        );
                    }

                    EditorGUILayout.EndFadeGroup();
                    EditorGUILayout.Space();
                }
                else
                {
                    EditorGUILayout.LabelField(name, "暂无值的JsonData");
                }

                return true;
            }

            return false;
        }
    }
}