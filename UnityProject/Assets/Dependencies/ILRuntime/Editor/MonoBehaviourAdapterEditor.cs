#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
// using LitJson;

[CustomEditor(typeof(MonoBehaviourAdapter.Adaptor), true)]
public class MonoBehaviourAdapterEditor : UnityEditor.UI.GraphicEditor
{
    // 每个动画都需要一个AnimBool
    private AnimBool fadeGroup;

    private void OnEnable()
    {
        this.fadeGroup = new AnimBool(true);
        // 注册动画监听
        this.fadeGroup.valueChanged.AddListener(this.Repaint);
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
                        }
                    }
                    catch (Exception e)
                    {
                        instance[i.Value] = EditorGUILayout.TextArea(name, $"(null)");
                    }
                }
                else
                {
                    // if (cType == typeof(JsonData))
                    // {
                    //     if (instance[i.Value] != null)
                    //     {
                    //         // target控制动画开始播放
                    //         this.fadeGroup.target = EditorGUILayout.Foldout(this.fadeGroup.target, name, true);
                    //
                    //         // 系统使用tween渐变faded数值
                    //         if (EditorGUILayout.BeginFadeGroup(this.fadeGroup.faded))
                    //         {
                    //             EditorGUILayout.TextArea(
                    //                 ((JsonData) instance[i.Value]).ToString()
                    //             );
                    //         }
                    //         // begin - end 之间元素会进行动画
                    //         EditorGUILayout.EndFadeGroup();
                    //         
                    //         EditorGUILayout.Space();
                    //         
                    //     }
                    //     else
                    //     {
                    //         EditorGUILayout.LabelField(name, "暂无值的JsonData");
                    //     }
                    //
                    //     continue;
                    // }

                    object obj = instance[i.Value];
                    if (typeof(UnityEngine.Object).IsAssignableFrom(cType))
                    {
                        //处理Unity类型
                        var res = EditorGUILayout.ObjectField(name, obj as UnityEngine.Object, cType, true);
                        instance[i.Value] = res;
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