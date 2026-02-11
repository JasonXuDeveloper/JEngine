# Section 18 — Virtualization Engine

## Overview

The Virtualization Engine renders large collections (10,000+ items) efficiently by maintaining only a small window of active `VisualElement` instances (~17 at any time for a typical viewport). As the user scrolls, elements that leave the viewport are recycled back into a pool and re-bound with new data for elements entering the viewport.

Two components are provided:

- **VirtualList** -- single-column list with variable or fixed item heights.
- **VirtualGrid** -- fixed-cell-size grid with configurable column count.

Both are backed by `VisualElementPool` for type-safe element recycling and integrate directly with `ReactiveList<T>` to respond to collection mutations (add, remove, move, reset) without full re-layout.

## Dependencies

| Dependency | Section | Purpose |
|---|---|---|
| Component | Section 6 | VirtualList and VirtualGrid extend Component |
| ReactiveList | Section 3 | Observable collection driving item changes |

## File Structure

```
Runtime/JUI/
└── Virtualization/
    ├── VisualElementPool.cs
    ├── VirtualList.cs
    └── VirtualGrid.cs
```

## API Design

### VisualElementPool

```csharp
/// <summary>
/// Type-safe pool for VisualElement instances. Rented elements are removed
/// from the visual tree; returned elements are hidden and held for reuse.
/// </summary>
public class VisualElementPool
{
    /// <summary>
    /// Rents an element of type <typeparamref name="T"/> from the pool.
    /// If the pool is empty, a new instance is created via the parameterless constructor.
    /// </summary>
    public T Rent<T>() where T : VisualElement, new();

    /// <summary>
    /// Returns an element to the pool. The element is removed from its parent
    /// and its inline styles are cleared.
    /// </summary>
    public void Return(VisualElement element);

    /// <summary>Current number of pooled (idle) elements across all types.</summary>
    public int PooledCount { get; }

    /// <summary>Current number of rented (active) elements across all types.</summary>
    public int ActiveCount { get; }

    /// <summary>Clears all pooled elements. Active elements are not affected.</summary>
    public void Clear();
}
```

### IItemTemplate

```csharp
/// <summary>
/// Defines how data items are projected into VisualElements.
/// The struct constraint enables JIT devirtualization of interface calls
/// in the hot scroll path.
/// </summary>
/// <typeparam name="TData">The data type for each item.</typeparam>
public interface IItemTemplate<TData>
{
    /// <summary>
    /// Creates a new VisualElement for this template. Called only when the pool
    /// is empty and a new element is needed.
    /// </summary>
    VisualElement Create();

    /// <summary>
    /// Binds data to an element. Called when an element enters the viewport
    /// (either freshly created or recycled from the pool).
    /// </summary>
    /// <param name="el">The element to bind to.</param>
    /// <param name="data">The data item.</param>
    /// <param name="index">The index in the source collection.</param>
    void Bind(VisualElement el, TData data, int index);

    /// <summary>
    /// Unbinds an element before it is returned to the pool. Use this to
    /// unsubscribe event handlers, clear textures, etc.
    /// </summary>
    /// <param name="el">The element being recycled.</param>
    void Unbind(VisualElement el);
}
```

### VirtualList

```csharp
/// <summary>
/// Virtualized single-column list. Renders only the visible portion of a
/// potentially huge data set, recycling elements as the user scrolls.
/// </summary>
/// <typeparam name="TData">The data type for each row.</typeparam>
public class VirtualList<TData> : Component
{
    /// <summary>The reactive data source. Mutations trigger incremental updates.</summary>
    public ReactiveList<TData> Items { get; set; }

    /// <summary>
    /// Estimated height of each item in pixels. Used for scroll thumb sizing
    /// and initial layout. Actual height is measured after bind.
    /// Default: 40.
    /// </summary>
    public float EstimatedItemHeight { get; set; } = 40f;

    /// <summary>
    /// Number of extra items to render above and below the visible window.
    /// Reduces flicker during fast scrolling. Default: 5.
    /// </summary>
    public int OverscanCount { get; set; } = 5;

    /// <summary>
    /// Sets the item template used to create, bind, and unbind elements.
    /// The struct constraint enables devirtualization.
    /// </summary>
    public void SetItemTemplate<T>(T template) where T : struct, IItemTemplate<TData>;

    /// <summary>Scrolls to the item at the given index, optionally animating.</summary>
    public void ScrollToIndex(int index, bool animate = true);

    /// <summary>Scrolls to the first item matching the predicate.</summary>
    public void ScrollTo(Func<TData, bool> predicate, bool animate = true);

    /// <summary>Forces a full re-layout and re-bind of visible items.</summary>
    public void Rebuild();
}
```

