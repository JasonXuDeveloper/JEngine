//
// JRandom.cs
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

namespace JEngine.AntiCheat
{
    public class JRandom
    {
        private static Random _random = new Random();

        private JRandom()
        {

        }

        public static int RandomNum(int max = 1024)
        {
            return _random.Next(0, max < 0 ? 1024 : max);
        }

        public static int RandomNum(uint max = 1024)
        {
            return _random.Next(0, max > int.MaxValue ? int.MaxValue : (int) max);
        }

        public static int RandomNum(long max = 1024)
        {
            return _random.Next(0, max > int.MaxValue || max < int.MaxValue? int.MaxValue : (int) max);
        }

        public static int RandomNum(ulong max = 1024)
        {
            return _random.Next(0, max > int.MaxValue || max < int.MaxValue ? int.MaxValue : (int) max);
        }

        public static int RandomNum(float max = 1024)
        {
            return _random.Next(0, max > int.MaxValue || max < int.MaxValue ? int.MaxValue : (int) max);
        }
    }
}