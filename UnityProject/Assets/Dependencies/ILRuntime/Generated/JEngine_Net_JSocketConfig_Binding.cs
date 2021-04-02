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
    unsafe class JEngine_Net_JSocketConfig_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(JEngine.Net.JSocketConfig);

            field = type.GetField("eventOpenName", flag);
            app.RegisterCLRFieldGetter(field, get_eventOpenName_0);
            app.RegisterCLRFieldSetter(field, set_eventOpenName_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_eventOpenName_0, AssignFromStack_eventOpenName_0);
            field = type.GetField("eventConnectName", flag);
            app.RegisterCLRFieldGetter(field, get_eventConnectName_1);
            app.RegisterCLRFieldSetter(field, set_eventConnectName_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_eventConnectName_1, AssignFromStack_eventConnectName_1);
            field = type.GetField("eventDisconnectName", flag);
            app.RegisterCLRFieldGetter(field, get_eventDisconnectName_2);
            app.RegisterCLRFieldSetter(field, set_eventDisconnectName_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_eventDisconnectName_2, AssignFromStack_eventDisconnectName_2);
            field = type.GetField("eventCloseName", flag);
            app.RegisterCLRFieldGetter(field, get_eventCloseName_3);
            app.RegisterCLRFieldSetter(field, set_eventCloseName_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_eventCloseName_3, AssignFromStack_eventCloseName_3);
            field = type.GetField("eventErrorName", flag);
            app.RegisterCLRFieldGetter(field, get_eventErrorName_4);
            app.RegisterCLRFieldSetter(field, set_eventErrorName_4);
            app.RegisterCLRFieldBinding(field, CopyToStack_eventErrorName_4, AssignFromStack_eventErrorName_4);


        }



        static object get_eventOpenName_0(ref object o)
        {
            return ((JEngine.Net.JSocketConfig)o).eventOpenName;
        }

        static StackObject* CopyToStack_eventOpenName_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.Net.JSocketConfig)o).eventOpenName;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_eventOpenName_0(ref object o, object v)
        {
            ((JEngine.Net.JSocketConfig)o).eventOpenName = (System.String)v;
        }

        static StackObject* AssignFromStack_eventOpenName_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @eventOpenName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((JEngine.Net.JSocketConfig)o).eventOpenName = @eventOpenName;
            return ptr_of_this_method;
        }

        static object get_eventConnectName_1(ref object o)
        {
            return ((JEngine.Net.JSocketConfig)o).eventConnectName;
        }

        static StackObject* CopyToStack_eventConnectName_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.Net.JSocketConfig)o).eventConnectName;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_eventConnectName_1(ref object o, object v)
        {
            ((JEngine.Net.JSocketConfig)o).eventConnectName = (System.String)v;
        }

        static StackObject* AssignFromStack_eventConnectName_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @eventConnectName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((JEngine.Net.JSocketConfig)o).eventConnectName = @eventConnectName;
            return ptr_of_this_method;
        }

        static object get_eventDisconnectName_2(ref object o)
        {
            return ((JEngine.Net.JSocketConfig)o).eventDisconnectName;
        }

        static StackObject* CopyToStack_eventDisconnectName_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.Net.JSocketConfig)o).eventDisconnectName;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_eventDisconnectName_2(ref object o, object v)
        {
            ((JEngine.Net.JSocketConfig)o).eventDisconnectName = (System.String)v;
        }

        static StackObject* AssignFromStack_eventDisconnectName_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @eventDisconnectName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((JEngine.Net.JSocketConfig)o).eventDisconnectName = @eventDisconnectName;
            return ptr_of_this_method;
        }

        static object get_eventCloseName_3(ref object o)
        {
            return ((JEngine.Net.JSocketConfig)o).eventCloseName;
        }

        static StackObject* CopyToStack_eventCloseName_3(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.Net.JSocketConfig)o).eventCloseName;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_eventCloseName_3(ref object o, object v)
        {
            ((JEngine.Net.JSocketConfig)o).eventCloseName = (System.String)v;
        }

        static StackObject* AssignFromStack_eventCloseName_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @eventCloseName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((JEngine.Net.JSocketConfig)o).eventCloseName = @eventCloseName;
            return ptr_of_this_method;
        }

        static object get_eventErrorName_4(ref object o)
        {
            return ((JEngine.Net.JSocketConfig)o).eventErrorName;
        }

        static StackObject* CopyToStack_eventErrorName_4(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.Net.JSocketConfig)o).eventErrorName;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_eventErrorName_4(ref object o, object v)
        {
            ((JEngine.Net.JSocketConfig)o).eventErrorName = (System.String)v;
        }

        static StackObject* AssignFromStack_eventErrorName_4(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @eventErrorName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((JEngine.Net.JSocketConfig)o).eventErrorName = @eventErrorName;
            return ptr_of_this_method;
        }



    }
}
