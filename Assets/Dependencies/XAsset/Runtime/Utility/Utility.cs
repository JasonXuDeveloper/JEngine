using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace libx
{
    public static class Utility
    {
        private static readonly MD5 md5 = MD5.Create();
        private static readonly CRC32 crc32 = new CRC32();

        public static string GetMD5Hash(string input)
        {
            var data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return ToHash(data);
        }

        public static string GetMD5Hash(Stream input)
        {
            var data = md5.ComputeHash(input);
            return ToHash(data);
        }

        public static bool VerifyMd5Hash(string input, string hash)
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            return 0 == comparer.Compare(input, hash);
        }

        public static string GetCRC32Hash(Stream input)
        {
            var data = crc32.ComputeHash(input);
            return ToHash(data);
        }

        public static uint GetCrc(byte[] bytes)
        {
            return CRC32.Compute(bytes);
        }

        public static string GetCRC32Hash(byte[] bytes)
        {
            var data = crc32.ComputeHash(bytes);
            return ToHash(data);
        }

        private static string ToHash(byte[] data)
        {
            var sb = new StringBuilder();
            foreach (var t in data)
                sb.Append(t.ToString("x2"));
            return sb.ToString();
        }

        public static string GetCRC32Hash(string input)
        {
            var data = crc32.ComputeHash(Encoding.UTF8.GetBytes(input));
            return ToHash(data);
        }

        public static bool VerifyCrc32Hash(string input, string hash)
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            return 0 == comparer.Compare(input, hash);
        }
    }

    internal sealed class CRC32 : HashAlgorithm
    {
        private const uint DefaultPolynomial = 0xedb88320u;
        private const uint DefaultSeed = 0xffffffffu;

        private static uint[] _defaultTable;

        private readonly uint _seed;
        private readonly uint[] _table;
        private uint _hash;

        public CRC32()
            : this(DefaultPolynomial, DefaultSeed)
        {
        }

        public CRC32(uint polynomial, uint seed)
        {
            if (!BitConverter.IsLittleEndian)
                throw new PlatformNotSupportedException("Not supported on Big Endian processors");

            _table = InitializeTable(polynomial);
            _seed = _hash = seed;
        }

        public override int HashSize => 32;

        public override void Initialize()
        {
            _hash = _seed;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            _hash = CalculateHash(_table, _hash, array, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            var hashBuffer = UInt32ToBigEndianBytes(~_hash);
            HashValue = hashBuffer;
            return hashBuffer;
        }

        public static uint Compute(byte[] buffer)
        {
            return Compute(DefaultSeed, buffer);
        }

        public static uint Compute(uint seed, byte[] buffer)
        {
            return Compute(DefaultPolynomial, seed, buffer);
        }

        public static uint Compute(uint polynomial, uint seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        private static uint[] InitializeTable(uint polynomial)
        {
            if (polynomial == DefaultPolynomial && _defaultTable != null)
                return _defaultTable;

            var createTable = new uint[256];
            for (var i = 0; i < 256; i++)
            {
                var entry = (uint) i;
                for (var j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry >>= 1;
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
                _defaultTable = createTable;

            return createTable;
        }

        private static uint CalculateHash(uint[] table, uint seed, IList<byte> buffer, int start, int size)
        {
            var hash = seed;
            for (var i = start; i < start + size; i++)
                hash = (hash >> 8) ^ table[buffer[i] ^ (hash & 0xff)];
            return hash;
        }

        private static byte[] UInt32ToBigEndianBytes(uint uint32)
        {
            var result = BitConverter.GetBytes(uint32);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }
    }
}