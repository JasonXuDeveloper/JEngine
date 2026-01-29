// JFormField.cs
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
    /// A form field with label and control in a two-column layout.
    /// </summary>
    public class JFormField : JComponent
    {
        private readonly Label _label;
        private readonly VisualElement _controlContainer;

        /// <summary>
        /// Creates a new form field with a label and control.
        /// </summary>
        /// <param name="labelText">The label text.</param>
        /// <param name="control">The control element (TextField, PopupField, etc.).</param>
        public JFormField(string labelText, VisualElement control = null) : base("j-form-field")
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.flexWrap = Wrap.NoWrap;
            style.marginBottom = Tokens.Spacing.Sm;
            style.minHeight = 22;
            style.maxHeight = 28;
            style.overflow = Overflow.Hidden;

            // Create label - compact, using shared tokens
            _label = new Label(labelText);
            _label.AddToClassList("j-form-field__label");
            _label.style.width = Tokens.Layout.FormLabelWidth;
            _label.style.minWidth = Tokens.Layout.FormLabelMinWidth;
            _label.style.maxWidth = 180;
            _label.style.color = Tokens.Colors.TextSecondary;
            _label.style.fontSize = Tokens.FontSize.Sm;
            _label.style.paddingRight = Tokens.Spacing.Sm;
            _label.style.flexShrink = 1;
            _label.style.overflow = Overflow.Hidden;
            _label.style.textOverflow = TextOverflow.Ellipsis;
            _label.style.whiteSpace = WhiteSpace.NoWrap;
            _label.style.unityTextAlign = TextAnchor.MiddleLeft;
            base.Add(_label);

            // Create control container
            _controlContainer = new VisualElement();
            _controlContainer.AddToClassList("j-form-field__control");
            _controlContainer.style.flexGrow = 1;
            _controlContainer.style.flexShrink = 1;
            _controlContainer.style.flexDirection = FlexDirection.Row;
            _controlContainer.style.alignItems = Align.Center;
            _controlContainer.style.minWidth = 80;
            _controlContainer.style.overflow = Overflow.Hidden;
            base.Add(_controlContainer);

            // Add control if provided
            if (control != null)
            {
                // Only JToggle should be fixed-size, all other controls (including buttons) should be responsive
                bool isFixedSize = control is JToggle;
                if (!isFixedSize)
                {
                    control.style.flexGrow = 1;
                    control.style.flexShrink = 1;
                }
                control.style.alignSelf = Align.Center;
                _controlContainer.Add(control);
            }
        }

        /// <summary>
        /// Gets the label element.
        /// </summary>
        public Label Label => _label;

        /// <summary>
        /// Gets the control container.
        /// </summary>
        public VisualElement ControlContainer => _controlContainer;

        /// <summary>
        /// Sets the control for this form field.
        /// </summary>
        /// <param name="control">The control element.</param>
        /// <returns>This form field for chaining.</returns>
        public JFormField WithControl(VisualElement control)
        {
            _controlContainer.Clear();
            if (control != null)
            {
                _controlContainer.Add(control);
            }
            return this;
        }

        /// <summary>
        /// Sets the label width.
        /// </summary>
        /// <param name="width">The label width in pixels.</param>
        /// <returns>This form field for chaining.</returns>
        public JFormField WithLabelWidth(float width)
        {
            _label.style.width = width;
            _label.style.minWidth = width;
            return this;
        }

        /// <summary>
        /// Hides the label (for full-width controls).
        /// </summary>
        /// <returns>This form field for chaining.</returns>
        public JFormField NoLabel()
        {
            _label.style.display = DisplayStyle.None;
            return this;
        }

        /// <summary>
        /// Adds child elements to the control container.
        /// </summary>
        public new JFormField Add(params VisualElement[] children)
        {
            foreach (var child in children)
            {
                if (child != null)
                {
                    _controlContainer.Add(child);
                }
            }
            return this;
        }
    }
}
