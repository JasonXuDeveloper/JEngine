using System;
using System.IO;
using System.Runtime.InteropServices;
using YooAsset;

/// <summary>
/// 文件偏移加密方式
/// </summary>
public class FileOffsetEncryption : IEncryptionServices
{
    public EncryptResult Encrypt(EncryptFileInfo fileInfo)
    {
        int offset = 32;
        byte[] fileData = File.ReadAllBytes(fileInfo.FilePath);
        var encryptedData = new byte[fileData.Length + offset];
        Buffer.BlockCopy(fileData, 0, encryptedData, offset, fileData.Length);

        Span<long> span = MemoryMarshal.Cast<byte, long>(encryptedData);
        span[0] = fileInfo.GetHashCode();
        span[1] = DateTime.Now.Ticks;
        span[2] = fileInfo.FilePath.GetHashCode();
        span[3] = fileInfo.FilePath.Length;
        
        return new EncryptResult
        {
            Encrypted = true,
            EncryptedData = encryptedData
        };
    }
}