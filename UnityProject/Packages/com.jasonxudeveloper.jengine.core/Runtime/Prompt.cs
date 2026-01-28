// Prompt.cs
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
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace JEngine.Core
{
    /// <summary>
    /// User prompt/dialog abstraction. Configure ShowDialogAsync to customize behavior.
    /// </summary>
    public static class Prompt
    {
        /// <summary>
        /// Shows a dialog to the user and returns the result.
        /// Parameters: title, content, okText (null to hide), noText (null to hide)
        /// Returns: true if OK clicked, false if No/Cancel clicked
        /// Default: logs warning and returns true (continues execution).
        /// </summary>
        public static Func<string, string, string, string, UniTask<bool>> ShowDialogAsync = DefaultShowDialog;

        private static UniTask<bool> DefaultShowDialog(string title, string content, string ok, string no)
        {
            Debug.LogWarning($"[JEngine] Dialog provider not configured. {title}: {content}");
            return UniTask.FromResult(true);
        }
    }
}
