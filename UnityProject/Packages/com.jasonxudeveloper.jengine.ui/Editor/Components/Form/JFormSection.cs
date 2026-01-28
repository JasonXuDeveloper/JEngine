// JFormSection.cs
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

namespace JEngine.UI.Editor.Components.Form
{
    /// <summary>
    /// A section containing grouped form fields with an optional header.
    /// </summary>
    public class JFormSection : JComponent
    {
        private readonly Label _header;
        private readonly VisualElement _fields;

        /// <summary>
        /// Creates a new form section with an optional header.
        /// </summary>
        /// <param name="headerText">The section header text, or null for no header.</param>
        public JFormSection(string headerText = null) : base("j-form-section")
        {
            style.marginBottom = Tokens.Spacing.Lg;

            // Create header if provided
            if (!string.IsNullOrEmpty(headerText))
            {
                _header = new Label(headerText);
                _header.AddToClassList("j-form-section__header");
                _header.style.color = Tokens.Colors.TextMuted;
                _header.style.fontSize = Tokens.FontSize.Sm;
                _header.style.unityFontStyleAndWeight = FontStyle.Bold;
                _header.style.marginBottom = Tokens.Spacing.MD;
                base.Add(_header);
            }

            // Create fields container
            _fields = new VisualElement();
            _fields.style.flexDirection = FlexDirection.Column;
            base.Add(_fields);
        }

        /// <summary>
        /// Gets the header label.
        /// </summary>
        public Label Header => _header;

        /// <summary>
        /// Gets the fields container.
        /// </summary>
        public VisualElement Fields => _fields;

        /// <summary>
        /// Adds a form field with label and control.
        /// </summary>
        /// <param name="label">The field label.</param>
        /// <param name="control">The control element.</param>
        /// <returns>This form section for chaining.</returns>
        public JFormSection AddField(string label, VisualElement control)
        {
            var field = new JFormField(label, control);
            _fields.Add(field);
            return this;
        }

        /// <summary>
        /// Adds a pre-built form field.
        /// </summary>
        /// <param name="field">The form field to add.</param>
        /// <returns>This form section for chaining.</returns>
        public JFormSection AddField(JFormField field)
        {
            _fields.Add(field);
            return this;
        }

        /// <summary>
        /// Adds child elements to the fields container.
        /// </summary>
        public new JFormSection Add(params VisualElement[] children)
        {
            foreach (var child in children)
            {
                if (child != null)
                {
                    _fields.Add(child);
                }
            }
            return this;
        }

        /// <summary>
        /// Sets the header text.
        /// </summary>
        /// <param name="text">The new header text.</param>
        /// <returns>This form section for chaining.</returns>
        public JFormSection WithHeader(string text)
        {
            if (_header != null)
            {
                _header.text = text;
            }
            return this;
        }
    }
}
