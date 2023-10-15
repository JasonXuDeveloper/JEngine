using System;
using System.Runtime.InteropServices;

namespace JEngine.Editor
{
    public class JEngineProjData
    {
        public string Prefix = "";
        public string EncryptPassword = "";
        public string Version = "0.8.0f7";

        public int Size()
        {
            return (Prefix.AsSpan().Length + EncryptPassword.AsSpan().Length + Version.AsSpan().Length) * 2 + 3;
        }

        public void AsBinary(ref Span<byte> data)
        {
            if (data.Length < Size())
                throw new Exception("data.Length < Size()");

            var prefix = MemoryMarshal.Cast<char, byte>(Prefix.AsSpan());
            var encryptPassword = MemoryMarshal.Cast<char, byte>(EncryptPassword.AsSpan());
            var version = MemoryMarshal.Cast<char, byte>(Version.AsSpan());

            data[0] = (byte)prefix.Length;
            prefix.CopyTo(data.Slice(1));
            data[prefix.Length + 1] = (byte)encryptPassword.Length;
            encryptPassword.CopyTo(data.Slice(prefix.Length + 2));
            data[prefix.Length + encryptPassword.Length + 2] = (byte)version.Length;
            version.CopyTo(data.Slice(prefix.Length + encryptPassword.Length + 3));
        }

        public void FromBinary(ref Span<byte> data)
        {
            var prefixLength = data[0];
            Prefix = MemoryMarshal.Cast<byte, char>(data.Slice(1, prefixLength)).ToString();
            var encryptPasswordLength = data[prefixLength + 1];
            EncryptPassword = MemoryMarshal.Cast<byte, char>(data.Slice(prefixLength + 2, encryptPasswordLength))
                .ToString();
            var versionLength = data[prefixLength + encryptPasswordLength + 2];
            Version = MemoryMarshal
                .Cast<byte, char>(data.Slice(prefixLength + encryptPasswordLength + 3, versionLength)).ToString();
        }
    }
}