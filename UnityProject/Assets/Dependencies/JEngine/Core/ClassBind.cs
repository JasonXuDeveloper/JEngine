using libx;
using System;
using Malee.List;
using System.Linq;
using UnityEngine;
using System.Reflection;
using ILRuntime.CLR.Utils;
using ILRuntime.CLR.TypeSystem;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;


namespace JEngine.Core
{
    [HelpURL("https://xgamedev.uoyou.com/classbind-v0-6.html")]
    public class ClassBind : MonoBehaviour
    {
        [FormerlySerializedAs("ScriptsToBind")] public ClassData[] scriptsToBind = new ClassData[1];

        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="classData"></param>
        public void SetVal(ClassData classData)
        {
            string classType = $"{classData.classNamespace + (classData.classNamespace == "" ? "" : ".")}{classData.className}";
            InitJEngine.Appdomain.LoadedTypes.TryGetValue(classType,out var type);
            Type t = type?.ReflectionType;//获取实际属性
            //这里获取适配器类型接口，不直接获取Mono适配器了，因为不同的类型适配器不一样
            var clrInstance = gameObject.GetComponents<CrossBindingAdaptorType>()
                .Last(clr => clr.ILInstance.Type == type as ILType);
            //绑定数据
            if (classData.requireBindFields)
            {
                classData.BoundData = false;
                var fields = classData.fields.ToArray();

                foreach (ClassField field in fields)
                {
                    if (field.fieldType == ClassField.FieldType.NotSupported) continue;

                    object obj = null;
                    try
                    {
                        if (field.fieldType == ClassField.FieldType.Number)
                        {
                            var fieldType = t.GetField(field.fieldName,
                                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                BindingFlags.Static).FieldType ?? t.GetProperty(field.fieldName,
                                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                BindingFlags.Static).PropertyType;

                            if (fieldType.FullName == typeof(SByte).FullName)
                            {
                                obj = SByte.Parse(field.value);
                                classData.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(Byte).FullName)
                            {
                                obj = Byte.Parse(field.value);
                                classData.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(Int16).FullName)
                            {
                                obj = Int16.Parse(field.value);
                                classData.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(UInt16).FullName)
                            {
                                obj = UInt16.Parse(field.value);
                                classData.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(Int32).FullName)
                            {
                                obj = Int32.Parse(field.value);
                                classData.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(UInt32).FullName)
                            {
                                obj = UInt32.Parse(field.value);
                                classData.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(Int64).FullName)
                            {
                                obj = Int64.Parse(field.value);
                                classData.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(UInt64).FullName)
                            {
                                obj = UInt64.Parse(field.value);
                                classData.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(Single).FullName)
                            {
                                obj = Single.Parse(field.value);
                                classData.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(Decimal).FullName)
                            {
                                obj = Decimal.Parse(field.value);
                                classData.BoundData = true;
                            }
                            else if (fieldType.FullName == typeof(Double).FullName)
                            {
                                obj = Double.Parse(field.value);
                                classData.BoundData = true;
                            }
                        }

                        else if (field.fieldType == ClassField.FieldType.String)
                        {
                            obj = field.value;
                            classData.BoundData = true;
                        }
                        else if (field.fieldType == ClassField.FieldType.Bool)
                        {
                            field.value = field.value.ToLower();
                            obj = field.value == "true";
                            classData.BoundData = true;
                        }

                        if (field.fieldType == ClassField.FieldType.GameObject)
                        {
                            GameObject go = field.gameObject;
                            if (go == null)
                            {
                                try
                                {
                                    go = field.value == "${this}"
                                        ? gameObject
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
                                catch (Exception) //找父物体（如果抛出空异常）
                                {
                                    go = FindSubGameObject(field);
                                    if (go == null) //如果父物体还不存在
                                    {
                                        continue;
                                    }
                                }
                            }

                            obj = go;
                            classData.BoundData = true;
                        }
                        else if (field.fieldType == ClassField.FieldType.UnityComponent)
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
                                        ? gameObject
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
                                catch (Exception) //找父物体（如果抛出空异常）
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
                                if (tp.FieldType is ILRuntime.Reflection.ILRuntimeType) //如果在热更中
                                {
                                    var components = go.GetComponents<CrossBindingAdaptorType>();
                                    foreach (var c in components)
                                    {
                                        if (c.ILInstance.Type.Name == tName)
                                        {
                                            obj = c.ILInstance;
                                            classData.BoundData = true;
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
                                        classData.BoundData = true;
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
                                    if (pi.PropertyType is ILRuntime.Reflection.ILRuntimeType) //如果在热更中
                                    {
                                        var components = go.GetComponents<CrossBindingAdaptorType>();
                                        foreach (var c in components)
                                        {
                                            if (c.ILInstance.Type.Name == tName)
                                            {
                                                obj = c.ILInstance;
                                                classData.BoundData = true;
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
                                            classData.BoundData = true;
                                        }
                                    }
                                }
                                else
                                {
                                    Log.PrintError(
                                        $"自动绑定{name}出错：{classType}.{field.fieldName}赋值出错：{field.fieldName}不存在");
                                }
                            }
                        }
                        else if (field.fieldType == ClassField.FieldType.HotUpdateResource)
                        {
                            obj = Assets.LoadAsset(field.value, typeof(Object)).asset;
                            classData.BoundData = true;
                        }
                    }
                    catch (Exception except)
                    {
                        Log.PrintError(
                            $"自动绑定{name}出错：{classType}.{field.fieldName}获取值{field.value}出错：{except.Message}，已跳过");
                    }

                    //如果有数据再绑定
                    if (classData.BoundData)
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
                                    $"自动绑定{name}出错：{classType}.{field.fieldName}赋值出错：{e.Message}，已跳过");
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
                                Log.PrintError($"自动绑定{name}出错：{classType}不存在{field.fieldName}，已跳过");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Active
        /// </summary>
        /// <param name="classData"></param>
        public void Active(ClassData classData)
        {
            string classType = $"{classData.classNamespace + (classData.classNamespace == "" ? "" : ".")}{classData.className}";
            IType type = InitJEngine.Appdomain.LoadedTypes[classType];
            Type t = type.ReflectionType; //获取实际属性
            //这边获取clrInstance的基类，这样可以获取不同适配器
            var clrInstance = gameObject.GetComponents<CrossBindingAdaptorType>()
                .Last(clr => clr.ILInstance.Type == type as ILType);
            //是否激活
            if (classData.activeAfter)
            {
                if (classData.BoundData == false && classData.requireBindFields && classData.fields.Count > 0)
                {
                    Log.PrintError($"自动绑定{name}出错：{classType}没有成功绑定数据，自动激活成功，但可能会抛出空异常！");
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
                    classData.Activated = true;
                }

                if (awakeMethod == null)
                {
                    Log.PrintError($"{t.FullName}不包含Awake方法，无法激活，已跳过");
                }
                else if (!classData.Activated)
                {
                    awakeMethod.Invoke(t, null);
                }

                classData.Activated = true;
            }

            Remove();
        }

        /// <summary>
        /// Remove cb
        /// </summary>
        private void Remove()
        {
            //添加后删除
            Destroy(this);
        }

        /// <summary>
        /// Add class
        /// </summary>
        /// <param name="classData"></param>
        /// <returns></returns>
        public string AddClass(ClassData classData)
        {
            //添加脚本
            string classType = $"{classData.classNamespace + (classData.classNamespace == "" ? "" : ".")}{classData.className}";
            if (!InitJEngine.Appdomain.LoadedTypes.ContainsKey(classType))
            {
                Log.PrintError($"自动绑定{name}出错：{classType}不存在，已跳过");
                return null;
            }

            IType type = InitJEngine.Appdomain.LoadedTypes[classType];
            Type t = type.ReflectionType; //获取实际属性

            //JBehaviour需自动赋值一个值
            var jBehaviourType = InitJEngine.Appdomain.LoadedTypes["JEngine.Core.JBehaviour"];
            bool isJBehaviour = t.IsSubclassOf(jBehaviourType.ReflectionType);
            bool isMono = t.IsSubclassOf(typeof(MonoBehaviour));

            bool needAdapter = t.BaseType != null &&
                               t.BaseType.GetInterfaces().Contains(typeof(CrossBindingAdaptorType));

            ILTypeInstance instance;
            if (classData.useConstructor)
            {
                instance = isMono ? new ILTypeInstance(type as ILType,false) : InitJEngine.Appdomain.Instantiate(classType);
            }
            else
            {
                instance = new ILTypeInstance(type as ILType, !isMono);
            }
            
            instance.CLRInstance = instance;
            
            //这里是ClassBind的灵魂，我都佩服我自己这么写，所以别乱改这块
            //非mono的跨域继承用特殊的，就是用JEngine提供的一个mono脚本，来显示字段，里面存ILTypeInstance
            //总之JEngine牛逼
            //是继承Mono封装的基类，用自动生成的
            if (needAdapter && isMono && t.BaseType?.FullName != typeof(MonoBehaviourAdapter.Adaptor).FullName && Type.GetType(t.BaseType?.FullName ?? string.Empty)!=null)
            {
                Type adapterType = Type.GetType(t.BaseType?.FullName ?? string.Empty);
                if (adapterType == null)
                {
                    Log.PrintError($"{t.FullName}, need to generate adapter");
                    return null;
                }

                //直接反射赋值一波了
                var clrInstance = gameObject.AddComponent(adapterType);

                var clrEnabled = t.GetProperty("enabled",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                var clrILInstance = t.GetField("instance",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                var clrAppDomain = t.GetField("appdomain",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                clrEnabled?.SetValue(clrInstance, false);
                clrILInstance?.SetValue(clrInstance, instance);
                clrAppDomain?.SetValue(clrInstance, InitJEngine.Appdomain);
                instance.CLRInstance = clrInstance;
            }
            //直接继承Mono的，非继承mono的，或不需要继承的，用这个
            else
            {
                //挂个适配器到编辑器（直接继承mono，非继承mono，无需继承，都可以用这个）
                var clrInstance = gameObject.AddComponent<MonoBehaviourAdapter.Adaptor>();
                clrInstance.enabled = false;
                clrInstance.ILInstance = instance;
                clrInstance.AppDomain = InitJEngine.Appdomain;

                //是MonoBehaviour继承，需要指定CLRInstance
                if (isMono && needAdapter)
                {
                    instance.CLRInstance = clrInstance;
                }

                //判断类型
                clrInstance.isMonoBehaviour = isMono;
                
                classData.Added = true;

                //JBehaviour额外处理
                if (isJBehaviour)
                {
                    clrInstance.isJBehaviour = true;
                    var go = t.GetField("_gameObject", BindingFlags.Public);
                    if (!(go is null)) go.SetValue(clrInstance.ILInstance, gameObject);
                }

                //JBehaviour返回实例ID
                if (isJBehaviour)
                {
                    var f = t.GetField("_instanceID", BindingFlags.NonPublic);
                    if (!(f is null))
                    {
                        var id = f.GetValue(clrInstance.ILInstance).ToString();
                        return id;
                    }
                }
            }
            
            if (classData.useConstructor && isMono)
            {
                var m = type.GetConstructor(Extensions.EmptyParamList);
                if (m != null)
                {
                    InitJEngine.Appdomain.Invoke(m, instance, null);
                }
            }

            return null;
        }


        private GameObject FindSubGameObject(ClassField field)
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
                    Log.PrintError($"自动绑定{name}出错：{field.value}对象被隐藏或不存在，无法获取，已跳过");
                }
            }
            else
            {
                Log.PrintError($"自动绑定{name}出错：{field.value}对象被隐藏或不存在，无法获取，已跳过");
            }

            return null;
        }

        
#if UNITY_EDITOR
        [ContextMenu("Convert Path to GameObject")]
        private void Convert()
        {
            foreach (ClassData @class in scriptsToBind)
            {
                Log.Print(
                    $"<color=#34ebc9>==========Start processing {@class.classNamespace}.{@class.className}==========</color>");
                var fields = @class.fields.ToArray();
                foreach (ClassField field in fields)
                {
                    if (field.fieldType == ClassField.FieldType.NotSupported) continue;
                    
                    if (field.fieldType == ClassField.FieldType.GameObject ||
                        field.fieldType == ClassField.FieldType.UnityComponent)
                    {
                        if (!string.IsNullOrEmpty(field.value))
                        {
                            if (field.value.Contains("."))
                            {
                                field.value =
                                    field.value.Remove(field.value.IndexOf(".", StringComparison.Ordinal));
                            }

                            GameObject go = field.gameObject;
                            if (go == null)
                            {
                                try
                                {
                                    go = field.value == "${this}"
                                        ? gameObject
                                        : GameObject.Find(field.value);
                                    if (go == null) //找父物体
                                    {
                                        go = FindSubGameObject(field);
                                    }
                                }
                                catch (Exception) //找父物体（如果抛出空异常）
                                {
                                    go = FindSubGameObject(field);
                                }   
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
                    $"<color=#34ebc9>==========Finish processing {@class.classNamespace}.{@class.className}==========</color>");
            }

            //转换后保存场景
            var scene = SceneManager.GetActiveScene();
            bool saveResult = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene,scene.path);
            Debug.Log("Saved Scene " + scene.path + " " +(saveResult ? "Success" : "Failed!"));
        }
#endif
    }


    [Serializable]
    public class ClassData
    {
        [FormerlySerializedAs("Namespace")] public string classNamespace = "HotUpdateScripts";
        [FormerlySerializedAs("Class")] public string className = "";
        [FormerlySerializedAs("ActiveAfter")] public bool activeAfter;
        [FormerlySerializedAs("RequireBindFields")] public bool requireBindFields;

        [FormerlySerializedAs("UseConstructor")] [Tooltip("是否使用构造函数")]
        public bool useConstructor = true;

        [FormerlySerializedAs("Fields")] [Reorderable(elementNameProperty = "fieldName")]
        public FieldList fields;

        public bool BoundData { get; set; }
        public bool Added { get; set; }
        public bool Activated { get; set; }
    }

    [Serializable]
    public class ClassField
    {
        public enum FieldType
        {
            Number,
            String,
            Bool,
            GameObject,
            UnityComponent,
            HotUpdateResource,
            NotSupported
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

    [Serializable]
    public class FieldList : ReorderableArray<ClassField>
    {
    }
}
