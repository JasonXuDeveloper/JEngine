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
        /// Base monochrome palette - the only place hex colors are defined.
        /// All button/toggle colors use the same grey scale for consistency.
        /// </summary>
        private static class Palette
        {
            // Pure
            public static readonly Color White = FromHex("#FFFFFF");
            public static readonly Color Black = FromHex("#111111");

            // Greys (light to dark) - used for all interactive elements
            // Adjusted darker for better contrast
            public static readonly Color Grey50 = FromHex("#F9FAFB");
            public static readonly Color Grey100 = FromHex("#F9F9F9");
            public static readonly Color Grey150 = FromHex("#EFF0F2");  // Hover state (between 100 and 200)
            public static readonly Color Grey200 = FromHex("#E5E7EB");
            public static readonly Color Grey300 = FromHex("#D1D5DB");
            public static readonly Color Grey400 = FromHex("#D4D4D4");  // Light button color
            public static readonly Color Grey500 = FromHex("#A0A0A0");  // Light button hover
            public static readonly Color Grey600 = FromHex("#707070");  // Light button active
            public static readonly Color Grey700 = FromHex("#424242");  // Dark button hover
            public static readonly Color Grey800 = FromHex("#2E2E2E");  // Dark button color (darker)
            public static readonly Color Grey840 = FromHex("#3A3A3A");  // Input hover (lighter than panel)
            public static readonly Color Grey850 = FromHex("#323232");  // Panel/card background
            public static readonly Color Grey875 = FromHex("#2C2C2C");  // Input background
            public static readonly Color Grey900 = FromHex("#222222");  // Dark button active
            public static readonly Color Grey925 = FromHex("#1C1C1C");  // BgSubtle, active states
            public static readonly Color Grey950 = FromHex("#0A0A0A");

            // Semantic text colors
            public static readonly Color TextDark = FromHex("#373737");
            public static readonly Color TextLight = FromHex("#E5E5E5");
            public static readonly Color SuccessHoverLight = FromHex("#1F1F1F");
            public static readonly Color SuccessActiveLight = FromHex("#030303");

            public static Color FromHex(string hex)
            {
                if (ColorUtility.TryParseHtmlString(hex, out var color))
                    return color;
                return Color.magenta;
            }
        }

        /// <summary>
        /// Monochrome color palette that adapts to Unity Editor theme.
        /// Dark theme is a clean grayscale inversion of the light theme.
        /// </summary>
        public static class Colors
        {
            // ===== BACKGROUND LAYERS =====

            /// <summary>Base layer - Deepest background</summary>
            public static Color BgBase => IsDarkTheme ? Palette.Grey950 : Palette.White;

            /// <summary>Subtle layer - Secondary containers</summary>
            public static Color BgSubtle => IsDarkTheme ? Palette.Grey925 : Palette.Grey50;

            /// <summary>Surface layer - Cards, panels, dropdowns (most common)</summary>
            public static Color BgSurface => IsDarkTheme ? Palette.Grey850 : Palette.Grey200;

            /// <summary>Elevated layer - Hover states, important elements (lighter in light mode for contrast)</summary>
            public static Color BgElevated => IsDarkTheme ? Palette.Grey800 : Palette.Grey100;

            /// <summary>Overlay layer - Modals, dropdowns, tooltips</summary>
            public static Color BgOverlay => IsDarkTheme ? Palette.Grey700 : Palette.Grey300;

            /// <summary>Hover state - lighter in dark mode, darker in light mode for visibility</summary>
            public static Color BgHover => IsDarkTheme ? Palette.Grey840 : Palette.Grey300;

            /// <summary>Input field background - more recessed than controls (darker in dark, lighter in light)</summary>
            public static Color BgInput => IsDarkTheme ? Palette.Grey875 : Palette.Grey200;

            // ===== PRIMARY (light button in dark, dark button in light) =====
            // Perfect inversion: Primary dark = Secondary light, Primary light = Secondary dark
            // Dark button color matches BgInput for unified control styling

            /// <summary>Primary button color - inverted between themes</summary>
            public static Color Primary => IsDarkTheme ? Palette.Grey400 : Palette.Grey900;

            public static Color PrimaryHover => IsDarkTheme ? Palette.Grey500 : Palette.Grey700;

            public static Color PrimaryActive => IsDarkTheme ? Palette.Grey600 : Palette.Grey925;

            /// <summary>Primary button text color</summary>
            public static Color PrimaryText => IsDarkTheme ? Palette.Black : Palette.White;

            // ===== SECONDARY (dark button in dark, light button in light) =====
            // Perfect inversion using same grey values as Primary
            // Matches BgInput so all controls share the same color

            /// <summary>Secondary button color - inverted between themes</summary>
            public static Color Secondary => IsDarkTheme ? Palette.Grey900 : Palette.Grey400;

            public static Color SecondaryHover => IsDarkTheme ? Palette.Grey800 : Palette.Grey500;

            public static Color SecondaryActive => IsDarkTheme ? Palette.Grey925 : Palette.Grey600;

            /// <summary>Secondary button text color</summary>
            public static Color SecondaryText => IsDarkTheme ? Palette.White : Palette.Black;

            // ===== SEMANTIC COLORS (all same in monochrome) =====

            /// <summary>Success state - neutral grey</summary>
            public static Color Success => IsDarkTheme ? Palette.Grey100 : Palette.Black;

            public static Color SuccessHover => IsDarkTheme ? Palette.TextLight : Palette.SuccessHoverLight;

            public static Color SuccessActive => IsDarkTheme ? Palette.Grey400 : Palette.SuccessActiveLight;

            /// <summary>Danger state - neutral grey (same as Success in monochrome)</summary>
            public static Color Danger => Success;

            public static Color DangerHover => SuccessHover;

            public static Color DangerActive => SuccessActive;

            /// <summary>Warning state - neutral grey (same as Success in monochrome)</summary>
            public static Color Warning => Success;

            public static Color WarningHover => SuccessHover;

            public static Color WarningActive => SuccessActive;

            // ===== TEXT HIERARCHY =====

            /// <summary>Primary text - Highest contrast</summary>
            public static Color TextPrimary => IsDarkTheme ? Palette.White : Palette.Black;

            /// <summary>Secondary text - Body text</summary>
            public static Color TextSecondary => IsDarkTheme ? Palette.Grey400 : Palette.TextDark;

            /// <summary>Muted text - Helper text</summary>
            public static Color TextMuted => IsDarkTheme ? Palette.Grey500 : Palette.Grey600;

            /// <summary>Main panel/page headers - Maximum impact</summary>
            public static Color TextHeader => TextPrimary;

            /// <summary>Section headers - Visual hierarchy</summary>
            public static Color TextSectionHeader => IsDarkTheme ? Palette.TextLight : Palette.Black;

            // ===== BORDERS =====

            /// <summary>Default border</summary>
            public static Color Border => IsDarkTheme ? Palette.Grey700 : Palette.Grey400;

            /// <summary>Light edge border</summary>
            public static Color BorderLight => Border;

            /// <summary>Dark edge border</summary>
            public static Color BorderDark => Border;

            /// <summary>Focus border</summary>
            public static Color BorderFocus => IsDarkTheme ? Palette.Grey600 : Palette.Grey500;

            /// <summary>Hover border</summary>
            public static Color BorderHover => IsDarkTheme ? Palette.Grey600 : Palette.Grey400;

            /// <summary>Subtle border for separators</summary>
            public static Color BorderSubtle => IsDarkTheme ? Palette.FromHex("#454545") : Palette.Grey400;

            // ===== STATUS COLORS (aliases) =====

            public static Color StatusInfo => Primary;
            public static Color StatusSuccess => Success;
            public static Color StatusWarning => Warning;
            public static Color StatusError => Danger;

            // ===== TOGGLE THUMB =====
            // Thumb always contrasts with track: OFF track=Secondary, ON track=Primary

            /// <summary>Toggle thumb when off - contrasts with Secondary track</summary>
            public static Color ToggleThumbOff => Primary;

            /// <summary>Toggle thumb when on - contrasts with Primary track</summary>
            public static Color ToggleThumbOn => Secondary;
        }

        /// <summary>
        /// Spacing values based on 4px grid.
        /// </summary>
        public static class Spacing
        {
            /// <summary>Extra small spacing (2px).</summary>
            public const float Xs = 2f;
            /// <summary>Small spacing (4px).</summary>
            public const float Sm = 4f;
            /// <summary>Medium spacing (8px).</summary>
            public const float MD = 8f;
            /// <summary>Large spacing (12px).</summary>
            public const float Lg = 12f;
            /// <summary>Extra large spacing (16px).</summary>
            public const float Xl = 16f;
            /// <summary>Extra extra large spacing (24px).</summary>
            public const float Xxl = 24f;
        }

        /// <summary>
        /// Font sizes for different text types.
        /// </summary>
        public static class FontSize
        {
            /// <summary>Extra small font (10px) - metadata, timestamps.</summary>
            public const float Xs = 10f;
            /// <summary>Small font (11px) - secondary labels, hints.</summary>
            public const float Sm = 11f;
            /// <summary>Base font (12px) - default body text.</summary>
            public const float Base = 12f;
            /// <summary>Medium font (13px) - emphasis text.</summary>
            public const float MD = 13f;
            /// <summary>Large font (14px) - section labels.</summary>
            public const float Lg = 14f;
            /// <summary>Extra large font (16px) - section headers.</summary>
            public const float Xl = 16f;
            /// <summary>Title font (18px) - panel headers.</summary>
            public const float Title = 18f;
        }

        /// <summary>
        /// Border radius values for rounded corners.
        /// </summary>
        public static class BorderRadius
        {
            /// <summary>Small radius (3px) - subtle rounding.</summary>
            public const float Sm = 3f;
            /// <summary>Medium radius (5px) - standard rounding.</summary>
            public const float MD = 5f;
            /// <summary>Large radius (8px) - prominent rounding.</summary>
            public const float Lg = 8f;
        }

        /// <summary>
        /// Animation/transition durations in milliseconds.
        /// </summary>
        public static class Transition
        {
            /// <summary>Fast transition (150ms) - micro-interactions.</summary>
            public const int Fast = 150;
            /// <summary>Normal transition (200ms) - standard animations.</summary>
            public const int Normal = 200;
        }

        /// <summary>
        /// Layout constants for form fields and controls.
        /// </summary>
        public static class Layout
        {
            /// <summary>Default form label width (140px).</summary>
            public const float FormLabelWidth = 140f;
            /// <summary>Minimum form label width (60px).</summary>
            public const float FormLabelMinWidth = 60f;
            /// <summary>Minimum touch target size (24px) for accessibility.</summary>
            public const float MinTouchTarget = 24f;
            /// <summary>Minimum control width (80px).</summary>
            public const float MinControlWidth = 80f;
        }
    }
}
