# Section 16 — Theming & Design Tokens

## Overview

Token-based theming system for JUI. A `Theme` ScriptableObject defines design tokens for colors, typography, spacing, border radius, animation timing, and shadows. `ThemeManager` applies themes reactively by swapping CSS custom property (variable) values on the root VisualElement. Theme changes propagate instantly through all elements that reference `--jui-*` variables in their USS. A companion USS file (`jui-tokens.uss`) declares all token variables with default values. A utility USS file (`jui-utilities.uss`) provides Tailwind-inspired utility classes for rapid prototyping. Runtime theme switching supports both instant `Apply()` and animated `TransitionTo()`.

## Dependencies

- Sections 1--6 (Sequential Core: signals for reactive theme state)

## File Structure

- `Runtime/JUI/Theming/Theme.cs`
- `Runtime/JUI/Theming/ThemeManager.cs`
- `Runtime/JUI/USS/jui-tokens.uss`
- `Runtime/JUI/USS/jui-utilities.uss`

## API Design

### Theme ScriptableObject

```csharp
/// <summary>
/// ScriptableObject defining a complete set of design tokens for a JUI theme.
/// Create instances via <c>Assets > Create > JUI > Theme</c> in the Unity editor.
/// Tokens are applied to the UI by <see cref="ThemeManager"/> as CSS custom properties.
/// </summary>
[CreateAssetMenu(menuName = "JUI/Theme", fileName = "NewTheme")]
public class Theme : ScriptableObject
{
    // ── Colors ──────────────────────────────────────────────────────

    /// <summary>Primary brand color. Used for primary buttons, active states, links.</summary>
    public Color Primary = new(0.2f, 0.47f, 0.96f, 1f);    // #3378F5

    /// <summary>Secondary brand color. Used for secondary buttons and accents.</summary>
    public Color Secondary = new(0.4f, 0.4f, 0.45f, 1f);   // #666673

    /// <summary>Accent/highlight color. Used for badges, highlights, selected states.</summary>
    public Color Accent = new(0.95f, 0.6f, 0.07f, 1f);     // #F29912

    /// <summary>Page/screen background color.</summary>
    public Color Background = new(0.08f, 0.08f, 0.1f, 1f); // #141419

    /// <summary>Surface color for cards, panels, and elevated containers.</summary>
    public Color Surface = new(0.13f, 0.13f, 0.16f, 1f);   // #212128

    /// <summary>Error/danger color. Used for destructive actions and error states.</summary>
    public Color Error = new(0.91f, 0.27f, 0.27f, 1f);     // #E84545

    /// <summary>Success color. Used for confirmations and success states.</summary>
    public Color Success = new(0.18f, 0.78f, 0.44f, 1f);   // #2EC770

    /// <summary>Warning color. Used for caution states and non-blocking alerts.</summary>
    public Color Warning = new(0.96f, 0.76f, 0.07f, 1f);   // #F5C212

    // ── Text Colors ─────────────────────────────────────────────────

    /// <summary>Primary text color. Used for headings and body text.</summary>
    public Color TextPrimary = new(0.93f, 0.93f, 0.95f, 1f);   // #EDEDF2

    /// <summary>Secondary/muted text color. Used for labels and descriptions.</summary>
    public Color TextSecondary = new(0.6f, 0.6f, 0.65f, 1f);   // #9999A6

    /// <summary>Disabled text color. Used for inactive elements.</summary>
    public Color TextDisabled = new(0.4f, 0.4f, 0.43f, 1f);    // #66666E

    // ── Typography ──────────────────────────────────────────────────

    /// <summary>Font used for headings (h1--h6). Null uses the default UI Toolkit font.</summary>
    public FontDefinition HeadingFont;

    /// <summary>Font used for body text, labels, and controls. Null uses the default UI Toolkit font.</summary>
    public FontDefinition BodyFont;

    /// <summary>
    /// Multiplier applied to heading font sizes. 1.0 = default sizes.
    /// Increase for larger headings, decrease for a more compact look.
    /// </summary>
    public float HeadingSizeMultiplier = 1f;

    // ── Spacing ─────────────────────────────────────────────────────

    /// <summary>
    /// Base spacing unit in pixels. All spacing tokens are computed as multiples
    /// of this value. Default is 8px (8-point grid system).
    /// <list type="bullet">
    /// <item><description>xs = SpacingUnit * 0.5 (4px)</description></item>
    /// <item><description>sm = SpacingUnit * 1.0 (8px)</description></item>
    /// <item><description>md = SpacingUnit * 2.0 (16px)</description></item>
    /// <item><description>lg = SpacingUnit * 3.0 (24px)</description></item>
    /// <item><description>xl = SpacingUnit * 4.0 (32px)</description></item>
    /// <item><description>2xl = SpacingUnit * 6.0 (48px)</description></item>
    /// <item><description>3xl = SpacingUnit * 8.0 (64px)</description></item>
    /// </list>
    /// </summary>
    public float SpacingUnit = 8f;

    // ── Border Radius ───────────────────────────────────────────────

    /// <summary>
    /// Base border radius in pixels. Derived tokens:
    /// <list type="bullet">
    /// <item><description>none = 0px</description></item>
    /// <item><description>sm = BorderRadius * 0.5 (2px)</description></item>
    /// <item><description>md = BorderRadius * 1.0 (4px)</description></item>
    /// <item><description>lg = BorderRadius * 2.0 (8px)</description></item>
    /// <item><description>xl = BorderRadius * 3.0 (12px)</description></item>
    /// <item><description>full = 9999px (pill shape)</description></item>
    /// </list>
    /// </summary>
    public float BorderRadius = 4f;

    // ── Animation ───────────────────────────────────────────────────

    /// <summary>
    /// Default transition duration in seconds for interactive state changes
    /// (hover, focus, press). Used as the base for <c>--jui-transition-duration</c>.
    /// </summary>
    public float TransitionDuration = 0.3f;

    // ── Shadows ─────────────────────────────────────────────────────

    /// <summary>
    /// Default text/box shadow applied to elevated elements. Set offset to (0,0)
    /// and blur to 0 to disable shadows entirely.
    /// </summary>
    public TextShadow DefaultShadow = new()
    {
        offset = new Vector2(0, 2),
        blurRadius = 4,
        color = new Color(0, 0, 0, 0.25f)
    };
}
```

