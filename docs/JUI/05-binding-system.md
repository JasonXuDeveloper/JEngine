# Section 5 — Binding System

## Overview

The binding system connects signals to UI elements declaratively. Four binding modes are supported: Push (Signal to UI), Sync (Signal bidirectional with UI), Pull (UI to Signal), and Once (mount-only assignment). Bindings use generic typed structures with struct converters, enabling JIT devirtualization of conversion calls. A separate `BindingEngine` supports POCO dirty-check polling for models that do not use signals.

**Note**: During sections 1-8, tests call `Batch.FlushPending()` and `EffectRunner.RunDirtyEffects()` directly -- JUIManager is not needed yet.

## Dependencies

- Section 1 — Reactive Primitives: Signal & Computed
- Section 2 — Effect System & Batch
- Section 3 — Reactive Collections
- Section 4 — DI Container & UI Layer Manager

## File Structure

- `Runtime/JUI/Binding/BindingMode.cs`
- `Runtime/JUI/Binding/IValueConverter.cs`
- `Runtime/JUI/Binding/IBinding.cs`
- `Runtime/JUI/Binding/SignalBinding.cs`
- `Runtime/JUI/Binding/ConvertedBinding.cs`
- `Runtime/JUI/Binding/BindingPool.cs`
- `Runtime/JUI/Binding/BindingEngine.cs`
- `Runtime/JUI/Binding/Converters/IntToStringConverter.cs`
- `Runtime/JUI/Binding/Converters/FloatToPercentConverter.cs`
- `Runtime/JUI/Binding/Converters/FloatToLengthPercentConverter.cs`
- `Runtime/JUI/Binding/Converters/BoolToDisplayConverter.cs`
- `Runtime/JUI/Binding/Converters/BoolToVisibilityConverter.cs`
- `Runtime/JUI/Binding/Converters/ColorToStyleConverter.cs`
- `Runtime/JUI/Binding/Converters/EnumToStringConverter.cs`
- `Runtime/JUI/Binding/Converters/SpriteToBackgroundConverter.cs`

## API Design

