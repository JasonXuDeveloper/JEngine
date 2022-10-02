using System;
using JEngine.Interface;

namespace JEngine.Core
{
    public class RegisterClrBind: IRegisterHelper
    {
        public void Register(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
            //CLR绑定（有再去绑定）
            Type t = Type.GetType("ILRuntime.Runtime.Generated.CLRBindings");
            if (t != null)
            {
                t.GetMethod("Initialize")?.Invoke(null, new object[]
                {
                    appdomain
                });
            }
        }
    }
}