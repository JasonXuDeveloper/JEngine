# Section 13 — Event System (Source-Generated)

## Overview

Fully source-generated event bus with zero-dictionary dispatch. The `[JUIEvent]` attribute on a `readonly partial struct` triggers a Roslyn source generator that emits static subscriber arrays, `Publish(in T)`, `PublishDeferred()`, `FlushDeferred()`, and `Subscribe<TState>()` methods directly onto the struct. A `[Subscribe]` attribute on a method auto-wires the subscription at component initialization. A `[Publish]` attribute on a method generates a typed wrapper that constructs and publishes the event. All dispatch is array-based with no dictionary lookup, swap-remove O(1) unsubscribe, and zero allocation per publish/subscribe call.

## Dependencies

- Section 9 (Source Generator Project Setup)
- Section 6 (Component Base Class)

## File Structure

- `SourceGenerators/JEngine.JUI.Generators/EventGenerator.cs`
- `SourceGenerators/JEngine.JUI.Generators/SubscribeGenerator.cs`
- `SourceGenerators/JEngine.JUI.Generators/PublishGenerator.cs`
- `Runtime/JUI/Events/EventTrampoline.cs`
- `Runtime/JUI/Events/EventSubscription.cs`
- `Runtime/JUI/Attributes/JUIEventAttribute.cs`
- `Runtime/JUI/Attributes/SubscribeAttribute.cs`
- `Runtime/JUI/Attributes/PublishAttribute.cs`

## API Design

### Attributes

```csharp
/// <summary>
/// Marks a readonly partial struct as a JUI event. The source generator emits
/// static subscriber storage, Publish, PublishDeferred, FlushDeferred, and
/// Subscribe methods directly onto the struct.
/// </summary>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class JUIEventAttribute : Attribute { }

/// <summary>
/// Marks a method as an event subscriber. The source generator emits a call to
/// <c>TEvent.Subscribe(handler, state)</c> during component initialization and
/// stores the returned <see cref="IDisposable"/> for automatic cleanup on dispose.
/// </summary>
/// <remarks>
/// The decorated method must accept exactly one parameter of the event struct type.
/// Optionally, the method may be static with a second parameter of the component type
/// (used as state) to avoid closure allocation.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SubscribeAttribute : Attribute
{
    /// <summary>
    /// The event struct type to subscribe to. Must be decorated with <see cref="JUIEventAttribute"/>.
    /// </summary>
    public Type EventType { get; }

    public SubscribeAttribute(Type eventType);
}

/// <summary>
/// Marks a method as an event publisher. The source generator emits a wrapper method
/// that constructs the event struct from the method parameters and calls
/// <c>TEvent.Publish(in evt)</c>.
/// </summary>
/// <remarks>
/// The method must be <c>partial</c>. The generator emits the implementation body.
/// Parameters must match the event struct's public readonly fields by name (case-insensitive).
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PublishAttribute : Attribute
{
    /// <summary>
    /// The event struct type to publish. Must be decorated with <see cref="JUIEventAttribute"/>.
    /// </summary>
    public Type EventType { get; }

    public PublishAttribute(Type eventType);
}
```

### Runtime Support Types

```csharp
/// <summary>
/// Wraps a typed subscriber handler as an untyped delegate without closure allocation.
/// The typed state is stored alongside the handler in the subscriber array entry.
/// This avoids boxing value-type states and avoids closure captures for reference-type states.
/// </summary>
internal static class EventTrampoline<TEvent, TState>
{
    /// <summary>
    /// Wraps a strongly-typed handler and its state into an untyped tuple suitable for
    /// storage in the event's static subscriber array.
    /// </summary>
    /// <param name="handler">The typed handler that receives the event and state.</param>
    /// <param name="state">The typed state passed to the handler on each invocation.</param>
    /// <returns>
    /// A tuple of (untyped handler delegate, boxed state) where the untyped handler
    /// internally casts state back to <typeparamref name="TState"/> and invokes the
    /// original typed handler. The trampoline delegate is cached per
    /// <typeparamref name="TState"/> type to avoid repeated allocation.
    /// </returns>
    public static (Action<TEvent, object> handler, object state) Wrap(
        Action<TEvent, TState> handler, TState state);
}

/// <summary>
/// Represents an active event subscription. Disposing this object removes the subscriber
/// from the event's static array using swap-remove for O(1) unsubscription.
/// </summary>
public readonly struct EventSubscription : IDisposable
{
    /// <summary>
    /// Removes this subscriber from the event's static subscriber array.
    /// Uses swap-remove: the last element in the array is moved into this subscriber's
    /// slot, and the count is decremented. O(1) time complexity.
    /// Safe to call multiple times (subsequent calls are no-ops).
    /// </summary>
    public void Dispose();
}
```

### Generated Code (emitted by EventGenerator onto the event struct)

