using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JEngine.Core;
using Malee.List;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace JEngine.Editor
{
    [CustomEditor(typeof(ClassBind))]
    internal class ClassBindEditor : UnityEditor.Editor
    {
        private static string _dllPath = "Assets/HotUpdateResources/Dll/Hidden~/HotUpdateScripts.dll";
        private ReorderableList _classBinds;

        void OnEnable()
        {
            _classBinds = new ReorderableList(serializedObject.FindProperty("scriptsToBind"))
            {
                elementNameProperty = "Class",
                sortable = true,
                multipleSelection = true,
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _classBinds.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(5);

            Setting.MakeHorizontal(50, () =>
            {
                if (GUILayout.Button(Setting.GetString(SettingString.ClassBindGetAllField), GUILayout.Height(30)))
                {
                    DoConvert(target as ClassBind);
                }
            });

            GUILayout.Space(5);

            Setting.MakeHorizontal(50, () =>
            {
                if (GUILayout.Button(Setting.GetString(SettingString.ClassBindGetAllType), GUILayout.Height(30)))
                {
                    DoFieldType(target as ClassBind);
                }
            });

            GUILayout.Space(5);

            Setting.MakeHorizontal(50, () =>
            {
                if (GUILayout.Button(Setting.GetString(SettingString.ClassBindRearrangeTitle), GUILayout.Height(30)))
                {
                    CleanFields(target as ClassBind);
                }
            });


            GUILayout.Space(15);
        }

        public static async void CleanFields(ClassBind instance, bool toast = true)
        {
            int affectCounts = 0;
            foreach (var data in instance.scriptsToBind) //遍历
            {
                string className = $"{data.classNamespace}.{data.className}";
                Assembly hotCode = Assembly
                    .LoadFile(_dllPath);
                Type t = hotCode.GetType(className); //加载热更类

                if (t == null)
                {
                    EditorUtility.DisplayDialog(Setting.GetString(SettingString.ClassBindErrorTitle),
                        String.Format(Setting.GetString(SettingString.ClassBindErrorContent),
                            className), "OK");
                    return;
                }

                for (int i = 0; i < data.fields.Count; i++)
                {
                    var field = data.fields[i];
                    var fieldType = t.GetField(field.fieldName,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                        BindingFlags.Static)?.FieldType ?? t.GetProperty(field.fieldName,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                        BindingFlags.Static)?.PropertyType;

                    if (fieldType == null)
                    {
                        Log.PrintError(String.Format(Setting.GetString(SettingString.ClassBindInvalidFieldDeleted),
                            className, field.fieldName));
                        data.fields.RemoveAt(i);
                        continue;
                    }

                    affectCounts++;

                    EditorUtility.DisplayProgressBar(Setting.GetString(SettingString.ClassBindProgress),
                        String.Format(Setting.GetString(SettingString.ClassBindRearrange),
                            field.fieldName, data.fields.IndexOf(field), data.fields.Length),
                        data.fields.IndexOf(field) / (float) data.fields.Length);

                    await Task.Delay(50); //延迟一下，动画更丝滑
                }

                var f = data.fields.OrderBy(s => s.fieldName);
                FieldList newF = new FieldList();
                foreach (var cf in f)
                {
                    newF.Add(cf);
                }

                data.fields = newF;


            }

            EditorUtility.ClearProgressBar();

            TrySave(instance, toast, String.Format(Setting.GetString(SettingString.ClassBindRearrangeResult),
                    affectCounts, instance.name), Setting.GetString(SettingString.ClassBindResultTitle),
                Setting.GetString(SettingString.Done));
        }

        public static async void DoFieldType(ClassBind instance, bool toast = true)
        {
            int affectCounts = 0;
            foreach (var data in instance.scriptsToBind) //遍历
            {
                string className = $"{data.classNamespace}.{data.className}";
                Assembly hotCode = Assembly
                    .LoadFile(_dllPath);
                Type t = hotCode.GetType(className); //加载热更类

                if (t == null)
                {
                    EditorUtility.DisplayDialog(Setting.GetString(SettingString.ClassBindErrorTitle),
                        String.Format(Setting.GetString(SettingString.ClassBindErrorContent),
                            className), "OK");
                    return;
                }

                foreach (var field in data.fields)
                {
                    var fieldType = t.GetField(field.fieldName,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                        BindingFlags.Static)?.FieldType ?? t.GetProperty(field.fieldName,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                        BindingFlags.Static)?.PropertyType;

                    if (fieldType == null)
                    {
                        Log.PrintError(String.Format(Setting.GetString(SettingString.ClassBindInvalidField),
                            className, field.fieldName));
                    }

                    SetType(field, fieldType, hotCode);
                    affectCounts++;

                    EditorUtility.DisplayProgressBar(Setting.GetString(SettingString.ClassBindProgress),
                        String.Format(Setting.GetString(SettingString.ClassBindProgressContentForGetField),
                            field.fieldName, data.fields.IndexOf(field), data.fields.Length),
                        data.fields.IndexOf(field) / (float) data.fields.Length);

                    await Task.Delay(50); //延迟一下，动画更丝滑
                }

            }

            EditorUtility.ClearProgressBar();

            TrySave(instance, toast, String.Format(Setting.GetString(SettingString.ClassBindResultContentForGetType),
                    affectCounts, instance.name), Setting.GetString(SettingString.ClassBindResultTitle),
                Setting.GetString(SettingString.Done));
        }

        public static async void DoConvert(ClassBind instance, bool toast = true)
        {
            int affectCounts = 0;
            foreach (var data in instance.scriptsToBind) //遍历
            {
                string className = $"{data.classNamespace}.{data.className}";
                Assembly hotCode = Assembly
                    .LoadFile(_dllPath);
                Type t = hotCode.GetType(className); //加载热更类

                if (t == null)
                {
                    EditorUtility.DisplayDialog(Setting.GetString(SettingString.ClassBindErrorTitle),
                        String.Format(Setting.GetString(SettingString.ClassBindErrorContent),
                            className), "OK");
                    return;
                }

                //热更实例
                object hotInstance = null;
                if (!t.IsSubclassOf(typeof(MonoBehaviour))) //JBehaviour/MonoBehaviour派生类不构造对象，不进行赋值
                {
                    if (hotCode.GetType("JEngine.Core.JBehaviour") != null &&
                        !t.IsSubclassOf(hotCode.GetType("JEngine.Core.JBehaviour")))
                    {
                        break;
                    }

                    hotInstance = Activator.CreateInstance(t);
                }


                var fieldsInCb = data.fields.Select(f => f.fieldName).ToList(); //全部已经设置的字段
                var members = new List<MemberInfo>(0);
                if (Setting.ClassBindIgnorePrivate)
                {
                    members.AddRange(t.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance |
                                                 BindingFlags.Public));
                    members.AddRange(t.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance |
                                                     BindingFlags.Public));
                }
                else
                {
                    members.AddRange(t.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance |
                                                 BindingFlags.Public | BindingFlags.NonPublic));
                    members.AddRange(t.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance |
                                                     BindingFlags.Public | BindingFlags.NonPublic));
                }

                foreach (var field in members)
                {
                    //跳过标签的
                    if (Setting.ClassBindIgnoreHideInInspector)
                    {
                        var attr = field.GetCustomAttributes(typeof(HideInInspector), false);
                        if (attr.Length > 0)
                        {
                            continue;
                        }
                    }

                    //遍历字段
                    EditorUtility.DisplayProgressBar(Setting.GetString(SettingString.ClassBindProgress),
                        String.Format(Setting.GetString(SettingString.ClassBindProgressContentForGetType),
                            $"{t.Name}:{field.Name}",
                            members.ToList().IndexOf(field), members.Count),
                        members.ToList().IndexOf(field) / (float) members.Count);

                    if (!fieldsInCb.Contains(field.Name))
                    {
                        ClassField cf = new ClassField();
                        string fieldName = field.Name;
                        cf.fieldName = fieldName;

                        Type fieldType = (field is PropertyInfo)
                            ? ((PropertyInfo) field).PropertyType
                            : ((FieldInfo) field).FieldType;

                        SetType(cf, fieldType, hotCode);
                        SetVal(ref cf, field, hotCode, hotInstance, instance.gameObject);

                        data.fields.Add(cf);
                        affectCounts++;
                    }

                    await Task.Delay(10); //延迟一下，动画更丝滑
                }
            }

            await Task.Delay(50); //延迟一下，动画更丝滑

            EditorUtility.ClearProgressBar();

            TrySave(instance, toast, String.Format(Setting.GetString(SettingString.ClassBindResultContentForSetField),
                    affectCounts, instance.name), Setting.GetString(SettingString.ClassBindResultTitle),
                Setting.GetString(SettingString.Done));
        }

        private static void TrySave(ClassBind instance, bool toast, string text, string title = "", string ok = "")
        {
            //转换后保存场景
            try
            {
                PrefabUtility.SavePrefabAsset(instance.gameObject, out _);
            }
            catch
            {
                try
                {
                    EditorSceneManager.SaveOpenScenes();
                }
                catch
                {
                    try
                    {
                        var scene = SceneManager.GetActiveScene();
                        EditorSceneManager.SaveScene(scene, scene.path);
                    }
                    catch
                    {
                        //ignored
                    }
                }
            }

            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();

            if (toast)
            {
                EditorUtility.DisplayDialog(title, text, ok);
            }
            else
            {
                Log.Print(text);
            }
        }

        private static void SetType(ClassField cf, Type type, Assembly hotCode)
        {
            if (type == typeof(GameObject))
            {
                cf.fieldType = ClassField.FieldType.GameObject;
            }
            else if (type == typeof(Component) || type.IsSubclassOf(typeof(MonoBehaviour)) ||
                     type.IsSubclassOf(hotCode.GetType("JEngine.Core.JBehaviour")))
            {
                cf.fieldType = ClassField.FieldType.UnityComponent;
            }
            else if (type.IsSubclassOf(typeof(Object)))
            {
                if (type == typeof(AudioClip) || type == typeof(Sprite) || type == typeof(TextAsset) ||
                    type == typeof(Material))
                {
                    cf.fieldType = ClassField.FieldType.HotUpdateResource;
                }
                else
                {
                    cf.fieldType = ClassField.FieldType.UnityComponent;
                }
            }
            else
            {
                var numType = new[]
                {
                    typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
                    typeof(ulong),
                    typeof(float), typeof(decimal), typeof(double)
                };
                if (numType.Contains(type))
                {
                    cf.fieldType = ClassField.FieldType.Number;
                }
                else if (type == typeof(string))
                {
                    cf.fieldType = ClassField.FieldType.String;
                }
                else if (type == typeof(bool))
                {
                    cf.fieldType = ClassField.FieldType.Bool;
                }
                else if (hotCode.GetTypes().Contains(type))
                {
                    cf.fieldType = ClassField.FieldType.UnityComponent;
                }
                else
                {
                    cf.fieldType = ClassField.FieldType.NotSupported;
                }
            }
        }

        private static void SetVal(ref ClassField cf, MemberInfo field, Assembly hotCode, object hotInstance,
            GameObject instance)
        {
            object value;
            var type = (field is PropertyInfo)
                ? ((PropertyInfo) field).PropertyType
                : ((FieldInfo) field).FieldType;
            if (hotInstance == null)
            {
                value = type.IsValueType ? Activator.CreateInstance(type) : null;
            }
            else
            {
                if (field is PropertyInfo)
                {
                    value = ((PropertyInfo) field).GetValue(hotInstance);
                }
                else
                {
                    value = ((FieldInfo) field).GetValue(hotInstance);
                }
            }

            SetVal(ref cf, type, hotCode, value, instance);
        }

        private static void SetVal(ref ClassField cf, Type type, Assembly hotCode, object value, GameObject instance)
        {
            if (type != typeof(Object) ||
                !type.IsSubclassOf(hotCode.GetType("JEngine.Core.JBehaviour")))
            {
                try
                {
                    if (type == typeof(String))
                    {
                        value = "";
                    }

                    cf.value = value.ToString();
                }
                catch
                {
                    Log.PrintWarning(String.Format(Setting.GetString(SettingString.ClassBindUnableSetFieldValue),
                        instance.name, type.Name, cf.fieldName));
                }
            }
        }
    }
}