```csharp
/// <summary>
/// Specifies the direction of data flow between a signal and a UI element.
/// </summary>
public enum BindingMode
{
    /// <summary>Signal writes to UI element. One-way: signal → element.</summary>
    Push,

    /// <summary>
    /// Bidirectional: signal → element AND element → signal.
    /// Includes a sync guard to prevent feedback loops.
    /// </summary>
    Sync,

    /// <summary>UI element writes to signal. One-way: element → signal.</summary>
    Pull,

    /// <summary>
    /// One-time assignment at mount. No ongoing tracking or updates.
    /// </summary>
    Once
}

/// <summary>
/// Type-safe value converter for transforming between signal values and UI property types.
/// Implementations SHOULD be structs to enable JIT devirtualization when used as
/// generic type parameters with a struct constraint.
/// </summary>
/// <typeparam name="TSource">The signal value type.</typeparam>
/// <typeparam name="TTarget">The UI property type.</typeparam>
public interface IValueConverter<TSource, TTarget>
{
    /// <summary>Converts a signal value to a UI property value (used by Push and Sync modes).</summary>
    /// <param name="value">The source signal value.</param>
    /// <returns>The converted target value for the UI element.</returns>
    TTarget Convert(TSource value);

    /// <summary>
    /// Converts a UI property value back to a signal value (used by Sync mode).
    /// Implementations may throw <see cref="NotSupportedException"/> if only Push mode is needed.
    /// </summary>
    /// <param name="value">The target UI value.</param>
    /// <returns>The converted source value for the signal.</returns>
    TSource ConvertBack(TTarget value);
}

/// <summary>
/// Non-generic binding interface for lifecycle management and pooling.
/// </summary>
public interface IBinding : IDisposable
{
    /// <summary>Activates the binding, creating effects and/or registering callbacks.</summary>
    void Bind();

    /// <summary>Deactivates the binding, removing effects and callbacks. Called before pool return.</summary>
    void Unbind();
}

/// <summary>
/// Direct signal-to-element binding without value conversion.
/// Uses a static lambda + typed state field pattern to avoid closure allocations.
/// Pooled via <see cref="BindingPool{TSource, TState}"/>.
/// </summary>
/// <typeparam name="TSource">The signal value type, same as the element property type.</typeparam>
/// <typeparam name="TState">Typed state struct capturing the signal, element, and apply delegate.</typeparam>
internal sealed class SignalBinding<TSource, TState> : IBinding
{
    /// <summary>Configures this binding instance for reuse from the pool.</summary>
    /// <param name="signal">The source signal.</param>
    /// <param name="element">The target UI element.</param>
    /// <param name="apply">Static apply action: (element, value) → void.</param>
    /// <param name="mode">The binding direction mode.</param>
    internal void Configure(
        IReadOnlySignal<TSource> signal,
        VisualElement element,
        Action<VisualElement, TSource> apply,
        BindingMode mode);

    public void Bind();
    public void Unbind();
    public void Dispose();
}

/// <summary>
/// Signal-to-element binding with a struct value converter.
/// The <typeparamref name="TConverter"/> struct constraint enables JIT devirtualization
/// of <see cref="IValueConverter{TSource, TTarget}.Convert"/> and
/// <see cref="IValueConverter{TSource, TTarget}.ConvertBack"/> calls.
/// Pooled via <see cref="BindingPool{TSource, TTarget, TState, TConverter}"/>.
/// </summary>
/// <typeparam name="TSource">The signal value type.</typeparam>
/// <typeparam name="TTarget">The UI element property type.</typeparam>
/// <typeparam name="TState">Typed state struct.</typeparam>
/// <typeparam name="TConverter">
/// Struct implementing <see cref="IValueConverter{TSource, TTarget}"/>.
/// The struct constraint guarantees no vtable dispatch on Convert/ConvertBack.
/// </typeparam>
internal sealed class ConvertedBinding<TSource, TTarget, TState, TConverter> : IBinding
    where TConverter : struct, IValueConverter<TSource, TTarget>
{
    /// <summary>Configures this binding instance for reuse from the pool.</summary>
    /// <param name="signal">The source signal.</param>
    /// <param name="element">The target UI element.</param>
    /// <param name="apply">Static apply action: (element, convertedValue) → void.</param>
    /// <param name="mode">The binding direction mode.</param>
    /// <param name="converter">The struct converter instance.</param>
    internal void Configure(
        IReadOnlySignal<TSource> signal,
        VisualElement element,
        Action<VisualElement, TTarget> apply,
        BindingMode mode,
        TConverter converter = default);

    public void Bind();
    public void Unbind();
    public void Dispose();
}

/// <summary>
/// Generic object pool for binding instances. Avoids per-binding allocation
/// after the pool is warmed up.
/// </summary>
/// <typeparam name="TBinding">The concrete binding type to pool.</typeparam>
public static class BindingPool<TBinding> where TBinding : class, IBinding, new()
{
    /// <summary>Rents a binding from the pool (or creates a new one if empty).</summary>
    /// <returns>A reset binding instance ready for <c>Configure()</c>.</returns>
    public static TBinding Rent();

    /// <summary>Returns a binding to the pool after <c>Unbind()</c> has been called.</summary>
    /// <param name="binding">The binding to return.</param>
    public static void Return(TBinding binding);
}

// ─── Built-in Converters ───────────────────────────────────────────────

/// <summary>
/// Converts int to string using an internal cache to avoid <c>int.ToString()</c> allocations
/// for frequently used values (typically -1 to 9999).
/// </summary>
public struct IntToStringConverter : IValueConverter<int, string>
{
    public string Convert(int value);
    public int ConvertBack(string value);
}

/// <summary>Converts float (0.0-1.0) to percentage string (e.g., "75%").</summary>
public struct FloatToPercentConverter : IValueConverter<float, string>
{
    public string Convert(float value);
    public float ConvertBack(string value);
}

/// <summary>Converts float (0.0-1.0) to USS <see cref="Length"/> percent value.</summary>
public struct FloatToLengthPercentConverter : IValueConverter<float, Length>
{
    public Length Convert(float value);
    public float ConvertBack(Length value);
}

/// <summary>Converts bool to <see cref="DisplayStyle"/>. True = Flex, False = None.</summary>
public struct BoolToDisplayConverter : IValueConverter<bool, DisplayStyle>
{
    public DisplayStyle Convert(bool value);
    public bool ConvertBack(DisplayStyle value);
}

/// <summary>Converts bool to <see cref="Visibility"/>. True = Visible, False = Hidden.</summary>
public struct BoolToVisibilityConverter : IValueConverter<bool, Visibility>
{
    public Visibility Convert(bool value);
    public bool ConvertBack(Visibility value);
}

/// <summary>Converts Unity <see cref="Color"/> to USS <see cref="StyleColor"/>.</summary>
public struct ColorToStyleConverter : IValueConverter<Color, StyleColor>
{
    public StyleColor Convert(Color value);
    public Color ConvertBack(StyleColor value);
}

/// <summary>
/// Converts any enum value to its string name via cached <c>Enum.GetName()</c>.
/// </summary>
/// <typeparam name="T">The enum type.</typeparam>
public struct EnumToStringConverter<T> : IValueConverter<T, string> where T : Enum
{
    public string Convert(T value);
    public T ConvertBack(string value);
}

/// <summary>Converts a <see cref="Sprite"/> to a USS <see cref="StyleBackground"/>.</summary>
public struct SpriteToBackgroundConverter : IValueConverter<Sprite, StyleBackground>
{
    public StyleBackground Convert(Sprite value);
    public Sprite ConvertBack(StyleBackground value);
}

// ─── POCO Binding Engine ───────────────────────────────────────────────

/// <summary>
/// Provides per-frame dirty-check polling for plain C# objects (POCOs) that
/// do not use the signal system. Only active if at least one POCO binding is registered.
/// </summary>
public static class BindingEngine
{
    /// <summary>
    /// Binds a POCO property (via getter) to a UI element. The binding is polled
    /// each frame via <see cref="Tick"/> and the element is updated only if the
    /// value has changed since the last poll.
    /// </summary>
    /// <typeparam name="TModel">The model object type.</typeparam>
    /// <typeparam name="TValue">The property value type.</typeparam>
    /// <param name="getter">Function to read the current value from the model.</param>
    /// <param name="model">The model instance.</param>
    /// <param name="element">The target UI element.</param>
    /// <param name="apply">Action to apply the converted value to the element.</param>
    /// <param name="convert">Function to convert the value to a string for display.</param>
    public static void BindPoco<TModel, TValue>(
        Func<TModel, TValue> getter,
        TModel model,
        VisualElement element,
        Action<VisualElement, string> apply,
        Func<TValue, string> convert);

    /// <summary>
    /// Polls all registered POCO bindings and pushes changed values to their elements.
    /// Called once per frame by JUIManager (Section 30). No-op if no POCO bindings exist.
    /// </summary>
    public static void Tick();
}
```

