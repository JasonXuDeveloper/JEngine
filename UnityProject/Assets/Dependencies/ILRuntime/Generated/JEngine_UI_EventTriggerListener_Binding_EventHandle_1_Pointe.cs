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
    unsafe class JEngine_UI_EventTriggerListener_Binding_EventHandle_1_PointerEventData_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>);
            args = new Type[]{typeof(JEngine.UI.UIEventHandle<UnityEngine.EventSystems.PointerEventData>)};
            method = type.GetMethod("AddListener", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, AddListener_0);


        }


        static StackObject* AddListener_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            JEngine.UI.UIEventHandle<UnityEngine.EventSystems.PointerEventData> @handle = (JEngine.UI.UIEventHandle<UnityEngine.EventSystems.PointerEventData>)typeof(JEngine.UI.UIEventHandle<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData> instance_of_this_method = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)typeof(JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.AddListener(@handle);

            return __ret;
        }



    }
}
