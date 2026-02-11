# Section 30 — JUIManager (Entry Point)

## Overview

JUIManager is the singleton `MonoBehaviour` that bootstraps and orchestrates every JUI subsystem. It owns a single `Update()` loop with a strict 6-phase execution model where each phase only performs work if something is pending. It provides the `Mount<T>()` API for attaching root components to UI layers, and a root `ProviderScope` for global dependency injection. JUIManager is the first thing initialized and the last thing torn down in any JUI application.

## Dependencies

| Section | What It Provides |
|---------|-----------------|
| 1 — Reactive Primitives | `Signal<T>`, flushed in Phase 1 |
| 2 — Effect System | `Batch`, `EffectRunner`, flushed in Phases 1 and 3 |
| 4 — DI Container | `ProviderScope`, root scope management |
| 5 — Binding System | `BindingEngine`, polled in Phase 2 |
| 6 — Component Base Class | `Component` lifecycle, mounted via `Mount<T>()` |
| 15 — Animation | `AnimationTicker`, ticked via `OnTick` event |
| 17 — Gestures | `GestureEngine`, ticked in Phase 4 |
| 19 — Screen Router | `ScreenRouter`, initialized from root scope |
| 20 — Audio | `UIAudioManager`, flushed in Phase 6 |
| 22 — Focus & Navigation | `FocusNavigator`, ticked in Phase 5 |

This section integrates ALL prior sections. It is the convergence point for the entire framework.

## File Structure

```
Packages/com.jasonxudeveloper.jengine.ui/
└── Runtime/JUI/Core/
    └── JUIManager.cs
```

## API Design

```csharp
namespace JEngine.JUI.Core;

/// <summary>
/// Singleton MonoBehaviour that drives the JUI reactive loop.
/// Bootstraps all subsystems, provides the root DI scope, and
/// manages the component mount lifecycle.
/// </summary>
/// <remarks>
/// JUIManager must be present in the scene for JUI to function.
/// It persists across scene loads via <c>DontDestroyOnLoad</c>.
/// The Update loop executes six phases in a fixed order. Each phase
/// checks for pending work before executing, ensuring zero CPU cost
/// when the UI is idle.
/// </remarks>
public sealed class JUIManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────

    /// <summary>
    /// The singleton instance. Set in <c>Awake()</c>, cleared in <c>OnDestroy()</c>.
    /// Access before Awake or after OnDestroy returns null.
    /// </summary>
    public static JUIManager Instance { get; private set; }

    // ── DI Root Scope ──────────────────────────────────────────────

    /// <summary>
    /// Root provider scope for global DI registrations.
    /// All component scopes are descendants of this scope.
    /// Register global services here (stores, bridges, routers).
    /// </summary>
    public ProviderScope RootScope { get; private set; }

    /// <summary>
    /// Convenience method for registering a service in the root scope.
    /// Equivalent to <c>RootScope.Provide&lt;T&gt;(instance)</c>.
    /// </summary>
    /// <typeparam name="T">The service type to register.</typeparam>
    /// <param name="instance">The service instance.</param>
    public void Provide<T>(T instance) where T : class;

    /// <summary>
    /// Convenience method for resolving a service from the root scope.
    /// Equivalent to <c>RootScope.Use&lt;T&gt;()</c>.
    /// </summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <returns>The resolved service instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no provider of type <typeparamref name="T"/> is registered.
    /// </exception>
    public T Use<T>() where T : class;

    // ── Mount API ──────────────────────────────────────────────────

    /// <summary>
    /// Mounts a root component into the specified UI layer.
    /// The component is instantiated, its scope is chained from <see cref="RootScope"/>,
    /// and its full lifecycle is invoked (OnInit, OnMount, initial effect run).
    /// </summary>
    /// <typeparam name="T">
    /// The component type to mount. Must inherit from <c>Component</c>
    /// and have a parameterless constructor.
    /// </typeparam>
    /// <param name="layer">
    /// The UI layer to mount into. Determines z-order and input priority.
    /// Default is <c>UILayer.HUD</c>.
    /// </param>
    /// <returns>The mounted component instance.</returns>
    public T Mount<T>(UILayer layer = UILayer.HUD) where T : Component, new();

    /// <summary>
    /// Mounts a root component into the specified UI layer with explicit
    /// initial state provided via a configuration action.
    /// </summary>
    /// <typeparam name="T">The component type to mount.</typeparam>
    /// <param name="layer">The UI layer to mount into.</param>
    /// <param name="configure">
    /// Action invoked on the component after construction but before OnMount.
    /// Use this to set initial signal values.
    /// </param>
    /// <returns>The mounted component instance.</returns>
    public T Mount<T>(UILayer layer, Action<T> configure) where T : Component, new();

    /// <summary>
    /// Unmounts a previously mounted root component. Invokes the full
    /// teardown lifecycle (OnUnmount, OnDispose) and removes the component's
    /// visual tree from the UI layer.
    /// </summary>
    /// <param name="component">The component to unmount.</param>
    public void Unmount(Component component);

    /// <summary>
    /// Unmounts all components from the specified layer.
    /// </summary>
    /// <param name="layer">The layer to clear.</param>
    public void UnmountAll(UILayer layer);

    /// <summary>
    /// Returns all currently mounted root components across all layers.
    /// </summary>
    public IReadOnlyList<Component> MountedComponents { get; }

    // ── Tick Event ─────────────────────────────────────────────────

    /// <summary>
    /// Per-frame tick event for components or systems that need manual
    /// per-frame updates. Fires once per frame at the end of the Update
    /// loop with <c>Time.unscaledDeltaTime</c>.
    /// </summary>
    /// <remarks>
    /// This is a rare-use escape hatch. Most UI state should flow through
    /// signals and effects, not per-frame polling. Use this for things like
    /// animation ticking or real-time clock displays.
    /// </remarks>
    public event Action<float> OnTick;

    // ── Lifecycle ──────────────────────────────────────────────────

    /// <summary>
    /// Unity Awake callback. Sets up the singleton, creates the root scope,
    /// initializes all subsystems, and marks this object as DontDestroyOnLoad.
    /// </summary>
    private void Awake();

    /// <summary>
    /// Unity Update callback. Executes the 6-phase reactive loop.
    /// </summary>
    private void Update();

    /// <summary>
    /// Unity OnDestroy callback. Unmounts all components, disposes the root
    /// scope, tears down all subsystems, and clears the singleton reference.
    /// </summary>
    private void OnDestroy();
}
```

