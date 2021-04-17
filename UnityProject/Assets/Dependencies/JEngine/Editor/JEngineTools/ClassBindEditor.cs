using System;
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
                elementNameProperty = "Class"
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


            GUILayout.Space(15);
        }

        public static async void DoFieldType(ClassBind instance,bool toast = true)
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
                EditorUtility.DisplayDialog(Setting.GetString(SettingString.ClassBindResultTitle),
                    String.Format(Setting.GetString(SettingString.ClassBindResultContentForGetType),
                        affectCounts, instance.name),
                    Setting.GetString(SettingString.Done));
            }
            else
            {
                Log.Print(String.Format(Setting.GetString(SettingString.ClassBindResultContentForGetType),
                        affectCounts, instance.name));
            }
        }

        public static async void DoConvert(ClassBind instance,bool toast = true)
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
                if (!t.IsSubclassOf(hotCode.GetType("JEngine.Core.JBehaviour")) &&
                    !t.IsSubclassOf(typeof(MonoBehaviour))) //JBehaviour/MonoBehaviour派生类不构造对象，不进行赋值
                {
                    hotInstance = Activator.CreateInstance(t);
                }


                var fieldsInCb = data.fields.Select(f => f.fieldName).ToList(); //全部已经设置的字段
                var fs = t.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance |
                                     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var ps = t.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance |
                                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                foreach (var field in fs)
                {
                    //遍历字段
                    EditorUtility.DisplayProgressBar(Setting.GetString(SettingString.ClassBindProgress),
                        String.Format(Setting.GetString(SettingString.ClassBindProgressContentForGetType),
                            $"{t.Name}:{field.Name}",
                            fs.ToList().IndexOf(field), fs.Length),
                        fs.ToList().IndexOf(field) / (float) fs.Length);

                    if (!fieldsInCb.Contains(field.Name))
                    {
                        ClassField cf = new ClassField();
                        string fieldName = field.Name;
                        cf.fieldName = fieldName;

                        SetType(cf, field.FieldType, hotCode);
                        SetVal(ref cf, field, hotCode, hotInstance,instance.gameObject);

                        data.fields.Add(cf);
                        affectCounts++;
                    }

                    await Task.Delay(10); //延迟一下，动画更丝滑
                }

                foreach (var property in ps)
                {
                    //遍历属性

                    EditorUtility.DisplayProgressBar(Setting.GetString(SettingString.ClassBindProgress),
                        String.Format(Setting.GetString(SettingString.ClassBindProgressContentForGetType),
                            $"{t.Name}:{property.Name}",
                            ps.ToList().IndexOf(property), ps.Length),
                        ps.ToList().IndexOf(property) / (float) ps.Length);
                    if (!fieldsInCb.Contains(property.Name))
                    {
                        ClassField cf = new ClassField();
                        string fieldName = property.Name;
                        cf.fieldName = fieldName;

                        SetType(cf, property.PropertyType, hotCode);
                        SetVal(ref cf, property, hotCode, hotInstance,instance.gameObject);

                        data.fields.Add(cf);
                        affectCounts++;
                    }

                    await Task.Delay(10); //延迟一下，动画更丝滑
                }
            }

            await Task.Delay(50); //延迟一下，动画更丝滑

            EditorUtility.ClearProgressBar();

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
                EditorUtility.DisplayDialog(Setting.GetString(SettingString.ClassBindResultTitle),
                    String.Format(Setting.GetString(SettingString.ClassBindResultContentForSetField),
                        affectCounts, instance.name),
                    Setting.GetString(SettingString.Done));
            }
            else
            {
                Log.Print(String.Format(Setting.GetString(SettingString.ClassBindResultContentForSetField),
                    affectCounts, instance.name));
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
                cf.fieldType = ClassField.FieldType.HotUpdateResource;
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
                else
                {
                    cf.fieldType = ClassField.FieldType.NotSupported;
                }
            }
        }

        private static void SetVal(ref ClassField cf, FieldInfo field, Assembly hotCode, object hotInstance,GameObject instance)
        {
            object value;
            if (hotInstance == null)
            {
                var type = field.FieldType;
                value = type.IsValueType ? Activator.CreateInstance(type) : null;
            }
            else
            {
                value = field.GetValue(hotInstance);
            }

            SetVal(ref cf, field.FieldType, hotCode, value,instance);
        }

        private static void SetVal(ref ClassField cf, PropertyInfo field, Assembly hotCode, object hotInstance,GameObject instance)
        {
            object value;
            if (hotInstance == null)
            {
                var type = field.PropertyType;
                value = type.IsValueType ? Activator.CreateInstance(type) : null;
            }
            else
            {
                value = field.GetValue(hotInstance);
            }

            SetVal(ref cf, field.PropertyType, hotCode, value,instance);
        }

        private static void SetVal(ref ClassField cf, Type type, Assembly hotCode, object value,GameObject instance)
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