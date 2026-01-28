// JStatusBar.cs
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
    /// A status bar with colored accent based on status type.
    /// </summary>
    public class JStatusBar : JComponent
    {
        private readonly Label _textLabel;
        private StatusType _status;

        /// <summary>
        /// Creates a new status bar with neutral monochrome styling.
        /// </summary>
        /// <param name="text">The status text.</param>
        /// <param name="status">The status type (Info, Success, Warning, Error).</param>
        public JStatusBar(string text = "", StatusType status = StatusType.Info) : base("j-status-bar")
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.paddingTop = Tokens.Spacing.MD;
            style.paddingRight = Tokens.Spacing.Lg;
            style.paddingBottom = Tokens.Spacing.MD;
            style.paddingLeft = Tokens.Spacing.Lg;

            // Border radius (8px)
            style.borderTopLeftRadius = Tokens.BorderRadius.MD;
            style.borderTopRightRadius = Tokens.BorderRadius.MD;
            style.borderBottomLeftRadius = Tokens.BorderRadius.MD;
            style.borderBottomRightRadius = Tokens.BorderRadius.MD;

            style.marginBottom = Tokens.Spacing.MD;

            // Thick accent border on left (3px)
            style.borderLeftWidth = 3;

            _textLabel = new Label(text);
            _textLabel.AddToClassList("j-status-bar__text");
            _textLabel.style.color = Tokens.Colors.TextPrimary;
            _textLabel.style.fontSize = Tokens.FontSize.Base;
            Add(_textLabel);

            SetStatus(status);
        }

        /// <summary>
        /// Gets the text label.
        /// </summary>
        public Label TextLabel => _textLabel;

        /// <summary>
        /// Gets or sets the status text.
        /// </summary>
        public string Text
        {
            get => _textLabel.text;
            set => _textLabel.text = value;
        }

        /// <summary>
        /// Gets or sets the status type.
        /// </summary>
        public StatusType Status
        {
            get => _status;
            set => SetStatus(value);
        }

        /// <summary>
        /// Sets the status type.
        /// </summary>
        /// <param name="status">The status type.</param>
        /// <returns>This status bar for chaining.</returns>
        public JStatusBar SetStatus(StatusType status)
        {
            // Remove existing status classes
            RemoveFromClassList("j-status-bar--info");
            RemoveFromClassList("j-status-bar--success");
            RemoveFromClassList("j-status-bar--warning");
            RemoveFromClassList("j-status-bar--error");

            _status = status;

            // Monochrome design: neutral grey backgrounds in both themes
            Color bgColor;
            Color accentColor;
            string statusClass;

            // Monochrome design: neutral grey backgrounds and borders in both themes
            var neutralBg = Tokens.Colors.BgSurface;
            var neutralBorder = Tokens.Colors.Border;
            (bgColor, accentColor, statusClass) = status switch
            {
                StatusType.Info => (neutralBg, neutralBorder, "j-status-bar--info"),
                StatusType.Success => (neutralBg, neutralBorder, "j-status-bar--success"),
                StatusType.Warning => (neutralBg, neutralBorder, "j-status-bar--warning"),
                StatusType.Error => (neutralBg, neutralBorder, "j-status-bar--error"),
                _ => (neutralBg, neutralBorder, "j-status-bar--info")
            };

            AddToClassList(statusClass);
            style.backgroundColor = bgColor;
            style.borderLeftColor = accentColor;

            return this;
        }

        /// <summary>
        /// Sets the status text.
        /// </summary>
        /// <param name="text">The new text.</param>
        /// <returns>This status bar for chaining.</returns>
        public JStatusBar WithText(string text)
        {
            _textLabel.text = text;
            return this;
        }
    }
}
