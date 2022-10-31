using JEngine.Interface;

namespace JEngine.Core
{
    public class RegisterLibraries : IRegisterHelper
    {
        public void Register(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
#if INIT_JE
            //Protobuf适配
            ProtoBuf.PType.RegisterILRuntimeCLRRedirection(appdomain);
            //LitJson适配
            LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(appdomain);
#if ILRuntime
            //Nino适配
            Nino.Serialization.ILRuntimeResolver.RegisterILRuntimeClrRedirection(appdomain);
#endif
#endif
        }
    }
}