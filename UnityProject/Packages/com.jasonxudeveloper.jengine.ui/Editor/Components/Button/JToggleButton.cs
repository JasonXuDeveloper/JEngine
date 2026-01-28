// JToggleButton.cs
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
    /// A two-state toggle button with configurable text and colors.
    /// </summary>
    public class JToggleButton : UnityEngine.UIElements.Button
    {
        private bool _isOn;
        private string _onText;
        private string _offText;
        private ButtonVariant _onVariant;
        private ButtonVariant _offVariant;
        private Action<bool> _onValueChanged;

        /// <summary>
        /// Creates a new toggle button.
        /// </summary>
        /// <param name="onText">Text when toggle is on.</param>
        /// <param name="offText">Text when toggle is off.</param>
        /// <param name="initialValue">Initial toggle state.</param>
        /// <param name="onVariant">Color variant when on.</param>
        /// <param name="offVariant">Color variant when off.</param>
        /// <param name="onValueChanged">Callback when value changes.</param>
        public JToggleButton(
            string onText,
            string offText,
            bool initialValue = false,
            ButtonVariant onVariant = ButtonVariant.Success,
            ButtonVariant offVariant = ButtonVariant.Danger,
            Action<bool> onValueChanged = null)
        {
            _onText = onText;
            _offText = offText;
            _onVariant = onVariant;
            _offVariant = offVariant;
            _onValueChanged = onValueChanged;

            AddToClassList("j-toggle-button");
            ApplyBaseStyles();

            // Set initial state
            SetValue(initialValue, notify: false);

            // Register click handler
            clicked += OnClicked;

            // Register hover events
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        /// <summary>
        /// Gets or sets the toggle value.
        /// </summary>
        public bool Value
        {
            get => _isOn;
            set => SetValue(value, notify: true);
        }

        /// <summary>
        /// Gets or sets the value changed callback.
        /// </summary>
        public Action<bool> OnValueChanged
        {
            get => _onValueChanged;
            set => _onValueChanged = value;
        }

        private void ApplyBaseStyles()
        {
            style.borderTopLeftRadius = Tokens.BorderRadius.Sm;
            style.borderTopRightRadius = Tokens.BorderRadius.Sm;
            style.borderBottomLeftRadius = Tokens.BorderRadius.Sm;
            style.borderBottomRightRadius = Tokens.BorderRadius.Sm;
            style.paddingTop = 4;
            style.paddingRight = 10;
            style.paddingBottom = 4;
            style.paddingLeft = 10;
            // Remove all margins (like JDropdown does)
            style.marginLeft = 0;
            style.marginRight = 0;
            style.marginTop = 0;
            style.marginBottom = 0;
            style.minHeight = 22;
            style.maxHeight = 24;
            style.fontSize = 11;
            style.unityFontStyleAndWeight = FontStyle.Normal;
            // No border for cleaner look
            style.borderTopWidth = 0;
            style.borderRightWidth = 0;
            style.borderBottomWidth = 0;
            style.borderLeftWidth = 0;
            // Text color set in UpdateVisuals based on variant
            // Don't set flexGrow/flexShrink here - let parent control layout
            style.overflow = Overflow.Hidden;
            style.textOverflow = TextOverflow.Ellipsis;
            style.whiteSpace = WhiteSpace.NoWrap;
        }

        private void OnClicked()
        {
            SetValue(!_isOn, notify: true);
        }

        /// <summary>
        /// Sets the toggle value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="notify">Whether to invoke the callback.</param>
        public void SetValue(bool value, bool notify = true)
        {
            _isOn = value;
            UpdateVisuals();

            if (notify)
            {
                _onValueChanged?.Invoke(_isOn);
            }
        }

        private void UpdateVisuals()
        {
            text = _isOn ? _onText : _offText;
            var variant = _isOn ? _onVariant : _offVariant;
            style.backgroundColor = JTheme.GetButtonColor(variant);

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
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            if (!enabledSelf) return;
            var variant = _isOn ? _onVariant : _offVariant;
            style.backgroundColor = JTheme.GetButtonHoverColor(variant);
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            var variant = _isOn ? _onVariant : _offVariant;
            style.backgroundColor = JTheme.GetButtonColor(variant);
        }

        /// <summary>
        /// Sets the on-state text.
        /// </summary>
        /// <param name="text">The text when on.</param>
        /// <returns>This button for chaining.</returns>
        public JToggleButton WithOnText(string text)
        {
            _onText = text;
            UpdateVisuals();
            return this;
        }

        /// <summary>
        /// Sets the off-state text.
        /// </summary>
        /// <param name="text">The text when off.</param>
        /// <returns>This button for chaining.</returns>
        public JToggleButton WithOffText(string text)
        {
            _offText = text;
            UpdateVisuals();
            return this;
        }

        /// <summary>
        /// Sets the on-state variant.
        /// </summary>
        /// <param name="variant">The variant when on.</param>
        /// <returns>This button for chaining.</returns>
        public JToggleButton WithOnVariant(ButtonVariant variant)
        {
            _onVariant = variant;
            UpdateVisuals();
            return this;
        }

        /// <summary>
        /// Sets the off-state variant.
        /// </summary>
        /// <param name="variant">The variant when off.</param>
        /// <returns>This button for chaining.</returns>
        public JToggleButton WithOffVariant(ButtonVariant variant)
        {
            _offVariant = variant;
            UpdateVisuals();
            return this;
        }

        /// <summary>
        /// Sets the button to fill available width.
        /// </summary>
        /// <returns>This button for chaining.</returns>
        public JToggleButton FullWidth()
        {
            style.flexGrow = 1;
            style.maxHeight = 24;
            return this;
        }

        /// <summary>
        /// Adds a CSS class.
        /// </summary>
        /// <param name="className">The class name.</param>
        /// <returns>This button for chaining.</returns>
        public JToggleButton WithClass(string className)
        {
            AddToClassList(className);
            return this;
        }
    }
}
