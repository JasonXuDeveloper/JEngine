using UnityEngine;
using UnityEditor;
using System.Collections;
using Malee.List;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JEngine.Core;
using JEngine.Editor;
using UnityEditor.SceneManagement;

namespace JEngine.Editor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ClassBind))]
    internal class ClassBindEditor : UnityEditor.Editor
    {

        private ReorderableList list1;

        void OnEnable()
        {

            list1 = new ReorderableList(serializedObject.FindProperty("ScriptsToBind"));
            list1.elementNameProperty = "Class";
        }

        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            //draw the list using GUILayout, you can of course specify your own position and label
            list1.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(15);

            JEngineSetting.MakeHorizontal(50, () =>
            {
                if (GUILayout.Button("自动获取fields", GUILayout.Height(30)))
                {
                    DoConvert();
                }
            });

            GUILayout.Space(15);
        }

        private async void DoConvert()
        {
            ClassBind instance = serializedObject.targetObject as ClassBind; //获取对象
            int affectCounts = 0;
            foreach (var _cb in instance.ScriptsToBind) //遍历
            {
                string className = $"{_cb.Namespace}.{_cb.Class}";
                Assembly hotCode = Assembly
                    .LoadFile("Assets/HotUpdateResources/Dll/Hidden~/HotUpdateScripts.dll");
                Type t = hotCode.GetType(className); //加载热更类

                if (t == null)
                {
                    EditorUtility.DisplayDialog("ClassBind Error", $"Class {className} does not exist " +
                                                                   $"in hot update scripts solution!\n" +
                                                                   $"'{className}'类在热更工程中不存在", "OK");
                    return;
                }

                //热更实例
                object hotInstance = null;
                if (!t.IsSubclassOf(hotCode.GetType("JEngine.Core.JBehaviour"))) //JBehaviour派生类不构造对象，不进行赋值
                {
                    hotInstance = Activator.CreateInstance(t);
                }

                var fieldsInCb = _cb.Fields.Select(f => f.fieldName).ToList(); //全部已经设置的字段
                var fs = t.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance |
                                     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var ps = t.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance |
                                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                foreach (var field in fs)
                {
                    //遍历字段

                    EditorUtility.DisplayProgressBar("ClassBind Progress",
                        "Converting FieldInfos " + fs.ToList().IndexOf(field) + "/" + fs.Length,
                        (float) fs.ToList().IndexOf(field) / (float) fs.Length);

                    if (!fieldsInCb.Contains(field.Name))
                    {
                        _ClassField cf = new _ClassField();
                        string fieldName = field.Name;
                        cf.fieldName = fieldName;

                        SetType(ref cf, field, hotCode);
                        SetVal(ref cf, field, hotCode, hotInstance);

                        _cb.Fields.Add(cf);
                        affectCounts++;
                    }

                    await Task.Delay(10); //延迟一下，动画更丝滑
                }

                EditorUtility.DisplayProgressBar("ClassBind Progress",
                    $"Converting FieldInfos {fs.Length}/{fs.Length}", 1);

                await Task.Delay(150); //延迟一下，动画更丝滑

                foreach (var property in ps)
                {
                    //遍历属性
                    EditorUtility.DisplayProgressBar("ClassBind Progress",
                        "Converting PropertyInfos " + ps.ToList().IndexOf(property) + "/" + ps.Length,
                        (float) ps.ToList().IndexOf(property) / (float) ps.Length);
                    if (!fieldsInCb.Contains(property.Name))
                    {
                        _ClassField cf = new _ClassField();
                        string fieldName = property.Name;
                        cf.fieldName = fieldName;

                        SetType(ref cf, property, hotCode);
                        SetVal(ref cf, property, hotCode, hotInstance);

                        _cb.Fields.Add(cf);
                        affectCounts++;
                    }

                    await Task.Delay(10); //延迟一下，动画更丝滑
                }

                EditorUtility.DisplayProgressBar("ClassBind Progress",
                    $"Converting PropertyInfos {ps.Length}/{ps.Length}", 1);

                await Task.Delay(150); //延迟一下，动画更丝滑

                EditorUtility.DisplayProgressBar("ClassBind Progress",
                    $"Processing next class", 1);

                await Task.Delay(250); //延迟一下，动画更丝滑
            }

            await Task.Delay(150); //延迟一下，动画更丝滑

            //转换后保存场景
            var scene = EditorSceneManager.GetActiveScene();
            bool saveResult = EditorSceneManager.SaveScene(scene, scene.path);
            string result = saveResult ? "succeeded" : "failed";
            string resultZh = saveResult ? "成功" : "失败";

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("ClassBind Result",
                $"Added {affectCounts} fields into ClassBind and saved the scene {result}\n" +
                $"ClassBind的fields添加了{affectCounts}个，且场景保存{resultZh}",
                "Done");
        }


        private void SetType(ref _ClassField cf, FieldInfo field, Assembly hotCode)
        {
            if (field.FieldType == typeof(GameObject))
            {
                cf.fieldType = _ClassField.FieldType.GameObject;
            }
            else if (field.FieldType == typeof(UnityEngine.Component))
            {
                cf.fieldType = _ClassField.FieldType.UnityComponent;
            }
            else if (field.FieldType.IsSubclassOf(hotCode.GetType("JEngine.Core.JBehaviour")))
            {
                cf.fieldType = _ClassField.FieldType.UnityComponent;
            }
            else if (field.FieldType == typeof(UnityEngine.Object))
            {
                cf.fieldType = _ClassField.FieldType.HotUpdateResource;
            }
            else
            {
                if (field.FieldType == typeof(short))
                {
                    cf.fieldType = _ClassField.FieldType.Short;
                }
                else if (field.FieldType == typeof(ushort))
                {
                    cf.fieldType = _ClassField.FieldType.UShort;
                }
                else if (field.FieldType == typeof(int))
                {
                    cf.fieldType = _ClassField.FieldType.Int;
                }
                else if (field.FieldType == typeof(uint))
                {
                    cf.fieldType = _ClassField.FieldType.UInt;
                }
                else if (field.FieldType == typeof(long))
                {
                    cf.fieldType = _ClassField.FieldType.Long;
                }
                else if (field.FieldType == typeof(ulong))
                {
                    cf.fieldType = _ClassField.FieldType.ULong;
                }
                else if (field.FieldType == typeof(float))
                {
                    cf.fieldType = _ClassField.FieldType.Float;
                }
                else if (field.FieldType == typeof(decimal))
                {
                    cf.fieldType = _ClassField.FieldType.Decimal;
                }
                else if (field.FieldType == typeof(double))
                {
                    cf.fieldType = _ClassField.FieldType.Double;
                }
                else if (field.FieldType == typeof(string))
                {
                    cf.fieldType = _ClassField.FieldType.String;
                }
                else if (field.FieldType == typeof(bool))
                {
                    cf.fieldType = _ClassField.FieldType.Bool;
                }
            }
        }

        private void SetType(ref _ClassField cf, PropertyInfo field, Assembly hotCode)
        {
            if (field.PropertyType == typeof(GameObject))
            {
                cf.fieldType = _ClassField.FieldType.GameObject;
            }
            else if (field.PropertyType == typeof(UnityEngine.Component))
            {
                cf.fieldType = _ClassField.FieldType.UnityComponent;
            }
            else if (field.PropertyType.IsSubclassOf(hotCode.GetType("JEngine.Core.JBehaviour")))
            {
                cf.fieldType = _ClassField.FieldType.UnityComponent;
            }
            else if (field.PropertyType == typeof(UnityEngine.Object))
            {
                cf.fieldType = _ClassField.FieldType.HotUpdateResource;
            }
            else
            {
                if (field.PropertyType == typeof(short))
                {
                    cf.fieldType = _ClassField.FieldType.Short;
                }
                else if (field.PropertyType == typeof(ushort))
                {
                    cf.fieldType = _ClassField.FieldType.UShort;
                }
                else if (field.PropertyType == typeof(int))
                {
                    cf.fieldType = _ClassField.FieldType.Int;
                }
                else if (field.PropertyType == typeof(uint))
                {
                    cf.fieldType = _ClassField.FieldType.UInt;
                }
                else if (field.PropertyType == typeof(long))
                {
                    cf.fieldType = _ClassField.FieldType.Long;
                }
                else if (field.PropertyType == typeof(ulong))
                {
                    cf.fieldType = _ClassField.FieldType.ULong;
                }
                else if (field.PropertyType == typeof(float))
                {
                    cf.fieldType = _ClassField.FieldType.Float;
                }
                else if (field.PropertyType == typeof(decimal))
                {
                    cf.fieldType = _ClassField.FieldType.Decimal;
                }
                else if (field.PropertyType == typeof(double))
                {
                    cf.fieldType = _ClassField.FieldType.Double;
                }
                else if (field.PropertyType == typeof(string))
                {
                    cf.fieldType = _ClassField.FieldType.String;
                }
                else if (field.PropertyType == typeof(bool))
                {
                    cf.fieldType = _ClassField.FieldType.Bool;
                }
            }
        }

        private void SetVal(ref _ClassField cf, FieldInfo field, Assembly hotCode, object hotInstance)
        {
            if (field.FieldType != typeof(UnityEngine.Object) ||
                !field.FieldType.IsSubclassOf(hotCode.GetType("JEngine.Core.JBehaviour")))
            {
                try
                {
                    cf.value = field.GetValue(hotInstance).ToString();
                }
                catch
                {
                    Log.PrintWarning($"无法对JBehaviour派生类的字段{cf.fieldName}自动赋值构造值（如果没有可忽略）");
                }
            }
        }

        private void SetVal(ref _ClassField cf, PropertyInfo field, Assembly hotCode, object hotInstance)
        {
            if (field.PropertyType != typeof(UnityEngine.Object) ||
                !field.PropertyType.IsSubclassOf(hotCode.GetType("JEngine.Core.JBehaviour")))
            {
                try
                {
                    cf.value = field.GetValue(hotInstance).ToString();
                }
                catch
                {
                    Log.PrintWarning($"无法对JBehaviour派生类的属性{cf.fieldName}自动赋值构造值（如果没有可忽略）");
                }
            }
        }
    }
}