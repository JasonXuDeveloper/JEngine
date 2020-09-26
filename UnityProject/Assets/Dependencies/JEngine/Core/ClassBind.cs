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
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter;
using libx;
using Malee.List;
using UnityEngine;

namespace JEngine.Core
{
    [DisallowMultipleComponent]
    public class ClassBind : MonoBehaviour
    {
        public _ClassBind[] ScriptsToBind = new _ClassBind[1];

        private async void Awake()
        {
            while (InitILrt.appDomain == null)
            {
                await Task.Delay(50);
            }

            Bind();
        }

        private void Bind()
        {
            var cb = this;
            foreach (var _class in cb.ScriptsToBind)
            {
                //添加脚本
                string classType = $"{_class.Namespace + (_class.Namespace == "" ? "" : ".")}{_class.Class}";
                if (!InitILrt.appDomain.LoadedTypes.ContainsKey(classType))
                {
                    Log.PrintError($"自动绑定{this.name}出错：{classType}不存在，已跳过");
                    continue;
                }

                IType type = InitILrt.appDomain.LoadedTypes[classType];
                var instance = new ILTypeInstance(type as ILType, false);
                var clrInstance = cb.gameObject.AddComponent<MonoBehaviourAdapter.Adaptor>();
                clrInstance.enabled = false;
                clrInstance.ILInstance = instance;
                clrInstance.AppDomain = InitILrt.appDomain;
                instance.CLRInstance = clrInstance;

                //绑定数据
                if (_class.RequireBindFields)
                {
                    _class.BoundData = false;

                    //获取实际属性
                    Type t = type.ReflectionType;

                    foreach (var field in _class.Fields)
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
                                obj = field.value == "true";
                                _class.BoundData = true;
                            }
                            else if (field.fieldType == _ClassField.FieldType.GameObject)
                            {
                                GameObject go = null;
                                try
                                {
                                    go = field.value == "${this}"
                                        ? this.gameObject
                                        : GameObject.Find(field.value);
                                    if (go == null)//找父物体
                                    {
                                        go = FindSubGameObject(field);
                                        if (go == null) //如果父物体还不存在
                                        {
                                            continue;
                                        }
                                    }
                                }
                                catch (Exception ex)//找父物体（如果抛出空异常）
                                {
                                    go = FindSubGameObject(field);
                                    if (go == null) //如果父物体还不存在
                                    {
                                        continue;
                                    }
                                }

                                obj = go;
                                _class.BoundData = true;
                            }
                            else if (field.fieldType == _ClassField.FieldType.UnityComponent)
                            {
                                GameObject go = null;
                                try
                                {
                                    if (field.value.Contains("."))
                                    {
                                        field.value = field.value.Remove(field.value.IndexOf(".", StringComparison.Ordinal));
                                    }
                                    go = field.value == "${this}"
                                        ? this.gameObject
                                        : GameObject.Find(field.value);
                                    if (go == null)//找父物体
                                    {
                                        go = FindSubGameObject(field);
                                        if (go == null) //如果父物体还不存在
                                        {
                                            continue;
                                        }
                                    }
                                }
                                catch (Exception ex)//找父物体（如果抛出空异常）
                                {
                                    go = FindSubGameObject(field);
                                    if (go == null) //如果父物体还不存在
                                    {
                                        continue;
                                    }
                                }

                                var tp = t.GetField(field.fieldName,
                                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                    BindingFlags.Static);
                                if (tp!=null)
                                {
                                    string tName = tp.FieldType.Name;
                                    if (tp.FieldType.Assembly.ToString().Contains("ILRuntime"))//如果在热更中
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
                            }
                            else if (field.fieldType == _ClassField.FieldType.HotUpdateResource)
                            {
                                obj = Assets.LoadAsset(field.value, typeof(UnityEngine.Object)).asset;
                                _class.BoundData = true;
                            }
                        }
                        catch (Exception except)
                        {
                            Log.PrintError($"自动绑定{this.name}出错：{classType}.{field.fieldName}获取值{field.value}出错：{except.Message}，已跳过");
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
                                Log.PrintError($"自动绑定{this.name}出错：{classType}不存在{field.fieldName}，已跳过");
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
                        continue;
                    }

                    clrInstance.enabled = true;
                    clrInstance.Awake();
                }

                Destroy(cb);
            }
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
    }


    [System.Serializable]
    public class _ClassBind
    {
        public string Namespace = "HotUpdateScripts";
        public string Class = "";
        public bool ActiveAfter = false;
        public bool RequireBindFields = false;
        [Tooltip("如果是GameObject，请填写完整路径，并且Active为true;\r\n" +
                 "如果是Unity脚本，需要填写GameObject全路径.脚本名称（脚本名称无空格，例如：Canvas/Text.Text，并且GameObject的Active为true）")]
        [Reorderable]public FieldList Fields;
        public bool BoundData
        {
            get;
            set;
        } = false;
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
        public string fieldName;
        public string value;
    }
    
    [System.Serializable]
    public class FieldList : ReorderableArray<_ClassField> {
    }
}
