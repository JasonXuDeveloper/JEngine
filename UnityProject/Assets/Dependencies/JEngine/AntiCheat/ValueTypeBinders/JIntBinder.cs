using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace JEngine.AntiCheat.ValueTypeBinders
{
    public unsafe class JIntBinder : ValueTypeBinder<JInt>
    {
        public override void CopyValueTypeToStack(ref JInt ins, StackObject* ptr, IList<object> mStack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            v->Value = ins.ObscuredInt;
            v = ILIntepreter.Minus(ptr, 2);
            v->Value = ins.ObscuredKey;
            v = ILIntepreter.Minus(ptr, 3);
            v->Value = ins.OriginalValue;
        }

        public override void AssignFromStack(ref JInt ins, StackObject* ptr, IList<object> mStack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            ins.ObscuredInt = v->Value;
            v = ILIntepreter.Minus(ptr, 2);
            ins.ObscuredKey = v->Value;
            v = ILIntepreter.Minus(ptr, 3);
            ins.OriginalValue = v->Value;
        }

        public override void RegisterCLRRedirection(AppDomain appdomain)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static |
                                BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(JInt);
            MethodInfo[] methods = type.GetMethods(flag).Where(t => !t.IsGenericMethod).ToArray();
            
            args = new[]{typeof(Int32)};
            method = type.GetConstructor(flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(method, NewJInt);
            
            args = new[]{typeof(String)};
            method = type.GetConstructor(flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(method, NewJInt2);
            
            args = new[]{typeof(Int32)};
            method = methods.Single(t => t.Name.Equals("op_Implicit") && t.ReturnType == typeof(JInt) && t.CheckMethodParams(args));
            appdomain.RegisterCLRMethodRedirection(method, JInt_Implicit);
            
            args = new[] { typeof(JInt), typeof(JInt) };
            method = type.GetMethod("op_Addition", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(method, JInt_Add);
            
            args = new[] { typeof(JInt), typeof(int) };
            method = type.GetMethod("op_Addition", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(method, JInt_Add2);
        }
        
        StackObject* JInt_Implicit(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            StackObject* ptr;
            StackObject* ret = ILIntepreter.Minus(esp, 1);

            ptr = ILIntepreter.Minus(esp, 1);
            Int32 val = ptr->Value;


            var resultOfThisMethod = (JInt)val;

            return ILIntepreter.PushObject(ret, mStack, resultOfThisMethod);
        }

        StackObject* JInt_Add(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = ILIntepreter.Minus(esp, 2);
            var ptr = ILIntepreter.Minus(esp, 1);

            JInt left, right;
            ParseJInt(out right, intp, ptr, mStack);

            ptr = ILIntepreter.Minus(esp, 2);
            ParseJInt(out left, intp, ptr, mStack);

            var res = left + right;
            PushJInt(ref res, intp, ret, mStack);

            return ret + 1;
        }
        
        StackObject* JInt_Add2(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = ILIntepreter.Minus(esp, 2);
            var ptr = ILIntepreter.Minus(esp, 1);

            int right = ptr->Value;

            JInt left;
            ptr = ILIntepreter.Minus(esp, 2);
            ParseJInt(out left, intp, ptr, mStack);

            var res = left + right;
            PushJInt(ref res, intp, ret, mStack);

            return ret + 1;
        }
        
        StackObject* NewJInt(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            StackObject* ptr;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            ptr = ILIntepreter.Minus(esp, 1);
            int val = ptr->Value;


            var resultOfThisMethod = new JInt(val);

            if(!isNewObj)
            {
                ret--;
                WriteBackInstance(ret, mStack, ref resultOfThisMethod);
                return ret;
            }

            return ILIntepreter.PushObject(ret, mStack, resultOfThisMethod);
        }
        
        StackObject* NewJInt2(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            StackObject* ptr;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            ptr = ILIntepreter.Minus(esp, 1);
            String val = (String)typeof(String).CheckCLRTypes(StackObject.ToObject(ptr, domain, mStack), 0);
            intp.Free(ptr);

            var resultOfThisMethod = new JInt(val);

            if(!isNewObj)
            {
                ret--;
                WriteBackInstance(ret, mStack, ref resultOfThisMethod);
                return ret;
            }

            return ILIntepreter.PushObject(ret, mStack, resultOfThisMethod);
        }
        
        void WriteBackInstance(StackObject* ptr, IList<object> mStack, ref JInt instanceOfThisMethod)
        {
            ptr = ILIntepreter.GetObjectAndResolveReference(ptr);
            switch(ptr->ObjectType)
            {
                case ObjectTypes.Object:
                    {
                        mStack[ptr->Value] = instanceOfThisMethod;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var obj = mStack[ptr->Value];
                        if(obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)obj)[ptr->ValueLow] = instanceOfThisMethod;
                        }
                        else
                        {
                            var t = domain.GetType(obj.GetType()) as CLRType;
                            if (t != null) t.SetFieldValue(ptr->ValueLow, ref obj, instanceOfThisMethod);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = domain.GetType(ptr->Value);
                        if(t is ILType)
                        {
                            ((ILType)t).StaticInstance[ptr->ValueLow] = instanceOfThisMethod;
                        }
                        else
                        {
                            ((CLRType)t).SetStaticFieldValue(ptr->ValueLow, instanceOfThisMethod);
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                 {
                     if (mStack[ptr->Value] is JInt[] instanceOfArrayReference)
                         instanceOfArrayReference[ptr->ValueLow] = instanceOfThisMethod;
                 }
                    break;
            }
        }

        public static void ParseJInt(out JInt j, ILIntepreter intp, StackObject* ptr, IList<object> mStack)
        {
            var a = ILIntepreter.GetObjectAndResolveReference(ptr);
            if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var src = *(StackObject**)&a->Value;
                j.ObscuredInt = ILIntepreter.Minus(src, 1)->Value;
                j.ObscuredKey = ILIntepreter.Minus(src, 2)->Value;
                j.OriginalValue = ILIntepreter.Minus(src, 3)->Value;
                intp.FreeStackValueType(ptr);
            }
            else
            {
                j = (JInt)StackObject.ToObject(a, intp.AppDomain, mStack);
                intp.Free(ptr);
            }
        }

        
        public void PushJInt(ref JInt j, ILIntepreter intp, StackObject* ptr, IList<object> mStack)
        {
            intp.AllocValueType(ptr, CLRType);
            var dst = *((StackObject**)&ptr->Value);
            CopyValueTypeToStack(ref j, dst, mStack);
        }
    }
}