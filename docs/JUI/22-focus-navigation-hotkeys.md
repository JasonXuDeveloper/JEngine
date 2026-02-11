# Section 22 — Focus, Navigation, Input & Hotkeys

## Overview

This section covers four interconnected systems that manage how users interact with JUI beyond pointer input:

1. **FocusNavigator** — Tracks the currently focused element as a reactive `Signal<VisualElement>`, detects input mode transitions (mouse, keyboard, gamepad, touch), and provides directional focus movement.
2. **NavMap** — Declares explicit navigation relationships between elements (up/down/left/right neighbors) or enables automatic spatial navigation within a container.
3. **JUIInput** — Adapts Unity's Input System into JUI's reactive model, translating `InputAction` events into Signal updates and providing a unified abstraction over keyboard, mouse, gamepad, and touch.
4. **HotkeyManager** — Manages context-scoped keyboard and gamepad shortcuts. The `[Hotkey]` attribute enables declarative registration that auto-binds on mount and auto-disposes on unmount. Hotkey contexts integrate with ScreenRouter so pushing/popping screens automatically switches active shortcut sets.

## Dependencies

| Section | What It Provides |
|---------|-----------------|
| 1 — Reactive Primitives | `Signal<T>`, `IReadOnlySignal<T>` for focus state and input mode |
| 2 — Effect System | `Effect()` for reacting to focus/input changes |
| 6 — Component Model | `JUIComponent` mount/unmount lifecycle for auto-registration |
| 9 — Generator Setup | Source generator processes `[Hotkey]` attributes |
| 14 — ScreenRouter | (Optional) Push/pop triggers hotkey context switches |

## File Structure

```
Packages/com.jasonxudeveloper.jengine.ui/
└── Runtime/JUI/Navigation/
    ├── FocusNavigator.cs       # Focus tracking, direction movement, focus traps
    ├── NavMap.cs               # Explicit & spatial neighbor declarations
    ├── JUIInput.cs             # Unity Input System adapter
    ├── HotkeyManager.cs        # Context-scoped shortcut registration
    ├── HotkeyBinding.cs        # Single hotkey binding data
    ├── HotkeyMap.cs            # Named collection of bindings for a context
    └── Attributes/
        └── HotkeyAttribute.cs  # Declarative hotkey registration attribute
```

## API Design

### FocusNavigator

```csharp
/// <summary>
/// Reactive focus management for UI Toolkit elements.
/// Tracks the focused element and input mode, provides directional navigation.
/// </summary>
public static class FocusNavigator
{
    /// <summary>
    /// The currently focused VisualElement. Null when no element has focus.
    /// </summary>
    public static Signal<VisualElement> FocusedElement { get; }

    /// <summary>
    /// Current input mode, updated automatically based on last input event.
    /// </summary>
    public static Signal<InputMode> CurrentInputMode { get; }

    /// <summary>
    /// Move focus in the given direction using NavMap neighbors or spatial lookup.
    /// </summary>
    public static void MoveFocus(Direction dir);

    /// <summary>
    /// Explicitly set focus to the given element.
    /// </summary>
    public static void SetFocus(VisualElement el);

    /// <summary>
    /// Trap focus within a container. Tab/direction navigation wraps within bounds.
    /// Used for modals, dropdowns, and other overlays.
    /// </summary>
    public static void TrapFocus(VisualElement container);

    /// <summary>
    /// Release the current focus trap, restoring previous navigation scope.
    /// </summary>
    public static void ReleaseTrap();

    /// <summary>
    /// Called internally each frame to process pending focus changes and input mode detection.
    /// </summary>
    internal static void Tick();
}
```

### Direction & InputMode Enums

```csharp
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public enum InputMode
{
    Mouse,
    Keyboard,
    Gamepad,
    Touch
}
```

### NavMap

