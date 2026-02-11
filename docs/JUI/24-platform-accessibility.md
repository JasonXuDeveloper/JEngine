# Section 24 — Platform, Mobile & Accessibility

## Overview

This section covers four systems that adapt JUI to different runtime environments and user needs:

1. **PlatformContext** — Detects the current platform (Desktop, Mobile, Console, TV), active input mode, and screen size category. All values are reactive signals, so UI can adapt dynamically when a user plugs in a gamepad or rotates a device.
2. **SafeArea** — Tracks device safe area insets (notches, rounded corners, system bars) and applies them to container elements so UI content stays within visible bounds.
3. **CursorManager** — Provides reactive cursor state management with support for custom cursors per element or context.
4. **UIAccessibility** — Exposes accessibility preferences (high contrast, reduced motion, font scale, screen reader) as reactive signals. Responsive USS classes are auto-applied to the root element so stylesheets can adapt without code. Semantic UXML attributes enable screen reader support.

Together, these systems ensure JUI applications work correctly across desktop, mobile, console, and TV platforms while meeting accessibility standards.

## Dependencies

| Section | What It Provides |
|---------|-----------------|
| 1 — Reactive Primitives | `Signal<T>`, `IReadOnlySignal<T>` for all reactive platform/accessibility state |
| 2 — Effect System | `Effect()` for reacting to platform/accessibility changes |
| 6 — Component Model | `JUIComponent` lifecycle for auto-applying USS classes |

## File Structure

```
Packages/com.jasonxudeveloper.jengine.ui/
└── Runtime/JUI/
    ├── Platform/
    │   ├── PlatformContext.cs       # Platform, input mode, screen size detection
    │   ├── SafeArea.cs              # Safe area tracking and application
    │   └── CursorManager.cs         # Reactive cursor type management
    └── Accessibility/
        └── UIAccessibility.cs       # Accessibility preferences and USS class injection
```

## API Design

### PlatformContext

```csharp
/// <summary>
/// Detects runtime platform, input mode, and screen size.
/// All values are reactive signals that update when conditions change.
/// </summary>
public static class PlatformContext
{
    /// <summary>
    /// The current platform category, detected at startup and updated if conditions change.
    /// </summary>
    public static Signal<UIPlatform> Current { get; }

    /// <summary>
    /// The current input mode. Updates when the user switches between mouse, keyboard,
    /// gamepad, or touch input. Shared with FocusNavigator.CurrentInputMode.
    /// </summary>
    public static Signal<InputMode> InputMode { get; }

    /// <summary>
    /// Current screen size category based on the narrowest dimension.
    /// Updates on window resize or device rotation.
    /// </summary>
    public static Signal<ScreenSize> ScreenCategory { get; }

    /// <summary>
    /// Breakpoint threshold for Small screens (below this width = Small).
    /// Default: 640 pixels.
    /// </summary>
    public static float SmallBreakpoint { get; set; } = 640f;

    /// <summary>
    /// Breakpoint threshold for Medium screens (Small to Medium = Medium).
    /// Default: 1024 pixels.
    /// </summary>
    public static float MediumBreakpoint { get; set; } = 1024f;

    /// <summary>
    /// Breakpoint threshold for Large screens (Medium to Large = Large, above = XLarge).
    /// Default: 1440 pixels.
    /// </summary>
    public static float LargeBreakpoint { get; set; } = 1440f;

    /// <summary>
    /// Current screen DPI. Useful for scaling calculations.
    /// </summary>
    public static IReadOnlySignal<float> ScreenDPI { get; }

    /// <summary>
    /// Whether the device is in portrait orientation (height > width).
    /// </summary>
    public static IReadOnlySignal<bool> IsPortrait { get; }
}
```

### Platform & Screen Enums

```csharp
public enum UIPlatform
{
    Desktop,    // Windows, macOS, Linux (standalone)
    Mobile,     // iOS, Android
    Console,    // PlayStation, Xbox, Switch
    TV          // tvOS, Android TV, Fire TV
}

public enum ScreenSize
{
    Small,      // < SmallBreakpoint (phones, small windows)
    Medium,     // SmallBreakpoint to MediumBreakpoint (tablets, medium windows)
    Large,      // MediumBreakpoint to LargeBreakpoint (desktops, laptops)
    XLarge      // > LargeBreakpoint (ultrawide, 4K)
}
```

### SafeArea

