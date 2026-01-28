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
using JEngine.UI.Editor.Utilities;
using UnityEngine.UIElements;

namespace JEngine.UI.Editor.Components.Form
{
    /// <summary>
    /// A styled dropdown matching the JEngine dark theme.
    /// </summary>
    public class JDropdown : VisualElement
    {
        private readonly PopupField<string> _popupField;
        private Action<string> _onValueChanged;
        private bool _stylesApplied;

        /// <summary>
        /// Creates a new styled dropdown.
        /// </summary>
        /// <param name="choices">Available choices.</param>
        /// <param name="defaultValue">Default selected value.</param>
        public JDropdown(List<string> choices, string defaultValue = null)
        {
            AddToClassList("j-dropdown");

            // Ensure choices list is valid
            if (choices == null || choices.Count == 0)
            {
                choices = new List<string> { "" };
            }

            // Determine initial index
            int defaultIndex = 0;
            if (!string.IsNullOrEmpty(defaultValue) && choices.Contains(defaultValue))
            {
                defaultIndex = choices.IndexOf(defaultValue);
            }

            _popupField = new PopupField<string>(choices, defaultIndex);

            // Style the container
            style.flexGrow = 1;
            style.flexShrink = 1;
            style.minWidth = 80;
            style.minHeight = 20;
            style.maxHeight = 24;
            style.alignSelf = Align.Center;

            // Apply styles
            _popupField.style.flexGrow = 1;
            _popupField.style.marginLeft = 0;
            _popupField.style.marginRight = 0;
            _popupField.style.marginTop = 0;
            _popupField.style.marginBottom = 0;

            Add(_popupField);

            // Apply styles using GeometryChangedEvent (fires after layout)
            _popupField.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            // Apply styles only once
            if (_stylesApplied) return;

            // Unregister to prevent multiple calls
            _popupField.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            // Use PopupFieldStyleHelper for reliable styling
            PopupFieldStyleHelper.ApplyStylesToPopupField(
                _popupField,
                enableHoverEffects: true,
                enableDebugLogging: false); // Set to true for debugging

            _stylesApplied = true;
        }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        public string Value
        {
            get => _popupField.value;
            set => _popupField.value = value;
        }

        /// <summary>
        /// Gets the internal PopupField for advanced operations.
        /// </summary>
        public PopupField<string> PopupField => _popupField;

        /// <summary>
        /// Gets or sets the choices.
        /// </summary>
        public List<string> Choices
        {
            get => _popupField.choices;
            set => _popupField.choices = value;
        }

        /// <summary>
        /// Registers a callback for value changes.
        /// </summary>
        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<string>> callback)
        {
            _popupField.RegisterValueChangedCallback(callback);
        }

        /// <summary>
        /// Sets the value changed callback.
        /// </summary>
        public JDropdown OnValueChanged(Action<string> callback)
        {
            _onValueChanged = callback;
            _popupField.RegisterValueChangedCallback(evt => _onValueChanged?.Invoke(evt.newValue));
            return this;
        }
    }
}
