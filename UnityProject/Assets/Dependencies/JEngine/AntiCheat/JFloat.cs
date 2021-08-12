//
// JFloat.cs
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
using System.Globalization;
using JEngine.Core;

namespace JEngine.AntiCheat
{
    public struct JFloat
    {
        private float _obscuredFloat;
        private int _obscuredKey;
        private float _originalValue;

        private float Value
        {
            get
            {
                var result = _obscuredFloat-_obscuredKey;
                var invalid = _originalValue > result
                    ? _originalValue - result > 0.0001
                    : result - _originalValue > 0.0001;
                if (invalid)
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
                    _obscuredKey = JRandom.RandomNum((int)value);
                    _obscuredFloat = value + _obscuredKey;
                }
            }
        }

        public JFloat(float val = 0)
        {
            _obscuredFloat = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            Value = val;
        }
        
        public JFloat(string val = "0")
        {
            _obscuredFloat = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            var result = float.TryParse(val,out var value);
            if (!result)
            {
                Log.PrintError($"无法将{val}变为{Value.GetType()},已改为0");
                Value = 0;
            }
            else
            {
                Value = value;
            }
        }

        public static implicit operator JFloat(float val) => new JFloat(val);
        public static implicit operator float(JFloat val) => val.Value;
        public static bool operator ==(JFloat a, JFloat b) => Math.Abs(a.Value - b.Value) <= float.Epsilon;
        public static bool operator !=(JFloat a, JFloat b) => Math.Abs(a.Value - b.Value) > float.Epsilon;

        public static JFloat operator ++(JFloat a)
        {
            a.Value++;
            return a;
        }

        public static JFloat operator --(JFloat a)
        {
            a.Value--;
            return a;
        }
        
        public static JFloat operator + (JFloat a, JFloat b) => new JFloat(a.Value + b.Value);
        public static JFloat operator + (JFloat a, float b) => new JFloat(a.Value + b);
        public static JFloat operator + (JFloat a, int b) => new JFloat(a.Value + b);
        
        public static JFloat operator - (JFloat a, JFloat b) => new JFloat(a.Value - b.Value);
        public static JFloat operator - (JFloat a, float b) => new JFloat(a.Value - b);
        public static JFloat operator - (JFloat a, int b) => new JFloat(a.Value - b);

        public static JFloat operator * (JFloat a, JFloat b) => new JFloat(a.Value * b.Value);
        public static JFloat operator * (JFloat a, float b) => new JFloat(a.Value * b);
        public static JFloat operator * (JFloat a, int b) => new JFloat(a.Value * b);

        public static JFloat operator / (JFloat a, JFloat b) => new JFloat(a.Value / b.Value);
        public static JFloat operator / (JFloat a, float b) => new JFloat(a.Value / b);
        public static JFloat operator / (JFloat a, int b) => new JFloat(a.Value / b);
        
        public static JFloat operator % (JFloat a, JFloat b) => new JFloat(a.Value % b.Value);
        public static JFloat operator % (JFloat a, float b) => new JFloat(a.Value % b);
        public static JFloat operator %(JFloat a, int b) => new JFloat(a.Value % b);

        public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj) => Value.Equals((obj is JFloat jFloat ? jFloat : default).Value);
    }
}