# Section 6 — Component Base Class

## Overview

Component is the abstract base class for all JUI UI components. It provides lifecycle management, scoped dependency injection, child component tracking, and runtime escape hatches for programmatic composition. Source generators fill in partial methods for attribute-driven features (injection, element queries, bindings, effects, subscriptions), but components work correctly without any generated code -- partial void methods compile to nothing when the generator produces no body.

## Dependencies

- Section 1 — Reactive Primitives: Signal & Computed
- Section 2 — Effect System & Batch
- Section 3 — Reactive Collections
- Section 4 — DI Container & UI Layer Manager
- Section 5 — Binding System

## File Structure

- `Runtime/JUI/Components/Component.cs`

## API Design

```csharp
/// <summary>
/// Abstract base class for all JUI UI components. A component owns a
/// <see cref="VisualElement"/> subtree, a <see cref="ProviderScope"/> for
/// scoped DI, and participates in a parent-child component tree with
/// deterministic lifecycle ordering.
///
/// <para>
/// Source generators emit partial method bodies for attribute-driven features.
/// Components without any source-gen attributes compile and run correctly --
/// partial void methods are elided by the compiler.
/// </para>
/// </summary>
public abstract class Component : IDisposable
{
    // ─── Properties ────────────────────────────────────────────────────

    /// <summary>
    /// The root VisualElement owned by this component, created by <see cref="Render"/>.
    /// Available after construction completes.
    /// </summary>
    public VisualElement Element { get; }

    /// <summary>
    /// The parent component in the component tree, or null if this is a root component.
    /// Set during construction when a parent mounts this component as a child.
    /// </summary>
    public Component Parent { get; }

    /// <summary>
    /// Read-only view of the child components mounted under this component.
    /// Updated automatically when children are added or removed.
    /// </summary>
    public ReadOnlyCollection<Component> Children { get; }

    /// <summary>
    /// The dependency injection scope for this component. Inherits from the
    /// parent component's scope (if any), forming a hierarchical DI chain
    /// that mirrors the component tree.
    /// </summary>
    public ProviderScope Scope { get; }

    /// <summary>
    /// A cancellation token that is cancelled when this component is disposed.
    /// Use this to tie async operations to the component's lifetime so they
    /// are automatically cancelled on unmount.
    /// </summary>
    public CancellationToken LifetimeToken { get; }

    // ─── User-Facing Lifecycle Hooks ───────────────────────────────────

    /// <summary>
    /// Creates and returns the root VisualElement subtree for this component.
    /// Called once during construction, after all partial init methods have run.
    /// Must not return null.
    /// </summary>
    /// <returns>The root VisualElement of this component's visual subtree.</returns>
    protected abstract VisualElement Render();

    /// <summary>
    /// Called after the component is fully constructed, all init methods have
    /// run, and <see cref="Render"/> has returned. Override to perform
    /// post-mount setup (e.g., subscribe to JUIManager.OnTick for per-frame logic).
    /// </summary>
    protected virtual void OnMount() { }

    /// <summary>
    /// Called at the beginning of <see cref="Dispose"/>, before any cleanup
    /// occurs. Override to perform teardown logic (e.g., unsubscribe from
    /// JUIManager.OnTick).
    /// </summary>
    protected virtual void OnUnmount() { }

    // ─── Source-Gen Partial Methods (NOT user-facing) ──────────────────

    /// <summary>
    /// Emitted by InjectGenerator. Resolves [Inject] dependencies from <see cref="Scope"/>
    /// and registers [ProvideBridge] services into <see cref="Scope"/> for child access.
    /// </summary>
    partial void InitScope();

    /// <summary>
    /// Emitted by ElementGenerator. Queries the rendered VisualElement tree
    /// for elements annotated with [UIComponent] and assigns them to fields.
    /// </summary>
    /// <param name="root">The root VisualElement returned by <see cref="Render"/>.</param>
    partial void InitElements(VisualElement root);

    /// <summary>
    /// Emitted by BindingGenerator. Creates and activates bindings for
    /// fields annotated with [Bind] or [BindSync].
    /// </summary>
    partial void InitBindings();

    /// <summary>
    /// Emitted by EffectGenerator. Registers effects for methods annotated
    /// with [Effect], wiring their signal dependencies automatically.
    /// </summary>
    partial void InitEffects();

    /// <summary>
    /// Emitted by EventGenerator. Subscribes methods annotated with [Subscribe]
    /// to the appropriate event channels.
    /// </summary>
    partial void InitSubscriptions();

    // ─── Runtime Escape Hatches ────────────────────────────────────────

    /// <summary>
    /// Creates a new signal owned by this component. The signal is automatically
    /// disposed when the component is disposed. Use for local reactive state
    /// that does not need attribute-driven source generation.
    /// </summary>
    /// <typeparam name="T">The signal value type.</typeparam>
    /// <param name="initial">The initial value of the signal.</param>
    /// <returns>A new signal tied to this component's lifetime.</returns>
    protected Signal<T> UseSignal<T>(T initial);

    /// <summary>
    /// Creates a new effect owned by this component. The effect is automatically
    /// disposed when the component is disposed.
    /// </summary>
    /// <typeparam name="TState">Typed state to avoid closure allocation.</typeparam>
    /// <param name="action">The effect action, receiving the typed state.</param>
    /// <param name="state">The state instance passed to the action on each run.</param>
    /// <returns>The created effect.</returns>
    protected IEffect UseEffect<TState>(Action<TState> action, TState state);

    /// <summary>
    /// Creates a new computed signal owned by this component. The computed signal
    /// is automatically disposed when the component is disposed.
    /// </summary>
    /// <typeparam name="TState">Typed state to avoid closure allocation.</typeparam>
    /// <typeparam name="T">The computed value type.</typeparam>
    /// <param name="fn">The computation function, receiving the typed state.</param>
    /// <param name="state">The state instance passed to the function.</param>
    /// <returns>A new computed signal tied to this component's lifetime.</returns>
    protected ComputedSignal<T> UseComputed<TState, T>(Func<TState, T> fn, TState state);

    /// <summary>
    /// Creates a new async signal owned by this component. Async signals support
    /// loading/error/value states for UniTask-based data fetching.
    /// Automatically disposed when the component is disposed.
    /// </summary>
    /// <typeparam name="T">The async result value type.</typeparam>
    /// <param name="defaultValue">The default value before the async operation completes.</param>
    /// <returns>A new async signal tied to this component's lifetime.</returns>
    protected AsyncSignal<T> UseAsync<T>(T defaultValue = default);

    // ─── Scoped DI ─────────────────────────────────────────────────────

    /// <summary>
    /// Registers a service instance in this component's scope, making it
    /// available to this component and all descendant components via
    /// <see cref="ProviderScope.Use{T}"/>.
    /// </summary>
    /// <typeparam name="T">The service type (must be a reference type).</typeparam>
    /// <param name="instance">The service instance to provide.</param>
    protected void Provide<T>(T instance) where T : class;

    // ─── Disposal ──────────────────────────────────────────────────────

    /// <summary>
    /// Disposes this component and all owned resources in a deterministic order:
    /// <list type="number">
    ///   <item><description>Calls <see cref="OnUnmount"/></description></item>
    ///   <item><description>Cancels <see cref="LifetimeToken"/> (cancels async operations)</description></item>
    ///   <item><description>Disposes subscriptions (event unsubscriptions)</description></item>
    ///   <item><description>Disposes children (recursive, depth-first)</description></item>
    ///   <item><description>Disposes effects (unbinds signal tracking)</description></item>
    ///   <item><description>Disposes signals (created via UseSignal/UseComputed/UseAsync)</description></item>
    ///   <item><description>Disposes scope (clears DI registrations)</description></item>
    ///   <item><description>Removes <see cref="Element"/> from parent VisualElement</description></item>
    /// </list>
    /// </summary>
    public void Dispose();
}
```