### ThemeManager

```csharp
/// <summary>
/// Manages the active theme and applies design tokens as CSS custom properties
/// on UIDocument root elements. Theme changes are reactive via
/// <see cref="CurrentSignal"/> -- effects and bindings can react to theme switches.
/// </summary>
public static class ThemeManager
{
    /// <summary>
    /// Reactive signal holding the currently active theme. Subscribe via effects
    /// or computed signals to react to theme changes.
    /// </summary>
    public static Signal<Theme> CurrentSignal { get; }

    /// <summary>
    /// Gets the currently applied theme. Shorthand for <c>CurrentSignal.Value</c>.
    /// Returns null if no theme has been applied yet.
    /// </summary>
    public static Theme Current => CurrentSignal.Value;

    /// <summary>
    /// Applies the given theme immediately. Updates all CSS custom properties on
    /// every active UIDocument root element. Sets <see cref="CurrentSignal"/> to
    /// the new theme, triggering reactive subscribers.
    /// </summary>
    /// <param name="theme">The theme to apply. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="theme"/> is null.</exception>
    public static void Apply(Theme theme);

    /// <summary>
    /// Transitions from the current theme to the given theme over
    /// <paramref name="dur"/> seconds. Color tokens are interpolated smoothly.
    /// Non-color tokens (spacing, radius, font) are applied instantly at the
    /// midpoint of the transition. Sets <see cref="CurrentSignal"/> to the new
    /// theme immediately (reactive subscribers fire at transition start).
    /// </summary>
    /// <param name="theme">The theme to transition to. Must not be null.</param>
    /// <param name="dur">Transition duration in seconds. Defaults to 0.3s.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="theme"/> is null.</exception>
    public static void TransitionTo(Theme theme, float dur = 0.3f);

    /// <summary>
    /// Registers a UIDocument root element to receive theme token updates.
    /// Called automatically by UILayerManager when creating new layer roots.
    /// Can be called manually for custom UIDocuments not managed by JUI.
    /// </summary>
    /// <param name="root">The root VisualElement of a UIDocument.</param>
    public static void RegisterRoot(VisualElement root);

    /// <summary>
    /// Unregisters a root element from theme updates. Called automatically
    /// when UILayerManager disposes a layer.
    /// </summary>
    /// <param name="root">The root VisualElement to unregister.</param>
    public static void UnregisterRoot(VisualElement root);
}
```

