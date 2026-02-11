# Section 3 — Reactive Collections

## Overview

ReactiveList<T> and ReactiveMap<TKey, TValue> are collections that notify effects when modified. They support item-level change tracking for fine-grained updates -- essential for inventory grids, buff bars, quest logs, and any UI backed by a dynamic data set.

## Dependencies

Section 1 (Signal), Section 2 (Effect/Batch)

## File Structure

- `Runtime/JUI/State/ReactiveList.cs`
- `Runtime/JUI/State/ReactiveMap.cs`

## API Design

```csharp
/// <summary>
/// A reactive list that notifies subscribed effects when items are added, removed,
/// replaced, reordered, or mutated in-place. Backed by a standard List&lt;T&gt; with
/// version tracking for change detection.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public sealed class ReactiveList<T> : IDisposable
{
    /// <summary>Creates an empty reactive list.</summary>
    public ReactiveList();

    /// <summary>Creates a reactive list populated with the given items.</summary>
    /// <param name="initial">The initial items to populate the list with.</param>
    public ReactiveList(IEnumerable<T> initial);

    /// <summary>
    /// Reactive read -- returns a read-only view of the list.
    /// Effects that read this property are automatically tracked as subscribers.
    /// </summary>
    public ReadOnlyCollection<T> Value { get; }

    /// <summary>
    /// Zero-copy hot-path iteration. Returns a span over the internal array
    /// without allocating a new collection. Reactive -- tracks the reading effect.
    /// </summary>
    /// <returns>A read-only span over the internal list storage.</returns>
    public ReadOnlySpan<T> AsSpan();

    /// <summary>
    /// Reactive indexer. Returns the item at the given index.
    /// Tracks the reading effect as a subscriber.
    /// </summary>
    /// <param name="index">The zero-based index of the item to retrieve.</param>
    public T this[int index] { get; }

    /// <summary>
    /// Reactive count. Returns the number of items in the list.
    /// Tracks the reading effect as a subscriber.
    /// </summary>
    public int Count { get; }

    // ─── Mutators ────────────────────────────────────────────────
    // All mutators bump the internal version counter and call
    // Batch.NotifyChanged() to schedule subscriber effects.

    /// <summary>Add an item to the end of the list.</summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item);

    /// <summary>Insert an item at the specified index.</summary>
    /// <param name="index">The zero-based index at which to insert.</param>
    /// <param name="item">The item to insert.</param>
    public void Insert(int index, T item);

    /// <summary>Remove the first occurrence of an item.</summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was found and removed; false otherwise.</returns>
    public bool Remove(T item);

    /// <summary>Remove the item at the specified index.</summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    public void RemoveAt(int index);

    /// <summary>Remove all items from the list.</summary>
    public void Clear();

    /// <summary>Sort the list in-place using the given comparison.</summary>
    /// <param name="comparison">The comparison delegate for sorting.</param>
    public void Sort(Comparison<T> comparison);

    /// <summary>Move an item from one index to another.</summary>
    /// <param name="from">The current index of the item.</param>
    /// <param name="to">The target index to move the item to.</param>
    public void Move(int from, int to);

    /// <summary>Replace the entire contents of the list with the given items.</summary>
    /// <param name="items">The new items to populate the list with.</param>
    public void ReplaceAll(IEnumerable<T> items);

    /// <summary>Replace the entire contents of the list from a span (zero-copy source).</summary>
    /// <param name="items">The span of new items to populate the list with.</param>
    public void ReplaceAll(ReadOnlySpan<T> items);

    // ─── Item-level change tracking ─────────────────────────────

    /// <summary>
    /// Notify subscribers that the item at the given index was mutated in-place.
    /// Use this after directly modifying a reference-type item's properties.
    /// </summary>
    /// <param name="index">The index of the mutated item.</param>
    public void NotifyItemChanged(int index);

    /// <summary>
    /// Notify subscribers that one or more items were mutated in-place.
    /// Use when multiple items changed and individual notifications are unnecessary.
    /// </summary>
    public void NotifyChanged();

    /// <summary>
    /// Replace the item at the given index with a new item.
    /// Bumps version and notifies subscribers.
    /// </summary>
    /// <param name="index">The zero-based index of the item to replace.</param>
    /// <param name="newItem">The replacement item.</param>
    public void SetItem(int index, T newItem);

    /// <summary>
    /// Update an item at the given index via a mutator callback.
    /// The item is passed to the callback for in-place modification,
    /// then subscribers are notified.
    /// </summary>
    /// <param name="index">The zero-based index of the item to update.</param>
    /// <param name="mutator">The action that mutates the item.</param>
    public void UpdateItem(int index, Action<T> mutator);

    /// <summary>
    /// Update an item at the given index via a typed-state mutator callback.
    /// Avoids closure allocation by passing state as a separate parameter.
    /// </summary>
    /// <typeparam name="TState">The type of the state parameter.</typeparam>
    /// <param name="index">The zero-based index of the item to update.</param>
    /// <param name="mutator">The action that mutates the item, receiving the item and state.</param>
    /// <param name="state">The state passed to the mutator.</param>
    public void UpdateItem<TState>(int index, Action<T, TState> mutator, TState state);

    /// <summary>
    /// Dispose the reactive list. Clears all subscribers, removes all items,
    /// and releases internal resources.
    /// </summary>
    public void Dispose();
}

/// <summary>
/// A reactive dictionary that notifies subscribed effects when entries are added,
/// removed, or modified. Backed by a standard Dictionary&lt;TKey, TValue&gt; with
/// version tracking for change detection.
/// </summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
public sealed class ReactiveMap<TKey, TValue> : IDisposable
{
    /// <summary>
    /// Reactive indexer. Gets or sets the value associated with the given key.
    /// Get: tracks the reading effect as a subscriber.
    /// Set: bumps version and notifies subscribers.
    /// </summary>
    /// <param name="key">The key to look up or assign.</param>
    public TValue this[TKey key] { get; set; }

    /// <summary>
    /// Reactive count. Returns the number of entries in the map.
    /// Tracks the reading effect as a subscriber.
    /// </summary>
    public int Count { get; }

    /// <summary>Add a new key-value pair. Bumps version and notifies subscribers.</summary>
    /// <param name="key">The key to add.</param>
    /// <param name="value">The value to associate with the key.</param>
    public void Add(TKey key, TValue value);

    /// <summary>Remove the entry with the given key.</summary>
    /// <param name="key">The key to remove.</param>
    /// <returns>True if the key was found and removed; false otherwise.</returns>
    public bool Remove(TKey key);

    /// <summary>
    /// Check whether the map contains the given key.
    /// Reactive -- tracks the reading effect as a subscriber.
    /// </summary>
    /// <param name="key">The key to check for.</param>
    /// <returns>True if the key exists; false otherwise.</returns>
    public bool ContainsKey(TKey key);

    /// <summary>
    /// Try to get the value associated with the given key.
    /// Reactive -- tracks the reading effect as a subscriber.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="value">The value if found; default otherwise.</param>
    /// <returns>True if the key was found; false otherwise.</returns>
    public bool TryGetValue(TKey key, out TValue value);

    /// <summary>
    /// Notify subscribers that the value for the given key was mutated in-place.
    /// Use after directly modifying a reference-type value's properties.
    /// </summary>
    /// <param name="key">The key whose value was mutated.</param>
    public void NotifyItemChanged(TKey key);

    /// <summary>
    /// Dispose the reactive map. Clears all subscribers, removes all entries,
    /// and releases internal resources.
    /// </summary>
    public void Dispose();
}
```