### UI Layers

```csharp
namespace JEngine.JUI.Core;

/// <summary>
/// Defines the z-order layers for UI rendering. Higher values render on top.
/// Each layer corresponds to a separate UIDocument with a configured sort order.
/// </summary>
public enum UILayer
{
    /// <summary>Background UI (world-space overlays, environmental UI).</summary>
    Background = 0,

    /// <summary>Main game HUD (health bars, minimap, action bars).</summary>
    HUD = 100,

    /// <summary>Full-screen menus and panels (inventory, settings, map).</summary>
    Screen = 200,

    /// <summary>Modal dialogs (confirmations, alerts, prompts).</summary>
    Modal = 300,

    /// <summary>Overlay UI (tooltips, notifications, loading screens).</summary>
    Overlay = 400,

    /// <summary>System-level UI (debug console, FPS counter). Always on top.</summary>
    System = 500
}
```

### Update Loop — 6-Phase Execution Model

```csharp
private void Update()
{
    // ── Phase 1: Flush Pending Signal Changes ──────────────────
    // Walk all signals that changed since last frame.
    // Mark their subscriber effects as dirty.
    // After this phase, no new signals are pending.
    Batch.FlushPending();

    // ── Phase 2: Poll POCO Dirty-Check Bindings ────────────────
    // For bindings in Pull mode (polling non-signal sources),
    // check if the source value has changed and update the target.
    // This phase is a no-op if no Pull bindings are registered.
    BindingEngine.Tick();

    // ── Phase 3: Run All Dirty Effects ─────────────────────────
    // Execute every effect marked dirty in Phase 1 or Phase 2,
    // in topological dependency order.
    // Signal→UI updates happen here (label.text = signal.Value, etc.)
    // This phase is a no-op if no effects are dirty.
    EffectRunner.RunDirtyEffects();

    // ── Phase 4: Advance Gesture Timers ────────────────────────
    // Tick all active gesture recognizers (LongPress duration,
    // tap interval timeout, etc.) by unscaled delta time.
    // This phase is a no-op if no recognizers are registered.
    GestureEngine.Tick(Time.unscaledDeltaTime);

    // ── Phase 5: Process Focus Changes ─────────────────────────
    // Apply queued focus transitions (Tab, Shift+Tab, gamepad
    // navigation). Updates the focus ring and fires focus events.
    // This phase is a no-op if no focus changes are pending.
    FocusNavigator.Tick();

    // ── Phase 6: Flush Debounced Audio ─────────────────────────
    // Play any audio clips that were requested this frame via
    // UIAudioManager.Play(). Debouncing prevents the same clip
    // from playing multiple times in one frame.
    // This phase is a no-op if no audio plays are pending.
    UIAudioManager.Tick();

    // ── Fire OnTick ────────────────────────────────────────────
    // Notify any subscribers that need per-frame updates.
    OnTick?.Invoke(Time.unscaledDeltaTime);
}
```

