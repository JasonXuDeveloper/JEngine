# Section 8 — Control Flow: For, ErrorBoundary, Portal

## Overview

`For<T>` maps a `ReactiveList<T>` to components with keyed reconciliation, producing minimal DOM mutations on list changes. `ErrorBoundary` catches exceptions from `Render`, effects, and event handlers, displaying fallback UI with optional retry. `Portal` renders content into a different visual location (e.g., a global overlay layer) while maintaining logical ownership in the component tree for DI and lifecycle purposes.

## Dependencies

- Section 6 (Component)
- Section 3 (ReactiveList)

## File Structure

```
Runtime/JUI/Components/
  For.cs
  ErrorBoundary.cs
  Portal.cs
```

## API Design

```csharp
/// <summary>
/// Reactive list renderer that maps each item in a <see cref="ReactiveList{T}"/>
/// to a component, performing keyed reconciliation to minimize DOM mutations
/// on list changes.
/// </summary>
/// <typeparam name="T">The element type of the source list.</typeparam>
/// <remarks>
/// <para>
/// Each item is wrapped in its own <see cref="IReadOnlySignal{T}"/> so that
/// in-place value updates trigger re-render of only the affected component,
/// without unmount/remount.
/// </para>
/// <para>
/// When a <see cref="Key"/> function is provided, identity-based diffing is
/// used: reorders produce DOM moves (not recreations), preserving component
/// state. Without <see cref="Key"/>, index-based tracking is used and
/// reorders cause dispose + recreate.
/// </para>
/// </remarks>
public class For<T> : Component
{
    /// <summary>
    /// The reactive list to iterate over. Changes to this list (add, remove,
    /// move, replace) trigger the reconciliation algorithm.
    /// </summary>
    public required ReactiveList<T> Each { get; init; }

    /// <summary>
    /// Factory invoked for each item. Receives a signal wrapping the item's
    /// current value and a signal wrapping the item's current index.
    /// </summary>
    /// <remarks>
    /// The item signal updates in place when the underlying value changes
    /// (e.g., via <c>ReactiveList.Replace</c>), allowing the component to
    /// react without being recreated.
    /// </remarks>
    public required Func<IReadOnlySignal<T>, IReadOnlySignal<int>, Component> Render { get; init; }

    /// <summary>
    /// Optional key extractor for identity-based reconciliation. When
    /// provided, the diff algorithm tracks items by key rather than index,
    /// enabling state-preserving reorders.
    /// </summary>
    /// <remarks>
    /// Keys must be unique within the list. Duplicate keys cause undefined
    /// reconciliation behavior.
    /// </remarks>
    public Func<T, object> Key { get; init; }

    /// <summary>
    /// Optional factory for a component displayed when the list is empty.
    /// Created when the list transitions to zero items, disposed when items
    /// are added.
    /// </summary>
    public Func<Component> Empty { get; init; }

    /// <summary>
    /// When <c>true</c>, enables UI Toolkit list virtualization. Only items
    /// within the scroll viewport are instantiated. Requires
    /// <see cref="EstimatedItemHeight"/> for layout calculations.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool Virtualize { get; init; } = false;

    /// <summary>
    /// Estimated height in pixels for each item, used by the virtualizer
    /// to calculate scroll extent and determine which items are visible.
    /// Only relevant when <see cref="Virtualize"/> is <c>true</c>.
    /// Defaults to <c>40f</c>.
    /// </summary>
    public float EstimatedItemHeight { get; init; } = 40f;
}

/// <summary>
/// Catches exceptions thrown during rendering, effect execution, or event
/// handling within its <see cref="Content"/> subtree, and displays a
/// <see cref="Fallback"/> component instead.
/// </summary>
/// <remarks>
/// <para>
/// <c>ErrorBoundary</c> wraps a content subtree and intercepts:
/// <list type="bullet">
///   <item><see cref="Component.Render"/> exceptions</item>
///   <item>Effect callback exceptions (via EffectRunner notification)</item>
///   <item>Event handler exceptions</item>
/// </list>
/// </para>
/// <para>
/// When an error is caught, the content subtree is disposed, the
/// <see cref="Fallback"/> is created with the caught exception, and
/// <see cref="OnError"/> is invoked if provided. <see cref="RetryRender"/>
/// can be called to attempt recovery up to <see cref="MaxRetries"/> times.
/// </para>
/// </remarks>
public class ErrorBoundary : Component
{
    /// <summary>
    /// Factory for the content subtree that this boundary protects.
    /// </summary>
    public required Func<Component> Content { get; init; }

    /// <summary>
    /// Factory invoked when an exception is caught. Receives the caught
    /// exception to enable contextual error display.
    /// </summary>
    public required Func<Exception, Component> Fallback { get; init; }

    /// <summary>
    /// Optional callback invoked when an exception is caught, before the
    /// fallback is rendered. Useful for logging or telemetry.
    /// </summary>
    public Action<Exception> OnError { get; init; }

    /// <summary>
    /// When <c>true</c>, <see cref="RetryRender"/> can be called to attempt
    /// re-creation of the <see cref="Content"/> subtree. Defaults to
    /// <c>true</c>.
    /// </summary>
    public bool AllowRetry { get; init; } = true;

    /// <summary>
    /// Maximum number of times <see cref="RetryRender"/> can be called before
    /// it becomes a no-op. Prevents infinite error loops. Defaults to
    /// <c>3</c>.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Disposes the current <see cref="Fallback"/> component and re-creates
    /// the <see cref="Content"/> subtree. Increments the internal retry
    /// counter. Becomes a no-op when <see cref="AllowRetry"/> is <c>false</c>
    /// or <see cref="MaxRetries"/> has been reached.
    /// </summary>
    public void RetryRender();
}

/// <summary>
/// Renders its <see cref="Content"/> into a different
/// <see cref="VisualElement"/> target while maintaining logical ownership
/// in the parent's component tree.
/// </summary>
/// <remarks>
/// <para>
/// The portal's content is visually attached to <see cref="Target"/> (or
/// the element associated with <see cref="TargetLayer"/>), but logically
/// remains part of the parent component's subtree. This means:
/// <list type="bullet">
///   <item>Scoped DI resolution walks the logical parent chain, not the
///   visual tree.</item>
///   <item>Disposing the parent disposes the portal content.</item>
///   <item>Lifecycle hooks (<see cref="Component.OnMount"/>,
///   <see cref="Component.OnUnmount"/>) fire normally.</item>
/// </list>
/// </para>
/// <para>
/// Common use cases: tooltips, modals, context menus, and notifications
/// that must render above all other content regardless of nesting depth.
/// </para>
/// </remarks>
public class Portal : Component
{
    /// <summary>
    /// Factory for the component to render at the target location.
    /// </summary>
    public required Func<Component> Content { get; init; }

    /// <summary>
    /// The <see cref="VisualElement"/> to attach the content to. The portal
    /// content is added as a child of this element.
    /// </summary>
    public required VisualElement Target { get; init; }

    /// <summary>
    /// Optional UI layer enum value. When set, the portal resolves
    /// <see cref="Target"/> automatically from the layer system instead
    /// of requiring an explicit <see cref="VisualElement"/> reference.
    /// Takes precedence over <see cref="Target"/> if both are specified.
    /// </summary>
    public UILayer? TargetLayer { get; init; }
}
```

