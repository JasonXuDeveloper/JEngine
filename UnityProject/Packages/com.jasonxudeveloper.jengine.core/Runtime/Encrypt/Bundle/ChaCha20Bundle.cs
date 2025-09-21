// ChaCha20Bundle.cs
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

using System.IO;
using JEngine.Core.Encrypt.Config;
using JEngine.Core.Encrypt.Manifest;
using JEngine.Core.Encrypt.Shared;
using UnityEngine;
using YooAsset;

namespace JEngine.Core.Encrypt.Bundle
{
    public class ChaCha20Bundle : BundleEncryptionConfig<ChaCha20Config, ChaCha20Manifest, ChaCha20Config, ChaCha20EncryptionServices,
        ChaCha20DecryptionServices>
    {
        public override ChaCha20Config ManifestConfig => ChaCha20Config.Instance;
        public override ChaCha20Config BundleConfig => ChaCha20Config.Instance;
    }

    public class ChaCha20EncryptionServices : IEncryptionServices
    {
        private readonly ChaCha20Config _config;

        public ChaCha20EncryptionServices(ChaCha20Config config)
        {
            _config = config;
        }

        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
#if UNITY_EDITOR
            var bytes = File.ReadAllBytes(fileInfo.FileLoadPath);

            // Use ChaCha20 stream cipher for encryption
            var encryptedData = ChaCha20Util.ChaCha20Encrypt(bytes, _config.key, _config.nonce);

            EncryptResult result = new EncryptResult()
            {
                Encrypted = true,
                EncryptedData = encryptedData
            };
            return result;
#else
            throw new System.NotSupportedException("ChaCha20 encryption is only supported in Unity Editor");
#endif
        }
    }

    public class ChaCha20DecryptionServices : IDecryptionServices
    {
        private readonly ChaCha20Config _config;

        public ChaCha20DecryptionServices(ChaCha20Config config)
        {
            _config = config;
        }

        /// <summary>
        /// 同步方式获取解密的资源包对象
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            byte[] decryptedData = ChaCha20Util.ChaCha20Decrypt(fileInfo.FileLoadPath, _config.key, _config.nonce);

            var assetBundle = AssetBundle.LoadFromMemory(decryptedData);
            DecryptResult decryptResult = new DecryptResult();
            decryptResult.Result = assetBundle;
            return decryptResult;
        }

        /// <summary>
        /// 异步方式获取解密的资源包对象
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            byte[] decryptedData = ChaCha20Util.ChaCha20Decrypt(fileInfo.FileLoadPath, _config.key, _config.nonce);

            DecryptResult decryptResult = new DecryptResult();
            decryptResult.CreateRequest = AssetBundle.LoadFromMemoryAsync(decryptedData);
            return decryptResult;
        }

        /// <summary>
        /// 后备方式获取解密的资源包
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundleFallback(DecryptFileInfo fileInfo)
        {
            byte[] decryptedData = ChaCha20Util.ChaCha20Decrypt(fileInfo.FileLoadPath, _config.key, _config.nonce);

            var assetBundle = AssetBundle.LoadFromMemory(decryptedData);
            DecryptResult decryptResult = new DecryptResult();
            decryptResult.Result = assetBundle;
            return decryptResult;
        }

        /// <summary>
        /// 获取解密的字节数据
        /// </summary>
        byte[] IDecryptionServices.ReadFileData(DecryptFileInfo fileInfo)
        {
            return ChaCha20Util.ChaCha20Decrypt(fileInfo.FileLoadPath, _config.key, _config.nonce);
        }

        /// <summary>
        /// 获取解密的文本数据
        /// </summary>
        string IDecryptionServices.ReadFileText(DecryptFileInfo fileInfo)
        {
            byte[] decryptedData = ((IDecryptionServices)this).ReadFileData(fileInfo);
            return System.Text.Encoding.UTF8.GetString(decryptedData);
        }
    }
}