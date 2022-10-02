using JEngine.Interface;

namespace JEngine.Core
{
    public class RegisterLibraries: IRegisterHelper
    {
        public void Register(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
            //Protobuf适配
            ProtoBuf.PType.RegisterILRuntimeCLRRedirection(appdomain);
            //LitJson适配
            LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(appdomain);
            //Nino适配
            Nino.Serialization.ILRuntimeResolver.RegisterILRuntimeClrRedirection(appdomain);
        }
    }
}