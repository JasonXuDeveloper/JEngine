using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ILRuntime.Runtime.Enviorment;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace JEngine.Core
{
    public class RegisterAdaptors : IRegisterHelper
    {
        public void Register(AppDomain appdomain)
        {
            //自动注册一波，无需再手动添加了，如果想要性能也可以手动自己加
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            var ilRuntimeAssembly = typeof(AppDomain).Assembly;
            var crossBindingAdaptorType = typeof(CrossBindingAdaptor);
            //全部程序集
            foreach (var assembly in assemblies)
            {
                //跳过ILRuntime的程序集
                if (assembly == ilRuntimeAssembly) continue;
                var types = assembly.GetTypes();
                //全部类型
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(crossBindingAdaptorType))
                    {
                        object obj = Activator.CreateInstance(type);
                        CrossBindingAdaptor adaptor = obj as CrossBindingAdaptor;
                        if (adaptor == null)
                        {
                            continue;
                        }

                        try
                        {
                            appdomain.RegisterCrossBindingAdaptor(adaptor);
                        }
                        catch
                        {
                            //ignore
                        }
                    }
                }
            }
        }
    }
}