## Data Structures

| Type | Purpose |
|------|---------|
| `JUIManager` (MonoBehaviour) | Singleton, owns the Update loop and root scope. Marked `DontDestroyOnLoad`. |
| `ProviderScope` (Section 4) | Hierarchical DI container. Root scope is created in `Awake()`. |
| `UILayer` (enum) | Determines which UIDocument a component is mounted into and its sort order. |
| `Dictionary<UILayer, UIDocument>` | Internal map from layer enum to the UIDocument instance for that layer. Created lazily on first mount to each layer. |
| `List<Component>` | Internal list of all mounted root components, used for `UnmountAll` and `MountedComponents`. |
| `bool _hasPendingWork` flags | Internal per-phase flags checked at the top of each phase to skip no-op work. Set by the subsystems when they have pending items. |

## Implementation Notes

### Singleton Pattern

```csharp
private void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);

    // Initialize subsystems
    RootScope = new ProviderScope(parent: null);
    InitializeUIDocuments();
    InitializeSubsystems();
}

private void OnDestroy()
{
    if (Instance != this) return;

    // Teardown in reverse order
    UnmountAllLayers();
    TeardownSubsystems();
    RootScope?.Dispose();
    RootScope = null;
    Instance = null;
}
```

- If a second JUIManager is instantiated (e.g., from a scene load), it destroys itself immediately.
- `OnDestroy` only runs teardown if this instance is the active singleton. This prevents double-teardown when the duplicate is destroyed.

### Mount Lifecycle

When `Mount<T>(layer)` is called:

1. **Construct**: `var component = new T();`
2. **Create scope**: `component.Scope = new ProviderScope(parent: RootScope);`
3. **Configure** (if configure action provided): `configure(component);`
4. **Resolve UIDocument**: Get or create the UIDocument for the specified layer.
5. **Attach**: Add the component's root `VisualElement` to the UIDocument's root.
6. **Init lifecycle**: Call `component.OnInit()` which triggers:
   - Source-generated `InitScope()` (DI injection)
   - Source-generated `InitBindings()` (binding setup)
   - Source-generated `InitEffects()` (effect registration)
   - Source-generated `InitSubscriptions()` (event subscription)
7. **Mount lifecycle**: Call `component.OnMount()` (user code).
8. **Initial effect run**: `Batch.RunAndFlush()` to ensure all effects execute synchronously for the first frame.
9. **Track**: Add to `_mountedComponents` list.

### UIDocument per Layer

Each `UILayer` maps to a separate `UIDocument` component on a child GameObject. UIDocuments are created lazily on first mount:

```csharp
private UIDocument GetOrCreateDocument(UILayer layer)
{
    if (_layerDocuments.TryGetValue(layer, out var doc))
        return doc;

    var go = new GameObject($"JUI-Layer-{layer}");
    go.transform.SetParent(transform);
    doc = go.AddComponent<UIDocument>();
    doc.sortingOrder = (int)layer;
    doc.panelSettings = _sharedPanelSettings;

    _layerDocuments[layer] = doc;
    return doc;
}
```

The `PanelSettings` asset is shared across all layers. It controls scale mode, reference resolution, and other global UI settings. It is assigned via the JUIManager inspector or created with default settings.

### Phase Ordering Rationale

The phase order is critical for correctness:

1. **Signals first** (Phase 1): Signal changes from the previous frame (or from user input callbacks) are collected. Effects are marked dirty but not yet run.
2. **POCO bindings** (Phase 2): Pull-mode bindings poll their sources. If a source changed, the target signal is updated, which adds more pending changes -- but these are handled in the NEXT frame's Phase 1 (or immediately if the binding triggers a direct effect dirty-mark).
3. **Effects** (Phase 3): All dirty effects run. This is where the actual UI updates happen (setting label text, toggling visibility, updating styles). Effects run in topological order so computed signals are fresh.
4. **Gestures** (Phase 4): Gesture timers advance. This is after effects so that gesture handlers can read up-to-date signal values.
5. **Focus** (Phase 5): Focus transitions are applied. This is after effects so that newly-visible elements can receive focus.
6. **Audio** (Phase 6): Audio plays are flushed. This is last because audio is a consequence of UI state changes, not a driver of them.

