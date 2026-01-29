// JTextField.cs
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
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace JEngine.UI.Editor.Components.Form
{
    /// <summary>
    /// A styled text field matching the JEngine dark theme.
    /// </summary>
    public class JTextField : VisualElement
    {
        private readonly TextField _textField;
        private readonly VisualElement _inputContainer;

        /// <summary>
        /// Creates a new styled text field.
        /// </summary>
        /// <param name="initialValue">Initial text value.</param>
        /// <param name="placeholder">Placeholder text when empty.</param>
        public JTextField(string initialValue = "", string placeholder = "")
        {
            AddToClassList("j-text-field");

            // Create internal TextField
            _textField = new TextField
            {
                value = initialValue
            };

            // Style the container using shared input styles
            JTheme.ApplyInputContainerStyle(this);

            // Access the internal input element and style it
            _textField.RegisterCallback<AttachToPanelEvent, JTextField>(static (_, field) =>
            {
                field.ApplyInternalStyles();
            }, this);

            // Apply container styles
            _textField.style.flexGrow = 1;
            _textField.style.marginLeft = 0;
            _textField.style.marginRight = 0;
            _textField.style.marginTop = 0;
            _textField.style.marginBottom = 0;

            Add(_textField);

            // Set placeholder via schedule (need element to be attached)
            if (!string.IsNullOrEmpty(placeholder))
            {
                schedule.Execute(() =>
                {
                    var input = _textField.Q<TextElement>();
                    if (input != null && string.IsNullOrEmpty(_textField.value))
                    {
                        // Unity doesn't have native placeholder, use workaround
                    }
                });
            }
        }

        private void ApplyInternalStyles()
        {
            // Style the text input element using shared styles
            var textInput = _textField.Q(className: "unity-text-field__input");
            if (textInput != null)
            {
                JTheme.ApplyInputElementStyle(textInput);
                textInput.style.fontSize = Tokens.FontSize.Sm;

                // Hover state
                textInput.RegisterCallback<MouseEnterEvent, VisualElement>(
                    static (_, input) => JTheme.ApplyInputHoverState(input), textInput);
                textInput.RegisterCallback<MouseLeaveEvent, VisualElement>(
                    static (_, input) => JTheme.ApplyInputNormalState(input), textInput);

                // Focus state
                _textField.RegisterCallback<FocusInEvent, VisualElement>(
                    static (_, input) => JTheme.ApplyInputFocusState(input), textInput);
                _textField.RegisterCallback<FocusOutEvent, VisualElement>(
                    static (_, input) => JTheme.ApplyInputNormalState(input), textInput);

                // Text cursor on input and its text element
                JTheme.ApplyTextCursor(textInput);
                var textElement = textInput.Q<TextElement>();
                if (textElement != null)
                {
                    JTheme.ApplyTextCursor(textElement);
                }
            }

            // Hide the label if present
            JTheme.HideFieldLabel(_textField);
        }

        /// <summary>
        /// Gets or sets the text value.
        /// </summary>
        public string Value
        {
            get => _textField.value;
            set => _textField.value = value;
        }

        /// <summary>
        /// Gets the internal TextField for binding.
        /// </summary>
        public TextField TextField => _textField;

        /// <summary>
        /// Registers a callback for value changes.
        /// </summary>
        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<string>> callback)
        {
            _textField.RegisterValueChangedCallback(callback);
        }

        /// <summary>
        /// Binds to a serialized property.
        /// </summary>
        public void BindProperty(SerializedProperty property)
        {
            _textField.BindProperty(property);
        }

        /// <summary>
        /// Sets whether the field is read-only.
        /// </summary>
        public JTextField SetReadOnly(bool readOnly)
        {
            _textField.isReadOnly = readOnly;
            return this;
        }

        /// <summary>
        /// Sets whether the field supports multiline.
        /// </summary>
        public JTextField SetMultiline(bool multiline)
        {
            _textField.multiline = multiline;
            return this;
        }
    }
}
