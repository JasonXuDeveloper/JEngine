// EnumHelpers.cs
//
//  Author:
//        JasonXuDeveloper <jason@xgamedev.net>
//
//  Copyright (c) 2025 JEngine
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace JEngine.UI.Editor.Utilities
{
    /// <summary>
    /// Helper methods for converting enums to/from strings for UI dropdowns.
    /// </summary>
    public static class EnumHelpers
    {
        /// <summary>
        /// Gets all enum value names as a list of strings.
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <returns>List of enum value names</returns>
        public static List<string> GetEnumNames<T>() where T : Enum
        {
            return Enum.GetNames(typeof(T)).ToList();
        }

        /// <summary>
        /// Converts a string back to enum value.
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <param name="value">The string representation of the enum value</param>
        /// <returns>The enum value</returns>
        public static T StringToEnum<T>(string value) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), value);
        }
    }
}