```csharp
// Developer writes:
[JUIEvent]
public readonly partial struct DamageEvent
{
    public readonly int Amount;
    public readonly Vector3 Position;
}

// EventGenerator emits:
public readonly partial struct DamageEvent
{
    // --- Static subscriber storage (no dictionary, direct array) ---
    private static (Action<DamageEvent, object> handler, object state)[] _subscribers
        = new (Action<DamageEvent, object>, object)[8];
    private static int _subscriberCount;

    // --- Deferred queue ---
    private static DamageEvent[] _deferredQueue = new DamageEvent[4];
    private static int _deferredCount;

    /// <summary>
    /// Publishes this event immediately to all current subscribers.
    /// Iterates the static subscriber array directly -- no dictionary lookup.
    /// </summary>
    /// <param name="evt">The event to publish, passed by readonly reference to avoid copies.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Publish(in DamageEvent evt)
    {
        var subs = _subscribers;
        var count = _subscriberCount;
        for (int i = 0; i < count; i++)
        {
            subs[i].handler(evt, subs[i].state);
        }
    }

    /// <summary>
    /// Queues this event for deferred publication. The event is stored in a static
    /// buffer and dispatched when <see cref="FlushDeferred"/> is called (typically
    /// at end-of-frame by JUIManager).
    /// </summary>
    /// <param name="evt">The event to defer.</param>
    public static void PublishDeferred(in DamageEvent evt)
    {
        if (_deferredCount == _deferredQueue.Length)
            Array.Resize(ref _deferredQueue, _deferredQueue.Length * 2);
        _deferredQueue[_deferredCount++] = evt;
    }

    /// <summary>
    /// Dispatches all deferred events in FIFO order, then clears the deferred queue.
    /// Called by JUIManager at end-of-frame after effects have flushed.
    /// </summary>
    internal static void FlushDeferred()
    {
        var queue = _deferredQueue;
        var count = _deferredCount;
        _deferredCount = 0;
        for (int i = 0; i < count; i++)
        {
            Publish(in queue[i]);
            queue[i] = default; // clear reference to allow GC
        }
    }

    /// <summary>
    /// Subscribes a typed handler to this event. Returns an <see cref="EventSubscription"/>
    /// that removes the handler when disposed.
    /// </summary>
    /// <typeparam name="TState">The type of state passed to the handler on each invocation.</typeparam>
    /// <param name="handler">The handler invoked with the event and state on each publish.</param>
    /// <param name="state">The state instance passed to the handler.</param>
    /// <returns>A disposable subscription. Dispose to unsubscribe.</returns>
    public static EventSubscription Subscribe<TState>(
        Action<DamageEvent, TState> handler, TState state)
    {
        var (wrappedHandler, wrappedState) =
            EventTrampoline<DamageEvent, TState>.Wrap(handler, state);

        if (_subscriberCount == _subscribers.Length)
            Array.Resize(ref _subscribers, _subscribers.Length * 2);

        int index = _subscriberCount++;
        _subscribers[index] = (wrappedHandler, wrappedState);

        return new EventSubscription(/* index, ref _subscribers, ref _subscriberCount */);
    }
}
```

### Generated Code (emitted by SubscribeGenerator)

```csharp
// Developer writes:
public partial class HUDComponent : Component
{
    [Subscribe(typeof(DamageEvent))]
    private void OnDamage(DamageEvent evt)
    {
        // show damage number at evt.Position
    }
}

// SubscribeGenerator emits in the partial class:
public partial class HUDComponent
{
    private EventSubscription _sub_OnDamage;

    // Called during component initialization (InitScope):
    partial void InitSubscriptions()
    {
        _sub_OnDamage = DamageEvent.Subscribe<HUDComponent>(
            static (evt, self) => self.OnDamage(evt), this);
    }

    // Called during component disposal:
    partial void DisposeSubscriptions()
    {
        _sub_OnDamage.Dispose();
    }
}
```

### Generated Code (emitted by PublishGenerator)

```csharp
// Developer writes:
public partial class CombatSystem
{
    [Publish(typeof(DamageEvent))]
    public static partial void EmitDamage(int amount, Vector3 position);
}

// PublishGenerator emits:
public partial class CombatSystem
{
    public static partial void EmitDamage(int amount, Vector3 position)
    {
        var evt = new DamageEvent { Amount = amount, Position = position };
        DamageEvent.Publish(in evt);
    }
}
```

## Data Structures

