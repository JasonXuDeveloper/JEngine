using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter;
using Malee.List;
using UnityEngine;

namespace JEngine.Core
{
    public class ClassBind: MonoBehaviour
    {
        public _ClassBind[] ScriptsToBind = new _ClassBind[1];

        private async void Start()
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
                    Log.PrintError($"{classType}不存在，已跳过");
                    continue;
                }
                IType type = InitILrt.appDomain.LoadedTypes[classType];
                var instance = new ILTypeInstance(type as ILType,false);
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
                                GameObject go = field.value == "${this}" ? this.gameObject : GameObject.Find(field.value);
                                
                                if (go == null)
                                {
                                    if (field.value.Contains("/")) //如果有父级
                                    {
                                        var parent =
                                            GameObject.Find(field.value.Remove(field.value.IndexOf('/'))); //寻找父物体
                                        if (parent != null)
                                        {
                                            go = parent.transform.Find(field.value.Substring(field.value.IndexOf('/')))
                                                .gameObject;
                                        }
                                        else
                                        {
                                            Log.PrintError($"{field.value}对象被隐藏或不存在，无法获取，已跳过");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        Log.PrintError($"{field.value}对象被隐藏或不存在，无法获取，已跳过");
                                        continue;
                                    }
                                }

                                obj = go;
                                _class.BoundData = true;
                            }
                            else if (field.fieldType == _ClassField.FieldType.UnityComponent)
                            {
                                GameObject go = field.value.Substring(0,7) == "${this}" ? this.gameObject : GameObject.Find(field.value.Substring(0, field.value.LastIndexOf('.')));
                                if (go == null)
                                {
                                    if (field.value.Contains("/")) //如果有父级
                                    {
                                        var parent =
                                            GameObject.Find(field.value.Remove(field.value.IndexOf('/'))); //寻找父物体
                                        if (parent != null)
                                        {
                                            go = parent.transform.Find(field.value.Substring(field.value.IndexOf('/')))
                                                .gameObject;
                                        }
                                        else
                                        {
                                            Log.PrintError($"{field.value}对象被隐藏或不存在，无法获取，已跳过");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        Log.PrintError($"{field.value}对象被隐藏或不存在，无法获取，已跳过");
                                        continue;
                                    }
                                }

                                foreach (var component in go.GetComponents<Component>())
                                {
                                    if (component.GetType().ToString()
                                        .Contains(field.value.Substring(field.value.LastIndexOf('.'))))
                                    {
                                        obj = component;
                                        _class.BoundData = true;
                                        break;
                                    }
                                }
                            }
                        }
                        catch(Exception except)
                        {
                            Log.PrintError($"{classType}.{field.fieldName}赋值出错：{except.Message}，已跳过");
                        }

                        //如果有数据再绑定
                        if (_class.BoundData)
                        {
                            if (t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Contains(t.GetField(field.fieldName,BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)))
                            {
                                try
                                {
                                    t.GetField(field.fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(clrInstance.ILInstance,obj);      
                                }
                                catch (Exception e)
                                {
                                    Log.PrintError($"{classType}.{field.fieldName}赋值出错：{e.Message}，已跳过");
                                }
                            }
                            else
                            {
                                Log.PrintError($"{classType}不存在{field.fieldName}，已跳过");
                            }
                        }
                    }
                }

                //是否激活
                if (_class.ActiveAfter)
                {
                    if (_class.BoundData == false && _class.RequireBindFields)
                    {
                        Log.PrintError($"{classType}没有成功绑定数据，无法自动激活，请手动！");
                        continue;
                    }
                    clrInstance.enabled = true;
                    clrInstance.Awake();
                }
                
                Destroy(cb);
            }
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
            UnityComponent
        }

        public FieldType fieldType;
        public string fieldName;
        public string value;
    }
    
    [System.Serializable]
    public class FieldList : ReorderableArray<_ClassField> {
    }
}
