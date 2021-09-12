using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using JEngine.Core;
using LitJson;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using ILRuntime.CLR.TypeSystem;
using Object = UnityEngine.Object;

namespace JEngine.Editor
{
    public static partial class SerializeILTypeInstance
    {
        public static int FadeGroupNum = 15;

        private static Dictionary<int, MethodInfo> _serializeMethods;

        public static Dictionary<int, MethodInfo> GetSerializeMethods()
        {
            //序列化成员变量
            if (_serializeMethods == null)
            {
                _serializeMethods = new Dictionary<int, MethodInfo>(0);
                var allMethods = typeof(SerializeILTypeInstance).GetMethods(BindingFlags.Public | BindingFlags.Static);
                foreach (var method in allMethods)
                {
                    var attr = method.GetCustomAttributes(typeof(SerializeTypeMethod), false);
                    if (attr.Length > 0)
                    {
                        _serializeMethods.Add(((SerializeTypeMethod) attr[0]).Priority, method);
                    }
                }

                _serializeMethods = _serializeMethods.OrderByDescending(s => s.Key)
                    .ToDictionary(s => s.Key, m => m.Value);

            }

            return _serializeMethods;
        }

        public static async Task OnEnable(AnimBool[] fadeGroup, UnityAction repaint)
        {
            for (int i = 0; i < fadeGroup.Length; i++)
            {
                fadeGroup[i] = new AnimBool(false);
                fadeGroup[i].valueChanged.AddListener(repaint);
            }

            while (Application.isEditor && Application.isPlaying)
            {
                try
                {
                    repaint();
                }
                catch
                {
                    //ignored
                }

                await Task.Delay(500);
            }
        }

        public static void OnDisable(AnimBool[] fadeGroup, UnityAction repaint)
        {
            // 移除动画监听
            foreach (var t in fadeGroup)
            {
                t.valueChanged.RemoveListener(repaint);
            }
        }

        public static void OnDestroy(ref bool displaying)
        {
            if (displaying)
                displaying = false;
        }

