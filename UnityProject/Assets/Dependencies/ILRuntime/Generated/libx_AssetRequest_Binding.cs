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
    unsafe class libx_AssetRequest_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(libx.AssetRequest);
            args = new Type[]{};
            method = type.GetMethod("get_asset", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_asset_0);
            args = new Type[]{};
            method = type.GetMethod("get_isDone", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_isDone_1);
            args = new Type[]{};
            method = type.GetMethod("get_progress", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_progress_2);
            args = new Type[]{};
            method = type.GetMethod("get_loadState", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_loadState_3);
            args = new Type[]{};
            method = type.GetMethod("get_error", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_error_4);

            field = type.GetField("completed", flag);
            app.RegisterCLRFieldGetter(field, get_completed_0);
            app.RegisterCLRFieldSetter(field, set_completed_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_completed_0, AssignFromStack_completed_0);


        }


        static StackObject* get_asset_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            libx.AssetRequest instance_of_this_method = (libx.AssetRequest)typeof(libx.AssetRequest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.asset;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* get_isDone_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            libx.AssetRequest instance_of_this_method = (libx.AssetRequest)typeof(libx.AssetRequest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.isDone;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* get_progress_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            libx.AssetRequest instance_of_this_method = (libx.AssetRequest)typeof(libx.AssetRequest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.progress;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_loadState_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            libx.AssetRequest instance_of_this_method = (libx.AssetRequest)typeof(libx.AssetRequest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.loadState;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* get_error_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            libx.AssetRequest instance_of_this_method = (libx.AssetRequest)typeof(libx.AssetRequest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.error;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


        static object get_completed_0(ref object o)
        {
            return ((libx.AssetRequest)o).completed;
        }

        static StackObject* CopyToStack_completed_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((libx.AssetRequest)o).completed;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_completed_0(ref object o, object v)
        {
            ((libx.AssetRequest)o).completed = (System.Action<libx.AssetRequest>)v;
        }

        static StackObject* AssignFromStack_completed_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<libx.AssetRequest> @completed = (System.Action<libx.AssetRequest>)typeof(System.Action<libx.AssetRequest>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((libx.AssetRequest)o).completed = @completed;
            return ptr_of_this_method;
        }



    }
}
