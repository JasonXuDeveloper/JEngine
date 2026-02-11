# Section 11 — InjectGenerator & BindingGenerator

## Overview

InjectGenerator processes `[Inject]` attributes on fields and properties to generate `InitScope()` methods that resolve dependencies from the component's `ProviderScope`. BindingGenerator processes `[Bind]` and `[BindSync]` attributes to generate static-lambda effects that wire signal values to UXML element properties, supporting all four binding modes: Push (signal to element), Pull (element to signal via callback), Sync (bidirectional), and Once (one-time assignment).

## Dependencies

- Section 9 — Source Generator Project Setup (IIncrementalGenerator, DiagnosticDescriptors)
- Section 5 — Binding System (binding modes, IValueConverter)
- Section 4 — DI Container (ProviderScope, Use/TryUse)

## File Structure

- `SourceGenerators/JEngine.JUI.Generators/InjectGenerator.cs`
- `SourceGenerators/JEngine.JUI.Generators/BindingGenerator.cs`
- `Runtime/JUI/Attributes/InjectAttribute.cs`
- `Runtime/JUI/Attributes/BindAttribute.cs`
- `Runtime/JUI/Attributes/BindSyncAttribute.cs`

## API Design

### InjectAttribute

```csharp
namespace JEngine.JUI;

/// <summary>
/// Marks a field or property for automatic dependency injection from the component's ProviderScope.
/// The generator emits a Scope.Use&lt;T&gt;() call in InitScope() to resolve the value at mount time.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class InjectAttribute : Attribute
{
    /// <summary>
    /// When true, uses Scope.TryUse&lt;T&gt;() instead of Scope.Use&lt;T&gt;().
    /// The field/property remains null (default) if no provider is found,
    /// instead of throwing InvalidOperationException.
    /// </summary>
    public bool Optional { get; set; } = false;
}
```

### BindAttribute

```csharp
namespace JEngine.JUI;

/// <summary>
/// Declares a binding between a signal field and a named UXML element.
/// The generator emits the appropriate binding code in InitBindings() based on the Mode.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class BindAttribute : Attribute
{
    /// <summary>
    /// The element name to bind to, typically referencing an El constant via nameof().
    /// Example: nameof(El.HealthText)
    /// </summary>
    public string Element { get; }

    /// <summary>
    /// The element property to bind to. Defaults to "text" for Label, "value" for input elements.
    /// Common values: "text", "value", "visible", "enabledSelf", "style.display"
    /// </summary>
    public string Property { get; set; } = null;

    /// <summary>
    /// Optional converter type that implements IValueConverter&lt;TSource, TTarget&gt;.
    /// Applied when the signal type differs from the element property type.
    /// Pass typeof(MyConverter) to specify a converter.
    /// </summary>
    public Type Converter { get; set; } = null;

    /// <summary>
    /// The binding mode. Defaults to Push (signal changes flow to the element).
    /// </summary>
    public BindingMode Mode { get; set; } = BindingMode.Push;

    /// <summary>
    /// For Pull mode only: name of the callback method invoked when the element value changes.
    /// The method must accept one parameter matching the element's value type.
    /// </summary>
    public string Callback { get; set; } = null;

    /// <summary>
    /// Creates a binding to the specified element.
    /// </summary>
    /// <param name="element">The element name (use nameof(El.X)).</param>
    public BindAttribute(string element)
    {
        Element = element;
    }
}

/// <summary>
/// Supported binding modes for signal-to-element bindings.
/// </summary>
public enum BindingMode
{
    /// <summary>Signal value flows to element property. Effect re-runs on signal change.</summary>
    Push,

    /// <summary>Element value changes invoke a callback. No effect is created.</summary>
    Pull,

    /// <summary>Bidirectional: signal changes update element, element changes update signal (with sync guard).</summary>
    Sync,

    /// <summary>One-time assignment in InitBindings. No ongoing tracking.</summary>
    Once
}
```

### BindSyncAttribute