## Data Structures

### For<T> Internal State

```
_childEntries : List<ForEntry<T>>   // ordered list of active entries

struct ForEntry<T>
{
    object Key;                      // identity key (from Key func or index)
    Signal<T> ItemSignal;            // mutable signal wrapping current value
    Signal<int> IndexSignal;         // mutable signal wrapping current index
    Component Instance;              // mounted component
}
```

### For<T> Diff Result

```
struct DiffOperation
{
    DiffOpKind Kind;    // Add, Remove, Move
    int OldIndex;       // source index (Remove, Move)
    int NewIndex;       // destination index (Add, Move)
    object Key;         // item key
}

enum DiffOpKind { Add, Remove, Move }
```

### ErrorBoundary Internal State

```
_retryCount     : int              // number of retries attempted
_isFaulted      : bool             // true after error caught
_contentInstance : Component?      // current content (null when faulted)
_fallbackInstance: Component?      // current fallback (null when healthy)
_caughtException : Exception?     // most recent exception
```

## Implementation Notes

### For<T> Keyed Diff Algorithm

**Without Key (index-based tracking):**

1. Subscribe to `ReactiveList<T>.CollectionChanged`.
2. On add: create new `ForEntry`, invoke `Render`, mount at index.
3. On remove: dispose entry at index, remove from `_childEntries`, update subsequent index signals.
4. On reorder: treat as remove + add at new position. Component state is lost.
5. On replace: update `ItemSignal.Value` in place. No unmount/remount.

**With Key (identity-based tracking):**

