// JStack.cs
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
    /// Vertical stack layout component with configurable gap.
    /// </summary>
    public class JStack : JComponent
    {
        /// <summary>
        /// Creates a new vertical stack with default medium gap.
        /// </summary>
        public JStack() : this(GapSize.MD)
        {
        }

        /// <summary>
        /// Creates a new vertical stack with the specified gap.
        /// </summary>
        /// <param name="gap">The gap size between children.</param>
        public JStack(GapSize gap) : base("j-stack")
        {
            style.flexDirection = FlexDirection.Column;
            SetGap(gap);
        }

        /// <summary>
        /// Sets the gap between children.
        /// </summary>
        /// <param name="gap">The gap size.</param>
        /// <returns>This stack for chaining.</returns>
        public JStack WithGap(GapSize gap)
        {
            SetGap(gap);
            return this;
        }

        private void SetGap(GapSize gap)
        {
            // Remove existing gap classes
            RemoveFromClassList("j-stack--gap-xs");
            RemoveFromClassList("j-stack--gap-sm");
            RemoveFromClassList("j-stack--gap-md");
            RemoveFromClassList("j-stack--gap-lg");
            RemoveFromClassList("j-stack--gap-xl");

            // Add new gap class
            var gapClass = gap switch
            {
                GapSize.Xs => "j-stack--gap-xs",
                GapSize.Sm => "j-stack--gap-sm",
                GapSize.MD => "j-stack--gap-md",
                GapSize.Lg => "j-stack--gap-lg",
                GapSize.Xl => "j-stack--gap-xl",
                _ => "j-stack--gap-md"
            };
            AddToClassList(gapClass);

            // Also set style directly for immediate effect
            var gapValue = gap switch
            {
                GapSize.Xs => Tokens.Spacing.Xs,
                GapSize.Sm => Tokens.Spacing.Sm,
                GapSize.MD => Tokens.Spacing.MD,
                GapSize.Lg => Tokens.Spacing.Lg,
                GapSize.Xl => Tokens.Spacing.Xl,
                _ => Tokens.Spacing.MD
            };

            // Apply gap using margin on children when added
            RegisterCallback<AttachToPanelEvent, (JStack stack, float gap)>(static (_, state) =>
            {
                state.stack.ApplyChildGaps(state.gap);
            }, (this, gapValue));
        }

        private void ApplyChildGaps(float gapValue)
        {
            for (int i = 0; i < childCount; i++)
            {
                var child = this[i];
                child.style.marginBottom = i < childCount - 1 ? gapValue : 0;
            }
        }

        /// <summary>
        /// Adds child elements with proper gap.
        /// </summary>
        public new JStack Add(params VisualElement[] children)
        {
            base.Add(children);
            return this;
        }
    }
}