| Type | Role |
|------|------|
| `(Action<TEvent, object> handler, object state)[]` | Per-event-type static subscriber array. Grows via `Array.Resize` (doubling), never shrinks. |
| `int _subscriberCount` | Logical length of the subscriber array. Decremented on swap-remove. |
| `TEvent[]` | Per-event-type static deferred queue. Grows via `Array.Resize` (doubling), cleared on flush. |
| `int _deferredCount` | Logical length of the deferred queue. Reset to 0 on flush. |
| `EventSubscription` | Readonly struct holding the index, array reference, and count reference for O(1) swap-remove on dispose. |
| `EventTrampoline<TEvent, TState>` | Static generic class that caches a trampoline delegate per `(TEvent, TState)` pair. The delegate casts `object` back to `TState` and invokes the typed handler. |

## Implementation Notes

- **Zero-dictionary dispatch**: Each event type's subscribers are stored in a `static` array directly on the struct. `Publish(in T)` iterates this array with a simple `for` loop. No `Dictionary<Type, ...>` is involved at any point, eliminating hash lookups and boxing on the publish path.
- **Swap-remove unsubscription**: When `EventSubscription.Dispose()` is called, the subscriber at the stored index is replaced with the last subscriber in the array, and `_subscriberCount` is decremented. This is O(1) but does not preserve subscriber ordering. If ordering matters, subscribers should use deferred publish instead.
- **Trampoline pattern**: `EventTrampoline<TEvent, TState>.Wrap()` returns a cached `Action<TEvent, object>` that internally casts `object` to `TState` and invokes the original typed handler. The trampoline delegate itself is allocated once per `(TEvent, TState)` generic instantiation and reused for all subscribers of that type pair. The `TState` instance is stored as `object` in the array entry (one box for reference types, which are already reference types; value-type states are boxed once on subscribe, not on publish).
- **Deferred publish**: `PublishDeferred` stores events in a FIFO queue. `FlushDeferred` is called by JUIManager at end-of-frame (after `Batch.FlushPending` and `EffectRunner.RunDirtyEffects`). This allows events to be raised from within effects or other event handlers without re-entrancy issues.
- **Array resizing**: Both the subscriber array and deferred queue start small (8 and 4 entries respectively) and double on overflow. They never shrink. This is appropriate because event systems typically reach a stable subscriber count early in the application lifecycle.
- **No-alloc publish path**: After initial subscription setup, `Publish(in T)` performs zero managed allocations. The event struct is passed by `in` reference (no copy for large structs), and the handler invocation is a direct delegate call.
- **Thread safety**: The event system is main-thread only. No locking is required. All static arrays are accessed only from the main thread.
- **Auto-dispose integration**: The `SubscribeGenerator` emits `InitSubscriptions()` and `DisposeSubscriptions()` partial methods that are called by the Component base class lifecycle. Subscriptions are automatically cleaned up when the component is disposed.

## Source Generator Notes

### EventGenerator

- **Trigger**: `[JUIEvent]` attribute on a `readonly partial struct`.
- **Validation**: The struct must be `readonly` and `partial`. If not, the generator emits a diagnostic error.
- **Output**: Emits a partial struct extension containing `_subscribers`, `_subscriberCount`, `_deferredQueue`, `_deferredCount`, `Publish`, `PublishDeferred`, `FlushDeferred`, and `Subscribe<TState>`.
- **Incremental**: Uses `ForAttributeWithMetadataName` for incremental generation. Only regenerates when the struct declaration or its fields change.

### SubscribeGenerator

- **Trigger**: `[Subscribe(typeof(TEvent))]` attribute on a method in a `partial class` that inherits from `Component`.
- **Validation**: The method must accept exactly one parameter of type `TEvent`. The containing class must be `partial`. The event type must be decorated with `[JUIEvent]`.
- **Output**: Emits `_sub_{MethodName}` field, `InitSubscriptions()` partial method body, and `DisposeSubscriptions()` partial method body. If multiple `[Subscribe]` methods exist, all subscriptions are aggregated into the same partial methods.
- **Static handler pattern**: The generated subscription uses a `static` lambda with the component instance as `TState`, avoiding closure allocation.

### PublishGenerator

- **Trigger**: `[Publish(typeof(TEvent))]` attribute on a `partial` method.
- **Validation**: Method parameters must match the event struct's public readonly fields by name (case-insensitive comparison). Parameter types must be assignment-compatible. The method must be `partial`.
- **Output**: Emits the method body that constructs the event struct from parameters and calls `TEvent.Publish(in evt)`.

## Usage Examples

