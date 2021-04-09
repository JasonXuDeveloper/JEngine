using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using JEngine.Core;
using JEngine.Interface;
using ProtoBuf;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using Debug = UnityEngine.Debug;
using Extensions = ILRuntime.CLR.Utils.Extensions;
using Object = System.Object;

namespace JEngine.Helper
{
    public class RegisterCLRMethodRedirectionHelper : IRegisterHelper
    {
        private static RegisterCLRMethodRedirectionHelper Instance;

        public static void HelperRegister(AppDomain appdomain)
        {
            if (Instance == null)
            {
                Instance = new RegisterCLRMethodRedirectionHelper();
            }

            Instance.Register(appdomain);
        }

        public unsafe void Register(AppDomain appdomain)
        {
            //从自动生成的里面复制的需要这个
            Type[] args;
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static |
                                BindingFlags.DeclaredOnly;

            //注册Add Component
            Type gameObjectType = typeof(GameObject);
            Type componentType = typeof(Component);
            var addComponentMethod = gameObjectType.GetMethods().ToList()
                .Find(i => i.Name == "AddComponent" && i.GetGenericArguments().Length == 1);
            appdomain.RegisterCLRMethodRedirection(addComponentMethod, AddComponent);
            var addComponentMethod2 = gameObjectType.GetMethods().ToList()
                .Find(i => i.Name == "AddComponent" && i.GetGenericArguments().Length != 1);
            appdomain.RegisterCLRMethodRedirection(addComponentMethod2, AddComponent2);

            //注册get，有2种get component，一个是GameObject调用，一个是脚本调用
            var getComponentMethod = gameObjectType.GetMethods().ToList()
                .Find(i => i.Name == "GetComponent" && i.GetGenericArguments().Length == 1);
            appdomain.RegisterCLRMethodRedirection(getComponentMethod, GetComponent);
            var getComponentMethod2 = componentType.GetMethods().ToList()
                .Find(i => i.Name == "GetComponent" && i.GetGenericArguments().Length == 1);
            appdomain.RegisterCLRMethodRedirection(getComponentMethod2, GetComponent);
            
            //get还能是字符串。
            args = new[]{typeof(String)};
            var getComponentMethod3 = gameObjectType.GetMethod("GetComponent", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(getComponentMethod3, GetComponent_1);
            var getComponentMethod4 = componentType.GetMethod("GetComponent", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(getComponentMethod4, GetComponent_1);

            //注册send message
            args = new[] {typeof(String)};
            var sendMessageMethod1 = gameObjectType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageMethod1, SendMessage_1);
            args = new[] {typeof(String), typeof(Object)};
            var sendMessageMethod2 = gameObjectType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageMethod2, SendMessage_2);
            args = new[] {typeof(String), typeof(SendMessageOptions)};
            var sendMessageMethod3 = gameObjectType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageMethod3, SendMessage_3);
            args = new[] {typeof(String), typeof(Object), typeof(SendMessageOptions)};
            var sendMessageMethod4 = gameObjectType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageMethod4, SendMessage_4);

