// EncryptionMapping.cs
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

using System.Collections.Generic;
using System.Linq;
using JEngine.Core.Encrypt.Bundle;

namespace JEngine.Core.Encrypt
{
    public enum EncryptionOption
    {
        Xor = 1,
        Aes = 2
    }

    public static class EncryptionMapping
    {
        public static readonly IReadOnlyDictionary<EncryptionOption, IBundleEncryptionConfig> Mapping =
            new Dictionary<EncryptionOption, IBundleEncryptionConfig>
            {
                { EncryptionOption.Xor, new XorBundle() },
                { EncryptionOption.Aes, new AesBundle() }
            };

        public static IBundleEncryptionConfig GetBundleConfig(EncryptionOption option)
        {
            if (Mapping.TryGetValue(option, out var config))
            {
                return config;
            }

            return Mapping.Values.First();
        }
    }
}