## Data Structures

| Type | Role |
|------|------|
| `Theme` | ScriptableObject holding all design token values. Created via asset menu. |
| `Signal<Theme>` | Reactive signal in `ThemeManager` holding the active theme. Enables reactive theme awareness. |
| `List<VisualElement> _roots` | Internal list in `ThemeManager` of all registered root elements that receive CSS variable updates. |
| CSS Custom Properties | `--jui-*` variables set on root elements via `style.SetCustomProperty()` or inline style. |

## Implementation Notes

- **CSS variable application**: `ThemeManager.Apply()` iterates all registered root elements and calls `root.style.SetCustomProperty("--jui-color-primary", theme.Primary)` (and equivalent for each token). All child elements that reference `var(--jui-color-primary)` in their USS automatically pick up the new value -- no manual per-element update needed.
- **Derived tokens**: Spacing and radius tokens are computed from the base values at apply time. For example, `--jui-spacing-md = theme.SpacingUnit * 2`. These computations happen in `ThemeManager.Apply()`, not in the USS file.
- **TransitionTo color interpolation**: Uses `JUIMotion.AnimateSignal` (from Section 15) or a direct LitMotion tween to interpolate each color token from the old value to the new value over the specified duration. Each frame, the interpolated color is set as a CSS variable. Non-color tokens (spacing, radius, fonts) cannot be meaningfully interpolated and are applied instantly at the midpoint.
- **Font handling**: `HeadingFont` and `BodyFont` are set as `--jui-font-heading` and `--jui-font-body` CSS variables. If null, the default UI Toolkit font is used (the variable is not set, allowing USS fallback).
- **Multiple UIDocuments**: Each UILayer's UIDocument has its own root element. `ThemeManager` maintains a list of all roots and applies tokens to each. This ensures consistent theming across HUD, popups, modals, and overlays.
- **Thread safety**: `ThemeManager` is main-thread only. No locking required.
- **Editor preview**: Theme changes in the Unity Inspector trigger `Apply()` automatically via an `OnValidate()` hook on the `Theme` ScriptableObject, enabling live preview in the editor.

## Source Generator Notes

N/A for this section -- theming is a purely runtime system. No source generation is required.

## USS Token Variables

The `jui-tokens.uss` file declares all design token variables with default fallback values. These defaults match the `Theme` class field defaults so that the UI renders correctly even before `ThemeManager.Apply()` is called.

