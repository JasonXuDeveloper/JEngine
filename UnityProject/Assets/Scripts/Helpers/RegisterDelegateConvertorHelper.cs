using System;
using System.Threading;
using ILRuntime.Runtime.Intepreter;
using LitJson;
using UnityEngine.Events;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using Object = UnityEngine.Object;

namespace JEngine.Helper
{
    public class RegisterDelegateConvertorHelper
    {
        private static RegisterDelegateConvertorHelper Instance;

        public static void HelperRegister(AppDomain appdomain)
        {
            if (Instance == null)
            {
                Instance = new RegisterDelegateConvertorHelper();
            }
            Instance.Register(appdomain);
        }

        public void Register(AppDomain appdomain)
        {
            appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<System.String>>((act) =>
            {
                return new UnityEngine.Events.UnityAction<System.String>((arg0) =>
                {
                    ((Action<System.String>) act)(arg0);
                });
            });
            appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<System.Boolean>>((act) =>
            {
                return new UnityEngine.Events.UnityAction<System.Boolean>((arg0) =>
                {
                    ((Action<System.Boolean>) act)(arg0);
                });
            });
            appdomain.DelegateManager.RegisterDelegateConvertor<System.Threading.WaitCallback>((act) =>
            {
                return new System.Threading.WaitCallback((state) => { ((Action<System.Object>) act)(state); });
            });

            appdomain.DelegateManager.RegisterDelegateConvertor<UnityAction>(act =>
            {
                return new UnityAction(() => { ((Action) act)(); });
            });
            appdomain.DelegateManager.RegisterDelegateConvertor<UnityAction<Single>>(act =>
            {
                return new UnityAction<Single>(arg0 => { ((Action<Single>) act)(arg0); });
            });
            appdomain.DelegateManager.RegisterDelegateConvertor<System.UnhandledExceptionEventHandler>((act) =>
            {
                return new System.UnhandledExceptionEventHandler((sender, e) =>
                {
                    ((Action<System.Object, System.UnhandledExceptionEventArgs>) act)(sender, e);
                });
            });
            appdomain.DelegateManager.RegisterDelegateConvertor<Predicate<Object>>(act =>
            {
                return new Predicate<Object>(obj => { return ((Func<Object, Boolean>) act)(obj); });
            });
            appdomain.DelegateManager
                .RegisterDelegateConvertor<Predicate<ILTypeInstance>>(act =>
                {
                    return new Predicate<ILTypeInstance>(obj =>
                    {
                        return ((Func<ILTypeInstance, Boolean>) act)(obj);
                    });
                });
            appdomain.DelegateManager.RegisterDelegateConvertor<UnityAction<Int32>>(act =>
            {
                return new UnityAction<Int32>(arg0 => { ((Action<Int32>) act)(arg0); });
            });
            appdomain.DelegateManager.RegisterDelegateConvertor<Action<JsonData>>(action =>
            {
                return new Action<JsonData>(a => { ((Action<JsonData>) action)(a); });
            });
            appdomain.DelegateManager.RegisterDelegateConvertor<UnityAction>(act =>
            {
                return new UnityAction(async () => { ((Action) act)(); });
            });
            appdomain.DelegateManager.RegisterDelegateConvertor<ThreadStart>(act =>
            {
                return new ThreadStart(() => { ((Action) act)(); });
            });
            appdomain.DelegateManager.RegisterDelegateConvertor<System.Predicate<global::CoroutineAdapter.Adaptor>>(
                (act) =>
                {
                    return new System.Predicate<global::CoroutineAdapter.Adaptor>((obj) =>
                    {
                        return ((Func<global::CoroutineAdapter.Adaptor, System.Boolean>) act)(obj);
                    });
                });
            appdomain.DelegateManager.RegisterDelegateConvertor<System.Predicate<global::MonoBehaviourAdapter.Adaptor>>((act) =>
            {
                return new System.Predicate<global::MonoBehaviourAdapter.Adaptor>((obj) =>
                {
                    return ((Func<global::MonoBehaviourAdapter.Adaptor, System.Boolean>)act)(obj);
                });
            });
            appdomain.DelegateManager.RegisterDelegateConvertor<System.Timers.ElapsedEventHandler>((act) =>
            {
                return new System.Timers.ElapsedEventHandler((sender, e) =>
                {
                    ((Action<System.Object, System.Timers.ElapsedEventArgs>)act)(sender, e);
                });
            });
        }
    }
}