## Data Structures

- `SignalBinding<TSource, TState>._effect` -- the `IEffect` created for Push/Sync mode that reads the signal and applies the value.
- `SignalBinding<TSource, TState>._syncGuard` -- `bool` flag set to `true` while applying in either direction to prevent Sync feedback loops.
- `SignalBinding<TSource, TState>._callbackHandle` -- stores the `RegisterValueChangedCallback` handle for Sync/Pull modes so it can be unregistered on `Unbind()`.
- `ConvertedBinding<TSource, TTarget, TState, TConverter>._converter` -- the struct converter instance, stored as a field (not boxed due to generic specialization).
- `BindingPool<TBinding>._pool` -- `Stack<TBinding>` storing unused binding instances.
- `BindingEngine._pocoBindings` -- `List<IPocoBinding>` storing all active POCO bindings for per-frame polling.
- `BindingEngine._pocoBinding._lastValue` -- cached previous value for dirty comparison.
- `IntToStringConverter` internally references `IntStringCache` -- a static `Dictionary<int, string>` pre-populated for common ranges (e.g., -1 to 9999).

## Implementation Notes

- **Push binding**: On `Bind()`, creates an `Effect` that reads `signal.Value` (auto-tracking) and calls `apply(element, value)`. The effect runs whenever the signal changes.
- **Sync binding**: Combines a Push effect (signal to element) with a `RegisterValueChangedCallback` (element to signal). A `bool _syncGuard` field prevents infinite feedback:
  - When the effect fires (signal changed), sets `_syncGuard = true`, applies to element, resets `_syncGuard`.
  - When the callback fires (element changed), checks `_syncGuard` -- if true, returns immediately; otherwise writes to signal.
