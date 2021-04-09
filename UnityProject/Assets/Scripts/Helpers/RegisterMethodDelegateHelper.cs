using System;
using System.Collections.Generic;
using System.Timers;
using ILRuntime.Runtime.Intepreter;
using JEngine.Interface;
using JEngine.Net;
using libx;
using UnityEngine;
using UnityEngine.EventSystems;
using WebSocketSharp;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using Object = System.Object;

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
            appdomain.DelegateManager.RegisterMethodDelegate<object>();
            appdomain.DelegateManager.RegisterMethodDelegate<object,object>();
            appdomain.DelegateManager.RegisterMethodDelegate<object,object,object>();
            appdomain.DelegateManager.RegisterMethodDelegate<object,object,object,object>();
            appdomain.DelegateManager.RegisterMethodDelegate<System.Int64>();
            appdomain.DelegateManager.RegisterMethodDelegate<Object, MessageEventArgs>();
            appdomain.DelegateManager.RegisterMethodDelegate<Object, ElapsedEventArgs>();
            appdomain.DelegateManager.RegisterMethodDelegate<Object[]>();
            appdomain.DelegateManager.RegisterMethodDelegate<SocketIOEvent>();
            appdomain.DelegateManager.RegisterMethodDelegate<AssetRequest>();
            appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Object>();
            appdomain.DelegateManager.RegisterMethodDelegate<Object>();
            appdomain.DelegateManager
                .RegisterFunctionDelegate<ILTypeInstance, Boolean>();
            appdomain.DelegateManager.RegisterMethodDelegate<List<UnityEngine.Object>>();
            appdomain.DelegateManager
                .RegisterMethodDelegate<IDictionary<String, UnityEngine.Object>>();
            appdomain.DelegateManager.RegisterMethodDelegate<Boolean>();
            appdomain.DelegateManager.RegisterMethodDelegate<Single>();
            appdomain.DelegateManager.RegisterMethodDelegate<Object, UnhandledExceptionEventArgs>();
            appdomain.DelegateManager.RegisterMethodDelegate<Boolean>();
            appdomain.DelegateManager.RegisterMethodDelegate<Boolean, GameObject>();
            appdomain.DelegateManager.RegisterMethodDelegate<Int32, Int32>();
            appdomain.DelegateManager.RegisterMethodDelegate<String>();
            appdomain.DelegateManager.RegisterMethodDelegate<ILTypeInstance>();
            appdomain.DelegateManager.RegisterMethodDelegate<GameObject>();
            appdomain.DelegateManager.RegisterMethodDelegate<UIBehaviour, UnityEngine.Object>();
            appdomain.DelegateManager.RegisterMethodDelegate<Transform, UnityEngine.Object>();
            appdomain.DelegateManager.RegisterMethodDelegate<GameObject>();
            appdomain.DelegateManager.RegisterMethodDelegate<Int32>();
            appdomain.DelegateManager.RegisterMethodDelegate<GameObject, Action>();
            appdomain.DelegateManager.RegisterMethodDelegate<GameObject, PointerEventData>();

        }
    }
}