# Section 17 — Rich Text & Gesture System

## Overview

This section covers two complementary subsystems:

1. **RichTextBuilder** -- a `ref struct` that composes Unity rich text tags with zero heap allocations. It writes into a caller-supplied `Span<char>` (or a small stackalloc buffer internally) and produces the final string only on `Build()`.

2. **Gesture System** -- a set of pooled recognizers (LongPress, DoubleTap, TripleTap, Swipe, Drag, Pinch) driven by a central `GestureEngine` that ticks recognizer timers each frame. Gesture handlers can be wired declaratively via the `[OnGesture]` source-generator attribute or imperatively through extension methods on `VisualElement`.

Both systems are designed around JUI's zero-alloc, struct-first philosophy. Gesture events are `readonly struct` values passed by `in` reference. Recognizers are pooled and returned automatically when the subscription is disposed.

## Dependencies

| Dependency | Section | Purpose |
|---|---|---|
| Component | Section 6 | Gesture attributes target Component subclasses |
| Generator Setup | Section 9 | `[OnGesture]` attribute wiring via source generators |

## File Structure

```
Runtime/JUI/
├── RichText/
│   └── RichTextBuilder.cs
├── Gestures/
│   ├── GestureEngine.cs
│   ├── LongPressRecognizer.cs
│   ├── MultiTapRecognizer.cs
│   ├── SwipeRecognizer.cs
│   ├── DragRecognizer.cs
│   ├── PinchRecognizer.cs
│   └── GestureEvents.cs          # LongPressEvent, MultiTapEvent, SwipeEvent, DragEvent
└── Attributes/
    └── OnGestureAttribute.cs
```

## API Design

### RichTextBuilder

```csharp
/// <summary>
/// Zero-allocation rich text composer. Writes Unity rich text tags into a
/// stack-allocated buffer and produces a single string on <see cref="Build"/>.
/// </summary>
/// <remarks>
/// Because this is a <c>ref struct</c>, it cannot be boxed, stored in fields,
/// or captured by lambdas. Use it as a local variable only.
/// </remarks>
public ref struct RichTextBuilder
{
    /// <summary>Creates a builder backed by the supplied buffer.</summary>
    public RichTextBuilder(Span<char> buffer);

    /// <summary>Wraps <paramref name="text"/> in &lt;b&gt; tags.</summary>
    public RichTextBuilder Bold(string text);

    /// <summary>Wraps <paramref name="text"/> in &lt;i&gt; tags.</summary>
    public RichTextBuilder Italic(string text);

    /// <summary>Wraps <paramref name="text"/> in &lt;color&gt; tags using the given color.</summary>
    public RichTextBuilder Color(string text, Color32 color);

    /// <summary>Wraps <paramref name="text"/> in &lt;size&gt; tags.</summary>
    public RichTextBuilder Size(string text, int sizePx);

    /// <summary>Appends plain (untagged) text.</summary>
    public RichTextBuilder Plain(string text);

    /// <summary>Inserts a &lt;sprite name="..."&gt; tag for inline sprites.</summary>
    public RichTextBuilder Sprite(string name);

    /// <summary>
    /// Finalizes the builder, allocates the result string, and returns it.
    /// After calling Build the builder must not be reused.
    /// </summary>
    public string Build();
}
```

All mutating methods return `this` by ref to enable fluent chaining. The `Color` overload converts `Color32` to a hex string (`#RRGGBBAA`) without allocating -- the 8 hex chars are written directly into the buffer.

### Gesture Events

