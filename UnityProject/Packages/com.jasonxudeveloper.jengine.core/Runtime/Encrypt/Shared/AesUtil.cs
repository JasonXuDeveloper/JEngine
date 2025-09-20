// AesUtil.cs
//
//  Author:
//        JasonXuDeveloper <jason@xgamedev.net>
//
//  Copyright (c) 2025 JEngine
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.

using System;
using System.IO;
using System.Security.Cryptography;

namespace JEngine.Core.Encrypt.Shared
{
    public static class AesUtil
    {
        /// <summary>
        /// Encrypts data using AES-256 CBC with PKCS7 padding
        /// </summary>
        public static byte[] AesEncrypt(byte[] data, byte[] key, byte[] iv)
        {
            ValidateKeyAndIv(key, iv);
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }

        /// <summary>
        /// Decrypts data using AES-256 CBC with PKCS7 padding
        /// </summary>
        public static byte[] AesDecrypt(byte[] encryptedData, byte[] key, byte[] iv)
        {
            ValidateKeyAndIv(key, iv);
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(encryptedData);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var result = new MemoryStream();
            cs.CopyTo(result);
            return result.ToArray();
        }

        /// <summary>
        /// Decrypts file data using AES-256 CBC with PKCS7 padding - optimized for large files
        /// </summary>
        public static byte[] AesDecrypt(string filePath, byte[] key, byte[] iv)
        {
            ValidateKeyAndIv(key, iv);
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var cs = new CryptoStream(fs, decryptor, CryptoStreamMode.Read);
            using var result = new MemoryStream();
            cs.CopyTo(result);
            return result.ToArray();
        }
        
        /// <summary>
        /// Validates that key and IV have correct sizes for AES-256 CBC
        /// </summary>
        private static void ValidateKeyAndIv(byte[] key, byte[] iv)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key), "AES key cannot be null");
            if (iv == null)
                throw new ArgumentNullException(nameof(iv), "AES IV cannot be null");
            if (key.Length != 32)
                throw new ArgumentException($"AES-256 requires a 32-byte key, got {key.Length} bytes", nameof(key));
            if (iv.Length != 16)
                throw new ArgumentException($"AES CBC requires a 16-byte IV, got {iv.Length} bytes", nameof(iv));
        }
    }
}