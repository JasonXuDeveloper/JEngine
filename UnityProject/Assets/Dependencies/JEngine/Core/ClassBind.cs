//
// ClassBind.cs
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2020 JEngine
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using libx;
using System;
using System.ComponentModel;
using Malee.List;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ProjectAdapter;
using UnityEngine;
using UnityEngine.Analytics;
using Component = UnityEngine.Component;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace JEngine.Core
{
    [HelpURL("https://xgamedev.uoyou.com/classbind-v0-6.html")]
    public class ClassBind : MonoBehaviour
    {
        public _ClassBind[] ScriptsToBind = new _ClassBind[1];

        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="_class"></param>
        public void SetVal(_ClassBind _class)
        {
            string classType = $"{_class.Namespace + (_class.Namespace == "" ? "" : ".")}{_class.Class}";
            IType type = Init.appdomain.LoadedTypes[classType];
            Type t = type.ReflectionType;//获取实际属性
            //这里获取适配器类型接口，不直接获取Mono适配器了，因为不同的类型适配器不一样
            var clrInstance = this.gameObject.GetComponents<CrossBindingAdaptorType>()
                .Last(clr => clr.ILInstance.Type == type as ILType);
            //绑定数据
            if (_class.RequireBindFields)
            {
                _class.BoundData = false;
                var fields = _class.Fields.ToArray();

                foreach (_ClassField field in fields)
                {
                    object obj = new object();
                    
                    try
                    {
                        if (field.fieldType == _ClassField.FieldType.Number)
                        {
                            var fieldType = t.GetField(field.fieldName,
                                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                BindingFlags.Static).FieldType;
                            if (fieldType == null)
                            {
                                fieldType = t.GetProperty(field.fieldName,
                                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                    BindingFlags.Static).PropertyType;
                            }
                            
                            if (fieldType.FullName == typeof(SByte).FullName)
                            {
                                obj = SByte.Parse(field.value);
                                _class.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(Byte).FullName)
                            {
                                obj = Byte.Parse(field.value);
                                _class.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(Int16).FullName)
                            {
                                obj = Int16.Parse(field.value);
                                _class.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(UInt16).FullName)
                            {
                                obj = UInt16.Parse(field.value);
                                _class.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(Int32).FullName)
                            {
                                obj = Int32.Parse(field.value);
                                _class.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(UInt32).FullName)
                            {
                                obj = UInt32.Parse(field.value);
                                _class.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(Int64).FullName)
                            {
                                obj = Int64.Parse(field.value);
                                _class.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(UInt64).FullName)
                            {
                                obj = UInt64.Parse(field.value);
                                _class.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(Single).FullName)
                            {
                                obj = Single.Parse(field.value);
                                _class.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(Decimal).FullName)
                            {
                                obj = Decimal.Parse(field.value);
                                _class.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(Double).FullName)
                            {
                                obj = Double.Parse(field.value);
                                _class.BoundData = true;
                            }
                        }

                        else if (field.fieldType == _ClassField.FieldType.String)
                        {
                            obj = field.value;
                            _class.BoundData = true;
                        }
                        else if (field.fieldType == _ClassField.FieldType.Bool)
                        {
                            field.value = field.value.ToLower();
                            obj = field.value == "true";
                            _class.BoundData = true;
                        }

                        if (field.fieldType == _ClassField.FieldType.GameObject)
                        {
                            GameObject go = field.gameObject;
                            if (go == null)
                            {
                                try
                                {
                                    go = field.value == "${this}"
                                        ? this.gameObject
                                        : GameObject.Find(field.value);
                                    if (go == null) //找父物体
                                    {
                                        go = FindSubGameObject(field);
                                        if (go == null) //如果父物体还不存在
                                        {
                                            continue;
                                        }
                                    }
                                }
                                catch (Exception ex) //找父物体（如果抛出空异常）
                                {
                                    go = FindSubGameObject(field);
                                    if (go == null) //如果父物体还不存在
                                    {
                                        continue;
                                    }
                                }
                            }

                            obj = go;
                            _class.BoundData = true;
                        }
                        else if (field.fieldType == _ClassField.FieldType.UnityComponent)
                        {
                            GameObject go = field.gameObject;
                            if (go == null)
                            {
                                try
                                {
                                    if (field.value.Contains("."))
                                    {
                                        field.value =
                                            field.value.Remove(field.value.IndexOf(".", StringComparison.Ordinal));
                                    }

                                    go = field.value == "${this}"
                                        ? this.gameObject
                                        : GameObject.Find(field.value);
                                    if (go == null) //找父物体
                                    {
                                        go = FindSubGameObject(field);
                                        if (go == null) //如果父物体还不存在
                                        {
                                            continue;
                                        }
                                    }
                                }
                                catch (Exception ex) //找父物体（如果抛出空异常）
                                {
                                    go = FindSubGameObject(field);
                                    if (go == null) //如果父物体还不存在
                                    {
                                        continue;
                                    }
                                }
                            }

                            var tp = t.GetField(field.fieldName,
                                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                BindingFlags.Static);
                            if (tp != null)
                            {
                                string tName = tp.FieldType.Name;
                                if (tp.FieldType.Assembly.ToString().Contains("ILRuntime")) //如果在热更中
                                {
                                    var components = go.GetComponents<CrossBindingAdaptorType>();
                                    foreach (var c in components)
                                    {
                                        if (c.ILInstance.Type.Name == tName)
                                        {
                                            obj = c.ILInstance;
                                            _class.BoundData = true;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    var component = go.GetComponents<Component>().ToList()
                                        .Find(c => c.GetType().ToString().Contains(tName));
                                    if (component != null)
                                    {
                                        obj = component;
                                        _class.BoundData = true;
                                    }
                                }
                            }
                            else
                            {
                                var pi = t.GetProperty(field.fieldName,
                                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                    BindingFlags.Static);
                                if (pi != null)
                                {
                                    string tName = pi.PropertyType.Name;
                                    if (pi.PropertyType.Assembly.ToString().Contains("ILRuntime")) //如果在热更中
                                    {
                                        var components = go.GetComponents<CrossBindingAdaptorType>();
                                        foreach (var c in components)
                                        {
                                            if (c.ILInstance.Type.Name == tName)
                                            {
                                                obj = c.ILInstance;
                                                _class.BoundData = true;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var component = go.GetComponents<Component>().ToList()
                                            .Find(c => c.GetType().ToString().Contains(tName));
                                        if (component != null)
                                        {
                                            obj = component;
                                            _class.BoundData = true;
                                        }
                                    }
                                }
                                else
                                {
                                    Log.PrintError(
                                        $"自动绑定{this.name}出错：{classType}.{field.fieldName}赋值出错：{field.fieldName}不存在");
                                }
                            }
                        }
                        else if (field.fieldType == _ClassField.FieldType.HotUpdateResource)
                        {
                            obj = Assets.LoadAsset(field.value, typeof(UnityEngine.Object)).asset;
                            _class.BoundData = true;
                        }
                    }
                    catch (Exception except)
                    {
                        Log.PrintError(
                            $"自动绑定{this.name}出错：{classType}.{field.fieldName}获取值{field.value}出错：{except.Message}，已跳过");
                    }

                    //如果有数据再绑定
                    if (_class.BoundData)
                    {
                        var fi = t.GetField(field.fieldName,
                            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                            BindingFlags.Static);
                        if (fi != null)
                        {
                            try
                            {
                                fi.SetValue(clrInstance.ILInstance, obj);
                            }
                            catch (Exception e)
                            {
                                Log.PrintError(
                                    $"自动绑定{this.name}出错：{classType}.{field.fieldName}赋值出错：{e.Message}，已跳过");
                            }
                        }
                        else
                        {
                            //没FieldInfo尝试PropertyInfo
                            var pi = t.GetProperty(field.fieldName,
                                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                BindingFlags.Static);
                            if (pi != null)
                            {
                                pi.SetValue(clrInstance.ILInstance, obj);
                            }
                            else
                            {
                                Log.PrintError($"自动绑定{this.name}出错：{classType}不存在{field.fieldName}，已跳过");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Active
        /// </summary>
        /// <param name="_class"></param>
        public void Active(_ClassBind _class)
        {
            string classType = $"{_class.Namespace + (_class.Namespace == "" ? "" : ".")}{_class.Class}";
            IType type = Init.appdomain.LoadedTypes[classType];
            Type t = type.ReflectionType; //获取实际属性
            //这边获取clrInstance的基类，这样可以获取不同适配器
            var clrInstance = this.gameObject.GetComponents<CrossBindingAdaptorType>()
                .Last(clr => clr.ILInstance.Type == type as ILType);
            //是否激活
            if (_class.ActiveAfter)
            {
                if (_class.BoundData == false && _class.RequireBindFields && _class.Fields.Count > 0)
                {
                    Log.PrintError($"自动绑定{this.name}出错：{classType}没有成功绑定数据，自动激活成功，但可能会抛出空异常！");
                }

                //Mono类型能设置enabled
                if (clrInstance.GetType().IsSubclassOf(typeof(MonoBehaviour)))
                {
                    ((MonoBehaviour) clrInstance).enabled = true;
                }
                
                //不管是啥类型，直接invoke这个awake方法
                var awakeMethod = clrInstance.GetType().GetMethod("Awake",
                    BindingFlags.Default | BindingFlags.Public
                                         | BindingFlags.Instance | BindingFlags.FlattenHierarchy |
                                         BindingFlags.NonPublic | BindingFlags.Static);
                if (awakeMethod == null)
                {
                    awakeMethod = t.GetMethod("Awake",
                        BindingFlags.Default | BindingFlags.Public
                                             | BindingFlags.Instance | BindingFlags.FlattenHierarchy |
                                             BindingFlags.NonPublic | BindingFlags.Static);
                }
                else
                {
                    awakeMethod.Invoke(clrInstance, null);
                    _class.Activated = true;
                }

                if (awakeMethod == null)
                {
                    Log.PrintError($"{t.FullName}不包含Awake方法，无法激活，已跳过");
                }
                else if (!_class.Activated)
                {
                    awakeMethod.Invoke(t, null);
                }

                _class.Activated = true;
            }

            Remove();
        }

        /// <summary>
        /// Remove cb
        /// </summary>
        public void Remove()
        {
            //添加后删除
            Destroy(this);
        }

        /// <summary>
        /// Add class
        /// </summary>
        /// <param name="_class"></param>
        /// <returns></returns>
        public string AddClass(_ClassBind _class)
        {
            //添加脚本
            string classType = $"{_class.Namespace + (_class.Namespace == "" ? "" : ".")}{_class.Class}";
            if (!Init.appdomain.LoadedTypes.ContainsKey(classType))
            {
                Log.PrintError($"自动绑定{this.name}出错：{classType}不存在，已跳过");
                return null;
            }

            IType type = Init.appdomain.LoadedTypes[classType];
            Type t = type.ReflectionType; //获取实际属性

            //JBehaviour需自动赋值一个值
            var JBehaviourType = Init.appdomain.LoadedTypes["JEngine.Core.JBehaviour"];
            bool isJBehaviour = t.IsSubclassOf(JBehaviourType.ReflectionType);
            bool isMono = t.IsSubclassOf(typeof(MonoBehaviour));

            bool needAdapter = t.BaseType != null &&
                               t.BaseType.GetInterfaces().Contains(typeof(CrossBindingAdaptorType));

            var instance = _class.UseConstructor
                ? Init.appdomain.Instantiate(classType)
                : new ILTypeInstance(type as ILType, !isMono);
            instance.CLRInstance = instance;

            if (_class.UseConstructor && isMono)
            {
                JEngine.Core.Log.PrintWarning($"{t.FullName}由于带构造函数生成，会有来自Unity的警告，请忽略");

            }

            //这里是classbind的灵魂，我都佩服我自己这么写，所以别乱改这块
            //非mono的跨域继承用特殊的，就是用JEngine提供的一个mono脚本，来显示字段，里面存ILTypeInstance
            //总之JEngine牛逼
            //是继承Mono封装的基类，用自动生成的
            if (needAdapter && isMono && t.BaseType?.FullName != typeof(MonoBehaviourAdapter.Adaptor).FullName && !Init.appdomain.LoadedTypes.ContainsKey(t.BaseType.FullName))
            {
                Type adapterType = Type.GetType(t.BaseType?.FullName);
                if (adapterType == null)
                {
                    Log.PrintError($"{t.FullName}, need to generate adapter");
                    return null;
                }

                //直接反射赋值一波了
                var clrInstance = gameObject.AddComponent(adapterType);
                var enabled = t.GetProperty("enabled",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                var ILInstance = t.GetField("instance",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                var AppDomain = t.GetField("appdomain",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                enabled.SetValue(clrInstance, false);
                ILInstance.SetValue(clrInstance, instance);
                AppDomain.SetValue(clrInstance, Init.appdomain);
                instance.CLRInstance = clrInstance;
            }
            //直接继承Mono的，非继承mono的，或不需要继承的，用这个
            else
            {
                //挂个适配器到编辑器（直接继承mono，非继承mono，无需继承，都可以用这个）
                var clrInstance = this.gameObject.AddComponent<MonoBehaviourAdapter.Adaptor>();
                clrInstance.enabled = false;
                clrInstance.ILInstance = instance;
                clrInstance.AppDomain = Init.appdomain;

                //是MonoBehaviour继承，需要指定CLRInstance
                if (isMono && needAdapter)
                {
                    instance.CLRInstance = clrInstance;
                }

                //判断类型
                clrInstance.isMonoBehaviour = isMono;
                
                _class.Added = true;

                //JBehaviour额外处理
                if (isJBehaviour)
                {
                    clrInstance.isJBehaviour = true;
                    var go = t.GetField("_gameObject", BindingFlags.Public);
                    go.SetValue(clrInstance.ILInstance, this.gameObject);
                }

                //JBehaviour返回实例ID
                if (isJBehaviour)
                {
                    var f = t.GetField("_instanceID", BindingFlags.NonPublic);
                    var id = f.GetValue(clrInstance.ILInstance).ToString();
                    return id;
                }
            }

            return null;
        }


        private GameObject FindSubGameObject(_ClassField field)
        {
            if (field.value.Contains("/")) //如果有父级
            {
                try
                {
                    var parent =
                        GameObject.Find(field.value.Substring(0,
                            field.value.IndexOf('/'))); //寻找父物体
                    var go = parent.transform
                        .Find(field.value.Substring(field.value.IndexOf('/') + 1))
                        .gameObject;
                    return go;
                }
                catch
                {
                    Log.PrintError($"自动绑定{this.name}出错：{field.value}对象被隐藏或不存在，无法获取，已跳过");
                }
            }
            else
            {
                Log.PrintError($"自动绑定{this.name}出错：{field.value}对象被隐藏或不存在，无法获取，已跳过");
            }

            return null;
        }

        
#if UNITY_EDITOR
        [ContextMenu("Convert Path to GameObject")]
        private void Convert()
        {
            foreach (_ClassBind _class in ScriptsToBind)
            {
                Log.Print(
                    $"<color=#34ebc9>==========Start processing {_class.Namespace}.{_class.Class}==========</color>");
                var fields = _class.Fields.ToArray();
                foreach (_ClassField field in fields)
                {
                    if (field.fieldType == _ClassField.FieldType.GameObject ||
                        field.fieldType == _ClassField.FieldType.UnityComponent)
                    {
                        if (!string.IsNullOrEmpty(field.value))
                        {
                            if (field.value.Contains("."))
                            {
                                field.value =
                                    field.value.Remove(field.value.IndexOf(".", StringComparison.Ordinal));
                            }

                            GameObject go = field.gameObject;
                            try
                            {
                                go = field.value == "${this}"
                                    ? this.gameObject
                                    : GameObject.Find(field.value);
                                if (go == null) //找父物体
                                {
                                    go = FindSubGameObject(field);
                                }
                            }
                            catch (Exception ex) //找父物体（如果抛出空异常）
                            {
                                go = FindSubGameObject(field);
                            }

                            if (go != null)
                            {
                                field.gameObject = go;
                                Log.Print(
                                    $"Convert path {field.value} to GameObject <color=green>successfully</color>");
                                field.value = "";
                            }
                            else
                            {
                                Log.PrintError(
                                    $"Convert path {field.value} to GameObject failed: path does not exists");
                            }
                        }
                    }
                }

                Log.Print(
                    $"<color=#34ebc9>==========Finish processing {_class.Namespace}.{_class.Class}==========</color>");
            }

            //转换后保存场景
            var scene = EditorSceneManager.GetActiveScene();
            bool saveResult = EditorSceneManager.SaveScene(scene,scene.path);
            Debug.Log("Saved Scene " + scene.path + " " +(saveResult ? "Success" : "Failed!"));
        }
#endif
    }


    [System.Serializable]
    public class _ClassBind
    {
        public string Namespace = "HotUpdateScripts";
        public string Class = "";
        public bool ActiveAfter = false;
        public bool RequireBindFields = false;

        [Tooltip("是否使用构造函数")]
        public bool UseConstructor = true;

        [Reorderable(elementNameProperty = "fieldName")]
        public FieldList Fields;

        public bool BoundData { get; set; } = false;
        public bool Added { get; set; } = false;
        public bool Activated { get; set; } = false;
    }

    [System.Serializable]
    public class _ClassField
    {
        public enum FieldType
        {
            Number,
            String,
            Bool,
            GameObject,
            UnityComponent,
            HotUpdateResource
        }

        public FieldType fieldType;

        [Tooltip("需要赋值的键的名字")] public string fieldName;

        [Header("For Normal Value and Hot Update Resource Field Type")]
        [Tooltip("非GameObject和UnityComponent的fieldType的值")]
        public string value;

        [Header("For GameObject or Unity Component Field Type")]
        [Tooltip("如果fieldType是GameObject或UnityComponent，此处可填，否则无效")]
        public GameObject gameObject;
    }

    [System.Serializable]
    public class FieldList : ReorderableArray<_ClassField>
    {
    }
}
