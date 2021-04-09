//
// CryptoHelper.cs
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
using System.Security.Cryptography;
using System.Text;

namespace JEngine.Core
{
    public class CryptoHelper
    { 
        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string EncryptStr(string value, string key)
        {
            try
            {
                Byte[] keyArray = Encoding.UTF8.GetBytes(key);
                Byte[] toEncryptArray = Encoding.UTF8.GetBytes(value);
                var rijndael = new RijndaelManaged();
                rijndael.Key = keyArray;
                rijndael.Mode = CipherMode.ECB;
                rijndael.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = rijndael.CreateEncryptor();
                Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception ex)
            {
                Log.PrintError(ex);
                return null;
            }
        }
        
        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string DecryptStr(string value, string key)
        {
            try
            {
                Byte[] keyArray = Encoding.UTF8.GetBytes(key);
                Byte[] toEncryptArray = Convert.FromBase64String(value);
                var rijndael = new RijndaelManaged();
                rijndael.Key = keyArray;
                rijndael.Mode = CipherMode.ECB;
                rijndael.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = rijndael.CreateDecryptor();
                Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception ex)
            {
                Log.PrintError(ex);
                return null;
            }
        }
        
        /// <summary>
        /// AES 算法加密(ECB模式) 将明文加密
        /// </summary>
        /// <param name="toEncryptArray,">明文</param>
        /// <param name="Key">密钥</param>
        /// <returns>加密后base64编码的密文</returns>
        public static byte[] AesEncrypt(byte[] toEncryptArray, string Key)
        {
            try
            {
                byte[] keyArray = Encoding.UTF8.GetBytes(Key);

                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray;
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = rDel.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return resultArray;
            }
            catch (Exception ex)
            {
                Log.PrintError(ex);
                return null;
            }
        }
        
        /// <summary>
        /// AES 算法解密(ECB模式) 将密文base64解码进行解密，返回明文
        /// </summary>
        /// <param name="toDecryptArray">密文</param>
        /// <param name="Key">密钥</param>
        /// <returns>明文</returns>
        public static byte[] AesDecrypt(byte[] toDecryptArray, string Key)
        {
            try
            {
                byte[] keyArray = Encoding.UTF8.GetBytes(Key);

                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray;
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = rDel.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
                return resultArray;
            }
            catch (Exception ex)
            {
                Log.PrintError(ex);
                return null;
            }
        }
        
        /// <summary>
        /// AES 算法加密(ECB模式) 无padding填充，用于分块解密
        /// </summary>
        /// <param name="toEncryptArray,">明文</param>
        /// <param name="Key">密钥</param>
        /// <returns>加密后base64编码的密文</returns>
        public static byte[] AesEncryptWithNoPadding(byte[] toEncryptArray, string Key)
        {
            try
            {
                byte[] keyArray = Encoding.UTF8.GetBytes(Key);

                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray;
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.None;

                ICryptoTransform cTransform = rDel.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return resultArray;
            }
            catch (Exception ex)
            {
                Log.PrintError(ex);
                return null;
            }
        }
        
        /// <summary>
        /// AES 算法解密(ECB模式) 无padding填充，用于分块解密
        /// </summary>
        /// <param name="toDecryptArray">密文</param>
        /// <param name="Key">密钥</param>
        /// <returns>明文</returns>
        public static byte[] AesDecryptWithNoPadding(byte[] toDecryptArray, string Key)
        {
            try
            {
                byte[] keyArray = Encoding.UTF8.GetBytes(Key);

                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray;
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.None;

                ICryptoTransform cTransform = rDel.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
                return resultArray;
            }
            catch (Exception ex)
            {
                Log.PrintError(ex);
                return null;
            }
        }
    }

}