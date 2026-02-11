# Section 7 — Control Flow: Show & Switch

## Overview

Show is a reactive if/else for UI subtrees. It creates or destroys components based on a boolean signal. Switch is a reactive switch/case that renders one of N components based on a signal's value. Both support KeepAlive mode (toggle `display:none` instead of create/destroy) and Transition animations.

## Dependencies

- Section 6 (Component)

## File Structure

```
Runtime/JUI/Components/
  Show.cs
  Switch.cs
```

## API Design

```csharp
/// <summary>
/// Reactive conditional renderer that creates or destroys a component
/// subtree based on a boolean signal.
/// </summary>
/// <remarks>
/// <para>
/// By default, <c>Show</c> fully creates the <see cref="Then"/> component when
/// <see cref="When"/> is <c>true</c> and disposes it when <c>false</c>.
/// If <see cref="Else"/> is provided, the alternate component is mounted while
/// <see cref="When"/> is <c>false</c>.
/// </para>
/// <para>
/// With <see cref="KeepAlive"/> enabled, both branches are created on first
/// toggle and subsequently toggled via <c>display:none</c> / <c>display:flex</c>.
/// <see cref="Component.OnMount"/> and <see cref="Component.OnUnmount"/> still
/// fire, but <see cref="Component.Dispose"/> only fires when the
/// <c>Show</c> itself is disposed.
/// </para>
/// </remarks>
public class Show : Component
{
    /// <summary>
    /// Boolean signal controlling which branch is visible.
    /// When <c>true</c>, <see cref="Then"/> is rendered; when <c>false</c>,
    /// <see cref="Else"/> is rendered (if provided).
    /// </summary>
    public required IReadOnlySignal<bool> When { get; init; }

    /// <summary>
    /// Factory for the component rendered when <see cref="When"/> is <c>true</c>.
    /// </summary>
    public required Func<Component> Then { get; init; }

    /// <summary>
    /// Optional factory for the component rendered when <see cref="When"/> is
    /// <c>false</c>. If omitted, nothing is rendered in the false branch.
    /// </summary>
    public Func<Component> Else { get; init; }

    /// <summary>
    /// When <c>true</c>, branch components are created once on first toggle
    /// and hidden/shown via <c>display:none</c> instead of being disposed and
    /// recreated. Defaults to <c>false</c>.
    /// </summary>
    public bool KeepAlive { get; init; } = false;

    /// <summary>
    /// Optional transition applied when mounting and unmounting branch
    /// components. Enter animation plays after <see cref="Component.OnMount"/>;
    /// exit animation plays before disposal (or hide in KeepAlive mode).
    /// </summary>
    public ITransition Transition { get; init; }
}

/// <summary>
/// Reactive multi-branch renderer that mounts one of N components based on
/// a signal's current value, similar to a <c>switch</c> statement.
/// </summary>
/// <typeparam name="T">
/// The type of the discriminator value. Must be usable as a dictionary key
/// (i.e., must implement <see cref="IEquatable{T}"/> or have a valid
/// <see cref="object.GetHashCode"/> / <see cref="object.Equals(object)"/>).
/// </typeparam>
/// <remarks>
/// Only one case is mounted at a time. When <see cref="Value"/> changes, the
/// current case is disposed and the matching case from <see cref="Cases"/> is
/// created and mounted. If no match exists, <see cref="Default"/> is used.
/// With <see cref="KeepAlive"/> enabled, visited cases are cached and toggled
/// via <c>display:none</c>.
/// </remarks>
public class Switch<T> : Component
{
    /// <summary>
    /// Signal whose value selects which case to render.
    /// </summary>
    public required IReadOnlySignal<T> Value { get; init; }

    /// <summary>
    /// Map from discriminator values to component factories. Only the factory
    /// matching the current <see cref="Value"/> is invoked.
    /// </summary>
    public required Dictionary<T, Func<Component>> Cases { get; init; }

    /// <summary>
    /// Optional factory invoked when <see cref="Value"/> does not match any
    /// key in <see cref="Cases"/>. If omitted, the slot is left empty.
    /// </summary>
    public Func<Component> Default { get; init; }

    /// <summary>
    /// When <c>true</c>, case components are created on first visit and
    /// subsequently shown/hidden via <c>display:none</c> rather than disposed
    /// and recreated. Defaults to <c>false</c>.
    /// </summary>
    public bool KeepAlive { get; init; } = false;

    /// <summary>
    /// Optional transition applied when switching between cases.
    /// Exit animation plays on the outgoing case before the incoming case's
    /// enter animation.
    /// </summary>
    public ITransition Transition { get; init; }
}
```

## Data Structures

| Type | Role |
|------|------|
| `IReadOnlySignal<bool>` | Condition signal for `Show` |
| `IReadOnlySignal<T>` | Discriminator signal for `Switch<T>` |
| `ITransition` | Interface for enter/exit animations (defined in Section 11) |
| `Dictionary<T, Func<Component>>` | Case registry mapping values to component factories |

Internal bookkeeping per instance:

```
Show:
  _thenInstance  : Component?      // current or cached Then component
  _elseInstance  : Component?      // current or cached Else component
  _activeBranch  : bool            // tracks which branch is mounted

Switch<T>:
  _activeKey     : T?              // currently mounted key
  _activeCase    : Component?      // currently mounted component
  _cache         : Dictionary<T, Component>  // KeepAlive cache (lazy)
```

## Implementation Notes

### Show Behavior

**Default mode (KeepAlive = false):**

1. Effect subscribes to `When`.
2. `When = true`: invoke `Then()`, mount the returned component as a child, run full lifecycle (`OnMount`).
3. `When = false`: dispose the Then component (full lifecycle including `OnUnmount`, `Dispose`). If `Else` is provided, invoke `Else()` and mount it.
4. Toggling repeats: each transition is a full create/dispose cycle.

