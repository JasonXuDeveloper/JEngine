// PopupFieldStyleHelper.cs
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

using System.Collections.Generic;
using System.Diagnostics;
using JEngine.UI.Editor.Theming;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace JEngine.UI.Editor.Utilities
{
    /// <summary>
    /// Utility class for reliably styling PopupField elements with retry logic.
    /// Handles Unity's asynchronous PopupField DOM structure creation.
    /// </summary>
    public static class PopupFieldStyleHelper
    {
        // Track styled elements to prevent duplicate styling
        private static readonly HashSet<int> StyledElements = new();

        // Track elements with registered hover callbacks
        private static readonly HashSet<int> HoverCallbacksRegistered = new();

        // Polling configuration
        private const int MaxRetries = 20;
        private const long TimeoutMs = 1000;
        private const long PollIntervalMs = 50;

        /// <summary>
        /// Applies custom styles to a PopupField with retry logic.
        /// Polls until child elements exist or timeout occurs.
        /// </summary>
        /// <param name="field">The PopupField to style</param>
        /// <param name="enableHoverEffects">Whether to enable hover color changes</param>
        /// <param name="enableDebugLogging">Whether to log retry attempts and timing</param>
        public static void ApplyStylesToPopupField(
            PopupField<string> field,
            bool enableHoverEffects = true,
            bool enableDebugLogging = false)
        {
            if (field == null) return;

            // Check if already styled (idempotency)
            int elementId = field.GetHashCode();
            if (StyledElements.Contains(elementId))
            {
                if (enableDebugLogging)
                {
                    Debug.Log($"[PopupFieldStyleHelper] Element {elementId} already styled, skipping");
                }
                return;
            }

            // Start timing
            var stopwatch = Stopwatch.StartNew();
            int retryCount = 0;

            // Schedule polling with retry logic
            field.schedule.Execute(() =>
            {
                retryCount++;

                // Query for child elements
                var inputElement = field.Q(className: "unity-base-popup-field__input");
                var arrowElement = field.Q(className: "unity-base-popup-field__arrow");
                var textElement = field.Q<TextElement>();
                var labelElement = field.Q<Label>(className: "unity-base-field__label");

                // Check if critical elements exist
                bool allElementsExist = inputElement != null && arrowElement != null && textElement != null;

                if (allElementsExist)
                {
                    // Apply styles
                    ApplyInputStyles(inputElement);
                    ApplyArrowStyles(arrowElement);
                    ApplyTextStyles(textElement);

                    if (labelElement != null)
                    {
                        ApplyLabelStyles(labelElement);
                    }

                    // Register hover effects if enabled
                    if (enableHoverEffects && !HoverCallbacksRegistered.Contains(elementId))
                    {
                        RegisterHoverCallbacks(inputElement, elementId);
                    }

                    // Mark as styled
                    StyledElements.Add(elementId);
                    stopwatch.Stop();

                    if (enableDebugLogging)
                    {
                        Debug.Log(
                            $"[PopupFieldStyleHelper] Successfully styled element {elementId} " +
                            $"after {retryCount} retries, {stopwatch.ElapsedMilliseconds}ms");
                    }

                    return; // Success - stop polling
                }

                // Check timeout conditions
                if (retryCount >= MaxRetries || stopwatch.ElapsedMilliseconds >= TimeoutMs)
                {
                    stopwatch.Stop();
                    Debug.LogWarning(
                        $"[PopupFieldStyleHelper] Timeout styling element {elementId} " +
                        $"after {retryCount} retries, {stopwatch.ElapsedMilliseconds}ms. " +
                        $"Elements found: input={inputElement != null}, arrow={arrowElement != null}, text={textElement != null}");
                }
            }).Until(() =>
            {
                // Stop condition: all elements exist OR timeout/max retries
                var inputElement = field.Q(className: "unity-base-popup-field__input");
                var arrowElement = field.Q(className: "unity-base-popup-field__arrow");
                var textElement = field.Q<TextElement>();

                bool allElementsExist = inputElement != null && arrowElement != null && textElement != null;
                bool timedOut = retryCount >= MaxRetries || stopwatch.ElapsedMilliseconds >= TimeoutMs;

                return allElementsExist || timedOut;
            }).Every(PollIntervalMs);
        }

        /// <summary>
        /// Applies styles to the input element (background, borders, colors).
        /// </summary>
        private static void ApplyInputStyles(VisualElement inputElement)
        {
            inputElement.style.backgroundColor = Tokens.Colors.BgSurface;
            inputElement.style.borderTopColor = Tokens.Colors.BorderSubtle;
            inputElement.style.borderRightColor = Tokens.Colors.BorderSubtle;
            inputElement.style.borderBottomColor = Tokens.Colors.BorderSubtle;
            inputElement.style.borderLeftColor = Tokens.Colors.BorderSubtle;
            inputElement.style.color = Tokens.Colors.TextPrimary;
        }

        /// <summary>
        /// Applies styles to the arrow element.
        /// </summary>
        private static void ApplyArrowStyles(VisualElement arrowElement)
        {
            arrowElement.style.unityBackgroundImageTintColor = Tokens.Colors.TextMuted;
        }

        /// <summary>
        /// Applies styles to the text element.
        /// </summary>
        private static void ApplyTextStyles(TextElement textElement)
        {
            textElement.style.color = Tokens.Colors.TextPrimary;
        }

        /// <summary>
        /// Applies styles to the label element (hide it).
        /// </summary>
        private static void ApplyLabelStyles(Label labelElement)
        {
            labelElement.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Registers hover callbacks for interactive color changes (without closures).
        /// </summary>
        private static void RegisterHoverCallbacks(VisualElement inputElement, int elementId)
        {
            inputElement.RegisterCallback<MouseEnterEvent>(OnPopupFieldMouseEnter);
            inputElement.RegisterCallback<MouseLeaveEvent>(OnPopupFieldMouseLeave);

            HoverCallbacksRegistered.Add(elementId);
        }

        private static void OnPopupFieldMouseEnter(MouseEnterEvent evt)
        {
            var element = (VisualElement)evt.currentTarget;
            element.style.backgroundColor = Tokens.Colors.BgHover;
            element.style.borderTopColor = Tokens.Colors.Border;
            element.style.borderRightColor = Tokens.Colors.Border;
            element.style.borderBottomColor = Tokens.Colors.Border;
            element.style.borderLeftColor = Tokens.Colors.Border;
        }

        private static void OnPopupFieldMouseLeave(MouseLeaveEvent evt)
        {
            var element = (VisualElement)evt.currentTarget;
            element.style.backgroundColor = Tokens.Colors.BgSurface;
            element.style.borderTopColor = Tokens.Colors.BorderSubtle;
            element.style.borderRightColor = Tokens.Colors.BorderSubtle;
            element.style.borderBottomColor = Tokens.Colors.BorderSubtle;
            element.style.borderLeftColor = Tokens.Colors.BorderSubtle;
        }

        /// <summary>
        /// Clears the styled elements cache. Useful for testing or cleanup.
        /// </summary>
        public static void ClearCache()
        {
            StyledElements.Clear();
            HoverCallbacksRegistered.Clear();
        }
    }
}
