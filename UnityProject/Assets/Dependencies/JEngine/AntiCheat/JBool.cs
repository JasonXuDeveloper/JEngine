//
// JBool.cs
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
namespace JEngine.AntiCheat
{
    public struct JBool
    {
        private int _value;
        private int _randomValue;
        private bool _originalValue;

        private bool Value
        {
            get
            {
                unchecked
                {
                    var result = _value - _randomValue != 0;
                    if (!_originalValue.Equals(result))
                    {
                        AntiCheatHelper.OnDetected();
                    }
                    return result;
                }
            }

            set
            {
                _originalValue = value;
                unchecked
                {
                    _randomValue = JRandom.RandomNum(1024);
                    _value = value ? 1 : 0 + _randomValue;
                }
            }
            
        }

        public JBool(bool value = false)
        {
            _value = 0;
            _randomValue = 0;
            _originalValue = true;
            Value = value;
        }

        public JBool(string value = "true")
        {
            _value = 0;
            _randomValue = 0;
            _originalValue = true;
            Value = value == "True";
        }

        public static implicit operator JBool(bool value) => new JBool(value);

        public static implicit operator bool(JBool value) => value.Value;

        public static bool operator == (JBool a, JBool b) => a.Value == b.Value;
        public static bool operator == (JBool a, bool b) => a.Value == b;
        
        public static bool operator != (JBool a, JBool b) => a.Value != b.Value;
        public static bool operator != (JBool a, bool b) => a.Value != b;

        public static bool operator ! (JBool a)
        {
            return !a.Value;
        }

        public override string ToString() => Value.ToString();

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj) => Value.Equals((obj is JBool ? (JBool) obj : default).Value);
    }
}