// IBundleEncryptionConfig.cs
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
using UnityEngine;
using YooAsset;

namespace JEngine.Core.Encrypt
{
    public interface IBundleEncryptionConfig
    {
        ScriptableObject ManifestConfigScriptableObject { get; }
        ScriptableObject BundleConfigScriptableObject { get; }
        IManifestEncryptionConfig ManifestEncryptionConfig { get; }
        IEncryptionServices Encryption { get; }
        IDecryptionServices Decryption { get; }
    }

    public abstract class BundleEncryptionConfig<TManifestConfig, TManifestEncryptionConfig, TBundleConfig, TEncryption,
        TDecryption>
        : IBundleEncryptionConfig
        where TManifestConfig : ScriptableObject
        where TManifestEncryptionConfig : IManifestEncryptionConfig
        where TBundleConfig : ScriptableObject
        where TEncryption : IEncryptionServices
        where TDecryption : IDecryptionServices
    {
        public abstract TManifestConfig ManifestConfig { get; }
        public abstract TBundleConfig BundleConfig { get; }
        
        public ScriptableObject ManifestConfigScriptableObject => ManifestConfig;
        public ScriptableObject BundleConfigScriptableObject => BundleConfig;

        public IManifestEncryptionConfig ManifestEncryptionConfig
        {
            get
            {
                if (_manifestEncryptionConfigInstance == null)
                {
                    _manifestEncryptionConfigInstance =
                        (TManifestEncryptionConfig)Activator.CreateInstance(typeof(TManifestEncryptionConfig),
                            new object[] { ManifestConfig });
                }

                return _manifestEncryptionConfigInstance;
            }
        }

        private TManifestEncryptionConfig _manifestEncryptionConfigInstance;

        public IEncryptionServices Encryption
        {
            get
            {
                if (_encryptionInstance == null)
                {
                    _encryptionInstance =
                        (TEncryption)Activator.CreateInstance(typeof(TEncryption), new object[] { BundleConfig });
                }

                return _encryptionInstance;
            }
        }

        private TEncryption _encryptionInstance;

        public IDecryptionServices Decryption
        {
            get
            {
                if (_decryptionInstance == null)
                {
                    _decryptionInstance =
                        (TDecryption)Activator.CreateInstance(typeof(TDecryption), new object[] { BundleConfig });
                }

                return _decryptionInstance;
            }
        }

        private TDecryption _decryptionInstance;
    }
}