```csharp
/// <summary>Fired when a long press is recognized.</summary>
public readonly struct LongPressEvent
{
    /// <summary>Screen position where the press began.</summary>
    public readonly Vector2 Position;

    /// <summary>Duration the pointer was held before recognition fired.</summary>
    public readonly float Duration;
}

/// <summary>Fired on double-tap, triple-tap, or higher multi-tap sequences.</summary>
public readonly struct MultiTapEvent
{
    /// <summary>Centroid position of the tap sequence.</summary>
    public readonly Vector2 Position;

    /// <summary>Number of taps detected (2 = double, 3 = triple).</summary>
    public readonly int TapCount;
}

/// <summary>Fired when a swipe gesture completes.</summary>
public readonly struct SwipeEvent
{
    /// <summary>Cardinal direction of the swipe.</summary>
    public readonly SwipeDirection Direction;

    /// <summary>Velocity in pixels per second at release.</summary>
    public readonly float Velocity;
}

/// <summary>Fired continuously during a drag gesture.</summary>
public readonly struct DragEvent
{
    /// <summary>Current phase of the drag.</summary>
    public readonly DragPhase Phase;

    /// <summary>Position where the drag originated.</summary>
    public readonly Vector2 StartPos;

    /// <summary>Frame delta since last DragEvent.</summary>
    public readonly Vector2 Delta;
}
```

### Enumerations

```csharp
public enum SwipeDirection { Left, Right, Up, Down }

public enum DragPhase { Begin, Move, End, Cancel }

public enum GestureType
{
    Tap,
    DoubleTap,
    TripleTap,
    LongPress,
    Swipe,
    Drag,
    Pinch
}
```

### Imperative Gesture API

```csharp
/// <summary>
/// Extension methods for attaching gesture recognizers to any VisualElement.
/// Each method returns an <see cref="IDisposable"/> that, when disposed,
/// removes the recognizer and returns it to the pool.
/// </summary>
public static class Gestures
{
    /// <summary>Registers a long-press handler.</summary>
    /// <param name="el">Target element.</param>
    /// <param name="handler">Callback invoked when the press exceeds <paramref name="minDuration"/>.</param>
    /// <param name="minDuration">Seconds the pointer must be held. Default 0.5s.</param>
    public static IDisposable OnLongPress(
        this VisualElement el,
        Action<LongPressEvent> handler,
        float minDuration = 0.5f);

    /// <summary>Registers a long-press handler with caller-supplied state (avoids closure allocation).</summary>
    public static IDisposable OnLongPress<TState>(
        this VisualElement el,
        Action<LongPressEvent, TState> handler,
        TState state,
        float minDuration = 0.5f);

    /// <summary>Registers a double-tap handler.</summary>
    /// <param name="maxInterval">Maximum seconds between taps. Default 0.3s.</param>
    public static IDisposable OnDoubleTap(
        this VisualElement el,
        Action<MultiTapEvent> handler,
        float maxInterval = 0.3f);

    /// <summary>Registers a swipe handler.</summary>
    /// <param name="minDistance">Minimum distance in pixels to qualify as a swipe.</param>
    /// <param name="minVelocity">Minimum velocity in px/s to qualify as a swipe.</param>
    public static IDisposable OnSwipe(
        this VisualElement el,
        Action<SwipeEvent> handler,
        float minDistance = 50f,
        float minVelocity = 200f);

    /// <summary>Registers a drag handler.</summary>
    /// <param name="threshold">Dead zone in pixels before the drag begins.</param>
    public static IDisposable OnDrag(
        this VisualElement el,
        Action<DragEvent> handler,
        float threshold = 5f);
}
```

### Declarative Gesture Attribute

```csharp
/// <summary>
/// Source-generator attribute. Wires the decorated method to a gesture
/// recognizer on the named element. The method must accept exactly one
/// parameter matching the gesture event type.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class OnGestureAttribute : Attribute
{
    public OnGestureAttribute(string elementName, GestureType gestureType);

    public string ElementName { get; }
    public GestureType GestureType { get; }

    /// <summary>Optional: min duration for LongPress (seconds). Default 0.5.</summary>
    public float MinDuration { get; set; } = 0.5f;

    /// <summary>Optional: max interval for DoubleTap/TripleTap (seconds). Default 0.3.</summary>
    public float MaxInterval { get; set; } = 0.3f;

    /// <summary>Optional: min distance for Swipe (pixels). Default 50.</summary>
    public float MinDistance { get; set; } = 50f;

    /// <summary>Optional: min velocity for Swipe (px/s). Default 200.</summary>
    public float MinVelocity { get; set; } = 200f;

    /// <summary>Optional: dead zone threshold for Drag (pixels). Default 5.</summary>
    public float DragThreshold { get; set; } = 5f;
}
```

