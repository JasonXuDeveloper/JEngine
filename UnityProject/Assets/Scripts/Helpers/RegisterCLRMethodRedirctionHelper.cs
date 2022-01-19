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
using ILRuntime.Reflection;
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
            var getComponentsMethod = gameObjectType.GetMethods().ToList()
                .Find(i => i.Name == "GetComponents" && i.GetGenericArguments().Length == 1);
            appdomain.RegisterCLRMethodRedirection(getComponentsMethod, GetComponents_7);
            var getComponentsMethod2 = componentType.GetMethods().ToList()
                .Find(i => i.Name == "GetComponents" && i.GetGenericArguments().Length == 1);
            appdomain.RegisterCLRMethodRedirection(getComponentsMethod2, GetComponents_7);

            //get还能是字符串。
            args = new[] {typeof(String)};
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

            //注册Invoke
            Type monoType = typeof(MonoBehaviour);
            args = new[] {typeof(String), typeof(Single)};
            var invokeMethod = monoType.GetMethod("Invoke", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(invokeMethod, Invoke_1);

            //注册InvokeRepeating
            args = new[] {typeof(String), typeof(Single), typeof(Single)};
            var invokeRepeatingMethod = monoType.GetMethod("InvokeRepeating", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(invokeRepeatingMethod, InvokeRepeating_2);

            //注册CancelInvoke
            args = new Type[] { };
            var cancelInvokeMethod = monoType.GetMethod("CancelInvoke", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(cancelInvokeMethod, CancelInvoke_3);
            args = new[] {typeof(String)};
            cancelInvokeMethod = monoType.GetMethod("CancelInvoke", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(cancelInvokeMethod, CancelInvoke_4);

            //注册isInvoking
            args = new Type[] { };
            var isInvokingMethod = monoType.GetMethod("IsInvoking", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(isInvokingMethod, IsInvoking_5);
            args = new[] {typeof(String)};
            isInvokingMethod = monoType.GetMethod("IsInvoking", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(isInvokingMethod, IsInvoking_6);

            //注册pb反序列化
            Type pbSerializeType = typeof(Serializer);
            args = new[] {typeof(Type), typeof(Stream)};
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

            //注册FindObject(s)OfType
            var objectType = typeof(UnityEngine.Object);
            args = new Type[]{typeof(System.Type)};
            var findObjectsOfTypeMethod2 = objectType.GetMethod("FindObjectsOfType", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(findObjectsOfTypeMethod2, FindObjectsOfType_11);
            var findObjectsOfTypeMethod = objectType.GetMethods().ToList().FindAll(f => f.Name == "FindObjectsOfType" && f.IsGenericMethod);
            foreach (var methodInfo in findObjectsOfTypeMethod)
            {
                appdomain.RegisterCLRMethodRedirection(methodInfo, FindObjectsOfType_10);
            }
            
            args = new Type[]{typeof(System.Type)};
            var findObjectOfTypeMethod2 = objectType.GetMethod("FindObjectOfType", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(findObjectOfTypeMethod2, FindObjectOfType_11);
            var findObjectOfTypeMethod = objectType.GetMethods().ToList().FindAll(f => f.Name == "FindObjectOfType" && f.IsGenericMethod);
            foreach (var methodInfo in findObjectOfTypeMethod)
            {
                appdomain.RegisterCLRMethodRedirection(methodInfo, FindObjectOfType_10);
            }
            

            //注册Instantiate和其他的
            var instantiateMethod = objectType.GetMethod("Instantiate", flag, null, args, null);
            var allMethods = objectType.GetMethods().ToList().FindAll(f => f.Name == "Instantiate");
            //GameObject的方便点，不需要再去Get类型
            args = new[] {typeof(GameObject)};
            foreach (var m in allMethods)
            {
                if (m.MatchGenericParameters(args, typeof(GameObject), typeof(GameObject)))
                {
                    instantiateMethod = m.MakeGenericMethod(args);
                    appdomain.RegisterCLRMethodRedirection(instantiateMethod, Instantiate_7);
                }
                else if (m.MatchGenericParameters(args, typeof(GameObject), typeof(GameObject),
                    typeof(Transform)))
                {
                    instantiateMethod = m.MakeGenericMethod(args);
                    appdomain.RegisterCLRMethodRedirection(instantiateMethod, Instantiate_8);
                }
                else if (m.MatchGenericParameters(args, typeof(GameObject), typeof(GameObject),
                    typeof(Transform), typeof(Boolean)))
                {
                    instantiateMethod = m.MakeGenericMethod(args);
                    appdomain.RegisterCLRMethodRedirection(instantiateMethod, Instantiate_9);
                }
                else if (m.MatchGenericParameters(args, typeof(GameObject), typeof(GameObject),
                    typeof(Vector3), typeof(Quaternion)))
                {
                    instantiateMethod = m.MakeGenericMethod(args);
                    appdomain.RegisterCLRMethodRedirection(instantiateMethod, Instantiate_10);
                }
                else if (m.MatchGenericParameters(args, typeof(GameObject), typeof(GameObject),
                    typeof(Vector3), typeof(Quaternion), typeof(Transform)))
                {
                    instantiateMethod = m.MakeGenericMethod(args);
                    appdomain.RegisterCLRMethodRedirection(instantiateMethod, Instantiate_11);
                }
            }

            //其他的需要复杂一点的
            foreach (var m in allMethods)
            {
                var allParams = m.GetParameters();
                if (allParams.Length == 1)
                {
                    appdomain.RegisterCLRMethodRedirection(m, Instantiate_12);
                }
                else if (allParams.Length == 2)
                {
                    appdomain.RegisterCLRMethodRedirection(m, Instantiate_13);
                }
                else if (allParams.Length == 3 &&
                         allParams[1].ParameterType == typeof(Transform) &&
                         allParams[2].ParameterType == typeof(Boolean))
                {
                    appdomain.RegisterCLRMethodRedirection(m, Instantiate_14);
                }
                else if (allParams.Length == 3 &&
                         allParams[1].ParameterType == typeof(Vector3) &&
                         allParams[2].ParameterType == typeof(Quaternion))
                {
                    appdomain.RegisterCLRMethodRedirection(m, Instantiate_15);
                }
                else if (allParams.Length == 4 &&
                         allParams[1].ParameterType == typeof(Vector3) &&
                         allParams[2].ParameterType == typeof(Quaternion) &&
                         allParams[3].ParameterType == typeof(Transform))
                {
                    appdomain.RegisterCLRMethodRedirection(m, Instantiate_16);
                }
                else
                {
                    appdomain.RegisterCLRMethodRedirection(m, Instantiate_17);
                }
            }

            Type type = typeof(Boolean?);
            args = new Type[] { };
            var getValueOrDefaultMethod = type.GetMethod("GetValueOrDefault", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(getValueOrDefaultMethod, GetValueOrDefault_0);
            appdomain.RegisterCLRCreateDefaultInstance(type, () => new Boolean?());
            
            type = typeof(System.UInt32?);
            args = new Type[]{};
            getValueOrDefaultMethod = type.GetMethod("GetValueOrDefault", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection( getValueOrDefaultMethod, GetValueOrDefault_1);
            appdomain.RegisterCLRCreateDefaultInstance(type, () => new System.UInt32?());
            
            type = typeof(System.Nullable<System.UInt64>);
            args = new Type[]{};
            getValueOrDefaultMethod  = type.GetMethod("GetValueOrDefault", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(getValueOrDefaultMethod, GetValueOrDefault_2);
            appdomain.RegisterCLRCreateDefaultInstance(type, () => new System.Nullable<System.UInt64>());

        }
        
        private static unsafe void WriteBackInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, IList<object> __mStack, ref System.Nullable<System.UInt64> instance_of_this_method)
        {
            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.Object:
                    {
                        __mStack[ptr_of_this_method->Value] = instance_of_this_method;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        }
                        else
                        {
                            var t = __domain.GetType(___obj.GetType()) as CLRType;
                            t.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, instance_of_this_method);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {
                            ((ILType)t).StaticInstance[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        }
                        else
                        {
                            ((CLRType)t).SetStaticFieldValue(ptr_of_this_method->ValueLow, instance_of_this_method);
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as System.Nullable<System.UInt64>[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;
            }
        }

        private static unsafe StackObject* GetValueOrDefault_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            System.Nullable<System.UInt64> instance_of_this_method = Convert.ToUInt64(
                typeof(System.Nullable<System.UInt64>).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (Extensions.TypeFlags) 16));
            var result_of_this_method = instance_of_this_method.GetValueOrDefault();

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            WriteBackInstance(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);

            __intp.Free(ptr_of_this_method);
            __ret->ObjectType = ObjectTypes.Long;
            *(ulong*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }
        
        private static unsafe void WriteBackInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, IList<object> __mStack, ref System.UInt32? instance_of_this_method)
        {
            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.Object:
                    {
                        __mStack[ptr_of_this_method->Value] = instance_of_this_method;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        }
                        else
                        {
                            var t = __domain.GetType(___obj.GetType()) as CLRType;
                            t.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, instance_of_this_method);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {
                            ((ILType)t).StaticInstance[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        }
                        else
                        {
                            ((CLRType)t).SetStaticFieldValue(ptr_of_this_method->ValueLow, instance_of_this_method);
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as System.UInt32?[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;
            }
        }

        private static unsafe StackObject* GetValueOrDefault_1(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            System.UInt32? instance_of_this_method =
                Convert.ToUInt32(typeof(System.UInt32?).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (Extensions.TypeFlags) 16));

            var result_of_this_method = instance_of_this_method.GetValueOrDefault();

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            WriteBackInstance(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);

            __intp.Free(ptr_of_this_method);
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = (int) result_of_this_method;
            return __ret + 1;
        }


        private static unsafe StackObject* GetValueOrDefault_0(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            bool? instance_of_this_method =
                Convert.ToBoolean(
                    typeof(bool?).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack)));
            
            var result_of_this_method = instance_of_this_method.GetValueOrDefault();

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            WriteBackInstance(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);

            __intp.Free(ptr_of_this_method);
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        private static unsafe void WriteBackInstance(AppDomain __domain,
            StackObject* ptr_of_this_method, IList<object> __mStack,
            ref Boolean? instance_of_this_method)
        {
            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            switch (ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.Object:
                {
                    __mStack[ptr_of_this_method->Value] = instance_of_this_method;
                }
                    break;
                case ObjectTypes.FieldReference:
                {
                    var ___obj = __mStack[ptr_of_this_method->Value];
                    if (___obj is ILTypeInstance instance)
                    {
                        instance[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    else
                    {
                        var t = __domain.GetType(___obj.GetType()) as CLRType;
                        t.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, instance_of_this_method);
                    }
                }
                    break;
                case ObjectTypes.StaticFieldReference:
                {
                    var t = __domain.GetType(ptr_of_this_method->Value);
                    if (t is ILType type)
                    {
                        type.StaticInstance[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    else
                    {
                        ((CLRType) t).SetStaticFieldValue(ptr_of_this_method->ValueLow, instance_of_this_method);
                    }
                }
                    break;
                case ObjectTypes.ArrayReference:
                {
                    var instance_of_arrayReference =
                        __mStack[ptr_of_this_method->Value] as Boolean?[];
                    instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                }
                    break;
            }
        }

        /// <summary>
        /// 帮助Instantiate找到GameObject
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ins"></param>
        /// <param name="returnScipt"></param>
        /// <param name="returnType"></param>
        private static void SetGOForInstantiate(object instance, out GameObject ins, out ILType returnType)
        {
            returnType = null;
            //判断类
            if (instance is GameObject gameObject)
            {
                ins = gameObject;
            }
            else if (instance is ILTypeInstance typeInstance) //如果是热更类需要处理
            {
                returnType = typeInstance.Type;
                ins = Tools.FindGOForHotClass(typeInstance);
            }
            else //如果本地类那简单
            {
                ins = (instance as Component)?.gameObject;
            }
        }


        private static object DoInstantiate(GameObject ins, GameObject res, AppDomain domain, IType type = null)
        {
            //没adapter不需要注意什么
            if (res.GetComponentsInChildren<CrossBindingAdaptorType>(true).Length == 0)
            {
                if (res.GetComponentsInChildren<ClassBind>(true).Length > 0)
                {
                    ClassBindMgr.DoBind(res.GetComponentsInChildren<ClassBind>(true).ToList());
                }

                return res;
            }

            //如果同时有adaptor和classbind，肯定是复制的，要给删了
            foreach (var t in res.GetComponentsInChildren<Transform>(true))
            {
                var go = t.gameObject;
                var cb = go.GetComponent<ClassBind>();
                if (cb != null && go.GetComponent<CrossBindingAdaptorType>() != null)
                {
                    UnityEngine.Object.DestroyImmediate(cb); //防止重复的ClassBind
                }
            }

            //重新赋值instance的热更脚本
            var clrInstances = res.GetComponentsInChildren<CrossBindingAdaptorType>(true); //clone的
            var clrInstances4Ins = ins.GetComponentsInChildren<CrossBindingAdaptorType>(true); //原来的
            ILTypeInstance result = null;
            for (int i = 0; i < clrInstances.Length; i++)
            {
                //获取对照适配器
                var clrInstance = clrInstances[i];
                var clrInstance4Ins = clrInstances4Ins[i];

                Core.Log.PrintWarning(
                    "因为复制了一个MonoBehaviour，会有一个可以忽略的警告：You are trying to create a MonoBehaviour using the 'new' keyword.  This is not allowed.  MonoBehaviours can only be added using AddComponent(). Alternatively, your script can inherit from ScriptableObject or no base class at all");
                ILTypeInstance ilInstance =
                    clrInstance4Ins.ILInstance
                        .Clone(); //这里会有个问题，因为是复制的，有的地方可能指向的this，这时复制过去的是老的this，也就是原来的对象的this的东西
                var t = clrInstance4Ins.GetType();
                if (ilInstance.Type == type && result == null)
                {
                    result = ilInstance;
                }

                if (clrInstance4Ins is MonoBehaviourAdapter.Adaptor)
                {
                    var self = ((MonoBehaviourAdapter.Adaptor) clrInstance);
                    self.Reset(); //重置clone的

                    if (domain.LoadedTypes.TryGetValue("JEngine.Core.JBehaviour",
                        out var jBehaviourType))
                    {
                        bool isJBehaviour = t.IsSubclassOf(jBehaviourType.ReflectionType);
                        if (isJBehaviour)
                        {
                            var go = self.gameObject;
                            jBehaviourType.ReflectionType.GetMethod("ResetJBehaviour",
                                BindingFlags.Default | BindingFlags.NonPublic)?.Invoke(ilInstance, new object[] {go});
                        }
                    }

                    //重新搞ILInstance
                    self.ILInstance = ilInstance;
                    self.AppDomain = domain;
                }
                else
                {
                    var clrILInstance = t.GetFields(
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                        .First(f => f.Name == "instance" && f.FieldType == typeof(ILTypeInstance));
                    var clrAppDomain = t.GetFields(
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                        .First(f => f.Name == "appdomain" && f.FieldType == typeof(AppDomain));
                    if (!(clrInstance is null))
                    {
                        clrILInstance.SetValue(clrInstance, ilInstance);
                        clrAppDomain.SetValue(clrInstance, domain);
                    }
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
                var awakeMethod = clrInstance?.GetType().GetMethod("Awake",
                    BindingFlags.Default | BindingFlags.Public
                                         | BindingFlags.Instance | BindingFlags.FlattenHierarchy |
                                         BindingFlags.NonPublic | BindingFlags.Static) ?? t.GetMethod("Awake",
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

            if (type != null)
            {
                return result;
            }

            return res;

        }

        private static unsafe StackObject* Instantiate_7(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            GameObject original =
                (GameObject) typeof(GameObject).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = UnityEngine.Object.Instantiate(original);
            return ILIntepreter.PushObject(__ret, __mStack, DoInstantiate(original, result_of_this_method, __domain));
        }

        private static unsafe StackObject* Instantiate_8(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Transform parent =
                (Transform) typeof(Transform).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            GameObject original =
                (GameObject) typeof(GameObject).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = UnityEngine.Object.Instantiate(original, parent);

            return ILIntepreter.PushObject(__ret, __mStack, DoInstantiate(original, result_of_this_method, __domain));
        }

        private static unsafe StackObject* Instantiate_9(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Boolean worldPositionStays = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Transform parent =
                (Transform) typeof(Transform).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            GameObject original =
                (GameObject) typeof(GameObject).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method =
                UnityEngine.Object.Instantiate(original, parent, worldPositionStays);

            return ILIntepreter.PushObject(__ret, __mStack, DoInstantiate(original, result_of_this_method, __domain));
        }

        private static unsafe StackObject* Instantiate_10(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Quaternion rotation =
                (Quaternion) typeof(Quaternion).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Vector3 position =
                (Vector3) typeof(Vector3).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            GameObject original =
                (GameObject) typeof(GameObject).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method =
                UnityEngine.Object.Instantiate(original, position, rotation);

            return ILIntepreter.PushObject(__ret, __mStack, DoInstantiate(original, result_of_this_method, __domain));
        }

        private static unsafe StackObject* Instantiate_11(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Transform parent =
                (Transform) typeof(Transform).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Quaternion rotation =
                (Quaternion) typeof(Quaternion).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            Vector3 position =
                (Vector3) typeof(Vector3).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            GameObject original =
                (GameObject) typeof(GameObject).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method =
                UnityEngine.Object.Instantiate(original, position, rotation, parent);
            DoInstantiate(original, result_of_this_method, __domain);

            return ILIntepreter.PushObject(__ret, __mStack, DoInstantiate(original, result_of_this_method, __domain));
        }

        private static unsafe StackObject* Instantiate_12(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);

            object original = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            __intp.Free(ptr_of_this_method);

            SetGOForInstantiate(original, out var go, out var type);
            var result_of_this_method = UnityEngine.Object.Instantiate(go);
            object res = DoInstantiate(go, result_of_this_method, __domain, type);
            if (type == null && res is GameObject gameObject && gameObject.GetType() != original.GetType())
            {
                res = gameObject.GetComponent(original.GetType());
            }

            return ILIntepreter.PushObject(__ret, __mStack, res);
        }

        private static unsafe StackObject* Instantiate_13(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Transform parent =
                (Transform) typeof(Transform).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            GameObject original =
                (GameObject) typeof(GameObject).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            SetGOForInstantiate(original, out var go, out var type);
            var result_of_this_method = UnityEngine.Object.Instantiate(go, parent);
            object res = DoInstantiate(go, result_of_this_method, __domain, type);
            if (type == null && res is GameObject gameObject && gameObject.GetType() != original.GetType())
            {
                res = gameObject.GetComponent(original.GetType());
            }

            return ILIntepreter.PushObject(__ret, __mStack, res);
        }

        private static unsafe StackObject* Instantiate_14(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Boolean worldPositionStays = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Transform parent =
                (Transform) typeof(Transform).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            GameObject original =
                (GameObject) typeof(GameObject).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            SetGOForInstantiate(original, out var go, out var type);
            var result_of_this_method = UnityEngine.Object.Instantiate(go, parent, worldPositionStays);
            object res = DoInstantiate(go, result_of_this_method, __domain, type);
            if (type == null && res is GameObject gameObject && gameObject.GetType() != original.GetType())
            {
                res = gameObject.GetComponent(original.GetType());
            }

            return ILIntepreter.PushObject(__ret, __mStack, res);
        }

        private static unsafe StackObject* Instantiate_15(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Quaternion rotation =
                (Quaternion) typeof(Quaternion).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Vector3 position =
                (Vector3) typeof(Vector3).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            GameObject original =
                (GameObject) typeof(GameObject).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            SetGOForInstantiate(original, out var go, out var type);
            var result_of_this_method = UnityEngine.Object.Instantiate(go, position, rotation);
            object res = DoInstantiate(go, result_of_this_method, __domain, type);
            if (type == null && res is GameObject gameObject && gameObject.GetType() != original.GetType())
            {
                res = gameObject.GetComponent(original.GetType());
            }

            return ILIntepreter.PushObject(__ret, __mStack, res);
        }

        private static unsafe StackObject* Instantiate_16(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Transform parent =
                (Transform) typeof(Transform).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Quaternion rotation =
                (Quaternion) typeof(Quaternion).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            Vector3 position =
                (Vector3) typeof(Vector3).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            GameObject original =
                (GameObject) typeof(GameObject).CheckCLRTypes(
                    StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            SetGOForInstantiate(original, out var go, out var type);
            var result_of_this_method = UnityEngine.Object.Instantiate(go, position, rotation, parent);
            object res = DoInstantiate(go, result_of_this_method, __domain, type);
            if (type == null && res is GameObject gameObject && gameObject.GetType() != original.GetType())
            {
                res = gameObject.GetComponent(original.GetType());
            }

            return ILIntepreter.PushObject(__ret, __mStack, res);
        }

        private static unsafe StackObject* Instantiate_17(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            throw new NotSupportedException("JEngine不支持这种Instantiate");
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

        /// <summary>
        /// 重定向CancelInvoke
        /// </summary>
        /// <param name="__intp"></param>
        /// <param name="__esp"></param>
        /// <param name="__mStack"></param>
        /// <param name="__method"></param>
        /// <param name="isNewObj"></param>
        /// <returns></returns>
        private static unsafe StackObject* CancelInvoke_3(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILTypeInstance val = (ILTypeInstance) (StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            MonoBehaviour adapter = (MonoBehaviour) typeof(MonoBehaviour).CheckCLRTypes(val);
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
        private static unsafe StackObject* CancelInvoke_4(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILTypeInstance val = (ILTypeInstance) (StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            MonoBehaviour adapter = (MonoBehaviour) typeof(MonoBehaviour).CheckCLRTypes(val);
            __intp.Free(ptr_of_this_method);

            //垃圾Unity，Invoke热更内的方法，目前得用本地的接口执行
            Type type = val.Type.ReflectionType;
            //系统反射接口
            MethodInfo mi = type.GetMethod(methodName,
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
                | BindingFlags.Instance
                | BindingFlags.FlattenHierarchy
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
        private static unsafe StackObject* IsInvoking_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILTypeInstance val = (ILTypeInstance) (StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            MonoBehaviour adapter = (MonoBehaviour) typeof(MonoBehaviour).CheckCLRTypes(val);
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
        private static unsafe StackObject* IsInvoking_6(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILTypeInstance val = (ILTypeInstance) (StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            MonoBehaviour adapter = (MonoBehaviour) typeof(MonoBehaviour).CheckCLRTypes(val);
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
                    | BindingFlags.FlattenHierarchy
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
        private static unsafe StackObject* InvokeRepeating_2(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Single repeatRate = *(float*) &ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Single time = *(float*) &ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            String methodName =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            ILTypeInstance val = (ILTypeInstance) (StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            MonoBehaviour adapter = (MonoBehaviour) typeof(MonoBehaviour).CheckCLRTypes(val);
            __intp.Free(ptr_of_this_method);

            //垃圾Unity，Invoke热更内的方法，目前得用本地的接口执行
            Type type = val.Type.ReflectionType;
            //系统反射接口
            MethodInfo mi = type.GetMethod(methodName,
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
                | BindingFlags.Instance
                | BindingFlags.FlattenHierarchy
            );

            if (mi == null || mi.GetParameters().Length != 0)
            {
                adapter.InvokeRepeating(methodName, time, repeatRate);
                return __ret;
            }

            _invokeRepeatingTokens[mi] = new CancellationTokenSource();

            DoInvokeRepeating(val, mi, time, repeatRate, adapter.gameObject);

            return __ret;
        }

        private static Dictionary<MethodInfo, CancellationTokenSource> _invokeRepeatingTokens =
            new Dictionary<MethodInfo, CancellationTokenSource>(0);

        private static Dictionary<MethodInfo, CancellationTokenSource> _invokeTokens =
            new Dictionary<MethodInfo, CancellationTokenSource>(0);

        private static async void DoInvokeRepeating<T>(T val, MethodInfo methodInfo, float time, float duration,
            GameObject go)
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
                catch (TaskCanceledException)
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
            MonoBehaviour adapter = (MonoBehaviour) typeof(MonoBehaviour).CheckCLRTypes(val);
            __intp.Free(ptr_of_this_method);

            //垃圾Unity，Invoke热更内的方法，目前得用本地的接口执行
            Type type = val.Type.ReflectionType;
            //系统反射接口
            MethodInfo mi = type.GetMethod(methodName,
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
                | BindingFlags.Instance
                | BindingFlags.FlattenHierarchy
            );

            if (mi == null || mi.GetParameters().Length != 0)
            {
                adapter.Invoke(methodName, time);
                return __ret;
            }

            _invokeTokens[mi] = new CancellationTokenSource();
            DoInvoke(val, mi, time, adapter.gameObject);

            return __ret;
        }

        private static async void DoInvoke<T>(T val, MethodInfo methodInfo, float time, GameObject go)
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
            catch (MissingReferenceException) //MissingReference是GO销毁
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
        private static async Task Wait(float time, CancellationToken token, GameObject go)
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
                    else //不满足条件就结束了
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
        /// 处理热更的SendMessage
        /// </summary>
        /// <param name="go"></param>
        /// <param name="methodName"></param>
        /// <param name="value"></param>
        /// <param name="option"></param>
        private static void DoSendMessageOnHotCode(AppDomain __domain, GameObject go, string methodName,
            object value = null,
            SendMessageOptions option = SendMessageOptions.RequireReceiver)
        {
            //这个option控制了找不到方法是否要报错，但是报错看着太头大，所以我这边就不处理了，留着这个接口

            if (!go.activeSelf) return; //不sendmessage隐藏的

            bool invoked = false;
            var clrInstances = go.GetComponents<CrossBindingAdaptorType>();
            foreach (var clrInstance in clrInstances)
            {
                if (clrInstance.ILInstance == null) continue;
                IType t = clrInstance.ILInstance.Type;
                if (value != null) //有参数就匹配去调用
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

        static unsafe StackObject* BroadcastMessage_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
            else if (instance is ILTypeInstance typeInstance)
            {
                instance_of_this_method = Tools.FindGOForHotClass(typeInstance);
            }
            else if (instance is Component component)
            {
                instance_of_this_method = component.gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"BroadcastMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain, instance_of_this_method, methodName);
            instance_of_this_method.BroadcastMessage(methodName);

            var go = instance_of_this_method.GetComponentsInChildren<Transform>(true);

            foreach (var g in go)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain, g.gameObject, methodName);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            return __ret;
        }

        static unsafe StackObject* BroadcastMessage_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
            else if (instance is ILTypeInstance typeInstance)
            {
                instance_of_this_method = Tools.FindGOForHotClass(typeInstance);
            }
            else if (instance is Component component)
            {
                instance_of_this_method = component.gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"BroadcastMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain, instance_of_this_method, methodName, value);
            instance_of_this_method.BroadcastMessage(methodName, value);

            var go = instance_of_this_method.GetComponentsInChildren<Transform>(true);

            foreach (var g in go)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain, g.gameObject, methodName, value);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            return __ret;
        }

        static unsafe StackObject* BroadcastMessage_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
            else if (instance is ILTypeInstance typeInstance)
            {
                instance_of_this_method = Tools.FindGOForHotClass(typeInstance);
            }
            else if (instance is Component component)
            {
                instance_of_this_method = component.gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"BroadcastMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain, instance_of_this_method, methodName, null, options);
            instance_of_this_method.BroadcastMessage(methodName, options);

            var go = instance_of_this_method.GetComponentsInChildren<Transform>(true);

            foreach (var g in go)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain, g.gameObject, methodName, null, options);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            return __ret;
        }

        static unsafe StackObject* BroadcastMessage_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
            else if (instance is ILTypeInstance typeInstance)
            {
                instance_of_this_method = Tools.FindGOForHotClass(typeInstance);
            }
            else if (instance is Component component)
            {
                instance_of_this_method = component.gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"BroadcastMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain, instance_of_this_method, methodName, value, options);
            instance_of_this_method.BroadcastMessage(methodName, value, options);

            var go = instance_of_this_method.GetComponentsInChildren<Transform>(true);

            foreach (var g in go)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain, g.gameObject, methodName, value, options);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            return __ret;
        }

        static unsafe StackObject* SendMessageUpwards_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
            else if (instance is ILTypeInstance typeInstance)
            {
                instance_of_this_method = Tools.FindGOForHotClass(typeInstance);
            }
            else if (instance is Component component)
            {
                instance_of_this_method = component.gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessageUpwards方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain, instance_of_this_method, methodName);
            instance_of_this_method.SendMessageUpwards(methodName);

            var go = instance_of_this_method.transform.parent.gameObject;
            while (go != null)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain, go, methodName);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                go = go.transform.parent.gameObject;
            }


            return __ret;
        }

        static unsafe StackObject* SendMessageUpwards_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
            else if (instance is ILTypeInstance typeInstance)
            {
                instance_of_this_method = Tools.FindGOForHotClass(typeInstance);
            }
            else if (instance is Component component)
            {
                instance_of_this_method = component.gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessageUpwards方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain, instance_of_this_method, methodName, value);
            instance_of_this_method.SendMessageUpwards(methodName, value);

            var go = instance_of_this_method.transform.parent.gameObject;
            while (go != null)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain, go, methodName, value);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                go = go.transform.parent.gameObject;
            }

            return __ret;
        }

        static unsafe StackObject* SendMessageUpwards_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
            else if (instance is ILTypeInstance typeInstance)
            {
                instance_of_this_method = Tools.FindGOForHotClass(typeInstance);
            }
            else if (instance is Component component)
            {
                instance_of_this_method = component.gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessageUpwards方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain, instance_of_this_method, methodName, null, options);
            instance_of_this_method.SendMessageUpwards(methodName, options);

            var go = instance_of_this_method.transform.parent.gameObject;
            while (go != null)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain, go, methodName, null, options);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                go = go.transform.parent.gameObject;
            }


            return __ret;
        }

        static unsafe StackObject* SendMessageUpwards_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
            else if (instance is ILTypeInstance typeInstance)
            {
                instance_of_this_method = Tools.FindGOForHotClass(typeInstance);
            }
            else if (instance is Component component)
            {
                instance_of_this_method = component.gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessageUpwards方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain, instance_of_this_method, methodName, value, options);
            instance_of_this_method.SendMessageUpwards(methodName, value, options);

            var go = instance_of_this_method.transform.parent.gameObject;
            while (go != null)
            {
                try
                {
                    DoSendMessageOnHotCode(__domain, go, methodName, value, options);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                go = go.transform.parent.gameObject;
            }

            return __ret;
        }

        static unsafe StackObject* SendMessage_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
            else if (instance is ILTypeInstance typeInstance)
            {
                instance_of_this_method = Tools.FindGOForHotClass(typeInstance);
            }
            else if (instance is Component component)
            {
                instance_of_this_method = component.gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain, instance_of_this_method, methodName);
            instance_of_this_method.SendMessage(methodName);

            return __ret;
        }

        static unsafe StackObject* SendMessage_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
            else if (instance is ILTypeInstance ilTypeInstance)
            {
                instance_of_this_method = Tools.FindGOForHotClass(ilTypeInstance);
            }
            else if (instance is Component component)
            {
                instance_of_this_method = component.gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain, instance_of_this_method, methodName, value);
            instance_of_this_method.SendMessage(methodName, value);

            return __ret;
        }

        static unsafe StackObject* SendMessage_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
            else if (instance is ILTypeInstance typeInstance)
            {
                instance_of_this_method = Tools.FindGOForHotClass(typeInstance);
            }
            else if (instance is Component component)
            {
                instance_of_this_method = component.gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain, instance_of_this_method, methodName, null, options);
            instance_of_this_method.SendMessage(methodName, options);

            return __ret;
        }

        static unsafe StackObject* SendMessage_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
            else if (instance is ILTypeInstance typeInstance)
            {
                instance_of_this_method = Tools.FindGOForHotClass(typeInstance);
            }
            else if (instance is Component component)
            {
                instance_of_this_method = component.gameObject;
            }
            else
            {
                return __esp;
            }

            __intp.Free(ptr_of_this_method);

            Debug.LogWarning($"SendMessage方法被重定向了，会尝试调用热更+本地脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(__domain, instance_of_this_method, methodName, value, options);
            instance_of_this_method.SendMessage(methodName, value, options);

            return __ret;
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
        static unsafe StackObject* PrintError(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
        static unsafe StackObject* PrintWarning(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
        static unsafe StackObject* Print(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
        static unsafe StackObject* LogError(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
        static unsafe StackObject* LogWarning(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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
        static unsafe StackObject* Log(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
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

        private static object DoAddComponent(IType type, GameObject instance, AppDomain domain)
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
                if (!InitJEngine.Appdomain.LoadedTypes.TryGetValue(type.FullName, out type))
                {
                    throw new KeyNotFoundException();
                }
                ILTypeInstance ilInstance = new ILTypeInstance(type as ILType, false);
                Type t = type.ReflectionType;
                Type baseType =
                    t.BaseType is ILRuntimeWrapperType wrapperType
                        ? wrapperType.RealType
                        : t.BaseType; //这个地方太坑了 你一旦热更工程代码写的骚 就会导致ILWrapperType这个问题出现 一般人还真不容易发现这个坑
                bool needAdapter = baseType != null &&
                                   baseType.GetInterfaces().Contains(typeof(CrossBindingAdaptorType));

                if (needAdapter && baseType != typeof(MonoBehaviourAdapter.Adaptor))
                {
                    Type adapterType = Type.GetType(t.BaseType?.FullName ?? string.Empty);
                    if (adapterType == null)
                    {
                        Core.Log.PrintError($"{t.FullName}, need to generate adapter");
                        return null;
                    }

                    //直接反射赋值一波了
                    var clrInstance = instance.AddComponent(adapterType);
                    var ilInsInfo = t.GetFields(
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                        .First(f => f.Name == "instance" && f.FieldType == typeof(ILTypeInstance));
                    var appDInfo = t.GetFields(
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                        .First(f => f.Name == "appdomain" && f.FieldType == typeof(AppDomain));
                    ilInsInfo.SetValue(clrInstance, ilInstance);
                    appDInfo.SetValue(clrInstance, domain);
                    ilInstance.CLRInstance = clrInstance;

                    var m = type.GetConstructor(Extensions.EmptyParamList);
                    if (m != null)
                    {
                        InitJEngine.Appdomain.Invoke(m, ilInstance, null);
                    }
                    
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
                    clrInstance.AppDomain = domain;
                    //这个实例默认创建的CLRInstance不是通过AddComponent出来的有效实例，所以得手动替换
                    ilInstance.CLRInstance = clrInstance;

                    var m = type.GetConstructor(Extensions.EmptyParamList);
                    if (m != null)
                    {
                        InitJEngine.Appdomain.Invoke(m, ilInstance, null);
                    }
                    
                    clrInstance.Awake(); //因为Unity调用这个方法时还没准备好所以这里补调一次
                }

                res = ilInstance;
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
        static unsafe StackObject* AddComponent2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            //CLR重定向的说明请看相关文档和教程，这里不多做解释
            AppDomain __domain = __intp.AppDomain;

            var ptr = ILIntepreter.Minus(__esp, 2);
            //成员方法的第一个参数为this
            GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
            if (instance == null)
                throw new NullReferenceException();
            __intp.Free(ptr);

            //成员方法的第2个参数为Type
            var ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Type _type =
                (Type) typeof(Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            if (_type == null)
                throw new NullReferenceException();

            IType type = __domain.LoadedTypes[_type.FullName];
            object res = DoAddComponent(type, instance, __domain);

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
        static unsafe StackObject* AddComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;

            var ptr = __esp - 1;
            GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
            if (instance == null)
                throw new NullReferenceException();
            __intp.Free(ptr);

            var genericArgument = __method.GenericArguments;
            if (genericArgument != null && genericArgument.Length == 1)
            {
                var type = genericArgument[0];
                object res = DoAddComponent(type, instance, __domain);

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
        private static unsafe StackObject* GetComponent_1(ILIntepreter __intp, StackObject* __esp,
            IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            String type =
                (String) typeof(String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            GameObject instance_of_this_method =
                (GameObject) typeof(GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain,
                    __mStack));
            __intp.Free(ptr_of_this_method);

            object result_of_this_method = instance_of_this_method.GetComponent(type); //先从本地匹配

            if (result_of_this_method == null) //本地没再从热更匹配
            {
                var typeName = __domain.LoadedTypes.Keys.ToList().Find(k => k.EndsWith(type));
                if (typeName != null) //如果有这个热更类型
                {
                    var cs =  Tools.GetHotComponent(instance_of_this_method, type);
                    result_of_this_method = cs != null && ((ILTypeInstance[]) cs).Length > 0 ? ((ILTypeInstance[]) cs)[0] : null;
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
        static unsafe StackObject* GetComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;

            var ptr = __esp - 1;
            var ins = StackObject.ToObject(ptr, __domain, __mStack);
            if (ins == null)
                throw new NullReferenceException();
            __intp.Free(ptr);

            var genericArgument = __method.GenericArguments;
            if (genericArgument != null && genericArgument.Length == 1)
            {
                var type = genericArgument[0];
                object res;
                GameObject instance = Tools.GetGameObject(ins);

                if (type is CLRType)
                {
                    res = instance != null ? instance.GetComponent(type.TypeForCLR) : null;
                }
                else
                {
                    var cs = Tools.GetHotComponent(instance, type as ILType);
                    res = cs != null && ((ILTypeInstance[]) cs).Length > 0 ? ((ILTypeInstance[]) cs)[0] : null;
                }

                return ILIntepreter.PushObject(ptr, __mStack, res);
            }

            return __esp;
        }


        static unsafe StackObject* GetComponents_7(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var ins = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (ins == null)
                throw new NullReferenceException();
            __intp.Free(ptr_of_this_method);

            var genericArgument = __method.GenericArguments;
            if (genericArgument != null && genericArgument.Length == 1)
            {
                var type = genericArgument[0];
                object res;
                GameObject instance = Tools.GetGameObject(ins);

                if (type is CLRType)
                {
                    //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                    var result = instance != null ? instance.GetComponents(type.TypeForCLR) : null;
                    res = result;
                    if (result != null)
                    {
                        int n = result.Length;
                        res = Array.CreateInstance(type.TypeForCLR, n);
                        for (int i = 0; i < n; i++)
                            ((Array) res).SetValue(result[i], i);
                    }
                }
                else
                {
                    var ilInstances = ((ILTypeInstance[])Tools.GetHotComponent(
                            instance,
                            type as ILType))
                        .Select(i => i.CLRInstance).ToArray();
                    int n = ilInstances.Length;
                    res = Array.CreateInstance(type.TypeForCLR, n);
                    for (int i = 0; i < n; i++)
                        ((Array) res).SetValue(ilInstances[i], i);
                }

                return ILIntepreter.PushObject(ptr_of_this_method, __mStack, res);
            }

            return __esp;
        }

        static unsafe StackObject* FindObjectsOfType_10(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var genericArgument = __method.GenericArguments;
            //FindObjectsOfType应该有且只有1个泛型参数
            if (genericArgument != null && genericArgument.Length == 1)
            {
                var type = genericArgument[0];
                object res;

                if (type is CLRType)
                {
                    res = UnityEngine.Object.FindObjectsOfType(type.TypeForCLR);
                }
                else
                {
                    var adapters = Tools.GetAllMonoAdapters();
                    var ilInstances = ((ILTypeInstance[]) Tools.GetHotComponent(adapters, type as ILType))
                        .Select(i => i.CLRInstance).ToArray();
                    int n = ilInstances.Length;
                    res = Array.CreateInstance(type.TypeForCLR, n);
                    for (int i = 0; i < n; i++)
                        ((Array) res).SetValue(ilInstances[i], i);
                }

                return ILIntepreter.PushObject(__ret, __mStack, res);
            }

            return __esp;

        }
        
        static unsafe StackObject* FindObjectsOfType_11(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);
            
            StackObject* ptr_of_this_method;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Type @type = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), 0);
            __intp.Free(ptr_of_this_method);

            object res;
            if (type is ILRuntimeType ilType)
            {
                var adapters = Tools.GetAllMonoAdapters();
                var ilInstances = ((ILTypeInstance[]) Tools.GetHotComponent(adapters, ilType.ILType))
                    .Select(i => i.CLRInstance).ToArray();
                int n = ilInstances.Length;
                res = Array.CreateInstance(ilType.ILType.TypeForCLR, n);
                for (int i = 0; i < n; i++)
                    ((Array) res).SetValue(ilInstances[i], i);
            }
            else
            {
                res = UnityEngine.Object.FindObjectsOfType(type);
            }

            return ILIntepreter.PushObject(__ret, __mStack, res);

        }
        
        static unsafe StackObject* FindObjectOfType_10(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var genericArgument = __method.GenericArguments;
            //FindObjectsOfType应该有且只有1个泛型参数
            if (genericArgument != null && genericArgument.Length == 1)
            {
                var type = genericArgument[0];
                object res;

                if (type is CLRType)
                {
                    res = UnityEngine.Object.FindObjectOfType(type.TypeForCLR);
                }
                else
                {
                    var adapters = Tools.GetAllMonoAdapters();
                    var ilInstances = ((ILTypeInstance[]) Tools.GetHotComponent(adapters, type as ILType));
                    res = ilInstances.Length > 0 ? ilInstances[0] : null;
                }

                return ILIntepreter.PushObject(__ret, __mStack, res);
            }

            return __esp;

        }
        
        static unsafe StackObject* FindObjectOfType_11(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
            CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);
            
            StackObject* ptr_of_this_method;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Type @type = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), 0);
            __intp.Free(ptr_of_this_method);

            object res;
            if (type is ILRuntimeType ilType)
            {
                var adapters = Tools.GetAllMonoAdapters();
                var ilInstances = ((ILTypeInstance[]) Tools.GetHotComponent(adapters, ilType.ILType));
                res = ilInstances.Length > 0 ? ilInstances[0] : null;
            }
            else
            {
                res = UnityEngine.Object.FindObjectOfType(type);
            }

            return ILIntepreter.PushObject(__ret, __mStack, res);

        }
    }
}