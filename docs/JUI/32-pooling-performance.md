# Section 32 — Pooling & Performance Polish

## Overview

This is the final performance optimization pass across the entire JUI framework. It introduces component pooling via the `[Poolable]` attribute, pool warmup via `[PoolWarm]`, an `IntStringCache` for alloc-free int-to-string conversion, and a comprehensive set of performance contract verification tests. The section also provides the definitive table of all source generator diagnostic codes (JUI001-JUI110, JUI200-JUI206) and their meanings.

The goal is to ensure that after initial setup, the JUI hot path -- signal changes, effect execution, binding updates, event publishing, and component recycling -- produces zero managed allocations per frame.

## Dependencies

| Section | What It Provides |
|---------|-----------------|
| 2 — Effect System | `EffectPool<TState>`, effect pooling infrastructure |
| 6 — Component Base Class | Component lifecycle (OnMount/OnUnmount) extended for pool rent/return |
| 7 — Show & Switch | `Show`/`Switch` control flow modified to rent from pool instead of `new` |
| 9 — Source Generator Setup | Generator infrastructure for `[Poolable]` and `[PoolWarm]` code emission |
| 13 — Event System | Zero-alloc event dispatch verification |
| 18 — Virtualization | VirtualList/VirtualGrid scrolling performance verification |
| 21 — Shader Effects | GPU pass timing verification |
| 30 — JUIManager | Entry point for warmup initialization and performance metrics |

## File Structure

```
Packages/com.jasonxudeveloper.jengine.ui/
├── Runtime/JUI/
│   ├── Pooling/
│   │   └── ComponentPool.cs             # Generic component pool with type-keyed storage
│   ├── Utilities/
│   │   └── IntStringCache.cs            # Alloc-free int→string cache
│   └── Attributes/
│       ├── PoolableAttribute.cs         # Marks a component as poolable
│       └── PoolWarmAttribute.cs         # Declares pool warmup counts

SourceGenerators/JEngine.JUI.Generators/
├── PoolableGenerator.cs                 # Emits pool rent/return integration
└── PoolWarmGenerator.cs                 # Emits JUIAutoWarmup class
```

## API Design

### PoolableAttribute

```csharp
namespace JEngine.JUI;

/// <summary>
/// Marks a JUI component as poolable. When <c>Show</c> or <c>Switch</c> would
/// normally instantiate this component with <c>new()</c>, the generated code instead
/// rents from <see cref="ComponentPool{T}"/>. When the component is unmounted,
/// it is returned to the pool instead of being discarded.
/// </summary>
/// <remarks>
/// Poolable components must be designed for reuse: all signal values should be
/// resettable, and <c>OnMount</c>/<c>OnUnmount</c> should properly initialize
/// and tear down state. The generated code calls a <c>ResetForPool()</c> partial
/// method before returning to the pool, giving the component a chance to clear
/// any non-signal state.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PoolableAttribute : Attribute
{
    /// <summary>
    /// Maximum number of instances to keep in the pool. When the pool is full,
    /// returned instances are discarded (GC'd) instead of pooled.
    /// Default is 16.
    /// </summary>
    public int MaxPoolSize { get; }

    /// <summary>Creates a PoolableAttribute with the specified max pool size.</summary>
    /// <param name="maxPoolSize">Maximum instances to pool. Default 16.</param>
    public PoolableAttribute(int maxPoolSize = 16);
}
```

### PoolWarmAttribute

```csharp
namespace JEngine.JUI;

/// <summary>
/// Declares how many effects and bindings to pre-allocate in the pool for this
/// component type. The source generator emits a <c>JUIAutoWarmup</c> class with
/// a <c>WarmAll()</c> method that pre-allocates pools for all annotated components.
/// Call <c>JUIAutoWarmup.WarmAll()</c> during loading screens to avoid allocation
/// spikes during gameplay.
/// </summary>
/// <remarks>
/// The warmup pre-allocates:
/// <list type="bullet">
/// <item>Component instances (via <see cref="ComponentPool{T}"/>)</item>
/// <item>Effect instances (via <c>EffectPool&lt;TState&gt;</c>)</item>
/// <item>Binding instances</item>
/// </list>
/// This is most useful for components that are created and destroyed frequently,
/// such as damage numbers, buff icons, loot popups, and chat messages.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PoolWarmAttribute : Attribute
{
    /// <summary>Number of effect instances to pre-allocate for this component type.</summary>
    public int Effects { get; }

    /// <summary>Number of binding instances to pre-allocate for this component type.</summary>
    public int Bindings { get; }

    /// <summary>Number of component instances to pre-allocate. Defaults to the
    /// <see cref="PoolableAttribute.MaxPoolSize"/> value if the component is also
    /// marked <c>[Poolable]</c>, or 4 otherwise.</summary>
    public int Components { get; set; }

    /// <summary>Creates a PoolWarmAttribute with the specified pre-allocation counts.</summary>
    /// <param name="effects">Number of effect instances to pre-allocate.</param>
    /// <param name="bindings">Number of binding instances to pre-allocate.</param>
    public PoolWarmAttribute(int effects, int bindings);
}
```