```css
/* jui-tokens.uss — JUI Design Token Variables */

:root {
    /* ── Colors ──────────────────────────────────────────── */
    --jui-color-primary:        #3378F5;
    --jui-color-secondary:      #666673;
    --jui-color-accent:         #F29912;
    --jui-color-background:     #141419;
    --jui-color-surface:        #212128;
    --jui-color-error:          #E84545;
    --jui-color-success:        #2EC770;
    --jui-color-warning:        #F5C212;

    /* ── Text Colors ─────────────────────────────────────── */
    --jui-color-text-primary:   #EDEDF2;
    --jui-color-text-secondary: #9999A6;
    --jui-color-text-disabled:  #66666E;

    /* ── Typography ──────────────────────────────────────── */
    --jui-font-heading:         initial;
    --jui-font-body:            initial;
    --jui-font-size-xs:         10px;
    --jui-font-size-sm:         12px;
    --jui-font-size-md:         14px;
    --jui-font-size-lg:         16px;
    --jui-font-size-xl:         20px;
    --jui-font-size-2xl:        24px;
    --jui-font-size-3xl:        30px;
    --jui-font-size-h1:         36px;
    --jui-font-size-h2:         30px;
    --jui-font-size-h3:         24px;
    --jui-font-size-h4:         20px;
    --jui-font-size-h5:         16px;
    --jui-font-size-h6:         14px;

    /* ── Spacing ─────────────────────────────────────────── */
    --jui-spacing-xs:           4px;
    --jui-spacing-sm:           8px;
    --jui-spacing-md:           16px;
    --jui-spacing-lg:           24px;
    --jui-spacing-xl:           32px;
    --jui-spacing-2xl:          48px;
    --jui-spacing-3xl:          64px;

    /* ── Border Radius ───────────────────────────────────── */
    --jui-radius-none:          0px;
    --jui-radius-sm:            2px;
    --jui-radius-md:            4px;
    --jui-radius-lg:            8px;
    --jui-radius-xl:            12px;
    --jui-radius-full:          9999px;

    /* ── Animation ───────────────────────────────────────── */
    --jui-transition-duration:  300ms;
    --jui-transition-fast:      150ms;
    --jui-transition-slow:      500ms;

    /* ── Shadows ─────────────────────────────────────────── */
    --jui-shadow-offset-x:      0px;
    --jui-shadow-offset-y:      2px;
    --jui-shadow-blur:           4px;
    --jui-shadow-color:          rgba(0, 0, 0, 0.25);

    /* ── Elevation (shadow presets) ──────────────────────── */
    --jui-shadow-sm:            0px 1px 2px rgba(0, 0, 0, 0.15);
    --jui-shadow-md:            0px 2px 4px rgba(0, 0, 0, 0.25);
    --jui-shadow-lg:            0px 4px 8px rgba(0, 0, 0, 0.30);
    --jui-shadow-xl:            0px 8px 16px rgba(0, 0, 0, 0.35);

    /* ── Borders ─────────────────────────────────────────── */
    --jui-border-width:         1px;
    --jui-border-color:         rgba(255, 255, 255, 0.1);

    /* ── Opacity ─────────────────────────────────────────── */
    --jui-opacity-disabled:     0.4;
    --jui-opacity-hover:        0.8;
    --jui-opacity-pressed:      0.6;

    /* ── Z-Index (matches UILayer enum) ──────────────────── */
    --jui-z-background:         0;
    --jui-z-hud:                100;
    --jui-z-popup:              200;
    --jui-z-modal:              300;
    --jui-z-tooltip:            400;
    --jui-z-context-menu:       500;
    --jui-z-notification:       600;
    --jui-z-debug:              900;
    --jui-z-overlay:            1000;
}
```

## USS Utility Classes

The `jui-utilities.uss` file provides Tailwind-inspired utility classes for rapid prototyping and layout. Each class maps to a single CSS property using the design token variables.

