using System;
using System.Collections.Generic;

namespace Nino.Serialization
{
    internal class FloatWrapper : NinoWrapperBase<float>
    {
        public override void Serialize(float val, Writer writer)
        {
            writer.Write(val);
        }

        public override float Deserialize(Reader reader)
        {
            return reader.ReadSingle();
        }
    }

    internal class FloatArrWrapper : NinoWrapperBase<float[]>
    {
        public override unsafe void Serialize(float[] val, Writer writer)
        {
            int len = val.Length;
            writer.CompressAndWrite(ref len);
            if (len > 0)
            {
                len *= 4;
                fixed (float* ptr = val)
                {
                    writer.Write((byte*)ptr, ref len);
                }
            }
        }

        public override unsafe float[] Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            float[] arr;
            if (len == 0)
            {
                arr = Array.Empty<float>();
            }
            else
            {
                arr = new float[len];
                fixed (float* arrPtr = arr)
                {
                    reader.ReadToBuffer((byte*)arrPtr, len * 4);
                }
            }
            return arr;
        }
    }

    internal class FloatListWrapper : NinoWrapperBase<List<float>>
    {
        public override void Serialize(List<float> val, Writer writer)
        {
            writer.CompressAndWrite(val.Count);
            foreach (var v in val)
            {
                writer.Write(v);
            }
        }

        public override List<float> Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new List<float>(len);
            //read item
            for (int i = 0; i < len; i++)
            {
                arr.Add(reader.ReadSingle());
            }
            return arr;
        }
    }

    internal class DoubleWrapper : NinoWrapperBase<double>
    {
        public override void Serialize(double val, Writer writer)
        {
            writer.Write(val);
        }

        public override double Deserialize(Reader reader)
        {
            return reader.ReadDouble();
        }
    }

    internal class DoubleArrWrapper : NinoWrapperBase<double[]>
    {
        public override unsafe void Serialize(double[] val, Writer writer)
        {
            int len = val.Length;
            writer.CompressAndWrite(ref len);
            if (len > 0)
            {
                len *= 8;
                fixed (double* ptr = val)
                {
                    writer.Write((byte*)ptr, ref len);
                }
            }
        }

        public override unsafe double[] Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            double[] arr;
            if (len == 0)
            {
                arr = Array.Empty<double>();
            }
            else
            {
                arr = new double[len];
                fixed (double* arrPtr = arr)
                {
                    reader.ReadToBuffer((byte*)arrPtr, len * 8);
                }
            }
            return arr;
        }
    }

    internal class DoubleListWrapper : NinoWrapperBase<List<double>>
    {
        public override void Serialize(List<double> val, Writer writer)
        {
            writer.CompressAndWrite(val.Count);
            foreach (var v in val)
            {
                writer.Write(v);
            }
        }

        public override List<double> Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new List<double>(len);
            //read item
            for (int i = 0; i < len; i++)
            {
                arr.Add(reader.ReadDouble());
            }
            return arr;
        }
    }

    internal class DecimalWrapper : NinoWrapperBase<decimal>
    {
        public override void Serialize(decimal val, Writer writer)
        {
            writer.Write(val);
        }

        public override decimal Deserialize(Reader reader)
        {
            return reader.ReadDecimal();
        }
    }

    internal class DecimalArrWrapper : NinoWrapperBase<decimal[]>
    {
        public override unsafe void Serialize(decimal[] val, Writer writer)
        {
            int len = val.Length;
            writer.CompressAndWrite(ref len);
            if (len > 0)
            {
                len *= 16;
                fixed (decimal* ptr = val)
                {
                    writer.Write((byte*)ptr, ref len);
                }
            }
        }

        public override unsafe decimal[] Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            decimal[] arr;
            if (len == 0)
            {
                arr = Array.Empty<decimal>();
            }
            else
            {
                arr = new decimal[len];
                fixed (decimal* arrPtr = arr)
                {
                    reader.ReadToBuffer((byte*)arrPtr, len * 16);
                }
            }
            return arr;
        }
    }

    internal class DecimalListWrapper : NinoWrapperBase<List<decimal>>
    {
        public override void Serialize(List<decimal> val, Writer writer)
        {
            writer.CompressAndWrite(val.Count);
            foreach (var v in val)
            {
                writer.Write(v);
            }
        }

        public override List<decimal> Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new List<decimal>(len);
            //read item
            for (int i = 0; i < len; i++)
            {
                arr.Add(reader.ReadDecimal());
            }
            return arr;
        }
    }
}