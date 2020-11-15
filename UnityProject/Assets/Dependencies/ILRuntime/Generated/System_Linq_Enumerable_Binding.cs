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
    unsafe class System_Linq_Enumerable_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Linq.Enumerable);
            Dictionary<string, List<MethodInfo>> genericMethods = new Dictionary<string, List<MethodInfo>>();
            List<MethodInfo> lst = null;                    
            foreach(var m in type.GetMethods())
            {
                if(m.IsGenericMethodDefinition)
                {
                    if (!genericMethods.TryGetValue(m.Name, out lst))
                    {
                        lst = new List<MethodInfo>();
                        genericMethods[m.Name] = lst;
                    }
                    lst.Add(m);
                }
            }
            args = new Type[]{typeof(System.Object)};
            if (genericMethods.TryGetValue("ToList", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.List<System.Object>), typeof(System.Collections.Generic.IEnumerable<System.Object>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ToList_0);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Object), typeof(System.Type)};
            if (genericMethods.TryGetValue("Select", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<System.Type>), typeof(System.Collections.Generic.IEnumerable<System.Object>), typeof(System.Func<System.Object, System.Type>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Select_1);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            if (genericMethods.TryGetValue("SelectMany", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Collections.Generic.IEnumerable<System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>>), typeof(System.Func<System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>, System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, SelectMany_2);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            if (genericMethods.TryGetValue("ToList", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ToList_3);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Reflection.ParameterInfo), typeof(System.Type)};
            if (genericMethods.TryGetValue("Select", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<System.Type>), typeof(System.Collections.Generic.IEnumerable<System.Reflection.ParameterInfo>), typeof(System.Func<System.Reflection.ParameterInfo, System.Type>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Select_4);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(global::MonoBehaviourAdapter.Adaptor)};
            if (genericMethods.TryGetValue("ToList", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.List<global::MonoBehaviourAdapter.Adaptor>), typeof(System.Collections.Generic.IEnumerable<global::MonoBehaviourAdapter.Adaptor>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ToList_5);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Collections.Generic.KeyValuePair<System.String, ILRuntime.Runtime.Intepreter.ILTypeInstance>)};
            if (genericMethods.TryGetValue("ElementAt", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.KeyValuePair<System.String, ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.String, ILRuntime.Runtime.Intepreter.ILTypeInstance>>), typeof(System.Int32)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ElementAt_6);

                        break;
                    }
                }
            }


        }


        static StackObject* ToList_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<System.Object> @source = (System.Collections.Generic.IEnumerable<System.Object>)typeof(System.Collections.Generic.IEnumerable<System.Object>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.ToList<System.Object>(@source);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Select_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<System.Object, System.Type> @selector = (System.Func<System.Object, System.Type>)typeof(System.Func<System.Object, System.Type>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<System.Object> @source = (System.Collections.Generic.IEnumerable<System.Object>)typeof(System.Collections.Generic.IEnumerable<System.Object>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Select<System.Object, System.Type>(@source, @selector);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* SelectMany_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>, System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>> @selector = (System.Func<System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>, System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>>)typeof(System.Func<System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>, System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>> @source = (System.Collections.Generic.IEnumerable<System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>>)typeof(System.Collections.Generic.IEnumerable<System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.SelectMany<System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>, ILRuntime.Runtime.Intepreter.ILTypeInstance>(@source, @selector);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* ToList_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance> @source = (System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.ToList<ILRuntime.Runtime.Intepreter.ILTypeInstance>(@source);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Select_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<System.Reflection.ParameterInfo, System.Type> @selector = (System.Func<System.Reflection.ParameterInfo, System.Type>)typeof(System.Func<System.Reflection.ParameterInfo, System.Type>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<System.Reflection.ParameterInfo> @source = (System.Collections.Generic.IEnumerable<System.Reflection.ParameterInfo>)typeof(System.Collections.Generic.IEnumerable<System.Reflection.ParameterInfo>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Select<System.Reflection.ParameterInfo, System.Type>(@source, @selector);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* ToList_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<global::MonoBehaviourAdapter.Adaptor> @source = (System.Collections.Generic.IEnumerable<global::MonoBehaviourAdapter.Adaptor>)typeof(System.Collections.Generic.IEnumerable<global::MonoBehaviourAdapter.Adaptor>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.ToList<global::MonoBehaviourAdapter.Adaptor>(@source);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* ElementAt_6(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @index = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.String, ILRuntime.Runtime.Intepreter.ILTypeInstance>> @source = (System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.String, ILRuntime.Runtime.Intepreter.ILTypeInstance>>)typeof(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.String, ILRuntime.Runtime.Intepreter.ILTypeInstance>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.ElementAt<System.Collections.Generic.KeyValuePair<System.String, ILRuntime.Runtime.Intepreter.ILTypeInstance>>(@source, @index);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }



    }
}