            //注册send message upwards
            args = new[] {typeof(String)};
            var sendMessageUpwardsMethod1 = gameObjectType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageUpwardsMethod1, SendMessageUpwards_1);
            args = new[] {typeof(String), typeof(Object)};
            var sendMessageUpwardsMethod2 = gameObjectType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageUpwardsMethod2, SendMessageUpwards_2);
            args = new[] {typeof(String), typeof(SendMessageOptions)};
            var sendMessageUpwardsMethod3 = gameObjectType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageUpwardsMethod3, SendMessageUpwards_3);
            args = new[] {typeof(String), typeof(Object), typeof(SendMessageOptions)};
            var sendMessageUpwardsMethod4 = gameObjectType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageUpwardsMethod4, SendMessageUpwards_4);

            //注册BroadcastMessage
            args = new[] {typeof(String)};
            var broadcastMessageMethod1 = gameObjectType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(broadcastMessageMethod1, BroadcastMessage_1);
            args = new[] {typeof(String), typeof(Object)};
            var broadcastMessageMethod2 = gameObjectType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(broadcastMessageMethod2, BroadcastMessage_2);
            args = new[] {typeof(String), typeof(SendMessageOptions)};
            var broadcastMessageMethod3 = gameObjectType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(broadcastMessageMethod3, BroadcastMessage_3);
            args = new[] {typeof(String), typeof(Object), typeof(SendMessageOptions)};
            var broadcastMessageMethod4 = gameObjectType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(broadcastMessageMethod4, BroadcastMessage_4);

            //注册send message 4 component
            args = new[] {typeof(String)};
            var sendMessageMethod4ComponentType1 = componentType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageMethod4ComponentType1, SendMessage_1);
            args = new[] {typeof(String), typeof(Object)};
            var sendMessageMethod4ComponentType2 = componentType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageMethod4ComponentType2, SendMessage_2);
            args = new[] {typeof(String), typeof(SendMessageOptions)};
            var sendMessageMethod4ComponentType3 = componentType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageMethod4ComponentType3, SendMessage_3);
            args = new[] {typeof(String), typeof(Object), typeof(SendMessageOptions)};
            var sendMessageMethod4ComponentType4 = componentType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageMethod4ComponentType4, SendMessage_4);

            //注册send message upwards 4 component
            args = new[] {typeof(String)};
            var sendMessageUpwardsMethod4ComponentType1 =
                componentType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageUpwardsMethod4ComponentType1, SendMessageUpwards_1);
            args = new[] {typeof(String), typeof(Object)};
            var sendMessageUpwardsMethod4ComponentType2 =
                componentType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageUpwardsMethod4ComponentType2, SendMessageUpwards_2);
            args = new[] {typeof(String), typeof(SendMessageOptions)};
            var sendMessageUpwardsMethod4ComponentType3 =
                componentType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageUpwardsMethod4ComponentType3, SendMessageUpwards_3);
            args = new[] {typeof(String), typeof(Object), typeof(SendMessageOptions)};
            var sendMessageUpwardsMethod4ComponentType4 =
                componentType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(sendMessageUpwardsMethod4ComponentType4, SendMessageUpwards_4);

            //注册BroadcastMessage 4 component
            args = new[] {typeof(String)};
            var broadcastMessageMethod4ComponentType1 =
                componentType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(broadcastMessageMethod4ComponentType1, BroadcastMessage_1);
            args = new[] {typeof(String), typeof(Object)};
            var broadcastMessageMethod4ComponentType2 =
                componentType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(broadcastMessageMethod4ComponentType2, BroadcastMessage_2);
            args = new[] {typeof(String), typeof(SendMessageOptions)};
            var broadcastMessageMethod4ComponentType3 =
                componentType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(broadcastMessageMethod4ComponentType3, BroadcastMessage_3);
            args = new[] {typeof(String), typeof(Object), typeof(SendMessageOptions)};
            var broadcastMessageMethod4ComponentType4 =
                componentType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(broadcastMessageMethod4ComponentType4, BroadcastMessage_4);

            //注册3种Log
            Type debugType = typeof(Debug);
            var logMethod = debugType.GetMethod("Log", new[] {typeof(object)});
            appdomain.RegisterCLRMethodRedirection(logMethod, Log);
            var logWarningMethod = debugType.GetMethod("LogWarning", new[] {typeof(object)});
            appdomain.RegisterCLRMethodRedirection(logWarningMethod, LogWarning);
            var logErrorMethod = debugType.GetMethod("LogError", new[] {typeof(object)});
            appdomain.RegisterCLRMethodRedirection(logErrorMethod, LogError);

            //注册3种Print
            Type printType = typeof(Log);
            var printMethod = printType.GetMethod("Print", new[] {typeof(object)});
            appdomain.RegisterCLRMethodRedirection(printMethod, Print);
            var printWarningMethod = printType.GetMethod("PrintWarning", new[] {typeof(object)});
            appdomain.RegisterCLRMethodRedirection(printWarningMethod, PrintWarning);
            var printErrorMethod = printType.GetMethod("PrintError", new[] {typeof(object)});
            appdomain.RegisterCLRMethodRedirection(printErrorMethod, PrintError);

            //注册Instantiate
            Type unityObjectType = typeof(UnityEngine.Object);
            var instantiateMethods = unityObjectType.GetMethods().ToList()
                .FindAll(i => i.Name == "Instantiate");
            foreach (var instantiateMethod in instantiateMethods)
            {
                appdomain.RegisterCLRMethodRedirection(instantiateMethod, Instantiate);
            }

            //注册Invoke
            Type monoType = typeof(MonoBehaviour);
            args = new[] {typeof(String), typeof(Single)};
            var invokeMethod = monoType.GetMethod("Invoke", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(invokeMethod, Invoke_1);
            
            //注册InvokeRepeating
            args = new[]{typeof(String), typeof(Single), typeof(Single)};
            var invokeRepeatingMethod = monoType.GetMethod("InvokeRepeating", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(invokeRepeatingMethod, InvokeRepeating_2);
            
            //注册CancelInvoke
            args = new Type[]{};
            var cancelInvokeMethod = monoType.GetMethod("CancelInvoke", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(cancelInvokeMethod, CancelInvoke_3);
            args = new[]{typeof(String)};
            cancelInvokeMethod = monoType.GetMethod("CancelInvoke", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(cancelInvokeMethod, CancelInvoke_4);
            
            //注册isInvoking
            args = new Type[]{};
            var isInvokingMethod = monoType.GetMethod("IsInvoking", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(isInvokingMethod, IsInvoking_5);
            args = new[]{typeof(String)};
            isInvokingMethod = monoType.GetMethod("IsInvoking", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(isInvokingMethod, IsInvoking_6);
            
            //注册pb反序列化
            Type pbSerializeType = typeof(Serializer);
            args = new[]{typeof(Type), typeof(Stream)};
            var pbDeserializeMethod = pbSerializeType.GetMethod("Deserialize", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(pbDeserializeMethod, Deserialize_1);
            args = new[]{typeof(ILTypeInstance)};
            Dictionary<string, List<MethodInfo>> genericMethods = new Dictionary<string, List<MethodInfo>>();
            List<MethodInfo> lst = null;                 
            if (genericMethods.TryGetValue("Deserialize", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(ILTypeInstance), typeof(Stream)))
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
        private static unsafe StackObject* Deserialize_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Stream source = (Stream)typeof(Stream).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Type type = (Type)typeof(Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = Serializer.Deserialize(type, source);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance, true);
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
        private static unsafe StackObject* Deserialize_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Stream source = (Stream)typeof(Stream).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var genericArgument = __method.GenericArguments;
            var type = genericArgument[0];
            var realType = type is CLRType ? type.TypeForCLR : type.ReflectionType;
            var result_of_this_method = Serializer.Deserialize(realType,source);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
        
        /// <summary>
        /// 重定向CancelInvoke
        /// </summary>
        /// <param name="__intp"></param>
        /// <param name="__esp"></param>
        /// <param name="__mStack"></param>
        /// <param name="__method"></param>
        /// <param name="isNewObj"></param>
        /// <returns></returns>
        private static unsafe  StackObject* CancelInvoke_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILTypeInstance val = (ILTypeInstance) (StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            MonoBehaviour adapter = (MonoBehaviour)typeof(MonoBehaviour).CheckCLRTypes(val);
            __intp.Free(ptr_of_this_method);

            //垃圾Unity，Invoke热更内的方法，目前得用本地的接口执行
            Type type = val.Type.ReflectionType;
            //系统反射接口
            MethodInfo[] methods = type.GetMethods(
                BindingFlags.IgnoreCase
                | BindingFlags.NonPublic
                | BindingFlags.Static);

            adapter.CancelInvoke();
            foreach (var mi in methods)
            {
                if (_invokeTokens.TryGetValue(mi, out var ts))
                {
                    ts.Cancel();
                }
                if (_invokeRepeatingTokens.TryGetValue(mi, out ts))
                {
                    ts.Cancel();
                }
            }
            
            return __ret;
        }
        
        /// <summary>
        /// 重定向CancelInvoke
        /// </summary>
        /// <param name="__intp"></param>
        /// <param name="__esp"></param>
        /// <param name="__mStack"></param>
        /// <param name="__method"></param>
        /// <param name="isNewObj"></param>
        /// <returns></returns>
        private static unsafe  StackObject* CancelInvoke_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            String methodName = (String)typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILTypeInstance val = (ILTypeInstance) (StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            MonoBehaviour adapter = (MonoBehaviour)typeof(MonoBehaviour).CheckCLRTypes(val);
            __intp.Free(ptr_of_this_method);

            //垃圾Unity，Invoke热更内的方法，目前得用本地的接口执行
            Type type = val.Type.ReflectionType;
            //系统反射接口
            MethodInfo mi = type.GetMethod(methodName,
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
                | BindingFlags.Instance
                |BindingFlags.FlattenHierarchy
                );

            if (mi == null)
            {
                adapter.CancelInvoke(methodName);
                return __ret;
            }

            if (_invokeTokens.TryGetValue(mi, out var ts))
            {
                ts.Cancel();
            }
            if (_invokeRepeatingTokens.TryGetValue(mi, out ts))
            {
                ts.Cancel();
            }

            return __ret;
        }

        /// <summary>
        /// 重定向IsInvoking
        /// </summary>
        /// <param name="__intp"></param>
        /// <param name="__esp"></param>
        /// <param name="__mStack"></param>
        /// <param name="__method"></param>
        /// <param name="isNewObj"></param>
        /// <returns></returns>
        private static unsafe  StackObject* IsInvoking_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILTypeInstance val = (ILTypeInstance) (StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            MonoBehaviour adapter = (MonoBehaviour)typeof(MonoBehaviour).CheckCLRTypes(val);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = adapter.IsInvoking();
            if (!result_of_this_method)
            {
                //垃圾Unity，Invoke热更内的方法，目前得用本地的接口执行
                Type type = val.Type.ReflectionType;
                //系统反射接口
                MethodInfo[] methods = type.GetMethods(
                    BindingFlags.IgnoreCase
                    | BindingFlags.NonPublic
                    | BindingFlags.Static);
                foreach (var mi in methods)
                {
                    if (_invokeTokens.TryGetValue(mi, out var ts))
                    {
                        if (!ts.IsCancellationRequested)
                        {
                            result_of_this_method = true;
                            break;
                        }
                    }
                    if (_invokeRepeatingTokens.TryGetValue(mi, out ts))
                    {
                        if (!ts.IsCancellationRequested)
                        {
                            result_of_this_method = true;
                            break;
                        }
                    }
                }
            }

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        /// <summary>
        /// 重定向IsInvoking
        /// </summary>
        /// <param name="__intp"></param>
        /// <param name="__esp"></param>
        /// <param name="__mStack"></param>
        /// <param name="__method"></param>
        /// <param name="isNewObj"></param>
        /// <returns></returns>
        private static unsafe StackObject* IsInvoking_6(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            String methodName = (String)typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILTypeInstance val = (ILTypeInstance) (StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            MonoBehaviour adapter = (MonoBehaviour)typeof(MonoBehaviour).CheckCLRTypes(val);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = adapter.IsInvoking(methodName);
            if (!result_of_this_method)
            {
                //垃圾Unity，Invoke热更内的方法，目前得用本地的接口执行
                Type type = val.Type.ReflectionType;
                //系统反射接口
                MethodInfo mi = type.GetMethod(methodName,
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
                | BindingFlags.Instance
                |BindingFlags.FlattenHierarchy
                );
                if (_invokeTokens.TryGetValue(mi, out var ts))
                {
                    if (!ts.IsCancellationRequested)
                    {
                        result_of_this_method = true;
                    }
                }

                if (!result_of_this_method)
                {
                    if (_invokeRepeatingTokens.TryGetValue(mi, out ts))
                    {
                        if (!ts.IsCancellationRequested)
                        {
                            result_of_this_method = true;
                        }
                    }
                }
            }

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }
        
        /// <summary>
        /// 重定向InvokeRepeating
        /// </summary>
        /// <param name="__intp"></param>
        /// <param name="__esp"></param>
        /// <param name="__mStack"></param>
        /// <param name="__method"></param>
        /// <param name="isNewObj"></param>
        /// <returns></returns>
        private static unsafe StackObject* InvokeRepeating_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Single repeatRate = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Single time = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            String methodName = (String)typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            ILTypeInstance val = (ILTypeInstance) (StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            MonoBehaviour adapter = (MonoBehaviour)typeof(MonoBehaviour).CheckCLRTypes(val);
            __intp.Free(ptr_of_this_method);

            //垃圾Unity，Invoke热更内的方法，目前得用本地的接口执行
            Type type = val.Type.ReflectionType;
            //系统反射接口
            MethodInfo mi = type.GetMethod(methodName,
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
                | BindingFlags.Instance
                |BindingFlags.FlattenHierarchy
                );

            if (mi == null)
            {
                adapter.InvokeRepeating(methodName, time, repeatRate);
                return __ret;
            }

            _invokeRepeatingTokens[mi] = new CancellationTokenSource();

            DoInvokeRepeating(val, mi, time,repeatRate,adapter.gameObject);

            return __ret;
        }
        
        private static Dictionary<MethodInfo, CancellationTokenSource> _invokeRepeatingTokens = new Dictionary<MethodInfo, CancellationTokenSource>(0);
        private static Dictionary<MethodInfo, CancellationTokenSource> _invokeTokens = new Dictionary<MethodInfo, CancellationTokenSource>(0);

        private static async void DoInvokeRepeating<T>(T val, MethodInfo methodInfo, float time,float duration,GameObject go)
        {
            if (time > 0)
            {
                await Wait(time, _invokeRepeatingTokens[methodInfo].Token, go);
            }

            while (!_invokeRepeatingTokens[methodInfo].IsCancellationRequested)
            {
                try
                {
                    if (go != null)
                    {
                        Loom.QueueOnMainThread(o =>
                            methodInfo?.Invoke(val, null), null);
                    }
                    else
                    {
                        _invokeRepeatingTokens[methodInfo].Cancel();
                    }
                }
                catch (MissingReferenceException)
                {
                    _invokeRepeatingTokens[methodInfo].Cancel();
                }
                try
                {
                    if (duration > 0)
                    {
                        await Wait(duration, _invokeRepeatingTokens[methodInfo].Token, go);
                    }
                }
                catch(TaskCanceledException)
                {
                    //会抛出TaskCanceledException，表示等待被取消
                }
            }
            _invokeRepeatingTokens.Remove(methodInfo);
        }


        /// <summary>
        /// 重定向Invoke
        /// </summary>
        /// <param name="__intp"></param>
        /// <param name="__esp"></param>
        /// <param name="__mStack"></param>
        /// <param name="__method"></param>
        /// <param name="isNewObj"></param>
        /// <returns></returns>
        private static unsafe StackObject* Invoke_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Single time = *(float*) &ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            ILTypeInstance val = (ILTypeInstance) (StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            MonoBehaviour adapter = (MonoBehaviour)typeof(MonoBehaviour).CheckCLRTypes(val);
            __intp.Free(ptr_of_this_method);

            //垃圾Unity，Invoke热更内的方法，目前得用本地的接口执行
            Type type = val.Type.ReflectionType;
            //系统反射接口
            MethodInfo mi = type.GetMethod(methodName,
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
                | BindingFlags.Instance
                |BindingFlags.FlattenHierarchy
                );
            
            if (mi == null)
            {
                adapter.Invoke(methodName,time);
                return __ret;
            }

            _invokeTokens[mi] = new CancellationTokenSource();
            DoInvoke(val, mi, time,adapter.gameObject);

            return __ret;
        }

        private static async void DoInvoke<T>(T val, MethodInfo methodInfo, float time,GameObject go)
        {
            if (time > 0)
            {
                await Wait(time, _invokeTokens[methodInfo].Token, go);
            }

            try
            {
                if (go != null)
                {
                    Loom.QueueOnMainThread(o =>
                        methodInfo?.Invoke(val, null), null);
                    _invokeTokens.Remove(methodInfo);
                }
            }
            catch (MissingReferenceException)//MissingReference是GO销毁
            {
                _invokeTokens[methodInfo].Cancel();
            }
        }

        /// <summary>
        /// 等待一定时间（针对Invoke设计）
        /// </summary>
        /// <param name="time"></param>
        /// <param name="token"></param>
        /// <param name="go"></param>
        /// <returns></returns>
        private static async Task Wait(float time, CancellationToken token,GameObject go)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                //当没取消的时候继续等
                while (!token.IsCancellationRequested && go != null)
                {
                    //当计时器的时间小于暂停时间转毫秒除以时间系数时，等待1帧
                    if (Time.timeScale == 0 || sw.ElapsedMilliseconds < (time * 1000) / Time.timeScale)
                    {
                        await Task.Delay(1, token);
                    }
                    else//不满足条件就结束了
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //会抛出TaskCanceledException，表示等待被取消，直接返回
                //MissingReference是GO销毁
                if (ex is TaskCanceledException || ex is MissingReferenceException)
                {
                    sw.Stop();
                    return;
                }
            }
                
            sw.Stop();
        }

        /// <summary>
        /// 找到热更对象的gameObject
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private static GameObject FindGOFromHotClass(ILTypeInstance instance)
        {
            var returnType = instance.Type;
            if (returnType.ReflectionType == typeof(MonoBehaviour))
            {
                var pi = returnType.ReflectionType.GetProperty("gameObject");
                return pi.GetValue(instance.CLRInstance) as GameObject;
            }

            if (returnType.ReflectionType.IsSubclassOf(typeof(MonoBehaviour)))
            {
                var pi = returnType.ReflectionType.BaseType.GetProperty("gameObject");
                return pi.GetValue(instance.CLRInstance) as GameObject;
            }

            // foreach (var p in returnType.ReflectionType.GetProperties())
            // {
            //     Debug.Log(p.Name);
            // }
            return null;
        }


        /// <summary>
        /// 处理热更的SendMessage
        /// </summary>
        /// <param name="go"></param>
        /// <param name="methodName"></param>
        /// <param name="value"></param>
        /// <param name="option"></param>
        private static void DoSendMessageOnHotCode(AppDomain __domain, GameObject go, string methodName, object value = null,
            SendMessageOptions option = SendMessageOptions.RequireReceiver)
        {
            //这个option控制了找不到方法是否要报错，但是报错看着太头大，所以我这边就不处理了，留着这个接口

            if (!go.activeSelf) return;//不sendmessage隐藏的

            bool invoked = false;
            var clrInstances = go.GetComponents<CrossBindingAdaptorType>();
            for (int i = 0; i < clrInstances.Length; i++)
            {
                var clrInstance = clrInstances[i];
                if (clrInstance.ILInstance != null) //ILInstance为null, 表示是无效的MonoBehaviour，要略过
                {
                    IType t = clrInstance.ILInstance.Type;
                    if (value != null)//有参数就匹配去调用
                    {
                        IMethod m = t.GetMethod(methodName, 1);
                        if (m != null)
                        {
                            __domain.Invoke(m, clrInstance.ILInstance, value);
                        }
                    }
                    //有参数无匹配，或无参数，都会invoke
                    IMethod method = t.GetMethod(methodName, 0);
                    if (method != null)
                    {
                        __domain.Invoke(method, clrInstance.ILInstance, null);
                    }
                }
            }
        }

        unsafe static StackObject* BroadcastMessage_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);

            GameObject instance_of_this_method;

            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (GameObject) typeof(GameObject).CheckCLRTypes(instance);
            }
            else if (instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else if (instance is Component)
            {
                instance_of_this_method = ((Component) instance).gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"BroadcastMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain,instance_of_this_method, methodName);
            instance_of_this_method.BroadcastMessage(methodName);

            var go = instance_of_this_method.GetComponentsInChildren<Transform>(true);

            foreach (var g in go)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain,g.gameObject, methodName);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            return __ret;
        }

        unsafe static StackObject* BroadcastMessage_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Object value =
                typeof(Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);

            GameObject instance_of_this_method;

            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (GameObject) typeof(GameObject).CheckCLRTypes(instance);
            }
            else if (instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else if (instance is Component)
            {
                instance_of_this_method = ((Component) instance).gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"BroadcastMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain,instance_of_this_method, methodName, value);
            instance_of_this_method.BroadcastMessage(methodName, value);

            var go = instance_of_this_method.GetComponentsInChildren<Transform>(true);

            foreach (var g in go)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain,g.gameObject, methodName, value);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            return __ret;
        }

        unsafe static StackObject* BroadcastMessage_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            SendMessageOptions options =
                (SendMessageOptions) typeof(SendMessageOptions).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);

            GameObject instance_of_this_method;

            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (GameObject) typeof(GameObject).CheckCLRTypes(instance);
            }
            else if (instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else if (instance is Component)
            {
                instance_of_this_method = ((Component) instance).gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"BroadcastMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain,instance_of_this_method, methodName, null, options);
            instance_of_this_method.BroadcastMessage(methodName, options);

            var go = instance_of_this_method.GetComponentsInChildren<Transform>(true);

            foreach (var g in go)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain,g.gameObject, methodName, null, options);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            return __ret;
        }

        unsafe static StackObject* BroadcastMessage_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            SendMessageOptions options =
                (SendMessageOptions) typeof(SendMessageOptions).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Object value =
                typeof(Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);

            GameObject instance_of_this_method;

            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (GameObject) typeof(GameObject).CheckCLRTypes(instance);
            }
            else if (instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else if (instance is Component)
            {
                instance_of_this_method = ((Component) instance).gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"BroadcastMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain,instance_of_this_method, methodName, value, options);
            instance_of_this_method.BroadcastMessage(methodName, value, options);

            var go = instance_of_this_method.GetComponentsInChildren<Transform>(true);

            foreach (var g in go)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain,g.gameObject, methodName, value, options);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            return __ret;
        }

        unsafe static StackObject* SendMessageUpwards_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);

            GameObject instance_of_this_method;

            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (GameObject) typeof(GameObject).CheckCLRTypes(instance);
            }
            else if (instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else if (instance is Component)
            {
                instance_of_this_method = ((Component) instance).gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessageUpwards方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain,instance_of_this_method, methodName);
            instance_of_this_method.SendMessageUpwards(methodName);

            var go = instance_of_this_method.transform.parent.gameObject;
            while (go != null)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain,go, methodName);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                go = go.transform.parent.gameObject;
            }


            return __ret;
        }

        unsafe static StackObject* SendMessageUpwards_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Object value =
                typeof(Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);

            GameObject instance_of_this_method;

            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (GameObject) typeof(GameObject).CheckCLRTypes(instance);
            }
            else if (instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else if (instance is Component)
            {
                instance_of_this_method = ((Component) instance).gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessageUpwards方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain,instance_of_this_method, methodName, value);
            instance_of_this_method.SendMessageUpwards(methodName, value);

            var go = instance_of_this_method.transform.parent.gameObject;
            while (go != null)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain,go, methodName, value);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                go = go.transform.parent.gameObject;
            }

            return __ret;
        }

        unsafe static StackObject* SendMessageUpwards_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            SendMessageOptions options =
                (SendMessageOptions) typeof(SendMessageOptions).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);

            GameObject instance_of_this_method;

            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (GameObject) typeof(GameObject).CheckCLRTypes(instance);
            }
            else if (instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else if (instance is Component)
            {
                instance_of_this_method = ((Component) instance).gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessageUpwards方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain,instance_of_this_method, methodName, null, options);
            instance_of_this_method.SendMessageUpwards(methodName, options);

            var go = instance_of_this_method.transform.parent.gameObject;
            while (go != null)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain,go, methodName, null, options);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                go = go.transform.parent.gameObject;
            }


            return __ret;
        }

        unsafe static StackObject* SendMessageUpwards_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            SendMessageOptions options =
                (SendMessageOptions) typeof(SendMessageOptions).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Object value =
                typeof(Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);

            GameObject instance_of_this_method;

            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (GameObject) typeof(GameObject).CheckCLRTypes(instance);
            }
            else if (instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else if (instance is Component)
            {
                instance_of_this_method = ((Component) instance).gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessageUpwards方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain,instance_of_this_method, methodName, value, options);
            instance_of_this_method.SendMessageUpwards(methodName, value, options);

            var go = instance_of_this_method.transform.parent.gameObject;
            while (go != null)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain,go, methodName, value, options);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                go = go.transform.parent.gameObject;
            }

            return __ret;
        }

        unsafe static StackObject* SendMessage_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);

            GameObject instance_of_this_method;

            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (GameObject) typeof(GameObject).CheckCLRTypes(instance);
            }
            else if (instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else if (instance is Component)
            {
                instance_of_this_method = ((Component) instance).gameObject;
            }
            else if (instance is Component)
            {
                instance_of_this_method = ((Component) instance).gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain,instance_of_this_method, methodName);
            instance_of_this_method.SendMessage(methodName);

            return __ret;
        }

        unsafe static StackObject* SendMessage_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Object value =
                typeof(Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);

            GameObject instance_of_this_method;

            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (GameObject) typeof(GameObject).CheckCLRTypes(instance);
            }
            else if (instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else if (instance is Component)
            {
                instance_of_this_method = ((Component) instance).gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain,instance_of_this_method, methodName, value);
            instance_of_this_method.SendMessage(methodName, value);

            return __ret;
        }

        unsafe static StackObject* SendMessage_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            SendMessageOptions options =
                (SendMessageOptions) typeof(SendMessageOptions).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);

            GameObject instance_of_this_method;

            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (GameObject) typeof(GameObject).CheckCLRTypes(instance);
            }
            else if (instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else if (instance is Component)
            {
                instance_of_this_method = ((Component) instance).gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain,instance_of_this_method, methodName, null, options);
            instance_of_this_method.SendMessage(methodName, options);

            return __ret;
        }

        unsafe static StackObject* SendMessage_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            SendMessageOptions options =
                (SendMessageOptions) typeof(SendMessageOptions).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Object value =
                typeof(Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);

            GameObject instance_of_this_method;

            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (GameObject) typeof(GameObject).CheckCLRTypes(instance);
            }
            else if (instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else if (instance is Component)
            {
                instance_of_this_method = ((Component) instance).gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain,instance_of_this_method, methodName, value, options);
            instance_of_this_method.SendMessage(methodName, value, options);

            return __ret;
        }


        /// <summary>
        /// 帮助Instantiate找到GameObject
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ins"></param>
        /// <param name="returnScipt"></param>
        /// <param name="returnType"></param>
        private static void SetGOForInstantiate(object instance, out GameObject ins, out bool returnScipt,
            out ILType returnType)
        {
            returnType = null;
            returnScipt = false;

            //判断类
            if (instance is GameObject)
            {
                ins = instance as GameObject;
            }
            else if (instance is ILTypeInstance) //如果是热更类需要处理
            {
                returnScipt = true;
                returnType = (instance as ILTypeInstance).Type;
                ins = FindGOFromHotClass((instance as ILTypeInstance));
            }
            else //如果本地类那简单
            {
                returnScipt = true;
                ins = (instance as Component).gameObject;
            }
        }

        private static void CleanGoForInstantiate(ref GameObject res, GameObject ins, AppDomain __domain)
        {
            //如果同时有adaptor和classbind，肯定是复制的，要给删了
            if (res.GetComponent<ClassBind>() != null && res.GetComponent<CrossBindingAdaptorType>() != null)
            {
                UnityEngine.Object.DestroyImmediate(res.GetComponent<ClassBind>()); //防止重复的ClassBind
            }

            //重新赋值instance的热更脚本
            var clrInstances = res.GetComponents<CrossBindingAdaptorType>(); //clone的
            var clrInstances4Ins = ins.GetComponents<CrossBindingAdaptorType>(); //原来的
            for (int i = 0; i < clrInstances.Length; i++)
            {
                //获取对照适配器
                var clrInstance = clrInstances[i];
                var clrInstance4Ins = clrInstances4Ins[i];

                //重新搞ILInstance
                ILTypeInstance
                    ilInstance =
                        clrInstance4Ins.ILInstance
                            .Clone(); //这里会有个问题，因为是复制的，有的地方可能指向的this，这时复制过去的是老的this，也就是原来的对象的this的东西
                var t = clrInstance4Ins.GetType();

                if (clrInstance4Ins is MonoBehaviourAdapter.Adaptor)
                {
                    ((MonoBehaviourAdapter.Adaptor) clrInstance).Reset(); //重置clone的
                    ((MonoBehaviourAdapter.Adaptor) clrInstance).ILInstance = ilInstance;
                    ((MonoBehaviourAdapter.Adaptor) clrInstance).AppDomain = __domain;
                }
                else
                {
                    var ILInstance = t.GetField("instance",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    var AppDomain = t.GetField("appdomain",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    ILInstance.SetValue(clrInstance, ilInstance);
                    AppDomain.SetValue(clrInstance, __domain);
                }

                if (clrInstance4Ins.ILInstance.CLRInstance == clrInstance4Ins) //clr指向
                {
                    ilInstance.CLRInstance = clrInstance;
                }
                else
                {
                    ilInstance.CLRInstance = ilInstance;
                }

                //补上Awake
                //不管是啥类型，直接invoke这个awake方法
                var awakeMethod = (clrInstance.GetType()!=null?clrInstance.GetType():t).GetMethod("Awake",
                    BindingFlags.Default | BindingFlags.Public
                                         | BindingFlags.Instance | BindingFlags.FlattenHierarchy |
                                         BindingFlags.NonPublic | BindingFlags.Static);
                if (awakeMethod != null)
                {
                    awakeMethod.Invoke(clrInstance, null);
                }
                else
                {
                    Debug.LogError($"{t.FullName}不包含Awake方法，无法激活，已跳过");

                }
            }
        }

        /// <summary>
        /// Instantiate 实现
        /// </summary>
        /// <param name="__intp"></param>
        /// <param name="__esp"></param>
        /// <param name="__mStack"></param>
        /// <param name="__method"></param>
        /// <param name="isNewObj"></param>
        /// <returns></returns>
        /// <exception cref="System.NullReferenceException"></exception>
        unsafe static StackObject* Instantiate(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            //CLR重定向的说明请看相关文档和教程，这里不多做解释
            AppDomain __domain = __intp.AppDomain;

            var ptr = __esp - 1;
            var instance = StackObject.ToObject(ptr, __domain, __mStack);
            if (instance == null)
                throw new NullReferenceException();
            __intp.Free(ptr);

            bool returnScipt = false;
            ILType returnType = null;
            object scriptResult = null;

            GameObject ins = null;
            Transform parent = null;
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            bool gotRot = false;
            bool instantiateInWorldSpace = false;
            bool gotIWS = false;


            //处理参数
            if (instance is Transform) //gameobject, transform
            {
                parent = instance as Transform;
                //获取真正的gameObject
                var ptr2 = __esp - 2;
                var param2 = StackObject.ToObject(ptr2, __domain, __mStack);
                __intp.Free(ptr2);
                if (param2 is GameObject)
                {
                    instance = param2;
                }
                else // gameobject, v3, quaternion, transform
                {
                    gotRot = true;
                    rotation = (Quaternion) param2;
                    var ptr3 = __esp - 3;
                    position = (Vector3) StackObject.ToObject(ptr3, __domain, __mStack);
                    __intp.Free(ptr3);
                }
            }

            if (instance is Quaternion) //gameobject, v3, quaternion
            {
                gotRot = true;
                rotation = (Quaternion) instance;
                //获取v3
                var ptr2 = __esp - 2;
                var param2 = StackObject.ToObject(ptr2, __domain, __mStack);
                position = (Vector3) param2;
                __intp.Free(ptr2);
                //获取真正的gameObject
                var ptr3 = __esp - 3;
                instance = StackObject.ToObject(ptr3, __domain, __mStack);
                __intp.Free(ptr3);
            }

            if (instance is int) //gameobject, transform, bool，ILRuntime把热更的bool变int传了，1是true，0是false
            {
                instantiateInWorldSpace = instance.ToString() == "1";
                // Debug.Log(instantiateInWorldSpace);
                gotIWS = true;
                //获取parent
                var ptr2 = __esp - 2;
                parent = StackObject.ToObject(ptr2, __domain, __mStack) as Transform;
                __intp.Free(ptr2);
                //获取真正的gameObject
                var ptr3 = __esp - 3;
                instance = StackObject.ToObject(ptr3, __domain, __mStack);
                __intp.Free(ptr3);
            }



            //处理对象
            SetGOForInstantiate(instance, out ins, out returnScipt, out returnType);

            //处理clasBind
            var cb = ins.GetComponentInChildren<ClassBind>(true);
            if (cb != null)
            {
                //执行绑定
                ClassBindMgr.DoBind();
            }

            //处理返回对象
            GameObject res = null;
            if (parent == null && !gotRot) //1参数
            {
                res = UnityEngine.Object.Instantiate(ins); //生成
            }
            else if (gotRot && parent == null)
            {
                res = UnityEngine.Object.Instantiate(ins, position, rotation); //生成
            }
            else if (gotRot && parent != null)
            {
                res = UnityEngine.Object.Instantiate(ins, position, rotation, parent); //生成
            }
            else if (gotIWS) //gameobject,transform,bool
            {
                res = UnityEngine.Object.Instantiate(ins, parent, instantiateInWorldSpace); //生成
            }
            else if (parent != null) //gameobject,transform
            {
                res = UnityEngine.Object.Instantiate(ins, parent); //生成
            }

            //如果同时有adaptor和classbind，肯定是复制的，要给删了
            if (res.GetComponentsInChildren<ClassBind>(true).Length > 0)
            {
                if (res.GetComponentsInChildren<CrossBindingAdaptorType>(true).Length == 0)
                {
                    ClassBindMgr.DoBind();
                    
                    if (returnScipt)
                    {
                        //如果返回本地类
                        if (returnType == null)
                        {
                            scriptResult = res.GetComponent(instance.GetType());
                        }

                        return ILIntepreter.PushObject(ptr, __mStack, scriptResult);
                    }

                    return ILIntepreter.PushObject(ptr, __mStack, res);
                }

                UnityEngine.Object.DestroyImmediate(res.GetComponent<ClassBind>()); //防止重复的ClassBind
            }

            //重新赋值instance的热更脚本
            var clrInstances = res.GetComponents<CrossBindingAdaptorType>(); //clone的
            var clrInstances4Ins = ins.GetComponents<CrossBindingAdaptorType>(); //原来的
            for (int i = 0; i < clrInstances.Length; i++)
            {
                //获取对照适配器
                var clrInstance = clrInstances[i];
                var clrInstance4Ins = clrInstances4Ins[i];

                ILTypeInstance
                    ilInstance =
                        clrInstance4Ins.ILInstance
                            .Clone(); //这里会有个问题，因为是复制的，有的地方可能指向的this，这时复制过去的是老的this，也就是原来的对象的this的东西
                var t = clrInstance4Ins.GetType();

                if (clrInstance4Ins is MonoBehaviourAdapter.Adaptor)
                {
                    ((MonoBehaviourAdapter.Adaptor) clrInstance).Reset(); //重置clone的
                    //重新搞ILInstance
                    ((MonoBehaviourAdapter.Adaptor) clrInstance).ILInstance = ilInstance;
                    ((MonoBehaviourAdapter.Adaptor) clrInstance).AppDomain = __domain;
                }
                else
                {
                    var ILInstance = t.GetField("instance",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    var AppDomain = t.GetField("appdomain",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    ILInstance.SetValue(clrInstance, ilInstance);
                    AppDomain.SetValue(clrInstance, __domain);
                }

                if (clrInstance4Ins.ILInstance.CLRInstance == clrInstance4Ins) //clr指向
                {
                    ilInstance.CLRInstance = clrInstance;
                }
                else
                {
                    ilInstance.CLRInstance = ilInstance;
                }

                //补上Awake
                //不管是啥类型，直接invoke这个awake方法
                var awakeMethod = (clrInstance.GetType()!=null?clrInstance.GetType():t).GetMethod("Awake",
                    BindingFlags.Default | BindingFlags.Public
                                         | BindingFlags.Instance | BindingFlags.FlattenHierarchy |
                                         BindingFlags.NonPublic | BindingFlags.Static);
                if (awakeMethod != null)
                {
                    awakeMethod.Invoke(clrInstance, null);
                }
                else
                {
                    Debug.LogError($"{t.FullName}不包含Awake方法，无法激活，已跳过");

                }

                if (ilInstance.Type == returnType && scriptResult == null)
                {
                    scriptResult = ilInstance;
                }
            }

            //处理子物体
            var go = res.GetComponentsInChildren<Transform>(true);
            var go2 = ins.GetComponentsInChildren<Transform>(true);

            if (go.Length != go2.Length)
            {
                Debug.LogError("[Instantiate 错误] 生成实例与原对象脚本数量不匹配");
                return __esp;
            }

            for (int i = 0; i < go.Length; i++)
            {
                if (go[i] == res.transform)
                {
                    continue;
                }

                var subGo = go[i].gameObject;
                CleanGoForInstantiate(ref subGo, go2[i].gameObject, __domain);
            }

            //处理好了后，如果还有classbind就是prefab，需要重新bind
            if (res.GetComponentInChildren<ClassBind>(true))
            {
                ClassBindMgr.DoBind(res.GetComponentsInChildren<ClassBind>(true).ToList());
            }

            if (returnScipt)
            {
                //如果返回本地类
                if (returnType == null)
                {
                    scriptResult = res.GetComponent(instance.GetType());
                }

                return ILIntepreter.PushObject(ptr, __mStack, scriptResult);
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        /// <summary>
        /// Log.PrintError 实现
        /// </summary>
        /// <param name="__intp"></param>
        /// <param name="__esp"></param>
        /// <param name="__mStack"></param>
        /// <param name="__method"></param>
        /// <param name="isNewObj"></param>
        /// <returns></returns>
        unsafe static StackObject* PrintError(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);

            object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var stacktrace = __domain.DebugService.GetStackTrace(__intp);
            Core.Log.PrintError(message + "\n\n==========ILRuntime StackTrace==========\n" + stacktrace);
            return __ret;
        }

        /// <summary>
        /// Log.PrintWarning 实现
        /// </summary>
        /// <param name="__intp"></param>
        /// <param name="__esp"></param>
        /// <param name="__mStack"></param>
        /// <param name="__method"></param>
        /// <param name="isNewObj"></param>
        /// <returns></returns>
        unsafe static StackObject* PrintWarning(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);

            object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var stacktrace = __domain.DebugService.GetStackTrace(__intp);
            Core.Log.PrintWarning(message + "\n\n==========ILRuntime StackTrace==========\n" + stacktrace);
            return __ret;
        }

        /// <summary>
        /// Log.Print 实现
        /// </summary>
        /// <param name="__intp"></param>
        /// <param name="__esp"></param>
        /// <param name="__mStack"></param>
        /// <param name="__method"></param>
        /// <param name="isNewObj"></param>
        /// <returns></returns>
        unsafe static StackObject* Print(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);

            object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var stacktrace = __domain.DebugService.GetStackTrace(__intp);
            Core.Log.Print(message + "\n\n==========ILRuntime StackTrace==========\n" + stacktrace);
            return __ret;
        }

        /// <summary>
        /// Debug.LogError 实现
        /// </summary>
        /// <param name="__intp"></param>
        /// <param name="__esp"></param>
        /// <param name="__mStack"></param>
        /// <param name="__method"></param>
        /// <param name="isNewObj"></param>
        /// <returns></returns>
        unsafe static StackObject* LogError(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);

            object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var stacktrace = __domain.DebugService.GetStackTrace(__intp);
            Debug.LogError(message + "\n\n==========ILRuntime StackTrace==========\n" + stacktrace);
            return __ret;
        }

        /// <summary>
        /// Debug.LogWarning 实现
        /// </summary>
        /// <param name="__intp"></param>
        /// <param name="__esp"></param>
        /// <param name="__mStack"></param>
        /// <param name="__method"></param>
        /// <param name="isNewObj"></param>
        /// <returns></returns>
        unsafe static StackObject* LogWarning(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);

            object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var stacktrace = __domain.DebugService.GetStackTrace(__intp);
            Debug.LogWarning(message + "\n\n==========ILRuntime StackTrace==========\n" + stacktrace);
            return __ret;
        }

        /// <summary>
        /// Debug.Log 实现
        /// </summary>
        /// <param name="__intp"></param>
        /// <param name="__esp"></param>
        /// <param name="__mStack"></param>
        /// <param name="__method"></param>
        /// <param name="isNewObj"></param>
        /// <returns></returns>
        unsafe static StackObject* Log(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);

            object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var stacktrace = __domain.DebugService.GetStackTrace(__intp);
            Debug.Log(message + "\n\n==========ILRuntime StackTrace==========\n" + stacktrace);
            return __ret;
        }

        private static object DoAddComponent(IType type, GameObject instance,AppDomain __domain)
        {
            object res;
            if (type is CLRType)
            {
                //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                res = instance.AddComponent(type.TypeForCLR);
            }
            else
            {
                //热更DLL内的类型比较麻烦。首先我们得自己手动创建实例
                ILTypeInstance ilInstance = new ILTypeInstance(type as ILType, false);
                Type t = type.ReflectionType;
                bool isMonoAdapter = t.BaseType?.FullName == typeof(MonoBehaviourAdapter.Adaptor).FullName;

                if (!isMonoAdapter && Type.GetType(t.BaseType.FullName) != null)
                {
                    Type adapterType = Type.GetType(t.BaseType?.FullName);
                    if (adapterType == null)
                    {
                        Core.Log.PrintError($"{t.FullName}, need to generate adapter");
                        return null;
                    }

                    //直接反射赋值一波了
                    var clrInstance = instance.AddComponent(adapterType);
                    var ILInstance = t.GetField("instance",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    var AppDomain = t.GetField("appdomain",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    ILInstance.SetValue(clrInstance, ilInstance);
                    AppDomain.SetValue(clrInstance, __domain);
                    ilInstance.CLRInstance = clrInstance;
                    bool activated = false;
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
                        activated = true;
                    }

                    if (awakeMethod == null)
                    {
                        Core.Log.PrintError($"{t.FullName}不包含Awake方法，无法激活，已跳过");
                    }
                    else if (!activated)
                    {
                        awakeMethod.Invoke(t, null);
                    }
                }
                else
                {
                    //接下来创建Adapter实例
                    var clrInstance = instance.AddComponent<MonoBehaviourAdapter.Adaptor>();
                    //unity创建的实例并没有热更DLL里面的实例，所以需要手动赋值
                    clrInstance.ILInstance = ilInstance;
                    clrInstance.AppDomain = __domain;
                    //这个实例默认创建的CLRInstance不是通过AddComponent出来的有效实例，所以得手动替换
                    ilInstance.CLRInstance = clrInstance;
                    clrInstance.Awake(); //因为Unity调用这个方法时还没准备好所以这里补调一次
                }

                res = ilInstance;

                var m = type.GetConstructor(Extensions.EmptyParamList);
                if (m != null)
                {
                    __domain.Invoke(m, res, null);
                }
            }
            return res;
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
        unsafe static StackObject* AddComponent2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            //CLR重定向的说明请看相关文档和教程，这里不多做解释
            AppDomain __domain = __intp.AppDomain;

            var  ptr = ILIntepreter.Minus(__esp, 2);
            //成员方法的第一个参数为this
            GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
            if (instance == null)
                throw new NullReferenceException();
            __intp.Free(ptr);
            
            //成员方法的第2个参数为Type
            var ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Type _type = (Type)typeof(Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            if (_type == null)
                throw new NullReferenceException();

            IType type = __domain.LoadedTypes[_type.FullName];
            object res = DoAddComponent(type,instance,__domain);

            return ILIntepreter.PushObject(ptr, __mStack, res);
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
        unsafe static StackObject* AddComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
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
                object res = DoAddComponent(type,instance,__domain);

                return ILIntepreter.PushObject(ptr, __mStack, res);
            }

            return __esp;
        }
        
        
        /// <summary>
        /// Get的字符串参数重定向
        /// </summary>
        /// <param name="__intp"></param>
        /// <param name="__esp"></param>
        /// <param name="__mStack"></param>
        /// <param name="__method"></param>
        /// <param name="isNewObj"></param>
        /// <returns></returns>
        private static unsafe StackObject* GetComponent_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            String type = (String)typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            GameObject instance_of_this_method = (GameObject)typeof(GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            object result_of_this_method = instance_of_this_method.GetComponent(type);//先从本地匹配
            
            if (result_of_this_method == null)//本地没再从热更匹配
            {
                var typeName = __domain.LoadedTypes.Keys.ToList().Find(k => k.EndsWith(type));
                if (typeName != null)//如果有这个热更类型
                {
                    //适配器全查找出来，匹配ILTypeInstance的真实类型的FullName
                    var clrInstances = instance_of_this_method.GetComponents<CrossBindingAdaptorType>();
                    for (int i = 0; i < clrInstances.Length; i++)
                    {
                        var clrInstance = clrInstances[i];
                        if (clrInstance.ILInstance != null) //ILInstance为null, 表示是无效的MonoBehaviour，要略过
                        {
                            if (clrInstance.ILInstance.Type.ReflectionType.FullName == typeName)
                            {
                                result_of_this_method = clrInstance.ILInstance; //交给ILRuntime的实例应该为ILInstance
                                break;
                            }
                        }
                    }
                }
            }

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
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
        unsafe static StackObject* GetComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            //CLR重定向的说明请看相关文档和教程，这里不多做解释
            AppDomain __domain = __intp.AppDomain;

            var ptr = __esp - 1;
            //成员方法的第一个参数为this
            var ins = StackObject.ToObject(ptr, __domain, __mStack);
            if (ins == null)
                throw new NullReferenceException();
            __intp.Free(ptr);

            var genericArgument = __method.GenericArguments;
            //AddComponent应该有且只有1个泛型参数
            if (genericArgument != null && genericArgument.Length == 1)
            {
                GameObject instance;

                if (ins is GameObject)
                {
                    instance = ins as GameObject;
                }
                else if (ins is Component)
                {
                    instance = ((Component) ins).gameObject;
                }
                else if (ins is ILTypeInstance)
                {
                    instance = FindGOFromHotClass(((ILTypeInstance) ins));
                }
                else
                {
                    Debug.LogError($"[GetComponent错误] 不支持的参数类型：{ins.GetType().FullName}，" +
                                   "请传参GameObject或继承MonoBehaviour的对象");
                    return __esp;
                }


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
                    var clrInstances = instance.GetComponents<CrossBindingAdaptorType>();
                    for (int i = 0; i < clrInstances.Length; i++)
                    {
                        var clrInstance = clrInstances[i];
                        if (clrInstance.ILInstance != null) //ILInstance为null, 表示是无效的MonoBehaviour，要略过
                        {
                            if (clrInstance.ILInstance.Type == type ||
                                clrInstance.ILInstance.Type.ReflectionType.IsSubclassOf(type.ReflectionType))
                            {
                                res = clrInstance.ILInstance; //交给ILRuntime的实例应该为ILInstance
                                break;
                            }
                        }
                    }

                    if (res == null) //如果是null，但有classBind，就先绑定
                    {
                        var cb = instance.GetComponent<ClassBind>();
                        if (cb != null)
                        {
                            //执行绑定
                            ClassBindMgr.DoBind();

                            //再次循环
                            clrInstances = instance.GetComponents<CrossBindingAdaptorType>();
                            for (int i = 0; i < clrInstances.Length; i++)
                            {
                                var clrInstance = clrInstances[i];
                                if (clrInstance.ILInstance != null) //ILInstance为null, 表示是无效的MonoBehaviour，要略过
                                {
                                    if (clrInstance.ILInstance.Type == type)
                                    {
                                        res = clrInstance.ILInstance; //交给ILRuntime的实例应该为ILInstance
                                        break;
                                    }
                                }
                            }
                        }

                    }
                }

                return ILIntepreter.PushObject(ptr, __mStack, res);
            }

            return __esp;
        }
    }
}