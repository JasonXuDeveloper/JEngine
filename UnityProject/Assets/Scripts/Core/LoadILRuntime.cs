using System;
using System.Threading;
using JEngine.Interface;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public static class LoadILRuntime
{
    public static void InitializeILRuntime(AppDomain appdomain)
    {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
        //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
        appdomain.UnityMainThreadID = Thread.CurrentThread.ManagedThreadId;
        appdomain.DebugService.StartDebugService(56000);
#endif
#if INIT_JE
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        var helperInterface = typeof(IRegisterHelper);
        //全部程序集
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();
            //全部类型
            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();
                //继承接口
                foreach (var @interface in interfaces)
                {
                    if (@interface == helperInterface)
                    {
                        //注册
                        var helper = (IRegisterHelper)Activator.CreateInstance(type);
                        helper.Register(appdomain);
                    }
                }
            }
        }
        //CLR绑定（有再去绑定），这个要在最后
        Type t = Type.GetType("ILRuntime.Runtime.Generated.CLRBindings");
        if (t != null)
        {
            t.GetMethod("Initialize")?.Invoke(null, new object[]
            {
                appdomain
            });
        }
#endif
    }
}
