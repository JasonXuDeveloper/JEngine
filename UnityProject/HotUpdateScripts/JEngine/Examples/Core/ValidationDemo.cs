//
// ValidationDemo.cs
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
using UnityEngine;
using JEngine.Core;

namespace JEngine.Examples
{
    public class ValidationDemo
    {
        public void Awake()
        {
            Debug.Log("目前验证器制作了对String的验证");
            Debug.Log("条件可以使用以下关键词（多个关键词用|拼接）: min:int,max:int,nullable,capital,lower,number");
            Debug.Log("一种写法是StringValidation.Validate(字符串内容，条件，out var result)，这个result是个JValidation，可以获取其ErrorMsg即错误信息");
            var str = "111";
            if (!StringValidation.Validate(str, "min:10", out var v))
            {
                Debug.LogError(v.ErrorMsg);
            }
            else
            {
                Debug.Log($"valid str: {str}");
            }

            Debug.Log("简单点也可以定义一个对象，然后用扩展方法验证，即：var result = 字符串内容.Validate(条件)");
            str = "1234567890987654321";
            var validated = str.Validate("max:10");
            if (!validated)
            {
                Debug.LogError(validated.ErrorMsg);
            }
            else
            {
                Debug.Log($"valid str: {str}");
            }

            Debug.Log("条件可以有多个，用|分割即可，即str.Validate(条件1|条件2|...)");
            str = "ABCDEFG";
            validated = StringValidation.Validate(str, $"max:{int.MaxValue}|capital");
            if (!validated)
            {
                Debug.LogError(validated.ErrorMsg);
            }
            else
            {
                Debug.Log($"valid str: {str}");
            }

            Debug.Log("判断是不是全是数字");
            str = "1234567890987654321";
            validated = str.Validate("number");
            if (!validated)
            {
                Debug.LogError(validated.ErrorMsg);
            }
            else
            {
                Debug.Log($"valid str: {str}");
            }

            Debug.Log("还可以对空string做验证");
            str = null;
            validated = str.Validate("nullable");
            if (!validated)
            {
                Debug.LogError(validated.ErrorMsg);
            }
            else
            {
                Debug.Log($"str is null: {str is null}");
            }
        }
    }
}