## Data Structures

- Internal `List<T> _items` in ReactiveList -- the backing store for all elements.
- Internal `ReadOnlyCollection<T> _readOnly` in ReactiveList -- cached read-only wrapper created once in the constructor via `_items.AsReadOnly()`.
- Internal `int _version` in both ReactiveList and ReactiveMap -- monotonically increasing counter bumped on every mutation.
- Internal `List<IEffect> _subscribers` in both collections -- effects subscribed to change notifications.
- Internal `Dictionary<TKey, TValue> _entries` in ReactiveMap -- the backing store for all key-value pairs.

## Implementation Notes

- **Backed by standard collections**: ReactiveList wraps `List<T>`, ReactiveMap wraps `Dictionary<TKey, TValue>`. No custom data structures -- standard BCL performance characteristics apply.
- **AsSpan()**: Uses `System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_items)` for zero-copy iteration over the internal array. This avoids allocating an enumerator or copying to an array.
- **Version bumps on every mutation**: Every mutator method (Add, Remove, Insert, Clear, Sort, Move, ReplaceAll, SetItem, NotifyItemChanged, NotifyChanged) increments `_version` and calls `Batch.NotifyChanged(this)` to schedule subscriber effects.
- **ReadOnlyCollection caching**: The `Value` property returns a pre-cached `ReadOnlyCollection<T>` created once in the constructor. This avoids allocating a new wrapper on every read.
- **Auto-tracking on reads**: `Value`, `AsSpan()`, `this[index]`, `Count`, `ContainsKey()`, and `TryGetValue()` all check `EffectTracker.Current` and subscribe the running effect if present -- identical to how Signal auto-tracking works.
- **UpdateItem<TState>**: Enables zero-closure item mutation by accepting a static lambda and typed state. The item at the given index is passed as the first argument to the mutator, the state as the second.
- **ReplaceAll(ReadOnlySpan<T>)**: Clears the internal list and copies from the span. Useful for bulk updates from native buffers or pooled arrays without intermediate IEnumerable allocation.
- **Move(from, to)**: Removes the item at `from` and inserts it at `to`. Single version bump. Used by drag-and-drop reordering in list UIs.
- **Dispose**: Clears the subscriber list, clears internal storage, and sets a disposed flag to prevent further use.
- **Thread safety**: Reactive collections are main-thread only. No locking is needed.

## Source Generator Notes

