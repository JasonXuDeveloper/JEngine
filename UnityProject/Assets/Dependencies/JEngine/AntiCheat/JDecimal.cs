//
// JDecimal.cs
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
    public struct JDecimal
    {
        private decimal _obscuredDecimal;
        private int _obscuredKey;
        private decimal _originalValue;

        private decimal Value
        {
            get
            {
                var result = _obscuredDecimal - _obscuredKey;
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
                    _obscuredKey = JRandom.RandomNum((int) value);
                    _obscuredDecimal = value + _obscuredKey;
                }
            }
        }

        public JDecimal(decimal val = 0)
        {
            _obscuredDecimal = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            Value = val;
        }
        
        public JDecimal(string val = "0")
        {
            _obscuredDecimal = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            var result = decimal.TryParse(val,out var _value);
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

        public static implicit operator JDecimal(decimal val) => new JDecimal(val);
        public static implicit operator decimal(JDecimal val) => val.Value;
        public static bool operator ==(JDecimal a, JDecimal b) => a.Value == b.Value;
        public static bool operator !=(JDecimal a, JDecimal b) => a.Value != b.Value;

        public static JDecimal operator ++(JDecimal a)
        {
            a.Value++;
            return a;
        }

        public static JDecimal operator --(JDecimal a)
        {
            a.Value--;
            return a;
        }
        
        public static JDecimal operator + (JDecimal a, JDecimal b) => new JDecimal(a.Value + b.Value);
        public static JDecimal operator + (JDecimal a, decimal b) => new JDecimal(a.Value + b);
        
        public static JDecimal operator - (JDecimal a, JDecimal b) => new JDecimal(a.Value - b.Value);
        public static JDecimal operator - (JDecimal a, decimal b) => new JDecimal(a.Value - b);

        public static JDecimal operator * (JDecimal a, JDecimal b) => new JDecimal(a.Value * b.Value);
        public static JDecimal operator * (JDecimal a, decimal b) => new JDecimal(a.Value * b);

        public static JDecimal operator / (JDecimal a, JDecimal b) => new JDecimal(a.Value / b.Value);
        public static JDecimal operator / (JDecimal a, decimal b) => new JDecimal(a.Value / b);
        
        public static JDecimal operator % (JDecimal a, JDecimal b) => new JDecimal(a.Value % b.Value);
        public static JDecimal operator % (JDecimal a, decimal b) => new JDecimal(a.Value % b);

        public override string ToString() => Value.ToString();

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj) => Value.Equals((obj is JDecimal ? (JDecimal) obj : default).Value);
    }
}