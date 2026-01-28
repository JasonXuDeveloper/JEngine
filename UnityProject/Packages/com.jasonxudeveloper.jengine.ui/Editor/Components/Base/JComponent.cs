// JComponent.cs
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

using UnityEngine.UIElements;

namespace JEngine.UI.Editor.Components
{
    /// <summary>
    /// Base class for all JEngine Editor UI components.
    /// Provides fluent API for common operations.
    /// </summary>
    public abstract class JComponent : VisualElement
    {
        /// <summary>
        /// Creates a new JComponent with the specified base class.
        /// </summary>
        /// <param name="baseClassName">The primary CSS class for this component.</param>
        protected JComponent(string baseClassName)
        {
            if (!string.IsNullOrEmpty(baseClassName))
            {
                AddToClassList(baseClassName);
            }
        }

        /// <summary>
        /// Adds a CSS class to this component.
        /// </summary>
        /// <param name="className">The class name to add.</param>
        /// <returns>This component for chaining.</returns>
        public JComponent WithClass(string className)
        {
            AddToClassList(className);
            return this;
        }

        /// <summary>
        /// Sets the name of this component.
        /// </summary>
        /// <param name="elementName">The element name.</param>
        /// <returns>This component for chaining.</returns>
        public JComponent WithName(string elementName)
        {
            name = elementName;
            return this;
        }

        /// <summary>
        /// Adds child elements to this component.
        /// </summary>
        /// <param name="children">The elements to add.</param>
        /// <returns>This component for chaining.</returns>
        public JComponent Add(params VisualElement[] children)
        {
            foreach (var child in children)
            {
                if (child != null)
                {
                    base.Add(child);
                }
            }
            return this;
        }

        /// <summary>
        /// Sets the flex grow value.
        /// </summary>
        /// <param name="value">The flex grow value.</param>
        /// <returns>This component for chaining.</returns>
        public JComponent WithFlexGrow(float value)
        {
            style.flexGrow = value;
            return this;
        }

        /// <summary>
        /// Sets the flex shrink value.
        /// </summary>
        /// <param name="value">The flex shrink value.</param>
        /// <returns>This component for chaining.</returns>
        public JComponent WithFlexShrink(float value)
        {
            style.flexShrink = value;
            return this;
        }

        /// <summary>
        /// Sets margin on all sides.
        /// </summary>
        /// <param name="value">The margin value.</param>
        /// <returns>This component for chaining.</returns>
        public JComponent WithMargin(float value)
        {
            style.marginTop = value;
            style.marginRight = value;
            style.marginBottom = value;
            style.marginLeft = value;
            return this;
        }

        /// <summary>
        /// Sets padding on all sides.
        /// </summary>
        /// <param name="value">The padding value.</param>
        /// <returns>This component for chaining.</returns>
        public JComponent WithPadding(float value)
        {
            style.paddingTop = value;
            style.paddingRight = value;
            style.paddingBottom = value;
            style.paddingLeft = value;
            return this;
        }

        /// <summary>
        /// Sets the visibility of this component.
        /// </summary>
        /// <param name="visible">Whether the component should be visible.</param>
        /// <returns>This component for chaining.</returns>
        public JComponent WithVisibility(bool visible)
        {
            style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            return this;
        }
    }
}
