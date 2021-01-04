using System;
using JEngine.Interface;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using Object = UnityEngine.Object;

namespace JEngine.Helper
{
    public class RegisterFunctionDelegateHelper : IRegisterHelper
    {
        private static RegisterFunctionDelegateHelper Instance;

        public static void HelperRegister(AppDomain appdomain)
        {
            if (Instance == null)
            {
                Instance = new RegisterFunctionDelegateHelper();
            }
            Instance.Register(appdomain);
        }
        
        public void Register(AppDomain appdomain)
        {
            appdomain.DelegateManager.RegisterFunctionDelegate<System.String, System.Boolean>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Object, System.String>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Reflection.ParameterInfo, System.String>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Threading.Tasks.Task<ILRuntime.Runtime.Intepreter.ILTypeInstance>>();
            appdomain.DelegateManager.RegisterFunctionDelegate<Object, Boolean>();
            appdomain.DelegateManager.RegisterFunctionDelegate<Boolean>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Reflection.ParameterInfo, System.Type>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Type, System.String>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Object, System.Type>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Object>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>, System.Boolean>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>, System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>>();
            appdomain.DelegateManager.RegisterFunctionDelegate<float>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Threading.Tasks.Task>();
            appdomain.DelegateManager.RegisterFunctionDelegate<global::CoroutineAdapter.Adaptor, System.Boolean>();
            appdomain.DelegateManager.RegisterFunctionDelegate<global::CoroutineAdapter.Adaptor, System.String>();
            appdomain.DelegateManager.RegisterFunctionDelegate<global::MonoBehaviourAdapter.Adaptor, System.Boolean>();
            appdomain.DelegateManager.RegisterFunctionDelegate<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.String>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Collections.Generic.KeyValuePair<System.String, ILRuntime.Runtime.Intepreter.ILTypeInstance>, ILRuntime.Runtime.Intepreter.ILTypeInstance>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Collections.Generic.KeyValuePair<System.String, ILRuntime.Runtime.Intepreter.ILTypeInstance>, System.Boolean>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Collections.Generic.KeyValuePair<System.String, ILRuntime.Runtime.Intepreter.ILTypeInstance>, System.String>();
        }
    }
}