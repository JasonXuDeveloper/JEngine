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
    /// Features glassmorphic design with vibrant accents and 5-layer glass system.
    /// </summary>
    public static class Tokens
    {
        /// <summary>
        /// Gets whether the editor is using the professional (dark) skin.
        /// </summary>
        public static bool IsDarkTheme => EditorGUIUtility.isProSkin;

        /// <summary>
        /// Glassmorphic color palette that adapts to Unity Editor theme.
        /// Features vibrant gradients, translucent glass layers, and glowing accents.
        /// </summary>
        public static class Colors
        {
            // ===== GLASSMORPHIC LAYERS (5-level opacity system) =====
            // Dark theme: Neutral warm grey palette
            // Light theme: Clean white/grey like shadcn
            /// <summary>Glass base layer (40% opacity) - Background elements</summary>
            public static Color BgBase => IsDarkTheme
                ? new Color(20f/255f, 24f/255f, 28f/255f, 0.4f)     // Warm dark grey with 40% opacity
                : FromHex("#FFFFFF");                                 // Pure white

            /// <summary>Glass subtle layer (50% opacity) - Secondary containers</summary>
            public static Color BgSubtle => IsDarkTheme
                ? new Color(32f/255f, 36f/255f, 42f/255f, 0.5f)     // Neutral grey with 50% opacity
                : FromHex("#F9FAFB");                                 // Very light grey (gray-50)

            /// <summary>Glass surface layer (60% opacity) - Cards, panels, dropdowns (most common)</summary>
            public static Color BgSurface => IsDarkTheme
                ? new Color(45f/255f, 52f/255f, 60f/255f, 0.65f)    // Warmer grey with 65% opacity
                : FromHex("#E5E7EB");                                 // Medium light grey (gray-200) - darker for dropdowns

            /// <summary>Glass elevated layer (70% opacity) - Hover states, important elements</summary>
            public static Color BgElevated => IsDarkTheme
                ? new Color(60f/255f, 68f/255f, 78f/255f, 0.75f)    // Lighter warm grey with 75% opacity
                : FromHex("#E5E7EB");                                 // Medium light grey (gray-200)

            /// <summary>Glass overlay layer (80% opacity) - Modals, dropdowns, tooltips</summary>
            public static Color BgOverlay => IsDarkTheme
                ? new Color(80f/255f, 90f/255f, 102f/255f, 0.85f)   // Brightest grey with 85% opacity
                : FromHex("#D1D5DB");                                 // Medium grey (gray-300)

            /// <summary>Hover state (increases opacity for "bring forward" effect)</summary>
            public static Color BgHover => IsDarkTheme
                ? new Color(60f/255f, 68f/255f, 78f/255f, 0.75f)    // BgElevated
                : FromHex("#E5E7EB");                                 // BgElevated (gray-200)

            // ===== VIBRANT ACCENT COLORS (with glow support) =====
            /// <summary>Primary Cyan accent - Main interactive color</summary>
            public static Color Accent => IsDarkTheme
                ? FromHex("#06B6D4")        // cyan-500
                : FromHex("#0891B2");        // cyan-600

            /// <summary>Cyan glow effect (50% opacity for simulated glow)</summary>
            public static Color AccentGlow => IsDarkTheme
                ? new Color(6f/255f, 182f/255f, 212f/255f, 0.5f)
                : new Color(8f/255f, 145f/255f, 178f/255f, 0.4f);

            public static Color AccentHover => IsDarkTheme
                ? FromHex("#22D3EE")        // cyan-400 (lighter)
                : FromHex("#0E7490");        // cyan-700 (darker)

            public static Color AccentMuted => IsDarkTheme
                ? FromHex("#0891B2")        // cyan-600
                : FromHex("#06B6D4");        // cyan-500

            // ===== SEMANTIC COLORS =====
            // Dark theme: Cyan accent (colorful)
            // Light theme: Medium-dark grey buttons (shadcn style)
            public static Color Primary => IsDarkTheme
                ? Accent                    // cyan-500 in dark mode
                : FromHex("#374151");        // Medium-dark grey (gray-700) in light mode

            public static Color PrimaryHover => IsDarkTheme
                ? AccentHover               // cyan-400 in dark mode
                : FromHex("#4B5563");        // Lighter grey (gray-600) in light mode

            public static Color PrimaryActive => IsDarkTheme
                ? FromHex("#0E7490")        // cyan-700 in dark mode
                : FromHex("#1F2937");        // Darker grey (gray-800) in light mode

            // ===== Secondary (Grey buttons) =====
            public static Color Secondary => IsDarkTheme
                ? new Color(71f/255f, 85f/255f, 105f/255f, 0.8f)    // slate-600 (glassy)
                : FromHex("#D1D5DB");                                  // Medium grey (gray-300) - darker

            public static Color SecondaryHover => IsDarkTheme
                ? new Color(100f/255f, 116f/255f, 139f/255f, 0.85f) // slate-500 (lighter)
                : FromHex("#9CA3AF");                                  // Darker grey (gray-400)

            public static Color SecondaryActive => IsDarkTheme
                ? new Color(51f/255f, 65f/255f, 85f/255f, 0.9f)     // slate-700 (darker)
                : FromHex("#6B7280");                                  // Much darker (gray-500)

            // ===== Success (Only use when semantically needed) =====
            // Dark theme: Emerald green (vibrant)
            // Light theme: Dark grey (neutral - reserve green for icons/text only)
            public static Color Success => IsDarkTheme
                ? FromHex("#10B981")        // emerald-500
                : FromHex("#111827");        // Near-black (gray-900) - neutral in light mode

            public static Color SuccessGlow => IsDarkTheme
                ? new Color(16f/255f, 185f/255f, 129f/255f, 0.5f)
                : new Color(17f/255f, 24f/255f, 39f/255f, 0.4f);

            public static Color SuccessHover => IsDarkTheme
                ? FromHex("#34D399")        // emerald-400
                : FromHex("#1F2937");        // gray-800 - neutral hover

            public static Color SuccessActive => IsDarkTheme
                ? FromHex("#059669")        // emerald-600
                : FromHex("#030712");        // gray-950 - neutral active

            // ===== Danger (Only use when semantically needed) =====
            // Dark theme: Rose red (vibrant)
            // Light theme: Dark grey (neutral - reserve red for icons/text only)
            public static Color Danger => IsDarkTheme
                ? FromHex("#F43F5E")        // rose-500
                : FromHex("#111827");        // Near-black (gray-900) - neutral in light mode

            public static Color DangerGlow => IsDarkTheme
                ? new Color(244f/255f, 63f/255f, 94f/255f, 0.5f)
                : new Color(17f/255f, 24f/255f, 39f/255f, 0.4f);

            public static Color DangerHover => IsDarkTheme
                ? FromHex("#FB7185")        // rose-400
                : FromHex("#1F2937");        // gray-800 - neutral hover

            public static Color DangerActive => IsDarkTheme
                ? FromHex("#E11D48")        // rose-600
                : FromHex("#030712");        // gray-950 - neutral active

            // ===== Warning (Only use when semantically needed) =====
            // Dark theme: Amber yellow (vibrant)
            // Light theme: Dark grey (neutral - reserve yellow for icons/text only)
            public static Color Warning => IsDarkTheme
                ? FromHex("#F59E0B")        // amber-500
                : FromHex("#111827");        // Near-black (gray-900) - neutral in light mode

            public static Color WarningGlow => IsDarkTheme
                ? new Color(245f/255f, 158f/255f, 11f/255f, 0.5f)
                : new Color(17f/255f, 24f/255f, 39f/255f, 0.4f);

            public static Color WarningHover => IsDarkTheme
                ? FromHex("#FBBF24")        // amber-400
                : FromHex("#1F2937");        // gray-800 - neutral hover

            public static Color WarningActive => IsDarkTheme
                ? FromHex("#D97706")        // amber-600
                : FromHex("#030712");        // gray-950 - neutral active

            // ===== TEXT HIERARCHY =====
            // Dark theme: Monochromatic blue/white
            // Light theme: Clean black/grey like shadcn
            /// <summary>Primary text - Highest contrast</summary>
            public static Color TextPrimary => IsDarkTheme
                ? FromHex("#FFFFFF")        // Pure white for primary text
                : FromHex("#111827");        // Near-black (gray-900)

            /// <summary>Secondary text - Body text</summary>
            public static Color TextSecondary => IsDarkTheme
                ? FromHex("#E2E8F0")        // slate-200 for body text
                : FromHex("#374151");        // Dark grey (gray-700)

            /// <summary>Muted text - Helper text</summary>
            public static Color TextMuted => IsDarkTheme
                ? FromHex("#94A3B8")        // slate-400 for helper text
                : FromHex("#6B7280");        // Medium grey (gray-500)

            /// <summary>Main panel/page headers - Maximum impact</summary>
            public static Color TextHeader => IsDarkTheme
                ? FromHex("#FFFFFF")        // Pure white for main headers
                : FromHex("#111827");        // Near-black (gray-900)

            /// <summary>Section headers - Visual hierarchy</summary>
            public static Color TextSectionHeader => IsDarkTheme
                ? FromHex("#93C5FD")        // blue-300 - soft light blue
                : FromHex("#111827");        // Near-black (gray-900) - NO COLOR in light mode

            // ===== BORDERS =====
            // Dark theme: Glass reflection borders
            // Light theme: Darker grey borders for inputs/cards
            /// <summary>Light edge border (top/left light reflection)</summary>
            public static Color BorderLight => IsDarkTheme
                ? new Color(1f, 1f, 1f, 0.2f)       // rgba(255,255,255,0.2)
                : FromHex("#D1D5DB");                 // gray-300 (darker for inputs)

            /// <summary>Dark edge border (bottom/right shadow)</summary>
            public static Color BorderDark => IsDarkTheme
                ? new Color(0f, 0f, 0f, 0.2f)       // rgba(0,0,0,0.2)
                : FromHex("#D1D5DB");                 // gray-300 (darker for inputs)

            /// <summary>Default border (for non-glass elements)</summary>
            public static Color Border => IsDarkTheme
                ? new Color(1f, 1f, 1f, 0.15f)      // Subtle white
                : FromHex("#D1D5DB");                 // gray-300 (darker)

            /// <summary>Focus border</summary>
            // Dark mode: Cyan accent
            // Light mode: Lighter grey (subtle)
            public static Color BorderFocus => IsDarkTheme
                ? Accent                      // Cyan in dark mode
                : FromHex("#9CA3AF");         // Light grey (gray-400) in light mode

            /// <summary>Focus glow effect</summary>
            public static Color BorderFocusGlow => IsDarkTheme
                ? AccentGlow
                : new Color(55f/255f, 65f/255f, 81f/255f, 0.4f); // Dark grey with opacity

            /// <summary>Hover border (brighter on interaction)</summary>
            public static Color BorderHover => IsDarkTheme
                ? new Color(1f, 1f, 1f, 0.3f)       // Brighter light
                : FromHex("#D1D5DB");                 // gray-300 (slightly darker on hover)

            /// <summary>Subtle border for separators and input borders</summary>
            public static Color BorderSubtle => IsDarkTheme
                ? new Color(1f, 1f, 1f, 0.05f)
                : FromHex("#D1D5DB");                 // gray-300 (darker for inputs)

            // ===== STATUS COLORS =====
            public static Color StatusInfo => Accent;
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
