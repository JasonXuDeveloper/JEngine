# Section 14 — Bridge System

## Overview

The Bridge System provides source-generated interop between plain C# model objects (POCOs) and JUI's reactive signal layer. `[ReactiveBridge(typeof(Model))]` on a `partial class` generates one `Signal<T>` per public property of the model, along with `Bind(model)`, `Sync()` (model to signals), `WriteBack()` (signals to model), and `Dispose()` methods. Bridges are pooled via static `Rent()`/`Return()` to eliminate allocation on repeated use. `[ProvideBridge(typeof(Model))]` on a field in a Component creates the bridge, registers it in the component's DI scope for child injection, and optionally enables automatic per-frame `Sync()` polling via the `AutoSync` flag.

## Dependencies

- Section 9 (Source Generator Project Setup)
- Section 4 (DI Container & UI Layer Manager)
- Section 1 (Reactive Primitives: Signal & Computed)

## File Structure

- `SourceGenerators/JEngine.JUI.Generators/BridgeGenerator.cs`
- `SourceGenerators/JEngine.JUI.Generators/ProvideBridgeGenerator.cs`
- `Runtime/JUI/Attributes/ReactiveBridgeAttribute.cs`
- `Runtime/JUI/Attributes/ProvideBridgeAttribute.cs`

## API Design

### Attributes

```csharp
/// <summary>
/// Marks a partial class as a reactive bridge for the specified model type.
/// The source generator emits one <see cref="Signal{T}"/> per public property
/// of the model, along with Bind, Sync, WriteBack, Dispose, Rent, and Return methods.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ReactiveBridgeAttribute : Attribute
{
    /// <summary>
    /// The model type to bridge. All public instance properties with a getter
    /// are reflected into signals. Properties without a setter are read-only
    /// (their signals are populated by Sync but WriteBack skips them).
    /// </summary>
    public Type ModelType { get; }

    public ReactiveBridgeAttribute(Type modelType);
}

/// <summary>
/// Marks a field in a Component as a bridge that should be automatically created,
/// bound to its model, and registered in the component's DI scope so child
/// components can inject it.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class ProvideBridgeAttribute : Attribute
{
    /// <summary>
    /// The model type this bridge wraps. Must match the type argument of the
    /// <see cref="ReactiveBridgeAttribute"/> on the bridge class.
    /// </summary>
    public Type ModelType { get; }

    /// <summary>
    /// When <c>true</c>, the generated code registers this bridge with
    /// <c>BindingEngine.Tick</c> so that <see cref="Sync"/> is called automatically
    /// each frame, polling the model for changes. Defaults to <c>false</c>.
    /// </summary>
    public bool AutoSync { get; set; } = false;

    public ProvideBridgeAttribute(Type modelType);
}
```

### Generated Code (emitted by BridgeGenerator)