```csharp
namespace JEngine.JUI;

/// <summary>
/// Shorthand for [Bind(element, Mode = BindingMode.Sync)].
/// Declares a two-way binding between a signal and an input element.
/// The generator emits both a push effect and a RegisterValueChangedCallback with sync guard.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class BindSyncAttribute : Attribute
{
    /// <summary>The element name to bind to (use nameof(El.X)).</summary>
    public string Element { get; }

    /// <summary>
    /// Optional converter type. Applied in both directions: forward for signal-to-element,
    /// reverse for element-to-signal.
    /// Must implement IValueConverter&lt;TSignal, TElement&gt; with both Convert and ConvertBack.
    /// </summary>
    public Type Converter { get; set; } = null;

    /// <summary>
    /// Creates a two-way binding to the specified element.
    /// </summary>
    /// <param name="element">The element name (use nameof(El.X)).</param>
    public BindSyncAttribute(string element)
    {
        Element = element;
    }
}
```

## Data Structures

- `InjectInfo` -- internal struct used by InjectGenerator: `{ FieldName, TypeSymbol, IsOptional, IsReadonly, Location }`.
- `BindInfo` -- internal struct used by BindingGenerator: `{ FieldName, SignalTypeArg, ElementName, ElementPropertyName, ConverterType, Mode, CallbackName, Location }`.
- `BindSyncInfo` -- internal struct used by BindingGenerator: `{ FieldName, SignalTypeArg, ElementName, ConverterType, Location }`.

## Implementation Notes

### InjectGenerator

- **Pipeline**: `ForAttributeWithMetadataName("JEngine.JUI.InjectAttribute")` collects all fields/properties with `[Inject]`. Groups by containing class. Emits one `InitScope()` partial method per class.
- **Type constraint**: `[Inject]` only supports reference types (`where T : class`). Value types trigger `JUI041` error.
- **Static members**: `[Inject]` on a static field/property triggers `JUI043` error.
- **Non-component classes**: `[Inject]` on a class that does not inherit `JUIComponent` triggers `JUI044` error.
- **Generated code ordering**: Fields are sorted alphabetically by type name for deterministic output.

### BindingGenerator

- **Element resolution**: The `Element` string must match a constant in the generated `El` class. The generator cross-references the `El` constants (from ElementGenerator) at the semantic model level. If no match is found, `JUI060` error is emitted.
- **Property inference**: If `Property` is not specified, the generator infers the default property based on element type:
  | Element Type | Default Property |
  |---|---|
  | `Label` | `text` |
  | `TextField` | `value` |
  | `Toggle` | `value` |
  | `Slider` | `value` |
  | `SliderInt` | `value` |
  | `DropdownField` | `value` |
  | `VisualElement` | `visible` |

- **Converter validation**: If a `Converter` type is specified, the generator verifies it implements `IValueConverter<TSource, TTarget>` where `TSource` matches the signal's type parameter and `TTarget` matches the element property type. Mismatch triggers `JUI061` error.
- **Sync guard**: For `BindingMode.Sync` and `[BindSync]`, the generator emits a `bool _syncGuard_fieldName` field. The push effect sets the guard before writing to the element; the `RegisterValueChangedCallback` checks the guard before writing back to the signal, preventing infinite loops.
- **Pull mode**: No effect is created. Only a `RegisterValueChangedCallback` is registered that invokes the named callback method. If `Callback` is null, `JUI065` error is emitted.
- **Once mode**: A direct assignment is emitted in `InitBindings()` with no effect, no callback, no ongoing tracking.

## Source Generator Notes

### InjectGenerator

```csharp
[Generator]
public sealed class InjectGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var injectFields = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "JEngine.JUI.InjectAttribute",
                predicate: static (node, _) =>
                    node is FieldDeclarationSyntax or PropertyDeclarationSyntax,
                transform: static (ctx, ct) => ExtractInjectInfo(ctx, ct));

        var grouped = injectFields.Collect()
            .SelectMany(static (items, _) => items.GroupBy(i => i.ContainingClass));

        context.RegisterSourceOutput(grouped,
            static (spc, group) => EmitInitScope(spc, group));
    }
}
```

