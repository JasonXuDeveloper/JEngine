# Section 12 — EffectGenerator

## Overview

EffectGenerator processes `[Effect]` attributes on methods to generate static-lambda effects with explicit dependency tracking and conditional execution support. It transforms declarative method annotations into imperative `UseEffect` calls in `InitEffects()`, handling simple effects (watch + run), conditional effects (`When`, `WhenAll`, `WhenAny`), and inverted conditions. For conditional effects, the generator emits an outer effect that monitors the condition signals and creates/disposes an inner effect that performs the actual work, with inner effects pooled to avoid allocation on rapid toggling.

## Dependencies

- Section 9 — Source Generator Project Setup (IIncrementalGenerator, DiagnosticDescriptors)
- Section 2 — Effect System & Batch (UseEffect, EffectPool, IEffect)

## File Structure

- `SourceGenerators/JEngine.JUI.Generators/EffectGenerator.cs`
- `Runtime/JUI/Attributes/EffectAttribute.cs`

## API Design

### EffectAttribute

```csharp
namespace JEngine.JUI;

/// <summary>
/// Marks a method as a reactive effect that re-runs when its watched signals change.
/// The generator emits UseEffect calls in InitEffects() with explicit dependency reads
/// that trigger auto-tracking.
///
/// The method must be parameterless and non-static. It must be declared on a class
/// that extends JUIComponent.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class EffectAttribute : Attribute
{
    /// <summary>
    /// Array of signal member names to watch. Each name must refer to a field or property
    /// of type Signal&lt;T&gt;, IReadOnlySignal&lt;T&gt;, or ComputedSignal&lt;T&gt; on the same class.
    /// The generator emits a force-read (e.g., <c>_ = self.Health.Value</c>) for each
    /// entry to register the effect as a subscriber via auto-tracking.
    /// </summary>
    public string[] Watch { get; set; }

    /// <summary>
    /// Optional single condition signal name. Must refer to a Signal&lt;bool&gt; or
    /// IReadOnlySignal&lt;bool&gt; on the same class.
    /// When specified, the effect only runs when the condition signal's value is true
    /// (or false, if <see cref="Invert"/> is true).
    /// Mutually exclusive with <see cref="WhenAll"/> and <see cref="WhenAny"/>.
    /// </summary>
    public string When { get; set; }

    /// <summary>
    /// Optional array of boolean signal names. The effect runs only when ALL condition
    /// signals are true (logical AND). Mutually exclusive with <see cref="When"/>.
    /// </summary>
    public string[] WhenAll { get; set; }

    /// <summary>
    /// Optional array of boolean signal names. The effect runs when ANY condition
    /// signal is true (logical OR). Mutually exclusive with <see cref="When"/>.
    /// </summary>
    public string[] WhenAny { get; set; }

    /// <summary>
    /// When true, inverts the condition logic:
    /// - <c>When</c>: effect runs when condition is false
    /// - <c>WhenAll</c>: effect runs when any condition is false (NOT ALL true)
    /// - <c>WhenAny</c>: effect runs when no condition is true (NONE true)
    /// Default is false.
    /// </summary>
    public bool Invert { get; set; } = false;
}
```

### Runtime Escape Hatch

```csharp
namespace JEngine.JUI;

/// <summary>
/// Runtime API for conditional effects, provided as an escape hatch when
/// the source-generated [Effect] attribute does not cover a use case.
/// Available as instance methods on JUIComponent.
/// </summary>
public abstract partial class JUIComponent
{
    /// <summary>
    /// Creates a conditional effect that activates when <paramref name="condition"/>
    /// returns true and deactivates (with cleanup) when it returns false.
    /// The condition function is itself tracked as an effect, so it re-evaluates
    /// when any signals read inside it change.
    /// </summary>
    /// <typeparam name="TState">Typed state to avoid closure allocations.</typeparam>
    /// <param name="condition">
    /// Function that reads boolean signal(s) and returns whether the inner effect should be active.
    /// </param>
    /// <param name="body">
    /// The inner effect body. Runs when condition is true and watched signals change.
    /// </param>
    /// <param name="cleanup">
    /// Optional cleanup action invoked when the condition transitions from true to false.
    /// Use for releasing resources, resetting UI state, etc.
    /// </param>
    /// <param name="state">Typed state passed to all three delegates.</param>
    /// <returns>An IDisposable that tears down both the outer and inner effects.</returns>
    protected IDisposable UseConditionalEffect<TState>(
        Func<TState, bool> condition,
        Action<TState> body,
        Action<TState> cleanup,
        TState state);
}
```

## Data Structures

