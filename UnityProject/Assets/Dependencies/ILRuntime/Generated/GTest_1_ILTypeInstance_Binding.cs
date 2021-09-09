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
    unsafe class GTest_1_ILTypeInstance_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(global::GTest<ILRuntime.Runtime.Intepreter.ILTypeInstance>);

            field = type.GetField("a", flag);
            app.RegisterCLRFieldGetter(field, get_a_0);
            app.RegisterCLRFieldSetter(field, set_a_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_a_0, AssignFromStack_a_0);
            field = type.GetField("b", flag);
            app.RegisterCLRFieldGetter(field, get_b_1);
            app.RegisterCLRFieldSetter(field, set_b_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_b_1, AssignFromStack_b_1);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }



        static object get_a_0(ref object o)
        {
            return ((global::GTest<ILRuntime.Runtime.Intepreter.ILTypeInstance>)o).a;
        }

        static StackObject* CopyToStack_a_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((global::GTest<ILRuntime.Runtime.Intepreter.ILTypeInstance>)o).a;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_a_0(ref object o, object v)
        {
            ((global::GTest<ILRuntime.Runtime.Intepreter.ILTypeInstance>)o).a = (System.Int32)v;
        }

        static StackObject* AssignFromStack_a_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Int32 @a = ptr_of_this_method->Value;
            ((global::GTest<ILRuntime.Runtime.Intepreter.ILTypeInstance>)o).a = @a;
            return ptr_of_this_method;
        }

        static object get_b_1(ref object o)
        {
            return ((global::GTest<ILRuntime.Runtime.Intepreter.ILTypeInstance>)o).b;
        }

        static StackObject* CopyToStack_b_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((global::GTest<ILRuntime.Runtime.Intepreter.ILTypeInstance>)o).b;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_b_1(ref object o, object v)
        {
            ((global::GTest<ILRuntime.Runtime.Intepreter.ILTypeInstance>)o).b = (ILRuntime.Runtime.Intepreter.ILTypeInstance)v;
        }

        static StackObject* AssignFromStack_b_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ILRuntime.Runtime.Intepreter.ILTypeInstance @b = (ILRuntime.Runtime.Intepreter.ILTypeInstance)typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            ((global::GTest<ILRuntime.Runtime.Intepreter.ILTypeInstance>)o).b = @b;
            return ptr_of_this_method;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = new global::GTest<ILRuntime.Runtime.Intepreter.ILTypeInstance>();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