### BindingGenerator

```csharp
[Generator]
public sealed class BindingGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Collect [Bind] fields
        var bindFields = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "JEngine.JUI.BindAttribute",
                predicate: static (node, _) =>
                    node is FieldDeclarationSyntax or PropertyDeclarationSyntax,
                transform: static (ctx, ct) => ExtractBindInfo(ctx, ct));

        // Collect [BindSync] fields
        var bindSyncFields = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "JEngine.JUI.BindSyncAttribute",
                predicate: static (node, _) =>
                    node is FieldDeclarationSyntax or PropertyDeclarationSyntax,
                transform: static (ctx, ct) => ExtractBindSyncInfo(ctx, ct));

        var combined = bindFields.Collect().Combine(bindSyncFields.Collect());

        context.RegisterSourceOutput(combined,
            static (spc, pair) => EmitInitBindings(spc, pair.Left, pair.Right));
    }
}
```

## Usage Examples

### Inject -- Required Dependency

```csharp
// Developer writes:
[UIComponent("InventoryPanel.uxml")]
public partial class InventoryPanel : JUIComponent
{
    [Inject]
    private IInventoryService _inventory;

    [Inject]
    private IAudioService _audio;
}

// Generator emits in InventoryPanel.Inject.g.cs:
partial class InventoryPanel
{
    partial void InitScope()
    {
        _audio = Scope.Use<IAudioService>();
        _inventory = Scope.Use<IInventoryService>();
    }
}
```

### Inject -- Optional Dependency

```csharp
// Developer writes:
[UIComponent("DebugOverlay.uxml")]
public partial class DebugOverlay : JUIComponent
{
    [Inject(Optional = true)]
    private IAnalyticsService _analytics;
}

// Generator emits:
partial class DebugOverlay
{
    partial void InitScope()
    {
        Scope.TryUse<IAnalyticsService>(out var _inject_analytics);
        _analytics = _inject_analytics;
    }
}
```

### Bind -- Push Mode (Default)

```csharp
// Developer writes:
[UIComponent("PlayerHUD.uxml", "PlayerHUD.uss")]
public partial class PlayerHUD : JUIComponent
{
    private readonly Signal<int> _health = new(100);

    [Bind(nameof(El.HealthText))]
    private Signal<int> Health => _health;
}

// Generator emits in PlayerHUD.Bind.g.cs:
partial class PlayerHUD
{
    partial void InitBindings()
    {
        // Push: signal -> element.text
        UseEffect(static (PlayerHUD self) =>
        {
            var value = self.Health.Value;
            self._el_healthText.text = value.ToString();
        }, this);
    }
}
```

### Bind -- Push Mode with Converter

```csharp
// Developer writes:
public class HealthBarConverter : IValueConverter<int, float>
{
    public float Convert(int value) => value / 100f;
    public int ConvertBack(float value) => (int)(value * 100);
}

[UIComponent("PlayerHUD.uxml")]
public partial class PlayerHUD : JUIComponent
{
    private readonly Signal<int> _health = new(100);

    [Bind(nameof(El.HealthFill), Property = "style.width",
          Converter = typeof(HealthBarConverter))]
    private Signal<int> Health => _health;
}

// Generator emits:
partial class PlayerHUD
{
    private static readonly HealthBarConverter _converter_Health = new();

    partial void InitBindings()
    {
        // Push with converter: signal -> convert -> element.style.width
        UseEffect(static (PlayerHUD self) =>
        {
            var raw = self.Health.Value;
            var converted = _converter_Health.Convert(raw);
            self._el_healthFill.style.width = new StyleLength(
                new Length(converted * 100f, LengthUnit.Percent));
        }, this);
    }
}
```

### Bind -- Pull Mode