```csharp
/// <summary>
/// Tracks device safe area and provides methods to apply safe area insets to UI containers.
/// Handles notches, rounded corners, home indicators, and system status bars.
/// </summary>
public static class SafeArea
{
    /// <summary>
    /// The current safe area rect in screen coordinates.
    /// Updates when the safe area changes (rotation, system UI changes).
    /// </summary>
    public static Signal<Rect> Current { get; }

    /// <summary>
    /// Safe area expressed as edge insets (top, bottom, left, right) in pixels.
    /// </summary>
    public static Signal<EdgeInsets> Insets { get; }

    /// <summary>
    /// Whether the device has a display notch or camera cutout.
    /// </summary>
    public static Signal<bool> HasNotch { get; }

    /// <summary>
    /// Apply safe area padding to a container element.
    /// The element's padding is reactively updated when the safe area changes.
    /// </summary>
    public static void Apply(VisualElement container);

    /// <summary>
    /// Remove safe area padding from a container element.
    /// </summary>
    public static void Remove(VisualElement container);

    /// <summary>
    /// Apply safe area only to specific edges.
    /// Useful when a side panel only needs left/right insets but not top/bottom.
    /// </summary>
    public static void Apply(VisualElement container, Edge edges);
}

/// <summary>
/// Edge insets in pixels, representing the distance from each screen edge to the safe area.
/// </summary>
public struct EdgeInsets
{
    public float Top;
    public float Bottom;
    public float Left;
    public float Right;

    public float Horizontal => Left + Right;
    public float Vertical => Top + Bottom;
}

[Flags]
public enum Edge
{
    None   = 0,
    Top    = 1 << 0,
    Bottom = 1 << 1,
    Left   = 1 << 2,
    Right  = 1 << 3,
    All    = Top | Bottom | Left | Right
}
```

### CursorManager

```csharp
/// <summary>
/// Manages cursor appearance reactively.
/// Supports system cursors, custom texture cursors, and per-element cursor assignment.
/// </summary>
public static class CursorManager
{
    /// <summary>
    /// The current active cursor type.
    /// </summary>
    public static IReadOnlySignal<CursorType> CurrentCursor { get; }

    /// <summary>
    /// Set the cursor type. Reverts to Default when the element loses hover.
    /// </summary>
    public static void SetCursor(CursorType type);

    /// <summary>
    /// Set a custom texture cursor with hotspot offset.
    /// </summary>
    public static void SetCustomCursor(Texture2D texture, Vector2 hotspot);

    /// <summary>
    /// Assign a cursor type that activates on hover over the given element.
    /// </summary>
    public static void SetElementCursor(VisualElement el, CursorType type);

    /// <summary>
    /// Show or hide the hardware cursor.
    /// </summary>
    public static Signal<bool> Visible { get; }
}

public enum CursorType
{
    Default,
    Pointer,      // Hand cursor (clickable)
    Text,         // I-beam (text input)
    Crosshair,
    Move,
    ResizeNS,     // Vertical resize
    ResizeEW,     // Horizontal resize
    ResizeNESW,   // Diagonal resize
    ResizeNWSE,   // Diagonal resize
    NotAllowed,
    Wait,
    Grab,
    Grabbing,
    Custom
}
```

### UIAccessibility

