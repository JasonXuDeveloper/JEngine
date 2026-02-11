# Section 2 — Effect System & Batch

## Overview

Effects are functions that re-run when their signal dependencies change. They bridge reactive state to the real world (UI elements). The Batch system coalesces signal changes so effects run once per frame. Effects do NOT poll per frame -- they only execute when marked dirty by a dependency change.

## Dependencies

Section 1 (Reactive Primitives)

## File Structure

- `Runtime/JUI/State/IEffect.cs`
- `Runtime/JUI/State/Effect.cs`
- `Runtime/JUI/State/EffectRunner.cs`
- `Runtime/JUI/State/EffectPool.cs`
- `Runtime/JUI/State/Batch.cs`

## API Design

```csharp
/// <summary>Base interface for all effect types. An effect is a side-effect function that
/// re-runs when its tracked signal dependencies change.</summary>
public interface IEffect : IDisposable
{
    /// <summary>Whether this effect needs to re-run. Set to true by Batch.FlushPending()
    /// when a tracked dependency has changed.</summary>
    bool IsDirty { get; set; }

    /// <summary>Execute the effect's action. Clears IsDirty after execution.
    /// During execution, auto-tracking registers this effect as a subscriber
    /// on any signal whose Value getter is read.</summary>
    void Run();

    /// <summary>Register a signal as a dependency of this effect.
    /// Called automatically by signal Value getters during Run().</summary>
    void TrackDependency(ISignalBase signal);
}

/// <summary>
/// An effect with a typed state field (no boxing). Uses a static lambda pattern
/// to avoid closure allocations. The state parameter carries all captured context.
/// </summary>
/// <typeparam name="TState">The type of captured state passed to the action.</typeparam>
public sealed class Effect<TState> : IEffect
{
    /// <summary>The static action to execute when the effect runs.
    /// Receives the typed state as its parameter.</summary>
    public Action<TState> Action { get; }

    /// <summary>Typed state passed to the action on each run.
    /// Avoids boxing for value types and closure allocation for reference types.</summary>
    public TState State { get; set; }

    /// <summary>Whether this effect needs to re-run. Set to true by Batch.FlushPending()
    /// when a tracked dependency has changed.</summary>
    public bool IsDirty { get; set; }

    /// <summary>Execute the effect: sets EffectTracker.Current to this, invokes Action(State),
    /// clears EffectTracker.Current, and sets IsDirty to false.</summary>
    public void Run();

    /// <summary>Register a signal as a dependency. The effect subscribes to the signal
    /// and tracks it for later unsubscription on dispose.</summary>
    public void TrackDependency(ISignalBase signal);

    /// <summary>Unsubscribe from all tracked signals, clear internal state,
    /// and return this effect to the pool.</summary>
    public void Dispose();
}

/// <summary>
/// Pool effects by type to avoid GC allocation. Effects are rented when created
/// and returned when disposed, enabling reuse across the lifetime of the application.
/// </summary>
/// <typeparam name="TState">The state type of the pooled effects.</typeparam>
public static class EffectPool<TState>
{
    /// <summary>Rent an effect from the pool with the given action and state.
    /// If the pool is empty, a new Effect is allocated.</summary>
    /// <param name="action">The static action to execute on each run.</param>
    /// <param name="state">The typed state to pass to the action.</param>
    /// <returns>A ready-to-use effect instance.</returns>
    public static Effect<TState> Rent(Action<TState> action, TState state);

    /// <summary>Return an effect to the pool for reuse. The effect's state and
    /// dependency list are cleared before pooling.</summary>
    /// <param name="effect">The effect to return.</param>
    public static void Return(Effect<TState> effect);
}

/// <summary>
/// Runs all dirty effects in dependency order. Effects that read computed signals
/// are scheduled after those computed signals recompute, ensuring consistent state.
/// </summary>
public static class EffectRunner
{
    /// <summary>
    /// Run all effects that have IsDirty == true, in topological dependency order.
    /// If no effects are dirty, this is a no-op (zero work).
    /// Called by JUIManager.Update() Phase 2 after Batch.FlushPending().
    /// </summary>
    public static void RunDirtyEffects();
}

/// <summary>
/// Manages pending signal changes. Coalesces multiple signal writes within a frame
/// so that effects see only the final state and run at most once per frame.
/// </summary>
public static class Batch
{
    /// <summary>
    /// Called by signal setters when a value changes. Adds the signal to the
    /// pending-changes set. No effects run yet -- they are deferred to FlushPending().
    /// Duplicate notifications for the same signal within a frame are deduplicated.
    /// </summary>
    /// <param name="signal">The signal whose value has changed.</param>
    public static void NotifyChanged(ISignalBase signal);

    /// <summary>
    /// Walk all pending signals, mark their subscribed effects as dirty.
    /// Clears the pending set afterward.
    /// Called once per frame in JUIManager.Update() Phase 1.
    /// </summary>
    public static void FlushPending();

    /// <summary>
    /// Execute an action atomically -- FlushPending cannot happen mid-action.
    /// Signal changes within the action are collected in the pending set but
    /// effects do not run until the next frame's FlushPending().
    /// </summary>
    /// <param name="action">The action to execute atomically.</param>
    public static void Run(Action action);

    /// <summary>
    /// Typed state overload to avoid closure allocation.
    /// Execute an action atomically with typed state.
    /// </summary>
    /// <typeparam name="TState">The type of state to pass.</typeparam>
    /// <param name="action">The action to execute atomically.</param>
    /// <param name="state">The typed state passed to the action.</param>
    public static void Run<TState>(Action<TState> action, TState state);

    /// <summary>
    /// Same as Run() but also immediately flushes pending changes AND runs dirty effects
    /// after the action completes. Use for tests and initialization where you need
    /// effects to execute synchronously. Not intended for normal game code.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public static void RunAndFlush(Action action);

    /// <summary>Typed state overload of RunAndFlush to avoid closure allocation.</summary>
    /// <typeparam name="TState">The type of state to pass.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="state">The typed state passed to the action.</param>
    public static void RunAndFlush<TState>(Action<TState> action, TState state);
}
```