## Data Structures

- `Component._children` -- `List<Component>` backing list for the `Children` read-only view.
- `Component._childrenReadOnly` -- `ReadOnlyCollection<Component>` wrapping `_children` via `AsReadOnly()`.
- `Component._ownedSignals` -- `List<IDisposable>` tracking signals, computed signals, and async signals created via `UseSignal`/`UseComputed`/`UseAsync` for deterministic disposal.
- `Component._ownedEffects` -- `List<IEffect>` tracking effects created via `UseEffect` for deterministic disposal.
- `Component._ownedSubscriptions` -- `List<IDisposable>` tracking event subscriptions for deterministic disposal.
- `Component._cts` -- `CancellationTokenSource` backing the `LifetimeToken` property, cancelled during `Dispose()`.
- `Component._scope` -- `ProviderScope` instance created in the constructor, parented to `Parent?.Scope`.

## Implementation Notes

### Lifecycle Order

The full lifecycle proceeds in this deterministic sequence:

1. **Constructor** -- allocates internal collections, creates `_cts`.
2. **Scope created** -- `new ProviderScope(parent: Parent?.Scope)`.
3. **InitScope()** -- source-generated: resolves `[Inject]` dependencies, registers `[ProvideBridge]` services.
4. **InitElements(root)** -- source-generated: after `Render()` returns the root element, queries child elements annotated with `[UIComponent]`.
5. **InitBindings()** -- source-generated: creates and activates bindings for `[Bind]`/`[BindSync]` annotated fields.
6. **InitEffects()** -- source-generated: registers effects for `[Effect]` annotated methods.
7. **InitSubscriptions()** -- source-generated: subscribes `[Subscribe]` annotated methods to event channels.
8. **Render()** -- user-implemented: builds and returns the VisualElement subtree.
9. **OnMount()** -- user-implemented: post-construction setup hook.

