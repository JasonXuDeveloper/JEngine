using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ILRuntime.CLR.Utils;
using ILRuntime.Mono.Cecil.Pdb;
using ILRuntime.Reflection;
using ILRuntime.Runtime.Intepreter;
using JEngine.Core;
using Malee.List;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using Object = UnityEngine.Object;

namespace JEngine.Editor
{
    [CustomEditor(typeof(ClassBind))]
    internal class ClassBindEditor : UnityEditor.Editor
    {
        private ReorderableList _classBinds;

        void OnEnable()
        {
            _classBinds = new ReorderableList(serializedObject.FindProperty("scriptsToBind"))
            {
                elementNameProperty = "className",
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

            GUILayout.Space(5);

            Setting.MakeHorizontal(50,
                () => { EditorGUILayout.HelpBox(Setting.GetString(SettingString.ClassBindInfo), MessageType.Info); });


            GUILayout.Space(15);
        }


        /// <summary>
        /// 清理/排序/删除
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="toast"></param>
        public static async void CleanFields(ClassBind instance, bool toast = true)
        {
            int affectCounts = 0;
            foreach (var data in instance.scriptsToBind) //遍历
            {
                string className =
                    $"{data.classNamespace + (string.IsNullOrEmpty(data.classNamespace) ? "" : ".")}{data.className}";
                Type t = JEngine.Core.Tools.GetHotType(className); //加载热更类

                if (t == null)
                {
                    EditorUtility.DisplayDialog(Setting.GetString(SettingString.ClassBindErrorTitle),
                        String.Format(Setting.GetString(SettingString.ClassBindErrorContent),
                            className), "OK");
                    return;
                }

                var flag =
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                    BindingFlags.Static;
                if (Setting.ClassBindGetFromBase)
                {
                    flag |= BindingFlags.FlattenHierarchy;
                }
                else
                {
                    flag |= BindingFlags.DeclaredOnly;
                }

                for (int i = 0; i < data.fields.Count; i++)
                {
                    var field = data.fields[i];
                    var fieldType = t.GetField(field.fieldName, flag)?.FieldType ??
                                    t.GetProperty(field.fieldName, flag)?.PropertyType;

                    if (fieldType == null)
                    {
                        Log.PrintError(String.Format(Setting.GetString(SettingString.ClassBindInvalidFieldDeleted),
                            className, field.fieldName));
                        data.fields.RemoveAt(i);
                        i--;
                        continue;
                    }

                    affectCounts++;

                    EditorUtility.DisplayProgressBar(Setting.GetString(SettingString.ClassBindProgress),
                        String.Format(Setting.GetString(SettingString.ClassBindRearrange),
                            field.fieldName, data.fields.IndexOf(field), data.fields.Length),
                        data.fields.IndexOf(field) / (float)data.fields.Length);

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

        /// <summary>
        /// 修改类型
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="toast"></param>
        public static async void DoFieldType(ClassBind instance, bool toast = true)
        {
            int affectCounts = 0;
            foreach (var data in instance.scriptsToBind) //遍历
            {
                string className =
                    $"{data.classNamespace + (string.IsNullOrEmpty(data.classNamespace) ? "" : ".")}{data.className}";
                Type t = JEngine.Core.Tools.GetHotType(className); //加载热更类

                if (t == null)
                {
                    EditorUtility.DisplayDialog(Setting.GetString(SettingString.ClassBindErrorTitle),
                        String.Format(Setting.GetString(SettingString.ClassBindErrorContent),
                            className), "OK");
                    return;
                }

                var flag =
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                    BindingFlags.Static;
                if (Setting.ClassBindGetFromBase)
                {
                    flag |= BindingFlags.FlattenHierarchy;
                }
                else
                {
                    flag |= BindingFlags.DeclaredOnly;
                }

                foreach (var field in data.fields)
                {
                    var fieldType = t.GetField(field.fieldName, flag)?.FieldType ??
                                    t.GetProperty(field.fieldName, flag)?.PropertyType;

                    if (fieldType == null)
                    {
                        Log.PrintError(String.Format(Setting.GetString(SettingString.ClassBindInvalidField),
                            className, field.fieldName));
                    }

                    SetType(field, fieldType);
                    affectCounts++;

                    EditorUtility.DisplayProgressBar(Setting.GetString(SettingString.ClassBindProgress),
                        String.Format(Setting.GetString(SettingString.ClassBindProgressContentForGetField),
                            field.fieldName, data.fields.IndexOf(field), data.fields.Length),
                        data.fields.IndexOf(field) / (float)data.fields.Length);

                    await Task.Delay(50); //延迟一下，动画更丝滑
                }
            }

            EditorUtility.ClearProgressBar();

            TrySave(instance, toast, String.Format(Setting.GetString(SettingString.ClassBindResultContentForGetType),
                    affectCounts, instance.name), Setting.GetString(SettingString.ClassBindResultTitle),
                Setting.GetString(SettingString.Done));
        }

        /// <summary>
        /// 自动匹配
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="toast"></param>
        public static async void DoConvert(ClassBind instance, bool toast = true)
        {
            int affectCounts = 0;
            foreach (var data in instance.scriptsToBind) //遍历
            {
                string className =
                    $"{data.classNamespace + (string.IsNullOrEmpty(data.classNamespace) ? "" : ".")}{data.className}";
                Type t = JEngine.Core.Tools.GetHotType(className); //加载热更类

                if (t == null)
                {
                    EditorUtility.DisplayDialog(Setting.GetString(SettingString.ClassBindErrorTitle),
                        String.Format(Setting.GetString(SettingString.ClassBindErrorContent),
                            className), "OK");
                    return;
                }

                var fieldsInCb = data.fields.Select(f => f.fieldName).ToList(); //全部已经设置的字段
                var flag = BindingFlags.Public | BindingFlags.NonPublic |
                           BindingFlags.SetProperty;
                var flag4Private = BindingFlags.Public | BindingFlags.SetProperty;

                if (Setting.ClassBindGetFromBase)
                {
                    flag |= BindingFlags.FlattenHierarchy;
                    flag4Private |= BindingFlags.FlattenHierarchy;
                }
                else
                {
                    flag |= BindingFlags.DeclaredOnly;
                    flag4Private |= BindingFlags.DeclaredOnly;
                }

                var members = new List<MemberInfo>(0);
                //忽略有ClassBindIgnore标签的
                var type = t;
                while (type != null && type.FullName != null && !type.FullName.Contains(typeof(System.Object).FullName)
                       && !type.FullName.Contains(typeof(MonoBehaviour).FullName)
                       && !type.FullName.Contains("JEngine.Core.JBehaviour"))
                {
                    var fs = type.GetFields(flag).ToList()
                        .FindAll(x => !Attribute.IsDefined(x, typeof(ClassBindIgnoreAttribute), true));
                    var ps = type.GetProperties(flag).ToList()
                        .FindAll(x => !Attribute.IsDefined(x, typeof(ClassBindIgnoreAttribute), true));
                    if (Setting.ClassBindIgnorePrivate)
                    {
                        members.AddRange(fs.FindAll(x => !x.IsPrivate));
                        members.AddRange(type.GetProperties(flag4Private).ToList().FindAll(x =>
                            !Attribute.IsDefined(x, typeof(ClassBindIgnoreAttribute), true)));
                    }
                    else
                    {
                        members.AddRange(fs);
                        members.AddRange(ps);
                    }

                    type = type.BaseType;
                }

                foreach (var field in members)
                {
                    //跳过HideInInspector标签的
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
                        members.ToList().IndexOf(field) / (float)members.Count);

                    if (!fieldsInCb.Contains(field.Name))
                    {
                        ClassField cf = new ClassField();
                        string fieldName = field.Name;
                        cf.fieldName = fieldName;

                        Type fieldType = (field is PropertyInfo)
                            ? ((PropertyInfo)field).PropertyType
                            : ((FieldInfo)field).FieldType;

                        SetType(cf, fieldType);
                        SetVal(ref cf, field, instance.gameObject);

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
            EditorUtility.SetDirty(instance);
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

        private static void SetType(ClassField cf, Type type)
        {
            type =
                type is ILRuntimeWrapperType wrapperType
                    ? wrapperType.RealType
                    : type;
            if (type == typeof(GameObject))
            {
                cf.fieldType = ClassField.FieldType.GameObject;
            }
            else if (type == typeof(Component) || type.IsSubclassOf(typeof(MonoBehaviour)) ||
                     JEngine.Core.Tools.IsJBehaviourType(type))
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
                else if (JEngine.Core.Tools.HasHotType(type.FullName))
                {
                    cf.fieldType = ClassField.FieldType.UnityComponent;
                }
                else
                {
                    cf.fieldType = ClassField.FieldType.NotSupported;
                }
            }
        }

        private static void SetVal(ref ClassField cf, MemberInfo field,
            GameObject instance)
        {
            if (cf.fieldType == ClassField.FieldType.UnityComponent) return;
            object value;
            var type = (field is PropertyInfo)
                ? ((PropertyInfo)field).PropertyType
                : ((FieldInfo)field).FieldType;
            value = type.IsValueType ? Activator.CreateInstance(type) : null;
            SetVal(ref cf, type, value, instance);
        }

        private static void SetVal(ref ClassField cf, Type type, object value, GameObject instance)
        {
            if (type is ILRuntimeType) return;
            if (type != typeof(Object) ||
                !JEngine.Core.Tools.IsJBehaviourType(type))
            {
                try
                {
                    if (type == typeof(String) || cf.fieldType == ClassField.FieldType.String)
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