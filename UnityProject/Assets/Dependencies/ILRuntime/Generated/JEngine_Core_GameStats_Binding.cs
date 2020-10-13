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
    unsafe class JEngine_Core_GameStats_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(JEngine.Core.GameStats);

            field = type.GetField("fps", flag);
            app.RegisterCLRFieldGetter(field, get_fps_0);
            app.RegisterCLRFieldSetter(field, set_fps_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_fps_0, AssignFromStack_fps_0);


        }



        static object get_fps_0(ref object o)
        {
            return JEngine.Core.GameStats.fps;
        }

        static StackObject* CopyToStack_fps_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = JEngine.Core.GameStats.fps;
            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_fps_0(ref object o, object v)
        {
            JEngine.Core.GameStats.fps = (System.Single)v;
        }

        static StackObject* AssignFromStack_fps_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Single @fps = *(float*)&ptr_of_this_method->Value;
            JEngine.Core.GameStats.fps = @fps;
            return ptr_of_this_method;
        }



    }
}
