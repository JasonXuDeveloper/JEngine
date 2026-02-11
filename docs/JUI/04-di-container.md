# Section 4 — DI Container & UI Layer Manager

## Overview

ProviderScope is a hierarchical dependency injection container that supports parent-chain resolution. UILayer is an enum defining the standard UI sort-order tiers, and UILayerManager provides lazy UIDocument creation per layer. Together they form the foundation for scoped service resolution in the component tree and deterministic rendering order across UI layers.

## Dependencies

- Section 1 — Reactive Primitives: Signal & Computed
- Section 2 — Effect System & Batch
- Section 3 — Reactive Collections

## File Structure

- `Runtime/JUI/Core/ProviderScope.cs`
- `Runtime/JUI/Core/UILayer.cs`
- `Runtime/JUI/Core/UILayerManager.cs`

## API Design

```csharp
/// <summary>
/// Hierarchical dependency injection scope. Each scope can store providers
/// and optionally delegates resolution to a parent scope when a provider
/// is not found locally. Components create child scopes automatically,
/// forming a DI tree that mirrors the component tree.
/// </summary>
public sealed class ProviderScope : IDisposable
{
    /// <summary>
    /// Creates a new scope. If <paramref name="parent"/> is provided,
    /// resolution walks up the parent chain when a local provider is not found.
    /// </summary>
    /// <param name="parent">Optional parent scope for fallback resolution.</param>
    public ProviderScope(ProviderScope parent = null);

    /// <summary>
    /// Registers a service instance in this scope. Overwrites any existing
    /// registration for the same type <typeparamref name="T"/> in this scope
    /// (does NOT affect parent scopes).
    /// </summary>
    /// <typeparam name="T">The service type (must be a reference type).</typeparam>
    /// <param name="instance">The service instance to register. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="instance"/> is null.</exception>
    public void Provide<T>(T instance) where T : class;

    /// <summary>
    /// Resolves a service by type. Searches this scope first, then walks the
    /// parent chain. Throws if the service is not found anywhere in the chain.
    /// </summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <returns>The resolved service instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no provider for <typeparamref name="T"/> is found in this scope or any ancestor.
    /// </exception>
    public T Use<T>() where T : class;

    /// <summary>
    /// Attempts to resolve a service by type without throwing.
    /// Searches this scope first, then walks the parent chain.
    /// </summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <param name="value">The resolved service, or null if not found.</param>
    /// <returns>True if the service was found; false otherwise.</returns>
    public bool TryUse<T>(out T value) where T : class;

    /// <summary>
    /// Clears all providers registered in this scope.
    /// Does NOT affect the parent scope.
    /// </summary>
    public void Dispose();
}

/// <summary>
/// Defines the standard UI layer sort-order tiers. Each layer gets its own
/// UIDocument with sortingOrder set to the enum's integer value, ensuring
/// deterministic rendering order across all UI surfaces.
/// </summary>
public enum UILayer
{
    /// <summary>Fullscreen backgrounds, skyboxes, ambient UI behind everything.</summary>
    Background = 0,

    /// <summary>Heads-up display: health bars, minimaps, score counters.</summary>
    HUD = 100,

    /// <summary>Popups: inventory panels, shop windows, dialog boxes.</summary>
    Popup = 200,

    /// <summary>Modal dialogs that block interaction with layers below.</summary>
    Modal = 300,

    /// <summary>Tooltips shown on hover or long-press.</summary>
    Tooltip = 400,

    /// <summary>Right-click or long-press context menus.</summary>
    ContextMenu = 500,

    /// <summary>Toast notifications and system messages.</summary>
    Notification = 600,

    /// <summary>Debug overlays (FPS counter, signal debugger, console).</summary>
    Debug = 900,

    /// <summary>Topmost overlay: loading screens, fade transitions, screen captures.</summary>
    Overlay = 1000
}

/// <summary>
/// Manages one UIDocument per UILayer, creating them lazily on first access.
/// Each UIDocument's sortingOrder is set to the integer value of the UILayer enum,
/// guaranteeing correct front-to-back ordering without manual configuration.
/// </summary>
public sealed class UILayerManager : IDisposable
{
    /// <summary>
    /// Gets the root VisualElement for the specified layer. If no UIDocument
    /// exists for this layer yet, one is created automatically with
    /// sortingOrder set to <c>(int)layer</c>.
    /// </summary>
    /// <param name="layer">The UI layer to retrieve.</param>
    /// <returns>The root VisualElement of the layer's UIDocument.</returns>
    public VisualElement GetLayerRoot(UILayer layer);

    /// <summary>
    /// Returns a read-only collection of all layers that have been accessed
    /// (i.e., whose UIDocuments have been created). Layers that have never
    /// been requested via <see cref="GetLayerRoot"/> are not included.
    /// </summary>
    public ReadOnlyCollection<UILayer> ActiveLayers { get; }

    /// <summary>
    /// Destroys all UIDocuments created by this manager and clears internal state.
    /// </summary>
    public void Dispose();
}
```