- **Pull binding**: Only registers `RegisterValueChangedCallback`. No effect is created. The signal is written when the user interacts with the element.
- **Once binding**: On `Bind()`, reads `signal.Value` once (via `Peek()` to avoid tracking), applies to element, and does nothing further. No effect, no callback.
- **Struct converter devirtualization**: Because `TConverter` has a `where TConverter : struct, IValueConverter<TSource, TTarget>` constraint, the JIT compiler can inline and devirtualize `Convert()`/`ConvertBack()` calls. There is no vtable lookup at runtime.
- **IntToStringConverter cache**: Uses a lazily-initialized static `IntStringCache` dictionary. Values within the pre-cached range return a cached string reference. Values outside the range fall back to `int.ToString()` (rare in typical UI scenarios).
- **BindingEngine.Tick()**: Iterates `_pocoBindings`, calls each binding's getter, compares with `_lastValue` via `EqualityComparer<TValue>.Default`. If changed, calls `convert()` then `apply()` and updates `_lastValue`. Skips entirely (early return) if `_pocoBindings.Count == 0`.
- **ConvertBack exceptions**: Converters used only in Push mode may throw `NotSupportedException` from `ConvertBack()`. This is acceptable because `ConvertBack()` is never called in Push or Once modes.

## Source Generator Notes

The `BindingGenerator` (detailed in Section 11) emits code in `InitBindings()` for each field/property annotated with binding attributes:

- `[Bind]` attribute generates a `BindingPool.Rent()` call, configures the binding with the signal, element, apply delegate, and `BindingMode.Push`.
- `[BindSync]` attribute generates the same but with `BindingMode.Sync`.
- If a `Converter` property is set on the attribute (e.g., `[Bind(Converter = typeof(BoolToDisplayConverter))]`), the generator emits a `ConvertedBinding` instead of `SignalBinding`.
- Generated code uses static lambdas and typed state to avoid closure allocations.

## Usage Examples

```csharp
// --- Push binding: signal → label text ---
var score = new Signal<int>(0);
var label = new Label();

// Manual binding (typically source-generated, shown here for clarity)
var binding = BindingPool<SignalBinding<int, (IReadOnlySignal<int>, Label)>>.Rent();
binding.Configure(
    signal: score,
    element: label,
    apply: static (el, value) => ((Label)el).text = value.ToString(),
    mode: BindingMode.Push);
binding.Bind();

score.Value = 42;
Batch.FlushPending();
EffectRunner.RunDirtyEffects();
// label.text is now "42"

// --- Push binding with struct converter ---
var isVisible = new Signal<bool>(true);
var panel = new VisualElement();

// BoolToDisplayConverter is a struct — JIT devirtualizes Convert()
var converted = BindingPool<ConvertedBinding<bool, DisplayStyle,
    (IReadOnlySignal<bool>, VisualElement), BoolToDisplayConverter>>.Rent();
converted.Configure(
    signal: isVisible,
    element: panel,
    apply: static (el, style) => el.style.display = style,
    mode: BindingMode.Push);
converted.Bind();

isVisible.Value = false;
Batch.FlushPending();
EffectRunner.RunDirtyEffects();
// panel.style.display is now DisplayStyle.None

// --- Sync binding: bidirectional with anti-feedback ---
var username = new Signal<string>("");
var textField = new TextField();

// Sync: signal ↔ textField
// When signal changes → textField.value updates
// When user types → signal.Value updates
// _syncGuard prevents infinite loop

// --- Once binding: mount-only ---
var title = new Signal<string>("Settings");
var header = new Label();
// Reads title.Peek() once at mount, assigns to header.text, no tracking

// --- POCO dirty-check binding ---
var player = new PlayerModel { Name = "Alice", Score = 0 };
BindingEngine.BindPoco(
    getter: static (PlayerModel m) => m.Score,
    model: player,
    element: label,
    apply: static (el, text) => ((Label)el).text = text,
    convert: static (int score) => score.ToString());

player.Score = 100;
BindingEngine.Tick(); // Detects change, updates label
BindingEngine.Tick(); // No change, no-op

// --- IntToStringConverter avoids allocation ---
var count = new Signal<int>(7);
var converter = new IntToStringConverter();
string result = converter.Convert(7);  // Returns cached "7", no allocation
string same = converter.Convert(7);    // Same string reference
```

