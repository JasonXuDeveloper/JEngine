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
    unsafe class JEngine_UI_EventTriggerListener_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(JEngine.UI.EventTriggerListener);
            args = new Type[]{typeof(UnityEngine.GameObject)};
            method = type.GetMethod("Get", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Get_0);

            field = type.GetField("onClick", flag);
            app.RegisterCLRFieldGetter(field, get_onClick_0);
            app.RegisterCLRFieldSetter(field, set_onClick_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_onClick_0, AssignFromStack_onClick_0);
            field = type.GetField("onDoubleClick", flag);
            app.RegisterCLRFieldGetter(field, get_onDoubleClick_1);
            app.RegisterCLRFieldSetter(field, set_onDoubleClick_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_onDoubleClick_1, AssignFromStack_onDoubleClick_1);
            field = type.GetField("onPress", flag);
            app.RegisterCLRFieldGetter(field, get_onPress_2);
            app.RegisterCLRFieldSetter(field, set_onPress_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_onPress_2, AssignFromStack_onPress_2);
            field = type.GetField("onUp", flag);
            app.RegisterCLRFieldGetter(field, get_onUp_3);
            app.RegisterCLRFieldSetter(field, set_onUp_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_onUp_3, AssignFromStack_onUp_3);
            field = type.GetField("onBeginDrag", flag);
            app.RegisterCLRFieldGetter(field, get_onBeginDrag_4);
            app.RegisterCLRFieldSetter(field, set_onBeginDrag_4);
            app.RegisterCLRFieldBinding(field, CopyToStack_onBeginDrag_4, AssignFromStack_onBeginDrag_4);
            field = type.GetField("onDrag", flag);
            app.RegisterCLRFieldGetter(field, get_onDrag_5);
            app.RegisterCLRFieldSetter(field, set_onDrag_5);
            app.RegisterCLRFieldBinding(field, CopyToStack_onDrag_5, AssignFromStack_onDrag_5);
            field = type.GetField("onEndDrag", flag);
            app.RegisterCLRFieldGetter(field, get_onEndDrag_6);
            app.RegisterCLRFieldSetter(field, set_onEndDrag_6);
            app.RegisterCLRFieldBinding(field, CopyToStack_onEndDrag_6, AssignFromStack_onEndDrag_6);
            field = type.GetField("onScroll", flag);
            app.RegisterCLRFieldGetter(field, get_onScroll_7);
            app.RegisterCLRFieldSetter(field, set_onScroll_7);
            app.RegisterCLRFieldBinding(field, CopyToStack_onScroll_7, AssignFromStack_onScroll_7);


        }


        static StackObject* Get_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.GameObject @go = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = JEngine.UI.EventTriggerListener.Get(@go);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


        static object get_onClick_0(ref object o)
        {
            return ((JEngine.UI.EventTriggerListener)o).onClick;
        }

        static StackObject* CopyToStack_onClick_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.UI.EventTriggerListener)o).onClick;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onClick_0(ref object o, object v)
        {
            ((JEngine.UI.EventTriggerListener)o).onClick = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)v;
        }

        static StackObject* AssignFromStack_onClick_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData> @onClick = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)typeof(JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((JEngine.UI.EventTriggerListener)o).onClick = @onClick;
            return ptr_of_this_method;
        }

        static object get_onDoubleClick_1(ref object o)
        {
            return ((JEngine.UI.EventTriggerListener)o).onDoubleClick;
        }

        static StackObject* CopyToStack_onDoubleClick_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.UI.EventTriggerListener)o).onDoubleClick;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onDoubleClick_1(ref object o, object v)
        {
            ((JEngine.UI.EventTriggerListener)o).onDoubleClick = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)v;
        }

        static StackObject* AssignFromStack_onDoubleClick_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData> @onDoubleClick = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)typeof(JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((JEngine.UI.EventTriggerListener)o).onDoubleClick = @onDoubleClick;
            return ptr_of_this_method;
        }

        static object get_onPress_2(ref object o)
        {
            return ((JEngine.UI.EventTriggerListener)o).onPress;
        }

        static StackObject* CopyToStack_onPress_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.UI.EventTriggerListener)o).onPress;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onPress_2(ref object o, object v)
        {
            ((JEngine.UI.EventTriggerListener)o).onPress = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)v;
        }

        static StackObject* AssignFromStack_onPress_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData> @onPress = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)typeof(JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((JEngine.UI.EventTriggerListener)o).onPress = @onPress;
            return ptr_of_this_method;
        }

        static object get_onUp_3(ref object o)
        {
            return ((JEngine.UI.EventTriggerListener)o).onUp;
        }

        static StackObject* CopyToStack_onUp_3(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.UI.EventTriggerListener)o).onUp;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onUp_3(ref object o, object v)
        {
            ((JEngine.UI.EventTriggerListener)o).onUp = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)v;
        }

        static StackObject* AssignFromStack_onUp_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData> @onUp = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)typeof(JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((JEngine.UI.EventTriggerListener)o).onUp = @onUp;
            return ptr_of_this_method;
        }

        static object get_onBeginDrag_4(ref object o)
        {
            return ((JEngine.UI.EventTriggerListener)o).onBeginDrag;
        }

        static StackObject* CopyToStack_onBeginDrag_4(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.UI.EventTriggerListener)o).onBeginDrag;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onBeginDrag_4(ref object o, object v)
        {
            ((JEngine.UI.EventTriggerListener)o).onBeginDrag = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)v;
        }

        static StackObject* AssignFromStack_onBeginDrag_4(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData> @onBeginDrag = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)typeof(JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((JEngine.UI.EventTriggerListener)o).onBeginDrag = @onBeginDrag;
            return ptr_of_this_method;
        }

        static object get_onDrag_5(ref object o)
        {
            return ((JEngine.UI.EventTriggerListener)o).onDrag;
        }

        static StackObject* CopyToStack_onDrag_5(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.UI.EventTriggerListener)o).onDrag;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onDrag_5(ref object o, object v)
        {
            ((JEngine.UI.EventTriggerListener)o).onDrag = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)v;
        }

        static StackObject* AssignFromStack_onDrag_5(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData> @onDrag = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)typeof(JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((JEngine.UI.EventTriggerListener)o).onDrag = @onDrag;
            return ptr_of_this_method;
        }

        static object get_onEndDrag_6(ref object o)
        {
            return ((JEngine.UI.EventTriggerListener)o).onEndDrag;
        }

        static StackObject* CopyToStack_onEndDrag_6(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.UI.EventTriggerListener)o).onEndDrag;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onEndDrag_6(ref object o, object v)
        {
            ((JEngine.UI.EventTriggerListener)o).onEndDrag = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)v;
        }

        static StackObject* AssignFromStack_onEndDrag_6(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData> @onEndDrag = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)typeof(JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((JEngine.UI.EventTriggerListener)o).onEndDrag = @onEndDrag;
            return ptr_of_this_method;
        }

        static object get_onScroll_7(ref object o)
        {
            return ((JEngine.UI.EventTriggerListener)o).onScroll;
        }

        static StackObject* CopyToStack_onScroll_7(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((JEngine.UI.EventTriggerListener)o).onScroll;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onScroll_7(ref object o, object v)
        {
            ((JEngine.UI.EventTriggerListener)o).onScroll = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)v;
        }

        static StackObject* AssignFromStack_onScroll_7(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData> @onScroll = (JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>)typeof(JEngine.UI.EventTriggerListener.EventHandle<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((JEngine.UI.EventTriggerListener)o).onScroll = @onScroll;
            return ptr_of_this_method;
        }



    }
}
