// JButtonGroup.cs
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

namespace JEngine.UI.Editor.Components.Button
{
    /// <summary>
    /// A horizontal row of buttons with responsive wrapping.
    /// </summary>
    public class JButtonGroup : JComponent
    {
        /// <summary>
        /// Creates a new button group.
        /// </summary>
        /// <param name="buttons">The buttons to include in the group.</param>
        public JButtonGroup(params VisualElement[] buttons) : base("j-button-group")
        {
            style.flexDirection = FlexDirection.Row;
            style.flexWrap = Wrap.Wrap;
            style.alignItems = Align.Center;

            foreach (var button in buttons)
            {
                if (button != null)
                {
                    // Apply group styling to buttons - compact
                    button.style.marginRight = Tokens.Spacing.Sm;
                    button.style.marginBottom = Tokens.Spacing.Xs;
                    button.style.flexGrow = 1;
                    button.style.flexShrink = 0;
                    button.style.minWidth = 100;
                    base.Add(button);
                }
            }

            // Remove right margin from last child
            if (childCount > 0)
            {
                this[childCount - 1].style.marginRight = 0;
            }
        }

        /// <summary>
        /// Adds buttons to this group.
        /// </summary>
        public new JButtonGroup Add(params VisualElement[] buttons)
        {
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    button.style.marginRight = Tokens.Spacing.Sm;
                    button.style.marginBottom = Tokens.Spacing.Xs;
                    button.style.flexGrow = 1;
                    button.style.flexShrink = 0;
                    button.style.minWidth = 100;
                    base.Add(button);
                }
            }

            // Update margins
            for (int i = 0; i < childCount; i++)
            {
                this[i].style.marginRight = i < childCount - 1 ? Tokens.Spacing.Sm : 0;
            }

            return this;
        }

        /// <summary>
        /// Disables flex-wrap (buttons won't wrap to next line).
        /// </summary>
        /// <returns>This button group for chaining.</returns>
        public JButtonGroup NoWrap()
        {
            style.flexWrap = Wrap.NoWrap;
            return this;
        }

        /// <summary>
        /// Sets buttons to fixed width (no flex grow).
        /// </summary>
        /// <returns>This button group for chaining.</returns>
        public JButtonGroup FixedWidth()
        {
            for (int i = 0; i < childCount; i++)
            {
                this[i].style.flexGrow = 0;
                this[i].style.flexBasis = StyleKeyword.Auto;
            }
            return this;
        }
    }
}