## Data Structures

- `ProviderScope._providers` -- `Dictionary<Type, object>` storing type-keyed registrations for this scope.
- `ProviderScope._parent` -- nullable reference to the parent scope for chain resolution.
- `UILayerManager._layerDocuments` -- `Dictionary<UILayer, UIDocument>` mapping each accessed layer to its lazily created UIDocument.
- `UILayerManager._activeLayers` -- `List<UILayer>` backing list for the `ActiveLayers` read-only view, kept sorted by enum value on insertion.
- `UILayerManager._activeLayersReadOnly` -- `ReadOnlyCollection<UILayer>` wrapping `_activeLayers` via `AsReadOnly()`, cached once.

## Implementation Notes

- **Parent chain resolution**: `Use<T>()` checks `_providers` for `typeof(T)`. On miss, recursively calls `_parent.Use<T>()`. If `_parent` is null and the key is not found, throws `InvalidOperationException` with a message naming the missing type.
- **TryUse short-circuit**: `TryUse<T>()` follows the same chain but returns `false` at the root instead of throwing. No exceptions on the miss path.
- **Provide overwrites locally**: Calling `Provide<T>()` with a type that already exists in the same scope overwrites the previous registration. This enables child components to shadow parent-provided services.
- **Dispose semantics**: `ProviderScope.Dispose()` clears `_providers` but does NOT dispose the parent scope. Each scope is owned by its creating component and disposed in the component's disposal chain.
- **Lazy UIDocument creation**: `GetLayerRoot()` checks `_layerDocuments`. On miss, it creates a new `GameObject` with a `UIDocument` component, sets `UIDocument.sortingOrder = (int)layer`, names the GameObject `"JUI-Layer-{layer}"`, and marks it `DontDestroyOnLoad`. The root `VisualElement` is returned.
- **ActiveLayers ordering**: When a new layer is created, it is inserted into `_activeLayers` in sorted order (by integer value) using binary search (`BinarySearch` + `Insert`). The `ReadOnlyCollection` wrapper reflects this automatically.
- **Thread safety**: Both `ProviderScope` and `UILayerManager` are main-thread only. No locking is required.

## Source Generator Notes

Source generators interact with `ProviderScope` in two ways (detailed in Section 11):

1. **`[Inject]` attribute**: The `InjectGenerator` emits code in `InitScope()` that calls `Scope.Use<T>()` to resolve dependencies and assign them to annotated fields/properties.
2. **`[ProvideBridge]` attribute**: The generator emits `Scope.Provide<T>(instance)` calls in `InitScope()` for services that this component exposes to its children.

No source generation is required for `UILayer` or `UILayerManager` themselves.

## Usage Examples

