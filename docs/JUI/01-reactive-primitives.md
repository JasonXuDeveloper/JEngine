# Section 1 — Reactive Primitives: Signal & Computed

## Overview

Signal and Computed are the foundational reactive primitives of JUI, inspired by SolidJS signals. They provide fine-grained reactivity without virtual DOM diffing. Signal is a read/write reactive variable. ComputedSignal is a lazy, read-only derived value.

## Dependencies

None — this is the first section, no prior sections required.

## File Structure

- `Runtime/JUI/State/ISignalBase.cs`
- `Runtime/JUI/State/IReadOnlySignal.cs`
- `Runtime/JUI/State/Signal.cs`
- `Runtime/JUI/State/ComputedSignal.cs`

## API Design

```csharp
/// <summary>Base interface for all signal types. Provides version tracking and subscriber management.</summary>
public interface ISignalBase
{
    /// <summary>Monotonically increasing version number, incremented on each value change.</summary>
    int Version { get; }

    /// <summary>Add an effect as a subscriber to this signal.</summary>
    void Subscribe(IEffect effect);

    /// <summary>Remove an effect subscriber.</summary>
    void Unsubscribe(IEffect effect);
}

/// <summary>Read-only reactive signal interface. Consumers can read and subscribe but not write.</summary>
public interface IReadOnlySignal<out T> : ISignalBase
{
    /// <summary>
    /// Gets the current value. Reading this property inside an Effect automatically
    /// registers the effect as a dependency (auto-tracking).
    /// </summary>
    T Value { get; }
}

/// <summary>
/// A reactive variable that holds a single value and notifies subscribers when changed.
/// Equality-checked: setting the same value does not trigger notifications.
/// </summary>
public sealed class Signal<T> : IReadOnlySignal<T>, ISignalBase, IDisposable
{
    /// <summary>Creates a new signal with the given initial value.</summary>
    public Signal(T initialValue = default);

    /// <summary>Creates a new signal with the given initial value and custom equality comparer.</summary>
    public Signal(T initialValue, IEqualityComparer<T> comparer);

    /// <summary>
    /// Gets or sets the current value.
    /// Get: tracks the reading effect as a dependency.
    /// Set: if the new value differs (via equality check), bumps version, adds to Batch pending set.
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// Read the current value WITHOUT tracking. Use in event handlers where you need
    /// the value but don't want the handler to become a dependency.
    /// </summary>
    public T Peek();

    /// <summary>Current version number.</summary>
    public int Version { get; }

    public void Subscribe(IEffect effect);
    public void Unsubscribe(IEffect effect);
    public void Dispose();
}

/// <summary>
/// A read-only signal whose value is lazily computed from other signals.
/// Only recomputes when its dependencies change AND someone reads it.
/// Supports diamond dependency resolution without double computation.
/// </summary>
public sealed class ComputedSignal<T> : IReadOnlySignal<T>, IDisposable
{
    /// <summary>
    /// Gets the computed value. Recomputes if dirty (a dependency changed since last read).
    /// Lazy: does not compute until first read.
    /// </summary>
    public T Value { get; }

    public int Version { get; }
    public void Subscribe(IEffect effect);
    public void Unsubscribe(IEffect effect);
    public void Dispose();
}

/// <summary>Factory methods for creating ComputedSignals with zero-closure patterns.</summary>
public static class Computed
{
    /// <summary>Create a computed signal from a function that reads other signals.</summary>
    public static ComputedSignal<T> From<T>(Func<T> computation);

    /// <summary>
    /// Create a computed signal with typed state (avoids closure allocation).
    /// Preferred pattern for production code.
    /// </summary>
    public static ComputedSignal<T> From<TState, T>(Func<TState, T> computation, TState state);
}
```

## Data Structures

- `IEffect` interface (forward reference to Section 2) — effects subscribe to signals.
- Internal `List<IEffect> _subscribers` on each signal for tracking.
- Internal `_version` counter (int, starts at 0).
- Internal `_isDirty` flag on ComputedSignal.
- Internal dependency tracking context (`EffectTracker.Current`) — thread-static.

## Implementation Notes

- **Equality check**: Use `EqualityComparer<T>.Default` unless custom comparer provided. Setting same value is a no-op.
- **Auto-tracking**: When an Effect is running, it sets `EffectTracker.Current` to itself. Any signal's `Value` getter checks `EffectTracker.Current` and calls `Subscribe(effect)` on itself.
- **Lazy computation**: ComputedSignal only recomputes when: (a) at least one dependency has changed (dirty flag set), AND (b) someone reads `.Value`. If never read, never computes.
- **Diamond dependency**: If signal S feeds both computed C1 and C2, and an effect reads both C1 and C2, changing S causes C1 and C2 to recompute once each, and the effect runs once.
- **Peek()**: Reads value without setting `EffectTracker.Current`, so the signal does NOT register the caller as a subscriber.
- **Dispose**: Unsubscribes from all dependency signals, clears subscriber list.
- **Thread safety**: Signals are main-thread only. No locking needed.

## Source Generator Notes

N/A for this section — signals are runtime primitives, not generated.

## Usage Examples

```csharp
// Basic signal
var health = new Signal<int>(100);
health.Value = 80;  // Subscribers notified
health.Value = 80;  // No notification (equality check)

// Peek (no-track read)
var currentHealth = health.Peek();  // Does not register as dependency

// Computed signal (closure-free)
var maxHealth = new Signal<int>(100);
var healthPct = Computed.From(
    static (state) => (float)state.health.Value / state.max.Value,
    (health: health, max: maxHealth));
// healthPct.Value = 0.8f — recomputes when health or maxHealth change

// Custom equality comparer
var position = new Signal<Vector3>(Vector3.zero, new ApproximateVector3Comparer(0.01f));
```

## Test Plan

1. **Signal set/get**: Create signal, set value, assert get returns same value.
2. **Equality skip**: Set same value twice, verify subscribers notified only once.
3. **Custom comparer**: Use custom comparer, verify equality check uses it.
4. **Peek no-track**: Read via `Peek()` inside effect context, verify signal does NOT track effect.
5. **Version increment**: Set different value, verify Version increments by 1.
6. **Computed lazy**: Create computed, don't read it, verify computation function never called.
7. **Computed recomputes on dependency change**: Change dependency signal, read computed, verify new value.
8. **Computed caching**: Read computed twice without dependency change, verify computation runs once.
9. **Diamond dependency**: Signal feeds two computeds, effect reads both, change signal — verify each computed recomputes once, effect runs once.
10. **Computed.From with typed state**: Verify zero-closure pattern works correctly.
11. **Dispose signal**: Dispose signal, verify subscribers cleared.
12. **Dispose computed**: Dispose computed, verify it unsubscribes from dependencies.

## Acceptance Criteria

- [ ] `Signal<T>` implements `IReadOnlySignal<T>`, `ISignalBase`, `IDisposable`
- [ ] `ComputedSignal<T>` implements `IReadOnlySignal<T>`, `IDisposable`
- [ ] Equality-checked setter prevents redundant notifications
- [ ] `Peek()` reads without dependency tracking
- [ ] Computed is lazy — no computation until first `.Value` read
- [ ] Computed dirty flag set when any dependency signal changes
- [ ] Diamond dependency resolution works correctly
- [ ] `Computed.From<TState, T>()` avoids closure allocation
- [ ] All public APIs have XML documentation
- [ ] Zero allocations on signal read/write in hot path (after initial setup)