```css
/* jui-utilities.uss — JUI Utility Classes */

/* ── Display ─────────────────────────────────────────────── */
.jui-hidden         { display: none; }
.jui-flex            { display: flex; }

/* ── Flex Direction ──────────────────────────────────────── */
.jui-flex-row        { flex-direction: row; }
.jui-flex-col        { flex-direction: column; }
.jui-flex-row-reverse { flex-direction: row-reverse; }
.jui-flex-col-reverse { flex-direction: column-reverse; }

/* ── Flex Wrap ───────────────────────────────────────────── */
.jui-flex-wrap       { flex-wrap: wrap; }
.jui-flex-nowrap     { flex-wrap: nowrap; }

/* ── Flex Grow/Shrink ────────────────────────────────────── */
.jui-flex-1          { flex-grow: 1; flex-shrink: 1; }
.jui-flex-auto       { flex-grow: 1; flex-shrink: 1; }
.jui-flex-none       { flex-grow: 0; flex-shrink: 0; }
.jui-grow            { flex-grow: 1; }
.jui-grow-0          { flex-grow: 0; }
.jui-shrink          { flex-shrink: 1; }
.jui-shrink-0        { flex-shrink: 0; }

/* ── Justify Content ─────────────────────────────────────── */
.jui-justify-start   { justify-content: flex-start; }
.jui-justify-center  { justify-content: center; }
.jui-justify-end     { justify-content: flex-end; }
.jui-justify-between { justify-content: space-between; }
.jui-justify-around  { justify-content: space-around; }

/* ── Align Items ─────────────────────────────────────────── */
.jui-items-start     { align-items: flex-start; }
.jui-items-center    { align-items: center; }
.jui-items-end       { align-items: flex-end; }
.jui-items-stretch   { align-items: stretch; }

/* ── Align Self ──────────────────────────────────────────── */
.jui-self-start      { align-self: flex-start; }
.jui-self-center     { align-self: center; }
.jui-self-end        { align-self: flex-end; }
.jui-self-stretch    { align-self: stretch; }

/* ── Padding ─────────────────────────────────────────────── */
.jui-p-0             { padding: 0; }
.jui-p-xs            { padding: var(--jui-spacing-xs); }
.jui-p-sm            { padding: var(--jui-spacing-sm); }
.jui-p-md            { padding: var(--jui-spacing-md); }
.jui-p-lg            { padding: var(--jui-spacing-lg); }
.jui-p-xl            { padding: var(--jui-spacing-xl); }
.jui-p-2xl           { padding: var(--jui-spacing-2xl); }
.jui-p-3xl           { padding: var(--jui-spacing-3xl); }

.jui-px-xs           { padding-left: var(--jui-spacing-xs); padding-right: var(--jui-spacing-xs); }
.jui-px-sm           { padding-left: var(--jui-spacing-sm); padding-right: var(--jui-spacing-sm); }
.jui-px-md           { padding-left: var(--jui-spacing-md); padding-right: var(--jui-spacing-md); }
.jui-px-lg           { padding-left: var(--jui-spacing-lg); padding-right: var(--jui-spacing-lg); }
.jui-px-xl           { padding-left: var(--jui-spacing-xl); padding-right: var(--jui-spacing-xl); }

.jui-py-xs           { padding-top: var(--jui-spacing-xs); padding-bottom: var(--jui-spacing-xs); }
.jui-py-sm           { padding-top: var(--jui-spacing-sm); padding-bottom: var(--jui-spacing-sm); }
.jui-py-md           { padding-top: var(--jui-spacing-md); padding-bottom: var(--jui-spacing-md); }
.jui-py-lg           { padding-top: var(--jui-spacing-lg); padding-bottom: var(--jui-spacing-lg); }
.jui-py-xl           { padding-top: var(--jui-spacing-xl); padding-bottom: var(--jui-spacing-xl); }

/* ── Margin ──────────────────────────────────────────────── */
.jui-m-0             { margin: 0; }
.jui-m-xs            { margin: var(--jui-spacing-xs); }
.jui-m-sm            { margin: var(--jui-spacing-sm); }
.jui-m-md            { margin: var(--jui-spacing-md); }
.jui-m-lg            { margin: var(--jui-spacing-lg); }
.jui-m-xl            { margin: var(--jui-spacing-xl); }
.jui-m-auto          { margin: auto; }

.jui-mx-xs           { margin-left: var(--jui-spacing-xs); margin-right: var(--jui-spacing-xs); }
.jui-mx-sm           { margin-left: var(--jui-spacing-sm); margin-right: var(--jui-spacing-sm); }
.jui-mx-md           { margin-left: var(--jui-spacing-md); margin-right: var(--jui-spacing-md); }
.jui-mx-lg           { margin-left: var(--jui-spacing-lg); margin-right: var(--jui-spacing-lg); }
.jui-mx-auto         { margin-left: auto; margin-right: auto; }

.jui-my-xs           { margin-top: var(--jui-spacing-xs); margin-bottom: var(--jui-spacing-xs); }
.jui-my-sm           { margin-top: var(--jui-spacing-sm); margin-bottom: var(--jui-spacing-sm); }
.jui-my-md           { margin-top: var(--jui-spacing-md); margin-bottom: var(--jui-spacing-md); }
.jui-my-lg           { margin-top: var(--jui-spacing-lg); margin-bottom: var(--jui-spacing-lg); }
.jui-my-auto         { margin-top: auto; margin-bottom: auto; }

/* ── Gap ─────────────────────────────────────────────────── */
.jui-gap-xs          { gap: var(--jui-spacing-xs); }
.jui-gap-sm          { gap: var(--jui-spacing-sm); }
.jui-gap-md          { gap: var(--jui-spacing-md); }
.jui-gap-lg          { gap: var(--jui-spacing-lg); }
.jui-gap-xl          { gap: var(--jui-spacing-xl); }

/* ── Width / Height ──────────────────────────────────────── */
.jui-w-full          { width: 100%; }
.jui-w-half          { width: 50%; }
.jui-w-auto          { width: auto; }
.jui-h-full          { height: 100%; }
.jui-h-half          { height: 50%; }
.jui-h-auto          { height: auto; }
.jui-min-w-0         { min-width: 0; }
.jui-min-h-0         { min-height: 0; }

/* ── Background Colors ───────────────────────────────────── */
.jui-bg-primary      { background-color: var(--jui-color-primary); }
.jui-bg-secondary    { background-color: var(--jui-color-secondary); }
.jui-bg-accent       { background-color: var(--jui-color-accent); }
.jui-bg-background   { background-color: var(--jui-color-background); }
.jui-bg-surface      { background-color: var(--jui-color-surface); }
.jui-bg-error        { background-color: var(--jui-color-error); }
.jui-bg-success      { background-color: var(--jui-color-success); }
.jui-bg-warning      { background-color: var(--jui-color-warning); }
.jui-bg-transparent  { background-color: rgba(0, 0, 0, 0); }

/* ── Text Colors ─────────────────────────────────────────── */
.jui-text-primary    { color: var(--jui-color-text-primary); }
.jui-text-secondary  { color: var(--jui-color-text-secondary); }
.jui-text-disabled   { color: var(--jui-color-text-disabled); }
.jui-text-accent     { color: var(--jui-color-accent); }
.jui-text-error      { color: var(--jui-color-error); }
.jui-text-success    { color: var(--jui-color-success); }
.jui-text-warning    { color: var(--jui-color-warning); }

/* ── Font Sizes ──────────────────────────────────────────── */
.jui-text-xs         { font-size: var(--jui-font-size-xs); }
.jui-text-sm         { font-size: var(--jui-font-size-sm); }
.jui-text-md         { font-size: var(--jui-font-size-md); }
.jui-text-lg         { font-size: var(--jui-font-size-lg); }
.jui-text-xl         { font-size: var(--jui-font-size-xl); }
.jui-text-2xl        { font-size: var(--jui-font-size-2xl); }
.jui-text-3xl        { font-size: var(--jui-font-size-3xl); }
.jui-text-h1         { font-size: var(--jui-font-size-h1); -unity-font-definition: var(--jui-font-heading); }
.jui-text-h2         { font-size: var(--jui-font-size-h2); -unity-font-definition: var(--jui-font-heading); }
.jui-text-h3         { font-size: var(--jui-font-size-h3); -unity-font-definition: var(--jui-font-heading); }
.jui-text-h4         { font-size: var(--jui-font-size-h4); -unity-font-definition: var(--jui-font-heading); }
.jui-text-h5         { font-size: var(--jui-font-size-h5); -unity-font-definition: var(--jui-font-heading); }
.jui-text-h6         { font-size: var(--jui-font-size-h6); -unity-font-definition: var(--jui-font-heading); }

/* ── Font Weight ─────────────────────────────────────────── */
.jui-font-normal     { -unity-font-style: normal; }
.jui-font-bold       { -unity-font-style: bold; }
.jui-font-italic     { -unity-font-style: italic; }
.jui-font-bold-italic { -unity-font-style: bold-and-italic; }

/* ── Text Alignment ──────────────────────────────────────── */
.jui-text-left       { -unity-text-align: middle-left; }
.jui-text-center     { -unity-text-align: middle-center; }
.jui-text-right      { -unity-text-align: middle-right; }
.jui-text-top-left   { -unity-text-align: upper-left; }
.jui-text-top-center { -unity-text-align: upper-center; }

/* ── Border Radius ───────────────────────────────────────── */
.jui-rounded-none    { border-radius: var(--jui-radius-none); }
.jui-rounded-sm      { border-radius: var(--jui-radius-sm); }
.jui-rounded-md      { border-radius: var(--jui-radius-md); }
.jui-rounded-lg      { border-radius: var(--jui-radius-lg); }
.jui-rounded-xl      { border-radius: var(--jui-radius-xl); }
.jui-rounded-full    { border-radius: var(--jui-radius-full); }

/* ── Borders ─────────────────────────────────────────────── */
.jui-border          { border-width: var(--jui-border-width); border-color: var(--jui-border-color); }
.jui-border-primary  { border-width: var(--jui-border-width); border-color: var(--jui-color-primary); }
.jui-border-error    { border-width: var(--jui-border-width); border-color: var(--jui-color-error); }
.jui-border-none     { border-width: 0; }

/* ── Overflow ────────────────────────────────────────────── */
.jui-overflow-hidden { overflow: hidden; }
.jui-overflow-scroll { overflow: scroll; }
.jui-overflow-visible { overflow: visible; }

/* ── Opacity ─────────────────────────────────────────────── */
.jui-opacity-0       { opacity: 0; }
.jui-opacity-25      { opacity: 0.25; }
.jui-opacity-50      { opacity: 0.5; }
.jui-opacity-75      { opacity: 0.75; }
.jui-opacity-100     { opacity: 1; }
.jui-opacity-disabled { opacity: var(--jui-opacity-disabled); }

/* ── Position ────────────────────────────────────────────── */
.jui-absolute        { position: absolute; }
.jui-relative        { position: relative; }
.jui-inset-0         { top: 0; right: 0; bottom: 0; left: 0; }

/* ── Cursor ──────────────────────────────────────────────── */
.jui-cursor-pointer  { cursor: link; }
.jui-cursor-default  { cursor: arrow; }

/* ── Transitions ─────────────────────────────────────────── */
.jui-transition      { transition-duration: var(--jui-transition-duration); transition-property: all; }
.jui-transition-fast { transition-duration: var(--jui-transition-fast); transition-property: all; }
.jui-transition-slow { transition-duration: var(--jui-transition-slow); transition-property: all; }
.jui-transition-none { transition-duration: 0ms; }

/* ── Interactive States (pseudo-class rules) ─────────────── */
.jui-hoverable:hover { opacity: var(--jui-opacity-hover); }
.jui-pressable:active { opacity: var(--jui-opacity-pressed); }
```

