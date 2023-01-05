# protobuf-net-v2-for-ILRuntime

适配了ILRuntime的protobuf-net

该部分代码已在隔壁[JEngine框架](https://github.com/JasonXuDeveloper/JEngine)使用了大半年了，稳得很

## 注意

不要继承IExtensible，不要继承IExtensible，不要继承IExtensible！

用生成出来的C#文件！

使用前需要在ILRuntime加载AppDomain之前注册一下：

```c#
//Protobuf适配
PType.RegisterILRuntimeCLRRedirection(appdomain);

//CLR重定向
public void Register()
{
  //注册pb反序列化
  Type pbSerializeType = typeof(Serializer);
  var args = new[] {typeof(Type), typeof(Stream)};
  var pbDeserializeMethod = pbSerializeType.GetMethod("Deserialize", flag, null, args, null);
  appdomain.RegisterCLRMethodRedirection(pbDeserializeMethod, Deserialize_1);
  args = new[] {typeof(ILTypeInstance)};
  Dictionary<string, List<MethodInfo>> genericMethods = new Dictionary<string, List<MethodInfo>>();
  List<MethodInfo> lst = null;
  foreach (var m in pbSerializeType.GetMethods())
  {
      if (m.IsGenericMethodDefinition)
      {
          if (!genericMethods.TryGetValue(m.Name, out lst))
          {
              lst = new List<MethodInfo>();
              genericMethods[m.Name] = lst;
          }

          lst.Add(m);
      }
  }

  if (genericMethods.TryGetValue("Deserialize", out lst))
  {
      foreach (var m in lst)
      {
          if (m.MatchGenericParameters(args, typeof(ILTypeInstance), typeof(Stream)))
          {
              var method = m.MakeGenericMethod(args);
              appdomain.RegisterCLRMethodRedirection(method, Deserialize_2);
              break;
          }
      }
  }
}

/// <summary>
/// pb net 反序列化重定向
/// </summary>
/// <param name="__intp"></param>
/// <param name="__esp"></param>
/// <param name="__mStack"></param>
/// <param name="__method"></param>
/// <param name="isNewObj"></param>
/// <returns></returns>
private static unsafe StackObject* Deserialize_1(ILIntepreter __intp, StackObject* __esp,
    IList<object> __mStack, CLRMethod __method, bool isNewObj)
{
    AppDomain __domain = __intp.AppDomain;
    StackObject* ptr_of_this_method;
    StackObject* __ret = ILIntepreter.Minus(__esp, 2);

    ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
    Stream source =
        (Stream) typeof(Stream).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
    __intp.Free(ptr_of_this_method);

    ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
    Type type = (Type) typeof(Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
    __intp.Free(ptr_of_this_method);


    var result_of_this_method = Serializer.Deserialize(type, source);

    object obj_result_of_this_method = result_of_this_method;
    if (obj_result_of_this_method is CrossBindingAdaptorType adaptorType)
    {
        return ILIntepreter.PushObject(__ret, __mStack, adaptorType.ILInstance, true);
    }

    return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method, true);
}

/// <summary>
/// pb net 反序列化重定向
/// </summary>
/// <param name="__intp"></param>
/// <param name="__esp"></param>
/// <param name="__mStack"></param>
/// <param name="__method"></param>
/// <param name="isNewObj"></param>
/// <returns></returns>
private static unsafe StackObject* Deserialize_2(ILIntepreter __intp, StackObject* __esp,
    IList<object> __mStack, CLRMethod __method, bool isNewObj)
{
    AppDomain __domain = __intp.AppDomain;
    StackObject* ptr_of_this_method;
    StackObject* __ret = ILIntepreter.Minus(__esp, 1);

    ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
    Stream source =
        (Stream) typeof(Stream).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
    __intp.Free(ptr_of_this_method);

    var genericArgument = __method.GenericArguments;
    var type = genericArgument[0];
    var realType = type is CLRType ? type.TypeForCLR : type.ReflectionType;
    var result_of_this_method = Serializer.Deserialize(realType, source);

    return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
}
```

## 限制
1. ProtoMember标签只能有一个int字段，不能有其他的（某些情况可以，参考后面的链接），参考[ILRuntime issue #593](https://github.com/Ourpalm/ILRuntime/issues/593)
2. Property不能就```{get;set;}```，轻则报错空引用，重则闪退

## 版本说明

- 魔改的是protobuf-net v2，具体是v2.x我就忘了，估计v2.4.1
- 支持proto2协议，proto3的话你用一些基础的特性应该都支持
- 确保协议在客户端和服务端一致，同proto文件可以先客户端生成c#文件，然后用生成的c#导出proto后同步给服务端使用，以免前后端协议不一致导致问题

## 支持的类型
- 主工程类型
- 纯热更类型
- 数组（热更/非热更的都可以）
- 列表（热更/非热更的都可以）
- 枚举（热更/非热更的都可以）
- 基础的Nullable类型（```Nullable<int>```之类的）
- 热更工程定义的泛型+热更工程的泛型参数

## 暂不支持的类型
- 带有热更类型的字典（map内部热更类型）
- 主工程定义的泛型+热更类型的泛型参数

## 代码导出
支持C#文件和proto文件互转
- 引用命名空间
  ```c#
  using System.IO;
  using ProtoBuf.Reflection;
  using Google.Protobuf.Reflection;
  ```
- proto转C#
  ```c#
  var set = new FileDescriptorSet();
  
  set.AddImportPath(inpath);//inpath是导入目录
  foreach (var inproto in inprotos)//inprotos是inpath下的全部proto文件
  {
    var s = inproto;
    if (!inproto.Contains(".proto"))
    {
      s += ".proto";
    }
  
    set.Add(s, true);
  }
  
  set.Process();
  var errors = set.GetErrors();
  CSharpCodeGenerator.ClearTypeNames();
  var files = CSharpCodeGenerator.Default.Generate(set);
  
  foreach (var file in files)
  {
    CSharpCodeGenerator.ClearTypeNames();
    var path = Path.Combine(outpath, file.Name);
    File.WriteAllText(path, file.Text);
  
    Debug.Log($"Generated cs file for {file.Name.Replace(".cs",".proto")} successfully to: {path}");
  }
  ```
- C#转proto
  ```c#
  //先热更里找
  Type t = Type.GetType(_class);//_class是要导出proto的C#类型的FullName
  
  var mi = typeof(Serializer).GetMethod("GetProto",new Type[] {})?.MakeGenericMethod(t);
  string proto = (string)mi?.Invoke(null,null);
  
  //如果有先删除，这里OUTPUT_PATH是输出路径
  if (File.Exists($"{OUTPUT_PATH}/{_class.Replace('.','_')}.proto"))
  {
    File.Delete($"{OUTPUT_PATH}/{_class.Replace('.','_')}.proto");
  }
  
  //生成
  FileStream stream = new FileStream($"{OUTPUT_PATH}/{_class.Replace('.','_')}.proto", FileMode.Append, FileAccess.Write);
  StreamWriter sw = new StreamWriter(stream);
  Stopwatch watch = new Stopwatch();
  sw.WriteLine(proto);
  watch.Stop();
  Debug.Log($"Generated proto file for {_class} successfully to: {OUTPUT_PATH}/{_class.Replace('.','_')}.proto in {watch.ElapsedMilliseconds} ms");
  sw.Dispose();
  ```
- 测试proto协议（这个可以手写也可以通过C#类导出）：
  ```proto
  syntax = "proto2";
  package JEngine.Examples;
  
  message DataClass {
     optional int32 id = 1 [default = 0];
     optional string name = 2;
     optional int64 money = 3 [default = 0];
     optional bool gm = 4 [default = false];
  }
  ```
- 测试C#类（这个是生成的，也可以手写出来后导出为proto）：
  ```c#
  // This file was generated by a tool; you should avoid making direct changes.
  // Consider using 'partial classes' to extend these types
  // Input: dataClass.proto
  
  #pragma warning disable CS1591, CS0612, CS3021
  
  namespace JEngine.Examples
  {
      [System.Serializable]
      [global::ProtoBuf.ProtoContract()]
      public partial class DataClass
      {
          [global::ProtoBuf.ProtoMember(1)]
          [global::System.ComponentModel.DefaultValue(0)]
          public int id
          {
              get { return __pbn__id ?? 0; }
              set { __pbn__id = value; }
          }
          public bool ShouldSerializeid() => __pbn__id != null;
          public void Resetid() => __pbn__id = null;
          private int? __pbn__id;
  
          [global::ProtoBuf.ProtoMember(2)]
          [global::System.ComponentModel.DefaultValue("")]
          public string name
          {
              get { return __pbn__name ?? ""; }
              set { __pbn__name = value; }
          }
          public bool ShouldSerializename() => __pbn__name != null;
          public void Resetname() => __pbn__name = null;
          private string __pbn__name;
  
          [global::ProtoBuf.ProtoMember(3)]
          [global::System.ComponentModel.DefaultValue(0)]
          public long money
          {
              get { return __pbn__money ?? 0; }
              set { __pbn__money = value; }
          }
          public bool ShouldSerializemoney() => __pbn__money != null;
          public void Resetmoney() => __pbn__money = null;
          private long? __pbn__money;
  
          [global::ProtoBuf.ProtoMember(4)]
          [global::System.ComponentModel.DefaultValue(false)]
          public bool gm
          {
              get { return __pbn__gm ?? false; }
              set { __pbn__gm = value; }
          }
          public bool ShouldSerializegm() => __pbn__gm != null;
          public void Resetgm() => __pbn__gm = null;
          private bool? __pbn__gm;
  
      }
  
  }
  
  public class ILRuntime_dataClass
  {
      static ILRuntime_dataClass()
      {
  
          //Initlize();
  
      }
      public static void Initlize()
      {
  
          ProtoBuf.PType.RegisterType("JEngine.Examples.DataClass", typeof(JEngine.Examples.DataClass));
  
      }
  }
  
  #pragma warning restore CS1591, CS0612, CS3021
  ```
  > 注意，```ILRuntime_dataClass``` 可以忽略，在最新版本中这个类是多余的，无用