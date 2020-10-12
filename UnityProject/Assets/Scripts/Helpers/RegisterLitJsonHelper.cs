using ILRuntime.Runtime.Enviorment;
using JEngine.AntiCheat;
using JEngine.Interface;
using LitJson;

namespace JEngine.Helper
{
    public class RegisterLitJsonHelper : IRegisterHelper
    {
        private static RegisterLitJsonHelper Instance;

        public static void HelperRegister(AppDomain appdomain)
        {
            if (Instance == null)
            {
                Instance = new RegisterLitJsonHelper();
            }
            Instance.Register(appdomain);
        }

        public void Register(AppDomain appdomain)
        {
            JsonMapper.RegisterExporter<float>((obj, writer) => writer.Write(obj.ToString())); //float->string
            JsonMapper.RegisterImporter<string, float>(input => float.Parse(input)); //string->float
            JsonMapper.RegisterExporter<JInt>((obj, writer) => writer.Write(obj.ToString())); //JInt->string
            JsonMapper.RegisterImporter<string, JInt>(input => new JInt(input)); //string->JInt
            JsonMapper.RegisterExporter<JBool>((obj, writer) => writer.Write(obj.ToString())); //JBool->string
            JsonMapper.RegisterImporter<string, JBool>(input => new JBool(input)); //string->JBool
            JsonMapper.RegisterExporter<JByte>((obj, writer) => writer.Write(obj.ToString())); //JByte->string
            JsonMapper.RegisterImporter<string, JByte>(input => new JByte(input)); //string->JByte
            JsonMapper.RegisterExporter<JLong>((obj, writer) => writer.Write(obj.ToString())); //JLong->string
            JsonMapper.RegisterImporter<string, JLong>(input => new JLong(input)); //string->JLong
            JsonMapper.RegisterExporter<JSByte>((obj, writer) => writer.Write(obj.ToString())); //JSByte->string
            JsonMapper.RegisterImporter<string, JSByte>(input => new JSByte(input)); //string->JSByte
            JsonMapper.RegisterExporter<JShort>((obj, writer) => writer.Write(obj.ToString())); //JShort->string
            JsonMapper.RegisterImporter<string, JShort>(input => new JShort(input)); //string->JShort
            JsonMapper.RegisterExporter<JUInt>((obj, writer) => writer.Write(obj.ToString())); //JUInt->string
            JsonMapper.RegisterImporter<string, JUInt>(input => new JUInt(input)); //string->JUInt
            JsonMapper.RegisterExporter<JULong>((obj, writer) => writer.Write(obj.ToString())); //JULong->string
            JsonMapper.RegisterImporter<string, JULong>(input => new JULong(input)); //string->JULong
            JsonMapper.RegisterExporter<JUShort>((obj, writer) => writer.Write(obj.ToString())); //JUShort->string
            JsonMapper.RegisterImporter<string, JUShort>(input => new JUShort(input)); //string->JUShort
            JsonMapper.RegisterExporter<JChar>((obj, writer) => writer.Write(obj.ToString())); //JChar->string
            JsonMapper.RegisterImporter<string, JChar>(input => new JChar(input)); //string->JChar
            JsonMapper.RegisterExporter<JString>((obj, writer) => writer.Write(obj.ToString())); //JString->string
            JsonMapper.RegisterImporter<string, JString>(input => new JString(input)); //string->JString
            JsonMapper.RegisterExporter<JFloat>((obj, writer) => writer.Write(obj.ToString())); //JFloat->string
            JsonMapper.RegisterImporter<string, JFloat>(input => new JFloat(input)); //string->JFloat
            JsonMapper.RegisterExporter<JDecimal>((obj, writer) => writer.Write(obj.ToString())); //JDecimal->string
            JsonMapper.RegisterImporter<string, JDecimal>(input => new JDecimal(input)); //string->JDecimal
            JsonMapper.RegisterExporter<JDouble>((obj, writer) => writer.Write(obj.ToString())); //JDouble->string
            JsonMapper.RegisterImporter<string, JDouble>(input => new JDouble(input)); //string->JDouble
        }
    }
}