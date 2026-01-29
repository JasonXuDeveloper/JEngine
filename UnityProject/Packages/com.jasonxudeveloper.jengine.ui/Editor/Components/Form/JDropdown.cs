// JDropdown.cs
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
using JEngine.UI.Editor.Theming;
using JEngine.UI.Editor.Utilities;
using UnityEngine.UIElements;

namespace JEngine.UI.Editor.Components.Form
{
    /// <summary>
    /// A styled dropdown for string choices.
    /// </summary>
    public class JDropdown : JDropdown<string>
    {
        /// <summary>
        /// Creates a new styled dropdown.
        /// </summary>
        /// <param name="choices">Available choices.</param>
        /// <param name="defaultValue">Default selected value.</param>
        public JDropdown(List<string> choices, string defaultValue = null)
            : base(choices, defaultValue, v => v, v => v)
        {
        }
    }

    /// <summary>
    /// A styled generic dropdown that works with any type including enums.
    /// </summary>
    /// <typeparam name="T">The type of values in the dropdown.</typeparam>
    public class JDropdown<T> : VisualElement
    {
        private readonly PopupField<T> _popupField;
        private Action<T> _onValueChanged;
        private VisualElement _inputElement;

        /// <summary>
        /// Creates a new styled generic dropdown.
        /// </summary>
        /// <param name="choices">Available choices.</param>
        /// <param name="defaultValue">Default selected value.</param>
        /// <param name="formatSelectedValue">Function to format the selected value display.</param>
        /// <param name="formatListItem">Function to format list item display.</param>
        public JDropdown(
            List<T> choices,
            T defaultValue = default,
            Func<T, string> formatSelectedValue = null,
            Func<T, string> formatListItem = null)
        {
            AddToClassList("j-dropdown");

            // Ensure choices list is valid
            if (choices == null || choices.Count == 0)
            {
                throw new ArgumentException("Choices list cannot be null or empty", nameof(choices));
            }

            // Determine initial index
            int defaultIndex = 0;
            if (defaultValue != null && choices.Contains(defaultValue))
            {
                defaultIndex = choices.IndexOf(defaultValue);
            }

            // Create popup field with formatters (use ToString as default)
            _popupField = new PopupField<T>(
                choices,
                defaultIndex,
                formatSelectedValue ?? (v => v?.ToString() ?? ""),
                formatListItem ?? (v => v?.ToString() ?? ""));

            // Style the container using shared input styles
            JTheme.ApplyInputContainerStyle(this);

            // Apply styles to popup field
            _popupField.style.flexGrow = 1;
            _popupField.style.marginLeft = 0;
            _popupField.style.marginRight = 0;
            _popupField.style.marginTop = 0;
            _popupField.style.marginBottom = 0;

            Add(_popupField);

            // Apply styles when attached to panel
            _popupField.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            // Apply stylesheets to enable USS :hover
            StyleSheetManager.ApplyAllStyleSheets(this);

            // Also apply inline styles for theme token colors
            ApplyInternalStyles();
        }

        private void ApplyInternalStyles()
        {
            // Style the input element using shared styles
            _inputElement = _popupField.Q(className: "unity-base-popup-field__input");
            JTheme.ApplyInputElementStyle(_inputElement);

            // Register hover on the wrapper (this) - PopupField internal elements have event issues
            // Using PointerOverEvent/PointerOutEvent as alternative to MouseEnter/MouseLeave
            RegisterCallback<PointerOverEvent>(OnPointerOver);
            RegisterCallback<PointerOutEvent>(OnPointerOut);

            // Style the arrow
            var arrowElement = _popupField.Q(className: "unity-base-popup-field__arrow");
            if (arrowElement != null)
            {
                arrowElement.style.unityBackgroundImageTintColor = Tokens.Colors.TextMuted;
            }

            // Style the text using shared styles
            var textElement = _popupField.Q<TextElement>();
            JTheme.ApplyInputTextStyle(textElement);

            // Pointer cursor on all elements
            JTheme.ApplyPointerCursor(this);
            JTheme.ApplyPointerCursor(_popupField);
            JTheme.ApplyPointerCursor(_inputElement);
            if (textElement != null) JTheme.ApplyPointerCursor(textElement);
            if (arrowElement != null) JTheme.ApplyPointerCursor(arrowElement);

            // Hide the label if present
            JTheme.HideFieldLabel(_popupField);
        }

        private void OnPointerOver(PointerOverEvent evt)
        {
            JTheme.ApplyInputHoverState(_inputElement);
        }

        private void OnPointerOut(PointerOutEvent evt)
        {
            JTheme.ApplyInputNormalState(_inputElement);
        }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        public T Value
        {
            get => _popupField.value;
            set => _popupField.value = value;
        }

        /// <summary>
        /// Gets the internal PopupField for advanced operations.
        /// </summary>
        public PopupField<T> PopupField => _popupField;

        /// <summary>
        /// Gets or sets the choices.
        /// </summary>
        public List<T> Choices
        {
            get => _popupField.choices;
            set => _popupField.choices = value;
        }

        /// <summary>
        /// Registers a callback for value changes.
        /// </summary>
        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<T>> callback)
        {
            _popupField.RegisterValueChangedCallback(callback);
        }

        /// <summary>
        /// Sets the value changed callback.
        /// </summary>
        public JDropdown<T> OnValueChanged(Action<T> callback)
        {
            _onValueChanged = callback;
            _popupField.RegisterValueChangedCallback(evt => _onValueChanged?.Invoke(evt.newValue));
            return this;
        }

        /// <summary>
        /// Creates a dropdown for an enum type.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <param name="defaultValue">Default selected value.</param>
        /// <returns>A new dropdown configured for the enum.</returns>
        public static JDropdown<TEnum> ForEnum<TEnum>(TEnum defaultValue = default) where TEnum : Enum
        {
            var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
            return new JDropdown<TEnum>(
                values,
                defaultValue,
                v => v.ToString(),
                v => v.ToString());
        }
    }
}
