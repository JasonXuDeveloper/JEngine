using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using JEngine.Core;
using libx;
using LitJson;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public class Init : MonoBehaviour
{
    public static Init Instance;
    AppDomain appdomain;
    System.IO.MemoryStream fs;

    void Start()
    {
        Instance = this;
        LoadHotFixAssembly();
        Application.targetFrameRate = 30;
    }

    void LoadHotFixAssembly()
    {
        appdomain = new ILRuntime.Runtime.Enviorment.AppDomain();

        var dllAsset = Assets.LoadAssetAsync("HotUpdateScripts.bytes",typeof(TextAsset));
        dllAsset.completed += delegate
        {
            if (dllAsset.error != null)
            {
                Log.PrintError(dllAsset.error);
                return;
            }

            var dll = (TextAsset)dllAsset.asset;
            
            fs = new MemoryStream(dll.bytes);
            
            try
            {
                appdomain.LoadAssembly(fs, null, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
            }
            catch(Exception e)
            {
                Log.PrintError("加载热更DLL失败，请确保HotUpdateResources/Dll里面有HotUpdateScripts.bytes文件，并且Build Bundle后将DLC传入服务器");
            }
            
            
            InitializeILRuntime();
            OnHotFixLoaded();
        };
    }

    unsafe void InitializeILRuntime()
    {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
        //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
        appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
        appdomain.DebugService.StartDebugService(56000);
#endif

        #region 这里添加ILRuntime的注册 HERE TO ADD ILRuntime Registerations
        
        appdomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
        appdomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
        appdomain.RegisterCrossBindingAdaptor(new IAsyncStateMachineClassInheritanceAdaptor());
        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.GameObject>();
        appdomain.DelegateManager.RegisterFunctionDelegate<System.Boolean>();
        appdomain.DelegateManager.RegisterFunctionDelegate<float>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.Int32>();
        appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.GameObject, System.Action>();
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<System.Int32>>((act) =>
        {
            return new UnityEngine.Events.UnityAction<System.Int32>((arg0) =>
            {
                ((Action<System.Int32>)act)(arg0);
            });
        });
        appdomain.DelegateManager.RegisterDelegateConvertor<Action<JsonData>>((action) =>
        {
            return new Action<JsonData>((a) =>
            {
                ((System.Action<JsonData>)action)(a);
            });
        });
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((act) =>
        {
            return new UnityEngine.Events.UnityAction(async () =>
            {
                ((Action)act)();
            });
        });
        appdomain.DelegateManager.RegisterDelegateConvertor<System.Threading.ThreadStart>((act) =>
        {
            return new System.Threading.ThreadStart(() =>
            {
                ((Action)act)();
            });
        });
        
        //添加MonoBehaviour核心方法
        var arr = typeof(GameObject).GetMethods();
        foreach (var i in arr)
        {
            if (i.Name == "AddComponent" && i.GetGenericArguments().Length == 1)
            {
                appdomain.RegisterCLRMethodRedirection(i, AddComponent);
            }
        }
        foreach (var i in arr)
        {
            if (i.Name == "GetComponent" && i.GetGenericArguments().Length == 1)
            {
                appdomain.RegisterCLRMethodRedirection(i, GetComponent);
            }
        }
        
        LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(appdomain);//绑定LitJson
        ILRuntime.Runtime.Generated.CLRBindings.Initialize(appdomain);//CLR绑定
        #endregion
    }

    unsafe void OnHotFixLoaded()
    {
        appdomain.Invoke("HotUpdateScripts.Program", "RunGame", null, null);
    }
    
    
    /// <summary>
    /// 将文件转换成byte[]数组
    /// </summary>
    /// <param name="fileUrl">文件路径文件名称</param>
    /// <returns>byte[]数组</returns>
    public static byte[] FileToByte(string fileUrl)
    {
        try
        {
            using (FileStream fs = new FileStream(fileUrl, FileMode.Open, FileAccess.Read))
            {
                byte[] byteArray = new byte[fs.Length];
                fs.Read(byteArray, 0, byteArray.Length);
                return byteArray;
            }
        }
        catch
        {
            return null;
        }
    }
    
    private void OnDestroy()
    {
        if (fs != null)
            fs.Close();
        fs = null;
    }
    
    
    /// <summary>
    /// GetComponent 的实现
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    MonoBehaviourAdapter.Adaptor GetComponent(ILType type)
    {
        var arr = GetComponents<MonoBehaviourAdapter.Adaptor>();
        for(int i = 0; i < arr.Length; i++)
        {
            var instance = arr[i];
            if(instance.ILInstance != null && instance.ILInstance.Type == type)
            {
                return instance;
            } 
        }
        return null;
    }
    
    /// <summary>
    /// GetComponentInChildren 的实现
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    MonoBehaviourAdapter.Adaptor GetComponentInChildren(ILType type)
    {
        var arr = GetComponents<MonoBehaviourAdapter.Adaptor>();
        for(int i = 0; i < arr.Length; i++)
        {
            var instance = arr[i];
            if(instance.ILInstance != null && instance.ILInstance.Type == type)
            {
                return instance;
            } 
        }
        return null;
    }

    /// <summary>
    /// AddComponent 实现
    /// </summary>
    /// <param name="__intp"></param>
    /// <param name="__esp"></param>
    /// <param name="__mStack"></param>
    /// <param name="__method"></param>
    /// <param name="isNewObj"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    unsafe static StackObject* AddComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        //CLR重定向的说明请看相关文档和教程，这里不多做解释
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        //成员方法的第一个参数为this
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
            throw new System.NullReferenceException();
        __intp.Free(ptr);

        var genericArgument = __method.GenericArguments;
        //AddComponent应该有且只有1个泛型参数
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res;
            if(type is CLRType)
            {
                //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                res = instance.AddComponent(type.TypeForCLR);
            }
            else
            {
                //热更DLL内的类型比较麻烦。首先我们得自己手动创建实例
                var ilInstance = new ILTypeInstance(type as ILType, false);//手动创建实例是因为默认方式会new MonoBehaviour，这在Unity里不允许
                //接下来创建Adapter实例
                var clrInstance = instance.AddComponent<MonoBehaviourAdapter.Adaptor>();
                //unity创建的实例并没有热更DLL里面的实例，所以需要手动赋值
                clrInstance.ILInstance = ilInstance;
                clrInstance.AppDomain = __domain;
                //这个实例默认创建的CLRInstance不是通过AddComponent出来的有效实例，所以得手动替换
                ilInstance.CLRInstance = clrInstance;

                res = clrInstance.ILInstance;//交给ILRuntime的实例应该为ILInstance

                clrInstance.Awake();//因为Unity调用这个方法时还没准备好所以这里补调一次
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }

    /// <summary>
    /// GetComponent 的实现
    /// </summary>
    /// <param name="__intp"></param>
    /// <param name="__esp"></param>
    /// <param name="__mStack"></param>
    /// <param name="__method"></param>
    /// <param name="isNewObj"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    unsafe static StackObject* GetComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        //CLR重定向的说明请看相关文档和教程，这里不多做解释
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        //成员方法的第一个参数为this
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
            throw new System.NullReferenceException();
        __intp.Free(ptr);

        var genericArgument = __method.GenericArguments;
        //AddComponent应该有且只有1个泛型参数
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res = null;
            if (type is CLRType)
            {
                //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                res = instance.GetComponent(type.TypeForCLR);
            }
            else
            {
                //因为所有DLL里面的MonoBehaviour实际都是这个Component，所以我们只能全取出来遍历查找
                var clrInstances = instance.GetComponents<MonoBehaviourAdapter.Adaptor>();
                for(int i = 0; i < clrInstances.Length; i++)
                {
                    var clrInstance = clrInstances[i];
                    if (clrInstance.ILInstance != null)//ILInstance为null, 表示是无效的MonoBehaviour，要略过
                    {
                        if (clrInstance.ILInstance.Type == type)
                        {
                            res = clrInstance.ILInstance;//交给ILRuntime的实例应该为ILInstance
                            break;
                        }
                    }
                }
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }
}