## Usage Examples

```csharp
// --- Create a theme asset ---
// In Unity: Assets > Create > JUI > Theme
// Configure colors, fonts, spacing in the Inspector

// --- Apply a theme at startup ---
var darkTheme = Resources.Load<Theme>("Themes/DarkTheme");
ThemeManager.Apply(darkTheme);

// --- Switch themes at runtime ---
var lightTheme = Resources.Load<Theme>("Themes/LightTheme");
ThemeManager.Apply(lightTheme); // instant switch

// --- Animated theme transition ---
ThemeManager.TransitionTo(lightTheme, 0.5f);
// Colors smoothly interpolate over 0.5 seconds

// --- React to theme changes ---
var fx = EffectPool<Signal<Theme>>.Rent(
    static (sig) => Debug.Log($"Theme changed to: {sig.Value.name}"),
    ThemeManager.CurrentSignal);
fx.Run();

// --- Use tokens in USS ---
// In your .uss file:
// .health-bar {
//     background-color: var(--jui-color-error);
//     border-radius: var(--jui-radius-md);
//     padding: var(--jui-spacing-sm);
//     transition-duration: var(--jui-transition-duration);
// }
//
// .health-bar:hover {
//     background-color: var(--jui-color-warning);
// }

// --- Use utility classes in components ---
// myPanel.AddToClassList("jui-flex-row");
// myPanel.AddToClassList("jui-items-center");
// myPanel.AddToClassList("jui-gap-md");
// myPanel.AddToClassList("jui-p-lg");
// myPanel.AddToClassList("jui-bg-surface");
// myPanel.AddToClassList("jui-rounded-lg");

// --- Compose utilities for a card layout ---
// card.AddToClassList("jui-flex-col");
// card.AddToClassList("jui-p-md");
// card.AddToClassList("jui-bg-surface");
// card.AddToClassList("jui-rounded-lg");
// card.AddToClassList("jui-border");
// card.AddToClassList("jui-gap-sm");
//
// title.AddToClassList("jui-text-h3");
// title.AddToClassList("jui-text-primary");
//
// subtitle.AddToClassList("jui-text-sm");
// subtitle.AddToClassList("jui-text-secondary");

// --- Custom theme for a specific game ---
[CreateAssetMenu(menuName = "JUI/Theme")]
public class Theme : ScriptableObject
{
    // Inherit all default tokens, customize:
    // Primary = cyberpunk neon blue
    // Background = dark grey
    // HeadingFont = pixel font
    // SpacingUnit = 4 (compact 4px grid)
}

// --- Register custom UIDocument roots ---
var customDoc = gameObject.AddComponent<UIDocument>();
ThemeManager.RegisterRoot(customDoc.rootVisualElement);
// Now this UIDocument's elements can use --jui-* variables
```