### GestureEngine

```csharp
/// <summary>
/// Central tick manager for all active gesture recognizers. Called once per
/// frame by the JUI runtime loop. Advances timers, detects timeouts, and
/// fires callbacks.
/// </summary>
internal static class GestureEngine
{
    /// <summary>Advances all registered recognizer timers by deltaTime.</summary>
    internal static void Tick(float deltaTime);

    /// <summary>Registers a recognizer for ticking. Called automatically by Gestures extensions.</summary>
    internal static void Register(IGestureRecognizer recognizer);

    /// <summary>Unregisters a recognizer. Called on IDisposable.Dispose().</summary>
    internal static void Unregister(IGestureRecognizer recognizer);
}
```

## Data Structures

### RichTextBuilder Internal Layout

```
┌──────────────────────────────────────────────────────────────┐
│ Span<char> buffer (stackalloc or caller-supplied)            │
│ [<b>Hello</b> <color=#FF0000FF>World</color> ···  unused  ] │
│  ^                                                    ^      │
│  0                                               _position  │
└──────────────────────────────────────────────────────────────┘
```

- `_position` tracks the write head.
- Each tag-writing method advances `_position` by the exact number of chars written.
- `Build()` calls `new string(buffer[.._position])` -- the single allocation.

### Recognizer Pool

```
ObjectPool<LongPressRecognizer>   (default capacity: 8)
ObjectPool<MultiTapRecognizer>    (default capacity: 8)
ObjectPool<SwipeRecognizer>       (default capacity: 4)
ObjectPool<DragRecognizer>        (default capacity: 4)
ObjectPool<PinchRecognizer>       (default capacity: 2)
```

Recognizers implement `IResettable` so the pool can clear state on return.

### GestureEngine Active List

```
SmallList<IGestureRecognizer> _active;   // Inline-4, spills to ArrayPool
```

Iterated every frame in `Tick()`. Typically contains fewer than 10 entries for any given screen.

## Implementation Notes

### RichTextBuilder

1. **Buffer sizing**: Default stackalloc is 512 chars. For longer compositions, the caller can supply a larger `Span<char>` from an `ArrayPool<char>` rental.

2. **Color formatting**: `Color32` is formatted as `#RRGGBBAA` using a lookup table of hex nibbles (no `ToString("X2")` calls). Each byte produces two chars written directly to the span.

3. **Nesting**: Tags can be nested arbitrarily (bold-italic, colored-bold, etc.) because each method writes both the opening and closing tag around the supplied text. For complex nesting (e.g., bold wrapper around multiple colored segments), use multiple calls and `Plain` for separators.

4. **Overflow**: If the buffer is exhausted, the builder truncates silently. A debug-only assertion logs a warning. Production builds never throw.

### Gesture Recognizers

1. **Pointer tracking**: Recognizers register `PointerDownEvent`, `PointerMoveEvent`, `PointerUpEvent`, and `PointerCancelEvent` on the target `VisualElement`. They do not use `PointerCapture` unless the gesture enters the active phase (drag, pinch).