```csharp
/// <summary>
/// Exposes user accessibility preferences as reactive signals.
/// Auto-applies USS classes to the UI root element for stylesheet-based adaptation.
/// </summary>
public static class UIAccessibility
{
    /// <summary>
    /// Whether high contrast mode is enabled.
    /// Reads from OS accessibility settings where available, otherwise manual toggle.
    /// When true, USS class "jui-high-contrast" is applied to the root.
    /// </summary>
    public static Signal<bool> HighContrastMode { get; }

    /// <summary>
    /// Whether reduced motion is preferred.
    /// Reads from OS "prefers-reduced-motion" where available.
    /// When true, USS class "jui-reduced-motion" is applied to the root.
    /// Animations should check this and use instant transitions instead.
    /// </summary>
    public static Signal<bool> ReducedMotion { get; }

    /// <summary>
    /// Global font scale multiplier.
    /// Default is 1.0. Reads from OS text size settings where available.
    /// USS variable --jui-font-scale is set on the root.
    /// </summary>
    public static Signal<float> FontScale { get; }

    /// <summary>
    /// Whether a screen reader (VoiceOver, TalkBack, Narrator) is active.
    /// When true, additional ARIA-like attributes are processed on elements.
    /// </summary>
    public static Signal<bool> ScreenReaderActive { get; }

    /// <summary>
    /// Set a semantic label on an element for screen reader announcement.
    /// Equivalent to UXML attribute jui-label="...".
    /// </summary>
    public static void SetLabel(VisualElement el, string label);

    /// <summary>
    /// Set a semantic role on an element (button, checkbox, slider, etc.).
    /// Equivalent to UXML attribute jui-role="...".
    /// </summary>
    public static void SetRole(VisualElement el, string role);

    /// <summary>
    /// Mark an element as a live region for dynamic content announcements.
    /// Equivalent to UXML attribute jui-live="polite|assertive".
    /// </summary>
    public static void SetLiveRegion(VisualElement el, LiveRegionMode mode);

    /// <summary>
    /// Announce a message to the screen reader without associating it with an element.
    /// </summary>
    public static void Announce(string message, LiveRegionMode mode = LiveRegionMode.Polite);
}

public enum LiveRegionMode
{
    /// <summary>Updates announced when user is idle.</summary>
    Polite,
    /// <summary>Updates announced immediately, interrupting current speech.</summary>
    Assertive
}
```

## Data Structures

### Platform Detection Logic

```
UIPlatform detection (evaluated at startup):

  #if UNITY_IOS || UNITY_ANDROID
    -> Mobile (unless SystemInfo indicates TV form factor)
  #elif UNITY_PS4 || UNITY_PS5 || UNITY_XBOXONE || UNITY_GAMECORE_XBOXSERIES || UNITY_SWITCH
    -> Console
  #elif UNITY_TVOS
    -> TV
  #else
    -> Desktop
  #endif

  Android TV / Fire TV detection:
    if (UNITY_ANDROID && Screen.width > 1280 && !Input.touchSupported)
      -> TV
```

### Screen Size Evaluation

```
On every frame (or window resize callback):

  float narrow = Mathf.Min(Screen.width, Screen.height);

  if (narrow < SmallBreakpoint)       -> Small
  else if (narrow < MediumBreakpoint) -> Medium
  else if (narrow < LargeBreakpoint)  -> Large
  else                                -> XLarge
```

### Safe Area Polling

```
Polled each frame (mobile) or on resize (desktop):

  Rect safeArea = Screen.safeArea;
  EdgeInsets insets = new EdgeInsets
  {
      Top    = safeArea.y,
      Bottom = Screen.height - (safeArea.y + safeArea.height),
      Left   = safeArea.x,
      Right  = Screen.width - (safeArea.x + safeArea.width)
  };
```

### USS Class Mapping

```
Applied to root VisualElement automatically:

  PlatformContext.Current  -> "jui-platform-desktop" | "jui-platform-mobile" | "jui-platform-console" | "jui-platform-tv"
  PlatformContext.InputMode -> "jui-input-mouse" | "jui-input-keyboard" | "jui-input-gamepad" | "jui-input-touch"
  ScreenCategory           -> "jui-screen-small" | "jui-screen-medium" | "jui-screen-large" | "jui-screen-xlarge"
  HighContrastMode         -> "jui-high-contrast" (added/removed)
  ReducedMotion            -> "jui-reduced-motion" (added/removed)
  IsPortrait               -> "jui-portrait" (added/removed)
```

## Implementation Notes

### Automatic USS Class Application

On initialization, the system subscribes to all platform/accessibility signals and updates USS classes on the root `VisualElement`:

