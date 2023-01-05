//
// CryptoMgr.cs
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2020 JEngine
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Text;
using System.Security.Cryptography;

namespace JEngine.Core
{
    public static class CryptoMgr
    {
        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string EncryptStr(string value, string key) =>
            Convert.ToBase64String(AesEncrypt(Encoding.UTF8.GetBytes(value), key));

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string DecryptStr(string value, string key) =>
            Encoding.UTF8.GetString(AesDecrypt(Convert.FromBase64String(value), key));

        /// <summary>
        /// AES 算法加密(ECB模式 PKCS7填充) 将明文加密
        /// </summary>
        /// <param name="data">明文</param>
        /// <param name="key">密钥</param>
        /// <param name="cipherMode">加密模式</param>
        /// <param name="paddingMode">填充方式</param>
        /// <returns>加密后base64编码的密文</returns>
        public static byte[] AesEncrypt(byte[] data, string key, CipherMode cipherMode = CipherMode.ECB,
            PaddingMode paddingMode = PaddingMode.PKCS7) =>
            AesEncrypt(data, Encoding.UTF8.GetBytes(key), cipherMode, paddingMode);

        /// <summary>
        /// AES 算法加密 将明文加密
        /// </summary>
        /// <param name="data">明文</param>
        /// <param name="key">密钥</param>
        /// <param name="cipherMode">加密模式</param>
        /// <param name="paddingMode">填充方式</param>
        /// <returns>加密后base64编码的密文</returns>
        public static byte[] AesEncrypt(byte[] data, byte[] key, CipherMode cipherMode = CipherMode.ECB,
            PaddingMode paddingMode = PaddingMode.PKCS7) =>
            AesEncrypt(data, key, 0, data.Length, cipherMode, paddingMode);

        /// <summary>
        /// AES 算法加密 将明文加密
        /// </summary>
        /// <param name="data">明文</param>
        /// <param name="key">密钥</param>
        /// <param name="offset">明文数据偏移</param>
        /// <param name="length">明文数据长度</param>
        /// <param name="cipherMode">加密模式</param>
        /// <param name="paddingMode">填充方式</param>
        /// <returns>加密后base64编码的密文</returns>
        public static byte[] AesEncrypt(byte[] data, byte[] key, int offset, int length,
            CipherMode cipherMode = CipherMode.ECB,
            PaddingMode paddingMode = PaddingMode.PKCS7)
        {
            try
            {
                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = key;
                rDel.Mode = cipherMode;
                rDel.Padding = paddingMode;
                return rDel.CreateEncryptor().TransformFinalBlock(data, offset, length);
            }
            catch (Exception ex)
            {
                Log.PrintError(ex);
                return ConstMgr.NullBytes;
            }
        }

        /// <summary>
        /// AES 算法解密 将密文解码进行解密，返回明文
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <param name="cipherMode">加密模式</param>
        /// <param name="paddingMode">填充方式</param>
        /// <returns></returns>
        public static byte[] AesDecrypt(byte[] data, string key, CipherMode cipherMode = CipherMode.ECB,
            PaddingMode paddingMode = PaddingMode.PKCS7) =>
            AesDecrypt(data, Encoding.UTF8.GetBytes(key), cipherMode, paddingMode);

        /// <summary>
        /// AES 算法解密 将密文解码进行解密，返回明文
        /// </summary>
        /// <param name="data">密文</param>
        /// <param name="key">密钥</param>
        /// <param name="cipherMode">加密模式</param>
        /// <param name="paddingMode">填充方式</param>
        /// <returns>明文</returns>
        public static byte[] AesDecrypt(byte[] data, byte[] key, CipherMode cipherMode = CipherMode.ECB,
            PaddingMode paddingMode = PaddingMode.PKCS7) =>
            AesDecrypt(data, key, 0, data.Length, cipherMode, paddingMode);

        /// <summary>
        /// AES 算法解密 将密文解码进行解密，返回明文
        /// </summary>
        /// <param name="data">密文</param>
        /// <param name="key">密钥</param>
        /// <param name="offset">明文数据偏移</param>
        /// <param name="length">明文数据长度</param>
        /// <param name="cipherMode">加密模式</param>
        /// <param name="paddingMode">填充方式</param>
        /// <returns>明文</returns>
        public static byte[] AesDecrypt(byte[] data, byte[] key, int offset, int length,
            CipherMode cipherMode = CipherMode.ECB,
            PaddingMode paddingMode = PaddingMode.PKCS7)
        {
            try
            {
                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = key;
                rDel.Mode = cipherMode;
                rDel.Padding = paddingMode;
                return rDel.CreateDecryptor().TransformFinalBlock(data, offset, length);
            }
            catch (Exception ex)
            {
                Log.PrintError(ex);
                return ConstMgr.NullBytes;
            }
        }
    }
}