```csharp
// Developer writes:
[UIComponent("Settings.uxml")]
public partial class SettingsPanel : JUIComponent
{
    private readonly Signal<float> _volume = new(0.8f);

    [Bind(nameof(El.VolumeSlider), Mode = BindingMode.Pull, Callback = nameof(OnVolumeChanged))]
    private Signal<float> Volume => _volume;

    private void OnVolumeChanged(float newValue)
    {
        _volume.Value = newValue;
        // Additional logic: save to prefs, etc.
    }
}

// Generator emits:
partial class SettingsPanel
{
    partial void InitBindings()
    {
        // Pull: element -> callback only, no effect
        _el_volumeSlider.RegisterValueChangedCallback<float>(
            static (evt, self) => self.OnVolumeChanged(evt.newValue),
            this);
    }
}
```

### Bind -- Once Mode

```csharp
// Developer writes:
[UIComponent("PlayerHUD.uxml")]
public partial class PlayerHUD : JUIComponent
{
    private readonly Signal<string> _playerName = new("Hero");

    [Bind(nameof(El.PlayerNameLabel), Mode = BindingMode.Once)]
    private Signal<string> PlayerName => _playerName;
}

// Generator emits:
partial class PlayerHUD
{
    partial void InitBindings()
    {
        // Once: direct assignment, no tracking
        _el_playerNameLabel.text = PlayerName.Value.ToString();
    }
}
```

### BindSync -- Two-Way Binding

```csharp
// Developer writes:
[UIComponent("ChatPanel.uxml")]
public partial class ChatPanel : JUIComponent
{
    private readonly Signal<string> _messageText = new("");

    [BindSync(nameof(El.MessageInput))]
    private Signal<string> MessageText => _messageText;
}

// Generator emits in ChatPanel.Bind.g.cs:
partial class ChatPanel
{
    private bool _syncGuard_MessageText;

    partial void InitBindings()
    {
        // Sync: signal -> element (push direction)
        UseEffect(static (ChatPanel self) =>
        {
            var value = self.MessageText.Value;
            self._syncGuard_MessageText = true;
            self._el_messageInput.value = value;
            self._syncGuard_MessageText = false;
        }, this);

        // Sync: element -> signal (pull direction)
        _el_messageInput.RegisterValueChangedCallback<string>(
            static (evt, self) =>
            {
                if (self._syncGuard_MessageText) return;
                self.MessageText.Value = evt.newValue;
            },
            this);
    }
}
```

### BindSync -- Two-Way with Converter

```csharp
// Developer writes:
public class PercentConverter : IValueConverter<float, string>
{
    public string Convert(float value) => $"{value:P0}";
    public float ConvertBack(string value) =>
        float.TryParse(value.TrimEnd('%'), out var f) ? f / 100f : 0f;
}

[UIComponent("Editor.uxml")]
public partial class EditorPanel : JUIComponent
{
    private readonly Signal<float> _opacity = new(1f);

    [BindSync(nameof(El.OpacityInput), Converter = typeof(PercentConverter))]
    private Signal<float> Opacity => _opacity;
}

// Generator emits:
partial class EditorPanel
{
    private static readonly PercentConverter _converter_Opacity = new();
    private bool _syncGuard_Opacity;

    partial void InitBindings()
    {
        // Sync push: signal -> convert -> element
        UseEffect(static (EditorPanel self) =>
        {
            var raw = self.Opacity.Value;
            var converted = _converter_Opacity.Convert(raw);
            self._syncGuard_Opacity = true;
            self._el_opacityInput.value = converted;
            self._syncGuard_Opacity = false;
        }, this);

        // Sync pull: element -> convertBack -> signal
        _el_opacityInput.RegisterValueChangedCallback<string>(
            static (evt, self) =>
            {
                if (self._syncGuard_Opacity) return;
                self.Opacity.Value = _converter_Opacity.ConvertBack(evt.newValue);
            },
            this);
    }
}
```

## Test Plan