**Clarification on ordering**: `Render()` is called between `InitScope()` and `InitElements()`. The actual sequence is: Constructor, Scope, InitScope, **Render**, InitElements(root), InitBindings, InitEffects, InitSubscriptions, OnMount. This is because `InitElements` needs the rendered tree to query against.

### Disposal Chain

Disposal proceeds in the reverse order of construction, ensuring that dependencies are available until the resources that use them are cleaned up:

1. `OnUnmount()` -- user teardown logic runs first while everything is still alive.
2. Cancel `_cts` -- async operations tied to `LifetimeToken` are cancelled.
3. Dispose `_ownedSubscriptions` -- event handlers are unsubscribed.
4. Dispose `_children` -- recursive depth-first disposal of the entire subtree.
5. Dispose `_ownedEffects` -- effects are unbound from their signal dependencies.
6. Dispose `_ownedSignals` -- signals, computed signals, and async signals are disposed.
7. Dispose `_scope` -- DI registrations are cleared.
8. Remove `Element` from its parent VisualElement (if attached).

### No OnUpdate

There is no `OnUpdate()` or per-frame callback on Component. This is intentional:

- **Reactive effects** handle state changes automatically via the signal/effect system.
- **Animations** use LitMotion (Section 15), which manages its own update loop.
- Components that truly need per-frame logic (e.g., a clock display, a continuously rotating element) subscribe to `JUIManager.OnTick` in `OnMount()` and unsubscribe in `OnUnmount()`.

### Partial Method Semantics

All five `partial void` methods (`InitScope`, `InitElements`, `InitBindings`, `InitEffects`, `InitSubscriptions`) compile to nothing if the source generator produces no body for them. This means:

- A component with zero attributes compiles and runs correctly with no overhead.
- Each attribute type only adds the corresponding init method body. A component with only `[Inject]` attributes gets only `InitScope()` generated; the other four remain no-ops.

### UseSignal / UseEffect / UseComputed / UseAsync

These "Use*" methods are runtime escape hatches for cases where attribute-driven source generation is not appropriate (e.g., dynamically computed state, conditional signal creation). Resources created via these methods are tracked in the owning component's disposal lists and are automatically disposed when the component is disposed.

