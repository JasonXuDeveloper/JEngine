using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JEngine.Core;
using JEngine.Editor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine.SceneManagement;
using ReorderableList = Malee.List.ReorderableList;

namespace JEngine.Editor
{
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

            GUILayout.Space(5);

            Setting.MakeHorizontal(50, () =>
            {
                if (GUILayout.Button("自动获取fields", GUILayout.Height(30)))
                {
                    DoConvert(target as ClassBind);
                }
            });
            
            GUILayout.Space(5);

            Setting.MakeHorizontal(50, () =>
            {
                if (GUILayout.Button("自动更新fieldType", GUILayout.Height(30)))
                {
                    DoFieldType(target as ClassBind);
                }
            });

            
            GUILayout.Space(15);
        }

        public static async void DoFieldType(ClassBind instance)
        {
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

                foreach (var field in _cb.Fields)
                {
                    var fieldType = t.GetField(field.fieldName,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                        BindingFlags.Static)?.FieldType;
                    if (fieldType == null)
                    {
                        fieldType = t.GetProperty(field.fieldName,
                            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                            BindingFlags.Static)?.PropertyType;
                    }

                    if (fieldType == null)
                    {
                        Log.PrintError($"{className}不存在{field.fieldName}，已跳过");
                    }

                    SetType(field, fieldType, hotCode);
                    affectCounts++;

                    EditorUtility.DisplayProgressBar("ClassBind Progress",
                        $"Getting Field for {field.fieldName} {_cb.Fields.IndexOf(field)}/{_cb.Fields.Length}",
                        (float) _cb.Fields.IndexOf(field) / (float) _cb.Fields.Length);

                    await Task.Delay(50); //延迟一下，动画更丝滑
                }
                
            } 
            
            //转换后保存场景
            bool saveResult = false;
            AssetDatabase.SaveAssets();
            bool isPreviewSceneObject = EditorSceneManager.IsPreviewSceneObject(Selection.activeGameObject);
            if (isPreviewSceneObject || PrefabUtility.IsPartOfAnyPrefab(instance.gameObject) || PrefabUtility.IsPartOfPrefabAsset(instance.gameObject))
            {
                PrefabUtility.SavePrefabAsset(instance.gameObject,out saveResult);
                EditorSceneManager.SaveOpenScenes();
            }
            else
            {
                var scene = EditorSceneManager.GetActiveScene();
                saveResult = EditorSceneManager.SaveScene(scene, scene.path);
            }
            
            string result = saveResult ? "succeeded" : "failed";
            string resultZh = saveResult ? "成功" : "失败";

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("ClassBind Result",
                $"Set {affectCounts} fieldTypes into ClassBind: {instance.name} and saved the scene {result}\n" +
                $"ClassBind: {instance.name}中{affectCounts}个fields已自动设置FieldType，且保存{resultZh}",
                "Done");
        }

        public static async void DoConvert(ClassBind instance)
        {
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

                        SetType(cf, field.FieldType, hotCode);
                        SetVal(ref cf, field, hotCode, hotInstance);

                        _cb.Fields.Add(cf);
                        affectCounts++;
                    }

                    await Task.Delay(10); //延迟一下，动画更丝滑
                }

                EditorUtility.DisplayProgressBar("ClassBind Progress",
                    $"Converting FieldInfos {fs.Length}/{fs.Length}", 1);

                await Task.Delay(50); //延迟一下，动画更丝滑

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

                        SetType(cf, property.PropertyType, hotCode);
                        SetVal(ref cf, property, hotCode, hotInstance);

                        _cb.Fields.Add(cf);
                        affectCounts++;
                    }

                    await Task.Delay(10); //延迟一下，动画更丝滑
                }

                EditorUtility.DisplayProgressBar("ClassBind Progress",
                    $"Converting PropertyInfos {ps.Length}/{ps.Length}", 1);

                await Task.Delay(50); //延迟一下，动画更丝滑

                EditorUtility.DisplayProgressBar("ClassBind Progress",
                    $"Processing next class", 1);

                await Task.Delay(150); //延迟一下，动画更丝滑
            }

            await Task.Delay(50); //延迟一下，动画更丝滑

            //转换后保存场景
            bool saveResult = false;
            AssetDatabase.SaveAssets();
            bool isPreviewSceneObject = EditorSceneManager.IsPreviewSceneObject(Selection.activeGameObject);
            if (PrefabUtility.IsPartOfAnyPrefab(instance.gameObject) || PrefabUtility.IsPartOfPrefabAsset(instance.gameObject))
            {
                PrefabUtility.SavePrefabAsset(instance.gameObject,out saveResult);
            }
            else if (isPreviewSceneObject)
            {
                EditorSceneManager.SaveOpenScenes();
            }
            else
            {
                var scene = EditorSceneManager.GetActiveScene();
                saveResult = EditorSceneManager.SaveScene(scene, scene.path);
            }
            
            string result = saveResult ? "succeeded" : "failed";
            string resultZh = saveResult ? "成功" : "失败";

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("ClassBind Result",
                $"Added {affectCounts} fields into ClassBind: {instance.name} and saved the scene {result}\n" +
                $"ClassBind: {instance.name}的fields添加了{affectCounts}个，且保存{resultZh}",
                "Done");

        }


        private static void SetType(_ClassField cf, Type type, Assembly hotCode)
        {
            if (type == typeof(GameObject))
            {
                cf.fieldType = _ClassField.FieldType.GameObject;
            }
            else if (type == typeof(UnityEngine.Component) || type.IsSubclassOf(typeof(MonoBehaviour)) || type.IsSubclassOf(hotCode.GetType("JEngine.Core.JBehaviour")))
            {
                cf.fieldType = _ClassField.FieldType.UnityComponent;
            }
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                cf.fieldType = _ClassField.FieldType.HotUpdateResource;
            }
            else
            {
                if (type == typeof(short))
                {
                    cf.fieldType = _ClassField.FieldType.Number;
                }
                else if (type == typeof(ushort))
                {
                    cf.fieldType = _ClassField.FieldType.Number;
                }
                else if (type == typeof(int))
                {
                    cf.fieldType = _ClassField.FieldType.Number;
                }
                else if (type == typeof(uint))
                {
                    cf.fieldType = _ClassField.FieldType.Number;
                }
                else if (type == typeof(long))
                {
                    cf.fieldType = _ClassField.FieldType.Number;
                }
                else if (type == typeof(ulong))
                {
                    cf.fieldType = _ClassField.FieldType.Number;
                }
                else if (type == typeof(float))
                {
                    cf.fieldType = _ClassField.FieldType.Number;
                }
                else if (type == typeof(decimal))
                {
                    cf.fieldType = _ClassField.FieldType.Number;
                }
                else if (type == typeof(double))
                {
                    cf.fieldType = _ClassField.FieldType.Number;
                }
                else if (type == typeof(string))
                {
                    cf.fieldType = _ClassField.FieldType.String;
                }
                else if (type == typeof(bool))
                {
                    cf.fieldType = _ClassField.FieldType.Bool;
                }
            }
        }

        private static void SetVal(ref _ClassField cf, FieldInfo field, Assembly hotCode, object hotInstance)
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

        private static void SetVal(ref _ClassField cf, PropertyInfo field, Assembly hotCode, object hotInstance)
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