### ComponentPool

```csharp
namespace JEngine.JUI.Pooling;

/// <summary>
/// Generic component pool keyed by component type. Provides zero-allocation
/// rent/return for frequently-created components. Thread-safe (main thread only,
/// no locking needed).
/// </summary>
/// <typeparam name="T">The component type. Must inherit from <c>Component</c>
/// and have a parameterless constructor.</typeparam>
public static class ComponentPool<T> where T : Component, new()
{
    /// <summary>
    /// Rents a component instance from the pool. If the pool is empty,
    /// a new instance is allocated via <c>new T()</c>.
    /// </summary>
    /// <returns>A component instance ready for mounting.</returns>
    public static T Rent();

    /// <summary>
    /// Returns a component instance to the pool for reuse. The component's
    /// generated <c>ResetForPool()</c> method is called before pooling.
    /// If the pool has reached its maximum size, the instance is discarded.
    /// </summary>
    /// <param name="instance">The component instance to return.</param>
    public static void Return(T instance);

    /// <summary>
    /// Pre-allocates the specified number of component instances in the pool.
    /// Called by <c>JUIAutoWarmup.WarmAll()</c>.
    /// </summary>
    /// <param name="count">Number of instances to pre-allocate.</param>
    public static void Warm(int count);

    /// <summary>
    /// Current number of available (pooled) instances.
    /// </summary>
    public static int AvailableCount { get; }

    /// <summary>
    /// Maximum pool size. Set by the <see cref="PoolableAttribute.MaxPoolSize"/>
    /// value at initialization.
    /// </summary>
    public static int MaxSize { get; set; }

    /// <summary>
    /// Clears all pooled instances. Used during scene transitions or memory pressure.
    /// </summary>
    public static void Clear();
}
```

### IntStringCache

```csharp
namespace JEngine.JUI.Utilities;

/// <summary>
/// Provides allocation-free int-to-string conversion for commonly used integer
/// ranges. Pre-caches string representations for values in the range [-1000, 10000].
/// Values outside this range fall back to <c>int.ToString()</c> (which allocates).
/// </summary>
/// <remarks>
/// This is particularly useful for UI elements that display frequently-changing
/// numeric values: health points, scores, timers, item counts, damage numbers.
/// Using <c>IntStringCache.Get(value)</c> instead of <c>value.ToString()</c>
/// eliminates per-frame string allocations in these hot paths.
/// </remarks>
public static class IntStringCache
{
    /// <summary>
    /// Returns the cached string representation of the given integer.
    /// For values in [-1000, 10000], returns a pre-cached string instance
    /// (zero allocation). For values outside this range, falls back to
    /// <c>int.ToString()</c>.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The string representation of the value.</returns>
    public static string Get(int value);

    /// <summary>
    /// Returns true if the value is within the cached range [-1000, 10000].
    /// </summary>
    /// <param name="value">The integer to check.</param>
    /// <returns>True if the value is cached; false if Get() will allocate.</returns>
    public static bool IsCached(int value);

    /// <summary>
    /// The minimum value in the cached range (inclusive). Default: -1000.
    /// </summary>
    public static int MinCachedValue { get; }

    /// <summary>
    /// The maximum value in the cached range (inclusive). Default: 10000.
    /// </summary>
    public static int MaxCachedValue { get; }
}
```

### Generated JUIAutoWarmup

