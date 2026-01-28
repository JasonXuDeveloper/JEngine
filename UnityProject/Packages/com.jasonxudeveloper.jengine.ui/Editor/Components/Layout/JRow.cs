// JRow.cs
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

namespace JEngine.UI.Editor.Components.Layout
{
    /// <summary>
    /// Horizontal flex row component with justify and align options.
    /// </summary>
    public class JRow : JComponent
    {
        /// <summary>
        /// Creates a new horizontal row with default settings.
        /// </summary>
        public JRow() : base("j-row")
        {
            style.flexDirection = FlexDirection.Row;
            style.flexWrap = Wrap.Wrap;
            style.alignItems = Align.Center;
        }

        /// <summary>
        /// Sets the horizontal alignment (justify-content).
        /// </summary>
        /// <param name="justify">The justify option.</param>
        /// <returns>This row for chaining.</returns>
        public JRow WithJustify(JustifyContent justify)
        {
            // Remove existing justify classes
            RemoveFromClassList("j-row--justify-start");
            RemoveFromClassList("j-row--justify-center");
            RemoveFromClassList("j-row--justify-end");
            RemoveFromClassList("j-row--justify-between");

            style.justifyContent = justify switch
            {
                JustifyContent.Start => Justify.FlexStart,
                JustifyContent.Center => Justify.Center,
                JustifyContent.End => Justify.FlexEnd,
                JustifyContent.SpaceBetween => Justify.SpaceBetween,
                _ => Justify.FlexStart
            };

            return this;
        }

        /// <summary>
        /// Sets the vertical alignment (align-items).
        /// </summary>
        /// <param name="align">The alignment option.</param>
        /// <returns>This row for chaining.</returns>
        public JRow WithAlign(AlignItems align)
        {
            // Remove existing align classes
            RemoveFromClassList("j-row--align-start");
            RemoveFromClassList("j-row--align-center");
            RemoveFromClassList("j-row--align-end");
            RemoveFromClassList("j-row--align-stretch");

            style.alignItems = align switch
            {
                AlignItems.Start => Align.FlexStart,
                AlignItems.Center => Align.Center,
                AlignItems.End => Align.FlexEnd,
                AlignItems.Stretch => Align.Stretch,
                _ => Align.Center
            };

            return this;
        }

        /// <summary>
        /// Disables flex-wrap (items won't wrap to next line).
        /// </summary>
        /// <returns>This row for chaining.</returns>
        public JRow NoWrap()
        {
            AddToClassList("j-row--nowrap");
            style.flexWrap = Wrap.NoWrap;
            return this;
        }

        /// <summary>
        /// Adds child elements to this row.
        /// </summary>
        public new JRow Add(params VisualElement[] children)
        {
            base.Add(children);
            return this;
        }
    }

    /// <summary>
    /// Horizontal justify-content options.
    /// </summary>
    public enum JustifyContent
    {
        Start,
        Center,
        End,
        SpaceBetween
    }

    /// <summary>
    /// Vertical align-items options.
    /// </summary>
    public enum AlignItems
    {
        Start,
        Center,
        End,
        Stretch
    }
}