### Zero-Cost Idle

Each phase checks a lightweight flag before doing work:

```csharp
// Phase 1 example
if (Batch.HasPending)
    Batch.FlushPending();

// Phase 3 example
if (EffectRunner.HasDirty)
    EffectRunner.RunDirtyEffects();
```

When the UI is idle (no signal changes, no gestures, no focus changes, no audio), the entire Update loop is effectively six branch-not-taken instructions. This ensures JUIManager has negligible overhead even in CPU-constrained frames.

### Subsystem Initialization Order

Subsystems are initialized in `Awake()` in dependency order:

```csharp
private void InitializeSubsystems()
{
    // 1. Reactive core (no dependencies)
    Batch.Initialize();
    EffectRunner.Initialize();

    // 2. Binding engine (depends on signals)
    BindingEngine.Initialize();

    // 3. Input-driven systems
    GestureEngine.Initialize();
    FocusNavigator.Initialize();

    // 4. Output systems
    UIAudioManager.Initialize();

    // 5. High-level systems (depend on all above)
    ScreenRouter.Initialize(RootScope);
    UIEffectManager.Initialize();
}
```

Teardown in `OnDestroy()` runs in reverse order.

## Source Generator Notes

JUIManager itself is not source-generated. However, it calls source-generated lifecycle methods on components during mount:

- `InitScope()` -- generated by InjectGenerator (Section 11)
- `InitBindings()` -- generated by BindingGenerator (Section 11)
- `InitEffects()` -- generated by EffectGenerator (Section 12)
- `InitSubscriptions()` -- generated by SubscribeGenerator (Section 13)

The Component base class defines these as `partial` methods. If no generator attributes are present on a component, the partial methods have no implementation and are compiled away as no-ops.

## Usage Examples

### Basic Setup

```csharp
// Typically, JUIManager is added to a persistent GameObject in the bootstrap scene.
// The simplest setup:

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private PanelSettings panelSettings;

    private void Start()
    {
        // JUIManager auto-initializes in Awake.
        // Register global services:
        var manager = JUIManager.Instance;

        manager.Provide(new PlayerStore());
        manager.Provide(new InventoryStore());
        manager.Provide(new SettingsStore());
        manager.Provide(new CameraBridge(Camera.main));

        // Mount the main HUD:
        manager.Mount<PlayerHUD>(UILayer.HUD);

        // Mount the screen router (manages full-screen panels):
        manager.Mount<ScreenRouterHost>(UILayer.Screen);
    }
}
```

### Mounting with Configuration

```csharp
// Pass initial state before OnMount runs:
var dialog = JUIManager.Instance.Mount<ConfirmDialog>(
    UILayer.Modal,
    d =>
    {
        d.Title.Value = "Delete Item?";
        d.Message.Value = "This cannot be undone.";
        d.OnConfirm = () => DeleteItem(selectedItem);
    });
```

### Unmounting

```csharp
// Unmount a specific component:
JUIManager.Instance.Unmount(dialog);

// Unmount all modals:
JUIManager.Instance.UnmountAll(UILayer.Modal);
```

### Using the Root Scope

```csharp
// Register a service:
JUIManager.Instance.Provide(new AudioSettings());

// Resolve a service (typically done via [Inject], but manual is available):
var settings = JUIManager.Instance.Use<AudioSettings>();
```

### OnTick for Manual Updates

```csharp
// Rare: per-frame clock display
JUIManager.Instance.OnTick += deltaTime =>
{
    _elapsedTime += deltaTime;
    _clockLabel.text = FormatTime(_elapsedTime);
};
```

### Scene Transitions

```csharp
// JUIManager persists across scenes via DontDestroyOnLoad.
// HUD stays mounted during scene loads.
// Screen-level components can be unmounted and remounted:

async UniTask LoadBattleScene()
{
    JUIManager.Instance.UnmountAll(UILayer.Screen);
    await SceneManager.LoadSceneAsync("Battle");
    JUIManager.Instance.Mount<BattleHUD>(UILayer.HUD);
}
```

## Test Plan