```csharp
// Generated by PoolWarmGenerator — collects all [PoolWarm] annotations

namespace JEngine.JUI;

/// <summary>
/// Auto-generated warmup class. Call <see cref="WarmAll"/> during a loading
/// screen to pre-allocate all pooled resources, eliminating allocation spikes
/// during gameplay.
/// </summary>
public static class JUIAutoWarmup
{
    /// <summary>
    /// Pre-allocates all component pools, effect pools, and binding pools
    /// for components annotated with <see cref="PoolWarmAttribute"/>.
    /// </summary>
    public static void WarmAll()
    {
        // Example generated output for a project with two [PoolWarm] components:

        // BuffIcon: [PoolWarm(effects: 16, bindings: 32)]
        ComponentPool<BuffIcon>.Warm(32);       // matches [Poolable(maxPoolSize: 32)]
        EffectPool<BuffIcon.EffectState>.Warm(16);
        BindingPool.Warm(32);

        // PlayerHUD: [PoolWarm(effects: 16, bindings: 32, Components = 2)]
        ComponentPool<PlayerHUD>.Warm(2);
        EffectPool<PlayerHUD.EffectState>.Warm(16);
        BindingPool.Warm(32);
    }
}
```

## Data Structures

| Type | Purpose |
|------|---------|
| `ComponentPool<T>` | Static generic class with a `Stack<T>` for pooled instances and an `int MaxSize` cap. One pool per component type. |
| `IntStringCache` | Static class with a pre-allocated `string[]` array of size 11001 (indices 0-11000 mapping to values -1000 to 10000). Initialized in a static constructor. |
| `PoolableAttribute` | Compile-time marker read by `PoolableGenerator`. Stores `MaxPoolSize`. |
| `PoolWarmAttribute` | Compile-time marker read by `PoolWarmGenerator`. Stores `Effects`, `Bindings`, and `Components` counts. |
| `JUIAutoWarmup` | Source-generated static class aggregating all warmup calls. |

## Implementation Notes

### Component Pool Lifecycle

When a `[Poolable]` component is used with `Show` or `Switch`:

**Rent (instead of new):**
```csharp
// Generated by PoolableGenerator in Show/Switch integration:
var instance = ComponentPool<BuffIcon>.Rent();
// instance may be a recycled object -- its signals still hold old values.
// The mount lifecycle handles re-initialization:
// 1. OnInit() re-runs generated InitScope/InitBindings/InitEffects
// 2. OnMount() runs user code which sets initial signal values
```

**Return (instead of discard):**
```csharp
// Generated teardown when the component is unmounted:
partial void ResetForPool()
{
    // User-implementable: clear any non-signal state
}

// Framework calls:
component.OnUnmount();
component.OnDispose();
component.ResetForPool();  // generated partial method
ComponentPool<BuffIcon>.Return(component);
```

### Pool Return Guard

To prevent returning the same instance twice:

```csharp
public static void Return(T instance)
{
    if (instance._isPooled) return;  // already in pool
    instance._isPooled = true;

    instance.ResetForPool();

    if (_pool.Count >= MaxSize)
        return;  // discard, let GC collect

    _pool.Push(instance);
}

public static T Rent()
{
    if (_pool.Count == 0)
        return new T();

    var instance = _pool.Pop();
    instance._isPooled = false;
    return instance;
}
```

The `_isPooled` flag is a `bool` field on the Component base class, set by the pool infrastructure.

### IntStringCache Implementation

```csharp
public static class IntStringCache
{
    private const int Offset = 1000;   // maps -1000 to index 0
    private const int Min = -1000;
    private const int Max = 10000;
    private static readonly string[] _cache;

    static IntStringCache()
    {
        _cache = new string[Max - Min + 1];
        for (int i = Min; i <= Max; i++)
        {
            _cache[i - Min] = i.ToString();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Get(int value)
    {
        if (value >= Min && value <= Max)
            return _cache[value - Min];
        return value.ToString();  // fallback: allocates
    }
}
```

The cache uses approximately 11001 string references. At an average of ~20 bytes per string object (header + chars for 1-5 digit numbers), this is roughly 220 KB of heap memory -- a negligible cost for eliminating per-frame string allocations.

### Show/Switch Pool Integration

The `PoolableGenerator` modifies the generated `Show` and `Switch` code to use pool rent/return:

```csharp
// Without [Poolable] (default):
var child = new BuffIcon();
parent.AddChild(child);

// With [Poolable]:
var child = ComponentPool<BuffIcon>.Rent();
parent.AddChild(child);

// On unmount without [Poolable]:
parent.RemoveChild(child);
child.Dispose();

// On unmount with [Poolable]:
parent.RemoveChild(child);
child.ResetForPool();
ComponentPool<BuffIcon>.Return(child);
```

