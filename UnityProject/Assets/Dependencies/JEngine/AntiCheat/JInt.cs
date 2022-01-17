//
// JInt.cs
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
    public struct JInt
    {
        internal int ObscuredInt;
        internal int ObscuredKey;
        internal int OriginalValue;

        private int Value
        {
            get
            {
                var result = ObscuredInt - ObscuredKey;
                if (!OriginalValue.Equals(result))
                {
                    AntiCheatHelper.OnDetected();
                }
                return result;
            }
            
            set
            {
                OriginalValue = value;
                unchecked
                {
                    ObscuredKey = JRandom.RandomNum(int.MaxValue - value);
                    ObscuredInt = value + ObscuredKey;
                }
            }
        }

        public JInt(int val = 0)
        {
            ObscuredInt = 0;
            ObscuredKey = 0;
            OriginalValue = 0;
            Value = val;
        }
        
        public JInt(string val = "0")
        {
            ObscuredInt = 0;
            ObscuredKey = 0;
            OriginalValue = 0;
            var result = int.TryParse(val,out var _value);
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

        public static implicit operator JInt(int val) => new JInt(val);
        public static implicit operator int(JInt val) => val.Value;
        
        public static bool operator ==(JInt a, JInt b) => a.Value == b.Value;
        public static bool operator ==(JInt a, int b) => a.Value == b;
        public static bool operator !=(JInt a, JInt b) => a.Value != b.Value;
        public static bool operator !=(JInt a, int b) => a.Value != b;

        public static JInt operator ++(JInt a)
        {
            a.Value++;
            return a;
        }

        public static JInt operator --(JInt a)
        {
            a.Value--;
            return a;
        }
        
        public static JInt operator + (JInt a, JInt b) => new JInt(a.Value + b.Value);
        public static JInt operator + (JInt a, int b) => new JInt(a.Value + b);
        
        public static JInt operator - (JInt a, JInt b) => new JInt(a.Value - b.Value);
        public static JInt operator - (JInt a, int b) => new JInt(a.Value - b);

        public static JInt operator * (JInt a, JInt b) => new JInt(a.Value * b.Value);
        public static JInt operator * (JInt a, int b) => new JInt(a.Value * b);

        public static JInt operator / (JInt a, JInt b) => new JInt(a.Value / b.Value);
        public static JInt operator / (JInt a, int b) => new JInt(a.Value / b);
        
        public static JInt operator % (JInt a, JInt b) => new JInt(a.Value % b.Value);
        public static JInt operator % (JInt a, int b) => new JInt(a.Value % b);

        public override string ToString() => Value.ToString();

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj) => Value.Equals(obj is JInt ? ((JInt) obj).Value : obj);
    }
}