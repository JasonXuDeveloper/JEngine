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
    unsafe class MonoBehaviourAdapter_Binding_Adaptor_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(global::MonoBehaviourAdapter.Adaptor);
            args = new Type[]{};
            method = type.GetMethod("get_ILInstance", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_ILInstance_0);

            field = type.GetField("isJBehaviour", flag);
            app.RegisterCLRFieldGetter(field, get_isJBehaviour_0);
            app.RegisterCLRFieldSetter(field, set_isJBehaviour_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_isJBehaviour_0, AssignFromStack_isJBehaviour_0);


        }


        static StackObject* get_ILInstance_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            global::MonoBehaviourAdapter.Adaptor instance_of_this_method = (global::MonoBehaviourAdapter.Adaptor)typeof(global::MonoBehaviourAdapter.Adaptor).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.ILInstance;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


        static object get_isJBehaviour_0(ref object o)
        {
            return ((global::MonoBehaviourAdapter.Adaptor)o).isJBehaviour;
        }

        static StackObject* CopyToStack_isJBehaviour_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((global::MonoBehaviourAdapter.Adaptor)o).isJBehaviour;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static void set_isJBehaviour_0(ref object o, object v)
        {
            ((global::MonoBehaviourAdapter.Adaptor)o).isJBehaviour = (System.Boolean)v;
        }

        static StackObject* AssignFromStack_isJBehaviour_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Boolean @isJBehaviour = ptr_of_this_method->Value == 1;
            ((global::MonoBehaviourAdapter.Adaptor)o).isJBehaviour = @isJBehaviour;
            return ptr_of_this_method;
        }



    }
}
