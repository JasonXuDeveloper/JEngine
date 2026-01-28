// JBreadcrumb.cs
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

using System.Collections.Generic;
using JEngine.UI.Editor.Theming;
using UnityEngine.UIElements;

namespace JEngine.UI.Editor.Components.Navigation
{
    /// <summary>
    /// Modern breadcrumb navigation with pill-style hover effects.
    /// Example: "Package › Scene" or "Core › Scripts › Bootstrap.cs"
    /// </summary>
    public class JBreadcrumb : VisualElement
    {
        private readonly List<BreadcrumbItem> _items = new();
        private readonly VisualElement _container;

        /// <summary>
        /// Represents a single breadcrumb item.
        /// </summary>
        public class BreadcrumbItem
        {
            /// <summary>
            /// Display text for this breadcrumb segment.
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// Whether this is the last item (affects styling).
            /// </summary>
            public bool IsLast { get; set; }

            public BreadcrumbItem(string label)
            {
                Label = label;
            }
        }

        /// <summary>
        /// Creates a new breadcrumb navigation component.
        /// </summary>
        public JBreadcrumb()
        {
            AddToClassList("j-breadcrumb");

            // Container for breadcrumb items
            _container = new VisualElement();
            _container.style.flexDirection = FlexDirection.Row;
            _container.style.alignItems = Align.Center;
            _container.style.flexGrow = 1;
            _container.style.flexShrink = 1;
            _container.style.overflow = Overflow.Hidden;

            // Set parent to grow/shrink
            style.flexGrow = 1;
            style.flexShrink = 1;

            Add(_container);
        }

        /// <summary>
        /// Adds a breadcrumb item to the path.
        /// </summary>
        /// <param name="label">Display text for this segment</param>
        /// <returns>This breadcrumb instance for chaining</returns>
        public JBreadcrumb AddItem(string label)
        {
            var item = new BreadcrumbItem(label);
            _items.Add(item);
            return this;
        }

        /// <summary>
        /// Adds multiple breadcrumb items from a path array.
        /// </summary>
        /// <param name="path">Array of labels (e.g., ["Package", "Scene"])</param>
        /// <returns>This breadcrumb instance for chaining</returns>
        public JBreadcrumb SetPath(params string[] path)
        {
            Clear();
            foreach (var label in path)
            {
                AddItem(label);
            }
            Build();
            return this;
        }

        /// <summary>
        /// Clears all breadcrumb items.
        /// </summary>
        public new void Clear()
        {
            _items.Clear();
            _container.Clear();
        }

        /// <summary>
        /// Builds the breadcrumb UI from the added items.
        /// Call this after adding all items.
        /// </summary>
        public void Build()
        {
            _container.Clear();

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                item.IsLast = (i == _items.Count - 1);

                // Create label
                var label = new Label(item.Label);
                label.style.fontSize = Tokens.FontSize.Sm;
                label.style.overflow = Overflow.Hidden;
                label.style.textOverflow = TextOverflow.Ellipsis;
                label.style.whiteSpace = WhiteSpace.NoWrap;
                label.style.flexShrink = item.IsLast ? 1 : 0; // Only last item can shrink

                if (item.IsLast)
                {
                    // Last item - pill style with background
                    label.style.paddingTop = Tokens.Spacing.Xs;
                    label.style.paddingBottom = Tokens.Spacing.Xs;
                    label.style.paddingLeft = Tokens.Spacing.MD;
                    label.style.paddingRight = Tokens.Spacing.MD;
                    label.style.backgroundColor = Tokens.Colors.BgElevated;
                    label.style.color = Tokens.Colors.TextPrimary;
                    label.style.borderTopLeftRadius = Tokens.BorderRadius.Sm;
                    label.style.borderTopRightRadius = Tokens.BorderRadius.Sm;
                    label.style.borderBottomLeftRadius = Tokens.BorderRadius.Sm;
                    label.style.borderBottomRightRadius = Tokens.BorderRadius.Sm;
                    label.style.borderTopWidth = 1;
                    label.style.borderBottomWidth = 1;
                    label.style.borderLeftWidth = 1;
                    label.style.borderRightWidth = 1;
                    label.style.borderTopColor = Tokens.Colors.Border;
                    label.style.borderBottomColor = Tokens.Colors.Border;
                    label.style.borderLeftColor = Tokens.Colors.Border;
                    label.style.borderRightColor = Tokens.Colors.Border;

                    JTheme.ApplyTransition(label);

                    // Hover effect for last item
                    label.RegisterCallback<MouseEnterEvent, Label>(OnLastItemMouseEnter, label);
                    label.RegisterCallback<MouseLeaveEvent, Label>(OnLastItemMouseLeave, label);
                }
                else
                {
                    // Non-last items - plain text, no background
                    label.style.color = Tokens.Colors.TextMuted;
                    label.style.paddingRight = Tokens.Spacing.Xs;
                }

                _container.Add(label);

                // Add separator (chevron) if not last item
                if (!item.IsLast)
                {
                    var separator = new Label("›");
                    separator.style.color = Tokens.Colors.TextMuted;
                    separator.style.fontSize = Tokens.FontSize.Sm;
                    separator.style.marginLeft = Tokens.Spacing.Xs;
                    separator.style.marginRight = Tokens.Spacing.Xs;
                    separator.style.flexShrink = 0; // Never shrink separator
                    _container.Add(separator);
                }
            }
        }

        /// <summary>
        /// Creates a simple breadcrumb from a path array (convenience method).
        /// </summary>
        /// <param name="path">Array of labels (e.g., ["Package", "Scene"])</param>
        /// <returns>Configured breadcrumb instance</returns>
        public static JBreadcrumb FromPath(params string[] path)
        {
            var breadcrumb = new JBreadcrumb();
            return breadcrumb.SetPath(path);
        }

        // Static event handlers to avoid closure allocation
        private static void OnLastItemMouseEnter(MouseEnterEvent evt, Label label)
        {
            label.style.backgroundColor = Tokens.Colors.BgHover;
            label.style.borderTopColor = Tokens.Colors.BorderHover;
            label.style.borderBottomColor = Tokens.Colors.BorderHover;
            label.style.borderLeftColor = Tokens.Colors.BorderHover;
            label.style.borderRightColor = Tokens.Colors.BorderHover;
        }

        private static void OnLastItemMouseLeave(MouseLeaveEvent evt, Label label)
        {
            label.style.backgroundColor = Tokens.Colors.BgElevated;
            label.style.borderTopColor = Tokens.Colors.Border;
            label.style.borderBottomColor = Tokens.Colors.Border;
            label.style.borderLeftColor = Tokens.Colors.Border;
            label.style.borderRightColor = Tokens.Colors.Border;
        }
    }
}
