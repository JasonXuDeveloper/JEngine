using System;
using System.Collections.Generic;
using ILRuntime.Runtime.Intepreter;
using JEngine.Interface;
using UnityEngine;
using UnityEngine.EventSystems;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using Object = UnityEngine.Object;

namespace JEngine.Helper
{
    public class RegisterMethodDelegateHelper: IRegisterHelper
    {
        private static RegisterMethodDelegateHelper Instance;

        public static void HelperRegister(AppDomain appdomain)
        {
            if (Instance == null)
            {
                Instance = new RegisterMethodDelegateHelper();
            }
            Instance.Register(appdomain);
        }
        
        public void Register(AppDomain appdomain)
        {
            appdomain.DelegateManager.RegisterMethodDelegate<System.Object, WebSocketSharp.MessageEventArgs>();
            appdomain.DelegateManager.RegisterMethodDelegate<System.Object, System.Timers.ElapsedEventArgs>();
            appdomain.DelegateManager.RegisterMethodDelegate<System.Object[]>();
            appdomain.DelegateManager.RegisterMethodDelegate<JEngine.Net.SocketIOEvent>();
            appdomain.DelegateManager.RegisterMethodDelegate<libx.AssetRequest>();
            appdomain.DelegateManager.RegisterMethodDelegate<Object>();
            appdomain.DelegateManager.RegisterMethodDelegate<System.Object>();
            appdomain.DelegateManager
                .RegisterFunctionDelegate<ILTypeInstance, Boolean>();
            appdomain.DelegateManager.RegisterMethodDelegate<List<Object>>();
            appdomain.DelegateManager
                .RegisterMethodDelegate<IDictionary<String, UnityEngine.Object>>();
            appdomain.DelegateManager.RegisterMethodDelegate<Boolean>();
            appdomain.DelegateManager.RegisterMethodDelegate<Single>();
            appdomain.DelegateManager.RegisterMethodDelegate<System.Object, System.UnhandledExceptionEventArgs>();
            appdomain.DelegateManager.RegisterMethodDelegate<System.Boolean>();
            appdomain.DelegateManager.RegisterMethodDelegate<Boolean, GameObject>();
            appdomain.DelegateManager.RegisterMethodDelegate<Int32, Int32>();
            appdomain.DelegateManager.RegisterMethodDelegate<String>();
            appdomain.DelegateManager.RegisterMethodDelegate<ILTypeInstance>();
            appdomain.DelegateManager.RegisterMethodDelegate<GameObject>();
            appdomain.DelegateManager.RegisterMethodDelegate<UIBehaviour, Object>();
            appdomain.DelegateManager.RegisterMethodDelegate<Transform, Object>();
            appdomain.DelegateManager.RegisterMethodDelegate<GameObject>();
            appdomain.DelegateManager.RegisterMethodDelegate<Int32>();
            appdomain.DelegateManager.RegisterMethodDelegate<GameObject, Action>();
            appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.GameObject, UnityEngine.EventSystems.PointerEventData>();

        }
    }
}