using System;
using LitJson;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace JEngine.Core
{
    public class ILRuntimeRegister : IRegisterHelper
    {
        public void Register(AppDomain appdomain)
        {
            appdomain.DelegateManager.RegisterDelegateConvertor<Action<JsonData>>(action =>
            {
                return new Action<JsonData>(a => { ((Action<JsonData>)action)(a); });
            });
            JsonMapper.RegisterILRuntimeCLRRedirection(appdomain);   
        }
    }
}