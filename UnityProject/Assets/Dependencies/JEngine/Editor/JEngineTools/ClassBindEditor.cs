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
                if (GUILayout.Button(Setting.GetString((int) SettingString.ClassBindGetAllField), GUILayout.Height(30)))
                {
                    DoConvert(target as ClassBind);
                }
            });
            
            GUILayout.Space(5);

            Setting.MakeHorizontal(50, () =>
            {
                if (GUILayout.Button(Setting.GetString((int) SettingString.ClassBindGetAllType), GUILayout.Height(30)))
                {
                    DoFieldType(target as ClassBind);
                }
            });

            
            GUILayout.Space(15);
        }

        public static async void DoFieldType(ClassBind instance)
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
                    EditorUtility.DisplayDialog(Setting.GetString((int) SettingString.ClassBindErrorTitle),
                        String.Format(Setting.GetString((int) SettingString.ClassBindErrorContent),
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
                        Log.PrintError(String.Format(Setting.GetString((int) SettingString.ClassBindInvalidField),
                            className, field.fieldName));
                    }

                    SetType(field, fieldType, hotCode);
                    affectCounts++;

                    EditorUtility.DisplayProgressBar(Setting.GetString((int) SettingString.ClassBindProgress),
                        String.Format(Setting.GetString((int) SettingString.ClassBindProgressContentForGetField),
                            field.fieldName, data.fields.IndexOf(field), data.fields.Length),
                        data.fields.IndexOf(field) / (float) data.fields.Length);

                    await Task.Delay(50); //延迟一下，动画更丝滑
                }
                
            } 
            
            EditorUtility.ClearProgressBar();
            
            //转换后保存场景
            bool saveResult;
            AssetDatabase.SaveAssets();
            bool isPreviewSceneObject = EditorSceneManager.IsPreviewSceneObject(Selection.activeGameObject);
            if (isPreviewSceneObject || PrefabUtility.IsPartOfAnyPrefab(instance.gameObject) || PrefabUtility.IsPartOfPrefabAsset(instance.gameObject))
            {
                PrefabUtility.SavePrefabAsset(instance.gameObject,out saveResult);
                EditorSceneManager.SaveOpenScenes();
            }
            else
            {
                var scene = SceneManager.GetActiveScene();
                saveResult = EditorSceneManager.SaveScene(scene, scene.path);
            }

            string result = Setting.GetString(saveResult ? (int) SettingString.Success : (int) SettingString.Fail);

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog(Setting.GetString((int) SettingString.ClassBindResultTitle),
                String.Format(Setting.GetString((int) SettingString.ClassBindResultContent),
                    affectCounts, instance.name, result),
                Setting.GetString((int) SettingString.Done));
        }

        public static async void DoConvert(ClassBind instance)
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
                    EditorUtility.DisplayDialog("ClassBind Error", $"Class {className} does not exist " +
                                                                   "in hot update scripts solution!\n" +
                                                                   $"'{className}'类在热更工程中不存在", "OK");
                    return;
                }

                //热更实例
                object hotInstance = null;
                if (!t.IsSubclassOf(hotCode.GetType("JEngine.Core.JBehaviour"))) //JBehaviour派生类不构造对象，不进行赋值
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

                    EditorUtility.DisplayProgressBar("ClassBind Progress",
                        "Converting FieldInfos " + fs.ToList().IndexOf(field) + "/" + fs.Length,
                        fs.ToList().IndexOf(field) / (float) fs.Length);

                    if (!fieldsInCb.Contains(field.Name))
                    {
                        ClassField cf = new ClassField();
                        string fieldName = field.Name;
                        cf.fieldName = fieldName;

                        SetType(cf, field.FieldType, hotCode);
                        SetVal(ref cf, field, hotCode, hotInstance);

                        data.fields.Add(cf);
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
                        ps.ToList().IndexOf(property) / (float) ps.Length);
                    if (!fieldsInCb.Contains(property.Name))
                    {
                        ClassField cf = new ClassField();
                        string fieldName = property.Name;
                        cf.fieldName = fieldName;

                        SetType(cf, property.PropertyType, hotCode);
                        SetVal(ref cf, property, hotCode, hotInstance);

                        data.fields.Add(cf);
                        affectCounts++;
                    }

                    await Task.Delay(10); //延迟一下，动画更丝滑
                }

                EditorUtility.DisplayProgressBar("ClassBind Progress",
                    $"Converting PropertyInfos {ps.Length}/{ps.Length}", 1);

                await Task.Delay(50); //延迟一下，动画更丝滑

                EditorUtility.DisplayProgressBar("ClassBind Progress",
                    "Processing next class", 1);

                await Task.Delay(150); //延迟一下，动画更丝滑
            }

            await Task.Delay(50); //延迟一下，动画更丝滑

            EditorUtility.ClearProgressBar();
            
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
                var scene = SceneManager.GetActiveScene();
                saveResult = EditorSceneManager.SaveScene(scene, scene.path);
            }
            
            string result = saveResult ? "succeeded" : "failed";
            string resultZh = saveResult ? "成功" : "失败";

            EditorUtility.DisplayDialog("ClassBind Result",
                $"Added {affectCounts} fields into ClassBind: {instance.name} and saved the scene {result}\n" +
                $"ClassBind: {instance.name}的fields添加了{affectCounts}个，且保存{resultZh}",
                "Done");

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

        private static void SetVal(ref ClassField cf, FieldInfo field, Assembly hotCode, object hotInstance)
        {
            SetVal(ref cf, field.FieldType, hotCode, field.GetValue(hotInstance));

        }

        private static void SetVal(ref ClassField cf, PropertyInfo field, Assembly hotCode, object hotInstance)
        {
            SetVal(ref cf, field.PropertyType, hotCode, field.GetValue(hotInstance));
        }

        private static void SetVal(ref ClassField cf, Type type, Assembly hotCode, object value)
        {
            if (type != typeof(Object) ||
                !type.IsSubclassOf(hotCode.GetType("JEngine.Core.JBehaviour")))
            {
                try
                {
                    cf.value = value.ToString();
                }
                catch
                {
                    Log.PrintWarning($"无法对JBehaviour派生类的属性{cf.fieldName}自动赋值构造值（如果没有可忽略）");
                }
            }
        }
    }
}