        public static bool NeedToHide(ILTypeInstance instance, string objName)
        {
            MemberInfo info = instance.Type.ReflectionType.GetField(objName);
            if (info == null)
            {
                info = instance.Type.ReflectionType.GetProperty(objName);
            }

            if (info != null)
            {
                var attr = info.GetCustomAttributes(typeof(HideInInspector), false);
                if (attr.Length > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool SerializeJBehaviourType(AnimBool[] fadeGroup, ILTypeInstance instance)
        {
            //如果JBehaviour
            var jBehaviourType = InitJEngine.Appdomain.LoadedTypes["JEngine.Core.JBehaviour"];
            var t = instance.Type.ReflectionType;
            if (t.IsSubclassOf(jBehaviourType.ReflectionType))
            {
                var f = t.GetField("_instanceID", BindingFlags.NonPublic);
                if (!(f is null))
                {
                    GUI.enabled = false;
                    var id = f.GetValue(instance).ToString();
                    EditorGUILayout.TextField("InstanceID", id);
                    GUI.enabled = true;
                }

                string frameModeStr = "FrameMode";
                string frequencyStr = "Frequency";
                string pausedStr = "Paused";
                string totalTimeStr = "TotalTime";
                string loopDeltaTimeStr = "LoopDeltaTime";
                string loopCountsStr = "LoopCounts";
                string timeScaleStr = "TimeScale";

                fadeGroup[0].target = EditorGUILayout.Foldout(fadeGroup[0].target,
                    "JBehaviour Stats", true);
                if (EditorGUILayout.BeginFadeGroup(fadeGroup[0].faded))
                {
                    var fm = t.GetField(frameModeStr, BindingFlags.Public);
                    bool frameMode = !(fm is null) &&
                                     EditorGUILayout.Toggle(frameModeStr, (bool) fm.GetValue(instance));
                    fm?.SetValue(instance, frameMode);

                    var fq = t.GetField(frequencyStr, BindingFlags.Public);
                    if (!(fq is null))
                    {
                        int frequency = EditorGUILayout.IntField(frequencyStr, (int) fq.GetValue(instance));
                        fq.SetValue(instance, frequency);
                    }

                    GUI.enabled = false;

                    var paused = t.GetField(pausedStr, BindingFlags.NonPublic);
                    if (!(paused is null)) EditorGUILayout.Toggle(pausedStr, (bool) paused.GetValue(instance));

                    var totalTime = t.GetField(totalTimeStr, BindingFlags.Public);
                    if (!(totalTime is null))
                        EditorGUILayout.FloatField(totalTimeStr, (float) totalTime.GetValue(instance));

                    var loopDeltaTime = t.GetField(loopDeltaTimeStr, BindingFlags.Public);
                    if (!(loopDeltaTime is null))
                        EditorGUILayout.FloatField(loopDeltaTimeStr, (float) loopDeltaTime.GetValue(instance));

                    var loopCounts = t.GetField(loopCountsStr, BindingFlags.Public);
                    if (!(loopCounts is null))
                        EditorGUILayout.LongField(loopCountsStr, (long) loopCounts.GetValue(instance));

                    GUI.enabled = true;

                    var timeScale = t.GetField(timeScaleStr, BindingFlags.Public);
                    if (!(timeScale is null))
                    {
                        var ts = EditorGUILayout.FloatField(timeScaleStr, (float) timeScale.GetValue(instance));
                        timeScale.SetValue(instance, ts);
                    }
                }

                EditorGUILayout.EndFadeGroup();

                if (instance.Type.FieldMapping.Count > 0)
                {
                    EditorGUILayout.Space(10);
                    EditorGUILayout.HelpBox(String.Format(Setting.GetString(SettingString.MemberVariables), t.Name),
                        MessageType.Info);
                }

                return true;
            }

            return false;
        }
    }

    public static partial class SerializeILTypeInstance
    {
        [SerializeTypeMethod(0)]
        public static bool SerializeClassBindHotType(AnimBool[] fadeGroup, Type cType, IType type,
            ILTypeInstance instance, KeyValuePair<string, int> i,
            string name)
        {
            var objValue = i.Value;
            var objName = i.Key;
            var obj = instance[objValue];
            try
            {
                var clrInstance = Tools.FindObjectsOfTypeAll<MonoBehaviourAdapter.Adaptor>()
                    .Find(adaptor =>
                        adaptor.ILInstance.Equals(instance[i.Value]));
                if (clrInstance != null)
                {
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField(objName, clrInstance, typeof(MonoBehaviourAdapter.Adaptor),
                        true);
                    GUI.enabled = true;
                }
                else
                {
                    EditorGUILayout.LabelField(objName, obj.ToString());
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        [SerializeTypeMethod(1)]
        public static bool SerializeCrossBindingAdaptorType(AnimBool[] fadeGroup, Type cType, IType type,
            ILTypeInstance instance, KeyValuePair<string, int> i,
            string name)
        {
            var objName = i.Key;
            //可绑定值，可以尝试更改
            if (cType.GetInterfaces().Contains(typeof(CrossBindingAdaptorType))) //需要跨域继承的普遍都有适配器
            {
                var clrInstance = (MonoBehaviour) Tools.FindObjectsOfTypeAll<CrossBindingAdaptorType>()
                    .Find(adaptor =>
                        adaptor.ILInstance.Equals(instance[i.Value]));
                if (clrInstance != null)
                {
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField(objName, clrInstance.gameObject, typeof(GameObject), true);
                    GUI.enabled = true;
                }

                return true;
            }

            return false;
        }

        [SerializeTypeMethod(2)]
        public static bool SerializeBindablePropertyType(AnimBool[] fadeGroup, Type cType, IType type,
            ILTypeInstance instance, KeyValuePair<string, int> i,
            string name)
        {
            var objValue = i.Value;
            var objName = i.Key;
            var obj = instance[objValue];
            //可绑定值，可以尝试更改
            if (type.ReflectionType.ToString().Contains("BindableProperty") && obj != null)
            {
                PropertyInfo fi = type.ReflectionType.GetProperty("Value");
                object val = fi?.GetValue(obj);

                string genericTypeStr = type.ReflectionType.ToString().Split('`')[1].Replace("1<", "")
                    .Replace(">", "");
                Type genericType = Type.GetType(genericTypeStr);
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

        [SerializeTypeMethod(3)]
        public static bool SerializeUnityObjectType(AnimBool[] fadeGroup, Type cType, IType type,
            ILTypeInstance instance, KeyValuePair<string, int> i,
            string name)
        {
            var objValue = i.Value;
            var obj = instance[objValue];
            if (typeof(Object).IsAssignableFrom(cType))
            {
                if (instance[objValue] == null && cType.GetInterfaces().Contains(typeof(CrossBindingAdaptorType)))
                {
                    EditorGUILayout.LabelField(name, "未赋值的热更类");
                    return true;
                }

                if (cType.GetInterfaces().Contains(typeof(CrossBindingAdaptorType)))
                {
                    try
                    {
                        var clrInstance = (MonoBehaviour) Tools.FindObjectsOfTypeAll<CrossBindingAdaptorType>()
                            .Find(adaptor =>
                                Equals(adaptor.ILInstance, instance[objValue]));
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField(name, clrInstance, cType, true);
                        GUI.enabled = true;
                    }
                    catch
                    {
                        EditorGUILayout.LabelField(name, "未赋值的热更类");
                    }
                }
                else
                {
                    //处理Unity类型
                    var res = EditorGUILayout.ObjectField(name, obj as Object, cType, true);
                    instance[i.Value] = res;
                }

                return true;
            }

            return false;
        }

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

        [SerializeTypeMethod(5)]
        public static bool SerializeStringType(AnimBool[] fadeGroup, Type cType, IType type, ILTypeInstance instance,
            KeyValuePair<string, int> i,
            string name)
        {
            if (cType == typeof(string))
            {
                var objValue = i.Value;
                if (instance[objValue] != null)
                {
                    instance[objValue] = EditorGUILayout.TextField(name, (string) instance[objValue]);
                }
                else
                {
                    instance[objValue] = EditorGUILayout.TextField(name, "");
                }

                return true;
            }

            return false;
        }

        [SerializeTypeMethod(6)]
        public static bool SerializePrimitiveType(AnimBool[] fadeGroup, Type cType, IType type, ILTypeInstance instance,
            KeyValuePair<string, int> i,
            string name)
        {
            if (cType.IsPrimitive) //如果是基础类型
            {
                var objValue = i.Value;

                try
                {
                    if (cType == typeof(float))
                    {
                        instance[objValue] = EditorGUILayout.FloatField(name, (float) instance[objValue]);
                    }
                    else if (cType == typeof(double))
                    {
                        instance[objValue] = EditorGUILayout.DoubleField(name, (double) instance[objValue]);
                    }
                    else if (cType == typeof(int))
                    {
                        instance[objValue] = EditorGUILayout.IntField(name, (int) instance[objValue]);
                    }
                    else if (cType == typeof(long))
                    {
                        instance[objValue] = EditorGUILayout.LongField(name, (long) instance[objValue]);
                    }
                    else if (cType == typeof(bool))
                    {
                        var result = bool.TryParse(instance[objValue].ToString(), out var value);
                        if (!result)
                        {
                            value = instance[objValue].ToString() == "1";
                        }

                        instance[objValue] = EditorGUILayout.Toggle(name, value);
                    }
                    else
                    {
                        EditorGUILayout.LabelField(name, instance[objValue].ToString());
                    }
                }
                catch (Exception e)
                {
                    Log.PrintError($"无法序列化{name}，{e.Message}");
                    EditorGUILayout.LabelField(name, instance[objValue].ToString());
                }

                return true;
            }

            return false;
        }
    }
}
