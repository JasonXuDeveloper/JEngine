// AesConfig.cs
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

using UnityEngine;

namespace JEngine.Core.Encrypt.Config
{
    public class AesConfig : EncryptConfig<AesConfig, byte[]>
    {
        [SerializeField] public byte[] iv; // 128-bit IV

#if UNITY_EDITOR
        private void OnEnable()
        {
            // Generate default random key and IV if empty
            if (key == null || key.Length != 32 || IsEmpty(key))
            {
                key = GenerateRandomBytes(32);
            }

            if (iv == null || iv.Length != 16 || IsEmpty(iv))
            {
                iv = GenerateRandomBytes(16);
            }
        }

        private void Awake()
        {
            OnEnable();
        }

        private static byte[] GenerateRandomBytes(int length)
        {
            var bytes = new byte[length];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return bytes;
        }

        private static bool IsEmpty(byte[] array)
        {
            foreach (var b in array)
            {
                if (b != 0) return false;
            }

            return true;
        }
#endif
    }
}