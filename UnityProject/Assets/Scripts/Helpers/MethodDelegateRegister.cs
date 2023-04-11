using JEngine.Core;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace JEngine.Helper
{
    public class MethodDelegateRegister : IRegisterHelper
    {
        public void Register(AppDomain appdomain)
        {
            /*  这里注册你所需要的MethodDelegate，参考报错的内容黏贴就好，举个例子：
             *
                appdomain.DelegateManager.RegisterMethodDelegate<object>();
             *
             */
        }
    }
}