using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ILRuntime.Runtime.Intepreter;
using JEngine.Interface;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

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
            appdomain.DelegateManager.RegisterFunctionDelegate<String, Boolean>();
            appdomain.DelegateManager.RegisterFunctionDelegate<Object, String>();
            appdomain.DelegateManager.RegisterFunctionDelegate<ParameterInfo, String>();
            appdomain.DelegateManager.RegisterFunctionDelegate<Task<ILTypeInstance>>();
            appdomain.DelegateManager.RegisterFunctionDelegate<UnityEngine.Object, Boolean>();
            appdomain.DelegateManager.RegisterFunctionDelegate<Boolean>();
            appdomain.DelegateManager.RegisterFunctionDelegate<ParameterInfo, Type>();
            appdomain.DelegateManager.RegisterFunctionDelegate<Type, String>();
            appdomain.DelegateManager.RegisterFunctionDelegate<Object, Type>();
            appdomain.DelegateManager.RegisterFunctionDelegate<Object>();
            appdomain.DelegateManager.RegisterFunctionDelegate<List<ILTypeInstance>, Boolean>();
            appdomain.DelegateManager.RegisterFunctionDelegate<List<ILTypeInstance>, IEnumerable<ILTypeInstance>>();
            appdomain.DelegateManager.RegisterFunctionDelegate<float>();
            appdomain.DelegateManager.RegisterFunctionDelegate<Task>();
            appdomain.DelegateManager.RegisterFunctionDelegate<CoroutineAdapter.Adaptor, Boolean>();
            appdomain.DelegateManager.RegisterFunctionDelegate<CoroutineAdapter.Adaptor, String>();
            appdomain.DelegateManager.RegisterFunctionDelegate<MonoBehaviourAdapter.Adaptor, Boolean>();
            appdomain.DelegateManager.RegisterFunctionDelegate<ILTypeInstance, String>();
            appdomain.DelegateManager.RegisterFunctionDelegate<KeyValuePair<String, ILTypeInstance>, ILTypeInstance>();
            appdomain.DelegateManager.RegisterFunctionDelegate<KeyValuePair<String, ILTypeInstance>, Boolean>();
            appdomain.DelegateManager.RegisterFunctionDelegate<KeyValuePair<String, ILTypeInstance>, String>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Type, System.Type>();
            appdomain.DelegateManager.RegisterFunctionDelegate<global::MonoBehaviourAdapter.Adaptor, System.String>();
        }
    }
}