```csharp
internal static void Initialize(VisualElement root)
{
    Effect(() =>
    {
        // Platform class
        root.EnableInClassList("jui-platform-desktop", PlatformContext.Current.Value == UIPlatform.Desktop);
        root.EnableInClassList("jui-platform-mobile", PlatformContext.Current.Value == UIPlatform.Mobile);
        root.EnableInClassList("jui-platform-console", PlatformContext.Current.Value == UIPlatform.Console);
        root.EnableInClassList("jui-platform-tv", PlatformContext.Current.Value == UIPlatform.TV);
    });

    Effect(() =>
    {
        // Input mode class
        root.EnableInClassList("jui-input-mouse", PlatformContext.InputMode.Value == InputMode.Mouse);
        root.EnableInClassList("jui-input-keyboard", PlatformContext.InputMode.Value == InputMode.Keyboard);
        root.EnableInClassList("jui-input-gamepad", PlatformContext.InputMode.Value == InputMode.Gamepad);
        root.EnableInClassList("jui-input-touch", PlatformContext.InputMode.Value == InputMode.Touch);
    });

    Effect(() =>
    {
        // Screen size class
        root.EnableInClassList("jui-screen-small", PlatformContext.ScreenCategory.Value == ScreenSize.Small);
        root.EnableInClassList("jui-screen-medium", PlatformContext.ScreenCategory.Value == ScreenSize.Medium);
        root.EnableInClassList("jui-screen-large", PlatformContext.ScreenCategory.Value == ScreenSize.Large);
        root.EnableInClassList("jui-screen-xlarge", PlatformContext.ScreenCategory.Value == ScreenSize.XLarge);
    });

    Effect(() =>
    {
        root.EnableInClassList("jui-high-contrast", UIAccessibility.HighContrastMode.Value);
        root.EnableInClassList("jui-reduced-motion", UIAccessibility.ReducedMotion.Value);
    });

    Effect(() =>
    {
        root.style.SetVariable("--jui-font-scale", UIAccessibility.FontScale.Value);
    });
}
```

### Safe Area Application

`SafeArea.Apply()` subscribes to the `Insets` signal and reactively updates the container's padding:

```csharp
public static void Apply(VisualElement container)
{
    Effect(() =>
    {
        var insets = Insets.Value;
        container.style.paddingTop = insets.Top;
        container.style.paddingBottom = insets.Bottom;
        container.style.paddingLeft = insets.Left;
        container.style.paddingRight = insets.Right;
    });
}

public static void Apply(VisualElement container, Edge edges)
{
    Effect(() =>
    {
        var insets = Insets.Value;
        if ((edges & Edge.Top) != 0) container.style.paddingTop = insets.Top;
        if ((edges & Edge.Bottom) != 0) container.style.paddingBottom = insets.Bottom;
        if ((edges & Edge.Left) != 0) container.style.paddingLeft = insets.Left;
        if ((edges & Edge.Right) != 0) container.style.paddingRight = insets.Right;
    });
}
```

### Reduced Motion Integration

Components should check `UIAccessibility.ReducedMotion` before running animations:

```csharp
if (UIAccessibility.ReducedMotion.Value)
{
    // Skip animation, apply final state instantly
    element.style.opacity = 1f;
}
else
{
    await AnimateSignal(_opacity, 0f, 1f, 0.3f, Easing.EaseOut);
}
```

The `jui-reduced-motion` USS class allows stylesheet-based alternatives:

```css
.my-element {
    transition-duration: 300ms;
}

.jui-reduced-motion .my-element {
    transition-duration: 0ms;
}
```

### Screen Reader Semantic Attributes

UXML elements can declare semantic attributes for accessibility:

```xml
<ui:Button jui-label="Save settings" jui-role="button" />
<ui:Label jui-live="polite" text="3 items in cart" />
<ui:VisualElement jui-role="dialog" jui-label="Confirm delete">
    ...
</ui:VisualElement>
```

These attributes are processed by `UIAccessibility` when `ScreenReaderActive` is true, providing information to the platform's native accessibility API.

### OS Accessibility Setting Detection

Where supported, JUI reads OS-level accessibility settings:

| Setting | iOS | Android | Windows | macOS |
|---------|-----|---------|---------|-------|
| High Contrast | UIAccessibility.isGrayscaleEnabled | AccessibilityManager | SystemParametersInfo(SPI_GETHIGHCONTRAST) | NSWorkspace.accessibilityDisplayShouldIncreaseContrast |
| Reduced Motion | UIAccessibility.isReduceMotionEnabled | Settings.Global.ANIMATOR_DURATION_SCALE | SystemParametersInfo(SPI_GETCLIENTAREAANIMATION) | NSWorkspace.accessibilityDisplayShouldReduceMotion |
| Font Scale | UIApplication.preferredContentSizeCategory | Configuration.fontScale | SystemParametersInfo(SPI_GETLOGFONT) | NSApplication.preferredContentSizeCategory |
| Screen Reader | UIAccessibility.isVoiceOverRunning | AccessibilityManager.isTouchExplorationEnabled | SystemParametersInfo(SPI_GETSCREENREADER) | NSWorkspace.isVoiceOverEnabled |

On platforms where native APIs are not accessible, the signals default to `false` / `1.0f` and can be set manually.

## Source Generator Notes