2. **LongPressRecognizer**: On `PointerDown`, records position and starts a timer. `GestureEngine.Tick()` checks elapsed time. If the pointer moves beyond a tolerance radius (10px), the recognizer resets. If the timer exceeds `minDuration`, the callback fires and the recognizer enters a "fired" state (won't fire again until the pointer is released and re-pressed).

3. **MultiTapRecognizer**: Counts sequential taps within `maxInterval`. After each `PointerUp`, starts a timeout. If another `PointerDown` arrives within the interval and within a 20px radius, increments the tap count. When the timeout expires, fires with the accumulated count (if >= target count). Supports both DoubleTap (target=2) and TripleTap (target=3).

4. **SwipeRecognizer**: On `PointerDown`, records start position and time. On `PointerUp`, computes distance and velocity. If both exceed thresholds, determines direction from the dominant axis and fires. The recognizer does not fire during the gesture -- only on completion.

5. **DragRecognizer**: On `PointerDown`, records start position. On `PointerMove`, if displacement exceeds threshold, transitions to `DragPhase.Begin` and captures the pointer. Subsequent moves fire `DragPhase.Move` with deltas. `PointerUp` fires `DragPhase.End`. `PointerCancel` fires `DragPhase.Cancel`.

6. **PinchRecognizer**: Tracks two pointer IDs simultaneously. Computes the distance between them each frame. Fires a `PinchEvent` (not listed in the primary event structs as it is touch-only) with scale factor and center point. Desktop fallback: Ctrl+scroll-wheel simulates pinch.

7. **Disposal**: Each `Gestures.OnXxx` call returns an `IDisposable`. Disposing unregisters all pointer event callbacks, calls `GestureEngine.Unregister`, resets the recognizer, and returns it to the pool. Components that use `[OnGesture]` have disposal wired into `OnDetach` automatically by the source generator.

### GestureEngine Tick

- `GestureEngine.Tick(deltaTime)` is called by the JUI root scheduler (same loop that ticks animations and signals).
- Iteration uses a reverse-index loop to allow recognizers to self-remove during tick.
- `deltaTime` is `Time.unscaledDeltaTime` so gestures work correctly during pause (timeScale = 0).

## Source Generator Notes

### [OnGesture] Wiring

Given this user code:

```csharp
public partial class SlotView : Component
{
    [OnGesture(nameof(El.SlotIcon), GestureType.LongPress, MinDuration = 0.8f)]
    private void ShowTooltip(LongPressEvent evt)
    {
        // user logic
    }
}
```

The generator emits into `SlotView.g.cs`:

```csharp
partial class SlotView
{
    private IDisposable __gesture_ShowTooltip;

    partial void __AttachGestures()
    {
        __gesture_ShowTooltip = El.SlotIcon.OnLongPress(
            static (in LongPressEvent evt, SlotView self) => self.ShowTooltip(evt),
            this,
            minDuration: 0.8f);
    }

    partial void __DetachGestures()
    {
        __gesture_ShowTooltip?.Dispose();
        __gesture_ShowTooltip = null;
    }
}
```

- `__AttachGestures()` is called from the generated `OnAttach` override.
- `__DetachGestures()` is called from the generated `OnDetach` override.
- The `static` lambda with `TState` avoids closure capture.

### Diagnostics

| ID | Severity | Message |
|---|---|---|
| JUI400 | Error | `[OnGesture]` method must accept exactly one parameter of the matching event type |
| JUI401 | Error | Element name in `[OnGesture]` does not match any `[El]` field |
| JUI402 | Warning | `[OnGesture]` with GestureType.Pinch on a non-touch platform target |
| JUI403 | Error | `[OnGesture]` method must be non-static and declared in a Component subclass |

## Usage Examples

### RichTextBuilder — Inline Composition

```csharp
// Compose rich text for a damage number popup
Span<char> buf = stackalloc char[256];
var text = new RichTextBuilder(buf)
    .Bold("CRITICAL ")
    .Color("2,847", new Color32(255, 50, 50, 255))
    .Plain(" damage")
    .Build();

label.text = text;  // "<b>CRITICAL </b><color=#FF3232FF>2,847</color> damage"
```

### RichTextBuilder — Multi-Segment

```csharp
Span<char> buf = stackalloc char[512];
var builder = new RichTextBuilder(buf);

foreach (var word in words)
{
    builder = word.IsHighlighted
        ? builder.Color(word.Text, highlightColor)
        : builder.Plain(word.Text);
    builder = builder.Plain(" ");
}

searchResultLabel.text = builder.Build();
```

### Imperative Gesture — Long Press

```csharp
public class InventorySlot : Component
{
    private IDisposable _longPressSub;

    protected override void OnAttach()
    {
        _longPressSub = El.Icon.OnLongPress(evt =>
        {
            ShowContextMenu(evt.Position);
        }, minDuration: 0.6f);
    }

    protected override void OnDetach()
    {
        _longPressSub?.Dispose();
    }
}
```

### Imperative Gesture — Swipe to Dismiss

```csharp
_swipeSub = card.OnSwipe(evt =>
{
    if (evt.Direction == SwipeDirection.Left && evt.Velocity > 300f)
    {
        DismissCard(card);
    }
}, minDistance: 80f, minVelocity: 300f);
```

### Imperative Gesture — Drag and Drop

```csharp
_dragSub = El.DraggableItem.OnDrag(evt =>
{
    switch (evt.Phase)
    {
        case DragPhase.Begin:
            CreateGhostImage(evt.StartPos);
            break;
        case DragPhase.Move:
            MoveGhostImage(evt.Delta);
            HighlightDropTarget(evt.StartPos + evt.Delta);
            break;
        case DragPhase.End:
            TryDrop(evt.StartPos + evt.Delta);
            DestroyGhostImage();
            break;
        case DragPhase.Cancel:
            DestroyGhostImage();
            break;
    }
});
```

### Declarative Gesture — Source Generator

```csharp
public partial class ItemSlot : Component
{
    [OnGesture(nameof(El.Icon), GestureType.LongPress, MinDuration = 0.5f)]
    private void OnIconLongPress(LongPressEvent evt)
    {
        tooltipManager.Show(itemData.Description, evt.Position);
    }

    [OnGesture(nameof(El.Icon), GestureType.DoubleTap)]
    private void OnIconDoubleTap(MultiTapEvent evt)
    {
        UseItem(itemData);
    }
}
```

### Combining Gestures

```csharp
public partial class MapView : Component
{
    private IDisposable _dragSub;
    private IDisposable _pinchSub;

    protected override void OnAttach()
    {
        _dragSub = El.MapCanvas.OnDrag(evt =>
        {
            if (evt.Phase == DragPhase.Move)
                PanMap(evt.Delta);
        });

        // Pinch-to-zoom (touch) or Ctrl+scroll (desktop)
        _pinchSub = El.MapCanvas.OnPinch(evt =>
        {
            ZoomMap(evt.ScaleFactor, evt.Center);
        });
    }

    protected override void OnDetach()
    {
        _dragSub?.Dispose();
        _pinchSub?.Dispose();
    }
}
```

## Test Plan

### RichTextBuilder Tests

| # | Test | Expectation |
|---|---|---|
| 1 | `Bold("x").Build()` | Returns `"<b>x</b>"` |
| 2 | `Italic("x").Build()` | Returns `"<i>x</i>"` |
| 3 | `Color("x", red).Build()` | Returns `"<color=#FF0000FF>x</color>"` |
| 4 | `Size("x", 24).Build()` | Returns `"<size=24>x</size>"` |
| 5 | `Plain("abc").Build()` | Returns `"abc"` (no tags) |
| 6 | `Sprite("coin").Build()` | Returns `"<sprite name=\"coin\">"` |
| 7 | Fluent chain of Bold + Color + Plain | Concatenated tags in order |
| 8 | Empty string inputs | Returns empty tags `"<b></b>"` |
| 9 | Buffer overflow (write exceeds span length) | Truncates without throwing |
| 10 | `Color32(0,0,0,0)` formatting | Produces `#00000000` |

### Gesture Recognizer Tests

| # | Test | Expectation |
|---|---|---|
| 11 | LongPress fires after minDuration | Callback invoked with correct position and duration |
| 12 | LongPress cancelled by movement > 10px | Callback not invoked |
| 13 | LongPress cancelled by PointerUp before duration | Callback not invoked |
| 14 | DoubleTap fires on two taps within maxInterval | MultiTapEvent.TapCount == 2 |
| 15 | DoubleTap does not fire if interval exceeded | Callback not invoked |
| 16 | TripleTap fires on three taps | MultiTapEvent.TapCount == 3 |
| 17 | Swipe fires when distance and velocity met | Correct SwipeDirection |
| 18 | Swipe does not fire below minDistance | Callback not invoked |
| 19 | Swipe does not fire below minVelocity | Callback not invoked |
| 20 | Drag Begin fires after threshold exceeded | DragPhase.Begin with correct StartPos |
| 21 | Drag Move fires with frame deltas | DragPhase.Move with non-zero Delta |
| 22 | Drag End fires on PointerUp | DragPhase.End |
| 23 | Drag Cancel fires on PointerCancel | DragPhase.Cancel |
| 24 | Disposing subscription stops callbacks | No further events after Dispose() |
| 25 | Disposed recognizer returned to pool | Pool count incremented |

### GestureEngine Tests

| # | Test | Expectation |
|---|---|---|
| 26 | Tick advances recognizer timers | LongPress fires after cumulative ticks >= minDuration |
| 27 | Unregister during Tick (self-removal) | No exception, recognizer removed |
| 28 | Multiple recognizers on same element | All fire independently |

### Source Generator Tests

| # | Test | Expectation |
|---|---|---|
| 29 | `[OnGesture]` with matching event param | Compiles, generates attach/detach |
| 30 | `[OnGesture]` with wrong param type | JUI400 diagnostic |
| 31 | `[OnGesture]` referencing missing element | JUI401 diagnostic |
| 32 | `[OnGesture]` on static method | JUI403 diagnostic |

## Acceptance Criteria

- [ ] `RichTextBuilder` is a `ref struct` with zero heap allocations until `Build()` is called
- [ ] All fluent methods on `RichTextBuilder` return the builder by ref for chaining
- [ ] `Color32` is formatted as `#RRGGBBAA` hex without `ToString` allocations
- [ ] Buffer overflow in `RichTextBuilder` truncates silently in release, asserts in debug
- [ ] All gesture events (`LongPressEvent`, `MultiTapEvent`, `SwipeEvent`, `DragEvent`) are `readonly struct`
- [ ] All gesture recognizers are pooled via `ObjectPool` and implement `IResettable`
- [ ] `Gestures.OnXxx` extension methods return `IDisposable` that unregisters callbacks, unregisters from `GestureEngine`, and returns the recognizer to the pool
- [ ] Stateful `OnLongPress<TState>` overload avoids closure allocation
- [ ] `GestureEngine.Tick()` uses `Time.unscaledDeltaTime` so gestures work during pause
- [ ] `GestureEngine` supports safe self-removal during iteration (reverse-index loop)
- [ ] `[OnGesture]` source generator emits `__AttachGestures` / `__DetachGestures` partial methods
- [ ] Generated gesture subscriptions use `static` lambdas with `TState` to avoid closures
- [ ] Diagnostics JUI400, JUI401, JUI402, JUI403 are emitted for invalid `[OnGesture]` usage
- [ ] `LongPressRecognizer` cancels if pointer moves beyond 10px tolerance
- [ ] `MultiTapRecognizer` supports configurable tap count target (2 for double, 3 for triple)
- [ ] `SwipeRecognizer` determines direction from dominant axis
- [ ] `DragRecognizer` captures pointer only after threshold is exceeded
- [ ] `PinchRecognizer` falls back to Ctrl+scroll on desktop platforms
- [ ] All recognizer disposal is wired into `OnDetach` for source-generated usages