1. On any list mutation, compute the new key sequence from the updated list.
2. Diff old key sequence against new key sequence to produce `DiffOperation[]`.
3. Apply operations in order:
   - `Remove`: dispose the component, remove from `_childEntries`.
   - `Add`: create new `ForEntry`, invoke `Render`, insert into `_childEntries` and DOM.
   - `Move`: reorder the `VisualElement` in the DOM, update `IndexSignal.Value`. No component recreation. State is preserved.
4. After all operations, update all `IndexSignal` values to reflect final positions.

**Empty state:**

- When list transitions from non-empty to empty: dispose all entries, create `Empty` component if provided.
- When list transitions from empty to non-empty: dispose `Empty` component, create entries.

**Virtualization (Virtualize = true):**

- Wraps content in a `ScrollView` with pooled item elements.
- Only items within the viewport (plus a small buffer) have mounted components.
- Scroll position changes trigger mount/unmount of items entering/leaving the viewport.
- `EstimatedItemHeight` used for scroll thumb sizing and viewport calculations.

### ErrorBoundary

**Error interception points:**

1. **Render exceptions**: wrap `Content()` invocation in try/catch.
2. **Effect exceptions**: the `EffectRunner` notifies the nearest ancestor `ErrorBoundary` when an effect callback throws. The notification walks up the logical component tree.
3. **Handler exceptions**: event handler delegates registered through the component system are wrapped in try/catch that forwards to the boundary.

**Error handling flow:**

1. Exception caught at any interception point.
2. Invoke `OnError(exception)` if provided.
3. Dispose the `Content` subtree (full lifecycle).
4. Create `Fallback(exception)` and mount it.
5. Set `_isFaulted = true`.

**Retry flow:**

1. `RetryRender()` called (e.g., from a button in the fallback UI).
2. If `!AllowRetry || _retryCount >= MaxRetries`: return (no-op).
3. Increment `_retryCount`.
4. Dispose `Fallback` component.
5. Re-invoke `Content()` factory, mount the result.
6. Set `_isFaulted = false`.
7. If the re-created content throws again, the cycle repeats from step 1.

**Faulted effects:**

- Effects that threw are NOT automatically re-subscribed on retry. This prevents infinite error loops where a faulted effect immediately throws again. The component must explicitly re-create any effects it needs.

### Portal

**Mount flow:**

1. `Portal.OnMount()` fires.
2. Invoke `Content()` factory, creating the component.
3. Resolve target: if `TargetLayer` is set, look up the layer's root `VisualElement`; otherwise use `Target`.
4. Attach the content's `VisualElement` to the resolved target.
5. Fire content component's `OnMount`.

**Logical vs. visual tree:**

- The content's `VisualElement` lives in the visual tree under `Target`.
- The content's `Component` lives in the logical tree under the `Portal`'s parent.
- DI scope resolution follows the logical tree: `[Inject]` on portal content resolves from the portal's parent scope, not the target's scope.

**Dispose flow:**

1. Parent component disposes (or `Portal` is removed from tree).
2. Portal disposes content component (full lifecycle).
3. Content's `VisualElement` is removed from `Target`.

## Source Generator Notes

No source generation is required for `For<T>`, `ErrorBoundary`, or `Portal`. These are concrete component classes used via object initializer syntax.

The source generator for `Component` (Section 6) already handles the child mounting lifecycle that these components delegate to. `ErrorBoundary` hooks into the EffectRunner's error notification mechanism, which is a runtime concern.

## Usage Examples

### For<T> with keyed reconciliation

```csharp
// Basic keyed list
new For<Enemy>
{
    Each = enemies,
    Key = e => e.Id,
    Render = (enemy, index) => new EnemyRow
    {
        Enemy = enemy,
        Index = index
    },
    Empty = () => new Label("No enemies remaining")
};

// Simple index-based list (no Key)
new For<string>
{
    Each = chatMessages,
    Render = (msg, idx) => new ChatBubble { Text = msg }
};

// Virtualized list for large datasets
new For<InventoryItem>
{
    Each = inventory.Items,
    Key = item => item.Guid,
    Render = (item, index) => new InventorySlot { Item = item },
    Virtualize = true,
    EstimatedItemHeight = 64f
};
```

### ErrorBoundary

