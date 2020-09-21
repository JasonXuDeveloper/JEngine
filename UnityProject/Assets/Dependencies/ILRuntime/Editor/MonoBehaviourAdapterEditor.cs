#if UNITY_EDITOR
using System;
using System.Threading.Tasks;
using ILRuntime.Runtime.Intepreter;
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
            await Task.Delay(1000);
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
            
            int index = 0;
            foreach (var i in instance.Type.FieldMapping)
            {
                //这里是取的所有字段，没有处理不是public的
                var name = i.Key;
                var type = instance.Type.FieldTypes[index];
                index++;

                var cType = type.TypeForCLR;
                if (cType.IsPrimitive) //如果是基础类型
                {
                    try
                    {
                        if (cType == typeof(float))
                        {
                            instance[i.Value] = EditorGUILayout.FloatField(name, (float) instance[i.Value]);
                        }
                        else if (cType == typeof(int))
                        {
                            instance[i.Value] = EditorGUILayout.IntField(name, (int) instance[i.Value]);
                        }
                        else if (cType == typeof(long))
                        {
                            instance[i.Value] = EditorGUILayout.LongField(name, (long) instance[i.Value]);
                        }
                        else if (cType == typeof(string))
                        {
                            instance[i.Value] = EditorGUILayout.TextArea(name, (string) instance[i.Value]);
                        }
                        else
                        {
                            //剩下的大家自己补吧
                            instance[i.Value] = EditorGUILayout.TextArea(name, instance[i.Value].ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        instance[i.Value] = EditorGUILayout.TextArea(name, "(null)");
                    }
                }
                else
                {
                    if (cType == typeof(JsonData))
                    {
                        if (instance[i.Value] != null)
                        {
                            // target控制动画开始播放
                            this.fadeGroup.target = EditorGUILayout.Foldout(this.fadeGroup.target, name, true);
                    
                            // 系统使用tween渐变faded数值
                            if (EditorGUILayout.BeginFadeGroup(this.fadeGroup.faded))
                            {
                                EditorGUILayout.TextArea(
                                    ((JsonData) instance[i.Value]).ToString()
                                );
                            }
                            // begin - end 之间元素会进行动画
                            EditorGUILayout.EndFadeGroup();
                            
                            EditorGUILayout.Space();
                            
                        }
                        else
                        {
                            EditorGUILayout.LabelField(name, "暂无值的JsonData");
                        }
                    
                        continue;
                    }

                    object obj = instance[i.Value];
                    
                    if (typeof(UnityEngine.Object).IsAssignableFrom(cType))
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
                    else
                    {
                        //其他类型现在没法处理
                        if (obj != null)
                            EditorGUILayout.TextField(name, obj.ToString());
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