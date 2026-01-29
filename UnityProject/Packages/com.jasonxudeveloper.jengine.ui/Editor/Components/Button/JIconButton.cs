// JIconButton.cs
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
using JEngine.UI.Editor.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace JEngine.UI.Editor.Components.Button
{
    /// <summary>
    /// A small icon button with transparent background, suitable for toolbars and inline actions.
    /// </summary>
    public class JIconButton : UnityEngine.UIElements.Button
    {
        /// <summary>
        /// Creates a new icon button.
        /// </summary>
        /// <param name="text">The button text (typically an icon character or short text).</param>
        /// <param name="onClick">Click handler.</param>
        /// <param name="tooltip">Optional tooltip.</param>
        public JIconButton(string text, Action onClick = null, string tooltip = null)
        {
            this.text = text;
            this.tooltip = tooltip;

            AddToClassList("j-icon-button");
            ApplyStyles();

            if (onClick != null)
            {
                clicked += onClick;
            }

            // Hover events
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        private void ApplyStyles()
        {
            // Small size
            style.width = 22;
            style.height = 18;
            style.minWidth = 18;
            style.minHeight = 18;

            // No padding/margin
            style.marginLeft = 2;
            style.marginRight = 0;
            style.marginTop = 0;
            style.marginBottom = 0;
            style.paddingLeft = 0;
            style.paddingRight = 0;
            style.paddingTop = 0;
            style.paddingBottom = 0;

            // Transparent background
            style.backgroundColor = Color.clear;

            // No border
            style.borderTopWidth = 0;
            style.borderRightWidth = 0;
            style.borderBottomWidth = 0;
            style.borderLeftWidth = 0;
            style.borderTopLeftRadius = Tokens.BorderRadius.Sm;
            style.borderTopRightRadius = Tokens.BorderRadius.Sm;
            style.borderBottomLeftRadius = Tokens.BorderRadius.Sm;
            style.borderBottomRightRadius = Tokens.BorderRadius.Sm;

            // Text styling
            style.fontSize = Tokens.FontSize.Xs;
            style.color = Tokens.Colors.TextMuted;
            style.unityTextAlign = TextAnchor.MiddleCenter;

            // Pointer cursor
            JTheme.ApplyPointerCursor(this);

            // Smooth transitions
            JTheme.ApplyTransition(this);
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            style.color = Tokens.Colors.TextPrimary;
            style.backgroundColor = Tokens.Colors.BgElevated;
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            style.color = Tokens.Colors.TextMuted;
            style.backgroundColor = Color.clear;
        }

        /// <summary>
        /// Sets the button tooltip.
        /// </summary>
        public JIconButton WithTooltip(string tooltip)
        {
            this.tooltip = tooltip;
            return this;
        }

        /// <summary>
        /// Sets custom size for the button.
        /// </summary>
        public JIconButton WithSize(float width, float height)
        {
            style.width = width;
            style.height = height;
            return this;
        }
    }
}