- `EffectInfo` -- internal struct used by EffectGenerator: `{ MethodName, WatchSignals: string[], ConditionKind, ConditionSignals: string[], Invert: bool, ContainingClass, Location }`.
- `ConditionKind` -- internal enum: `None`, `When`, `WhenAll`, `WhenAny`.
- Inner effects are pooled via `EffectPool<TState>` to avoid GC allocation when conditions toggle rapidly.
- Outer (condition-monitoring) effects track only the condition signal(s); inner effects track only the Watch signal(s). This separation ensures that toggling the condition does not re-read the watched signals unnecessarily.

## Implementation Notes

### Simple Effects (no condition)

For a method with `[Effect(Watch = new[] { "A", "B" })]`, the generator emits:

```csharp
// In InitEffects():
UseEffect(static (MyComponent self) =>
{
    _ = self.A.Value;   // Force-read: registers auto-tracking dependency
    _ = self.B.Value;   // Force-read: registers auto-tracking dependency
    self.MyMethod();
}, this);
```

The discard reads (`_ = ...`) ensure that `EffectTracker.Current` (set during `Effect.Run()`) sees the signal accesses and subscribes the effect to those signals.

### Conditional Effects (When)

For `[Effect(Watch = new[] { "Shield" }, When = "IsShielded")]`, the generator emits an outer/inner effect pattern:

```csharp
// In InitEffects():
{
    IEffect innerEffect = null;

    // Outer effect: monitors condition
    UseEffect(static (MyComponent self) =>
    {
        var active = self.IsShielded.Value;

        if (active && innerEffect == null)
        {
            // Condition became true: create inner effect
            innerEffect = UseEffect(static (MyComponent s) =>
            {
                _ = s.Shield.Value;
                s.UpdateShieldBar();
            }, self);
        }
        else if (!active && innerEffect != null)
        {
            // Condition became false: dispose inner effect
            innerEffect.Dispose();
            innerEffect = null;
        }
    }, this);
}
```

The outer effect tracks only `IsShielded`. When it transitions to `true`, the inner effect is created and begins tracking `Shield`. When it transitions to `false`, the inner effect is disposed (unsubscribing from `Shield`).

### Conditional Effects (WhenAll)

For `[Effect(Watch = new[] { "X" }, WhenAll = new[] { "A", "B" })]`:

```csharp
// In InitEffects():
{
    IEffect innerEffect = null;

    UseEffect(static (MyComponent self) =>
    {
        var active = self.A.Value && self.B.Value;

        if (active && innerEffect == null)
        {
            innerEffect = UseEffect(static (MyComponent s) =>
            {
                _ = s.X.Value;
                s.TargetMethod();
            }, self);
        }
        else if (!active && innerEffect != null)
        {
            innerEffect.Dispose();
            innerEffect = null;
        }
    }, this);
}
```

### Conditional Effects (WhenAny)

For `[Effect(Watch = new[] { "X" }, WhenAny = new[] { "A", "B" })]`:

```csharp
// Condition expression becomes:
var active = self.A.Value || self.B.Value;
// Remainder identical to WhenAll pattern
```

### Inverted Conditions

When `Invert = true`, the condition boolean is negated:

```csharp
// [Effect(Watch = ..., When = "IsShielded", Invert = true)]
var active = !self.IsShielded.Value;

// [Effect(Watch = ..., WhenAll = new[] { "A", "B" }, Invert = true)]
var active = !(self.A.Value && self.B.Value);

// [Effect(Watch = ..., WhenAny = new[] { "A", "B" }, Invert = true)]
var active = !(self.A.Value || self.B.Value);
```

### Inner Effect Pooling

Inner effects are rented from `EffectPool<TState>` on creation and returned on disposal. This means rapid toggling of a condition (e.g., a shield flickering on/off) does not allocate new effect objects each time. The pool recycles the `Effect<TState>` instance, resetting its action and state on rent.

### Static Lambda Requirement

All emitted lambdas are `static` to guarantee zero closure allocations. The component instance is passed explicitly as the `TState` parameter. This is critical for hot-path performance -- effects may run every frame when signals change frequently.

## Source Generator Notes

### EffectGenerator

```csharp
[Generator]
public sealed class EffectGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find methods with [Effect] attribute
        var effectMethods = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "JEngine.JUI.EffectAttribute",
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, ct) => ExtractEffectInfo(ctx, ct));

        // Group by containing class
        var grouped = effectMethods.Collect()
            .SelectMany(static (items, _) =>
                items.GroupBy(i => i.ContainingClass));

        // Emit InitEffects() for each class
        context.RegisterSourceOutput(grouped,
            static (spc, group) => EmitInitEffects(spc, group));
    }
}
```

### Validation Rules

The generator validates each `[Effect]` usage before emitting code:

| Condition | Diagnostic | Severity |
|---|---|---|
| Watch array member is not a signal type | JUI080 | Error |
| When member is not `Signal<bool>` or `IReadOnlySignal<bool>` | JUI081 | Error |
| Method has parameters | JUI082 | Error |
| Watch array is empty | JUI083 | Warning |
| Method is static | JUI084 | Error |
| Class does not extend JUIComponent | JUI085 | Error |
| When and WhenAll/WhenAny both specified | JUI086 | Error |
| WhenAll/WhenAny member is not boolean signal | JUI087 | Error |

### Generated File Structure

For a class `PlayerHUD` with three `[Effect]` methods, the generator emits:

```
PlayerHUD.Effect.g.cs
```

containing:

```csharp
// <auto-generated />
namespace HotUpdate.Code;

partial class PlayerHUD
{
    partial void InitEffects()
    {
        // Effect for UpdateHealthBar
        UseEffect(static (PlayerHUD self) => { ... }, this);

        // Effect for UpdateManaBar
        UseEffect(static (PlayerHUD self) => { ... }, this);

        // Conditional effect for UpdateShieldBar
        {
            IEffect innerEffect = null;
            UseEffect(static (PlayerHUD self) => { ... }, this);
        }
    }
}
```

## Usage Examples

### Simple Effect

```csharp
[UIComponent("PlayerHUD.uxml", "PlayerHUD.uss")]
public partial class PlayerHUD : JUIComponent
{
    private readonly Signal<int> _health = new(100);
    private readonly Signal<int> _maxHealth = new(100);

    public Signal<int> Health => _health;
    public Signal<int> MaxHealth => _maxHealth;

    [Effect(Watch = new[] { nameof(Health), nameof(MaxHealth) })]
    private void UpdateHealthBar()
    {
        var pct = (float)Health.Value / MaxHealth.Value;
        _el_healthFill.style.width = new StyleLength(
            new Length(pct * 100f, LengthUnit.Percent));

        _el_healthFill.EnableInClassList(Cls.BarFillCritical, pct < 0.2f);
        _el_healthFill.EnableInClassList(Cls.BarFillLow, pct < 0.5f && pct >= 0.2f);
    }
}

// Generator emits in InitEffects():
// UseEffect(static (PlayerHUD self) => {
//     _ = self.Health.Value;
//     _ = self.MaxHealth.Value;
//     self.UpdateHealthBar();
// }, this);
```

### Conditional Effect with When

```csharp
[UIComponent("PlayerHUD.uxml")]
public partial class PlayerHUD : JUIComponent
{
    public Signal<float> Shield { get; } = new(0f);
    public Signal<bool> IsShielded { get; } = new(false);

    [Effect(Watch = new[] { nameof(Shield) }, When = nameof(IsShielded))]
    private void UpdateShieldBar()
    {
        _el_shieldFill.style.width = new StyleLength(
            new Length(Shield.Value * 100f, LengthUnit.Percent));
        _el_shieldContainer.style.display = DisplayStyle.Flex;
    }
}

// When IsShielded becomes false:
//   - inner effect disposes (stops watching Shield)
//   - shield bar stops updating
// When IsShielded becomes true:
//   - inner effect created (starts watching Shield)
//   - shield bar updates immediately
```

### Conditional Effect with WhenAll

```csharp
[UIComponent("BossUI.uxml")]
public partial class BossUI : JUIComponent
{
    public Signal<float> BossHealth { get; } = new(1f);
    public Signal<bool> IsBossPhase { get; } = new(false);
    public Signal<bool> IsPlayerAlive { get; } = new(true);

    [Effect(Watch = new[] { nameof(BossHealth) },
            WhenAll = new[] { nameof(IsBossPhase), nameof(IsPlayerAlive) })]
    private void UpdateBossHealthBar()
    {
        // Only runs when BOTH IsBossPhase AND IsPlayerAlive are true
        _el_bossBar.style.width = new StyleLength(
            new Length(BossHealth.Value * 100f, LengthUnit.Percent));
    }
}
```

### Conditional Effect with WhenAny

```csharp
[UIComponent("AlertPanel.uxml")]
public partial class AlertPanel : JUIComponent
{
    public Signal<string> AlertText { get; } = new("");
    public Signal<bool> HasCriticalAlert { get; } = new(false);
    public Signal<bool> HasWarningAlert { get; } = new(false);

    [Effect(Watch = new[] { nameof(AlertText) },
            WhenAny = new[] { nameof(HasCriticalAlert), nameof(HasWarningAlert) })]
    private void ShowAlert()
    {
        // Runs when EITHER HasCriticalAlert OR HasWarningAlert is true
        _el_alertLabel.text = AlertText.Value;
        _el_alertPanel.style.display = DisplayStyle.Flex;
    }
}
```

### Inverted Condition

