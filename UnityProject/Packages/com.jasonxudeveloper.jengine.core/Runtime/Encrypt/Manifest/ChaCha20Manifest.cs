// ChaCha20Manifest.cs
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

using JEngine.Core.Encrypt.Config;
using JEngine.Core.Encrypt.Shared;
using YooAsset;

namespace JEngine.Core.Encrypt.Manifest
{
    public class ChaCha20Manifest : ManifestEncryptionConfig<ChaCha20Config, ChaCha20ManifestProcess, ChaCha20ManifestRestore>
    {
        public ChaCha20Manifest(ChaCha20Config config) : base(config)
        {
        }
    }

    public class ChaCha20ManifestProcess : IManifestProcessServices
    {
        private readonly ChaCha20Config _config;

        public ChaCha20ManifestProcess(ChaCha20Config config)
        {
            _config = config;
        }

        public byte[] ProcessManifest(byte[] fileData)
        {
            // Use ChaCha20 stream cipher for manifest encryption
            return ChaCha20Util.ChaCha20Encrypt(fileData, _config.key, _config.nonce);
        }
    }

    public class ChaCha20ManifestRestore : IManifestRestoreServices
    {
        private readonly ChaCha20Config _config;

        public ChaCha20ManifestRestore(ChaCha20Config config)
        {
            _config = config;
        }

        public byte[] RestoreManifest(byte[] fileData)
        {
            // Use ChaCha20 stream cipher for manifest decryption
            return ChaCha20Util.ChaCha20Decrypt(fileData, _config.key, _config.nonce);
        }
    }
}