```csharp
// Developer writes:
public class PlayerModel
{
    public int Health { get; set; }
    public string Name { get; set; }
    public float Speed { get; }  // read-only (no setter)
}

[ReactiveBridge(typeof(PlayerModel))]
public partial class PlayerBridge { }

// BridgeGenerator emits:
public partial class PlayerBridge : IDisposable
{
    // --- Signals (one per public property) ---
    private Signal<int> _health;
    private Signal<string> _name;
    private Signal<float> _speed;

    /// <summary>Reactive signal mirroring <see cref="PlayerModel.Health"/>.</summary>
    public Signal<int> Health => _health;

    /// <summary>Reactive signal mirroring <see cref="PlayerModel.Name"/>.</summary>
    public Signal<string> Name => _name;

    /// <summary>
    /// Reactive signal mirroring <see cref="PlayerModel.Speed"/>.
    /// Read-only: <see cref="WriteBack"/> does not write this property back to the model.
    /// </summary>
    public Signal<float> Speed => _speed;

    // --- Bound model reference ---
    private PlayerModel _model;

    /// <summary>
    /// Binds this bridge to a model instance. Initializes all signals with the
    /// model's current property values. Must be called before <see cref="Sync"/>
    /// or <see cref="WriteBack"/>.
    /// </summary>
    /// <param name="model">The model instance to bridge. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="model"/> is null.</exception>
    public void Bind(PlayerModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _health.Value = model.Health;
        _name.Value = model.Name;
        _speed.Value = model.Speed;
    }

    /// <summary>
    /// Reads all properties from the bound model and pushes their values into the
    /// corresponding signals. Use this to pull external changes into the reactive layer.
    /// Signals only notify subscribers if the value actually changed (equality check).
    /// </summary>
    public void Sync()
    {
        _health.Value = _model.Health;
        _name.Value = _model.Name;
        _speed.Value = _model.Speed;
    }

    /// <summary>
    /// Writes all writable signal values back to the bound model's properties.
    /// Read-only properties (no setter) are skipped. Use this to push UI changes
    /// back to the data layer.
    /// </summary>
    public void WriteBack()
    {
        _model.Health = _health.Value;
        _model.Name = _name.Value;
        // _model.Speed is read-only — skipped
    }

    /// <summary>
    /// Disposes all signals and clears the model reference. If this bridge was
    /// rented from the pool, call <see cref="Return"/> instead of Dispose directly.
    /// </summary>
    public void Dispose()
    {
        _health?.Dispose();
        _name?.Dispose();
        _speed?.Dispose();
        _model = null;
    }

    // --- Object pool ---
    private static readonly Stack<PlayerBridge> _pool = new(4);

    /// <summary>
    /// Rents a bridge instance from the pool. If the pool is empty, a new instance
    /// is allocated with fresh signals.
    /// </summary>
    /// <returns>A ready-to-use bridge instance. Call <see cref="Bind"/> before use.</returns>
    public static PlayerBridge Rent()
    {
        if (_pool.TryPop(out var bridge))
            return bridge;

        return new PlayerBridge
        {
            _health = new Signal<int>(default),
            _name = new Signal<string>(default),
            _speed = new Signal<float>(default),
        };
    }

    /// <summary>
    /// Returns a bridge instance to the pool for reuse. Calls <see cref="Dispose"/>
    /// to clear state before pooling. The bridge can be rented again via <see cref="Rent"/>.
    /// </summary>
    /// <param name="bridge">The bridge to return.</param>
    public static void Return(PlayerBridge bridge)
    {
        bridge.Dispose();
        // Re-create signals for next Rent (signals are disposed, need fresh ones)
        bridge._health = new Signal<int>(default);
        bridge._name = new Signal<string>(default);
        bridge._speed = new Signal<float>(default);
        _pool.Push(bridge);
    }
}
```

### Generated Code (emitted by ProvideBridgeGenerator)

```csharp
// Developer writes:
public partial class PlayerHUD : Component
{
    [ProvideBridge(typeof(PlayerModel), AutoSync = true)]
    private PlayerBridge _player;

    private PlayerModel _model; // set externally or injected
}

// ProvideBridgeGenerator emits in the partial class:
public partial class PlayerHUD
{
    // Called during component initialization (InitScope):
    partial void InitBridges()
    {
        _player = PlayerBridge.Rent();
        _player.Bind(_model);
        Scope.Provide<PlayerBridge>(_player);

        // AutoSync = true: register with BindingEngine for per-frame Sync
        BindingEngine.RegisterAutoSync(_player, static (b) => b.Sync());
    }

    // Called during component disposal:
    partial void DisposeBridges()
    {
        BindingEngine.UnregisterAutoSync(_player);
        PlayerBridge.Return(_player);
        _player = null;
    }
}
```

### Child Component Injection

```csharp
// A child component injecting the bridge provided by a parent:
public partial class HealthBar : Component
{
    [Inject]
    private PlayerBridge _player;  // resolved from parent's ProviderScope

    protected override void Render()
    {
        // Bind directly to the bridge's signals
        Bind(healthFill, "width", _player.Health,
            static (h) => new StyleLength(new Length(h, LengthUnit.Percent)));
    }
}
```

## Data Structures

| Type | Role |
|------|------|
| `Signal<T>` per property | One signal per public property of the bridged model. Created in `Rent()`, disposed in `Return()`. |
| `TModel _model` | Reference to the bound model instance. Set by `Bind()`, cleared by `Dispose()`. |
| `Stack<TBridge> _pool` | Per-bridge-type static object pool. Starts with capacity 4. |
| `BindingEngine` auto-sync list | Internal list in BindingEngine that stores `(object bridge, Action<object> sync)` tuples. Iterated once per frame when `AutoSync` is enabled. |

## Implementation Notes

