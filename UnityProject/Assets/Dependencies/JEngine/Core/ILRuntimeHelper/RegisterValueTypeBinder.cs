#if INIT_JE
using ILRuntime.Runtime.Enviorment;
using JEngine.AntiCheat;
using JEngine.AntiCheat.ValueTypeBinders;
using JEngine.Interface;
using UnityEngine;

namespace JEngine.Core
{
    public class RegisterValueTypeBinder: IRegisterHelper
    {
        public void Register(AppDomain appdomain)
        {
            appdomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
            appdomain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
            appdomain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());
            appdomain.RegisterValueTypeBinder(typeof(JInt), new JIntBinder());
        }
    }
}
#endif