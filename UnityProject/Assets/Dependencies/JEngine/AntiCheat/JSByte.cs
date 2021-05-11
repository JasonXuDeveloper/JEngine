//
// JSByte.cs
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
    public struct JSByte
    {
        private sbyte _obscuredSByte;
        private sbyte _obscuredKey;
        private sbyte _originalValue;

        private sbyte Value
        {
            get
            {
                var result=(sbyte)(_obscuredSByte-_obscuredKey);
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
                    _obscuredKey = (sbyte) JRandom.RandomNum(sbyte.MaxValue - value);
                    _obscuredSByte = (sbyte) (value + _obscuredKey);
                }
            }
        }

        public JSByte(sbyte val = 0)
        {
            _obscuredSByte = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            Value = val;
        }
        public JSByte(int val = 0)
        {
            _obscuredSByte = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            
            Value = val > sbyte.MaxValue ? sbyte.MaxValue : (sbyte) val;
        }
        
        public JSByte(string val = "0")
        {
            _obscuredSByte = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            var result = sbyte.TryParse(val,out var _value);
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

        public static implicit operator JSByte(sbyte val) => new JSByte(val);
        public static implicit operator sbyte(JSByte val) => val.Value;
        public static bool operator ==(JSByte a, JSByte b) => a.Value == b.Value;
        public static bool operator !=(JSByte a, JSByte b) => a.Value != b.Value;

        public static JSByte operator ++(JSByte a)
        {
            a.Value++;
            return a;
        }

        public static JSByte operator --(JSByte a)
        {
            a.Value--;
            return a;
        }
        
        public static JSByte operator + (JSByte a, JSByte b) => new JSByte(a.Value + b.Value);
        public static JSByte operator + (JSByte a, sbyte b) => new JSByte(a.Value + b);
        
        public static JSByte operator - (JSByte a, JSByte b) => new JSByte(a.Value - b.Value);
        public static JSByte operator - (JSByte a, sbyte b) => new JSByte(a.Value - b);

        public static JSByte operator * (JSByte a, JSByte b) => new JSByte(a.Value * b.Value);
        public static JSByte operator * (JSByte a, sbyte b) => new JSByte(a.Value * b);

        public static JSByte operator / (JSByte a, JSByte b) => new JSByte(a.Value / b.Value);
        public static JSByte operator / (JSByte a, sbyte b) => new JSByte(a.Value / b);
        
        public static JSByte operator % (JSByte a, JSByte b) => new JSByte(a.Value % b.Value);
        public static JSByte operator % (JSByte a, sbyte b) => new JSByte(a.Value % b);

        public override string ToString() => Value.ToString();

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj) => Value.Equals((obj is JSByte ? (JSByte) obj : default).Value);
    }
}