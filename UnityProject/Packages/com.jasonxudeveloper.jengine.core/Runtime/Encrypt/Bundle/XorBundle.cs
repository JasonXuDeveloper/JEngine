// XorBundle.cs
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
using JEngine.Core.Encrypt.Config;
using JEngine.Core.Encrypt.Manifest;
using UnityEngine;
using YooAsset;

namespace JEngine.Core.Encrypt.Bundle
{
    public class XorBundle : BundleEncryptionConfig<XorConfig, XorManifest, XorConfig, XorEncryptionServices,
        XorDecryptionServices, XorWebDecryptionServices>
    {
        public override XorConfig ManifestConfig  =>  XorConfig.Instance;
        public override XorConfig BundleConfig  =>  XorConfig.Instance;
    }

    public class XorEncryptionServices : IEncryptionServices
    {
        private readonly XorConfig _config;

        public XorEncryptionServices(XorConfig config)
        {
            _config = config;
        }

        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
#if UNITY_EDITOR
            var bytes = File.ReadAllBytes(fileInfo.FileLoadPath);
            var key = _config.key;

            Span<byte> span = bytes;
            for (int i = 0; i < span.Length; i++)
            {
                span[i] ^= key[i % key.Length];
            }

            EncryptResult result = new EncryptResult();
            result.Encrypted = true;
            result.EncryptedData = bytes;
            return result;
#else
            throw new System.NotSupportedException("XOR encryption is only supported in Unity Editor");
#endif
        }
    }

    public class XorDecryptionServices : IDecryptionServices
    {
        private readonly XorConfig _config;

        public XorDecryptionServices(XorConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Synchronously get decrypted asset bundle object
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            XorStream stream =
                new XorStream(_config.key, fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            DecryptResult decryptResult = new DecryptResult();
            decryptResult.ManagedStream = stream;
            decryptResult.Result =
                AssetBundle.LoadFromStream(stream, fileInfo.FileLoadCRC, 2048);
            return decryptResult;
        }

        /// <summary>
        /// Asynchronously get decrypted asset bundle object
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            XorStream stream =
                new XorStream(_config.key, fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            DecryptResult decryptResult = new DecryptResult();
            decryptResult.ManagedStream = stream;
            decryptResult.CreateRequest =
                AssetBundle.LoadFromStreamAsync(stream, fileInfo.FileLoadCRC, 2048);
            return decryptResult;
        }

        /// <summary>
        /// Fallback method to get decrypted asset bundle
        /// Note: When normal decryption method fails, fallback loading will be triggered!
        /// Description: It is recommended to load asset bundles through LoadFromMemory() method as a fallback mechanism.
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundleFallback(DecryptFileInfo fileInfo)
        {
            byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);
            var key = _config.key;
            Span<byte> span = fileData;
            for (int i = 0; i < span.Length; i++)
            {
                span[i] ^= key[i % key.Length];
            }

            var assetBundle = AssetBundle.LoadFromMemory(fileData);
            DecryptResult decryptResult = new DecryptResult();
            decryptResult.Result = assetBundle;
            return decryptResult;
        }

        /// <summary>
        /// Get decrypted byte data
        /// </summary>
        byte[] IDecryptionServices.ReadFileData(DecryptFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get decrypted text data
        /// </summary>
        string IDecryptionServices.ReadFileText(DecryptFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class XorWebDecryptionServices : IWebDecryptionServices
    {
        private readonly XorConfig _config;

        public XorWebDecryptionServices(XorConfig config)
        {
            _config = config;
        }

        public WebDecryptResult LoadAssetBundle(WebDecryptFileInfo fileInfo)
        {
            var data = fileInfo.FileData;
            var key = _config.key;
            Span<byte> span = data;
            for (int i = 0; i < span.Length; i++)
            {
                span[i] ^= key[i % key.Length];
            }

            var assetBundle = AssetBundle.LoadFromMemory(data);
            WebDecryptResult decryptResult = new WebDecryptResult();
            decryptResult.Result = assetBundle;
            return decryptResult;
        }
    }
}