### VirtualGrid

```csharp
/// <summary>
/// Virtualized grid layout. Renders items in a fixed-column grid, recycling
/// rows of cells as the user scrolls vertically.
/// </summary>
/// <typeparam name="TData">The data type for each cell.</typeparam>
public class VirtualGrid<TData> : Component
{
    /// <summary>The reactive data source.</summary>
    public ReactiveList<TData> Items { get; set; }

    /// <summary>Number of columns. Default: 4.</summary>
    public int ColumnCount { get; set; } = 4;

    /// <summary>Fixed size of each cell in pixels. Default: (80, 80).</summary>
    public Vector2 CellSize { get; set; } = new(80, 80);

    /// <summary>Spacing between cells in pixels. Default: 4.</summary>
    public float Spacing { get; set; } = 4f;

    /// <summary>
    /// Number of extra rows to render above and below the visible window.
    /// Default: 2.
    /// </summary>
    public int OverscanRows { get; set; } = 2;

    /// <summary>Sets the item template for grid cells.</summary>
    public void SetItemTemplate<T>(T template) where T : struct, IItemTemplate<TData>;

    /// <summary>Scrolls to bring the item at the given index into view.</summary>
    public void ScrollToIndex(int index, bool animate = true);

    /// <summary>Forces a full re-layout and re-bind of visible cells.</summary>
    public void Rebuild();
}
```

## Data Structures

### Visible Window Tracking

```
┌─────────────────────────────────────────────┐
│  Total content height = N * estimatedHeight │
│                                             │
│  ┌─────────────────────────────────────┐    │
│  │  overscan top (OverscanCount items) │    │
│  ├─────────────────────────────────────┤    │
│  │                                     │    │
│  │  visible viewport (~12-17 items)    │    │ ◄── ScrollView.scrollOffset
│  │                                     │    │
│  ├─────────────────────────────────────┤    │
│  │  overscan bottom (OverscanCount)    │    │
│  └─────────────────────────────────────┘    │
│                                             │
└─────────────────────────────────────────────┘
```

- **Anchor index**: The index of the first item whose top edge is at or above the scroll offset.
- **Visible range**: `[anchorIndex - overscan, anchorIndex + visibleCount + overscan]`, clamped to `[0, Items.Count)`.
- **Recycled set**: Items outside the visible range are unbound and returned to the pool.

### VisualElementPool Internal Structure

```
Dictionary<Type, Stack<VisualElement>> _pools;
    key: typeof(T) from Rent<T>()
    value: stack of idle elements

int _activeCount;  // Total rented elements (for diagnostics)
```

### VirtualList Layout State

```csharp
internal struct LayoutState
{
    public int AnchorIndex;        // First visible item index
    public int VisibleCount;       // Number of items in viewport
    public float ScrollOffset;     // Current scroll position (px)
    public float TotalHeight;      // Estimated total content height (px)
    public float ViewportHeight;   // Height of the scroll container (px)
}
```

### Active Element Map

```csharp
// Maps data index → active VisualElement for O(1) lookup during scroll
Dictionary<int, VisualElement> _activeElements;  // Capacity: visibleCount + 2*overscan
```

## Implementation Notes

### Scroll Event Handling

1. `VirtualList` wraps its content in a `ScrollView`. On each `GeometryChangedEvent` or scroll callback, it recomputes the visible range.

2. The new visible range is diffed against the previous range:
   - Indices that left the range: call `template.Unbind(el)`, then `pool.Return(el)`.
   - Indices that entered the range: call `pool.Rent<T>()`, then `template.Bind(el, data, index)`, then position the element via `style.top = index * estimatedHeight`.

3. This diff-based approach means a typical scroll frame recycles 1-2 elements, not the entire viewport.

### Estimated vs Actual Height

- `EstimatedItemHeight` is used to compute total scroll height (`Items.Count * EstimatedItemHeight`) and scroll thumb size. This allows the scroll bar to appear correct even though most items have never been measured.

- After an item is bound and laid out, its actual measured height is cached in a `float[]` array indexed by data index. Subsequent scroll calculations use actual heights for items that have been measured, and estimated heights for those that have not.

- If measured heights deviate significantly from estimates, the total content height is adjusted smoothly to avoid scroll jumps.

### ReactiveList Integration

`VirtualList` subscribes to `ReactiveList<TData>.CollectionChanged` and handles mutations incrementally:

| Mutation | Behavior |
|---|---|
| `Add(item)` | If new index is in visible range, rent and bind. Adjust total height. |
| `Insert(index, item)` | Shift active elements below insertion point. Rent if visible. |
| `RemoveAt(index)` | If index is in visible range, unbind and return. Shift remaining. |
| `Move(from, to)` | Unbind at `from`, rebind at `to` (or recycle if out of range). |
| `Clear()` / `Reset` | Return all active elements, rebuild from scratch. |

### VirtualGrid Row Recycling

`VirtualGrid` virtualizes at the row level, not the cell level. Each "row" is a horizontal container holding `ColumnCount` cells. When a row scrolls out of view, the entire row container (and its cells) is returned to the pool. This simplifies the recycling logic and ensures cells within a row are always laid out together.

Row index for data index `i`: `i / ColumnCount`
Column index for data index `i`: `i % ColumnCount`
Total rows: `Ceil(Items.Count / ColumnCount)`

### Zero-Alloc Scroll Path

The hot path (scroll callback -> diff ranges -> recycle/bind) is allocation-free:

- Range diff uses two `int` comparisons (old start/end vs new start/end).
- `pool.Rent<T>()` reuses existing elements.
- `template.Bind()` is devirtualized because `T` is a struct implementing the interface.
- No LINQ, no lambdas, no boxing in the scroll path.

### Thread Safety

`VirtualList` and `VirtualGrid` are not thread-safe. All mutations to `Items` and scroll interactions must occur on the main thread. `ReactiveList` enforces this with a debug-only thread check.

## Source Generator Notes

No source generation is required for the virtualization system. `IItemTemplate<TData>` is a plain interface implemented by user-defined structs. The struct constraint provides the performance benefit (devirtualization) without generator involvement.

If a future iteration adds a `[VirtualItem]` attribute for declarative template definition, a generator would emit the `IItemTemplate` implementation. This is not in scope for the initial version.

## Usage Examples

### Basic VirtualList

```csharp
public struct PlayerEntry
{
    public string Name;
    public int Score;
    public string AvatarUrl;
}

public struct PlayerItemTemplate : VirtualList<PlayerEntry>.IItemTemplate<PlayerEntry>
{
    public VisualElement Create()
    {
        var row = new VisualElement();
        row.AddToClassList("leaderboard-row");

        var name = new Label { name = "player-name" };
        var score = new Label { name = "player-score" };
        row.Add(name);
        row.Add(score);

        return row;
    }

    public void Bind(VisualElement el, PlayerEntry data, int index)
    {
        el.Q<Label>("player-name").text = data.Name;
        el.Q<Label>("player-score").text = data.Score.ToString("N0");

        // Alternate row styling
        el.EnableInClassList("row-even", index % 2 == 0);
        el.EnableInClassList("row-odd", index % 2 != 0);
    }

    public void Unbind(VisualElement el)
    {
        // No cleanup needed for simple labels
    }
}

public class LeaderboardScreen : Component
{
    private VirtualList<PlayerEntry> _list;

    protected override VisualElement Render()
    {
        var items = new ReactiveList<PlayerEntry>();
        // Populate with 10,000 entries...

        _list = new VirtualList<PlayerEntry>
        {
            Items = items,
            EstimatedItemHeight = 48f,
            OverscanCount = 5
        };
        _list.SetItemTemplate(new PlayerItemTemplate());

        return _list;
    }
}
```

### VirtualGrid — Inventory

```csharp
public struct InventoryCell : VirtualGrid<ItemData>.IItemTemplate<ItemData>
{
    public VisualElement Create()
    {
        var cell = new VisualElement();
        cell.AddToClassList("inventory-cell");

        var icon = new VisualElement { name = "icon" };
        var qty = new Label { name = "quantity" };
        cell.Add(icon);
        cell.Add(qty);

        return cell;
    }

    public void Bind(VisualElement el, ItemData data, int index)
    {
        var icon = el.Q("icon");
        icon.style.backgroundImage = new StyleBackground(data.Icon);

        var qty = el.Q<Label>("quantity");
        qty.text = data.Quantity > 1 ? data.Quantity.ToString() : "";
        qty.visible = data.Quantity > 1;
    }

    public void Unbind(VisualElement el)
    {
        // Release texture reference
        el.Q("icon").style.backgroundImage = StyleKeyword.Null;
    }
}

public class InventoryPanel : Component
{
    protected override VisualElement Render()
    {
        var grid = new VirtualGrid<ItemData>
        {
            Items = inventoryItems,   // ReactiveList<ItemData>
            ColumnCount = 6,
            CellSize = new Vector2(64, 64),
            Spacing = 4f,
            OverscanRows = 2
        };
        grid.SetItemTemplate(new InventoryCell());

        return grid;
    }
}
```