## Data Structures

- `IEffect` interface -- base contract for all effect instances; implemented by `Effect<TState>`.
- Internal `HashSet<ISignalBase> _pendingChanges` in Batch -- deduplicates signals that changed this frame.
- Internal `List<IEffect> _allEffects` in EffectRunner -- topologically sorted list of all registered effects.
- Internal `List<ISignalBase> _trackedSignals` in each `Effect<TState>` -- signals this effect depends on, for cleanup on dispose.
- Internal `bool _isBatching` flag in Batch -- prevents FlushPending during a Run() call.
- Internal `Stack<Effect<TState>> _pool` in `EffectPool<TState>` -- per-type pool of reusable effect instances.
- `EffectTracker.Current` (`[ThreadStatic]` static field) -- the currently executing effect, used for auto-dependency tracking.

## Implementation Notes

- **Effect.Run()**: Sets `EffectTracker.Current = this`, executes `Action(State)`, clears `EffectTracker.Current`, sets `IsDirty = false`. Any signal read during execution calls `TrackDependency()` on this effect automatically.
- **Auto-dependency tracking**: During `Run()`, any signal's `Value` getter checks `EffectTracker.Current` and, if non-null, calls `signal.Subscribe(EffectTracker.Current)` and `EffectTracker.Current.TrackDependency(signal)`.
- **EffectRunner** maintains a topologically sorted list -- effects that depend on computed signals run after those computeds recompute. This prevents effects from seeing stale computed values.
- **Batch deduplication**: Uses `HashSet<ISignalBase>` for pending changes. If the same signal is set multiple times in one frame, it appears only once in the pending set.
- **Batch.Run** sets `_isBatching = true` to prevent `FlushPending` from executing during the action. After the action completes, `_isBatching` is restored to its previous value.
- **FlushPending**: Iterates the pending signals set, for each signal iterates its subscriber list and marks each effect's `IsDirty = true`, then clears the pending set. This is O(signals * subscribers) but typically very small.
- **Effect disposal**: Unsubscribes from all tracked signals (calls `signal.Unsubscribe(this)` for each), clears the tracked signals list, and returns the effect to `EffectPool<TState>`.
- **Thread safety**: Effects and Batch are main-thread only. No locking is needed.

