//
// JLong.cs
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
    public struct JLong
    {
        private long _obscuredLong;
        private long _obscuredKey;
        private long _originalValue;

        private long Value
        {
            get
            {
                var result =_obscuredLong-_obscuredKey;
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
                    _obscuredKey = JRandom.RandomNum(long.MaxValue - value);
                    _obscuredLong = value + _obscuredKey;
                }
            }
        }

        public JLong(long val = 0)
        {
            _obscuredLong = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            Value = val;
        }
        
        public JLong(string val = "0")
        {
            _obscuredLong = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            var result = long.TryParse(val,out var _value);
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

        public static implicit operator JLong(long val) => new JLong(val);
        public static implicit operator long(JLong val) => val.Value;
        public static bool operator ==(JLong a, JLong b) => a.Value == b.Value;
        public static bool operator !=(JLong a, JLong b) => a.Value != b.Value;

        public static JLong operator ++(JLong a)
        {
            a.Value++;
            return a;
        }

        public static JLong operator --(JLong a)
        {
            a.Value--;
            return a;
        }
        
        public static JLong operator + (JLong a, JLong b) => new JLong(a.Value + b.Value);
        public static JLong operator + (JLong a, long b) => new JLong(a.Value + b);
        
        public static JLong operator - (JLong a, JLong b) => new JLong(a.Value - b.Value);
        public static JLong operator - (JLong a, long b) => new JLong(a.Value - b);

        public static JLong operator * (JLong a, JLong b) => new JLong(a.Value * b.Value);
        public static JLong operator * (JLong a, long b) => new JLong(a.Value * b);

        public static JLong operator / (JLong a, JLong b) => new JLong(a.Value / b.Value);
        public static JLong operator / (JLong a, long b) => new JLong(a.Value / b);
        
        public static JLong operator % (JLong a, JLong b) => new JLong(a.Value % b.Value);
        public static JLong operator % (JLong a, long b) => new JLong(a.Value % b);

        public override string ToString() => Value.ToString();

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj) => Value.Equals((obj is JLong ? (JLong) obj : default).Value);
    }
}