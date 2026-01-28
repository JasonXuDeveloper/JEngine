// Tokens.cs
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

using UnityEditor;
using UnityEngine;

namespace JEngine.UI.Editor.Theming
{
    /// <summary>
    /// Design tokens for JEngine Editor UI framework.
    /// Provides consistent colors, spacing, and typography values.
    /// Automatically adapts to Unity Editor's dark/light theme.
    /// Dark theme is a clean grayscale inversion of the light theme.
    /// </summary>
    public static class Tokens
    {
        /// <summary>
        /// Gets whether the editor is using the professional (dark) skin.
        /// </summary>
        public static bool IsDarkTheme => EditorGUIUtility.isProSkin;

        /// <summary>
        /// Monochrome color palette that adapts to Unity Editor theme.
        /// Dark theme is a clean grayscale inversion of the light theme.
        /// </summary>
        public static class Colors
        {
            // ===== BACKGROUND LAYERS =====
            // Dark theme: Pure neutral greys (inverted from light)
            // Light theme: Pure white to light greys

            /// <summary>Base layer - Deepest background</summary>
            public static Color BgBase => IsDarkTheme
                ? FromHex("#0A0A0A")        // Near-black (neutral grey)
                : FromHex("#FFFFFF");        // Pure white

            /// <summary>Subtle layer - Secondary containers</summary>
            public static Color BgSubtle => IsDarkTheme
                ? FromHex("#1A1A1A")        // Very dark grey (neutral)
                : FromHex("#F9FAFB");        // Very light grey (gray-50)

            /// <summary>Surface layer - Cards, panels, dropdowns (most common)</summary>
            public static Color BgSurface => IsDarkTheme
                ? FromHex("#2A2A2A")        // Dark grey (neutral) - inverted from gray-200
                : FromHex("#E5E7EB");        // Medium light grey (gray-200)

            /// <summary>Elevated layer - Hover states, important elements</summary>
            public static Color BgElevated => IsDarkTheme
                ? FromHex("#3A3A3A")        // Medium-dark grey (neutral)
                : FromHex("#E5E7EB");        // Medium light grey (gray-200)

            /// <summary>Overlay layer - Modals, dropdowns, tooltips</summary>
            public static Color BgOverlay => IsDarkTheme
                ? FromHex("#4A4A4A")        // Medium grey (neutral) - inverted from gray-300
                : FromHex("#D1D5DB");        // Medium grey (gray-300)

            /// <summary>Hover state</summary>
            public static Color BgHover => IsDarkTheme
                ? FromHex("#3A3A3A")        // Medium-dark grey (neutral)
                : FromHex("#E5E7EB");        // gray-200

            // ===== SEMANTIC COLORS =====
            // All use neutral grayscale values, no vibrant colors

            /// <summary>Primary button color</summary>
            public static Color Primary => IsDarkTheme
                ? FromHex("#C8C8C8")        // Light grey (neutral) - inverted from light's #373737
                : FromHex("#374151");        // Medium-dark grey (original light theme color)

            public static Color PrimaryHover => IsDarkTheme
                ? FromHex("#E0E0E0")        // Lighter grey (neutral)
                : FromHex("#4B5563");        // Slightly lighter (original)

            public static Color PrimaryActive => IsDarkTheme
                ? FromHex("#F0F0F0")        // Very light grey (neutral)
                : FromHex("#1F2937");        // Very dark grey (original)

            /// <summary>Secondary button color</summary>
            public static Color Secondary => IsDarkTheme
                ? FromHex("#4A4A4A")        // Medium-dark grey (neutral) - inverted from light's #D0D0D0
                : FromHex("#D0D0D0");        // Medium grey (neutral)

            public static Color SecondaryHover => IsDarkTheme
                ? FromHex("#6A6A6A")        // Medium grey (neutral)
                : FromHex("#9A9A9A");        // Darker on hover

            public static Color SecondaryActive => IsDarkTheme
                ? FromHex("#9A9A9A")        // Light-medium grey (neutral)
                : FromHex("#6A6A6A");        // Much darker

            /// <summary>Success state - neutral grey</summary>
            public static Color Success => IsDarkTheme
                ? FromHex("#F9F9F9")        // Very light grey (neutral) - inverted from #111111
                : FromHex("#111111");        // Near-black (neutral)

            public static Color SuccessHover => IsDarkTheme
                ? FromHex("#E5E5E5")        // Light grey - inverted
                : FromHex("#1F1F1F");        // Dark grey

            public static Color SuccessActive => IsDarkTheme
                ? FromHex("#D0D0D0")        // Medium-light grey - inverted
                : FromHex("#030303");        // Almost black

            /// <summary>Danger state - neutral grey</summary>
            public static Color Danger => IsDarkTheme
                ? FromHex("#F9F9F9")        // Very light grey (neutral) - inverted
                : FromHex("#111111");        // Near-black (neutral)

            public static Color DangerHover => IsDarkTheme
                ? FromHex("#E5E5E5")        // Light grey - inverted
                : FromHex("#1F1F1F");        // Dark grey

            public static Color DangerActive => IsDarkTheme
                ? FromHex("#D0D0D0")        // Medium-light grey - inverted
                : FromHex("#030303");        // Almost black