## Source Generator Notes

N/A for this section -- effects are runtime primitives. Source-generated effect registration is covered in Section 12 (EffectGenerator).

## Usage Examples

```csharp
// Create a signal and an effect that updates a label
var playerName = new Signal<string>("Hero");
var nameLabel = root.Q<Label>("player-name");

var fx = EffectPool<(Signal<string> name, Label label)>.Rent(
    static (state) => state.label.text = state.name.Value,
    (name: playerName, label: nameLabel));

// Initial run to establish dependencies
fx.Run();
// nameLabel.text is now "Hero"

// Later, change the signal:
playerName.Value = "Champion";
// On next frame: Batch.FlushPending() marks fx dirty,
// EffectRunner.RunDirtyEffects() re-runs fx,
// nameLabel.text becomes "Champion"

// Multiple changes in one frame coalesce:
playerName.Value = "Warrior";
playerName.Value = "Mage";
// Effect runs once with final value "Mage"

// Batch.Run for atomic multi-signal updates:
var health = new Signal<int>(100);
var mana = new Signal<int>(50);

Batch.Run(static (state) =>
{
    state.health.Value = 80;
    state.mana.Value = 30;
}, (health, mana));
// Both changes collected; effects see (80, 30) together, never (80, 50) or (100, 30)

// Batch.RunAndFlush for synchronous test assertions:
Batch.RunAndFlush(static (state) =>
{
    state.Value = 42;
}, health);
// Effects have already run -- safe to assert immediately

// Dispose to stop listening:
fx.Dispose();
playerName.Value = "Ghost";
// fx does NOT run -- it has been disposed and returned to the pool
```

## Test Plan

1. **Effect re-runs ONLY on dependency signal change** -- create an effect reading signal A, change signal B, verify effect does not run. Change signal A, verify effect runs.
2. **Effect runs once even if multiple deps change in same frame** -- create an effect reading signals A and B, change both in one frame, verify effect runs exactly once.
3. **Batch.Run atomicity** -- set two signals inside Batch.Run, verify effect never sees intermediate state (only the final state of both signals).
4. **Batch.RunAndFlush immediate execution** -- set a signal inside RunAndFlush, verify the effect has already run by the time RunAndFlush returns.
5. **Effect disposal stops re-runs** -- dispose an effect, change its dependency signal, verify the effect does not run.
6. **No dirty effects = zero work** -- call RunDirtyEffects with no dirty effects, verify no effects execute and minimal overhead.
7. **EffectPool Rent/Return reuse** -- rent an effect, dispose it (returns to pool), rent again, verify the same instance is returned.
8. **Dependency order: effect reading computed runs after computed recomputes** -- create a computed that reads signal A, create an effect that reads the computed, change signal A, verify the computed recomputes before the effect runs.
9. **Effect can be created, run, and disposed without leaks** -- create, run, and dispose many effects in a loop, verify no subscriber list growth on signals.

## Acceptance Criteria

- [ ] `IEffect` interface defines `IsDirty`, `Run()`, `TrackDependency()`, and inherits `IDisposable`
- [ ] `Effect<TState>` implements `IEffect` with typed state and static lambda pattern
- [ ] `EffectPool<TState>` provides zero-allocation Rent/Return lifecycle
- [ ] `EffectRunner.RunDirtyEffects()` executes effects in topological dependency order
- [ ] `EffectRunner.RunDirtyEffects()` is a no-op when no effects are dirty
- [ ] `Batch.NotifyChanged()` deduplicates pending signals via HashSet
- [ ] `Batch.FlushPending()` marks subscriber effects dirty and clears the pending set
- [ ] `Batch.Run()` prevents FlushPending during execution (atomicity guarantee)
- [ ] `Batch.Run<TState>()` typed overload avoids closure allocation
- [ ] `Batch.RunAndFlush()` immediately flushes and runs effects (for tests/initialization)
- [ ] Effect disposal unsubscribes from all tracked signals and returns to pool
- [ ] Auto-dependency tracking via `EffectTracker.Current` works correctly during `Run()`
- [ ] All public APIs have XML documentation
- [ ] Zero allocations on effect re-run hot path (after initial setup)