### ScrollToIndex

```csharp
// Jump to a specific player in the leaderboard
_list.ScrollToIndex(playerRank - 1, animate: true);

// Find and scroll to a specific item
_list.ScrollTo(p => p.Name == "TargetPlayer", animate: true);
```

### Dynamic Updates

```csharp
// Adding items triggers incremental update, not full rebuild
items.Add(new PlayerEntry { Name = "NewPlayer", Score = 9001 });

// Removing items recycles the element if it was visible
items.RemoveAt(0);

// Bulk operations: use BeginBatch/EndBatch to defer layout
items.BeginBatch();
for (int i = 0; i < 100; i++)
    items.Add(GenerateEntry());
items.EndBatch();  // Single layout pass
```

## Test Plan

### VisualElementPool Tests

| # | Test | Expectation |
|---|---|---|
| 1 | `Rent<T>()` on empty pool | Returns new instance, `ActiveCount` == 1 |
| 2 | `Return(el)` then `Rent<T>()` | Returns the same instance (reference equality) |
| 3 | `Return(el)` decrements `ActiveCount`, increments `PooledCount` | Counts correct |
| 4 | `Rent<T>()` with multiple types | Separate pools per type |
| 5 | `Clear()` empties all pooled elements | `PooledCount` == 0, `ActiveCount` unchanged |
| 6 | `Return(el)` removes element from parent | `el.parent` is null |

### VirtualList Tests

| # | Test | Expectation |
|---|---|---|
| 7 | Render with 10,000 items | Active element count <= visibleCount + 2*overscan |
| 8 | Scroll down by one item height | Top element recycled, bottom element created |
| 9 | Scroll to bottom then back to top | Elements recycled and re-bound correctly |
| 10 | `Items.Add()` while item is in view | New element appears without full rebuild |
| 11 | `Items.RemoveAt()` for visible item | Element removed and recycled |
| 12 | `Items.Clear()` | All active elements returned to pool |
| 13 | `ScrollToIndex(5000)` | Scroll offset positions item 5000 at top of viewport |
| 14 | `SetItemTemplate` called after items set | Elements re-created with new template |
| 15 | `EstimatedItemHeight` changed | Total content height recalculated |
| 16 | Zero items | Empty list, no errors |
| 17 | One item | Single element rendered, no recycling |

### VirtualGrid Tests

| # | Test | Expectation |
|---|---|---|
| 18 | 100 items with ColumnCount=4 | 25 rows, only visible rows instantiated |
| 19 | Scroll down one row height | Top row recycled, bottom row created |
| 20 | Partial last row (items not divisible by columns) | Last row has correct number of cells |
| 21 | ColumnCount changed dynamically | Grid relays out correctly |
| 22 | CellSize changed dynamically | Cell dimensions update, scroll recalculated |

### Performance Tests

| # | Test | Expectation |
|---|---|---|
| 23 | Scroll 10,000-item list for 60 frames | Zero GC allocations in scroll path |
| 24 | Bind/Unbind 1000 cycles | No element leak (ActiveCount returns to 0) |
| 25 | Rapid scroll (100px/frame for 120 frames) | No visual glitches, element count stable |

## Acceptance Criteria

- [ ] `VisualElementPool` reuses elements across `Rent`/`Return` cycles without leaks
- [ ] `VisualElementPool.Return` removes the element from its parent and clears inline styles
- [ ] `VirtualList` renders 10,000 items with ~17 active elements (viewport-dependent)
- [ ] `VirtualGrid` renders 10,000 items with only visible rows instantiated
- [ ] Scroll path (callback -> diff -> recycle -> bind) produces zero GC allocations
- [ ] `IItemTemplate<TData>` struct constraint enables JIT devirtualization of `Create`/`Bind`/`Unbind`
- [ ] `VirtualList` reacts to `ReactiveList` mutations (Add, Remove, Insert, Move, Clear) incrementally
- [ ] `VirtualGrid` recycles at the row level, not the cell level
- [ ] `EstimatedItemHeight` is used for unmeasured items; actual measured heights are cached and used when available
- [ ] Scroll thumb size and position reflect total content height accurately
- [ ] `ScrollToIndex` and `ScrollTo` navigate to the correct position with optional animation
- [ ] `OverscanCount` / `OverscanRows` render extra items beyond the viewport to reduce flicker
- [ ] `VirtualGrid` handles partial last rows (item count not divisible by column count)
- [ ] `Rebuild()` forces a complete re-layout and re-bind of all visible items
- [ ] Thread safety: debug-only assertion fires if `Items` is mutated off the main thread
- [ ] No source generator is required for the virtualization system in this version
