// XorManifest.cs
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
using JEngine.Core.Encrypt.Config;
using YooAsset;

namespace JEngine.Core.Encrypt.Manifest
{
    public class XorManifest : ManifestEncryptionConfig<XorConfig, XorManifestProcess, XorManifestRestore>
    {
        public XorManifest(XorConfig config) : base(config)
        {
        }
    }

    public class XorManifestProcess : IManifestProcessServices
    {
        private readonly XorConfig _config;

        public XorManifestProcess(XorConfig config)
        {
            _config = config;
        }

        public byte[] ProcessManifest(byte[] fileData)
        {
#if UNITY_EDITOR
            var key = _config.key;
            Span<byte> dataSpan = fileData;
            for (int i = 0; i < dataSpan.Length; i++)
            {
                dataSpan[i] ^= key[i % key.Length];
            }

            return fileData;
#else
            throw new System.NotSupportedException("XOR manifest encryption is only supported in Unity Editor");
#endif
        }
    }

    public class XorManifestRestore : IManifestRestoreServices
    {
        private readonly XorConfig _config;

        public XorManifestRestore(XorConfig config)
        {
            _config = config;
        }

        public byte[] RestoreManifest(byte[] fileData)
        {
            var key = _config.key;
            Span<byte> dataSpan = fileData;
            for (int i = 0; i < dataSpan.Length; i++)
            {
                dataSpan[i] ^= key[i % key.Length];
            }

            return fileData;
        }
    }
}