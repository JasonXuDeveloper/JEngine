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
using ILRuntime.Runtime.Intepreter;
using ProjectAdapter;
using UnityEngine;
using Component = UnityEngine.Component;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace JEngine.Core
{
    [HelpURL("https://github.com/JasonXuDeveloper/JEngine/wiki/%e4%bb%a3%e7%a0%81%e7%bb%91%e5%ae%9a")]
    public class ClassBind : MonoBehaviour
    {
        public _ClassBind[] ScriptsToBind = new _ClassBind[1];

        private bool _binding = false;

        private async void Awake()
        {
            while (!Init.Inited)
            {
                await Task.Delay(10);
            }

            Bind();
        }

        public void Bind()
        {
            if(_binding) return;
            _binding = true;
            
            var cb = this;
            foreach (_ClassBind _class in cb.ScriptsToBind)
            {
                if (_class == null) return;
                AddClass(_class);
            }
            
            //添加后删除
            Destroy(cb);
        }

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
            Type t = type.ReflectionType;//获取实际属性
            var instance = _class.UseConstructor
                ? Init.appdomain.Instantiate(classType)
                : new ILTypeInstance(type as ILType, false);
                
                
                
            //JBehaviour需自动赋值一个值
            var JBehaviourType = Init.appdomain.LoadedTypes["JEngine.Core.JBehaviour"];
            bool isJBehaviour = t.IsSubclassOf(JBehaviourType.ReflectionType);
            bool isMono = t.IsSubclassOf(typeof(MonoBehaviour));
            
            if (_class.UseConstructor && isMono)
            {
                JEngine.Core.Log.PrintWarning($"{t.FullName}由于带构造函数生成，会有来自Unity的警告，请忽略");

            }

            var clrInstance = this.gameObject.AddComponent<MonoBehaviourAdapter.Adaptor>();
            clrInstance.enabled = false;
            clrInstance.ILInstance = instance;
            clrInstance.AppDomain = Init.appdomain;
            instance.CLRInstance = clrInstance;
            
            //判断类型
            clrInstance.isMonoBehaviour = isMono;
            
            if (isJBehaviour)
            {
                clrInstance.isJBehaviour = true;
                var go = t.GetField("_gameObject",BindingFlags.Public);
                go.SetValue(clrInstance.ILInstance, this.gameObject);
            }
                
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
                        if (field.fieldType == _ClassField.FieldType.Short)
                        {
                            obj = int.Parse(field.value);
                            _class.BoundData = true;
                        }
                        else if (field.fieldType == _ClassField.FieldType.UShort)
                        {
                            obj = ushort.Parse(field.value);
                            _class.BoundData = true;
                        }
                        else if (field.fieldType == _ClassField.FieldType.Int)
                        {
                            obj = short.Parse(field.value);
                            _class.BoundData = true;
                        }
                        else if (field.fieldType == _ClassField.FieldType.UInt)
                        {
                            obj = uint.Parse(field.value);
                            _class.BoundData = true;
                        }
                        else if (field.fieldType == _ClassField.FieldType.Long)
                        {
                            obj = long.Parse(field.value);
                            _class.BoundData = true;
                        }
                        else if (field.fieldType == _ClassField.FieldType.ULong)
                        {
                            obj = ulong.Parse(field.value);
                            _class.BoundData = true;
                        }
                        else if (field.fieldType == _ClassField.FieldType.Float)
                        {
                            obj = float.Parse(field.value);
                            _class.BoundData = true;
                        }
                        else if (field.fieldType == _ClassField.FieldType.Decimal)
                        {
                            obj = decimal.Parse(field.value);
                            _class.BoundData = true;
                        }
                        else if (field.fieldType == _ClassField.FieldType.Double)
                        {
                            obj = Double.Parse(field.value);
                            _class.BoundData = true;
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
                                    var components = go.GetComponents<MonoBehaviourAdapter.Adaptor>();
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
                                        var components = go.GetComponents<MonoBehaviourAdapter.Adaptor>();
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
                                    Log.PrintError($"自动绑定{this.name}出错：{classType}.{field.fieldName}赋值出错：{field.fieldName}不存在");
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

            //是否激活
            if (_class.ActiveAfter)
            {
                if (_class.BoundData == false && _class.RequireBindFields)
                {
                    Log.PrintError($"自动绑定{this.name}出错：{classType}没有成功绑定数据，无法自动激活，请手动！");
                    return null;
                }

                clrInstance.enabled = true;
                clrInstance.Awake();
            }
                
            //JBehaviour返回实例ID
            if (isJBehaviour)
            {
                var f = t.GetField("_instanceID", BindingFlags.NonPublic);
                var id = f.GetValue(clrInstance.ILInstance).ToString();
                return id;
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

        [Tooltip("如果是GameObject，请填写完整路径，如果没有父物体，请务必为Active，如果有父物体，请确保父物体是Active;\r\n" +
                 "如果是Unity脚本，请填写该脚本所属的GameObject完整路径，参考GameObject写法")]
        [Reorderable(elementNameProperty = "fieldName")]
        public FieldList Fields;

        public bool BoundData { get; set; } = false;
    }

    [System.Serializable]
    public class _ClassField
    {
        public enum FieldType
        {
            Short,
            UShort,
            Int,
            UInt,
            Long,
            ULong,
            Float,
            Decimal,
            Double,
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