## Test Plan

1. **Push binding updates element on signal change**: Create a Push binding between a signal and a label, change the signal value, flush batch, verify the label text is updated.
2. **Sync anti-feedback loop (no infinite cycle)**: Create a Sync binding between a signal and a text field. Change the signal value, verify the text field updates. Simulate user input on the text field, verify the signal updates. Verify no infinite recursion or stack overflow.
3. **Pull only writes signal (never writes element)**: Create a Pull binding, change the signal value, verify the element is NOT updated. Simulate user input, verify the signal IS updated.
4. **Once only applies at mount time**: Create a Once binding, verify the element receives the initial value. Change the signal, verify the element is NOT updated.
5. **Converter struct devirtualization (verify struct constraint)**: Verify that `ConvertedBinding` has a `where TConverter : struct` constraint at compile time. Verify that `Convert()`/`ConvertBack()` produce correct results with each built-in converter.
6. **POCO dirty check detection**: Register a POCO binding, mutate the model property, call `Tick()`, verify the element is updated. Call `Tick()` again without mutation, verify no update occurs.
7. **IntToStringConverter uses cache**: Convert the same integer twice, verify the returned strings are reference-equal (`ReferenceEquals`).
8. **BindingPool Rent/Return**: Rent a binding, use it, unbind it, return it to the pool. Rent again, verify the same instance is returned (pool reuse).
9. **Sync guard prevents feedback during effect application**: Set `_syncGuard` manually (or trigger via signal change), verify that the element-to-signal callback is suppressed during the guard window.
10. **ConvertBack throws for Push-only converters**: Verify that calling `ConvertBack()` on a converter designed for Push-only throws `NotSupportedException` when appropriate.
11. **Multiple bindings on same element**: Create two Push bindings targeting different properties of the same element, verify both update independently.
12. **Unbind removes effect and callback**: Call `Unbind()` on a Sync binding, change the signal, verify the element is NOT updated. Simulate input, verify the signal is NOT updated.

## Acceptance Criteria

- [ ] `BindingMode` enum has exactly four values: Push, Sync, Pull, Once
- [ ] `IValueConverter<TSource, TTarget>` defines `Convert()` and `ConvertBack()`
- [ ] `SignalBinding` uses static lambda + typed state (no closure allocation)
- [ ] `ConvertedBinding` has `where TConverter : struct` constraint for JIT devirtualization
- [ ] Sync mode includes `_syncGuard` to prevent feedback loops
- [ ] Pull mode creates no effect (only callback)
- [ ] Once mode uses `Peek()` and creates no ongoing tracking
- [ ] All eight built-in converters are implemented as structs
- [ ] `IntToStringConverter` uses `IntStringCache` for zero-allocation common values
- [ ] `BindingPool<TBinding>.Rent()` reuses returned instances
- [ ] `BindingEngine.Tick()` is a no-op when no POCO bindings are registered
- [ ] `BindingEngine.Tick()` only updates elements whose POCO values have changed
- [ ] All public APIs have XML documentation
- [ ] Zero allocations on the binding hot path (Push effect execution, Sync guard check)
