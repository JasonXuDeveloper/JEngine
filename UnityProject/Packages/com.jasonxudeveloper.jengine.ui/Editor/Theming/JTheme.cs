// JTheme.cs
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
using UnityEngine;
using UnityEngine.UIElements;

namespace JEngine.UI.Editor.Theming
{
    /// <summary>
    /// Theme configuration and application utilities for JEngine Editor UI.
    /// </summary>
    public static class JTheme
    {
        /// <summary>
        /// Applies base theme styles to a visual element.
        /// </summary>
        /// <param name="element">The element to style.</param>
        public static void ApplyBase(VisualElement element)
        {
            element.style.backgroundColor = Tokens.Colors.BgBase;
            element.style.color = Tokens.Colors.TextPrimary;
        }

        /// <summary>
        /// Applies elevated surface styles (for cards, sections).
        /// </summary>
        /// <param name="element">The element to style.</param>
        public static void ApplyElevated(VisualElement element)
        {
            element.style.backgroundColor = Tokens.Colors.BgElevated;
            element.style.borderTopColor = Tokens.Colors.BorderSubtle;
            element.style.borderRightColor = Tokens.Colors.BorderSubtle;
            element.style.borderBottomColor = Tokens.Colors.BorderSubtle;
            element.style.borderLeftColor = Tokens.Colors.BorderSubtle;
            element.style.borderTopWidth = 1f;
            element.style.borderRightWidth = 1f;
            element.style.borderBottomWidth = 1f;
            element.style.borderLeftWidth = 1f;
            element.style.borderTopLeftRadius = Tokens.BorderRadius.Sm;
            element.style.borderTopRightRadius = Tokens.BorderRadius.Sm;
            element.style.borderBottomLeftRadius = Tokens.BorderRadius.Sm;
            element.style.borderBottomRightRadius = Tokens.BorderRadius.Sm;
        }

        /// <summary>
        /// Applies header text styles.
        /// </summary>
        /// <param name="label">The label to style.</param>
        public static void ApplyHeaderStyle(Label label)
        {
            label.style.color = Tokens.Colors.TextHeader;
            label.style.fontSize = Tokens.FontSize.Lg;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = Tokens.Spacing.MD;
        }

        /// <summary>
        /// Applies muted text styles.
        /// </summary>
        /// <param name="label">The label to style.</param>
        public static void ApplyMutedStyle(Label label)
        {
            label.style.color = Tokens.Colors.TextMuted;
            label.style.fontSize = Tokens.FontSize.Sm;
        }

        /// <summary>
        /// Applies surface glass layer style (60% opacity).
        /// </summary>
        /// <param name="element">The element to style.</param>
        public static void ApplySurface(VisualElement element)
        {
            element.style.backgroundColor = Tokens.Colors.BgSurface;
        }

        /// <summary>
        /// Applies subtle glass layer style (50% opacity).
        /// </summary>
        /// <param name="element">The element to style.</param>
        public static void ApplySubtle(VisualElement element)
        {
            element.style.backgroundColor = Tokens.Colors.BgSubtle;
        }

        /// <summary>
        /// Applies borders with theme-appropriate colors.
        /// </summary>
        /// <param name="element">The element to apply borders to.</param>
        /// <param name="subtle">Use subtle borders if true.</param>
        public static void ApplyBorder(VisualElement element, bool subtle = false)
        {
            if (subtle)
            {
                element.style.borderLeftColor = Tokens.Colors.BorderSubtle;
                element.style.borderRightColor = Tokens.Colors.BorderSubtle;
                element.style.borderTopColor = Tokens.Colors.BorderSubtle;
                element.style.borderBottomColor = Tokens.Colors.BorderSubtle;
            }
            else
            {
                // Uniform borders using BorderLight/BorderDark (same in monochrome)
                element.style.borderTopColor = Tokens.Colors.BorderLight;
                element.style.borderLeftColor = Tokens.Colors.BorderLight;
                element.style.borderBottomColor = Tokens.Colors.BorderDark;
                element.style.borderRightColor = Tokens.Colors.BorderDark;
            }

            element.style.borderLeftWidth = 1f;
            element.style.borderRightWidth = 1f;
            element.style.borderTopWidth = 1f;
            element.style.borderBottomWidth = 1f;
        }