N/A for this section -- reactive collections are runtime primitives, not generated.

## Usage Examples

```csharp
// Inventory list
var inventory = new ReactiveList<Item>();

// Effect that rebuilds inventory UI when items change
var inventoryGrid = root.Q<VisualElement>("inventory-grid");
var fx = EffectPool<(ReactiveList<Item> items, VisualElement grid)>.Rent(
    static (state) =>
    {
        state.grid.Clear();
        var span = state.items.AsSpan();  // zero-copy iteration
        for (int i = 0; i < span.Length; i++)
            state.grid.Add(new ItemSlot(span[i]));
    },
    (items: inventory, grid: inventoryGrid));
fx.Run();

// Mutators trigger the effect on next frame
inventory.Add(new Item("Sword", 1));
inventory.Add(new Item("Shield", 1));
// Effect runs once on next frame with both items

// In-place mutation with notification
inventory.UpdateItem(0, static (item, newCount) => item.Count = newCount, 5);
// Effect re-runs, showing Sword x5

// Zero-closure item mutation with typed state
var damageAmount = 10;
inventory.UpdateItem<int>(0, static (item, dmg) =>
{
    item.Durability -= dmg;
}, damageAmount);

// Bulk replace from span (e.g., server sync)
Span<Item> serverItems = stackalloc Item[3];
// ... populate serverItems ...
inventory.ReplaceAll(serverItems);

// Drag-and-drop reorder
inventory.Move(from: 2, to: 0);

// ReactiveMap for key-value data
var questLog = new ReactiveMap<string, QuestState>();

questLog.Add("main_quest", QuestState.Active);
questLog["side_quest"] = QuestState.Active;

// Reactive read in effect
var fx2 = EffectPool<(ReactiveMap<string, QuestState> quests, Label label)>.Rent(
    static (state) =>
    {
        if (state.quests.TryGetValue("main_quest", out var quest))
            state.label.text = $"Main Quest: {quest}";
    },
    (quests: questLog, label: root.Q<Label>("quest-status")));
fx2.Run();

// In-place mutation notification
questLog["main_quest"] = QuestState.Completed;
// Effect re-runs on next frame

// Cleanup
inventory.Dispose();
questLog.Dispose();
```

## Test Plan

1. **All mutators trigger subscriber effects** -- Add, Remove, Insert, Clear, Sort, Move, ReplaceAll each trigger subscribed effects on the next flush.
2. **NotifyItemChanged triggers effects** -- mutate a reference-type item in-place, call NotifyItemChanged, verify subscriber effects run.
3. **AsSpan zero-copy** -- verify AsSpan returns a span over the internal array (same memory, no copy).
4. **UpdateItem with static lambda avoids closure** -- verify UpdateItem<TState> works correctly and does not allocate.
5. **SetItem replaces and notifies** -- call SetItem at an index, verify the item is replaced and subscribers are notified.
6. **Count/indexer/Value all trigger auto-tracking** -- read each inside an effect, change the list, verify the effect is marked dirty.
7. **ReactiveMap Add/Remove/indexer all reactive** -- each operation triggers subscriber effects.
8. **ReactiveMap NotifyItemChanged** -- mutate a value in-place, call NotifyItemChanged, verify subscriber effects run.
9. **Dispose clears subscribers** -- dispose a collection, verify no further notifications are sent and subscriber list is empty.
10. **ReplaceAll(ReadOnlySpan<T>)** -- bulk replace from a span, verify all items replaced and effect runs once.
11. **Move reorders correctly** -- move an item, verify the list order and that effect runs once (not twice for remove+insert).
12. **Multiple mutations in one frame coalesce** -- add three items in one frame, verify the effect runs once with all three items.

## Acceptance Criteria

- [ ] `ReactiveList<T>` implements `IDisposable`
- [ ] `ReactiveList<T>` provides reactive `Value`, `AsSpan()`, indexer, and `Count`
- [ ] All ReactiveList mutators (Add, Insert, Remove, RemoveAt, Clear, Sort, Move, ReplaceAll) bump version and notify
- [ ] `AsSpan()` returns zero-copy span via `CollectionsMarshal.AsSpan()` or equivalent
- [ ] `ReadOnlyCollection<T>` is cached (not re-allocated on each `Value` read)
- [ ] `UpdateItem<TState>` avoids closure allocation with typed state parameter
- [ ] `NotifyItemChanged(int)` and `NotifyChanged()` support in-place mutation tracking
- [ ] `ReplaceAll(ReadOnlySpan<T>)` supports bulk replacement from spans
- [ ] `ReactiveMap<TKey, TValue>` implements `IDisposable`
- [ ] ReactiveMap indexer, Count, ContainsKey, and TryGetValue are all reactive (auto-tracking)
- [ ] ReactiveMap Add, Remove, indexer set, and NotifyItemChanged all notify subscribers
- [ ] Dispose clears subscribers and internal storage on both collection types
- [ ] All public APIs have XML documentation
- [ ] Zero allocations on reactive reads in hot path (after initial setup)
