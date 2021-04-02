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
    unsafe class UnityEngine_EventSystems_EventTrigger_Binding_Entry_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(UnityEngine.EventSystems.EventTrigger.Entry);

            field = type.GetField("eventID", flag);
            app.RegisterCLRFieldGetter(field, get_eventID_0);
            app.RegisterCLRFieldSetter(field, set_eventID_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_eventID_0, AssignFromStack_eventID_0);
            field = type.GetField("callback", flag);
            app.RegisterCLRFieldGetter(field, get_callback_1);
            app.RegisterCLRFieldSetter(field, set_callback_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_callback_1, AssignFromStack_callback_1);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }



        static object get_eventID_0(ref object o)
        {
            return ((UnityEngine.EventSystems.EventTrigger.Entry)o).eventID;
        }

        static StackObject* CopyToStack_eventID_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((UnityEngine.EventSystems.EventTrigger.Entry)o).eventID;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_eventID_0(ref object o, object v)
        {
            ((UnityEngine.EventSystems.EventTrigger.Entry)o).eventID = (UnityEngine.EventSystems.EventTriggerType)v;
        }

        static StackObject* AssignFromStack_eventID_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            UnityEngine.EventSystems.EventTriggerType @eventID = (UnityEngine.EventSystems.EventTriggerType)typeof(UnityEngine.EventSystems.EventTriggerType).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((UnityEngine.EventSystems.EventTrigger.Entry)o).eventID = @eventID;
            return ptr_of_this_method;
        }

        static object get_callback_1(ref object o)
        {
            return ((UnityEngine.EventSystems.EventTrigger.Entry)o).callback;
        }

        static StackObject* CopyToStack_callback_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((UnityEngine.EventSystems.EventTrigger.Entry)o).callback;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_callback_1(ref object o, object v)
        {
            ((UnityEngine.EventSystems.EventTrigger.Entry)o).callback = (UnityEngine.EventSystems.EventTrigger.TriggerEvent)v;
        }

        static StackObject* AssignFromStack_callback_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            UnityEngine.EventSystems.EventTrigger.TriggerEvent @callback = (UnityEngine.EventSystems.EventTrigger.TriggerEvent)typeof(UnityEngine.EventSystems.EventTrigger.TriggerEvent).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((UnityEngine.EventSystems.EventTrigger.Entry)o).callback = @callback;
            return ptr_of_this_method;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = new UnityEngine.EventSystems.EventTrigger.Entry();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