```csharp
// Basic error boundary
new ErrorBoundary
{
    Content = () => new DynamicContent(dataSource),
    Fallback = ex => new Column
    {
        Children =
        {
            new Label($"Something went wrong: {ex.Message}"),
            new Button("Retry") { OnClick = () => /* retry ref */ }
        }
    },
    OnError = ex => Debug.LogException(ex)
};

// Error boundary with retry button wired up
ErrorBoundary boundary = null;
boundary = new ErrorBoundary
{
    Content = () => new PluginPanel(pluginId),
    Fallback = ex => new Column
    {
        Children =
        {
            new Label($"Plugin failed: {ex.Message}"),
            new Button("Retry") { OnClick = () => boundary.RetryRender() }
        }
    },
    OnError = ex => Analytics.TrackError("plugin_crash", ex),
    MaxRetries = 2
};

// Nested error boundaries (inner catches first)
new ErrorBoundary
{
    Content = () => new Column
    {
        Children =
        {
            new SafeWidget(),
            new ErrorBoundary
            {
                Content = () => new RiskyWidget(),
                Fallback = ex => new Label("Widget failed")
            }
        }
    },
    Fallback = ex => new Label("Entire panel failed")
};
```

### Portal

```csharp
// Render a tooltip in the overlay layer
new Portal
{
    Content = () => new Tooltip(tooltipText, position),
    Target = UIRoot.GetLayer(UILayer.Overlay)
};

// Render a modal dialog above everything
new Portal
{
    Content = () => new ConfirmDialog
    {
        Title = "Delete save?",
        OnConfirm = DeleteSave,
        OnCancel = CloseDialog
    },
    TargetLayer = UILayer.Modal
};

// Context menu at cursor position
new Portal
{
    Content = () => new ContextMenu
    {
        Position = cursorPos,
        Items = contextMenuItems
    },
    TargetLayer = UILayer.Popup
};
```

## Test Plan

### For<T>

1. **Keyed add**: Add item to `ReactiveList`, verify new component created and mounted at correct index.
2. **Keyed remove**: Remove item, verify only that component is disposed, others untouched.
3. **Keyed reorder**: Reorder list, verify components are moved (same reference), not recreated.
4. **Keyed replace**: Replace item value, verify `ItemSignal` updates in place, component not recreated.
5. **Index signals update**: After add/remove/reorder, verify all `IndexSignal` values are correct.
6. **Empty shown**: Clear all items, verify `Empty` component created.
7. **Empty hidden**: Add item to empty list, verify `Empty` disposed and item component created.
8. **Without Key reorder**: Reorder causes dispose + recreate (state lost, different instance).
9. **Duplicate keys**: Verify defined behavior (warning logged, graceful degradation).
10. **Virtualization**: Only viewport items mounted; scroll triggers mount/unmount of items.

### ErrorBoundary

11. **Catches Render exception**: `Content` factory throws, verify `Fallback` shown with correct exception.
12. **Catches effect exception**: Effect in content throws, verify boundary catches it and shows `Fallback`.
13. **OnError callback**: Verify `OnError` invoked with caught exception before `Fallback` renders.
14. **RetryRender re-creates content**: Call `RetryRender`, verify `Fallback` disposed, `Content` re-invoked.
15. **MaxRetries exceeded**: After N retries, verify `RetryRender` is a no-op.
16. **AllowRetry = false**: Verify `RetryRender` is always a no-op.
17. **Faulted effects not re-subscribed**: After retry, verify previously faulted effects do not auto-restart.
18. **Content subtree fully disposed on error**: Verify all child components of content are disposed.

### Portal

19. **Renders in target**: Verify content's `VisualElement` is a child of `Target`, not of portal's parent.
20. **Disposes with parent**: Dispose portal's parent, verify content disposed and removed from `Target`.
21. **DI from logical parent**: `[Inject]` in portal content resolves from portal's parent scope.
22. **TargetLayer resolution**: Set `TargetLayer`, verify content attached to layer's root element.
23. **Lifecycle hooks fire**: Verify `OnMount`/`OnUnmount` fire normally on portal content.

## Acceptance Criteria

- [ ] `For<T>` creates one component per list item, keyed by `Key` function
- [ ] Keyed reconciliation produces minimal DOM mutations (add/remove/move, no unnecessary recreations)
- [ ] Reordered items are moved in the DOM, not recreated (same component instance)
- [ ] `ItemSignal` and `IndexSignal` update in place on list mutations
- [ ] `Empty` component shown/hidden based on list emptiness
- [ ] Virtualization only mounts viewport-visible items
- [ ] `ErrorBoundary` catches Render, effect, and handler exceptions
- [ ] `ErrorBoundary` disposes faulted content subtree and shows `Fallback`
- [ ] `RetryRender` re-creates content up to `MaxRetries` times
- [ ] Faulted effects are not re-subscribed on retry
- [ ] `Portal` content is visually attached to `Target` but logically part of parent tree
- [ ] Portal content resolves DI from logical parent scope
- [ ] Portal content is disposed when parent is disposed
- [ ] All tests from the test plan pass