No source generator processing is required for this section. All APIs are runtime-only with reactive Signal-based state management. Platform and accessibility USS classes are applied automatically without attribute-driven code generation.

However, the `jui-label`, `jui-role`, and `jui-live` UXML attributes are processed at runtime by `UIAccessibility` when elements are attached to the panel. This does not involve source generation.

## Usage Examples

### Responsive Layout with USS

```css
/* Default: desktop layout */
.main-content {
    flex-direction: row;
    padding: 24px;
}

/* Mobile: stack vertically, reduce padding */
.jui-platform-mobile .main-content,
.jui-screen-small .main-content {
    flex-direction: column;
    padding: 12px;
}

/* Gamepad: larger hit targets */
.jui-input-gamepad .nav-button {
    min-height: 64px;
    font-size: 20px;
}

/* High contrast: stronger borders */
.jui-high-contrast .card {
    border-width: 2px;
    border-color: white;
}

/* Large screens: multi-column */
.jui-screen-xlarge .grid {
    flex-wrap: wrap;
    flex-direction: row;
}
.jui-screen-xlarge .grid > .item {
    width: 25%;
}
```

### Safe Area for Mobile

```csharp
public partial class GameUI : JUIComponent
{
    protected override void OnMount()
    {
        // Apply full safe area to the main container
        SafeArea.Apply(El.MainContainer);

        // Only apply bottom inset to the action bar (for home indicator)
        SafeArea.Apply(El.ActionBar, Edge.Bottom);

        // React to notch presence
        Effect(() =>
        {
            El.StatusBar.style.display =
                SafeArea.HasNotch.Value
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
        });
    }
}
```

### Adaptive UI Based on Platform

```csharp
public partial class ControlsHint : JUIComponent
{
    protected override void OnMount()
    {
        Effect(() =>
        {
            switch (PlatformContext.InputMode.Value)
            {
                case InputMode.Keyboard:
                    El.HintText.text = "Press Enter to confirm";
                    break;
                case InputMode.Gamepad:
                    El.HintText.text = "Press A to confirm";
                    break;
                case InputMode.Touch:
                    El.HintText.text = "Tap to confirm";
                    break;
                default:
                    El.HintText.text = "Click to confirm";
                    break;
            }
        });
    }
}
```

### Accessible Dialog

```csharp
public partial class DeleteConfirmDialog : JUIComponent
{
    protected override void OnMount()
    {
        // Set semantic attributes
        UIAccessibility.SetRole(El.DialogRoot, "dialog");
        UIAccessibility.SetLabel(El.DialogRoot, "Confirm deletion");
        UIAccessibility.SetLabel(El.DeleteButton, "Delete item permanently");
        UIAccessibility.SetLabel(El.CancelButton, "Cancel and go back");

        // Live region for status updates
        UIAccessibility.SetLiveRegion(El.StatusLabel, LiveRegionMode.Polite);
    }

    private void OnDeleteComplete()
    {
        // Screen reader announces this change
        El.StatusLabel.text = "Item deleted successfully";
    }
}
```

### Font Scaling

```csharp
public partial class ReadingView : JUIComponent
{
    protected override void OnMount()
    {
        // React to font scale changes
        Effect(() =>
        {
            float scale = UIAccessibility.FontScale.Value;
            El.ContentText.style.fontSize = 16f * scale;
            El.HeadingText.style.fontSize = 24f * scale;
        });
    }
}
```

Or via USS using the CSS variable:

```css
.body-text {
    font-size: calc(16px * var(--jui-font-scale, 1));
}

.heading-text {
    font-size: calc(24px * var(--jui-font-scale, 1));
}
```

### Reduced Motion Check

```csharp
public partial class SplashScreen : JUIComponent
{
    protected override async UniTask PlayIntro()
    {
        if (UIAccessibility.ReducedMotion.Value)
        {
            // Skip animation entirely
            El.Logo.style.opacity = 1f;
            El.Title.style.opacity = 1f;
            return;
        }

        // Full animation sequence
        await AnimateSignal(_logoOpacity, 0f, 1f, 0.5f, Easing.EaseOut);
        await UniTask.Delay(200);
        await AnimateSignal(_titleOpacity, 0f, 1f, 0.3f, Easing.EaseOut);
    }
}
```

### Custom Cursor per Element