```csharp
// --- Define events ---
[JUIEvent]
public readonly partial struct DamageEvent
{
    public readonly int Amount;
    public readonly Vector3 Position;
}

[JUIEvent]
public readonly partial struct HealEvent
{
    public readonly int Amount;
    public readonly string Source;
}

// --- Subscribe in a component ---
public partial class DamageNumberDisplay : Component
{
    [Subscribe(typeof(DamageEvent))]
    private void OnDamage(DamageEvent evt)
    {
        SpawnFloatingText($"-{evt.Amount}", evt.Position, Color.red);
    }

    [Subscribe(typeof(HealEvent))]
    private void OnHeal(HealEvent evt)
    {
        SpawnFloatingText($"+{evt.Amount}", Vector3.zero, Color.green);
    }
}

// --- Publish from game logic ---
public partial class CombatSystem
{
    [Publish(typeof(DamageEvent))]
    public static partial void EmitDamage(int amount, Vector3 position);

    [Publish(typeof(HealEvent))]
    public static partial void EmitHeal(int amount, string source);
}

// Usage:
CombatSystem.EmitDamage(42, hitPoint);
CombatSystem.EmitHeal(20, "Potion");

// --- Manual subscribe (non-component code) ---
var sub = DamageEvent.Subscribe<DamageTracker>(
    static (evt, tracker) => tracker.TotalDamage += evt.Amount,
    damageTracker);

// Later:
sub.Dispose(); // O(1) swap-remove unsubscription

// --- Deferred publish (safe from within effects or event handlers) ---
DamageEvent.PublishDeferred(new DamageEvent { Amount = 10, Position = Vector3.zero });
// Event dispatched at end-of-frame when JUIManager calls FlushDeferred()

// --- Multiple independent event types ---
// DamageEvent and HealEvent have completely separate subscriber arrays.
// Publishing a DamageEvent does not iterate HealEvent subscribers.
```

## Test Plan

1. **Publish delivers to all subscribers**: Subscribe three handlers to the same event type, publish one event, verify all three handlers are invoked with the correct event data.
2. **Subscribe returns disposable**: Subscribe a handler, dispose the returned `EventSubscription`, publish an event, verify the disposed handler is NOT invoked.
3. **Swap-remove correctness**: Subscribe handlers A, B, C (in order). Dispose B. Publish an event. Verify A and C are still invoked. Verify the subscriber count is 2.
4. **Deferred publish queues correctly**: Call `PublishDeferred` three times. Verify no handlers are invoked. Call `FlushDeferred`. Verify handlers are invoked three times in FIFO order.
5. **FlushDeferred clears queue**: After calling `FlushDeferred`, call it again immediately. Verify no handlers are invoked on the second call.
6. **Static dispatch -- no dictionary lookup**: Verify (via generated code inspection) that `Publish` iterates a static array on the struct, not a `Dictionary<Type, ...>`.
7. **Multiple event types are independent**: Subscribe to `DamageEvent` and `HealEvent` separately. Publish `DamageEvent`. Verify only `DamageEvent` subscribers are invoked. Publish `HealEvent`. Verify only `HealEvent` subscribers are invoked.
8. **Subscribe with typed state avoids closure**: Subscribe with a typed state object. Verify the handler receives the correct state instance. Verify no closure is allocated (the delegate is a static lambda).
9. **Double-dispose is safe**: Dispose the same `EventSubscription` twice. Verify no exception is thrown and the subscriber array is not corrupted.
10. **Array growth on overflow**: Subscribe more handlers than the initial array capacity (8). Verify all handlers are invoked on publish. Verify the array grew correctly.
11. **[Subscribe] auto-wires on component init**: Create a component with `[Subscribe]` methods. Verify the subscription is active after initialization.
12. **[Subscribe] auto-disposes on component dispose**: Dispose the component. Publish the event. Verify the `[Subscribe]` handler is NOT invoked.
13. **[Publish] generates correct struct construction**: Call a `[Publish]`-attributed method with specific arguments. Verify the published event struct has matching field values.
14. **Zero-alloc publish hot path**: After initial setup, publish 1000 events in a loop. Verify zero managed allocations (via Unity profiler or allocation tracker).

## Acceptance Criteria

- [ ] `[JUIEvent]` attribute triggers source generation on `readonly partial struct`
- [ ] Generator emits static subscriber array, not a dictionary
- [ ] `Publish(in T)` iterates array directly with `AggressiveInlining`
- [ ] `PublishDeferred(in T)` queues events for end-of-frame dispatch
- [ ] `FlushDeferred()` dispatches deferred events in FIFO order and clears the queue
- [ ] `Subscribe<TState>()` returns `EventSubscription` (disposable, swap-remove O(1))
- [ ] `EventTrampoline<TEvent, TState>` caches the trampoline delegate per type pair
- [ ] `[Subscribe]` on a component method auto-wires subscription at init and auto-disposes on component disposal
- [ ] `[Publish]` on a partial method generates struct construction and `Publish(in evt)` call
- [ ] Multiple event types maintain fully independent subscriber arrays
- [ ] Double-dispose of `EventSubscription` is a safe no-op
- [ ] Generator emits diagnostic errors for non-readonly, non-partial structs
- [ ] Generator emits diagnostic errors for parameter/field name mismatches in `[Publish]`
- [ ] All public APIs have XML documentation
- [ ] Zero allocations on the publish hot path after initial setup