```csharp
/// <summary>
/// Declares navigation relationships between elements for directional focus movement.
/// </summary>
public static class NavMap
{
    /// <summary>
    /// Set explicit directional neighbors for an element.
    /// Null parameters leave that direction using spatial fallback.
    /// </summary>
    public static void SetNeighbors(
        VisualElement el,
        VisualElement up = null,
        VisualElement down = null,
        VisualElement left = null,
        VisualElement right = null);

    /// <summary>
    /// Remove all neighbor declarations for an element.
    /// </summary>
    public static void ClearNeighbors(VisualElement el);

    /// <summary>
    /// Enable automatic spatial navigation within a container.
    /// Focusable children are navigated based on their layout positions.
    /// </summary>
    public static void SetSpatialContainer(VisualElement container);

    /// <summary>
    /// Disable spatial navigation for a container.
    /// </summary>
    public static void ClearSpatialContainer(VisualElement container);

    /// <summary>
    /// Resolve the neighbor in the given direction for the given element.
    /// Checks explicit neighbors first, then spatial containers, then returns null.
    /// </summary>
    internal static VisualElement Resolve(VisualElement from, Direction dir);
}
```

### JUIInput

```csharp
/// <summary>
/// Adapts Unity Input System actions into JUI's reactive Signal model.
/// </summary>
public static class JUIInput
{
    /// <summary>
    /// Create a reactive signal that tracks an InputAction's value.
    /// The signal updates when the action is performed and resets when cancelled.
    /// </summary>
    public static IReadOnlySignal<T> ActionSignal<T>(InputActionReference action)
        where T : struct;

    /// <summary>
    /// Create a reactive signal that is true while the action is held.
    /// </summary>
    public static IReadOnlySignal<bool> ActionHeld(InputActionReference action);

    /// <summary>
    /// Create a reactive signal that pulses true for one frame on action trigger.
    /// </summary>
    public static IReadOnlySignal<bool> ActionTriggered(InputActionReference action);

    /// <summary>
    /// The navigation direction vector from the current input device (WASD, D-pad, left stick).
    /// </summary>
    public static IReadOnlySignal<Vector2> NavigationVector { get; }

    /// <summary>
    /// Submit/confirm action (Enter, A button, etc.).
    /// </summary>
    public static IReadOnlySignal<bool> Submit { get; }

    /// <summary>
    /// Cancel/back action (Escape, B button, etc.).
    /// </summary>
    public static IReadOnlySignal<bool> Cancel { get; }
}
```

### HotkeyManager

```csharp
/// <summary>
/// Manages context-scoped keyboard and gamepad shortcuts.
/// Contexts allow different screens/modes to have independent shortcut sets.
/// </summary>
public static class HotkeyManager
{
    /// <summary>
    /// Register a hotkey binding for the given input action.
    /// The callback is invoked only when the specified context is active.
    /// </summary>
    /// <typeparam name="TState">State object passed to the callback.</typeparam>
    public static void Register<TState>(
        InputActionReference action,
        Action<TState> callback,
        TState state,
        string context = null);

    /// <summary>
    /// Register a hotkey binding without state.
    /// </summary>
    public static void Register(
        InputActionReference action,
        Action callback,
        string context = null);

    /// <summary>
    /// Unregister a previously registered hotkey.
    /// </summary>
    public static void Unregister(InputActionReference action);

    /// <summary>
    /// Set the active hotkey context. Only bindings in this context (and global bindings) fire.
    /// </summary>
    public static void SetContext(string context);

    /// <summary>
    /// The current active context name.
    /// </summary>
    public static IReadOnlySignal<string> CurrentContext { get; }

    /// <summary>
    /// Master enable/disable for all hotkey processing.
    /// </summary>
    public static Signal<bool> Enabled { get; }

    /// <summary>
    /// Get the display string for a hotkey's current binding (e.g., "Ctrl+S", "LB").
    /// Useful for showing shortcut hints in UI.
    /// </summary>
    public static string GetBindingDisplayString(InputActionReference action);

    /// <summary>
    /// Get all registered bindings for the given context (or global if null).
    /// </summary>
    public static IReadOnlyList<HotkeyBinding> GetBindings(string context = null);
}
```

### HotkeyBinding

```csharp
/// <summary>
/// Data class representing a single hotkey registration.
/// </summary>
public sealed class HotkeyBinding
{
    public InputActionReference Action { get; }
    public string Context { get; }
    public string DisplayName { get; }
    public string DisplayBinding { get; }
    public bool IsGlobal => Context == null;
}
```

### HotkeyMap

