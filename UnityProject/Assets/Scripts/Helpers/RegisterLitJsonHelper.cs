using ILRuntime.Runtime.Enviorment;
using JEngine.AntiCheat;
using JEngine.Core;
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
            JsonMapper.RegisterExporter<float>((obj, writer) => writer.Write(double.Parse(obj.ToString()))); //float->double
            JsonMapper.RegisterImporter<string, float>(float.Parse); //string->float
            JsonMapper.RegisterImporter<double, float>(input => (float)input); //string->float
            
            //给BindableProperty注册转换
            //这里注册了几个简单的类型
            JsonMapper.RegisterExporter<BindableProperty<object>>((obj, writer) => writer.Write(obj.ToString()));
            JsonMapper.RegisterImporter<string, BindableProperty<object>>(input => new BindableProperty<object>(input));
            JsonMapper.RegisterExporter<BindableProperty<byte>>((obj, writer) => writer.Write(obj.ToString()));
            JsonMapper.RegisterImporter<string, BindableProperty<byte>>(input => new BindableProperty<byte>(byte.Parse(input)));
            JsonMapper.RegisterExporter<BindableProperty<sbyte>>((obj, writer) => writer.Write(obj.ToString()));
            JsonMapper.RegisterImporter<string, BindableProperty<sbyte>>(input => new BindableProperty<sbyte>(sbyte.Parse(input)));
            JsonMapper.RegisterExporter<BindableProperty<int>>((obj, writer) => writer.Write(obj.ToString()));
            JsonMapper.RegisterImporter<string, BindableProperty<int>>(input => new BindableProperty<int>(int.Parse(input)));
            JsonMapper.RegisterExporter<BindableProperty<uint>>((obj, writer) => writer.Write(obj.ToString()));
            JsonMapper.RegisterImporter<string, BindableProperty<uint>>(input => new BindableProperty<uint>(uint.Parse(input)));
            JsonMapper.RegisterExporter<BindableProperty<bool>>((obj, writer) => writer.Write(obj.ToString()));
            JsonMapper.RegisterImporter<string, BindableProperty<bool>>(input => new BindableProperty<bool>(bool.Parse(input)));
            JsonMapper.RegisterExporter<BindableProperty<short>>((obj, writer) => writer.Write(obj.ToString()));
            JsonMapper.RegisterImporter<string, BindableProperty<short>>(input => new BindableProperty<short>(short.Parse(input)));
            JsonMapper.RegisterExporter<BindableProperty<ushort>>((obj, writer) => writer.Write(obj.ToString()));
            JsonMapper.RegisterImporter<string, BindableProperty<ushort>>(input => new BindableProperty<ushort>(ushort.Parse(input)));
            JsonMapper.RegisterExporter<BindableProperty<long>>((obj, writer) => writer.Write(obj.ToString()));
            JsonMapper.RegisterImporter<string, BindableProperty<long>>(input => new BindableProperty<long>(long.Parse(input)));
            JsonMapper.RegisterExporter<BindableProperty<ulong>>((obj, writer) => writer.Write(obj.ToString()));
            JsonMapper.RegisterImporter<string, BindableProperty<ulong>>(input => new BindableProperty<ulong>(ulong.Parse(input)));
            JsonMapper.RegisterExporter<BindableProperty<char>>((obj, writer) => writer.Write(obj.ToString()));
            JsonMapper.RegisterImporter<string, BindableProperty<char>>(input => new BindableProperty<char>(char.Parse(input)));
            JsonMapper.RegisterExporter<BindableProperty<double>>((obj, writer) => writer.Write(obj.ToString()));
            JsonMapper.RegisterImporter<string, BindableProperty<double>>(input => new BindableProperty<double>(double.Parse(input)));
            JsonMapper.RegisterExporter<BindableProperty<float>>((obj, writer) => writer.Write(obj.ToString()));
            JsonMapper.RegisterImporter<string, BindableProperty<float>>(input => new BindableProperty<float>(float.Parse(input)));
            JsonMapper.RegisterExporter<BindableProperty<decimal>>((obj, writer) => writer.Write(obj.ToString()));
            JsonMapper.RegisterImporter<string, BindableProperty<decimal>>(input => new BindableProperty<decimal>(decimal.Parse(input)));
            
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