```csharp
[UIComponent("LoadingScreen.uxml")]
public partial class LoadingScreen : JUIComponent
{
    public Signal<float> Progress { get; } = new(0f);
    public Signal<bool> IsLoaded { get; } = new(false);

    [Effect(Watch = new[] { nameof(Progress) }, When = nameof(IsLoaded), Invert = true)]
    private void UpdateLoadingBar()
    {
        // Runs when IsLoaded is FALSE (i.e., still loading)
        // Stops running once IsLoaded becomes true
        _el_progressBar.style.width = new StyleLength(
            new Length(Progress.Value * 100f, LengthUnit.Percent));
    }
}
```

### Runtime Escape Hatch

```csharp
// For cases where [Effect] attribute is not flexible enough:
protected override void OnMount()
{
    // Dynamic condition based on runtime logic
    UseConditionalEffect(
        condition: static (self) => self.IsShielded.Value && self.Shield.Value > 0f,
        body: static (self) =>
        {
            _ = self.ShieldDecayRate.Value;
            self.UpdateShieldDecay();
        },
        cleanup: static (self) =>
        {
            self._el_shieldDecayIndicator.style.display = DisplayStyle.None;
        },
        state: this);
}
```

## Test Plan

1. **Simple effect runs on watched signal change**: Create a component with `[Effect(Watch = new[] { "A" })]`, change signal A, verify the method is called.
2. **Simple effect tracks all watched signals**: Watch signals A and B, change only B, verify the method is called. Change only A, verify the method is called again.
3. **Conditional effect only runs when When is true**: Set `When` condition to false, change watched signal, verify method is NOT called. Set condition to true, verify method runs.
4. **Cleanup on condition becoming false**: Condition is true (inner effect active), set condition to false, verify inner effect is disposed (method stops running on signal changes).
5. **WhenAll requires all conditions true**: Set two `WhenAll` conditions: A=true B=false, change watched signal, verify method does NOT run. Set B=true, verify method runs.
6. **WhenAny requires any condition true**: Set two `WhenAny` conditions: A=false B=false, change watched signal, verify method does NOT run. Set A=true, verify method runs.
7. **Invert = true runs when condition is false**: `When = "X", Invert = true`, set X to false, change watched signal, verify method runs. Set X to true, verify method stops.
8. **Inner effects pooled (no alloc on toggle)**: Toggle a condition true/false 10 times, verify the inner effect instance is reused from the pool (no new allocations after first creation).
9. **When and WhenAll mutually exclusive**: Specify both `When` and `WhenAll`, verify `JUI086` diagnostic error.
10. **Watch member not a signal**: Reference a plain `int` field in Watch, verify `JUI080` error.
11. **When member not bool signal**: Reference a `Signal<int>` in When, verify `JUI081` error.
12. **Method with parameters rejected**: Apply `[Effect]` to a method with parameters, verify `JUI082` error.
13. **Empty Watch array**: Specify `Watch = new string[0]`, verify `JUI083` warning.
14. **Static method rejected**: Apply `[Effect]` to a static method, verify `JUI084` error.
15. **Non-component class rejected**: Apply `[Effect]` on a class not extending JUIComponent, verify `JUI085` error.
16. **WhenAll/WhenAny non-bool signal**: Reference a `Signal<string>` in WhenAll, verify `JUI087` error.
17. **Multiple effects on one class**: Declare three `[Effect]` methods on one class, verify all three are emitted in `InitEffects()`.
18. **Generated code compiles**: Verify all emitted code compiles without errors against mock runtime types.
19. **Runtime UseConditionalEffect**: Create a conditional effect via the escape hatch, toggle condition, verify body runs and cleanup fires.

## Acceptance Criteria

- [ ] `EffectAttribute` supports `Watch`, `When`, `WhenAll`, `WhenAny`, and `Invert` properties
- [ ] `When`, `WhenAll`, and `WhenAny` are mutually exclusive (JUI086 on conflict)
- [ ] Simple effects emit `UseEffect` with force-read discards for each watched signal
- [ ] Conditional effects emit outer/inner effect pattern with create/dispose lifecycle
- [ ] `WhenAll` generates `&&` condition across all boolean signals
- [ ] `WhenAny` generates `||` condition across all boolean signals
- [ ] `Invert = true` negates the condition expression
- [ ] All emitted lambdas are `static` with typed state (zero closure allocations)
- [ ] Inner effects are pooled via `EffectPool<TState>` for allocation-free toggling
- [ ] Method must be parameterless (JUI082), non-static (JUI084), on a JUIComponent subclass (JUI085)
- [ ] Watch members must be signal types (JUI080); condition members must be boolean signals (JUI081, JUI087)
- [ ] `UseConditionalEffect<TState>` runtime API provides escape hatch for dynamic conditions
- [ ] Generated `InitEffects()` is a partial void method, omitted when no effects are declared
- [ ] Generated code includes `// <auto-generated />` header
- [ ] All public APIs have XML documentation
- [ ] Multiple `[Effect]` methods on one class are all emitted in a single `InitEffects()` method
