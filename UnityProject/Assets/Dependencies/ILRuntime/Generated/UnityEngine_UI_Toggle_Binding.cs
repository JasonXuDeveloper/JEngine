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
    unsafe class UnityEngine_UI_Toggle_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(UnityEngine.UI.Toggle);
            args = new Type[]{};
            method = type.GetMethod("get_isOn", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_isOn_0);

            field = type.GetField("onValueChanged", flag);
            app.RegisterCLRFieldGetter(field, get_onValueChanged_0);
            app.RegisterCLRFieldSetter(field, set_onValueChanged_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_onValueChanged_0, AssignFromStack_onValueChanged_0);


        }


        static StackObject* get_isOn_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.UI.Toggle instance_of_this_method = (UnityEngine.UI.Toggle)typeof(UnityEngine.UI.Toggle).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.isOn;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }


        static object get_onValueChanged_0(ref object o)
        {
            return ((UnityEngine.UI.Toggle)o).onValueChanged;
        }

        static StackObject* CopyToStack_onValueChanged_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((UnityEngine.UI.Toggle)o).onValueChanged;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onValueChanged_0(ref object o, object v)
        {
            ((UnityEngine.UI.Toggle)o).onValueChanged = (UnityEngine.UI.Toggle.ToggleEvent)v;
        }

        static StackObject* AssignFromStack_onValueChanged_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            UnityEngine.UI.Toggle.ToggleEvent @onValueChanged = (UnityEngine.UI.Toggle.ToggleEvent)typeof(UnityEngine.UI.Toggle.ToggleEvent).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((UnityEngine.UI.Toggle)o).onValueChanged = @onValueChanged;
            return ptr_of_this_method;
        }



    }
}
