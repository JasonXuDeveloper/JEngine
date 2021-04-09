//
// JByte.cs
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
    public struct JByte
    {
        private byte _obscuredByte;
        private byte _obscuredKey;
        private byte _originalValue;

        private byte Value
        {
            get
            {
                var result = (byte) (_obscuredByte - _obscuredKey);
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
                    _obscuredKey = (byte) JRandom.RandomNum(byte.MaxValue - value);
                    _obscuredByte = (byte) (value + _obscuredKey);
                }
            }
        }

        public JByte(byte val = 0)
        {
            _obscuredByte = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            Value = val;
        }
        public JByte(int val = 0)
        {
            _obscuredByte = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            Value = val > byte.MaxValue ? byte.MaxValue : (byte) val;
        }
        
        public JByte(string val = "0")
        {
            _obscuredByte = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            var result = byte.TryParse(val,out var _value);
            if (!result)
            {
                Log.PrintError($"无法将{val}变为{Value.GetType()},已改为0");
                Value = 0;
            }
            else
            {
                Value = _value;
            }
        }

        public static implicit operator JByte(byte val) => new JByte(val);
        public static implicit operator byte(JByte val) => val.Value;
        public static bool operator ==(JByte a, JByte b) => a.Value == b.Value;
        public static bool operator !=(JByte a, JByte b) => a.Value != b.Value;

        public static JByte operator ++(JByte a)
        {
            a.Value++;
            return a;
        }

        public static JByte operator --(JByte a)
        {
            a.Value--;
            return a;
        }
        
        public static JByte operator + (JByte a, JByte b) => new JByte(a.Value + b.Value);
        public static JByte operator + (JByte a, byte b) => new JByte(a.Value + b);
        
        public static JByte operator - (JByte a, JByte b) => new JByte(a.Value - b.Value);
        public static JByte operator - (JByte a, byte b) => new JByte(a.Value - b);

        public static JByte operator * (JByte a, JByte b) => new JByte(a.Value * b.Value);
        public static JByte operator * (JByte a, byte b) => new JByte(a.Value * b);

        public static JByte operator / (JByte a, JByte b) => new JByte(a.Value / b.Value);
        public static JByte operator / (JByte a, byte b) => new JByte(a.Value / b);
        
        public static JByte operator % (JByte a, JByte b) => new JByte(a.Value % b.Value);
        public static JByte operator % (JByte a, byte b) => new JByte(a.Value % b);

        public override string ToString() => Value.ToString();

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj) => Value.Equals((obj is JByte ? (JByte) obj : default).Value);
    }
}