1. **InjectGenerator -- required injection**: Verify `Scope.Use<T>()` is emitted for `[Inject]` fields.
2. **InjectGenerator -- optional injection**: Verify `Scope.TryUse<T>()` is emitted for `[Inject(Optional = true)]`.
3. **InjectGenerator -- multiple injections sorted**: Verify fields are resolved in alphabetical type-name order for deterministic output.
4. **InjectGenerator -- value type rejected**: Apply `[Inject]` to an `int` field, verify `JUI041` error.
5. **InjectGenerator -- static member rejected**: Apply `[Inject]` to a static field, verify `JUI043` error.
6. **InjectGenerator -- non-component rejected**: Apply `[Inject]` to a plain class, verify `JUI044` error.
7. **InjectGenerator -- duplicate type warning**: Two `[Inject]` fields of the same type, verify `JUI045` warning.
8. **BindingGenerator -- push mode**: Verify effect is emitted that reads signal and writes to element property.
9. **BindingGenerator -- push with converter**: Verify converter instance is created and `Convert()` is called in the effect.
10. **BindingGenerator -- pull mode**: Verify `RegisterValueChangedCallback` is emitted with callback invocation, no effect.
11. **BindingGenerator -- pull mode without callback**: Verify `JUI065` error when `Callback` is null.
12. **BindingGenerator -- once mode**: Verify direct assignment in `InitBindings` with no effect or callback.
13. **BindingGenerator -- sync mode**: Verify both push effect and pull callback are emitted with sync guard.
14. **BindingGenerator -- sync guard prevents loop**: Verify `_syncGuard_*` field is set before element write and checked before signal write.
15. **BindSyncAttribute -- generates same as Bind(Mode=Sync)**: Verify `[BindSync]` output matches `[Bind(Mode = BindingMode.Sync)]`.
16. **BindSyncAttribute -- with converter**: Verify both `Convert` and `ConvertBack` are used in forward and reverse directions.
17. **BindingGenerator -- element not found**: Reference a non-existent El constant, verify `JUI060` error.
18. **BindingGenerator -- non-signal field**: Apply `[Bind]` to a plain `int` field, verify `JUI064` error.
19. **BindingGenerator -- read-only signal with sync**: Apply `[BindSync]` to an `IReadOnlySignal<T>`, verify `JUI063` error.
20. **BindingGenerator -- converter type mismatch**: Specify a converter with wrong type parameters, verify `JUI061` error.
21. **BindingGenerator -- property inference**: Verify `Label` infers `text`, `Toggle` infers `value`, `VisualElement` infers `visible`.
22. **Generated code compiles**: Verify all generated binding code compiles without errors against a mock UI Toolkit API.

## Acceptance Criteria

- [ ] `InjectAttribute` supports `Optional` property (default false)
- [ ] `InjectGenerator` emits `InitScope()` with `Scope.Use<T>()` for required injections
- [ ] `InjectGenerator` emits `Scope.TryUse<T>()` for optional injections
- [ ] `InjectGenerator` rejects value types (`JUI041`), static members (`JUI043`), and non-components (`JUI044`)
- [ ] `BindAttribute` accepts `Element`, `Property`, `Converter`, `Mode`, and `Callback` parameters
- [ ] `BindSyncAttribute` is shorthand for `Bind(Mode = BindingMode.Sync)`
- [ ] Push mode generates a static-lambda effect reading the signal and writing to the element property
- [ ] Pull mode generates only a `RegisterValueChangedCallback` with named callback invocation
- [ ] Once mode generates a direct assignment with no effect or callback
- [ ] Sync mode generates both push effect and pull callback with a `_syncGuard_*` field
- [ ] Converters are instantiated as `private static readonly` fields and validated at compile time
- [ ] Element property is inferred from element type when not explicitly specified
- [ ] All binding modes use static lambdas with typed state to avoid closure allocations
- [ ] Cross-referencing with El constants validates element names at compile time
- [ ] All diagnostics (JUI040-JUI079) are emitted for the appropriate error conditions
- [ ] Generated code includes `// <auto-generated />` header
- [ ] All public APIs have XML documentation
