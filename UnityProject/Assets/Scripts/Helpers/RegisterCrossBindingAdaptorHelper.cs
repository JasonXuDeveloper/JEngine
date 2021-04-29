using System;
using System.Linq;
using System.Reflection;
using ILRuntime.Runtime.Enviorment;
using JEngine.Interface;
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
            //自动注册一波，无需再手动添加了，如果想要性能也可以手动自己加
            Assembly assembly = typeof(InitJEngine).Assembly;
            foreach (Type type in assembly.GetTypes().ToList().FindAll(t=>t.IsSubclassOf(typeof(CrossBindingAdaptor))))
            {
                object obj = Activator.CreateInstance(type);
                CrossBindingAdaptor adaptor = obj as CrossBindingAdaptor;
                if (adaptor == null)
                {
                    continue;
                }
                appdomain.RegisterCrossBindingAdaptor(adaptor);
            }
        }
    }
}