- **Signal creation in Rent()**: Signals are created as part of `Rent()`, not in the constructor. This allows the pool to reuse the bridge shell while providing fresh signals for each rental cycle. On `Return()`, old signals are disposed and new ones are allocated in preparation for the next `Rent()`.
- **Sync is equality-checked**: Because `Signal<T>.Value` setter performs equality checks, calling `Sync()` when nothing has changed on the model does not trigger any subscriber notifications. This makes `AutoSync` safe to use every frame without performance concerns.
- **WriteBack skips read-only properties**: The generator inspects each property for a public setter. Properties without a setter (get-only) have their signals populated by `Sync()` but are excluded from `WriteBack()`. The generator emits a comment in the generated code documenting which properties are skipped.
- **AutoSync registration**: When `AutoSync = true`, the `ProvideBridgeGenerator` emits a call to `BindingEngine.RegisterAutoSync()` during `InitBridges()`. The binding engine calls `Sync()` on all registered bridges once per frame, before effects run. This polls the model for external changes (e.g., from game systems that modify the model directly).
- **Pool reuse**: The bridge pool avoids GC pressure for short-lived UI screens that create and destroy bridges frequently (e.g., inventory item tooltips). `Rent()` and `Return()` are O(1).
- **DI scope integration**: `Scope.Provide<TBridge>(bridge)` makes the bridge available to all child components via `[Inject]`. Children do not need to know which parent created the bridge -- they simply request it by type.
- **Thread safety**: Bridges are main-thread only. The pool is not thread-safe.
- **Nested models**: For models with nested object properties (e.g., `PlayerModel.Equipment` of type `EquipmentModel`), the generator creates a `Signal<EquipmentModel>` for the nested property. If fine-grained reactivity is needed for the nested model, create a separate `[ReactiveBridge(typeof(EquipmentModel))]` and compose bridges manually.

## Source Generator Notes

### BridgeGenerator

- **Trigger**: `[ReactiveBridge(typeof(TModel))]` attribute on a `partial class`.
- **Validation**: The class must be `partial`. The model type must have at least one public instance property with a getter. If the model type has no public properties, a diagnostic warning is emitted.
- **Property discovery**: Uses `GetMembers().OfType<IPropertySymbol>()` filtered to public, non-static, non-indexer properties with a getter.
- **Type mapping**: Each property type maps directly to `Signal<PropertyType>`. For value types, the signal stores the value directly. For reference types, the signal stores the reference.
- **Output**: Emits the partial class extension with signals, Bind, Sync, WriteBack, Dispose, Rent, Return, and the static pool field.
- **Incremental**: Uses `ForAttributeWithMetadataName`. Regenerates when the bridge class declaration or the model type's public properties change.

### ProvideBridgeGenerator

- **Trigger**: `[ProvideBridge(typeof(TModel))]` attribute on a field in a `partial class` that inherits from `Component`.
- **Validation**: The field type must be a class decorated with `[ReactiveBridge]`. The field's bridge model type must match the `ProvideBridge` model type attribute argument. The containing class must be `partial` and inherit from `Component`.
- **Output**: Emits `InitBridges()` and `DisposeBridges()` partial methods. If `AutoSync = true`, emits `BindingEngine.RegisterAutoSync` and `BindingEngine.UnregisterAutoSync` calls.
- **Multiple bridges**: If a component has multiple `[ProvideBridge]` fields, all are aggregated into the same `InitBridges()` and `DisposeBridges()` methods.

## Usage Examples

