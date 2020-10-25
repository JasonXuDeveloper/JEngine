#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Threading.Tasks;
using ILRuntime.Runtime.Intepreter;
using JEngine.Core;
using LitJson;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

// using LitJson;

[CustomEditor(typeof(MonoBehaviourAdapter.Adaptor), true)]
public class MonoBehaviourAdapterEditor : Editor
{
    private bool displaying;   
    
    // 每个动画都需要一个AnimBool
    private AnimBool fadeGroup;
    private async void OnEnable()
    {
        this.fadeGroup = new AnimBool(true);
        // 注册动画监听
        this.fadeGroup.valueChanged.AddListener(this.Repaint);

        displaying = true;
        
        while (true && Application.isEditor && Application.isPlaying && displaying)
        {
            try
            {
                this.Repaint();
            }
            catch{}
            await Task.Delay(500);
        }
    }

    private void OnDestroy()
    {
        displaying = false;
    }

    private void OnDisable()
    {
        // 移除动画监听
        this.fadeGroup.valueChanged.RemoveListener(this.Repaint);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        MonoBehaviourAdapter.Adaptor clr = target as MonoBehaviourAdapter.Adaptor;
        var instance = clr.ILInstance;
        if (instance != null)
        {
            EditorGUILayout.LabelField("Script", clr.ILInstance.Type.Name);
            
            //如果JBehaviour
            var JBehaviourType = Init.appdomain.LoadedTypes["JEngine.Core.JBehaviour"];
            var t = instance.Type.ReflectionType;
            if (t.IsSubclassOf(JBehaviourType.ReflectionType))
            {
                var f = t.GetField("_instanceID", BindingFlags.NonPublic);
                var id =  f.GetValue(instance).ToString();
                EditorGUILayout.LabelField("InstanceID", id);

                GUI.enabled = false;
                var paused = t.GetField("Paused", BindingFlags.NonPublic);
                EditorGUILayout.Toggle("Paused", (bool)paused.GetValue(instance));
                GUI.enabled = true;
            }
            
            int index = 0;
            foreach (var i in instance.Type.FieldMapping)
            {
                //这里是取的所有字段，没有处理不是public的
                var name = i.Key;
                var type = instance.Type.FieldTypes[index];
                index++;

                var cType = type.TypeForCLR;
                object obj = instance[i.Value];
                if (cType.IsPrimitive) //如果是基础类型
                {
                    try
                    {
                        if (cType == typeof(float))
                        {
                            instance[i.Value] = EditorGUILayout.FloatField(name, (float) instance[i.Value]);
                        }
                        else if (cType == typeof(double))
                        {
                            instance[i.Value] = EditorGUILayout.DoubleField(name, (float) instance[i.Value]);
                        }
                        else if (cType == typeof(int))
                        {
                            instance[i.Value] = EditorGUILayout.IntField(name, (int) instance[i.Value]);
                        }
                        else if (cType == typeof(long))
                        {
                            instance[i.Value] = EditorGUILayout.LongField(name, (long) instance[i.Value]);
                        }
                        else if (cType == typeof(bool))
                        {
                            var result = bool.TryParse(instance[i.Value].ToString(), out var value);
                            if (!result)
                            {
                                value = instance[i.Value].ToString() == "1";
                            }
                            instance[i.Value] = EditorGUILayout.Toggle(name, value );
                        }
                        else
                        {
                            EditorGUILayout.LabelField(name, instance[i.Value].ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        Log.PrintError($"无法序列化{name}，{e.Message}");
                        EditorGUILayout.LabelField(name, instance[i.Value].ToString());
                    }
                }
                else
                {
                    if (cType == typeof(string))
                    {
                        if (obj != null)
                        {
                            instance[i.Value] = EditorGUILayout.TextField(name, (string) instance[i.Value]);
                        }
                        else
                        {
                            instance[i.Value] = EditorGUILayout.TextField(name, "");
                        }
                    }
                    else if (cType == typeof(JsonData))
                    {
                        if (instance[i.Value] != null)
                        {
                            this.fadeGroup.target = EditorGUILayout.Foldout(this.fadeGroup.target, name, true);
                            if (EditorGUILayout.BeginFadeGroup(this.fadeGroup.faded))
                            {
                                EditorGUILayout.TextArea(
                                    ((JsonData) instance[i.Value]).ToString()
                                );
                            }
                            EditorGUILayout.EndFadeGroup();
                            EditorGUILayout.Space();
                        }
                        else
                        {
                            EditorGUILayout.LabelField(name, "暂无值的JsonData");
                        }
                    }
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(cType))
                    {
                        if (obj == null && cType == typeof(MonoBehaviourAdapter.Adaptor))
                        {
                            EditorGUILayout.LabelField(name, "未赋值或自动赋值的热更类");
                            break;
                        }
                        
                        //处理Unity类型
                        var res = EditorGUILayout.ObjectField(name, obj as UnityEngine.Object, cType, true);
                        instance[i.Value] = res;
                    }
                    else if (type.ReflectionType.ToString().Contains("BindableProperty") && obj != null)
                    {
                        PropertyInfo fi = type.ReflectionType.GetProperty("Value");
                        object val = fi.GetValue(obj);
                        EditorGUILayout.LabelField(name, val.ToString());
                    }
                    else
                    {
                        //其他类型现在没法处理
                        if (obj != null)
                            EditorGUILayout.LabelField(name, obj.ToString());
                        else
                            EditorGUILayout.LabelField(name, "(null)");
                    }
                }
            }
        }

        // 应用属性修改
        this.serializedObject.ApplyModifiedProperties();
    }
}
#endif