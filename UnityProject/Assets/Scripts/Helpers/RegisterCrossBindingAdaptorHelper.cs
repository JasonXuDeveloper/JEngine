using JEngine.Interface;
using ProjectAdapter;
using ProtoBuf;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace JEngine.Helper
{
    public class RegisterCrossBindingAdaptorHelper: IRegisterHelper
    {
        private static RegisterCrossBindingAdaptorHelper Instance;

        public static void HelperRegister(AppDomain appdomain)
        {
            if (Instance == null)
            {
                Instance = new RegisterCrossBindingAdaptorHelper();
            }
            Instance.Register(appdomain);
        }
        
        public void Register(AppDomain appdomain)
        {
            appdomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
            appdomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
            appdomain.RegisterCrossBindingAdaptor(new IAsyncStateMachineClassInheritanceAdaptor());
            appdomain.RegisterCrossBindingAdaptor(new ExceptionAdapter());
            appdomain.RegisterCrossBindingAdaptor(new IExtensibleAdapter()); 
            appdomain.RegisterCrossBindingAdaptor(new ExampleAPIAdapter());
        }
    }
}