```csharp
/// <summary>
/// A named, serializable collection of hotkey bindings for a context.
/// Can be defined as a ScriptableObject asset for designer-friendly configuration.
/// </summary>
[CreateAssetMenu(menuName = "JUI/Hotkey Map")]
public class HotkeyMap : ScriptableObject
{
    public string ContextName;
    public List<HotkeyMapEntry> Entries;
}

[Serializable]
public struct HotkeyMapEntry
{
    public InputActionReference Action;
    public string DisplayName;
}
```

### HotkeyAttribute

```csharp
/// <summary>
/// Declaratively register a method as a hotkey handler.
/// Auto-registers on mount, auto-disposes on unmount.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class HotkeyAttribute : Attribute
{
    /// <summary>
    /// The name of the InputActionReference field on this component.
    /// </summary>
    public string ActionField { get; }

    /// <summary>
    /// The hotkey context. Null means global (always active).
    /// </summary>
    public string Context { get; set; }

    public HotkeyAttribute(string actionField) { }
}
```

## Data Structures

### Focus Trap Stack

```
Stack<VisualElement> _trapStack
  - TrapFocus pushes container onto stack
  - ReleaseTrap pops and restores previous scope
  - MoveFocus checks top of stack for containment boundary
  - Empty stack = unrestricted navigation
```

### NavMap Internal Storage

```
Dictionary<VisualElement, NavNeighbors> _explicitNeighbors
  NavNeighbors: { Up, Down, Left, Right } — nullable VisualElement refs

HashSet<VisualElement> _spatialContainers
  - Containers registered for automatic spatial navigation
  - On Resolve: find focusable children, pick nearest in direction
```

### Hotkey Registry

```
Dictionary<string, List<HotkeyRegistration>> _contextBindings
  Key: context name (null key = global)
  Value: list of (InputActionReference, Delegate, state) tuples

string _activeContext
  - SetContext updates this
  - On input: check _activeContext bindings first, then global
```

## Implementation Notes

### Input Mode Detection

`FocusNavigator.Tick()` inspects the most recent input event type each frame:

- **Mouse**: Any `PointerMoveEvent` or `PointerDownEvent` with `pointerId == 0`
- **Keyboard**: Any `KeyDownEvent`
- **Gamepad**: Any input from `Gamepad.current`
- **Touch**: Any `PointerDownEvent` with `pointerId > 0` or `TouchPhase` events

Mode transitions update `CurrentInputMode` and toggle visual focus indicators (focus rings appear in Keyboard/Gamepad mode, hidden in Mouse/Touch mode).

### Spatial Navigation Algorithm

When `NavMap.Resolve()` falls through to spatial lookup:

1. Get the focused element's world-space bounding rect.
2. Collect all focusable elements within the spatial container.
3. Filter to elements in the correct half-plane for the direction (e.g., for `Direction.Right`, only elements whose center X > current center X).
4. Score candidates by angular proximity to the ideal direction vector.
5. Among ties, prefer the closest element by distance.

```csharp
internal static VisualElement SpatialResolve(VisualElement from, Direction dir, VisualElement container)
{
    var fromRect = from.worldBound;
    var fromCenter = fromRect.center;
    var dirVector = DirectionToVector(dir);

    VisualElement best = null;
    float bestScore = float.MaxValue;

    foreach (var candidate in GetFocusableChildren(container))
    {
        if (candidate == from) continue;
        var candidateCenter = candidate.worldBound.center;
        var delta = candidateCenter - fromCenter;

        // Must be in correct half-plane
        if (Vector2.Dot(delta.normalized, dirVector) < 0.2f) continue;

        // Score: angular deviation + distance penalty
        float angle = Vector2.Angle(delta, dirVector);
        float dist = delta.magnitude;
        float score = angle + dist * 0.01f;

        if (score < bestScore) { bestScore = score; best = candidate; }
    }
    return best;
}
```

### ScreenRouter Integration

When ScreenRouter pushes or pops a screen, it calls `HotkeyManager.SetContext()` with the new screen's context name. This enables patterns such as:

- **Main Menu** screen uses context `"main-menu"` with shortcuts for New Game, Load, Settings.
- **Gameplay HUD** screen uses context `"gameplay"` with shortcuts for Inventory, Map, Pause.
- **Modal Dialog** pushes context `"dialog"` which suppresses gameplay shortcuts.

