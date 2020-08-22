using System;
using System.Collections.Generic;
using System.Threading;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Generated;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using Object = UnityEngine.Object;
// using LitJson;

public class InitILrt : MonoBehaviour
{
    public static unsafe void InitializeILRuntime(AppDomain appdomain)
    {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
        //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
        appdomain.UnityMainThreadID = Thread.CurrentThread.ManagedThreadId;
        appdomain.DebugService.StartDebugService(56000);
#endif

        #region 这里添加ILRuntime的注册 HERE TO ADD ILRuntime Registerations

        appdomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
        appdomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
        appdomain.RegisterCrossBindingAdaptor(new ExceptionAdapter());
        appdomain.RegisterCrossBindingAdaptor(new IAsyncStateMachineClassInheritanceAdaptor());
        
        appdomain.DelegateManager.RegisterMethodDelegate<libx.AssetRequest>();
        appdomain.DelegateManager.RegisterFunctionDelegate<System.Threading.Tasks.Task<ILRuntime.Runtime.Intepreter.ILTypeInstance>>();
        appdomain.DelegateManager.RegisterMethodDelegate<Object>();
        appdomain.DelegateManager.RegisterMethodDelegate<System.Object>();
        appdomain.DelegateManager
            .RegisterFunctionDelegate<ILTypeInstance, Boolean>();
        appdomain.DelegateManager.RegisterMethodDelegate<List<Object>>();
        appdomain.DelegateManager
            .RegisterMethodDelegate<IDictionary<String, UnityEngine.Object>>();
        appdomain.DelegateManager.RegisterMethodDelegate<Boolean>();
        appdomain.DelegateManager.RegisterMethodDelegate<Single>();
        appdomain.DelegateManager.RegisterMethodDelegate<Boolean, GameObject>();
        appdomain.DelegateManager.RegisterMethodDelegate<Int32, Int32>();
        appdomain.DelegateManager.RegisterMethodDelegate<String>();
        appdomain.DelegateManager.RegisterMethodDelegate<ILTypeInstance>();
        appdomain.DelegateManager.RegisterMethodDelegate<GameObject>();
        appdomain.DelegateManager.RegisterMethodDelegate<UIBehaviour, Object>();
        appdomain.DelegateManager.RegisterMethodDelegate<Transform, Object>();
        appdomain.DelegateManager.RegisterMethodDelegate<GameObject>();
        appdomain.DelegateManager.RegisterMethodDelegate<Int32>();
        appdomain.DelegateManager.RegisterMethodDelegate<GameObject, Action>();
        appdomain.DelegateManager.RegisterFunctionDelegate<Object, Boolean>();
        appdomain.DelegateManager.RegisterFunctionDelegate<Boolean>();
        appdomain.DelegateManager.RegisterFunctionDelegate<float>();
        appdomain.DelegateManager.RegisterFunctionDelegate<System.Threading.Tasks.Task>();
        
        appdomain.DelegateManager.RegisterDelegateConvertor<System.Threading.WaitCallback>((act) =>
        {
            return new System.Threading.WaitCallback((state) =>
            {
                ((Action<System.Object>)act)(state);
            });
        });
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityAction>(act =>
        {
            return new UnityAction(() => { ((Action) act)(); });
        });
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityAction<Single>>(act =>
        {
            return new UnityAction<Single>(arg0 =>
            {
                ((Action<Single>) act)(arg0);
            });
        });
        appdomain.DelegateManager.RegisterDelegateConvertor<Predicate<Object>>(act =>
        {
            return new Predicate<Object>(obj =>
            {
                return ((Func<Object, Boolean>) act)(obj);
            });
        });
        appdomain.DelegateManager
            .RegisterDelegateConvertor<Predicate<ILTypeInstance>>(act =>
            {
                return new Predicate<ILTypeInstance>(obj =>
                {
                    return ((Func<ILTypeInstance, Boolean>) act)(obj);
                });
            });
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityAction<Int32>>(act =>
        {
            return new UnityAction<Int32>(arg0 =>
            {
                ((Action<Int32>) act)(arg0);
            });
        });
        // appdomain.DelegateManager.RegisterDelegateConvertor<Action<JsonData>>(action =>
        // {
        //     return new Action<JsonData>(a => { ((Action<JsonData>) action)(a); });
        // });
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityAction>(act =>
        {
            return new UnityAction(async () => { ((Action) act)(); });
        });
        appdomain.DelegateManager.RegisterDelegateConvertor<ThreadStart>(act =>
        {
            return new ThreadStart(() => { ((Action) act)(); });
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

        // JsonMapper.RegisterILRuntimeCLRRedirection(appdomain); //绑定LitJson
        CLRBindings.Initialize(appdomain); //CLR绑定

        #endregion
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
    /// <exception cref="System.NullReferenceException"></exception>
    unsafe static StackObject* AddComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        //CLR重定向的说明请看相关文档和教程，这里不多做解释
        AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        //成员方法的第一个参数为this
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
            throw new NullReferenceException();
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
    /// <exception cref="System.NullReferenceException"></exception>
    unsafe static StackObject* GetComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        //CLR重定向的说明请看相关文档和教程，这里不多做解释
        AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        //成员方法的第一个参数为this
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
            throw new NullReferenceException();
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
