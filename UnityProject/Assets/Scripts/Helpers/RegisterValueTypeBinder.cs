using ILRuntime.Runtime.Enviorment;
using JEngine.AntiCheat;
using JEngine.AntiCheat.ValueTypeBinders;
using JEngine.Interface;
using UnityEngine;

namespace JEngine.Helper
{
    public class RegisterValueTypeBinderHelper: IRegisterHelper
    {
        private static RegisterValueTypeBinderHelper Instance;

        public static void HelperRegister(AppDomain appdomain)
        {
            if (Instance == null)
            {
                Instance = new RegisterValueTypeBinderHelper();
            }
            Instance.Register(appdomain);
        }
        
        public void Register(AppDomain appdomain)
        {
            appdomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
            appdomain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
            appdomain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());
            appdomain.RegisterValueTypeBinder(typeof(JInt), new JIntBinder());
        }
    }
}