// JProgressBar.cs
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

using JEngine.UI.Editor.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace JEngine.UI.Editor.Components.Feedback
{
    /// <summary>
    /// A progress bar indicator.
    /// </summary>
    public class JProgressBar : JComponent
    {
        private readonly VisualElement _fill;
        private float _progress;
        private bool _useSuccessColor;

        /// <summary>
        /// Creates a new progress bar.
        /// </summary>
        /// <param name="initialProgress">Initial progress value (0-1).</param>
        public JProgressBar(float initialProgress = 0f) : base("j-progress-bar")
        {
            // Apply container styles
            style.height = 8;
            style.backgroundColor = Tokens.Colors.BgSurface;
            style.borderTopLeftRadius = Tokens.BorderRadius.Sm;
            style.borderTopRightRadius = Tokens.BorderRadius.Sm;
            style.borderBottomLeftRadius = Tokens.BorderRadius.Sm;
            style.borderBottomRightRadius = Tokens.BorderRadius.Sm;
            style.overflow = Overflow.Hidden;

            // Create fill element
            _fill = new VisualElement();
            _fill.AddToClassList("j-progress-bar__fill");
            _fill.style.height = Length.Percent(100);
            _fill.style.backgroundColor = Tokens.Colors.Primary;
            _fill.style.borderTopLeftRadius = Tokens.BorderRadius.Sm;
            _fill.style.borderTopRightRadius = Tokens.BorderRadius.Sm;
            _fill.style.borderBottomLeftRadius = Tokens.BorderRadius.Sm;
            _fill.style.borderBottomRightRadius = Tokens.BorderRadius.Sm;
            Add(_fill);

            SetProgress(initialProgress);
        }

        /// <summary>
        /// Gets the fill element.
        /// </summary>
        public VisualElement Fill => _fill;

        /// <summary>
        /// Gets or sets the progress value (0-1).
        /// </summary>
        public float Progress
        {
            get => _progress;
            set => SetProgress(value);
        }

        /// <summary>
        /// Sets the progress value.
        /// </summary>
        /// <param name="value">Progress value (0-1).</param>
        /// <returns>This progress bar for chaining.</returns>
        public JProgressBar SetProgress(float value)
        {
            _progress = Mathf.Clamp01(value);
            _fill.style.width = Length.Percent(_progress * 100);
            return this;
        }

        /// <summary>
        /// Uses success color when progress reaches 100%.
        /// </summary>
        /// <param name="useSuccess">Whether to use success color.</param>
        /// <returns>This progress bar for chaining.</returns>
        public JProgressBar WithSuccessOnComplete(bool useSuccess = true)
        {
            _useSuccessColor = useSuccess;
            UpdateFillColor();
            return this;
        }

        private void UpdateFillColor()
        {
            if (_useSuccessColor && _progress >= 1f)
            {
                _fill.style.backgroundColor = Tokens.Colors.Success;
                _fill.AddToClassList("j-progress-bar__fill--success");
            }
            else
            {
                _fill.style.backgroundColor = Tokens.Colors.Primary;
                _fill.RemoveFromClassList("j-progress-bar__fill--success");
            }
        }

        /// <summary>
        /// Sets the height of the progress bar.
        /// </summary>
        /// <param name="height">The height in pixels.</param>
        /// <returns>This progress bar for chaining.</returns>
        public JProgressBar WithHeight(float height)
        {
            style.height = height;
            return this;
        }

        /// <summary>
        /// Sets the fill color.
        /// </summary>
        /// <param name="color">The fill color.</param>
        /// <returns>This progress bar for chaining.</returns>
        public JProgressBar WithColor(Color color)
        {
            _fill.style.backgroundColor = color;
            return this;
        }

        /// <summary>
        /// Sets the fill color using a button variant.
        /// </summary>
        /// <param name="variant">The variant to use for color.</param>
        /// <returns>This progress bar for chaining.</returns>
        public JProgressBar WithVariant(ButtonVariant variant)
        {
            _fill.style.backgroundColor = JTheme.GetButtonColor(variant);
            return this;
        }
    }
}