        /// <summary>
        /// Applies smooth transitions.
        /// </summary>
        /// <param name="element">The element to apply transitions to.</param>
        public static void ApplyTransition(VisualElement element)
        {
            element.style.transitionDuration = new List<TimeValue>
            {
                new(Tokens.Transition.Normal, TimeUnit.Millisecond)
            };
            element.style.transitionProperty = new List<StylePropertyName>
            {
                new("background-color"),
                new("border-color"),
                new("border-top-color"),
                new("border-left-color"),
                new("border-bottom-color"),
                new("border-right-color"),
                new("color"),
                new("opacity")
            };
        }

        /// <summary>
        /// Applies card style (surface + borders + transitions).
        /// </summary>
        /// <param name="element">The element to style as a card.</param>
        public static void ApplyGlassCard(VisualElement element)
        {
            ApplySurface(element);
            ApplyBorder(element);
            ApplyTransition(element);

            element.style.borderTopLeftRadius = Tokens.BorderRadius.MD;
            element.style.borderTopRightRadius = Tokens.BorderRadius.MD;
            element.style.borderBottomLeftRadius = Tokens.BorderRadius.MD;
            element.style.borderBottomRightRadius = Tokens.BorderRadius.MD;
        }

        /// <summary>
        /// Gets the appropriate button background color for a variant.
        /// </summary>
        public static Color GetButtonColor(ButtonVariant variant)
        {
            return variant switch
            {
                ButtonVariant.Primary => Tokens.Colors.Primary,
                ButtonVariant.Secondary => Tokens.Colors.Secondary,
                ButtonVariant.Success => Tokens.Colors.Success,
                ButtonVariant.Danger => Tokens.Colors.Danger,
                ButtonVariant.Warning => Tokens.Colors.Warning,
                _ => Tokens.Colors.Primary
            };
        }

        /// <summary>
        /// Gets the hover color for a button variant.
        /// </summary>
        public static Color GetButtonHoverColor(ButtonVariant variant)
        {
            return variant switch
            {
                ButtonVariant.Primary => Tokens.Colors.PrimaryHover,
                ButtonVariant.Secondary => Tokens.Colors.SecondaryHover,
                ButtonVariant.Success => Tokens.Colors.SuccessHover,
                ButtonVariant.Danger => Tokens.Colors.DangerHover,
                ButtonVariant.Warning => Tokens.Colors.WarningHover,
                _ => Tokens.Colors.PrimaryHover
            };
        }

        /// <summary>
        /// Gets the active/pressed color for a button variant.
        /// </summary>
        public static Color GetButtonActiveColor(ButtonVariant variant)
        {
            return variant switch
            {
                ButtonVariant.Primary => Tokens.Colors.PrimaryActive,
                ButtonVariant.Secondary => Tokens.Colors.SecondaryActive,
                ButtonVariant.Success => Tokens.Colors.SuccessActive,
                ButtonVariant.Danger => Tokens.Colors.DangerActive,
                ButtonVariant.Warning => Tokens.Colors.WarningActive,
                _ => Tokens.Colors.PrimaryActive
            };
        }
    }

    /// <summary>
    /// Button visual variants.
    /// </summary>
    public enum ButtonVariant
    {
        Primary,
        Secondary,
        Success,
        Danger,
        Warning
    }

    /// <summary>
    /// Gap sizes for layout components.
    /// </summary>
    public enum GapSize
    {
        Xs,
        Sm,
        MD,
        Lg,
        Xl
    }

    /// <summary>
    /// Status types for feedback components.
    /// </summary>
    public enum StatusType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
