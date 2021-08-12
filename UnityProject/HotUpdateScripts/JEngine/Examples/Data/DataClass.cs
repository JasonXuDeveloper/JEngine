//
// DataClass.cs
//
// Author:
//        JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2020 
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
namespace JEngine.Examples
{
    public partial class DataClass
    {
        /// <summary>
        /// Property which holds the real value
        /// </summary>
        public long Money
        {
            get
            {
                return money;
            }
            set
            {
                money = value;
                if (BindableMoney != null)
                {
                    BindableMoney.Value = value;
                }
                else
                {
                    BindableMoney = new BindableProperty<long>(value);
                }
            }
        }

        /*
        * Fields to bind
        */
        public BindableProperty<long> BindableMoney = new BindableProperty<long>(0);


        public void Awake()
        {
            Log.Print("[DataClass] DataClass 被 Active After了");
        }
    }
}