```csharp
// Inside ScreenRouter (simplified)
public void Push(IScreen screen)
{
    _screenStack.Push(screen);
    HotkeyManager.SetContext(screen.HotkeyContext);
    FocusNavigator.TrapFocus(screen.RootElement);
}

public void Pop()
{
    var removed = _screenStack.Pop();
    FocusNavigator.ReleaseTrap();
    var current = _screenStack.Peek();
    HotkeyManager.SetContext(current.HotkeyContext);
}
```

### Focus Trap Behavior

When a focus trap is active:

- `MoveFocus()` wraps at container boundaries (last element -> first element).
- `Tab`/`Shift+Tab` cycles within the container.
- Clicking outside the container does not move focus (click is consumed).
- Multiple traps stack: inner trap (e.g., dropdown inside modal) takes priority.

## Source Generator Notes

The source generator processes `[Hotkey]` attributes on methods within `JUIComponent` subclasses and emits:

1. **Registration in `OnMount()`**: Calls `HotkeyManager.Register()` with the method reference and specified context.
2. **Unregistration in `OnUnmount()`**: Calls `HotkeyManager.Unregister()` for each registered action.
3. **Compile-time validation**:
   - Verifies the `ActionField` references an `InputActionReference` field on the component.
   - Verifies the method signature matches `Action` or `Action<T>` for stateful variants.
   - Warns if duplicate actions are registered in the same context.

Generated code pattern:

```csharp
// Generated in partial class
partial void OnMount_Hotkeys()
{
    HotkeyManager.Register(_confirmAction, OnConfirm, context: "menu");
    HotkeyManager.Register(_cancelAction, OnCancel, context: "menu");
}

partial void OnUnmount_Hotkeys()
{
    HotkeyManager.Unregister(_confirmAction);
    HotkeyManager.Unregister(_cancelAction);
}
```

## Usage Examples

### Basic Keyboard Navigation

```csharp
public partial class MainMenu : JUIComponent
{
    protected override void OnMount()
    {
        // Enable spatial navigation within the button container
        NavMap.SetSpatialContainer(El.ButtonList);

        // Set initial focus
        FocusNavigator.SetFocus(El.NewGameButton);

        // React to focus changes
        Effect(() =>
        {
            var focused = FocusNavigator.FocusedElement.Value;
            // Update selection indicator position
            if (focused != null)
                El.SelectionArrow.style.top = focused.worldBound.y;
        });
    }
}
```

### Explicit Navigation Grid

```csharp
public partial class InventoryGrid : JUIComponent
{
    protected override void OnMount()
    {
        // 3x3 grid with explicit wrap-around
        var slots = new[] {
            El.Slot0, El.Slot1, El.Slot2,
            El.Slot3, El.Slot4, El.Slot5,
            El.Slot6, El.Slot7, El.Slot8
        };

        for (int i = 0; i < 9; i++)
        {
            NavMap.SetNeighbors(slots[i],
                up:    slots[(i - 3 + 9) % 9],
                down:  slots[(i + 3) % 9],
                left:  slots[i % 3 == 0 ? i + 2 : i - 1],
                right: slots[i % 3 == 2 ? i - 2 : i + 1]);
        }

        FocusNavigator.SetFocus(slots[0]);
    }
}
```

### Declarative Hotkeys

```csharp
public partial class GameplayHUD : JUIComponent
{
    [SerializeField] private InputActionReference _inventoryAction;
    [SerializeField] private InputActionReference _mapAction;
    [SerializeField] private InputActionReference _pauseAction;

    [Hotkey(nameof(_inventoryAction), Context = "gameplay")]
    private void OnOpenInventory()
    {
        ScreenRouter.Push<InventoryScreen>();
    }

    [Hotkey(nameof(_mapAction), Context = "gameplay")]
    private void OnOpenMap()
    {
        ScreenRouter.Push<MapScreen>();
    }

    [Hotkey(nameof(_pauseAction), Context = "gameplay")]
    private void OnPause()
    {
        ScreenRouter.Push<PauseMenu>();
    }
}
```

### Modal with Focus Trap