## Test Plan

1. **Apply sets CSS variables on root**: Call `ThemeManager.Apply(theme)`, verify `--jui-color-primary` on the root element matches `theme.Primary`.
2. **Apply updates all registered roots**: Register two root elements, apply a theme, verify both roots have updated variables.
3. **CurrentSignal updates on Apply**: Subscribe an effect to `ThemeManager.CurrentSignal`, call `Apply(newTheme)`, verify the effect fires with the new theme.
4. **TransitionTo interpolates colors**: Call `TransitionTo(newTheme, 1f)`, advance time by 0.5s, verify color variables are approximately the midpoint between old and new theme colors.
5. **TransitionTo applies non-color tokens at midpoint**: Call `TransitionTo`, verify spacing/radius variables change at the 50% mark (not gradually).
6. **Token derivation**: Set `SpacingUnit = 10`, apply theme, verify `--jui-spacing-md` is 20px (10 * 2).
7. **Radius derivation**: Set `BorderRadius = 6`, apply theme, verify `--jui-radius-lg` is 12px (6 * 2).
8. **Heading size multiplier**: Set `HeadingSizeMultiplier = 1.5`, apply theme, verify `--jui-font-size-h1` is 54px (36 * 1.5).
9. **Null font uses default**: Set `HeadingFont` to null, apply theme, verify `--jui-font-heading` is not set (or set to `initial`).
10. **RegisterRoot/UnregisterRoot**: Register a root, apply a theme (tokens applied). Unregister, apply a new theme, verify the unregistered root is NOT updated.
11. **USS token file loads without errors**: Load `jui-tokens.uss` as a stylesheet, verify no parse errors.
12. **USS utility classes apply correct styles**: Add `jui-flex-row` to an element, verify `flexDirection == FlexDirection.Row`. Test representative classes from each category.
13. **Theme ScriptableObject serializes/deserializes**: Create a theme, save, reload, verify all values are preserved.
14. **Apply with null throws ArgumentNullException**: Call `Apply(null)`, verify `ArgumentNullException`.

