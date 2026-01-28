// JButton.cs
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
    /// A themed button with variant styling.
    /// </summary>
    public class JButton : UnityEngine.UIElements.Button
    {
        private ButtonVariant _variant;
        private Color _baseColor;
        private Color _hoverColor;
        private Color _activeColor;

        /// <summary>
        /// Creates a new button with text and optional click action.
        /// </summary>
        /// <param name="text">The button text.</param>
        /// <param name="onClick">Optional click handler.</param>
        /// <param name="variant">The button variant (Primary, Secondary, Success, Danger, Warning).</param>
        public JButton(string text, Action onClick = null, ButtonVariant variant = ButtonVariant.Primary)
        {
            this.text = text;
            _variant = variant;

            AddToClassList("j-button");

            // Apply base styles
            ApplyBaseStyles();

            // Apply variant
            SetVariant(variant);

            // Register click handler
            if (onClick != null)
            {
                clicked += onClick;
            }

            // Register hover/active events for color changes
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<FocusInEvent>(OnFocusIn);
            RegisterCallback<FocusOutEvent>(OnFocusOut);
            RegisterCallback<ClickEvent>(OnClick);
        }

        /// <summary>
        /// Gets or sets the button variant.
        /// </summary>
        public ButtonVariant Variant
        {
            get => _variant;
            set => SetVariant(value);
        }

        private void ApplyBaseStyles()
        {
            // Glassmorphic border radius (8px)
            style.borderTopLeftRadius = Tokens.BorderRadius.MD;
            style.borderTopRightRadius = Tokens.BorderRadius.MD;
            style.borderBottomLeftRadius = Tokens.BorderRadius.MD;
            style.borderBottomRightRadius = Tokens.BorderRadius.MD;

            // Enhanced padding (6px vertical, 14px horizontal)
            style.paddingTop = Tokens.Spacing.Sm;
            style.paddingRight = Tokens.Spacing.Lg;
            style.paddingBottom = Tokens.Spacing.Sm;
            style.paddingLeft = Tokens.Spacing.Lg;

            // Remove all margins
            style.marginLeft = 0;
            style.marginRight = 0;
            style.marginTop = 0;
            style.marginBottom = 0;

            // Enhanced min-height (28px instead of 22px)
            style.minHeight = 28;

            // Font styling
            style.fontSize = Tokens.FontSize.Base;
            style.unityFontStyleAndWeight = FontStyle.Bold;

            // No border for glassmorphic look
            style.borderTopWidth = 0;
            style.borderRightWidth = 0;
            style.borderBottomWidth = 0;
            style.borderLeftWidth = 0;

            // Don't set flexGrow/flexShrink here - let parent control layout
            style.overflow = Overflow.Hidden;
            style.textOverflow = TextOverflow.Ellipsis;
            style.whiteSpace = WhiteSpace.NoWrap;

            // Smooth glassmorphic transitions
            JTheme.ApplyTransition(this);
        }

        /// <summary>
        /// Sets the button variant and updates colors.
        /// </summary>
        /// <param name="variant">The variant to apply.</param>
        /// <returns>This button for chaining.</returns>
        public JButton SetVariant(ButtonVariant variant)
        {
            // Remove existing variant classes
            RemoveFromClassList("j-button--primary");
            RemoveFromClassList("j-button--secondary");
            RemoveFromClassList("j-button--success");
            RemoveFromClassList("j-button--danger");
            RemoveFromClassList("j-button--warning");

            _variant = variant;
            _baseColor = JTheme.GetButtonColor(variant);
            _hoverColor = JTheme.GetButtonHoverColor(variant);
            _activeColor = JTheme.GetButtonActiveColor(variant);

            // Add new variant class
            var variantClass = variant switch
            {
                ButtonVariant.Primary => "j-button--primary",
                ButtonVariant.Secondary => "j-button--secondary",
                ButtonVariant.Success => "j-button--success",
                ButtonVariant.Danger => "j-button--danger",
                ButtonVariant.Warning => "j-button--warning",
                _ => "j-button--primary"
            };
            AddToClassList(variantClass);

            // Apply base color
            style.backgroundColor = _baseColor;

            // Set text color based on variant and theme
            // Secondary in light mode: black text (light background)
            // All others: white text
            if (variant == ButtonVariant.Secondary && !Tokens.IsDarkTheme)
            {
                style.color = Tokens.Colors.TextPrimary; // Black in light mode
            }
            else
            {
                style.color = Color.white; // White for all others
            }

            return this;
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            if (!enabledSelf) return;
            style.backgroundColor = _hoverColor;
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            style.backgroundColor = _baseColor;
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (!enabledSelf) return;
            style.backgroundColor = _activeColor;
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (!enabledSelf) return;
            style.backgroundColor = _hoverColor;
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            // Add subtle focus ring for keyboard navigation
            style.borderTopWidth = 1;
            style.borderRightWidth = 1;
            style.borderBottomWidth = 1;
            style.borderLeftWidth = 1;

            // Light mode: lighter grey border
            // Dark mode: cyan border
            var focusColor = Tokens.IsDarkTheme
                ? Tokens.Colors.BorderFocus
                : Tokens.Colors.BorderSubtle;

            style.borderTopColor = focusColor;
            style.borderRightColor = focusColor;
            style.borderBottomColor = focusColor;
            style.borderLeftColor = focusColor;
        }

        private void OnFocusOut(FocusOutEvent evt)
        {
            // Remove focus ring
            style.borderTopWidth = 0;
            style.borderRightWidth = 0;
            style.borderBottomWidth = 0;
            style.borderLeftWidth = 0;
        }

        private void OnClick(ClickEvent evt)
        {
            // Blur the button after click to remove focus border
            Blur();
        }

        /// <summary>
        /// Sets the button text.
        /// </summary>
        /// <param name="buttonText">The new text.</param>
        /// <returns>This button for chaining.</returns>
        public JButton WithText(string buttonText)
        {
            text = buttonText;
            return this;
        }

        /// <summary>
        /// Adds a CSS class.
        /// </summary>
        /// <param name="className">The class name.</param>
        /// <returns>This button for chaining.</returns>
        public JButton WithClass(string className)
        {
            AddToClassList(className);
            return this;
        }

        /// <summary>
        /// Sets whether the button is enabled.
        /// </summary>
        /// <param name="isEnabled">Whether the button should be enabled.</param>
        /// <returns>This button for chaining.</returns>
        public JButton WithEnabled(bool isEnabled)
        {
            SetEnabled(isEnabled);
            return this;
        }

        /// <summary>
        /// Sets the button to fill available width.
        /// </summary>
        /// <returns>This button for chaining.</returns>
        public JButton FullWidth()
        {
            style.flexGrow = 1;
            style.flexShrink = 1;
            style.minWidth = 60;
            style.maxHeight = 26;
            // Remove horizontal padding for edge-to-edge fill
            style.paddingLeft = 0;
            style.paddingRight = 0;
            return this;
        }

        /// <summary>
        /// Makes the button compact (smaller padding).
        /// </summary>
        /// <returns>This button for chaining.</returns>
        public JButton Compact()
        {
            style.paddingTop = 2;
            style.paddingBottom = 2;
            style.paddingLeft = 6;
            style.paddingRight = 6;
            style.minHeight = 18;
            style.maxHeight = 20;
            style.fontSize = 10;
            return this;
        }

        /// <summary>
        /// Sets the minimum width.
        /// </summary>
        /// <returns>This button for chaining.</returns>
        public JButton WithMinWidth(float minWidth)
        {
            style.minWidth = minWidth;
            return this;
        }
    }
}