```csharp
public partial class EditorCanvas : JUIComponent
{
    protected override void OnMount()
    {
        CursorManager.SetElementCursor(El.Canvas, CursorType.Crosshair);
        CursorManager.SetElementCursor(El.ResizeHandle, CursorType.ResizeNWSE);
        CursorManager.SetElementCursor(El.DragArea, CursorType.Grab);
    }
}
```

## Test Plan

| # | Test Case | Expectation |
|---|-----------|-------------|
| 1 | `PlatformContext.Current` on standalone build | Returns `Desktop` |
| 2 | `PlatformContext.Current` on iOS | Returns `Mobile` |
| 3 | `ScreenCategory` at 600px width | Returns `Small` (below 640 default breakpoint) |
| 4 | `ScreenCategory` at 800px width | Returns `Medium` |
| 5 | `ScreenCategory` at 1200px width | Returns `Large` |
| 6 | `ScreenCategory` at 1600px width | Returns `XLarge` |
| 7 | Custom breakpoints set, then resize | `ScreenCategory` uses custom thresholds |
| 8 | `SafeArea.Apply(container)` on notched device | Padding matches `Screen.safeArea` insets |
| 9 | `SafeArea.Apply(container, Edge.Bottom)` | Only bottom padding applied |
| 10 | `SafeArea.Remove(container)` | All safe area padding removed |
| 11 | `SafeArea.HasNotch` on iPhone | Returns `true` |
| 12 | `SafeArea.HasNotch` on non-notched device | Returns `false` |
| 13 | Root element USS classes on Desktop + Mouse | Has `jui-platform-desktop` and `jui-input-mouse` |
| 14 | Switch to gamepad input | `jui-input-mouse` removed, `jui-input-gamepad` added |
| 15 | `HighContrastMode.Value = true` | `jui-high-contrast` class added to root |
| 16 | `ReducedMotion.Value = true` | `jui-reduced-motion` class added to root |
| 17 | `FontScale.Value = 1.5f` | `--jui-font-scale` CSS variable set to `1.5` |
| 18 | `SetLabel(el, "Save")` | Element has `jui-label` attribute with value "Save" |
| 19 | `SetRole(el, "button")` | Element has `jui-role` attribute with value "button" |
| 20 | `SetLiveRegion(el, Assertive)` | Element has `jui-live` attribute with value "assertive" |
| 21 | `Announce("Done")` with screen reader active | Platform screen reader receives announcement |
| 22 | `CursorManager.SetElementCursor(el, Pointer)` on hover | Cursor changes to pointer hand |
| 23 | `CursorManager.Visible.Value = false` | Hardware cursor hidden |
| 24 | `IsPortrait` on portrait device | Returns `true` |
| 25 | Window resize from 800px to 500px | `ScreenCategory` transitions Small, USS classes update |

## Acceptance Criteria

- [ ] `PlatformContext.Current` correctly detects Desktop, Mobile, Console, and TV platforms at runtime
- [ ] `PlatformContext.InputMode` transitions reactively between Mouse, Keyboard, Gamepad, and Touch based on input events
- [ ] `PlatformContext.ScreenCategory` updates reactively on window resize or device rotation using configurable breakpoints
- [ ] `SafeArea.Current` and `SafeArea.Insets` track the device safe area reactively
- [ ] `SafeArea.Apply()` sets reactive padding on containers; `SafeArea.Apply(el, edges)` supports partial edge application
- [ ] `SafeArea.HasNotch` correctly detects notched devices
- [ ] `CursorManager` supports system cursor types, custom texture cursors, and per-element cursor assignment
- [ ] `UIAccessibility.HighContrastMode`, `ReducedMotion`, `FontScale`, and `ScreenReaderActive` are reactive signals
- [ ] Where supported, accessibility signals read from OS-level accessibility settings automatically
- [ ] All responsive USS classes (`jui-platform-*`, `jui-input-*`, `jui-screen-*`, `jui-high-contrast`, `jui-reduced-motion`, `jui-portrait`) are auto-applied to the root element
- [ ] `--jui-font-scale` CSS variable is set on the root and updates when `FontScale` changes
- [ ] `SetLabel`, `SetRole`, and `SetLiveRegion` apply semantic attributes for screen reader support
- [ ] `Announce()` delivers messages to the platform screen reader when active
- [ ] UXML attributes `jui-label`, `jui-role`, and `jui-live` are processed at runtime for accessibility
- [ ] All platform and accessibility state is cleaned up properly and does not leak subscriptions
