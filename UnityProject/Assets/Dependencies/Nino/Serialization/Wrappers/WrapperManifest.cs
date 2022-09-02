using System;
using Nino.Shared.Util;
using System.Collections.Generic;

namespace Nino.Serialization
{
    public static class WrapperManifest
    {
        private static readonly Dictionary<int, INinoWrapper> Wrappers = new Dictionary<int, INinoWrapper>(300)
        {
            { typeof(byte).GetTypeHashCode(), new ByteWrapper() },
            { typeof(byte[]).GetTypeHashCode(), new ByteArrWrapper() },
            { typeof(List<byte>).GetTypeHashCode(), new ByteListWrapper() },
            { typeof(sbyte).GetTypeHashCode(), new SByteWrapper() },
            { typeof(sbyte[]).GetTypeHashCode(), new SByteArrWrapper() },
            { typeof(List<sbyte>).GetTypeHashCode(), new SByteListWrapper() },
            { typeof(short).GetTypeHashCode(), new ShortWrapper() },
            { typeof(short[]).GetTypeHashCode(), new ShortArrWrapper() },
            { typeof(List<short>).GetTypeHashCode(), new ShortListWrapper() },
            { typeof(ushort).GetTypeHashCode(), new UShortWrapper() },
            { typeof(ushort[]).GetTypeHashCode(), new UShortArrWrapper() },
            { typeof(List<ushort>).GetTypeHashCode(), new UShortListWrapper() },
            { typeof(int).GetTypeHashCode(), new IntWrapper() },
            { typeof(int[]).GetTypeHashCode(), new IntArrWrapper() },
            { typeof(List<int>).GetTypeHashCode(), new IntListWrapper() },
            { typeof(uint).GetTypeHashCode(), new UIntWrapper() },
            { typeof(uint[]).GetTypeHashCode(), new UIntArrWrapper() },
            { typeof(List<uint>).GetTypeHashCode(), new UIntListWrapper() },
            { typeof(long).GetTypeHashCode(), new LongWrapper() },
            { typeof(long[]).GetTypeHashCode(), new LongArrWrapper() },
            { typeof(List<long>).GetTypeHashCode(), new LongListWrapper() },
            { typeof(ulong).GetTypeHashCode(), new ULongWrapper() },
            { typeof(ulong[]).GetTypeHashCode(), new ULongArrWrapper() },
            { typeof(List<ulong>).GetTypeHashCode(), new ULongListWrapper() },
            { typeof(float).GetTypeHashCode(), new FloatWrapper() },
            { typeof(float[]).GetTypeHashCode(), new FloatArrWrapper() },
            { typeof(List<float>).GetTypeHashCode(), new FloatListWrapper() },
            { typeof(double).GetTypeHashCode(), new DoubleWrapper() },
            { typeof(double[]).GetTypeHashCode(), new DoubleArrWrapper() },
            { typeof(List<double>).GetTypeHashCode(), new DoubleListWrapper() },
            { typeof(decimal).GetTypeHashCode(), new DecimalWrapper() },
            { typeof(decimal[]).GetTypeHashCode(), new DecimalArrWrapper() },
            { typeof(List<decimal>).GetTypeHashCode(), new DecimalListWrapper() },
            { typeof(string).GetTypeHashCode(), new StringWrapper() },
            { typeof(string[]).GetTypeHashCode(), new StringArrWrapper() },
            { typeof(List<string>).GetTypeHashCode(), new StringListWrapper() },
            { typeof(char).GetTypeHashCode(), new CharWrapper() },
            { typeof(char[]).GetTypeHashCode(), new CharArrWrapper() },
            { typeof(List<char>).GetTypeHashCode(), new CharListWrapper() },
            { typeof(bool).GetTypeHashCode(), new BoolWrapper() },
            { typeof(bool[]).GetTypeHashCode(), new BoolArrWrapper() },
            { typeof(List<bool>).GetTypeHashCode(), new BoolListWrapper() },
            { typeof(DateTime).GetTypeHashCode(), new DateTimeWrapper() },
            { typeof(DateTime[]).GetTypeHashCode(), new DateTimeArrWrapper() },
            { typeof(List<DateTime>).GetTypeHashCode(), new DateTimeListWrapper() },
        };

        public static bool TryGetWrapper(Type type, out INinoWrapper wrapper)
        {
            return Wrappers.TryGetValue(type.GetTypeHashCode(), out wrapper);
        }

        public static void AddWrapper(Type type, INinoWrapper wrapper)
        {
            Wrappers[type.GetTypeHashCode()] = wrapper;
        }
    }
}