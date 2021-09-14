//
// BindableProperty.cs
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

namespace JEngine.Core
{
    public class BindableProperty<T>
    {
        public delegate void onChange(T val);
        public delegate void onChangeWithOldVal(T oldVal, T newVal);
        public onChange OnChange;
        public onChangeWithOldVal OnChangeWithOldVal;

        private T _value;
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!Equals(_value, value))
                {
                    T oldValue = _value;
                    _value = value;
                    OnChange?.Invoke(_value);
                    OnChangeWithOldVal?.Invoke(oldValue, _value);
                }
            }
        }

        public BindableProperty(T val)
        {
            _value = val;
        }

        public override string ToString()
        {
            return (Value != null ? Convert.ToString(Value) : "null");
        }

        public void Clear()
        {
            _value = default(T);
        }

        public static implicit operator T(BindableProperty<T> t)
        {
            return t.Value;
        }
    }


}
