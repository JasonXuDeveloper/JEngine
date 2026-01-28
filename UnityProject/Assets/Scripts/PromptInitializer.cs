// PromptInitializer.cs
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

// ============================================================================
// IMPORTANT: This script requires the JEngine.UI package to be installed.
//
// To use this script:
// 1. Add the JEngine.UI package via OpenUPM:
//    openupm add com.jasonxudeveloper.jengine.ui
//
// 2. Or add to your manifest.json:
//    "com.jasonxudeveloper.jengine.ui": "1.0.0"
//
// If you don't need MessageBox dialogs, you can:
// - Delete this script entirely (Bootstrap will log warnings and continue)
// - Implement your own custom dialog provider by assigning to Prompt.ShowDialogAsync
// ============================================================================

using JEngine.Core;
using JEngine.UI;
using UnityEngine;

/// <summary>
/// Initializes the Prompt system to use MessageBox for dialogs.
/// This runs automatically before any scene loads.
/// </summary>
public static class PromptInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Register MessageBox as the dialog provider for JEngine.Core
        Prompt.ShowDialogAsync = MessageBox.Show;
        Debug.Log("[JEngine] Prompt system initialized with MessageBox provider.");
    }
}
