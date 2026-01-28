// JCard.cs
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
using UnityEngine.UIElements;

namespace JEngine.UI.Editor.Components.Layout
{
    /// <summary>
    /// A bordered container with padding and elevated background.
    /// </summary>
    public class JCard : JComponent
    {
        /// <summary>
        /// Creates a new card container with subtle borders.
        /// </summary>
        public JCard() : base("j-card")
        {
            // Apply card styling using JTheme helper
            JTheme.ApplyGlassCard(this);

            // Set padding
            style.paddingTop = Tokens.Spacing.Lg;
            style.paddingRight = Tokens.Spacing.Lg;
            style.paddingBottom = Tokens.Spacing.Lg;
            style.paddingLeft = Tokens.Spacing.Lg;

            // Set margin
            style.marginBottom = Tokens.Spacing.Lg;

            // Register hover effect for elevated glass
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        /// <summary>
        /// Adds child elements to this card.
        /// </summary>
        public new JCard Add(params VisualElement[] children)
        {
            base.Add(children);
            return this;
        }

        /// <summary>
        /// Removes bottom margin from this card.
        /// </summary>
        /// <returns>This card for chaining.</returns>
        public JCard NoMargin()
        {
            style.marginBottom = 0;
            return this;
        }

        /// <summary>
        /// Sets compact padding.
        /// </summary>
        /// <returns>This card for chaining.</returns>
        public JCard Compact()
        {
            style.paddingTop = Tokens.Spacing.MD;
            style.paddingRight = Tokens.Spacing.MD;
            style.paddingBottom = Tokens.Spacing.MD;
            style.paddingLeft = Tokens.Spacing.MD;
            return this;
        }

        // Static event handlers to avoid closure allocation
        private void OnMouseEnter(MouseEnterEvent evt)
        {
            style.backgroundColor = Tokens.Colors.BgElevated;
            style.borderTopColor = Tokens.Colors.BorderHover;
            style.borderLeftColor = Tokens.Colors.BorderHover;
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            style.backgroundColor = Tokens.Colors.BgSurface;
            style.borderTopColor = Tokens.Colors.BorderLight;
            style.borderLeftColor = Tokens.Colors.BorderLight;
        }
    }
}
