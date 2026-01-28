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
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace JEngine.Core
{
    /// <summary>
    /// Provides an abstraction layer for displaying user prompts and dialogs.
    /// This allows the core framework to request user input without depending on specific UI implementations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By default, dialogs log an error and return false. To enable actual dialogs,
    /// assign a handler to <see cref="ShowDialogAsync"/> before Bootstrap runs.
    /// </para>
    /// <para>
    /// Example using JEngine.UI.MessageBox:
    /// <code>
    /// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    /// static void Init() => Prompt.ShowDialogAsync = MessageBox.Show;
    /// </code>
    /// </para>
    /// </remarks>
    public static class Prompt
    {
        private static volatile Func<string, string, string, string, UniTask<bool>> _showDialogAsync = DefaultShowDialog;

        /// <summary>
        /// Gets or sets the dialog handler function.
        /// </summary>
        /// <value>
        /// A function that displays a dialog to the user.
        /// Parameters: title, content, okText (null to hide OK button), noText (null to hide No button).
        /// Returns: true if OK clicked, false if No/Cancel clicked.
        /// </value>
        /// <remarks>
        /// This property is thread-safe. Assign before Bootstrap runs using
        /// <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]</c>.
        /// </remarks>
        public static Func<string, string, string, string, UniTask<bool>> ShowDialogAsync
        {
            get => _showDialogAsync;
            set => Interlocked.Exchange(ref _showDialogAsync, value ?? DefaultShowDialog);
        }

        private static UniTask<bool> DefaultShowDialog(string title, string content, string ok, string no)
        {
            Debug.LogError($"[JEngine] Dialog provider not configured. Cannot display: {title}: {content}");
            return UniTask.FromResult(false);
        }
    }
}