## Acceptance Criteria

- [ ] `Theme` ScriptableObject defines tokens for colors, text colors, typography, spacing, radius, animation, and shadows
- [ ] `Theme` is creatable via `Assets > Create > JUI > Theme`
- [ ] `ThemeManager.Apply()` sets all `--jui-*` CSS custom properties on all registered root elements
- [ ] `ThemeManager.CurrentSignal` is a reactive `Signal<Theme>` updated on `Apply()` and `TransitionTo()`
- [ ] `ThemeManager.TransitionTo()` interpolates color tokens over the specified duration
- [ ] `ThemeManager.TransitionTo()` applies non-color tokens at the transition midpoint
- [ ] `RegisterRoot()` and `UnregisterRoot()` manage the set of roots that receive theme updates
- [ ] Derived spacing tokens are computed as multiples of `SpacingUnit`
- [ ] Derived radius tokens are computed as multiples of `BorderRadius`
- [ ] Heading font sizes respect `HeadingSizeMultiplier`
- [ ] `jui-tokens.uss` declares all `--jui-*` variables with default fallback values
- [ ] `jui-utilities.uss` provides utility classes for display, flex, padding, margin, gap, sizing, colors, typography, radius, borders, overflow, opacity, position, cursor, and transitions
- [ ] Utility classes reference `--jui-*` variables (not hardcoded values) for theme reactivity
- [ ] All public APIs have XML documentation
- [ ] Zero allocations on `Apply()` after initial setup (no string concatenation or list allocation per call)