| # | Test | Expectation |
|---|------|-------------|
| 1 | **Singleton creation** | `JUIManager.Instance` is non-null after Awake, null before |
| 2 | **Duplicate prevention** | Second JUIManager instance destroys itself, Instance unchanged |
| 3 | **DontDestroyOnLoad** | JUIManager survives scene transitions |
| 4 | **Singleton cleanup** | After OnDestroy, `JUIManager.Instance` is null |
| 5 | **Mount creates component** | `Mount<T>()` returns non-null component with initialized scope |
| 6 | **Mount renders to correct layer** | Component's root VisualElement is a child of the layer's UIDocument root |
| 7 | **Mount with configure** | Configure action is called before OnMount |
| 8 | **Mount triggers lifecycle** | OnInit and OnMount are called in order |
| 9 | **Mount runs initial effects** | Effects registered during OnMount execute synchronously |
| 10 | **Unmount triggers teardown** | OnUnmount and OnDispose are called, VisualElement removed from layer |
| 11 | **UnmountAll clears layer** | All components in the specified layer are unmounted |
| 12 | **MountedComponents list** | Contains all mounted components, updates on mount/unmount |
| 13 | **Update Phase 1** | `Batch.FlushPending()` marks effects dirty when signals have changed |
| 14 | **Update Phase 2** | `BindingEngine.Tick()` polls Pull bindings and updates targets |
| 15 | **Update Phase 3** | `EffectRunner.RunDirtyEffects()` runs effects in topological order |
| 16 | **Update Phase 4** | `GestureEngine.Tick()` advances gesture recognizer timers |
| 17 | **Update Phase 5** | `FocusNavigator.Tick()` processes queued focus changes |
| 18 | **Update Phase 6** | `UIAudioManager.Tick()` flushes debounced audio plays |
| 19 | **Phase order** | Phases execute in strict 1-2-3-4-5-6 order every frame |
| 20 | **Zero-cost idle** | With no pending work, each phase is a no-op (verified via profiler -- no method calls beyond flag checks) |
| 21 | **Provide/Use** | `Provide<T>()` registers in root scope, `Use<T>()` resolves from root scope |
| 22 | **Provide duplicate type** | Overwrites previous registration (last-write-wins) |
| 23 | **Use unregistered type** | Throws `InvalidOperationException` with descriptive message |
| 24 | **OnTick fires** | OnTick event fires once per frame with `Time.unscaledDeltaTime` |
| 25 | **OnTick with no subscribers** | No exception, no overhead |
| 26 | **Subsystem init order** | Subsystems initialize in dependency order (Batch before EffectRunner, etc.) |
| 27 | **Subsystem teardown order** | Subsystems tear down in reverse initialization order |
| 28 | **Layer UIDocument creation** | First mount to a layer creates a UIDocument with correct sort order |
| 29 | **Layer UIDocument reuse** | Second mount to same layer reuses existing UIDocument |
| 30 | **Root scope disposal** | OnDestroy disposes root scope, all child scopes are disposed transitively |

## Acceptance Criteria

- [ ] `JUIManager` is a `sealed` class extending `MonoBehaviour` with singleton pattern
- [ ] Singleton set in `Awake()`, cleared in `OnDestroy()`, duplicate instances self-destruct
- [ ] `DontDestroyOnLoad` applied in `Awake()`
- [ ] `RootScope` is a `ProviderScope` with no parent, created in `Awake()`
- [ ] `Provide<T>()` delegates to `RootScope.Provide<T>()`
- [ ] `Use<T>()` delegates to `RootScope.Use<T>()` and throws on unregistered type
- [ ] `Mount<T>(layer)` creates component, chains scope from RootScope, attaches to correct UIDocument, and runs full lifecycle
- [ ] `Mount<T>(layer, configure)` calls configure action after construction but before OnMount
- [ ] `Unmount(component)` runs teardown lifecycle and removes VisualElement from layer
- [ ] `UnmountAll(layer)` unmounts all components in the specified layer
- [ ] `MountedComponents` returns all currently mounted root components
- [ ] `Update()` executes exactly 6 phases in order: FlushPending, BindingEngine.Tick, RunDirtyEffects, GestureEngine.Tick, FocusNavigator.Tick, UIAudioManager.Tick
- [ ] Each phase checks a pending-work flag and is a no-op when idle
- [ ] `OnTick` fires once per frame with `Time.unscaledDeltaTime` after all phases complete
- [ ] `UILayer` enum defines 6 layers with increasing sort order values
- [ ] Each UILayer maps to a lazily-created UIDocument child GameObject
- [ ] Subsystems initialize in dependency order and tear down in reverse order
- [ ] All public APIs have XML documentation
- [ ] Zero allocations in the Update loop when no work is pending
