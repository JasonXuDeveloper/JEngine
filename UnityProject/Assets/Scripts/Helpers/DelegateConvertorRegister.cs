using JEngine.Core;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace JEngine.Helper
{
    public class DelegateConvertorRegister: IRegisterHelper
    {
        public void Register(AppDomain appdomain)
        {
            /*  这里注册你所需要的DelegateConvertor，参考报错的内容黏贴就好，举个例子：
             *
                appdomain.DelegateManager.RegisterDelegateConvertor<System.Predicate<System.String>>(act =>
                {
                    return new System.Predicate<System.String>(obj =>
                    {
                        return ((System.Func<System.String, System.Boolean>)act)(obj);
                    });
                });
             *
             */
        }
    }
}