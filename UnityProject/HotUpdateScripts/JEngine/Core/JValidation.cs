//
// JValidation.cs
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2022 JEngine
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
using System.Collections.Generic;

namespace JEngine.Core
{
    public class JValidation
    {
        private bool _valid = true;
        public string ErrorMsg;

        public void SetErr(string err)
        {
            _valid = false;
            ErrorMsg = err;
        }

        public static implicit operator bool(JValidation value) => value._valid;
    }

    // For string
    public static class StringValidation
    {
        /// <summary>
        /// 验证字符串
        /// condition可以使用以下关键词（多个关键词用|拼接）: min:int,max:int,nullable,capital,lower,number
        /// </summary>
        /// <param name="str"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static JValidation Validate(this string str, string condition)
        {
            int minLength = -1;
            int maxLength = -1;
            bool nullable = false;
            bool allCapital = false;
            bool allLower = false;
            bool number = false;
            string nullableStr = "字符串不可为空", minLengthStr = "字符串长度至少{0}位",
                maxLengthStr = "字符串长度最多{0}位", allCapitalStr = "字符串需要全大写", allLowerStr = "字符串需要全小写",
                numberStr = "字符串需要全部是数字";

            var conds = condition.Split('|');
            for (int i = 0, cnt = conds.Length; i < cnt; i++)
            {
                var c = conds[i];
                if (c == "nullable")
                {
                    if (nullable) continue;
                    nullable = true;
                }
                if (c.StartsWith("min"))
                {
                    if (minLength != -1) continue;
                    if (!c.Contains(":"))
                    {
                        throw new ArgumentException("min condition requires the format of: 'min:{(int)number}', for example 'min:10'");
                    }
                    minLength = int.Parse(c.Split(':')[1]);
                }
                if (c.StartsWith("max"))
                {
                    if (maxLength != -1) continue;
                    if (!c.Contains(":"))
                    {
                        throw new ArgumentException("max condition requires the format of: 'max:{(int)number}', for example 'max:10'");
                    }
                    maxLength = int.Parse(c.Split(':')[1]);
                }
                if (c == "capital")
                {
                    if (allCapital) continue;
                    allCapital = true;
                }
                if (c == "lower")
                {
                    if (allLower) continue;
                    allLower = true;
                }
                if (c == "number")
                {
                    if (number) continue;
                    number = true;
                }
            }

            var ret = new JValidation();
            if (str == null && !nullable)
            {
                ret.SetErr(nullableStr);
                return ret;
            }
            else if (str == null)
            {
                return ret;
            }
            var l = str.Length;
            if (l < minLength)
            {
                ret.SetErr(string.Format(minLengthStr, minLength));
                return ret;
            }
            if (l > maxLength && maxLength >= 0)
            {
                ret.SetErr(string.Format(maxLengthStr, maxLength));
                return ret;
            }
            if (allCapital)
            {
                if (str != str.ToUpper())
                {
                    ret.SetErr(allCapitalStr);
                    return ret;
                }
            }
            if (allLower)
            {
                if (str != str.ToLower())
                {
                    ret.SetErr(allLowerStr);
                    return ret;
                }
            }
            if (number)
            {
                for (int i = 0; i < l; i++)
                {
                    char c = str[i];
                    if (c < '0' || c > '9')
                    {
                        ret.SetErr(numberStr);
                        return ret;
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// 验证字符串
        /// condition可以使用以下关键词（多个关键词用|拼接）: min:int,max:int,nullable,capital,lower,number
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ret"></param>
        /// <param name="cond"></param>
        /// <returns></returns>
        public static JValidation Validate(this string str, string cond, out JValidation ret)
        {
            ret = Validate(str, cond);
            return ret;
        }
    }
}
