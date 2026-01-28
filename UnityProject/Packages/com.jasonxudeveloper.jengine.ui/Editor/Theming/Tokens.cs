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
            // Dark theme: Near-black to dark greys (inverted from light)
            // Light theme: Pure white to light greys

            /// <summary>Base layer - Deepest background</summary>
            public static Color BgBase => IsDarkTheme
                ? FromHex("#0F1419")        // Near-black (darker than gray-900)
                : FromHex("#FFFFFF");        // Pure white

            /// <summary>Subtle layer - Secondary containers</summary>
            public static Color BgSubtle => IsDarkTheme
                ? FromHex("#1F2937")        // Dark grey (gray-800)
                : FromHex("#F9FAFB");        // Very light grey (gray-50)

            /// <summary>Surface layer - Cards, panels, dropdowns (most common)</summary>
            public static Color BgSurface => IsDarkTheme
                ? FromHex("#374151")        // Medium-dark grey (gray-700)
                : FromHex("#E5E7EB");        // Medium light grey (gray-200)

            /// <summary>Elevated layer - Hover states, important elements</summary>
            public static Color BgElevated => IsDarkTheme
                ? FromHex("#4B5563")        // Lighter dark grey (gray-600)
                : FromHex("#E5E7EB");        // Medium light grey (gray-200)

            /// <summary>Overlay layer - Modals, dropdowns, tooltips</summary>
            public static Color BgOverlay => IsDarkTheme
                ? FromHex("#6B7280")        // Medium grey (gray-500)
                : FromHex("#D1D5DB");        // Medium grey (gray-300)

            /// <summary>Hover state</summary>
            public static Color BgHover => IsDarkTheme
                ? FromHex("#4B5563")        // gray-600
                : FromHex("#E5E7EB");        // gray-200

            // ===== SEMANTIC COLORS =====
            // All use grayscale values, no vibrant colors

            /// <summary>Primary button color</summary>
            public static Color Primary => IsDarkTheme
                ? FromHex("#D1D5DB")        // Light grey (gray-300) - inverted from light's gray-700
                : FromHex("#374151");        // Medium-dark grey (gray-700)

            public static Color PrimaryHover => IsDarkTheme
                ? FromHex("#E5E7EB")        // Lighter grey (gray-200) - inverted from light's gray-600
                : FromHex("#4B5563");        // gray-600

            public static Color PrimaryActive => IsDarkTheme
                ? FromHex("#F3F4F6")        // Very light grey (gray-100) - inverted from light's gray-800
                : FromHex("#1F2937");        // gray-800

            /// <summary>Secondary button color</summary>
            public static Color Secondary => IsDarkTheme
                ? FromHex("#4B5563")        // Medium-dark grey (gray-600) - inverted from light's gray-300
                : FromHex("#D1D5DB");        // Medium grey (gray-300)

            public static Color SecondaryHover => IsDarkTheme
                ? FromHex("#6B7280")        // Medium grey (gray-500) - inverted from light's gray-400
                : FromHex("#9CA3AF");        // gray-400

            public static Color SecondaryActive => IsDarkTheme
                ? FromHex("#9CA3AF")        // Light-medium grey (gray-400) - inverted from light's gray-500
                : FromHex("#6B7280");        // gray-500

            /// <summary>Success state - neutral grey</summary>
            public static Color Success => IsDarkTheme
                ? FromHex("#F9FAFB")        // Very light grey (gray-50) - inverted from light's gray-900
                : FromHex("#111827");        // Near-black (gray-900)

            public static Color SuccessHover => IsDarkTheme
                ? FromHex("#E5E7EB")        // gray-200 - inverted from light's gray-800
                : FromHex("#1F2937");        // gray-800

            public static Color SuccessActive => IsDarkTheme
                ? FromHex("#D1D5DB")        // gray-300 - inverted from light's gray-950
                : FromHex("#030712");        // gray-950

            /// <summary>Danger state - neutral grey</summary>
            public static Color Danger => IsDarkTheme
                ? FromHex("#F9FAFB")        // Very light grey (gray-50) - inverted
                : FromHex("#111827");        // Near-black (gray-900)

            public static Color DangerHover => IsDarkTheme
                ? FromHex("#E5E7EB")        // gray-200 - inverted
                : FromHex("#1F2937");        // gray-800

            public static Color DangerActive => IsDarkTheme
                ? FromHex("#D1D5DB")        // gray-300 - inverted
                : FromHex("#030712");        // gray-950

            /// <summary>Warning state - neutral grey</summary>
            public static Color Warning => IsDarkTheme
                ? FromHex("#F9FAFB")        // Very light grey (gray-50) - inverted
                : FromHex("#111827");        // Near-black (gray-900)

            public static Color WarningHover => IsDarkTheme
                ? FromHex("#E5E7EB")        // gray-200 - inverted
                : FromHex("#1F2937");        // gray-800

            public static Color WarningActive => IsDarkTheme
                ? FromHex("#D1D5DB")        // gray-300 - inverted
                : FromHex("#030712");        // gray-950

            // ===== TEXT HIERARCHY =====
            // Dark theme: White to light greys (inverted)
            // Light theme: Black to dark greys

            /// <summary>Primary text - Highest contrast</summary>
            public static Color TextPrimary => IsDarkTheme
                ? FromHex("#FFFFFF")        // Pure white - inverted from light's gray-900
                : FromHex("#111827");        // Near-black (gray-900)

            /// <summary>Secondary text - Body text</summary>
            public static Color TextSecondary => IsDarkTheme
                ? FromHex("#D1D5DB")        // Light grey (gray-300) - inverted from light's gray-700
                : FromHex("#374151");        // Dark grey (gray-700)

            /// <summary>Muted text - Helper text</summary>
            public static Color TextMuted => IsDarkTheme
                ? FromHex("#9CA3AF")        // Medium-light grey (gray-400) - inverted from light's gray-500
                : FromHex("#6B7280");        // Medium grey (gray-500)

            /// <summary>Main panel/page headers - Maximum impact</summary>
            public static Color TextHeader => IsDarkTheme
                ? FromHex("#FFFFFF")        // Pure white
                : FromHex("#111827");        // Near-black (gray-900)

            /// <summary>Section headers - Visual hierarchy</summary>
            public static Color TextSectionHeader => IsDarkTheme
                ? FromHex("#E5E7EB")        // Light grey (gray-200) - inverted, no color
                : FromHex("#111827");        // Near-black (gray-900)

            // ===== BORDERS =====
            // Dark theme: Medium-dark greys (inverted)
            // Light theme: Medium greys

            /// <summary>Light edge border</summary>
            public static Color BorderLight => IsDarkTheme
                ? FromHex("#4B5563")        // gray-600 - inverted from light's gray-300
                : FromHex("#D1D5DB");        // gray-300

            /// <summary>Dark edge border</summary>
            public static Color BorderDark => IsDarkTheme
                ? FromHex("#4B5563")        // gray-600 - inverted
                : FromHex("#D1D5DB");        // gray-300

            /// <summary>Default border</summary>
            public static Color Border => IsDarkTheme
                ? FromHex("#4B5563")        // gray-600 - inverted
                : FromHex("#D1D5DB");        // gray-300

            /// <summary>Focus border</summary>
            public static Color BorderFocus => IsDarkTheme
                ? FromHex("#6B7280")        // gray-500 - inverted from light's gray-400
                : FromHex("#9CA3AF");        // gray-400

            /// <summary>Hover border</summary>
            public static Color BorderHover => IsDarkTheme
                ? FromHex("#6B7280")        // gray-500 - inverted
                : FromHex("#D1D5DB");        // gray-300

            /// <summary>Subtle border for separators</summary>
            public static Color BorderSubtle => IsDarkTheme
                ? FromHex("#374151")        // gray-700 - inverted
                : FromHex("#D1D5DB");        // gray-300

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
