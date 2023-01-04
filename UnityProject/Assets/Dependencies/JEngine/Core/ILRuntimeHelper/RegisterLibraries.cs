namespace JEngine.Core
{
    public class RegisterLibraries : IRegisterHelper
    {
        public void Register(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
#if INIT_JE
            //Protobuf适配
            ProtoBuf.PType.RegisterILRuntimeCLRRedirection(appdomain);
#endif
        }
    }
}