### Zero-Alloc Verification Strategy

Performance contracts are verified using a combination of:

1. **Unity Profiler markers**: Each JUI subsystem wraps its hot path in `ProfilerMarker.Auto()` scopes. Tests read the profiler data to verify allocation counts.
2. **`GC.GetAllocatedBytesForCurrentThread()`**: Before/after measurement in tight loops to detect managed allocations.
3. **Source-generated allocation audits**: The generator can optionally emit `[MethodImpl(MethodImplOptions.NoInlining)]` on hot-path methods to make them visible in profiler traces.

## Performance Contracts

| Metric | Target | Verification Method |
|--------|--------|---------------------|
| Draw calls per UI layer | <= 2 | GPU profiler, UI Toolkit batch count |
| Mount 100 components (cold) | < 2 ms | `Stopwatch` in test, averaged over 10 runs |
| Mount 100 components (warm pool) | < 1 ms | `Stopwatch` in test with pre-warmed pool |
| Effect re-run (warm pool) | 0 alloc | `GC.GetAllocatedBytesForCurrentThread()` delta = 0 |
| Binding update (push mode) | 0 alloc | `GC.GetAllocatedBytesForCurrentThread()` delta = 0 |
| Signal set (same value, equality skip) | 0 alloc | `GC.GetAllocatedBytesForCurrentThread()` delta = 0 |
| Signal set (new value) | 0 alloc | `GC.GetAllocatedBytesForCurrentThread()` delta = 0 |
| VirtualList 100k items scroll | < 0.5 ms, 0 alloc | Profiler marker timing, alloc tracker |
| EventBus publish (source-gen) | < 0.005 ms, 0 alloc | `Stopwatch` + alloc tracker, 10k iterations averaged |
| Gesture tick (10 recognizers) | < 0.05 ms | Profiler marker timing |
| Screen transition (push) | < 1 frame input lag | Measure frames between input event and visual change |
| UIEffect shader pass | < 0.3 ms GPU | GPU profiler marker |
| Conditional effect toggle (true->false) | 0 alloc | Inner effect pooled, `GC.GetAllocatedBytesForCurrentThread()` delta = 0 |
| Component pool rent | 0 alloc | `GC.GetAllocatedBytesForCurrentThread()` delta = 0 (warm pool) |
| Component pool return | 0 alloc | `GC.GetAllocatedBytesForCurrentThread()` delta = 0 |
| IntStringCache.Get (cached range) | 0 alloc | `GC.GetAllocatedBytesForCurrentThread()` delta = 0 |
| Batch.FlushPending (100 signals) | < 0.1 ms | Profiler marker timing |
| EffectRunner.RunDirtyEffects (100 effects) | < 0.5 ms | Profiler marker timing |

## Source Generator Notes

### PoolableGenerator

- **Trigger**: `[Poolable]` attribute on a `partial class` inheriting from `Component`.
- **Validation**: Class must be `partial`. Class must have a parameterless constructor. `MaxPoolSize` must be > 0.
- **Output**: Modifies the generated mount/unmount code to use `ComponentPool<T>.Rent()` and `ComponentPool<T>.Return()`. Emits a `partial void ResetForPool()` declaration for the developer to optionally implement.
- **Diagnostic**: `JUI090` (Error) if class is not partial. `JUI091` (Warning) if MaxPoolSize is unusually large (> 256).

### PoolWarmGenerator