            /// <summary>Warning state - neutral grey</summary>
            public static Color Warning => IsDarkTheme
                ? FromHex("#F9F9F9")        // Very light grey (neutral) - inverted
                : FromHex("#111111");        // Near-black (neutral)

            public static Color WarningHover => IsDarkTheme
                ? FromHex("#E5E5E5")        // Light grey - inverted
                : FromHex("#1F1F1F");        // Dark grey

            public static Color WarningActive => IsDarkTheme
                ? FromHex("#D0D0D0")        // Medium-light grey - inverted
                : FromHex("#030303");        // Almost black

            // ===== TEXT HIERARCHY =====
            // Dark theme: White to light greys (inverted, neutral)
            // Light theme: Black to dark greys

            /// <summary>Primary text - Highest contrast</summary>
            public static Color TextPrimary => IsDarkTheme
                ? FromHex("#FFFFFF")        // Pure white - inverted from light's #111111
                : FromHex("#111111");        // Near-black (neutral)

            /// <summary>Secondary text - Body text</summary>
            public static Color TextSecondary => IsDarkTheme
                ? FromHex("#D0D0D0")        // Light grey (neutral) - inverted from light's #373737
                : FromHex("#373737");        // Dark grey (neutral)

            /// <summary>Muted text - Helper text</summary>
            public static Color TextMuted => IsDarkTheme
                ? FromHex("#9A9A9A")        // Medium-light grey (neutral) - inverted from light's #6A6A6A
                : FromHex("#6A6A6A");        // Medium grey (neutral)

            /// <summary>Main panel/page headers - Maximum impact</summary>
            public static Color TextHeader => IsDarkTheme
                ? FromHex("#FFFFFF")        // Pure white
                : FromHex("#111111");        // Near-black (neutral)

            /// <summary>Section headers - Visual hierarchy</summary>
            public static Color TextSectionHeader => IsDarkTheme
                ? FromHex("#E5E5E5")        // Light grey (neutral) - inverted, no color
                : FromHex("#111111");        // Near-black (neutral)

            // ===== BORDERS =====
            // Dark theme: Medium-dark greys (inverted, neutral)
            // Light theme: Medium greys

            /// <summary>Light edge border</summary>
            public static Color BorderLight => IsDarkTheme
                ? FromHex("#4A4A4A")        // Medium-dark grey (neutral) - inverted from light's #D0D0D0
                : FromHex("#D0D0D0");        // Medium grey (neutral)

            /// <summary>Dark edge border</summary>
            public static Color BorderDark => IsDarkTheme
                ? FromHex("#4A4A4A")        // Medium-dark grey (neutral) - inverted
                : FromHex("#D0D0D0");        // Medium grey (neutral)

            /// <summary>Default border</summary>
            public static Color Border => IsDarkTheme
                ? FromHex("#4A4A4A")        // Medium-dark grey (neutral) - inverted
                : FromHex("#D0D0D0");        // Medium grey (neutral)

            /// <summary>Focus border</summary>
            public static Color BorderFocus => IsDarkTheme
                ? FromHex("#6A6A6A")        // Medium grey (neutral) - inverted from light's #9A9A9A
                : FromHex("#9A9A9A");        // Medium-light grey (neutral)

            /// <summary>Hover border</summary>
            public static Color BorderHover => IsDarkTheme
                ? FromHex("#6A6A6A")        // Medium grey (neutral) - inverted
                : FromHex("#D0D0D0");        // Medium grey (neutral)

            /// <summary>Subtle border for separators</summary>
            public static Color BorderSubtle => IsDarkTheme
                ? FromHex("#2A2A2A")        // Dark grey (neutral) - inverted
                : FromHex("#D0D0D0");        // Medium grey (neutral)

            // ===== STATUS COLORS =====
            // All use the same neutral grey values
            public static Color StatusInfo => Primary;
            public static Color StatusSuccess => Success;
            public static Color StatusWarning => Warning;
            public static Color StatusError => Danger;

            private static Color FromHex(string hex)
            {
                if (ColorUtility.TryParseHtmlString(hex, out var color))
                    return color;
                return Color.magenta;
            }
        }

        /// <summary>
        /// Spacing values based on 8px grid.
        /// </summary>
        public static class Spacing
        {
            public const float Xs = 2f;
            public const float Sm = 4f;
            public const float MD = 8f;
            public const float Lg = 12f;
            public const float Xl = 16f;
            public const float Xxl = 24f;
        }

        /// <summary>
        /// Font sizes for different text types.
        /// </summary>
        public static class FontSize
        {
            public const float Xs = 10f;
            public const float Sm = 11f;
            public const float Base = 12f;
            public const float MD = 13f;
            public const float Lg = 14f;
            public const float Xl = 16f;
            public const float Title = 18f;
        }

        /// <summary>
        /// Border radius values.
        /// </summary>
        public static class BorderRadius
        {
            public const float Sm = 3f;
            public const float MD = 5f;
            public const float Lg = 8f;
        }

        /// <summary>
        /// Animation/transition durations in milliseconds.
        /// </summary>
        public static class Transition
        {
            public const int Fast = 150;
            public const int Normal = 200;
        }

        /// <summary>
        /// Layout constants.
        /// </summary>
        public static class Layout
        {
            public const float FormLabelWidth = 140f;
            public const float FormLabelMinWidth = 60f;
            public const float MinTouchTarget = 24f;
            public const float MinControlWidth = 80f;
        }
    }
}
