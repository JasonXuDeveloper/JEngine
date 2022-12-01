using System;
using System.Collections.Generic;

namespace Nino.Serialization
{
    internal class BoolWrapper : NinoWrapperBase<bool>
    {
        public override void Serialize(bool val, Writer writer)
        {
            writer.Write(val);
        }

        public override bool Deserialize(Reader reader)
        {
            return reader.ReadBool();
        }
    }

    internal class BoolArrWrapper : NinoWrapperBase<bool[]>
    {
        public override unsafe void Serialize(bool[] val, Writer writer)
        {
            int len = val.Length;
            writer.CompressAndWrite(ref len);
            if (len > 0)
            {
                fixed (bool* ptr = val)
                {
                    writer.Write((byte*)ptr, ref len);
                }
            }
        }

        public override unsafe bool[] Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            bool[] arr;
            if (len == 0)
            {
                arr = Array.Empty<bool>();
            }
            else
            {
                arr = new bool[len];
                fixed (bool* arrPtr = arr)
                {
                    reader.ReadToBuffer((byte*)arrPtr, len);
                }
            }

            return arr;
        }
    }

    internal class BoolListWrapper : NinoWrapperBase<List<bool>>
    {
        public override void Serialize(List<bool> val, Writer writer)
        {
            writer.CompressAndWrite(val.Count);
            foreach (var v in val)
            {
                writer.Write(v);
            }
        }

        public override List<bool> Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new List<bool>(len);
            //read item
            int i = 0;
            while (i++ < len)
            {
                arr.Add(reader.ReadBool());
            }
            return arr;
        }
    }

    internal class CharWrapper : NinoWrapperBase<char>
    {
        public override void Serialize(char val, Writer writer)
        {
            writer.Write(val);
        }

        public override char Deserialize(Reader reader)
        {
            return reader.ReadChar();
        }
    }

    internal class CharArrWrapper : NinoWrapperBase<char[]>
    {
        public override unsafe void Serialize(char[] val, Writer writer)
        {
            int len = val.Length;
            writer.CompressAndWrite(ref len);
            if (len > 0)
            {
                len *= 2;
                fixed (char* ptr = val)
                {
                    writer.Write((byte*)ptr, ref len);
                }
            }
        }

        public override unsafe char[] Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            char[] arr;
            if (len == 0)
            {
                arr = Array.Empty<char>();
            }
            else
            {
                arr = new char[len];
                fixed (char* arrPtr = arr)
                {
                    reader.ReadToBuffer((byte*)arrPtr, len * 2);
                }
            }
            return arr;
        }
    }

    internal class CharListWrapper : NinoWrapperBase<List<char>>
    {
        public override void Serialize(List<char> val, Writer writer)
        {
            writer.CompressAndWrite(val.Count);
            foreach (var v in val)
            {
                writer.Write(v);
            }
        }

        public override List<char> Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new List<char>(len);
            //read item
            int i = 0;
            while (i++ < len)
            {
                arr.Add(reader.ReadChar());
            }
            return arr;
        }
    }

    internal class StringWrapper : NinoWrapperBase<string>
    {
        public override void Serialize(string val, Writer writer)
        {
            writer.Write(val);
        }

        public override string Deserialize(Reader reader)
        {
            return reader.ReadString();
        }
    }

    internal class StringArrWrapper : NinoWrapperBase<string[]>
    {
        public override void Serialize(string[] val, Writer writer)
        {
            writer.CompressAndWrite(val.Length);
            foreach (var v in val)
            {
                writer.Write(v);
            }
        }

        public override string[] Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new string[len];
            int i = 0;
            while (i < len)
            {
                arr[i++] = reader.ReadString();
            }
            return arr;
        }
    }

    internal class StringListWrapper : NinoWrapperBase<List<string>>
    {
        public override void Serialize(List<string> val, Writer writer)
        {
            writer.CompressAndWrite(val.Count);
            foreach (var v in val)
            {
                writer.Write(v);
            }
        }

        public override List<string> Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new List<string>(len);
            //read item
            int i = 0;
            while (i++ < len)
            {
                arr.Add(reader.ReadString());
            }
            return arr;
        }
    }

    internal class DateTimeWrapper : NinoWrapperBase<DateTime>
    {
        public override void Serialize(DateTime val, Writer writer)
        {
            writer.Write(val);
        }

        public override DateTime Deserialize(Reader reader)
        {
            return reader.ReadDateTime();
        }
    }

    internal class DateTimeArrWrapper : NinoWrapperBase<DateTime[]>
    {
        public override void Serialize(DateTime[] val, Writer writer)
        {
            writer.CompressAndWrite(val.Length);
            foreach (var v in val)
            {
                writer.Write(v);
            }
        }

        public override DateTime[] Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new DateTime[len];
            int i = 0;
            while (i < len)
            {
                arr[i++] = reader.ReadDateTime();
            }
            return arr;
        }
    }

    internal class DateTimeListWrapper : NinoWrapperBase<List<DateTime>>
    {
        public override void Serialize(List<DateTime> val, Writer writer)
        {
            writer.CompressAndWrite(val.Count);
            foreach (var v in val)
            {
                writer.Write(v);
            }
        }

        public override List<DateTime> Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new List<DateTime>(len);
            int i = 0;
            while (i++ < len)
            {
                arr.Add(reader.ReadDateTime());
            }
            return arr;
        }
    }

    internal class GenericWrapper<T> : NinoWrapperBase<T>
    {
        public Serializer.ImporterDelegate<T> Importer;
        public Deserializer.ExporterDelegate<T> Exporter;
        
        public override void Serialize(T val, Writer writer)
        {
            if(Importer == null)
                throw new InvalidOperationException($"Importer is null for type: {typeof(T)}");
            Importer(val, writer);
        }

        public override T Deserialize(Reader reader)
        {
            if(Exporter == null)
                throw new InvalidOperationException($"Exporter is null for type: {typeof(T)}");
            return Exporter(reader);
        }
    }
}