- **Trigger**: `[PoolWarm]` attribute on any `partial class` inheriting from `Component`.
- **Validation**: Effects and Bindings counts must be >= 0. If the class also has `[Poolable]`, the Components count defaults to `MaxPoolSize`.
- **Output**: Emits `JUIAutoWarmup` class (one per assembly) with `WarmAll()` method that calls `ComponentPool<T>.Warm()`, `EffectPool<TState>.Warm()`, and `BindingPool.Warm()` for each annotated type.
- **Incremental**: Uses `ForAttributeWithMetadataName` and aggregates all annotated types into a single output file.
- **Diagnostic**: `JUI092` (Warning) if `[PoolWarm]` is used without `[Poolable]` (components won't be pooled, only effects and bindings).

### Complete Diagnostic Code Table

| ID | Generator | Severity | Message |
|----|-----------|----------|---------|
| JUI001 | ElementGenerator | Warning | Element name attribute is empty string |
| JUI002 | ElementGenerator | Warning | Duplicate element name '{0}' in UXML file |
| JUI003 | ElementGenerator | Error | UXML file '{0}' referenced in [UIComponent] not found |
| JUI004 | ElementGenerator | Error | UXML file '{0}' contains malformed XML: {1} |
| JUI005 | ElementGenerator | Warning | Element type '{0}' could not be resolved to a known UI Toolkit type |
| JUI006 | ElementGenerator | Error | Element name '{0}' maps to C# keyword after PascalCase conversion |
| JUI007 | ElementGenerator | Error | Element name '{0}' contains characters invalid for a C# identifier |
| JUI008 | ElementGenerator | Warning | UXML namespace prefix '{0}' could not be resolved |
| JUI020 | StyleGenerator | Error | USS file '{0}' referenced in [UIComponent] not found |
| JUI021 | StyleGenerator | Error | USS file '{0}' contains syntax errors: {1} |
| JUI022 | StyleGenerator | Warning | Duplicate class name '{0}' in USS file |
| JUI023 | StyleGenerator | Error | CSS variable name '{0}' maps to C# keyword after conversion |
| JUI024 | StyleGenerator | Info | CSS variable '{0}' value type could not be inferred; defaulting to String |
| JUI025 | StyleGenerator | Error | Class name '{0}' contains characters invalid for a C# identifier |
| JUI040 | InjectGenerator | Warning | [Inject] type '{0}' not registered in any ancestor scope |
| JUI041 | InjectGenerator | Error | [Inject] applied to non-reference type '{0}' (value types not supported) |
| JUI042 | InjectGenerator | Info | [Inject] field '{0}' is not readonly; consider making it readonly |
| JUI043 | InjectGenerator | Error | [Inject] applied to static member '{0}' |
| JUI044 | InjectGenerator | Error | [Inject] used in class '{0}' which does not extend Component |
| JUI045 | InjectGenerator | Warning | Duplicate [Inject] for type '{0}' in class '{1}' |
| JUI060 | BindingGenerator | Error | [Bind] references element '{0}' not found in El constants |
| JUI061 | BindingGenerator | Error | [Bind] converter type '{0}' does not implement IValueConverter |
| JUI062 | BindingGenerator | Error | [Bind] signal type '{0}' incompatible with element property type '{1}' |
| JUI063 | BindingGenerator | Error | [BindSync] applied to read-only signal '{0}' (Sync requires read-write) |
| JUI064 | BindingGenerator | Error | [Bind] applied to '{0}' which is not a signal field |
| JUI065 | BindingGenerator | Error | [Bind] mode is Pull but no callback method specified |
| JUI066 | BindingGenerator | Warning | [BindSync] on type '{0}' which does not support two-way binding |
| JUI080 | EffectGenerator | Error | [Effect] Watch references '{0}' which is not a signal |
| JUI081 | EffectGenerator | Error | [Effect] When references '{0}' which is not a boolean signal |
| JUI082 | EffectGenerator | Error | [Effect] method '{0}' has parameters (must be parameterless) |
| JUI083 | EffectGenerator | Warning | [Effect] Watch array is empty; effect will never run |
| JUI084 | EffectGenerator | Error | [Effect] applied to static method '{0}' |
| JUI085 | EffectGenerator | Error | [Effect] in class '{0}' which does not extend Component |
| JUI086 | EffectGenerator | Error | [Effect] When and WhenAll/WhenAny are mutually exclusive on '{0}' |
| JUI087 | EffectGenerator | Error | [Effect] WhenAll/WhenAny references '{0}' which is not a boolean signal |
| JUI090 | PoolableGenerator | Error | [Poolable] class '{0}' must be partial |
| JUI091 | PoolableGenerator | Warning | [Poolable] MaxPoolSize {0} is unusually large (> 256) |
| JUI092 | PoolWarmGenerator | Warning | [PoolWarm] on '{0}' without [Poolable]; only effects and bindings will be warmed |
| JUI100 | ValidateGenerator | Error | [UIComponent] attribute missing UXML path argument |
| JUI101 | ValidateGenerator | Error | Class '{0}' with generator attributes is not marked partial |
| JUI102 | ValidateGenerator | Error | Class '{0}' with generator attributes does not inherit Component |
| JUI103 | ValidateGenerator | Warning | Generated code would conflict with existing member '{0}' |
| JUI104 | ValidateGenerator | Error | Multiple [UIComponent] attributes on class '{0}' |
| JUI105 | ValidateGenerator | Error | Additional file '{0}' could not be loaded by the generator |
| JUI106 | ValidateGenerator | Warning | Source generator version mismatch: generator {0}, runtime {1} |
| JUI107 | ValidateGenerator | Info | Generated partial method '{0}' has no user implementation |
| JUI108 | ValidateGenerator | Warning | UXML path '{0}' uses backslashes (not portable) |
| JUI109 | ValidateGenerator | Error | Attribute argument is null or whitespace |
| JUI110 | ValidateGenerator | Error | Validation rule '{0}' is incompatible with target member type '{1}' |
| JUI200 | NamingAnalyzer | Warning | Element name '{0}' should be kebab-case |
| JUI201 | NamingAnalyzer | Info | Element name '{0}' should follow {purpose}-{type} pattern |
| JUI202 | NamingAnalyzer | Warning | Element name '{0}' is too generic |
| JUI203 | NamingAnalyzer | Warning | USS class '{0}' should follow 'jui-{component}' convention |
| JUI204 | NamingAnalyzer | Info | Component class '{0}' should have a role suffix |
| JUI205 | NamingAnalyzer | Warning | Signal field '{0}' should follow _camelCase convention |
| JUI206 | NamingAnalyzer | Warning | Event struct '{0}' should end with 'Event' suffix |

## Usage Examples

### Poolable Component

```csharp
[Poolable(maxPoolSize: 32)]
[UIComponent("BuffIcon.uxml")]
public partial class BuffIcon : Component
{
    private Signal<Sprite> _icon = new(null);
    private Signal<float> _duration = new(0f);
    private Signal<int> _stacks = new(1);

    protected override void OnMount()
    {
        // Called on each rent -- set up fresh state
        // Signals still hold values from previous use,
        // but they'll be overwritten by the caller.
    }

    protected override void OnUnmount()
    {
        // Called before return -- clean up subscriptions
    }

    // Optional: clear non-signal state before returning to pool
    partial void ResetForPool()
    {
        // Clear any cached references that shouldn't persist
    }
}

// Usage: Show rents from pool automatically
Show<BuffIcon>(when: showBuff, configure: icon =>
{
    icon._icon.Value = buffSprite;
    icon._duration.Value = 5f;
    icon._stacks.Value = 3;
});
// When showBuff becomes false, BuffIcon is returned to pool (not GC'd)
```

### Pool Warmup

```csharp
[Poolable(maxPoolSize: 32)]
[PoolWarm(effects: 16, bindings: 32)]
[UIComponent("BuffIcon.uxml")]
public partial class BuffIcon : Component { /* ... */ }

[Poolable(maxPoolSize: 8)]
[PoolWarm(effects: 8, bindings: 16, Components = 2)]
[UIComponent("DamageNumber.uxml")]
public partial class DamageNumber : Component { /* ... */ }

// During loading screen:
public class LoadingScreen : Component
{
    protected override void OnMount()
    {
        // Pre-allocate all pools before gameplay begins
        JUIAutoWarmup.WarmAll();
        // BuffIcon: 32 components, 16 effects, 32 bindings pre-allocated
        // DamageNumber: 2 components, 8 effects, 16 bindings pre-allocated
    }
}
```

### IntStringCache

```csharp
// Before (allocates every frame):
healthLabel.text = currentHealth.ToString();  // allocates ~24 bytes

// After (zero allocation for values -1000 to 10000):
healthLabel.text = IntStringCache.Get(currentHealth);  // cached string

// In an effect:
Effect(static (state) =>
{
    state.label.text = IntStringCache.Get(state.health.Value);
}, (label: healthLabel, health: _health));
// Zero allocation per frame, even when health changes every frame
```

### IntStringCache with Formatted Strings

```csharp
// For formatted strings, combine IntStringCache with RichTextBuilder:
Span<char> buf = stackalloc char[64];
var text = new RichTextBuilder(buf)
    .Plain("HP: ")
    .Color(IntStringCache.Get(health.Value), healthColor)
    .Plain("/")
    .Plain(IntStringCache.Get(maxHealth.Value))
    .Build();
// Only Build() allocates (one string). No int.ToString() allocations.
```

### Performance Contract Verification Test

```csharp
[Test]
public void EffectRerun_WarmPool_ZeroAlloc()
{
    // Setup
    var signal = new Signal<int>(0);
    var label = new Label();
    var fx = EffectPool<(Signal<int> s, Label l)>.Rent(
        static (state) => state.l.text = IntStringCache.Get(state.s.Value),
        (s: signal, l: label));
    fx.Run(); // initial run, warms caches

    // Measure
    long before = GC.GetAllocatedBytesForCurrentThread();
    for (int i = 0; i < 1000; i++)
    {
        signal.Value = i % 100; // stay in cached range
        Batch.FlushPending();
        EffectRunner.RunDirtyEffects();
    }
    long after = GC.GetAllocatedBytesForCurrentThread();

    // Assert
    Assert.AreEqual(0, after - before, "Effect re-run must be zero-alloc");
}

[Test]
public void EventPublish_ZeroAlloc()
{
    var counter = 0;
    var sub = DamageEvent.Subscribe<int>(
        static (in DamageEvent evt, int state) => { /* no-op */ }, 0);

    long before = GC.GetAllocatedBytesForCurrentThread();
    for (int i = 0; i < 10000; i++)
    {
        DamageEvent.Publish(new DamageEvent { Amount = i, Position = Vector3.zero });
    }
    long after = GC.GetAllocatedBytesForCurrentThread();

    sub.Dispose();
    Assert.AreEqual(0, after - before, "Event publish must be zero-alloc");
}

[Test]
public void Mount100Components_Under2ms()
{
    // Warm pool first
    ComponentPool<BuffIcon>.Warm(100);

    var sw = Stopwatch.StartNew();
    var components = new List<BuffIcon>(100);
    for (int i = 0; i < 100; i++)
    {
        components.Add(JUIManager.Instance.Mount<BuffIcon>(UILayer.HUD));
    }
    sw.Stop();

    Assert.Less(sw.Elapsed.TotalMilliseconds, 2.0, "Mount 100 components must be < 2ms");

    // Cleanup
    foreach (var c in components)
        JUIManager.Instance.Unmount(c);
}
```

## Test Plan

### Component Pool Tests

| # | Test | Expectation |
|---|------|-------------|
| 1 | `Rent()` from empty pool | Returns new instance via `new T()` |
| 2 | `Return()` then `Rent()` | Returns the same instance (pool reuse) |
| 3 | `Return()` when pool is at MaxPoolSize | Instance is discarded (not pooled) |
| 4 | `Return()` same instance twice | Second call is no-op (guard flag) |
| 5 | `Warm(10)` on empty pool | `AvailableCount` becomes 10 |
| 6 | `Clear()` empties pool | `AvailableCount` becomes 0 |
| 7 | `Rent()` returns instance with `_isPooled = false` | Instance is ready for use |
| 8 | Pool respects MaxPoolSize cap | Cannot exceed MaxPoolSize instances in pool |

### PoolWarm Tests

| # | Test | Expectation |
|---|------|-------------|
| 9 | `JUIAutoWarmup.WarmAll()` pre-allocates components | `ComponentPool<T>.AvailableCount` matches expected count |
| 10 | `JUIAutoWarmup.WarmAll()` pre-allocates effects | `EffectPool<TState>.AvailableCount` matches expected count |
| 11 | `WarmAll()` is idempotent | Calling twice does not double allocations |

### IntStringCache Tests

| # | Test | Expectation |
|---|------|-------------|
| 12 | `Get(0)` returns "0" | Correct string value |
| 13 | `Get(42)` returns "42" | Correct string value |
| 14 | `Get(-500)` returns "-500" | Correct string value |
| 15 | `Get(10000)` returns "10000" | Correct string value (boundary) |
| 16 | `Get(-1000)` returns "-1000" | Correct string value (boundary) |
| 17 | `Get(42)` returns same string instance on repeated calls | `ReferenceEquals` is true |
| 18 | `Get(10001)` falls back to `ToString()` | Correct value but not reference-equal |
| 19 | `IsCached(5000)` returns true | Within range |
| 20 | `IsCached(20000)` returns false | Outside range |

### [Poolable] Source Generator Tests

| # | Test | Expectation |
|---|------|-------------|
| 21 | `[Poolable]` on non-partial class | JUI090 error emitted |
| 22 | `[Poolable]` on partial Component subclass | Code generates without errors |
| 23 | Generated mount code uses `ComponentPool<T>.Rent()` | Source inspection confirms |
| 24 | Generated unmount code uses `ComponentPool<T>.Return()` | Source inspection confirms |
| 25 | Generated code includes `ResetForPool()` partial method | Source inspection confirms |
| 26 | `[Poolable(maxPoolSize: 300)]` | JUI091 warning emitted (> 256) |

### [PoolWarm] Source Generator Tests

| # | Test | Expectation |
|---|------|-------------|
| 27 | `[PoolWarm]` without `[Poolable]` | JUI092 warning emitted |
| 28 | `[PoolWarm]` with `[Poolable]` | No warning, WarmAll() generated |
| 29 | Multiple `[PoolWarm]` classes | Single `JUIAutoWarmup` class with all warm calls |

### Performance Contract Tests

| # | Test | Expectation |
|---|------|-------------|
| 30 | Effect re-run (warm pool) | 0 bytes allocated over 1000 iterations |
| 31 | Binding update (push mode) | 0 bytes allocated over 1000 iterations |
| 32 | Signal set (new value) | 0 bytes allocated over 1000 iterations |
| 33 | Signal set (same value, skip) | 0 bytes allocated over 1000 iterations |
| 34 | Event publish | 0 bytes allocated over 10000 iterations |
| 35 | Component pool rent (warm) | 0 bytes allocated |
| 36 | Component pool return | 0 bytes allocated |
| 37 | IntStringCache.Get (cached range) | 0 bytes allocated over 10000 calls |
| 38 | Mount 100 components (warm pool) | < 2 ms |
| 39 | Gesture tick (10 recognizers) | < 0.05 ms |
| 40 | VirtualList 100k scroll | < 0.5 ms, 0 alloc |

### Diagnostic Code Completeness Tests

| # | Test | Expectation |
|---|------|-------------|
| 41 | All diagnostic IDs JUI001-JUI110 are unique | No duplicate IDs |
| 42 | All diagnostic IDs JUI200-JUI206 are unique | No duplicate IDs |
| 43 | Each diagnostic has correct severity (Error, Warning, Info) | Matches table above |
| 44 | Each diagnostic has a non-empty message format string | No empty messages |
| 45 | Error-severity diagnostics prevent code generation | Affected class not emitted |

## Acceptance Criteria

- [ ] `[Poolable]` attribute marks components for pool-based rent/return instead of new/discard
- [ ] `ComponentPool<T>` provides `Rent()`, `Return()`, `Warm()`, and `Clear()` with configurable `MaxSize`
- [ ] `ComponentPool<T>.Return()` calls generated `ResetForPool()` partial method before pooling
- [ ] `ComponentPool<T>.Return()` discards instances when pool is at MaxPoolSize
- [ ] Double-return is a safe no-op (guard flag prevents double-pooling)
- [ ] `[PoolWarm]` attribute declares pre-allocation counts for effects, bindings, and components
- [ ] `PoolWarmGenerator` emits `JUIAutoWarmup.WarmAll()` covering all annotated components
- [ ] `JUIAutoWarmup.WarmAll()` pre-allocates component, effect, and binding pools
- [ ] `IntStringCache.Get()` returns cached string for values in [-1000, 10000] with zero allocation
- [ ] `IntStringCache.Get()` returns same string instance for same integer value (`ReferenceEquals`)
- [ ] `IntStringCache.Get()` falls back to `int.ToString()` for values outside cached range
- [ ] `Show` and `Switch` generated code uses pool rent/return for `[Poolable]` components
- [ ] All performance contracts pass: zero-alloc hot paths for effects, bindings, signals, events, pool operations, and IntStringCache
- [ ] Mount 100 components from warm pool completes in < 2 ms
- [ ] All diagnostic codes JUI001-JUI110 and JUI200-JUI206 are unique with correct severities
- [ ] `PoolableGenerator` emits JUI090 (Error) for non-partial classes and JUI091 (Warning) for oversized pools
- [ ] `PoolWarmGenerator` emits JUI092 (Warning) when `[PoolWarm]` is used without `[Poolable]`
- [ ] All public APIs have XML documentation
- [ ] Performance contract tests are automated and run as part of the test suite