```csharp
// --- Define a model ---
public class CharacterModel
{
    public string Name { get; set; }
    public int Level { get; set; }
    public float HealthPercent { get; set; }
    public bool IsAlive { get; }  // read-only
}

// --- Create a bridge ---
[ReactiveBridge(typeof(CharacterModel))]
public partial class CharacterBridge { }
// Generated: Signal<string> Name, Signal<int> Level,
//            Signal<float> HealthPercent, Signal<bool> IsAlive
//            Bind(), Sync(), WriteBack() (skips IsAlive), Dispose()
//            static Rent(), static Return()

// --- Provide bridge in a parent component ---
public partial class CharacterPanel : Component
{
    private CharacterModel _model;

    [ProvideBridge(typeof(CharacterModel), AutoSync = true)]
    private CharacterBridge _character;

    public CharacterPanel(CharacterModel model)
    {
        _model = model;
    }

    protected override void Render()
    {
        // _character is automatically rented, bound, and provided to scope
        Add(new CharacterHeader());
        Add(new HealthBar());
        Add(new LevelDisplay());
    }
}

// --- Inject bridge in child components ---
public partial class CharacterHeader : Component
{
    [Inject]
    private CharacterBridge _character;

    protected override void Render()
    {
        var nameLabel = El<Label>();
        Bind(nameLabel, "text", _character.Name);
    }
}

public partial class HealthBar : Component
{
    [Inject]
    private CharacterBridge _character;

    protected override void Render()
    {
        var fill = El<VisualElement>("health-fill");
        Bind(fill, "width", _character.HealthPercent,
            static (pct) => new StyleLength(new Length(pct * 100, LengthUnit.Percent)));
    }
}

// --- Manual bridge usage (non-component code) ---
var bridge = CharacterBridge.Rent();
bridge.Bind(someModel);

// Read model state reactively
var fx = EffectPool<CharacterBridge>.Rent(
    static (b) => Debug.Log($"Health: {b.HealthPercent.Value}"),
    bridge);
fx.Run();

// Push UI changes back to model
bridge.HealthPercent.Value = 0.5f;
bridge.WriteBack();
// someModel.HealthPercent is now 0.5f

// Clean up
fx.Dispose();
CharacterBridge.Return(bridge);

// --- Multiple bridges in one component ---
public partial class BattleHUD : Component
{
    [ProvideBridge(typeof(PlayerModel), AutoSync = true)]
    private PlayerBridge _player;

    [ProvideBridge(typeof(EnemyModel), AutoSync = true)]
    private EnemyBridge _enemy;

    // Both bridges are rented, bound, provided, and auto-synced
}
```

## Test Plan

1. **Bridge creates signals matching model properties**: Create a `[ReactiveBridge]` for a model with 3 properties. Verify the bridge exposes 3 signals with matching types.
2. **Bind initializes signals from model**: Create a model with known values, call `Bind(model)`, verify each signal's `Value` matches the model's property.
3. **Sync reads model into signals**: Modify the model's properties directly, call `Sync()`, verify signals reflect the new values.
4. **Sync is equality-checked (no spurious notifications)**: Subscribe an effect to a bridge signal. Call `Sync()` without changing the model. Verify the effect is NOT marked dirty.
5. **WriteBack writes signals to model**: Change bridge signal values, call `WriteBack()`, verify the model's properties match the signal values.
6. **WriteBack skips read-only properties**: Create a bridge for a model with a read-only property. Call `WriteBack()`. Verify no exception is thrown and the read-only property is unchanged.
7. **ProvideBridge is injectable by children**: Create a parent component with `[ProvideBridge]`, add a child component with `[Inject]` on the same bridge type. Verify the child receives the bridge instance.
8. **Pool reuse -- Rent/Return cycle**: Rent a bridge, return it, rent again. Verify the same bridge shell is reused (not a new allocation). Verify signals are fresh.
9. **AutoSync polls model each frame**: Enable `AutoSync`, modify the model externally, advance one frame (simulate BindingEngine.Tick), verify signals are updated.
10. **Dispose clears model reference and signals**: Dispose a bridge, verify all signals are disposed and `_model` is null.
11. **Return disposes and re-pools**: Return a bridge, verify it is in the pool. Rent it again, verify signals are fresh and model is null (requires `Bind` before use).
12. **Multiple ProvideBridge fields in one component**: Create a component with two `[ProvideBridge]` fields. Verify both are rented, bound, and provided independently.
13. **Nested model property creates Signal of nested type**: Create a model with a property of a complex type. Verify the bridge creates `Signal<ComplexType>` for it.

## Acceptance Criteria

- [ ] `[ReactiveBridge(typeof(T))]` generates one `Signal<TProp>` per public property of `T`
- [ ] `Bind(model)` initializes all signals from the model's current state
- [ ] `Sync()` reads model properties into signals with equality-checked updates
- [ ] `WriteBack()` writes signal values back to model, skipping read-only properties
- [ ] `Dispose()` disposes all signals and clears the model reference
- [ ] `Rent()` returns a pooled instance with fresh signals; `Return()` disposes and re-pools
- [ ] `[ProvideBridge]` generates `Rent()`, `Bind()`, and `Scope.Provide<T>()` calls in `InitBridges()`
- [ ] `[ProvideBridge]` generates `Return()` and scope cleanup in `DisposeBridges()`
- [ ] `AutoSync = true` registers the bridge for per-frame `Sync()` via `BindingEngine`
- [ ] Child components can inject bridges via `[Inject]` from the parent's `ProviderScope`
- [ ] Generator emits diagnostic errors for non-partial classes or models with no public properties
- [ ] All public APIs have XML documentation
- [ ] Zero allocations on `Sync()` and `WriteBack()` hot paths
