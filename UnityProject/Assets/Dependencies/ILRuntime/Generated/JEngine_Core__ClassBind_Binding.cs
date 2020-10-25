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
    unsafe class JEngine_Core__ClassBind_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(JEngine.Core._ClassBind);

            field = type.GetField("Namespace", flag);
            app.RegisterCLRFieldGetter(field, get_Namespace_0);
            app.RegisterCLRFieldSetter(field, set_Namespace_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_Namespace_0, AssignFromStack_Namespace_0);
            field = type.GetField("Class", flag);
            app.RegisterCLRFieldGetter(field, get_Class_1);
            app.RegisterCLRFieldSetter(field, set_Class_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_Class_1, AssignFromStack_Class_1);
            field = type.GetField("ActiveAfter", flag);
            app.RegisterCLRFieldGetter(field, get_ActiveAfter_2);
            app.RegisterCLRFieldSetter(field, set_ActiveAfter_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_ActiveAfter_2, AssignFromStack_ActiveAfter_2);
            field = type.GetField("UseConstructor", flag);
            app.RegisterCLRFieldGetter(field, get_UseConstructor_3);
            app.RegisterCLRFieldSetter(field, set_UseConstructor_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_UseConstructor_3, AssignFromStack_UseConstructor_3);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }



        static object get_Namespace_0(ref object o)
        {
            return ((JEngine.Core._ClassBind)o).Namespace;
        }

        static StackObject* CopyToStack_Namespace_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.Core._ClassBind)o).Namespace;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Namespace_0(ref object o, object v)
        {
            ((JEngine.Core._ClassBind)o).Namespace = (System.String)v;
        }

        static StackObject* AssignFromStack_Namespace_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @Namespace = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((JEngine.Core._ClassBind)o).Namespace = @Namespace;
            return ptr_of_this_method;
        }

        static object get_Class_1(ref object o)
        {
            return ((JEngine.Core._ClassBind)o).Class;
        }

        static StackObject* CopyToStack_Class_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.Core._ClassBind)o).Class;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Class_1(ref object o, object v)
        {
            ((JEngine.Core._ClassBind)o).Class = (System.String)v;
        }

        static StackObject* AssignFromStack_Class_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @Class = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((JEngine.Core._ClassBind)o).Class = @Class;
            return ptr_of_this_method;
        }

        static object get_ActiveAfter_2(ref object o)
        {
            return ((JEngine.Core._ClassBind)o).ActiveAfter;
        }

        static StackObject* CopyToStack_ActiveAfter_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.Core._ClassBind)o).ActiveAfter;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static void set_ActiveAfter_2(ref object o, object v)
        {
            ((JEngine.Core._ClassBind)o).ActiveAfter = (System.Boolean)v;
        }

        static StackObject* AssignFromStack_ActiveAfter_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Boolean @ActiveAfter = ptr_of_this_method->Value == 1;
            ((JEngine.Core._ClassBind)o).ActiveAfter = @ActiveAfter;
            return ptr_of_this_method;
        }

        static object get_UseConstructor_3(ref object o)
        {
            return ((JEngine.Core._ClassBind)o).UseConstructor;
        }

        static StackObject* CopyToStack_UseConstructor_3(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.Core._ClassBind)o).UseConstructor;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static void set_UseConstructor_3(ref object o, object v)
        {
            ((JEngine.Core._ClassBind)o).UseConstructor = (System.Boolean)v;
        }

        static StackObject* AssignFromStack_UseConstructor_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Boolean @UseConstructor = ptr_of_this_method->Value == 1;
            ((JEngine.Core._ClassBind)o).UseConstructor = @UseConstructor;
            return ptr_of_this_method;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = new JEngine.Core._ClassBind();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
