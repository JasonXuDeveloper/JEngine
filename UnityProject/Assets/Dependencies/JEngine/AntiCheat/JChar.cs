//
// JChar.cs
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

using System;
using JEngine.Core;

namespace JEngine.AntiCheat
{
    public struct JChar
    {
        private int _obscuredChar;
        private int _obscuredKey;
        private char _originalValue;

        private char Value
        {
            get
            {
                var result = (char)(_obscuredChar-_obscuredKey);
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
                    _obscuredKey = JRandom.RandomNum(value);
                    _obscuredChar = value + _obscuredKey;
                }
            }
        }
        
        public JChar(int val = 0)
        {
            _obscuredChar = 0;
            _obscuredKey = 0;
            _originalValue = ' ';
            try
            {
                Value = Convert.ToChar(val);
            }
            catch
            {
                Log.PrintError($"无法将{val}变为{Value.GetType()},已改为{Char.MinValue}");
                Value = Char.MinValue;
            }
        }

        public JChar(string val = "")
        {
            _obscuredChar = 0;
            _obscuredKey = 0;
            _originalValue = ' ';
            var result = char.TryParse(val,out var _value);
            if (!result)
            {
                Log.PrintError($"无法将{val}变为{Value.GetType()},已改为0");
                Value = Char.MinValue;
            }
            else
            {
                Value = _value;
            }
        }

        public static implicit operator JChar(int val) => new JChar(val);
        public static implicit operator int(JChar val) => val.Value;
        public static bool operator ==(JChar a, JChar b) => a.Value == b.Value;
        public static bool operator !=(JChar a, JChar b) => a.Value != b.Value;

        public static JChar operator ++(JChar a)
        {
            a.Value++;
            return a;
        }

        public static JChar operator --(JChar a)
        {
            a.Value--;
            return a;
        }
        
        public static JChar operator + (JChar a, JChar b) => new JChar(a.Value + b.Value);
        public static JChar operator + (JChar a, int b) => new JChar(a.Value + b);
        
        public static JChar operator - (JChar a, JChar b) => new JChar(a.Value - b.Value);
        public static JChar operator - (JChar a, int b) => new JChar(a.Value - b);

        public static JChar operator * (JChar a, JChar b) => new JChar(a.Value * b.Value);
        public static JChar operator * (JChar a, int b) => new JChar(a.Value * b);

        public static JChar operator / (JChar a, JChar b) => new JChar(a.Value / b.Value);
        public static JChar operator / (JChar a, int b) => new JChar(a.Value / b);
        
        public static JChar operator % (JChar a, JChar b) => new JChar(a.Value % b.Value);
        public static JChar operator % (JChar a, int b) => new JChar(a.Value % b);

        public override string ToString() => Value.ToString();

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj) => Value.Equals((obj is JChar ? (JChar) obj : default).Value);
    }
}