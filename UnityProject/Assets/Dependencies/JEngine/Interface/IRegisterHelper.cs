using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace JEngine.Interface
{
    public interface IRegisterHelper
    { 
        void Register(AppDomain appdomain);
    }
}