```
UseSignal<T>(initial)     → creates Signal<T>, adds to _ownedSignals
UseEffect<TState>(...)    → creates Effect, adds to _ownedEffects
UseComputed<TState, T>()  → creates ComputedSignal<T>, adds to _ownedSignals
UseAsync<T>(default)      → creates AsyncSignal<T>, adds to _ownedSignals
```

### Scope Inheritance

A component's `ProviderScope` is automatically parented to its parent component's scope. This means:

- A service provided at a high-level component (e.g., a screen) is available to all descendant components via `Scope.Use<T>()`.
- A child component can shadow a parent's service by calling `Provide<T>()` with the same type.
- Root components (no parent) have a scope with no parent, so `Use<T>()` only resolves from that scope.

## Source Generator Notes

The five partial methods on Component are the primary extension points for source generators:

| Partial Method | Generator | Triggered By | What It Emits |
|---|---|---|---|
| `InitScope()` | InjectGenerator | `[Inject]`, `[ProvideBridge]` | `field = Scope.Use<T>()`, `Scope.Provide<T>(instance)` |
| `InitElements(root)` | ElementGenerator | `[UIComponent]` | `field = root.Q<T>(name)` queries |
| `InitBindings()` | BindingGenerator | `[Bind]`, `[BindSync]` | `BindingPool.Rent()`, `Configure()`, `Bind()` |
| `InitEffects()` | EffectGenerator | `[Effect]` | `Effect.Create(method, state)` |
| `InitSubscriptions()` | EventGenerator | `[Subscribe]` | `EventBus.Subscribe<T>(method)` |

Generators are covered in detail in Sections 9-13. The Component base class only defines the partial method signatures and calls them in the correct lifecycle order.

## Usage Examples

```csharp
// --- Minimal component (no source-gen attributes) ---
public class HelloLabel : Component
{
    protected override VisualElement Render()
    {
        return new Label("Hello, JUI!");
    }
}

// --- Component with runtime escape hatches ---
public class CounterComponent : Component
{
    private Signal<int> _count;
    private Label _label;

    protected override VisualElement Render()
    {
        _count = UseSignal(0);
        _label = new Label("0");

        UseEffect(static state =>
        {
            state.label.text = state.count.Value.ToString();
        }, (label: _label, count: _count));

        var button = new Button(() => _count.Value++) { text = "Increment" };

        var root = new VisualElement();
        root.Add(_label);
        root.Add(button);
        return root;
    }
}

// --- Component with scoped DI ---
public class GameScreen : Component
{
    protected override VisualElement Render()
    {
        // Provide a service for all children
        Provide<IInventoryService>(new InventoryService());

        var root = new VisualElement();
        // Children can resolve IInventoryService via Scope.Use<>()
        return root;
    }
}

// --- Component with async lifetime management ---
public class DataPanel : Component
{
    protected override VisualElement Render()
    {
        var data = UseAsync<PlayerData>();
        var label = new Label("Loading...");

        // Async operation tied to component lifetime
        LoadDataAsync(data, LifetimeToken).Forget();

        UseEffect(static state =>
        {
            state.label.text = state.data.Value?.Name ?? "Loading...";
        }, (label, data));

        return label;
    }

    private async UniTask LoadDataAsync(AsyncSignal<PlayerData> target, CancellationToken ct)
    {
        var result = await PlayerAPI.FetchAsync(ct);
        target.Value = result;
    }
}

// --- Component with per-frame logic (rare case) ---
public class ClockDisplay : Component
{
    private Signal<string> _timeText;
    private Label _label;

    protected override VisualElement Render()
    {
        _timeText = UseSignal(DateTime.Now.ToString("HH:mm:ss"));
        _label = new Label();

        UseEffect(static state =>
        {
            state.label.text = state.time.Value;
        }, (label: _label, time: _timeText));

        return _label;
    }

    protected override void OnMount()
    {
        JUIManager.OnTick += UpdateClock;
    }

    protected override void OnUnmount()
    {
        JUIManager.OnTick -= UpdateClock;
    }

    private void UpdateClock()
    {
        _timeText.Value = DateTime.Now.ToString("HH:mm:ss");
    }
}
```