**KeepAlive mode:**

1. On first `When = true`: create Then component, mount it.
2. On first `When = false`: create Else component (if provided), mount it. Hide Then via `style.display = DisplayStyle.None`.
3. Subsequent toggles: show one, hide the other. Fire `OnMount`/`OnUnmount` on show/hide.
4. `Dispose` fires for both only when the `Show` component itself is disposed.

**Transition integration:**

- On mount: component is created, `OnMount` fires, then enter animation plays.
- On unmount (default mode): exit animation plays, animation completes (awaited), then component is disposed.
- On unmount (KeepAlive): exit animation plays, animation completes, then `display:none` is set.

### Switch Behavior

1. Effect subscribes to `Value`.
2. On value change:
   - Default mode: dispose current case component, look up new value in `Cases`, invoke factory, mount.
   - KeepAlive mode: hide current case (`display:none`), check cache for new value, create if not cached, show.
3. If value not found in `Cases`:
   - If `Default` is provided, invoke it as the fallback.
   - If `Default` is not provided, the slot is empty (no component mounted, no error).
4. Transition: exit animation on outgoing case completes before enter animation on incoming case starts.

### Edge Cases

- Rapid toggling during transition: queue the final state, cancel intermediate transitions.
- `Show` with `When` initially `false` and no `Else`: nothing rendered, no error.
- `Switch` with empty `Cases` dictionary: always uses `Default` or empty slot.
- `Switch` value changes to the same value: no-op (skip dispose/create).

## Source Generator Notes

No source generation required for Show or Switch. These are concrete component classes used directly via object initializer syntax.

The source generator for `Component` (Section 6) already handles the child mounting lifecycle that Show and Switch delegate to.

## Usage Examples

```csharp
// Simple show/hide
new Show
{
    When = isTooltipVisible,
    Then = () => new ItemTooltip(item)
};

// Show with else branch
new Show
{
    When = auth.IsLoggedIn,
    Then = () => new ProfilePanel(),
    Else = () => new LoginForm()
};

// KeepAlive for heavy components (e.g., inventory with scroll state)
new Show
{
    When = input.IsInventoryOpen,
    Then = () => new InventoryScreen(),
    KeepAlive = true
};

// Show with transition
new Show
{
    When = showDialog,
    Then = () => new ConfirmDialog(),
    Transition = new FadeTransition { Duration = 0.2f }
};

// Switch on enum for tabbed settings
new Switch<SettingsTab>
{
    Value = activeTab,
    Cases = new()
    {
        [SettingsTab.Audio] = () => new AudioPanel(),
        [SettingsTab.Video] = () => new VideoPanel(),
        [SettingsTab.Controls] = () => new ControlsPanel(),
    },
    Default = () => new Label("Unknown tab")
};

// Switch with KeepAlive for expensive panels
new Switch<SettingsTab>
{
    Value = activeTab,
    Cases = new()
    {
        [SettingsTab.Audio] = () => new AudioPanel(),
        [SettingsTab.Video] = () => new VideoPanel(),
    },
    KeepAlive = true
};
```

## Test Plan

1. **Show creates on true**: Set `When = true`, verify `Then` component is created and mounted.
2. **Show destroys on false**: Toggle `When` to `false`, verify `Then` component is disposed.
3. **Show with Else swaps branches**: Toggle `When`, verify Then disposed and Else created (and vice versa).
4. **KeepAlive preserves instance**: Toggle `When` twice, verify the returned component reference is the same object.
5. **KeepAlive fires OnMount/OnUnmount but not Dispose**: Verify lifecycle hooks fire on toggle, Dispose only on Show disposal.
6. **KeepAlive sets display:none**: Verify hidden branch has `style.display == DisplayStyle.None`.
7. **Switch renders matching case**: Set `Value` to a key in `Cases`, verify correct component mounted.
8. **Switch disposes previous case**: Change `Value`, verify old component disposed before new one created.
9. **Switch Default fallback**: Set `Value` to key not in `Cases`, verify `Default` factory invoked.
10. **Switch empty slot**: Set `Value` to key not in `Cases` with no `Default`, verify no component and no error.
11. **Switch KeepAlive preserves all visited cases**: Visit multiple cases, verify instances cached and reused.
12. **Switch same-value no-op**: Set `Value` to current value, verify no dispose/create cycle.
13. **Transition hooks called on mount/unmount**: Verify enter animation starts after mount, exit completes before dispose.
14. **Rapid toggling**: Toggle `When` multiple times during a transition, verify final state is correct.

## Acceptance Criteria

- [ ] `Show` creates `Then` component when `When` signal is `true`
- [ ] `Show` disposes `Then` and optionally creates `Else` when `When` is `false`
- [ ] `KeepAlive` mode reuses component instances across toggles
- [ ] `KeepAlive` fires `OnMount`/`OnUnmount` on toggle but defers `Dispose` to parent disposal
- [ ] Hidden components use `display:none`, shown components use `display:flex`
- [ ] `ITransition` enter/exit animations integrate with mount/unmount lifecycle
- [ ] `Switch<T>` renders the component matching `Value` from `Cases`
- [ ] `Switch<T>` disposes old case before creating new case (default mode)
- [ ] `Switch<T>` falls back to `Default` when `Value` has no match
- [ ] `Switch<T>` leaves slot empty when no match and no `Default`
- [ ] `Switch<T>` KeepAlive caches visited cases
- [ ] Rapid toggles during transitions resolve to the correct final state
- [ ] All tests from the test plan pass
