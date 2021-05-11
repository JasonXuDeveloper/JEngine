//
// JString.cs
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

using JEngine.Core;

namespace JEngine.AntiCheat
{
    public struct JString
    {
        private string _obscuredString;
        private string _originalValue;

        private string Value {
            get
            {
                var result = CryptoHelper.DecryptStr(_obscuredString,InitJEngine.Instance.key);
                if (!_originalValue.Equals(result))
                {
                    AntiCheatHelper.OnDetected();
                }
                return result;
            }
            
            set
            {
                _originalValue = value;
                unchecked
                {
                    _obscuredString = CryptoHelper.EncryptStr(value, InitJEngine.Instance.key);
                }
            }
        }
        
        public JString(string val = "")
        {
            _obscuredString = "";
            _originalValue = "";
            Value = val;
        }

        public static implicit operator JString(string val) => new JString(val);
        public static implicit operator string(JString val) => val.Value;
        public static bool operator ==(JString a, JString b) => a.Value == b.Value;
        public static bool operator !=(JString a, JString b) => a.Value != b.Value;

        public static JString operator + (JString a, JString b) => new JString(a.Value + b.Value);
        public static JString operator + (JString a, string b) => new JString(a.Value + b);
        
        public override string ToString() => Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj) => Value.Equals((obj is JString ? (JString) obj : default).Value);
    }
}