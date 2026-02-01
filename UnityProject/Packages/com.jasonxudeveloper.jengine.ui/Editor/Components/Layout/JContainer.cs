// JContainer.cs
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
    /// Responsive container component with max-width constraints.
    /// Centers content horizontally with configurable maximum width.
    /// Based on Tailwind CSS container patterns.
    /// </summary>
    public class JContainer : JComponent
    {
        private ContainerSize _size;

        /// <summary>
        /// Creates a new container with the default large (1024px) size.
        /// </summary>
        public JContainer() : this(ContainerSize.Lg)
        {
        }

        /// <summary>
        /// Creates a new container with the specified size.
        /// </summary>
        /// <param name="size">The container size (max-width constraint).</param>
        public JContainer(ContainerSize size) : base("j-container")
        {
            // Set width to 100% and center with auto margins
            style.width = Length.Percent(100);
            style.marginLeft = StyleKeyword.Auto;
            style.marginRight = StyleKeyword.Auto;

            SetSize(size);
        }

        /// <summary>
        /// Gets or sets the container size.
        /// </summary>
        public ContainerSize Size
        {
            get => _size;
            set => SetSize(value);
        }

        /// <summary>
        /// Sets the container size.
        /// </summary>
        /// <param name="size">The container size.</param>
        /// <returns>This container for chaining.</returns>
        public JContainer WithSize(ContainerSize size)
        {
            SetSize(size);
            return this;
        }

        /// <summary>
        /// Sets horizontal padding on the container.
        /// </summary>
        /// <param name="padding">The padding value in pixels.</param>
        /// <returns>This container for chaining.</returns>
        public JContainer WithHorizontalPadding(float padding)
        {
            style.paddingLeft = padding;
            style.paddingRight = padding;
            return this;
        }

        /// <summary>
        /// Applies responsive horizontal padding based on container size.
        /// Larger containers get more padding.
        /// </summary>
        /// <returns>This container for chaining.</returns>
        public JContainer WithResponsivePadding()
        {
            var padding = _size switch
            {
                ContainerSize.Xs => Tokens.Spacing.MD,
                ContainerSize.Sm => Tokens.Spacing.Lg,
                ContainerSize.Md => Tokens.Spacing.Xl,
                ContainerSize.Lg => Tokens.Spacing.Xxl,
                ContainerSize.Xl => Tokens.Spacing.Xxl,
                ContainerSize.Full => Tokens.Spacing.Lg,
                _ => Tokens.Spacing.Lg
            };

            style.paddingLeft = padding;
            style.paddingRight = padding;
            return this;
        }

        /// <summary>
        /// Sets the container to fluid mode (no max-width constraint).
        /// </summary>
        /// <returns>This container for chaining.</returns>
        public JContainer Fluid()
        {
            SetSize(ContainerSize.Full);
            return this;
        }

        /// <summary>
        /// Adds child elements to this container.
        /// </summary>
        /// <param name="children">The elements to add.</param>
        /// <returns>This container for chaining.</returns>
        public new JContainer Add(params VisualElement[] children)
        {
            base.Add(children);
            return this;
        }

        private void SetSize(ContainerSize size)
        {
            _size = size;

            // Remove existing size classes
            RemoveFromClassList("j-container--xs");
            RemoveFromClassList("j-container--sm");
            RemoveFromClassList("j-container--md");
            RemoveFromClassList("j-container--lg");
            RemoveFromClassList("j-container--xl");
            RemoveFromClassList("j-container--full");

            // Add new size class
            var sizeClass = size switch
            {
                ContainerSize.Xs => "j-container--xs",
                ContainerSize.Sm => "j-container--sm",
                ContainerSize.Md => "j-container--md",
                ContainerSize.Lg => "j-container--lg",
                ContainerSize.Xl => "j-container--xl",
                ContainerSize.Full => "j-container--full",
                _ => "j-container--lg"
            };
            AddToClassList(sizeClass);

            // Also set max-width style directly for immediate effect
            if (size == ContainerSize.Full)
            {
                style.maxWidth = StyleKeyword.None;
            }
            else
            {
                var maxWidth = size switch
                {
                    ContainerSize.Xs => Tokens.Container.Xs,
                    ContainerSize.Sm => Tokens.Container.Sm,
                    ContainerSize.Md => Tokens.Container.Md,
                    ContainerSize.Lg => Tokens.Container.Lg,
                    ContainerSize.Xl => Tokens.Container.Xl,
                    _ => Tokens.Container.Lg
                };
                style.maxWidth = maxWidth;
            }
        }
    }
}
