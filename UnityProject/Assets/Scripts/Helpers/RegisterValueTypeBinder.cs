using ILRuntime.Runtime.Enviorment;
using JEngine.Core;

namespace JEngine.Helper
{
    public class ValueTypeBinderRegister: IRegisterHelper
    {
        public void Register(AppDomain appdomain)
        {
            /*  这里注册你所需要的ValueTypeBinder，值类型绑定的话得自己写，写了后可以注册，举个例子：
             *
                appdomain.RegisterValueTypeBinder(typeof(UnityEngine.Vector3), new Vector3Binder());
             *
             */
        }
    }
}