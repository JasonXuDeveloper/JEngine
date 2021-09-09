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
    unsafe class JSONTest_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(global::JSONTest);

            field = type.GetField("a", flag);
            app.RegisterCLRFieldGetter(field, get_a_0);
            app.RegisterCLRFieldSetter(field, set_a_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_a_0, AssignFromStack_a_0);
            field = type.GetField("b", flag);
            app.RegisterCLRFieldGetter(field, get_b_1);
            app.RegisterCLRFieldSetter(field, set_b_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_b_1, AssignFromStack_b_1);
            field = type.GetField("c", flag);
            app.RegisterCLRFieldGetter(field, get_c_2);
            app.RegisterCLRFieldSetter(field, set_c_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_c_2, AssignFromStack_c_2);
            field = type.GetField("d", flag);
            app.RegisterCLRFieldGetter(field, get_d_3);
            app.RegisterCLRFieldSetter(field, set_d_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_d_3, AssignFromStack_d_3);
            field = type.GetField("e", flag);
            app.RegisterCLRFieldGetter(field, get_e_4);
            app.RegisterCLRFieldSetter(field, set_e_4);
            app.RegisterCLRFieldBinding(field, CopyToStack_e_4, AssignFromStack_e_4);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }



        static object get_a_0(ref object o)
        {
            return ((global::JSONTest)o).a;
        }

        static StackObject* CopyToStack_a_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((global::JSONTest)o).a;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_a_0(ref object o, object v)
        {
            ((global::JSONTest)o).a = (System.Int32)v;
        }

        static StackObject* AssignFromStack_a_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Int32 @a = ptr_of_this_method->Value;
            ((global::JSONTest)o).a = @a;
            return ptr_of_this_method;
        }

        static object get_b_1(ref object o)
        {
            return ((global::JSONTest)o).b;
        }

        static StackObject* CopyToStack_b_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((global::JSONTest)o).b;
            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_b_1(ref object o, object v)
        {
            ((global::JSONTest)o).b = (System.Single)v;
        }

        static StackObject* AssignFromStack_b_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Single @b = *(float*)&ptr_of_this_method->Value;
            ((global::JSONTest)o).b = @b;
            return ptr_of_this_method;
        }

        static object get_c_2(ref object o)
        {
            return ((global::JSONTest)o).c;
        }

        static StackObject* CopyToStack_c_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((global::JSONTest)o).c;
            __ret->ObjectType = ObjectTypes.Double;
            *(double*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_c_2(ref object o, object v)
        {
            ((global::JSONTest)o).c = (System.Double)v;
        }

        static StackObject* AssignFromStack_c_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Double @c = *(double*)&ptr_of_this_method->Value;
            ((global::JSONTest)o).c = @c;
            return ptr_of_this_method;
        }

        static object get_d_3(ref object o)
        {
            return ((global::JSONTest)o).d;
        }

        static StackObject* CopyToStack_d_3(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((global::JSONTest)o).d;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static void set_d_3(ref object o, object v)
        {
            ((global::JSONTest)o).d = (System.Boolean)v;
        }

        static StackObject* AssignFromStack_d_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Boolean @d = ptr_of_this_method->Value == 1;
            ((global::JSONTest)o).d = @d;
            return ptr_of_this_method;
        }

        static object get_e_4(ref object o)
        {
            return ((global::JSONTest)o).e;
        }

        static StackObject* CopyToStack_e_4(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((global::JSONTest)o).e;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_e_4(ref object o, object v)
        {
            ((global::JSONTest)o).e = (System.String)v;
        }

        static StackObject* AssignFromStack_e_4(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @e = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            ((global::JSONTest)o).e = @e;
            return ptr_of_this_method;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = new global::JSONTest();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
