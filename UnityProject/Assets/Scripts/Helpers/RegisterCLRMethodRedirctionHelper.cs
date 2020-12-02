using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using JEngine.Core;
using JEngine.Interface;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using Object = UnityEngine.Object;

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
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            
            //注册Add Component
            Type gameObjectType = typeof(GameObject);
            Type componentType = typeof(Component);
            var addComponentMethod = gameObjectType.GetMethods().ToList()
                .Find(i => i.Name == "AddComponent" && i.GetGenericArguments().Length == 1);
            appdomain.RegisterCLRMethodRedirection(addComponentMethod, AddComponent);

            //注册get，有2种get component，一个是GameObject调用，一个是脚本调用
            var getComponentMethod = gameObjectType.GetMethods().ToList()
                .Find(i => i.Name == "GetComponent" && i.GetGenericArguments().Length == 1);
            appdomain.RegisterCLRMethodRedirection(getComponentMethod, GetComponent);
            var getComponentMethod2 = componentType.GetMethods().ToList()
                .Find(i => i.Name == "GetComponent" && i.GetGenericArguments().Length == 1);
            appdomain.RegisterCLRMethodRedirection(getComponentMethod2, GetComponent);
            
            //注册send message
            args = new Type[]{typeof(System.String)};
            var SendMessageMethod_1 = gameObjectType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageMethod_1, SendMessage_1);
            args = new Type[]{typeof(System.String), typeof(System.Object)};
            var SendMessageMethod_2 = gameObjectType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageMethod_2, SendMessage_2);
            args = new Type[]{typeof(System.String), typeof(UnityEngine.SendMessageOptions)};
            var SendMessageMethod_3 = gameObjectType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageMethod_3, SendMessage_3);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(UnityEngine.SendMessageOptions)};
            var SendMessageMethod_4 = gameObjectType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageMethod_4, SendMessage_4);
            
            //注册send message upwards
            args = new Type[]{typeof(System.String)};
            var SendMessageUpwardsMethod_1 = gameObjectType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageUpwardsMethod_1, SendMessageUpwards_1);
            args = new Type[]{typeof(System.String), typeof(System.Object)};
            var SendMessageUpwardsMethod_2 = gameObjectType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageUpwardsMethod_2, SendMessageUpwards_2);
            args = new Type[]{typeof(System.String), typeof(UnityEngine.SendMessageOptions)};
            var SendMessageUpwardsMethod_3 = gameObjectType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageUpwardsMethod_3, SendMessageUpwards_3);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(UnityEngine.SendMessageOptions)};
            var SendMessageUpwardsMethod_4 = gameObjectType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageUpwardsMethod_4, SendMessageUpwards_4);
            
            //注册BroadcastMessage
            args = new Type[]{typeof(System.String)};
            var BroadcastMessageMethod_1 = gameObjectType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(BroadcastMessageMethod_1, BroadcastMessage_1);
            args = new Type[]{typeof(System.String), typeof(System.Object)};
            var BroadcastMessageMethod_2 = gameObjectType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(BroadcastMessageMethod_2, BroadcastMessage_2);
            args = new Type[]{typeof(System.String), typeof(UnityEngine.SendMessageOptions)};
            var BroadcastMessageMethod_3 = gameObjectType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(BroadcastMessageMethod_3, BroadcastMessage_3);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(UnityEngine.SendMessageOptions)};
            var BroadcastMessageMethod_4 = gameObjectType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(BroadcastMessageMethod_4, BroadcastMessage_4);

            //注册send message 4 component
            args = new Type[]{typeof(System.String)};
            var SendMessageMethod4ComponentType_1 = componentType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageMethod4ComponentType_1, SendMessage_1);
            args = new Type[]{typeof(System.String), typeof(System.Object)};
            var SendMessageMethod4ComponentType_2 = componentType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageMethod4ComponentType_2, SendMessage_2);
            args = new Type[]{typeof(System.String), typeof(UnityEngine.SendMessageOptions)};
            var SendMessageMethod4ComponentType_3 = componentType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageMethod4ComponentType_3, SendMessage_3);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(UnityEngine.SendMessageOptions)};
            var SendMessageMethod4ComponentType_4 = componentType.GetMethod("SendMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageMethod4ComponentType_4, SendMessage_4);
            
            //注册send message upwards 4 component
            args = new Type[]{typeof(System.String)};
            var SendMessageUpwardsMethod4ComponentType_1 = componentType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageUpwardsMethod4ComponentType_1, SendMessageUpwards_1);
            args = new Type[]{typeof(System.String), typeof(System.Object)};
            var SendMessageUpwardsMethod4ComponentType_2 = componentType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageUpwardsMethod4ComponentType_2, SendMessageUpwards_2);
            args = new Type[]{typeof(System.String), typeof(UnityEngine.SendMessageOptions)};
            var SendMessageUpwardsMethod4ComponentType_3 = componentType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageUpwardsMethod4ComponentType_3, SendMessageUpwards_3);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(UnityEngine.SendMessageOptions)};
            var SendMessageUpwardsMethod4ComponentType_4 = componentType.GetMethod("SendMessageUpwards", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(SendMessageUpwardsMethod4ComponentType_4, SendMessageUpwards_4);
            
            //注册BroadcastMessage 4 component
            args = new Type[]{typeof(System.String)};
            var BroadcastMessageMethod4ComponentType_1 = componentType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(BroadcastMessageMethod4ComponentType_1, BroadcastMessage_1);
            args = new Type[]{typeof(System.String), typeof(System.Object)};
            var BroadcastMessageMethod4ComponentType_2 = componentType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(BroadcastMessageMethod4ComponentType_2, BroadcastMessage_2);
            args = new Type[]{typeof(System.String), typeof(UnityEngine.SendMessageOptions)};
            var BroadcastMessageMethod4ComponentType_3 = componentType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(BroadcastMessageMethod4ComponentType_3, BroadcastMessage_3);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(UnityEngine.SendMessageOptions)};
            var BroadcastMessageMethod4ComponentType_4 = componentType.GetMethod("BroadcastMessage", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(BroadcastMessageMethod4ComponentType_4, BroadcastMessage_4);
            
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
                appdomain.RegisterCLRMethodRedirection(instantiateMethod,Instantiate);
            }
        }

        /// <summary>
        /// 找到热更对象的gameObject
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private static GameObject FindGOFromHotClass(ILTypeInstance instance)
        {
            var returnType = (instance as ILTypeInstance).Type;
            if (returnType.ReflectionType == typeof(MonoBehaviour))
            {
                var pi = returnType.ReflectionType.GetProperty("gameObject");
                return pi.GetValue((instance as ILTypeInstance).CLRInstance) as GameObject;
            }
            else if (returnType.ReflectionType.IsSubclassOf(typeof(MonoBehaviour)))
            {
                var pi = returnType.ReflectionType.BaseType.GetProperty("gameObject");
                return pi.GetValue((instance as ILTypeInstance).CLRInstance) as GameObject;
            }
            else
            {
                // foreach (var p in returnType.ReflectionType.GetProperties())
                // {
                //     Debug.Log(p.Name);
                // }
                return null;
            }
        }


        /// <summary>
        /// 处理热更的SendMessage
        /// </summary>
        /// <param name="go"></param>
        /// <param name="methodName"></param>
        /// <param name="value"></param>
        /// <param name="option"></param>
        private static void DoSendMessageOnHotCode(GameObject go, string methodName, object value = null,SendMessageOptions option = SendMessageOptions.RequireReceiver)
        {
            bool found = false;
            
            var clrInstances = go.GetComponents<MonoBehaviourAdapter.Adaptor>();
            for (int i = 0; i < clrInstances.Length; i++)
            {
                var clrInstance = clrInstances[i];
                if (clrInstance.ILInstance != null) //ILInstance为null, 表示是无效的MonoBehaviour，要略过
                {
                    IType t = clrInstance.ILInstance.Type;
                    if (value == null)
                    {
                        IMethod m = t.GetMethod(methodName, 0);
                        if (m != null)
                        {
                            Init.appdomain.Invoke(m, clrInstance.ILInstance, null);
                            found = true;
                        }
                    }
                    else
                    {
                        IMethod m = t.GetMethod(methodName, 1);
                        if (m != null)
                        {
                            Init.appdomain.Invoke(m, clrInstance.ILInstance, value);
                            found = true;
                        }
                    }
                }
            }
        }
        
        unsafe static StackObject* BroadcastMessage_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String @methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);

            UnityEngine.GameObject instance_of_this_method;
                
            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (UnityEngine.GameObject) typeof(UnityEngine.GameObject).CheckCLRTypes(instance);
            }
            else if(instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else
            {
                return __esp;
            }
            
            __intp.Free(ptr_of_this_method);
            
            Debug.LogError($"BroadcastMessage方法被重定向了，会尝试调用本地+热更的脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(instance_of_this_method, methodName);
            var go = instance_of_this_method.GetComponentsInChildren<Transform>(true);

            foreach (var g in go)
            {
                DoSendMessageOnHotCode(g.gameObject, methodName);
            }
            
            instance_of_this_method.SendMessageUpwards(@methodName);
            
            return __ret;
        }

        unsafe static StackObject* BroadcastMessage_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object @value = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.String @methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            
            UnityEngine.GameObject instance_of_this_method;
                
            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (UnityEngine.GameObject) typeof(UnityEngine.GameObject).CheckCLRTypes(instance);
            }
            else if(instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else
            {
                return __esp;
            }
            __intp.Free(ptr_of_this_method);
            
            Debug.LogError($"BroadcastMessage方法被重定向了，会尝试调用本地+热更的脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(instance_of_this_method, methodName,value);
            var go = instance_of_this_method.GetComponentsInChildren<Transform>(true);

            foreach (var g in go)
            {
                DoSendMessageOnHotCode(g.gameObject, methodName,value);
            }

            instance_of_this_method.SendMessageUpwards(@methodName, @value);
            
            return __ret;
        }

        unsafe static StackObject* BroadcastMessage_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.SendMessageOptions @options = (UnityEngine.SendMessageOptions)typeof(UnityEngine.SendMessageOptions).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.String @methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            
            UnityEngine.GameObject instance_of_this_method;
                
            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (UnityEngine.GameObject) typeof(UnityEngine.GameObject).CheckCLRTypes(instance);
            }
            else if(instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else
            {
                return __esp;
            }
            __intp.Free(ptr_of_this_method);
            
            Debug.LogError($"BroadcastMessage方法被重定向了，会尝试调用本地+热更的脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(instance_of_this_method, methodName,null,options);
            var go = instance_of_this_method.GetComponentsInChildren<Transform>(true);

            foreach (var g in go)
            {
                DoSendMessageOnHotCode(g.gameObject,methodName,null,options);
            }
            
            instance_of_this_method.SendMessageUpwards(@methodName, @options);

            return __ret;
        }

        unsafe static StackObject* BroadcastMessage_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.SendMessageOptions @options = (UnityEngine.SendMessageOptions)typeof(UnityEngine.SendMessageOptions).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object @value = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.String @methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            
            UnityEngine.GameObject instance_of_this_method;
                
            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (UnityEngine.GameObject) typeof(UnityEngine.GameObject).CheckCLRTypes(instance);
            }
            else if(instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else
            {
                return __esp;
            }
            __intp.Free(ptr_of_this_method);
            
            Debug.LogError($"BroadcastMessage方法被重定向了，会尝试调用本地+热更的脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(instance_of_this_method, methodName, value, options);
            var go = instance_of_this_method.GetComponentsInChildren<Transform>(true);

            foreach (var g in go)
            {
                DoSendMessageOnHotCode(g.gameObject,methodName, value, options);
            }

            instance_of_this_method.SendMessageUpwards(@methodName, @value,@options);

            return __ret;
        }
        
        unsafe static StackObject* SendMessageUpwards_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String @methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            
            UnityEngine.GameObject instance_of_this_method;
                
            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (UnityEngine.GameObject) typeof(UnityEngine.GameObject).CheckCLRTypes(instance);
            }
            else if(instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else
            {
                return __esp;
            }
            __intp.Free(ptr_of_this_method);
            
            Debug.LogError($"SendMessageUpwards方法被重定向了，会尝试调用本地+热更的脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(instance_of_this_method, methodName);
            try
            {
                var go = instance_of_this_method.transform.parent.gameObject;
                while (go != null)
                {
                    DoSendMessageOnHotCode(go, methodName);
                    go = go.transform.parent.gameObject;
                }
            }
            catch{}
            
            instance_of_this_method.SendMessageUpwards(@methodName);
            
            return __ret;
        }

        unsafe static StackObject* SendMessageUpwards_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object @value = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.String @methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            
            UnityEngine.GameObject instance_of_this_method;
                
            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (UnityEngine.GameObject) typeof(UnityEngine.GameObject).CheckCLRTypes(instance);
            }
            else if(instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else
            {
                return __esp;
            }
            __intp.Free(ptr_of_this_method);
            
            Debug.LogError($"SendMessageUpwards方法被重定向了，会尝试调用本地+热更的脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(instance_of_this_method, methodName,value);
            try
            {
                var go = instance_of_this_method.transform.parent.gameObject;
                while (go != null)
                {
                    DoSendMessageOnHotCode(go, methodName, value);
                    go = go.transform.parent.gameObject;
                }
            }
            catch{}

            instance_of_this_method.SendMessageUpwards(@methodName, @value);
            
            return __ret;
        }

        unsafe static StackObject* SendMessageUpwards_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.SendMessageOptions @options = (UnityEngine.SendMessageOptions)typeof(UnityEngine.SendMessageOptions).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.String @methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            
            UnityEngine.GameObject instance_of_this_method;
                
            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (UnityEngine.GameObject) typeof(UnityEngine.GameObject).CheckCLRTypes(instance);
            }
            else if(instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else
            {
                return __esp;
            }
            __intp.Free(ptr_of_this_method);
            
            Debug.LogError($"SendMessageUpwards方法被重定向了，会尝试调用本地+热更的脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(instance_of_this_method, methodName,null,options);
            try
            {
                var go = instance_of_this_method.transform.parent.gameObject;
                while (go != null)
                {
                    DoSendMessageOnHotCode(instance_of_this_method, methodName,null,options);
                    go = go.transform.parent.gameObject;
                }
            }
            catch{}
            
            instance_of_this_method.SendMessageUpwards(@methodName, @options);

            return __ret;
        }

        unsafe static StackObject* SendMessageUpwards_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.SendMessageOptions @options = (UnityEngine.SendMessageOptions)typeof(UnityEngine.SendMessageOptions).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object @value = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.String @methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            
            UnityEngine.GameObject instance_of_this_method;
                
            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (UnityEngine.GameObject) typeof(UnityEngine.GameObject).CheckCLRTypes(instance);
            }
            else if(instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else
            {
                return __esp;
            }
            __intp.Free(ptr_of_this_method);
            
            Debug.LogError($"SendMessageUpwards方法被重定向了，会尝试调用本地+热更的脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(instance_of_this_method, methodName, value, options);
            try
            {
                var go = instance_of_this_method.transform.parent.gameObject;
                while (go != null)
                {
                    DoSendMessageOnHotCode(go, methodName, value,options);
                    go = go.transform.parent.gameObject;
                }
            }
            catch{}

            instance_of_this_method.SendMessageUpwards(@methodName, @value,@options);

            return __ret;
        }

        unsafe static StackObject* SendMessage_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String @methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            
            UnityEngine.GameObject instance_of_this_method;
                
            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (UnityEngine.GameObject) typeof(UnityEngine.GameObject).CheckCLRTypes(instance);
            }
            else if(instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else
            {
                return __esp;
            }
            __intp.Free(ptr_of_this_method);

            Debug.LogError($"SendMessage方法被重定向了，会尝试调用本地+热更的脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");
            
            DoSendMessageOnHotCode(instance_of_this_method, methodName);
            instance_of_this_method.SendMessage(@methodName);
            
            return __ret;
        }

        unsafe static StackObject* SendMessage_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object @value = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.String @methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            
            UnityEngine.GameObject instance_of_this_method;
                
            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (UnityEngine.GameObject) typeof(UnityEngine.GameObject).CheckCLRTypes(instance);
            }
            else if(instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else
            {
                return __esp;
            }
            __intp.Free(ptr_of_this_method);
            
            Debug.LogError($"SendMessage方法被重定向了，会尝试调用本地+热更的脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(instance_of_this_method, methodName,value);
            instance_of_this_method.SendMessage(@methodName, @value);
            
            return __ret;
        }

        unsafe static StackObject* SendMessage_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.SendMessageOptions @options = (UnityEngine.SendMessageOptions)typeof(UnityEngine.SendMessageOptions).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.String @methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            
            UnityEngine.GameObject instance_of_this_method;
                
            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (UnityEngine.GameObject) typeof(UnityEngine.GameObject).CheckCLRTypes(instance);
            }
            else if(instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else
            {
                return __esp;
            }
            __intp.Free(ptr_of_this_method);
            
            Debug.LogError($"SendMessage方法被重定向了，会尝试调用本地+热更的脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(instance_of_this_method, methodName,null,options);
            instance_of_this_method.SendMessage(@methodName, @options);

            return __ret;
        }

        unsafe static StackObject* SendMessage_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.SendMessageOptions @options = (UnityEngine.SendMessageOptions)typeof(UnityEngine.SendMessageOptions).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object @value = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.String @methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            
            UnityEngine.GameObject instance_of_this_method;
                
            object instance = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
            if (instance is GameObject)
            {
                instance_of_this_method =
                    (UnityEngine.GameObject) typeof(UnityEngine.GameObject).CheckCLRTypes(instance);
            }
            else if(instance is ILTypeInstance)
            {
                instance_of_this_method = FindGOFromHotClass((ILTypeInstance) instance);
            }
            else
            {
                return __esp;
            }
            __intp.Free(ptr_of_this_method);
            
            Debug.LogError($"SendMessage方法被重定向了，会尝试调用本地+热更的脚本的'{methodName}'方法，如果本地没对应，会报错，可忽略");

            DoSendMessageOnHotCode(instance_of_this_method, methodName,value,options);
            instance_of_this_method.SendMessage(@methodName, @value, @options);

            return __ret;
        }
        
        
        /// <summary>
        /// 帮助Instantiate找到GameObject
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ins"></param>
        /// <param name="returnScipt"></param>
        /// <param name="returnType"></param>
        private static void SetGOForInstantiate(object instance,out GameObject ins,out bool returnScipt, out ILType returnType)
        {
            returnType = null;
            returnScipt = false;

            //判断类
            if (instance is GameObject)
            {
                ins = instance as GameObject;
            }
            else if(instance is ILTypeInstance)//如果是热更类需要处理
            {
                returnScipt = true;
                returnType = (instance as ILTypeInstance).Type;
                ins = FindGOFromHotClass((instance as ILTypeInstance));
            }
            else//如果本地类那简单
            {
                returnScipt = true;
                ins = (instance as Component).gameObject;
            }
        }

        private static void CleanGoForInstantiate(ref GameObject res,GameObject ins,AppDomain __domain)
        {
            //如果同时有adaptor和classbind，肯定是复制的，要给删了
            if (res.GetComponent<ClassBind>() && res.GetComponent<MonoBehaviourAdapter.Adaptor>())
            {
                UnityEngine.Object.DestroyImmediate(res.GetComponent<ClassBind>());//防止重复的ClassBind
            }
            
            //重新赋值instance的热更脚本
            var clrInstances = res.GetComponents<MonoBehaviourAdapter.Adaptor>();//clone的
            var clrInstances4Ins = ins.GetComponents<MonoBehaviourAdapter.Adaptor>();//原来的
            
            // Debug.Log(res.name+":"+clrInstances.Length);
            // Debug.Log(ins.name+":"+clrInstances4Ins.Length);
            
            for (int i = 0; i < clrInstances.Length; i++)
            {
                //获取对照适配器
                var clrInstance = clrInstances[i];
                var clrInstance4Ins = clrInstances4Ins[i];
                    
                clrInstance.Reset();//重置clone的

                //重新搞ILInstance
                ILTypeInstance ilInstance = clrInstance4Ins.ILInstance.Clone();//这里会有个问题，因为是复制的，有的地方可能指向的this，这时复制过去的是老的this，也就是原来的对象的this的东西
                clrInstance.ILInstance = ilInstance;
                clrInstance.AppDomain = __domain;
                    
                if (clrInstance4Ins.ILInstance.CLRInstance == clrInstance4Ins)//clr指向
                {
                    ilInstance.CLRInstance = clrInstance;
                }
                else
                {
                    ilInstance.CLRInstance = ilInstance;
                }
                
                //补上Awake
                clrInstance.Awake();
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
            var instance = StackObject.ToObject(ptr, __domain, __mStack) as object;
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
            if (instance is Transform)//gameobject, transform
            {
                parent = instance as Transform;
                //获取真正的gameObject
                var ptr2 = __esp - 2;
                var param2 = StackObject.ToObject(ptr2, __domain, __mStack) as object;
                __intp.Free(ptr2);
                if (param2 is GameObject)
                {
                    instance = param2;
                }
                else// gameobject, v3, quaternion, transform
                {
                    gotRot = true;
                    rotation = (Quaternion)param2;
                    var ptr3 = __esp - 3;
                    position = (Vector3)StackObject.ToObject(ptr3, __domain, __mStack);
                    __intp.Free(ptr3);
                }
            }
            if (instance is Quaternion)//gameobject, v3, quaternion
            {
                gotRot = true;
                rotation = (Quaternion) instance;
                //获取v3
                var ptr2 = __esp - 2;
                var param2 = StackObject.ToObject(ptr2, __domain, __mStack) as object;
                position = (Vector3)param2;
                __intp.Free(ptr2);
                //获取真正的gameObject
                var ptr3 = __esp - 3;
                instance = StackObject.ToObject(ptr3, __domain, __mStack);
                __intp.Free(ptr3);
            }
            if (instance is int)//gameobject, transform, bool，ILRuntime把热更的bool变int传了，1是true，0是false
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
                instance = StackObject.ToObject(ptr3, __domain, __mStack) as object;
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
            if (parent == null && !gotRot)//1参数
            {
               res = UnityEngine.Object.Instantiate(ins);//生成
            }
            else if (gotRot && parent == null)
            {
                res = UnityEngine.Object.Instantiate(ins,position,rotation); //生成
            }
            else if (gotRot && parent != null)
            {
                res = UnityEngine.Object.Instantiate(ins,position,rotation,parent); //生成
            }
            else if(gotIWS)//gameobject,transform,bool
            {
                res = UnityEngine.Object.Instantiate(ins,parent,instantiateInWorldSpace);//生成
            }
            else if (parent != null)//gameobject,transform
            {
               res = UnityEngine.Object.Instantiate(ins,parent);//生成
            }

            //如果同时有adaptor和classbind，肯定是复制的，要给删了
            if (res.GetComponent<ClassBind>() && res.GetComponent<MonoBehaviourAdapter.Adaptor>())
            {
                UnityEngine.Object.DestroyImmediate(res.GetComponent<ClassBind>());//防止重复的ClassBind
            }

            //重新赋值instance的热更脚本
            var clrInstances = res.GetComponents<MonoBehaviourAdapter.Adaptor>();//clone的
            var clrInstances4Ins = ins.GetComponents<MonoBehaviourAdapter.Adaptor>();//原来的
            for (int i = 0; i < clrInstances.Length; i++)
            {
                //获取对照适配器
                var clrInstance = clrInstances[i];
                var clrInstance4Ins = clrInstances4Ins[i];
                    
                clrInstance.Reset();//重置clone的

                //重新搞ILInstance
                ILTypeInstance ilInstance = clrInstance4Ins.ILInstance.Clone();//这里会有个问题，因为是复制的，有的地方可能指向的this，这时复制过去的是老的this，也就是原来的对象的this的东西
                clrInstance.ILInstance = ilInstance;
                clrInstance.AppDomain = __domain;
                    
                if (clrInstance4Ins.ILInstance.CLRInstance == clrInstance4Ins)//clr指向
                {
                    ilInstance.CLRInstance = clrInstance;
                }
                else
                {
                    ilInstance.CLRInstance = ilInstance;
                }
                
                //补上Awake
                clrInstance.Awake();
                
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
                if (go[i] == res)
                {
                    continue;
                }
                var subGo = go[i].gameObject;
                CleanGoForInstantiate(ref subGo,go2[i].gameObject,__domain);
            }
            
            //处理好了后，如果还有classbind就是prefab，需要重新bind
            if (res.GetComponentInChildren<ClassBind>(true))
            {
                ClassBindMgr.DoBind();
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
            Core.Log.PrintError(message + "\n" + stacktrace);
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
            Core.Log.PrintWarning(message + "\n" + stacktrace);
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
            Core.Log.Print(message + "\n" + stacktrace);
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
            Debug.LogError(message + "\n" + stacktrace);
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
            Debug.LogWarning(message + "\n" + stacktrace);
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
            Debug.Log(message + "\n" + stacktrace);
            return __ret;
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
                object res;
                if (type is CLRType)
                {
                    //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                    res = instance.AddComponent(type.TypeForCLR);
                }
                else
                {
                    //热更DLL内的类型比较麻烦。首先我们得自己手动创建实例
                    ILTypeInstance ilInstance = Init.appdomain.Instantiate(type.ReflectionType.FullName);
                    JEngine.Core.Log.PrintWarning($"{type.ReflectionType.FullName}由于带构造函数生成，会有来自Unity的警告，请忽略");
                    
                    //接下来创建Adapter实例
                    var clrInstance = instance.AddComponent<MonoBehaviourAdapter.Adaptor>();
                    //unity创建的实例并没有热更DLL里面的实例，所以需要手动赋值
                    clrInstance.ILInstance = ilInstance;
                    clrInstance.AppDomain = __domain;
                    //这个实例默认创建的CLRInstance不是通过AddComponent出来的有效实例，所以得手动替换
                    ilInstance.CLRInstance = clrInstance;

                    res = clrInstance.ILInstance; //交给ILRuntime的实例应该为ILInstance

                    clrInstance.Awake(); //因为Unity调用这个方法时还没准备好所以这里补调一次
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
                else if(ins is Component)
                {
                    instance = ((Component) ins).gameObject;
                }
                else if(ins is ILTypeInstance)
                {
                    instance = FindGOFromHotClass(((ILTypeInstance) ins));
                }
                else
                {
                    Debug.LogError($"[GetComponent错误] 不支持的参数类型：{ins.GetType().FullName}，" +
                                   $"请传参GameObject或继承MonoBehaviour的对象");
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
                    var clrInstances = instance.GetComponents<MonoBehaviourAdapter.Adaptor>();
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

                    if (res == null)//如果是null，但有classBind，就先绑定
                    {
                        var cb = instance.GetComponent<ClassBind>();
                        if (cb != null)
                        {
                            //执行绑定
                            ClassBindMgr.DoBind();
                            
                            //再次循环
                            clrInstances = instance.GetComponents<MonoBehaviourAdapter.Adaptor>();
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