## Test Plan

1. **Full lifecycle order verification**: Create a test component that records each lifecycle step (constructor, InitScope, Render, InitElements, InitBindings, InitEffects, InitSubscriptions, OnMount) into a list. Verify the list matches the expected order.
2. **Partial methods called in correct sequence**: Use a test component with a source generator mock that populates all five partial methods. Verify they execute in the documented order relative to Render and OnMount.
3. **UseSignal creates owned signal, disposed with component**: Call `UseSignal<int>(0)`, verify the signal works normally. Dispose the component, verify the signal is disposed (subscribers cleared).
4. **UseEffect tracks dependencies correctly**: Create a signal via `UseSignal`, create an effect via `UseEffect` that reads it. Change the signal, verify the effect runs.
5. **Provide/inject in child component**: Create a parent component that calls `Provide<IService>(instance)`. Create a child component whose scope resolves `IService`. Verify the child gets the parent's instance.
6. **LifetimeToken cancels on dispose**: Start a long-running async operation using `LifetimeToken`. Dispose the component. Verify the `CancellationToken` is cancelled and the async operation observes cancellation.
7. **Component without source-gen attributes compiles (partial methods are no-op)**: Create a minimal component with only `Render()`. Verify it constructs, mounts, and disposes without errors.
8. **Children collection updated correctly on mount/unmount**: Mount child components, verify `Children` reflects them. Dispose a child, verify it is removed from `Children`.
9. **Scope inherits from parent component's scope**: Provide a service in a grandparent component. Verify a grandchild component can resolve it through the scope chain.
10. **Disposal chain order verified**: Create a test component that records disposal order of OnUnmount, subscriptions, children, effects, signals, and scope. Verify the order matches: OnUnmount, async cancellation, subscriptions, children, effects, signals, scope, element removal.
11. **UseComputed creates owned computed, disposed with component**: Call `UseComputed`, verify it recomputes on dependency change. Dispose the component, verify the computed is disposed.
12. **UseAsync creates owned async signal, disposed with component**: Call `UseAsync<T>()`, set its value asynchronously, verify the value is accessible. Dispose the component, verify the async signal is disposed.
13. **Double dispose is safe**: Call `Dispose()` twice on the same component, verify no exception is thrown.
14. **Element is removed from parent VisualElement on dispose**: Mount a component's element into a parent VisualElement. Dispose the component. Verify the element is no longer a child of the parent VisualElement.

## Acceptance Criteria

- [ ] `Component` is an abstract class implementing `IDisposable`
- [ ] `Render()` is abstract and must return a non-null `VisualElement`
- [ ] `OnMount()` and `OnUnmount()` are virtual with empty default implementations
- [ ] Five partial void methods: `InitScope`, `InitElements`, `InitBindings`, `InitEffects`, `InitSubscriptions`
- [ ] Partial methods compile to nothing when no generator body is emitted
- [ ] Lifecycle order: Constructor, Scope, InitScope, Render, InitElements, InitBindings, InitEffects, InitSubscriptions, OnMount
- [ ] Disposal order: OnUnmount, cancel token, subscriptions, children, effects, signals, scope, element removal
- [ ] `UseSignal<T>()` creates a component-owned signal added to `_ownedSignals`
- [ ] `UseEffect<TState>()` creates a component-owned effect added to `_ownedEffects`
- [ ] `UseComputed<TState, T>()` creates a component-owned computed added to `_ownedSignals`
- [ ] `UseAsync<T>()` creates a component-owned async signal added to `_ownedSignals`
- [ ] `Provide<T>()` delegates to `Scope.Provide<T>()`
- [ ] `Scope` is parented to `Parent?.Scope` for hierarchical DI resolution
- [ ] `LifetimeToken` is cancelled during `Dispose()`
- [ ] `Children` is a `ReadOnlyCollection<Component>` reflecting current children
- [ ] Double `Dispose()` is safe (no exception on second call)
- [ ] All public APIs have XML documentation
- [ ] No `OnUpdate()` method exists -- per-frame logic uses `JUIManager.OnTick`
