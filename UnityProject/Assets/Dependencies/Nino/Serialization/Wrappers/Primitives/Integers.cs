using System;
using System.Collections.Generic;

namespace Nino.Serialization
{
    internal class ByteWrapper : NinoWrapperBase<byte>
    {
        public override void Serialize(byte val, Writer writer)
        {
            writer.Write(val);
        }

        public override byte Deserialize(Reader reader)
        {
            return reader.ReadByte();
        }
    }

    internal class ByteArrWrapper : NinoWrapperBase<byte[]>
    {
        public override void Serialize(byte[] val, Writer writer)
        {
            writer.Write(val);
        }

        public override byte[] Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            return len != 0 ? reader.ReadBytes(len) : Array.Empty<byte>();
        }
    }

    internal class ByteListWrapper : NinoWrapperBase<List<byte>>
    {
        public override void Serialize(List<byte> val, Writer writer)
        {
            writer.Write(val);
        }

        public override List<byte> Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new List<byte>(len);
            //read item
            int i = 0;
            while (i++ < len)
            {
                arr.Add(reader.ReadByte());
            }

            return arr;
        }
    }

    internal class SByteWrapper : NinoWrapperBase<sbyte>
    {
        public override void Serialize(sbyte val, Writer writer)
        {
            writer.Write(val);
        }

        public override sbyte Deserialize(Reader reader)
        {
            return reader.ReadSByte();
        }
    }

    internal class SByteArrWrapper : NinoWrapperBase<sbyte[]>
    {
        public override unsafe void Serialize(sbyte[] val, Writer writer)
        {
            int len = val.Length;
            writer.CompressAndWrite(ref len);
            if (len > 0)
            {
                fixed (sbyte* ptr = val)
                {
                    writer.Write((byte*)ptr, ref len);
                }
            }
        }

        public override unsafe sbyte[] Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            sbyte[] arr;
            if (len == 0)
            {
                arr = Array.Empty<sbyte>();
            }
            else
            {
                arr = new sbyte[len];
                fixed (sbyte* arrPtr = arr)
                {
                    reader.ReadToBuffer((byte*)arrPtr, len);
                }
            }

            return arr;
        }
    }

    internal class SByteListWrapper : NinoWrapperBase<List<sbyte>>
    {
        public override void Serialize(List<sbyte> val, Writer writer)
        {
            writer.CompressAndWrite(val.Count);
            foreach (var v in val)
            {
                writer.Write(v);
            }
        }

        public override List<sbyte> Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new List<sbyte>(len);
            //read item
            int i = 0;
            while (i++ < len)
            {
                arr.Add(reader.ReadSByte());
            }
            return arr;
        }
    }

    internal class ShortWrapper : NinoWrapperBase<short>
    {
        public override void Serialize(short val, Writer writer)
        {
            writer.Write(val);
        }

        public override short Deserialize(Reader reader)
        {
            return reader.ReadInt16();
        }
    }

    internal class ShortArrWrapper : NinoWrapperBase<short[]>
    {
        public override unsafe void Serialize(short[] val, Writer writer)
        {
            int len = val.Length;
            writer.CompressAndWrite(ref len);
            if (len > 0)
            {
                len *= 2;
                fixed (short* ptr = val)
                {
                    writer.Write((byte*)ptr, ref len);
                }
            }
        }

        public override unsafe short[] Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            short[] arr;
            if (len == 0)
            {
                arr = Array.Empty<short>();
            }
            else
            {
                arr = new short[len];
                fixed (short* arrPtr = arr)
                {
                    reader.ReadToBuffer((byte*)arrPtr, len * 2);
                }
            }
            return arr;
        }
    }

    internal class ShortListWrapper : NinoWrapperBase<List<short>>
    {
        public override void Serialize(List<short> val, Writer writer)
        {
            writer.CompressAndWrite(val.Count);
            foreach (var v in val)
            {
                writer.Write(v);
            }
        }

        public override List<short> Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new List<short>(len);
            //read item
            int i = 0;
            while (i++ < len)
            {
                arr.Add(reader.ReadInt16());
            }
            return arr;
        }
    }

    internal class UShortWrapper : NinoWrapperBase<ushort>
    {
        public override void Serialize(ushort val, Writer writer)
        {
            writer.Write(val);
        }

        public override ushort Deserialize(Reader reader)
        {
            return reader.ReadUInt16();
        }
    }

    internal class UShortArrWrapper : NinoWrapperBase<ushort[]>
    {
        public override unsafe void Serialize(ushort[] val, Writer writer)
        {
            int len = val.Length;
            writer.CompressAndWrite(ref len);
            if (len > 0)
            {
                len *= 2;
                fixed (ushort* ptr = val)
                {
                    writer.Write((byte*)ptr, ref len);
                }
            }
        }

        public override unsafe ushort[] Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            ushort[] arr;
            if (len == 0)
            {
                arr = Array.Empty<ushort>();
            }
            else
            {
                arr = new ushort[len];
                fixed (ushort* arrPtr = arr)
                {
                    reader.ReadToBuffer((byte*)arrPtr, len * 2);
                }
            }
            return arr;
        }
    }

    internal class UShortListWrapper : NinoWrapperBase<List<ushort>>
    {
        public override void Serialize(List<ushort> val, Writer writer)
        {
            writer.CompressAndWrite(val.Count);
            foreach (var v in val)
            {
                writer.Write(v);
            }
        }

        public override List<ushort> Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new List<ushort>(len);
            //read item
            int i = 0;
            while (i++ < len)
            {
                arr.Add(reader.ReadUInt16());
            }
            return arr;
        }
    }

    internal class IntWrapper : NinoWrapperBase<int>
    {
        public override void Serialize(int val, Writer writer)
        {
            writer.CompressAndWrite(ref val);
        }

        public override int Deserialize(Reader reader)
        {
            return reader.DecompressAndReadNumber<int>();
        }
    }

    internal class IntArrWrapper : NinoWrapperBase<int[]>
    {
        public override void Serialize(int[] val, Writer writer)
        {
            writer.CompressAndWrite(val.Length);
            foreach (var v in val)
            {
                writer.CompressAndWrite(v);
            }
        }

        public override int[] Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new int[len];
            //read item
            int i = 0;
            while (i < len)
            {
                reader.DecompressAndReadNumber(ref arr[i++]);
            }
            return arr;
        }
    }

    internal class IntListWrapper : NinoWrapperBase<List<int>>
    {
        public override void Serialize(List<int> val, Writer writer)
        {
            writer.CompressAndWrite(val.Count);
            foreach (var v in val)
            {
                writer.CompressAndWrite(v);
            }
        }

        public override List<int> Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new List<int>(len);
            //read item
            int i = 0;
            while (i++ < len)
            {
                arr.Add(reader.DecompressAndReadNumber<int>());
            }
            return arr;
        }
    }

    internal class UIntWrapper : NinoWrapperBase<uint>
    {
        public override void Serialize(uint val, Writer writer)
        {
            writer.CompressAndWrite(ref val);
        }

        public override uint Deserialize(Reader reader)
        {
            return reader.DecompressAndReadNumber<uint>();
        }
    }

    internal class UIntArrWrapper : NinoWrapperBase<uint[]>
    {
        public override void Serialize(uint[] val, Writer writer)
        {
            writer.CompressAndWrite(val.Length);
            foreach (var v in val)
            {
                writer.CompressAndWrite(v);
            }
        }

        public override uint[] Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new uint[len];
            //read item
            int i = 0;
            while (i < len)
            {
                reader.DecompressAndReadNumber(ref arr[i++]);
            }
            return arr;
        }
    }

    internal class UIntListWrapper : NinoWrapperBase<List<uint>>
    {
        public override void Serialize(List<uint> val, Writer writer)
        {
            writer.CompressAndWrite(val.Count);
            foreach (var v in val)
            {
                writer.CompressAndWrite(v);
            }
        }

        public override List<uint> Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new List<uint>(len);
            //read item
            int i = 0;
            while (i++ < len)
            {
                arr.Add(reader.DecompressAndReadNumber<uint>());
            }
            return arr;
        }
    }

    internal class LongWrapper : NinoWrapperBase<long>
    {
        public override void Serialize(long val, Writer writer)
        {
            writer.CompressAndWrite(ref val);
        }

        public override long Deserialize(Reader reader)
        {
            return reader.DecompressAndReadNumber<long>();
        }
    }

    internal class LongArrWrapper : NinoWrapperBase<long[]>
    {
        public override void Serialize(long[] val, Writer writer)
        {
            writer.CompressAndWrite(val.Length);
            foreach (var v in val)
            {
                writer.CompressAndWrite(v);
            }
        }

        public override long[] Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new long[len];
            //read item
            int i = 0;
            while (i < len)
            {
                reader.DecompressAndReadNumber(ref arr[i++]);
            }
            return arr;
        }
    }

    internal class LongListWrapper : NinoWrapperBase<List<long>>
    {
        public override void Serialize(List<long> val, Writer writer)
        {
            writer.CompressAndWrite(val.Count);
            foreach (var v in val)
            {
                writer.CompressAndWrite(v);
            }
        }

        public override List<long> Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new List<long>(len);
            //read item
            int i = 0;
            while (i++ < len)
            {
                arr.Add(reader.DecompressAndReadNumber<long>());
            }
            return arr;
        }
    }

    internal class ULongWrapper : NinoWrapperBase<ulong>
    {
        public override void Serialize(ulong val, Writer writer)
        {
            writer.CompressAndWrite(ref val);
        }

        public override ulong Deserialize(Reader reader)
        {
            return reader.DecompressAndReadNumber<ulong>();
        }
    }

    internal class ULongArrWrapper : NinoWrapperBase<ulong[]>
    {
        public override void Serialize(ulong[] val, Writer writer)
        {
            writer.CompressAndWrite(val.Length);
            foreach (var v in val)
            {
                writer.CompressAndWrite(v);
            }
        }

        public override ulong[] Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new ulong[len];
            //read item
            int i = 0;
            while (i < len)
            {
                reader.DecompressAndReadNumber(ref arr[i++]);
            }
            return arr;
        }
    }

    internal class ULongListWrapper : NinoWrapperBase<List<ulong>>
    {
        public override void Serialize(List<ulong> val, Writer writer)
        {
            writer.CompressAndWrite(val.Count);
            foreach (var v in val)
            {
                writer.CompressAndWrite(v);
            }
        }

        public override List<ulong> Deserialize(Reader reader)
        {
            int len = reader.ReadLength();
            var arr = new List<ulong>(len);
            //read item
            int i = 0;
            while (i++ < len)
            {
                arr.Add(reader.DecompressAndReadNumber<ulong>());
            }
            return arr;
        }
    }
}