```csharp
public partial class ConfirmDialog : JUIComponent
{
    protected override void OnMount()
    {
        // Trap focus within the dialog
        FocusNavigator.TrapFocus(El.DialogContainer);
        FocusNavigator.SetFocus(El.ConfirmButton);

        // Explicit two-button navigation
        NavMap.SetNeighbors(El.ConfirmButton, right: El.CancelButton);
        NavMap.SetNeighbors(El.CancelButton, left: El.ConfirmButton);
    }

    protected override void OnUnmount()
    {
        FocusNavigator.ReleaseTrap();
    }
}
```

### Showing Hotkey Hints in UI

```csharp
public partial class ActionBar : JUIComponent
{
    [SerializeField] private InputActionReference _interactAction;

    protected override void OnMount()
    {
        // Display current binding (adapts to input device)
        Effect(() =>
        {
            var mode = FocusNavigator.CurrentInputMode.Value;
            var hint = HotkeyManager.GetBindingDisplayString(_interactAction);
            El.InteractHint.text = $"[{hint}] Interact";
        });
    }
}
```

## Test Plan

| # | Test Case | Expectation |
|---|-----------|-------------|
| 1 | `SetFocus(el)` | `FocusedElement.Value` equals `el` |
| 2 | `SetFocus(null)` | `FocusedElement.Value` is null, no errors |
| 3 | `MoveFocus(Right)` with explicit neighbor | Focus moves to declared right neighbor |
| 4 | `MoveFocus(Right)` with spatial container | Focus moves to nearest element to the right |
| 5 | `MoveFocus(Right)` at boundary without neighbor | Focus does not change |
| 6 | `TrapFocus(container)` then `MoveFocus` past boundary | Focus wraps within container |
| 7 | `TrapFocus` then `ReleaseTrap` | Navigation scope restored to previous |
| 8 | Nested `TrapFocus` (modal inside modal) | Inner trap takes priority; release restores outer |
| 9 | Keyboard input detected | `CurrentInputMode.Value` == `Keyboard` |
| 10 | Gamepad input after keyboard | `CurrentInputMode.Value` transitions to `Gamepad` |
| 11 | `HotkeyManager.Register` in context "menu" | Callback fires when context is "menu" and action triggered |
| 12 | `HotkeyManager.Register` in context "menu", active context is "game" | Callback does NOT fire |
| 13 | Global hotkey (context = null) | Fires regardless of active context |
| 14 | `SetContext("game")` | Only "game" and global bindings active |
| 15 | `Enabled.Value = false` | No hotkey callbacks fire |
| 16 | `[Hotkey]` on mount | `HotkeyManager.Register` called by generated code |
| 17 | `[Hotkey]` on unmount | `HotkeyManager.Unregister` called by generated code |
| 18 | `GetBindingDisplayString` for keyboard | Returns string like "Ctrl+S" |
| 19 | `GetBindingDisplayString` for gamepad | Returns string like "LB" |
| 20 | ScreenRouter push triggers `SetContext` | Active hotkey context matches pushed screen |

## Acceptance Criteria

- [ ] `FocusNavigator.FocusedElement` is a reactive `Signal<VisualElement>` that updates on focus change
- [ ] `FocusNavigator.CurrentInputMode` detects and transitions between Mouse, Keyboard, Gamepad, and Touch
- [ ] `MoveFocus(Direction)` resolves targets via explicit `NavMap` neighbors first, then spatial fallback
- [ ] `TrapFocus` constrains navigation within a container with wrap-around; traps stack correctly
- [ ] `NavMap.SetSpatialContainer` enables automatic nearest-neighbor spatial navigation for focusable children
- [ ] `JUIInput` wraps Unity Input System actions as reactive signals (`ActionSignal<T>`, `ActionHeld`, `ActionTriggered`)
- [ ] `HotkeyManager` supports context-scoped registration; only active context and global bindings fire
- [ ] `[Hotkey]` attribute generates mount/unmount registration code via source generator
- [ ] ScreenRouter push/pop automatically switches `HotkeyManager` context and manages focus traps
- [ ] `GetBindingDisplayString` returns human-readable binding text that adapts to the current input device
- [ ] Focus ring visual indicators appear in Keyboard/Gamepad mode and hide in Mouse/Touch mode
- [ ] All focus and hotkey state is cleaned up on component unmount with no leaked registrations
