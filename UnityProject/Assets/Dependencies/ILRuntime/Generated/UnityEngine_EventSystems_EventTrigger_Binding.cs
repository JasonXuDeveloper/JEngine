using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class UnityEngine_EventSystems_EventTrigger_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(UnityEngine.EventSystems.EventTrigger);
            args = new Type[]{typeof(System.Collections.Generic.List<UnityEngine.EventSystems.EventTrigger.Entry>)};
            method = type.GetMethod("set_triggers", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_triggers_0);
            args = new Type[]{};
            method = type.GetMethod("get_triggers", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_triggers_1);


        }


        static StackObject* set_triggers_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.List<UnityEngine.EventSystems.EventTrigger.Entry> @value = (System.Collections.Generic.List<UnityEngine.EventSystems.EventTrigger.Entry>)typeof(System.Collections.Generic.List<UnityEngine.EventSystems.EventTrigger.Entry>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.EventSystems.EventTrigger instance_of_this_method = (UnityEngine.EventSystems.EventTrigger)typeof(UnityEngine.EventSystems.EventTrigger).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.triggers = value;

            return __ret;
        }

        static StackObject* get_triggers_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.EventSystems.EventTrigger instance_of_this_method = (UnityEngine.EventSystems.EventTrigger)typeof(UnityEngine.EventSystems.EventTrigger).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.triggers;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }



    }
}
