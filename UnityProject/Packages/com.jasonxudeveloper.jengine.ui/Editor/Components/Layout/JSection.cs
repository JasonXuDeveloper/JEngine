// JSection.cs
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

namespace JEngine.UI.Editor.Components.Layout
{
    /// <summary>
    /// A card container with a header label.
    /// </summary>
    public class JSection : JComponent
    {
        private readonly Label _header;
        private readonly VisualElement _content;

        /// <summary>
        /// Creates a new glassmorphic section with a glowing header.
        /// </summary>
        /// <param name="title">The section title.</param>
        public JSection(string title) : base("j-section")
        {
            // Apply glassmorphic card styling
            JTheme.ApplyGlassCard(this);

            // Set padding and margin - standard
            style.paddingTop = Tokens.Spacing.Lg;
            style.paddingRight = Tokens.Spacing.Lg;
            style.paddingBottom = Tokens.Spacing.Lg;
            style.paddingLeft = Tokens.Spacing.Lg;
            style.marginBottom = Tokens.Spacing.Lg;
            style.overflow = Overflow.Hidden;

            // Create header - warm amber accent for visual hierarchy
            _header = new Label(title);
            _header.AddToClassList("j-section__header");
            _header.style.color = Tokens.Colors.TextSectionHeader;  // Warm amber for distinction
            _header.style.fontSize = Tokens.FontSize.Xl;             // Larger (18px)
            _header.style.unityFontStyleAndWeight = FontStyle.Bold;
            _header.style.paddingBottom = Tokens.Spacing.MD;
            _header.style.marginBottom = Tokens.Spacing.Lg;
            _header.style.borderBottomColor = Tokens.Colors.BorderSubtle;
            _header.style.borderBottomWidth = 1;
            base.Add(_header);

            // Create content container
            _content = new VisualElement();
            _content.AddToClassList("j-section__content");
            _content.style.flexDirection = FlexDirection.Column;
            _content.style.overflow = Overflow.Hidden;
            base.Add(_content);
        }

        /// <summary>
        /// Gets the header label for customization.
        /// </summary>
        public Label Header => _header;

        /// <summary>
        /// Gets the content container.
        /// </summary>
        public VisualElement Content => _content;

        /// <summary>
        /// Adds child elements to the content area.
        /// </summary>
        public new JSection Add(params VisualElement[] children)
        {
            foreach (var child in children)
            {
                if (child != null)
                {
                    _content.Add(child);
                }
            }
            return this;
        }

        /// <summary>
        /// Sets the header text.
        /// </summary>
        /// <param name="title">The new title.</param>
        /// <returns>This section for chaining.</returns>
        public JSection WithTitle(string title)
        {
            _header.text = title;
            return this;
        }

        /// <summary>
        /// Hides the header.
        /// </summary>
        /// <returns>This section for chaining.</returns>
        public JSection NoHeader()
        {
            _header.style.display = DisplayStyle.None;
            return this;
        }

        /// <summary>
        /// Removes bottom margin from this section.
        /// </summary>
        /// <returns>This section for chaining.</returns>
        public JSection NoMargin()
        {
            style.marginBottom = 0;
            return this;
        }
    }
}