```csharp
// --- Basic scope chain ---
var root = new ProviderScope();
var child = new ProviderScope(parent: root);

root.Provide<ILogger>(new ConsoleLogger());
var logger = child.Use<ILogger>(); // Resolves from root

// --- Child shadows parent ---
child.Provide<ILogger>(new FileLogger());
var childLogger = child.Use<ILogger>();  // FileLogger (local)
var rootLogger = root.Use<ILogger>();    // ConsoleLogger (unchanged)

// --- Safe resolution ---
if (child.TryUse<IAudioService>(out var audio))
{
    audio.PlayClick();
}
// No exception if IAudioService was never provided

// --- Missing service throws ---
try
{
    child.Use<IMissingService>(); // throws InvalidOperationException
}
catch (InvalidOperationException ex)
{
    Debug.LogWarning(ex.Message);
    // "No provider found for type 'IMissingService' in scope chain."
}

// --- UILayerManager usage ---
var layerManager = new UILayerManager();

// First access creates the UIDocument with sortingOrder = 100
VisualElement hudRoot = layerManager.GetLayerRoot(UILayer.HUD);
hudRoot.Add(new Label("Health: 100"));

// Modal layer renders above HUD (sortingOrder = 300)
VisualElement modalRoot = layerManager.GetLayerRoot(UILayer.Modal);
modalRoot.Add(new Label("Are you sure?"));

// Only HUD and Modal are active
ReadOnlyCollection<UILayer> active = layerManager.ActiveLayers;
// active = [UILayer.HUD, UILayer.Modal]
```

## Test Plan

1. **Scope chain resolution (child to parent to grandparent)**: Register a provider in the grandparent scope, resolve from the grandchild scope, verify the correct instance is returned.
2. **Child overrides parent provider**: Register the same type in both parent and child scopes, resolve from child, verify the child's instance is returned. Resolve from parent, verify the parent's instance is unchanged.
3. **TryUse returns false for missing provider**: Call `TryUse<T>()` for a type never registered in any scope, verify it returns false and `out` parameter is null.
4. **Use throws InvalidOperationException for missing provider**: Call `Use<T>()` for a type never registered, verify `InvalidOperationException` is thrown with a descriptive message.
5. **UILayer ordering matches enum integer values**: Verify that `(int)UILayer.Background == 0`, `(int)UILayer.HUD == 100`, ..., `(int)UILayer.Overlay == 1000`.
6. **Lazy UIDocument creation on first GetLayerRoot call**: Call `GetLayerRoot(UILayer.HUD)` for the first time, verify a UIDocument is created. Call again, verify the same UIDocument is returned (no duplicate creation).
7. **ActiveLayers reflects only accessed layers**: Access HUD and Modal layers, verify `ActiveLayers` contains exactly those two in sorted order. Verify layers never accessed are absent.
8. **ActiveLayers sorted by enum value**: Access layers in reverse order (Overlay, then HUD), verify `ActiveLayers` returns them sorted ascending by integer value.
9. **Provide with null throws ArgumentNullException**: Call `Provide<T>(null)`, verify `ArgumentNullException` is thrown.
10. **Dispose clears providers but not parent**: Dispose a child scope, verify `Use<T>()` on the child now throws (or `TryUse` returns false), but the parent scope is unaffected.
11. **UILayerManager.Dispose destroys UIDocuments**: Dispose the manager, verify all created GameObjects/UIDocuments are destroyed and `ActiveLayers` is empty.

## Acceptance Criteria

- [ ] `ProviderScope` implements `IDisposable`
- [ ] `Provide<T>()` stores instance keyed by `typeof(T)`, rejects null
- [ ] `Use<T>()` walks parent chain and throws `InvalidOperationException` on miss
- [ ] `TryUse<T>()` walks parent chain and returns false on miss (no exceptions)
- [ ] Child scope can shadow a parent-provided service without affecting the parent
- [ ] `UILayer` enum values are spaced for extensibility (0, 100, 200, ..., 1000)
- [ ] `UILayerManager.GetLayerRoot()` lazily creates one UIDocument per layer
- [ ] UIDocument `sortingOrder` equals `(int)UILayer` for correct rendering order
- [ ] `ActiveLayers` returns a read-only, sorted view of accessed layers only
- [ ] `UILayerManager.Dispose()` destroys all created GameObjects and UIDocuments
- [ ] All public APIs have XML documentation
- [ ] Zero allocations on `Use<T>()` / `TryUse<T>()` calls after initial setup
