// ChaCha20Config.cs
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

using JEngine.Core.Encrypt.Shared;
using UnityEngine;

namespace JEngine.Core.Encrypt.Config
{
    public class ChaCha20Config : EncryptConfig<ChaCha20Config, byte[]>
    {
        [SerializeField] public byte[] nonce; // 96-bit nonce

#if UNITY_EDITOR
        private void OnEnable()
        {
            // Generate default random key and nonce if empty
            if (key == null || key.Length != 32 || CryptoUtils.IsEmpty(key))
            {
                key = CryptoUtils.GenerateRandomBytes(32);
            }

            if (nonce == null || nonce.Length != 12 || CryptoUtils.IsEmpty(nonce))
            {
                nonce = CryptoUtils.GenerateRandomBytes(12);
            }
        }

        private void Awake()
        {
            OnEnable();
        }
#endif
        public override void RegenerateKey()
        {
            key = CryptoUtils.GenerateRandomBytes(32);
            nonce = CryptoUtils.GenerateRandomBytes(12);
        }
    }
}