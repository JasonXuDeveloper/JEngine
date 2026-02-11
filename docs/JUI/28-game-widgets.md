# Section 28 — Game-Specific Widgets

## Overview

Purpose-built game UI widgets that compose JUI primitives (signals, effects, animations, pooling, virtualization) into ready-to-use components for common game UI patterns. Each widget is a full `Component` with reactive signal bindings, built-in animations, and pooling strategies for performance-critical scenarios like damage numbers and buff icons.

These widgets are opinionated implementations of patterns that appear in the majority of games. They are designed to be usable out-of-the-box with sensible defaults while remaining fully customizable through signals, CSS theming, and component composition.

## Dependencies

- Section 27 (Base Widgets & Layout) -- game widgets compose base controls (Button, Label, ProgressBar, etc.).
- Section 18 (Virtualization) -- `InventoryGrid` and `BuffBar` use virtualized grid rendering.
- Section 15 (Animation) -- all game widgets use the animation system for transitions, tweens, and effects.
- Section 8 (For & ErrorBoundary & Portal) -- `For<T>` used for dynamic lists (buffs, quests, action bar slots).
- Section 1 (Signal & Computed) -- all widget state is signal-driven.
- Section 2 (Effect System) -- effects drive animations and state synchronization.
- Section 13 (Event System) -- widgets publish game events (damage dealt, cooldown ready, dialogue complete).

## File Structure

- `Runtime/JUI/GameWidgets/HealthBar.cs`
- `Runtime/JUI/GameWidgets/CooldownOverlay.cs`
- `Runtime/JUI/GameWidgets/InventoryGrid.cs`
- `Runtime/JUI/GameWidgets/ResourceCounter.cs`
- `Runtime/JUI/GameWidgets/ActionBar.cs`
- `Runtime/JUI/GameWidgets/BuffBar.cs`
- `Runtime/JUI/GameWidgets/DialogueBox.cs`
- `Runtime/JUI/GameWidgets/DamageNumber.cs`
- `Runtime/JUI/GameWidgets/QuestTracker.cs`
- `Runtime/JUI/GameWidgets/Minimap.cs`
- `Runtime/JUI/GameWidgets/LoadingScreen.cs`

## API Design

### HealthBar

```csharp
/// <summary>
/// A game health bar with animated drain (ghost bar), shield overlay, damage flash,
/// and color threshold transitions. Binds to current/max health signals for automatic updates.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-health</c>, <c>jui-health__fill</c>, <c>jui-health__ghost</c>,
/// <c>jui-health__shield</c>, <c>jui-health__label</c>, <c>jui-health--critical</c>,
/// <c>jui-health--damage-flash</c>.
///
/// Visual composition:
/// - Background track (dark)
/// - Ghost bar (lighter, shows where health was before damage, drains slowly)
/// - Fill bar (main health color, updates immediately)
/// - Shield overlay (blue translucent bar over the fill, shows shield/armor)
/// - Label overlay (optional "80/100" text)
/// </remarks>
public sealed class HealthBar : Component
{
    /// <summary>Create a health bar.</summary>
    /// <param name="showLabel">If true, displays "current/max" text over the bar.</param>
    public HealthBar(bool showLabel = true);

    /// <summary>Current health value. Drives the fill bar width.</summary>
    public Signal<float> Current { get; }

    /// <summary>Maximum health value. Drives the fill bar proportion.</summary>
    public Signal<float> Max { get; }

    /// <summary>
    /// Current shield/armor value. Renders as a translucent blue overlay on top of the
    /// health fill. 0 = no shield displayed.
    /// </summary>
    public Signal<float> Shield { get; }

    /// <summary>
    /// Health percentage threshold below which the bar turns to the critical color
    /// (red) and optionally pulses. Default: 0.25 (25%).
    /// </summary>
    public float CriticalThreshold { get; set; }

    /// <summary>
    /// Duration of the ghost bar drain animation. The ghost bar lingers at the previous
    /// health level and slowly drains to match the current fill. Default: 0.8 seconds.
    /// </summary>
    public float GhostDrainDuration { get; set; }

    /// <summary>
    /// Duration of the damage flash effect (brief white/red flash on the fill bar).
    /// Default: 0.15 seconds.
    /// </summary>
    public float DamageFlashDuration { get; set; }

    /// <summary>
    /// Bind to external current and max signals for reactive updates.
    /// </summary>
    /// <param name="current">Signal for current health.</param>
    /// <param name="max">Signal for maximum health.</param>
    /// <returns>This HealthBar for fluent chaining.</returns>
    public HealthBar Bind(IReadOnlySignal<float> current, IReadOnlySignal<float> max);
}
```

### CooldownOverlay

```csharp
/// <summary>
/// A radial sweep overlay for ability cooldowns. Shows a darkened sweep that reveals
/// the icon as the cooldown progresses, a countdown timer text, and a ready-flash
/// animation when the cooldown completes.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-cooldown</c>, <c>jui-cooldown__sweep</c>,
/// <c>jui-cooldown__timer</c>, <c>jui-cooldown--ready</c>.
///
/// Visual composition:
/// - Underlying ability icon (passed as child or bound texture)
/// - Dark semi-transparent sweep overlay (radial, driven by shader)
/// - Timer text centered on the overlay (e.g., "3.2")
/// - Ready flash: brief white glow animation when cooldown reaches 0
/// </remarks>
public sealed class CooldownOverlay : Component
{
    /// <summary>Create a cooldown overlay.</summary>
    public CooldownOverlay();

    /// <summary>
    /// Current remaining cooldown time in seconds. Drives the radial sweep angle.
    /// 0 = ready (no overlay).
    /// </summary>
    public Signal<float> Remaining { get; }

    /// <summary>
    /// Total cooldown duration in seconds. Used to calculate the sweep proportion
    /// (Remaining / Duration).
    /// </summary>
    public Signal<float> Duration { get; }

    /// <summary>Whether to show the countdown timer text. Default: true.</summary>
    public bool ShowTimer { get; set; }

    /// <summary>Whether to play the ready-flash animation when cooldown completes. Default: true.</summary>
    public bool ReadyFlash { get; set; }

    /// <summary>
    /// Bind to external remaining and duration signals.
    /// </summary>
    public CooldownOverlay Bind(IReadOnlySignal<float> remaining, IReadOnlySignal<float> duration);
}
```

### InventoryGrid

```csharp
/// <summary>
/// A grid-based inventory display composing VirtualGrid, DragDrop, Tooltip, and
/// ContextMenu. Supports configurable cell size and column count, item stacking,
/// rarity borders, and empty slot placeholders.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-inventory</c>, <c>jui-inventory__cell</c>,
/// <c>jui-inventory__cell--empty</c>, <c>jui-inventory__cell--selected</c>,
/// <c>jui-inventory__icon</c>, <c>jui-inventory__stack-count</c>,
/// <c>jui-inventory__rarity--common</c>, <c>jui-inventory__rarity--rare</c>,
/// <c>jui-inventory__rarity--epic</c>, <c>jui-inventory__rarity--legendary</c>.
///
/// Visual composition per cell:
/// - Empty slot background
/// - Item icon (texture)
/// - Stack count badge (bottom-right corner)
/// - Rarity border (colored glow)
/// - Tooltip on hover/longpress showing item details
/// - Context menu on right-click showing Use/Drop/Split actions
/// </remarks>
/// <typeparam name="T">The inventory item data type.</typeparam>
public sealed class InventoryGrid<T> : Component where T : class
{
    /// <summary>Create an inventory grid.</summary>
    /// <param name="items">
    /// Reactive list of inventory slots. Null entries represent empty slots.
    /// </param>
    /// <param name="columns">Number of columns in the grid.</param>
    /// <param name="cellSize">Size of each cell in pixels.</param>
    public InventoryGrid(ReactiveList<T> items, int columns = 8, float cellSize = 64f);

    /// <summary>The currently selected slot index. -1 = no selection.</summary>
    public Signal<int> SelectedIndex { get; }

    /// <summary>Function to extract the icon texture from an item.</summary>
    public Func<T, Texture2D> IconSelector { get; set; }

    /// <summary>Function to extract the stack count from an item. Default: returns 1.</summary>
    public Func<T, int> StackCountSelector { get; set; }

    /// <summary>Function to extract the rarity tier from an item for border coloring.</summary>
    public Func<T, ItemRarity> RaritySelector { get; set; }

    /// <summary>Function to build tooltip content for an item.</summary>
    public Func<T, Element> TooltipBuilder { get; set; }

    /// <summary>Function to build context menu for an item.</summary>
    public Action<T, int, ContextMenu> ContextMenuBuilder { get; set; }

    /// <summary>Whether drag-drop reordering is enabled. Default: true.</summary>
    public bool AllowDragDrop { get; set; }

    /// <summary>
    /// Callback invoked when an item is moved via drag-drop.
    /// Parameters: (fromIndex, toIndex).
    /// </summary>
    public Action<int, int> OnItemMoved { get; set; }
}

/// <summary>Rarity tiers for inventory item border coloring.</summary>
public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }
```

### ResourceCounter

```csharp
/// <summary>
/// A compact resource display with icon, count label, and animated delta indicator.
/// When the count changes, a "+5" or "-3" label floats up from the counter and fades out.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-resource</c>, <c>jui-resource__icon</c>,
/// <c>jui-resource__count</c>, <c>jui-resource__delta</c>,
/// <c>jui-resource__delta--positive</c>, <c>jui-resource__delta--negative</c>.
///
/// Visual composition:
/// - Icon (left, fixed size)
/// - Count label (right of icon, formatted number)
/// - Delta label (spawned above count on change, floats up 30px and fades over 1s)
///
/// Animation: Delta labels are pooled. On count change, a delta label is rented from the
/// pool, positioned at the count label's location, animated upward with opacity fade, then
/// returned to the pool.
/// </remarks>
public sealed class ResourceCounter : Component
{
    /// <summary>Create a resource counter with the given icon.</summary>
    /// <param name="icon">The resource icon texture.</param>
    public ResourceCounter(Texture2D icon);

    /// <summary>The current resource count. Animated deltas are shown on change.</summary>
    public Signal<int> Count { get; }

    /// <summary>The resource icon.</summary>
    public Signal<Texture2D> Icon { get; }

    /// <summary>
    /// Number format string for the count display. Default: "N0" (e.g., "1,234").
    /// </summary>
    public string Format { get; set; }

    /// <summary>
    /// Duration of the delta float-up animation in seconds. Default: 1.0.
    /// </summary>
    public float DeltaAnimationDuration { get; set; }

    /// <summary>Color for positive deltas. Default: green.</summary>
    public Color PositiveColor { get; set; }

    /// <summary>Color for negative deltas. Default: red.</summary>
    public Color NegativeColor { get; set; }

    /// <summary>Bind the count to an external signal.</summary>
    public ResourceCounter Bind(IReadOnlySignal<int> count);
}
```

### ActionBar

```csharp
/// <summary>
/// A horizontal bar of hotkey-bound ability slots, each with a CooldownOverlay,
/// drag-reorder support, and keybind labels. Typical layout: 1-9 number keys plus
/// additional modifier slots.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-action-bar</c>, <c>jui-action-bar__slot</c>,
/// <c>jui-action-bar__slot--active</c>, <c>jui-action-bar__keybind</c>,
/// <c>jui-action-bar__icon</c>.
///
/// Visual composition per slot:
/// - Background frame
/// - Ability icon texture
/// - CooldownOverlay (when on cooldown)
/// - Keybind label (top-left corner, e.g., "1", "Q", "Shift+1")
/// - Active highlight (border glow when the ability is toggled on)
///
/// Slots are rendered via For<SlotData> and support drag-drop reordering.
/// </remarks>
/// <typeparam name="T">The ability/action data type.</typeparam>
public sealed class ActionBar<T> : Component
{
    /// <summary>Create an action bar.</summary>
    /// <param name="slots">Reactive list of slot data. Null entries are empty slots.</param>
    /// <param name="slotCount">Fixed number of slots. Default: 10.</param>
    public ActionBar(ReactiveList<T> slots, int slotCount = 10);

    /// <summary>Function to extract the ability icon from slot data.</summary>
    public Func<T, Texture2D> IconSelector { get; set; }

    /// <summary>Function to extract cooldown remaining time from slot data.</summary>
    public Func<T, IReadOnlySignal<float>> CooldownRemainingSelector { get; set; }

    /// <summary>Function to extract cooldown duration from slot data.</summary>
    public Func<T, IReadOnlySignal<float>> CooldownDurationSelector { get; set; }

    /// <summary>
    /// Keybind labels for each slot. Array length must match slotCount.
    /// Example: ["1", "2", "3", ..., "0"].
    /// </summary>
    public string[] KeybindLabels { get; set; }

    /// <summary>Whether drag-drop reordering of slots is enabled. Default: true.</summary>
    public bool AllowDragDrop { get; set; }

    /// <summary>Callback invoked when a slot is clicked or its keybind is pressed.</summary>
    public Action<T, int> OnSlotActivated { get; set; }

    /// <summary>Callback invoked when a slot is moved via drag-drop.</summary>
    public Action<int, int> OnSlotMoved { get; set; }
}
```

### BuffBar

```csharp
/// <summary>
/// A grid of active buff/debuff icons with timer rings, stack count badges, and
/// tooltip on hover/longpress. Uses For<T> for reactive list rendering and pooling
/// for efficient add/remove.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-buffs</c>, <c>jui-buffs__icon</c>, <c>jui-buffs__timer-ring</c>,
/// <c>jui-buffs__stack</c>, <c>jui-buffs--debuff</c>.
///
/// Visual composition per buff:
/// - Buff/debuff icon texture (32x32 default)
/// - Timer ring: radial progress ring around the icon showing remaining duration
/// - Stack count badge (top-right corner, shown when stacks > 1)
/// - Border color: blue for buffs, red for debuffs
/// - Tooltip on hover showing name, description, remaining time, stacks
/// </remarks>
/// <typeparam name="T">The buff data type.</typeparam>
public sealed class BuffBar<T> : Component
{
    /// <summary>Create a buff bar.</summary>
    /// <param name="buffs">Reactive list of active buffs.</param>
    public BuffBar(ReactiveList<T> buffs);

    /// <summary>Function to extract the icon texture from buff data.</summary>
    public Func<T, Texture2D> IconSelector { get; set; }

    /// <summary>Function to extract remaining duration signal from buff data.</summary>
    public Func<T, IReadOnlySignal<float>> RemainingSelector { get; set; }

    /// <summary>Function to extract total duration from buff data.</summary>
    public Func<T, float> DurationSelector { get; set; }

    /// <summary>Function to extract stack count signal from buff data.</summary>
    public Func<T, IReadOnlySignal<int>> StackCountSelector { get; set; }

    /// <summary>Function to determine if the buff is a debuff (red border).</summary>
    public Func<T, bool> IsDebuffSelector { get; set; }

    /// <summary>Function to build tooltip content for a buff.</summary>
    public Func<T, Element> TooltipBuilder { get; set; }

    /// <summary>Size of each buff icon in pixels. Default: 32.</summary>
    public float IconSize { get; set; }

    /// <summary>Gap between buff icons in pixels. Default: 4.</summary>
    public float Gap { get; set; }
}
```

### DialogueBox

```csharp
/// <summary>
/// A dialogue display with typewriter text effect (character-by-character reveal),
/// speaker portrait, choice buttons, skip button, and auto-advance timer. Designed
/// for RPG/visual novel dialogue systems.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-dialogue</c>, <c>jui-dialogue__portrait</c>,
/// <c>jui-dialogue__name</c>, <c>jui-dialogue__text</c>, <c>jui-dialogue__choices</c>,
/// <c>jui-dialogue__choice</c>, <c>jui-dialogue__skip</c>,
/// <c>jui-dialogue__advance-indicator</c>.
///
/// Visual composition:
/// - Portrait image (left side, optional)
/// - Speaker name label (above text)
/// - Text area with typewriter reveal
/// - Advance indicator (blinking arrow/triangle when text is fully revealed)
/// - Choice buttons (shown after text completes, if choices are provided)
/// - Skip button (top-right corner, skips typewriter to show full text)
/// </remarks>
public sealed class DialogueBox : Component
{
    /// <summary>Create a dialogue box.</summary>
    public DialogueBox();

    /// <summary>The full text to display. Typewriter reveal starts when this changes.</summary>
    public Signal<string> Text { get; }

    /// <summary>The speaker's name displayed above the text.</summary>
    public Signal<string> SpeakerName { get; }

    /// <summary>The speaker's portrait image. Null = no portrait.</summary>
    public Signal<Texture2D> Portrait { get; }

    /// <summary>
    /// Available dialogue choices. Shown as buttons after the typewriter completes.
    /// Empty list = no choices (advance to next dialogue on click/key).
    /// </summary>
    public ReactiveList<DialogueChoice> Choices { get; }

    /// <summary>
    /// Typewriter speed in characters per second. Default: 40.
    /// Set to 0 for instant reveal.
    /// </summary>
    public float TypewriterSpeed { get; set; }

    /// <summary>
    /// Whether the typewriter is currently revealing text. Becomes false when all
    /// text is visible.
    /// </summary>
    public IReadOnlySignal<bool> IsTyping { get; }

    /// <summary>
    /// Auto-advance delay in seconds after text is fully revealed. 0 = manual advance only.
    /// </summary>
    public float AutoAdvanceDelay { get; set; }

    /// <summary>
    /// Skip the typewriter effect and show the full text immediately.
    /// If text is already fully shown, advances to the next dialogue.
    /// </summary>
    public void Skip();

    /// <summary>
    /// Display a dialogue line and wait for the player to advance or choose.
    /// Returns the chosen option index, or -1 if no choices were presented.
    /// </summary>
    /// <param name="speakerName">The speaker's name.</param>
    /// <param name="text">The dialogue text.</param>
    /// <param name="portrait">Optional portrait texture.</param>
    /// <param name="choices">Optional dialogue choices.</param>
    /// <returns>UniTask completing with the choice index when the player advances.</returns>
    public UniTask<int> ShowLineAsync(
        string speakerName, string text,
        Texture2D portrait = null,
        IReadOnlyList<DialogueChoice> choices = null);
}

/// <summary>A single dialogue choice option.</summary>
public readonly struct DialogueChoice
{
    /// <summary>Display text for the choice button.</summary>
    public string Text { get; init; }

    /// <summary>Whether this choice is enabled (greyed out if false).</summary>
    public bool Enabled { get; init; }

    /// <summary>Optional condition text shown in parentheses (e.g., "[Charm 15]").</summary>
    public string Condition { get; init; }
}
```

### DamageNumber

```csharp
/// <summary>
/// A floating damage number that spawns at a position, floats upward, and fades out.
/// Pooled via the JUI object pooling system for zero-allocation spawning in combat.
/// Supports critical hit scaling, damage type coloring, and random horizontal scatter.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-dmg-number</c>, <c>jui-dmg-number--crit</c>,
/// <c>jui-dmg-number--heal</c>, <c>jui-dmg-number--{type}</c>.
///
/// Visual composition:
/// - Text label showing the damage amount
/// - Positioned absolutely at spawn point
/// - Animation: float up 80px over 1s with ease-out, fade opacity 1 -> 0 over last 0.3s
/// - Critical hits: 2x font size, bold, slight bounce at start
/// - Color by damage type: physical=white, fire=orange, ice=cyan, poison=green, heal=green
///
/// Pooling strategy:
/// - DamageNumber instances are pooled with a max pool size of 50.
/// - On spawn, a pooled instance is rented, configured with amount/position/type, and
///   added to the overlay layer.
/// - On animation complete, the instance is removed from the visual tree and returned
///   to the pool.
/// - The pool pre-warms 10 instances on first use.
/// </remarks>
public sealed class DamageNumber : Component
{
    /// <summary>Create a damage number (typically called by the pool, not directly).</summary>
    public DamageNumber();

    /// <summary>The damage amount to display.</summary>
    public Signal<int> Amount { get; }

    /// <summary>Whether this is a critical hit (2x size, bounce animation).</summary>
    public Signal<bool> IsCritical { get; }

    /// <summary>The damage type, controlling the text color.</summary>
    public Signal<DamageType> Type { get; }

    /// <summary>
    /// Configure and show a damage number at the given screen position.
    /// Called after renting from the pool.
    /// </summary>
    /// <param name="amount">The damage amount.</param>
    /// <param name="screenPosition">Screen-space position to spawn at.</param>
    /// <param name="type">Damage type for coloring.</param>
    /// <param name="isCritical">Whether this is a critical hit.</param>
    public void Show(int amount, Vector2 screenPosition, DamageType type = DamageType.Physical,
                     bool isCritical = false);

    /// <summary>
    /// Float-up distance in pixels. Default: 80.
    /// </summary>
    public float FloatDistance { get; set; }

    /// <summary>
    /// Duration of the float-up animation in seconds. Default: 1.0.
    /// </summary>
    public float AnimationDuration { get; set; }

    /// <summary>
    /// Maximum random horizontal scatter in pixels. Each spawn offsets randomly
    /// within [-scatter, +scatter] to prevent overlapping numbers. Default: 20.
    /// </summary>
    public float HorizontalScatter { get; set; }

    /// <summary>
    /// Static factory method that rents from the pool, configures, and shows.
    /// </summary>
    public static DamageNumber Spawn(int amount, Vector2 screenPosition,
                                      DamageType type = DamageType.Physical,
                                      bool isCritical = false);
}

/// <summary>Damage type for coloring damage numbers.</summary>
public enum DamageType { Physical, Fire, Ice, Lightning, Poison, Holy, Shadow, Heal }
```

### QuestTracker

```csharp
/// <summary>
/// A quest tracking panel showing active quests with their objectives, progress bars,
/// and checkmark animations on objective completion. Uses For<T> for reactive quest list
/// rendering.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-quest-tracker</c>, <c>jui-quest__entry</c>,
/// <c>jui-quest__title</c>, <c>jui-quest__objective</c>,
/// <c>jui-quest__objective--completed</c>, <c>jui-quest__progress</c>,
/// <c>jui-quest__checkmark</c>.
///
/// Visual composition per quest:
/// - Quest title (bold, with optional quest type icon)
/// - List of objectives, each with:
///   - Checkbox/checkmark (animated on completion: scale 0->1 with bounce)
///   - Objective text (strikethrough when completed)
///   - Progress bar (for "collect 5/10" type objectives)
///   - Progress text ("5/10")
/// </remarks>
/// <typeparam name="TQuest">The quest data type.</typeparam>
/// <typeparam name="TObjective">The objective data type.</typeparam>
public sealed class QuestTracker<TQuest, TObjective> : Component
{
    /// <summary>Create a quest tracker.</summary>
    /// <param name="quests">Reactive list of active/tracked quests.</param>
    public QuestTracker(ReactiveList<TQuest> quests);

    /// <summary>Function to extract the quest title from quest data.</summary>
    public Func<TQuest, string> TitleSelector { get; set; }

    /// <summary>Function to extract the list of objectives from quest data.</summary>
    public Func<TQuest, IReadOnlyList<TObjective>> ObjectivesSelector { get; set; }

    /// <summary>Function to extract objective description text.</summary>
    public Func<TObjective, string> ObjectiveTextSelector { get; set; }

    /// <summary>Function to extract objective current progress.</summary>
    public Func<TObjective, IReadOnlySignal<int>> ObjectiveProgressSelector { get; set; }

    /// <summary>Function to extract objective target count.</summary>
    public Func<TObjective, int> ObjectiveTargetSelector { get; set; }

    /// <summary>Function to determine if an objective is completed.</summary>
    public Func<TObjective, IReadOnlySignal<bool>> ObjectiveCompletedSelector { get; set; }

    /// <summary>Maximum number of quests to display simultaneously. Default: 5.</summary>
    public int MaxVisible { get; set; }
}
```

### Minimap

```csharp
/// <summary>
/// A minimap panel showing a RenderTexture view of the game world with icon overlays,
/// ping effects, and zoom control. The minimap camera is managed externally; this widget
/// handles only the UI presentation.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-minimap</c>, <c>jui-minimap__view</c>, <c>jui-minimap__icon</c>,
/// <c>jui-minimap__icon--player</c>, <c>jui-minimap__icon--enemy</c>,
/// <c>jui-minimap__icon--objective</c>, <c>jui-minimap__ping</c>,
/// <c>jui-minimap__border</c>.
///
/// Visual composition:
/// - Circular or square frame with border
/// - RenderTexture display from the minimap camera
/// - Icon overlays positioned relative to player position (computed from world-to-minimap transform)
/// - Ping effect: expanding ring animation at a minimap position (triggered externally)
/// - Zoom level indicator (optional)
/// </remarks>
public sealed class Minimap : Component
{
    /// <summary>Create a minimap.</summary>
    /// <param name="size">Size of the minimap in pixels.</param>
    /// <param name="circular">If true, renders with a circular mask. Default: true.</param>
    public Minimap(float size = 200f, bool circular = true);

    /// <summary>The RenderTexture from the minimap camera.</summary>
    public Signal<RenderTexture> MapTexture { get; }

    /// <summary>Current zoom level (1.0 = default). Reactive and controllable.</summary>
    public Signal<float> Zoom { get; }

    /// <summary>Minimum zoom level. Default: 0.5.</summary>
    public float MinZoom { get; set; }

    /// <summary>Maximum zoom level. Default: 3.0.</summary>
    public float MaxZoom { get; set; }

    /// <summary>
    /// Add a tracked icon to the minimap. The icon's position is updated each frame
    /// based on the world position signal.
    /// </summary>
    /// <param name="id">Unique identifier for this icon (for later removal).</param>
    /// <param name="worldPosition">Signal providing the icon's world position.</param>
    /// <param name="icon">The icon texture.</param>
    /// <param name="color">Icon tint color.</param>
    public void AddIcon(string id, IReadOnlySignal<Vector3> worldPosition,
                        Texture2D icon, Color color = default);

    /// <summary>Remove a tracked icon by ID.</summary>
    /// <param name="id">The icon's unique identifier.</param>
    public void RemoveIcon(string id);

    /// <summary>
    /// Play a ping animation at the given world position on the minimap.
    /// Shows an expanding ring that fades out over ~1 second.
    /// </summary>
    /// <param name="worldPosition">World position to ping.</param>
    /// <param name="color">Ping ring color. Default: yellow.</param>
    public void Ping(Vector3 worldPosition, Color color = default);

    /// <summary>
    /// The world-to-minimap transform function. Converts a world position to a normalized
    /// minimap position (0,0 = top-left, 1,1 = bottom-right). Must be set by the consumer
    /// based on the minimap camera's projection.
    /// </summary>
    public Func<Vector3, Vector2> WorldToMinimap { get; set; }
}
```

### LoadingScreen

```csharp
/// <summary>
/// A full-screen loading display with progress bar, rotating tips text, background art,
/// and async scene load integration. Designed to be shown during scene transitions or
/// initial game load.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-loading</c>, <c>jui-loading__background</c>,
/// <c>jui-loading__progress</c>, <c>jui-loading__tip</c>,
/// <c>jui-loading__title</c>.
///
/// Visual composition:
/// - Full-screen background image (or solid color)
/// - Title text (e.g., game name or area name)
/// - Progress bar (ProgressBar widget, determinate or indeterminate)
/// - Tip text (rotates through a list of tips on a timer)
///
/// Scene load integration:
/// - `ShowForSceneLoad` accepts a scene name and manages the `SceneManager.LoadSceneAsync`
///   operation, binding the progress to the loading screen automatically.
/// </remarks>
public sealed class LoadingScreen : Component
{
    /// <summary>Create a loading screen.</summary>
    public LoadingScreen();

    /// <summary>Loading progress (0..1). Bound to the internal ProgressBar.</summary>
    public Signal<float> Progress { get; }

    /// <summary>Title text displayed above the progress bar.</summary>
    public Signal<string> Title { get; }

    /// <summary>Background image. Null = solid background color.</summary>
    public Signal<Texture2D> Background { get; }

    /// <summary>List of tip strings to rotate through during loading.</summary>
    public IReadOnlyList<string> Tips { get; set; }

    /// <summary>
    /// Interval between tip rotations in seconds. Default: 5.
    /// </summary>
    public float TipRotationInterval { get; set; }

    /// <summary>The currently displayed tip text. Reactive.</summary>
    public IReadOnlySignal<string> CurrentTip { get; }

    /// <summary>
    /// Show the loading screen, load the scene asynchronously, and hide when complete.
    /// Progress is automatically bound to the scene load operation progress.
    /// </summary>
    /// <param name="sceneName">The scene to load.</param>
    /// <param name="title">Optional title text. Defaults to scene name.</param>
    /// <param name="additionalWork">
    /// Optional additional async work to perform after the scene loads but before
    /// hiding the loading screen (e.g., initializing game systems).
    /// </param>
    /// <returns>A UniTask that completes when the loading screen is hidden.</returns>
    public UniTask ShowForSceneLoad(
        string sceneName,
        string title = null,
        Func<UniTask> additionalWork = null);

    /// <summary>
    /// Show the loading screen with manual progress control. Returns when
    /// <see cref="Hide"/> is called.
    /// </summary>
    public void Show();

    /// <summary>Hide the loading screen with a fade-out animation.</summary>
    public void Hide();
}
```

## Data Structures

| Type | Role |
|------|------|
| `Signal<float> Current/Max` | Health bar signals driving fill width calculation (`Current / Max`). |
| `Signal<float> Remaining/Duration` | Cooldown overlay signals driving sweep angle (`Remaining / Duration`). |
| `ReactiveList<T>` | Backing data for InventoryGrid, ActionBar, BuffBar, QuestTracker. Items can be added/removed reactively. |
| `DialogueChoice` | Readonly struct for dialogue choice options with text, enabled state, and condition label. |
| `DamageType` enum | Damage type for DamageNumber coloring (Physical, Fire, Ice, etc.). |
| `ItemRarity` enum | Item rarity for InventoryGrid border coloring (Common through Legendary). |
| `Dictionary<string, (VisualElement icon, IDisposable effect)>` | Internal icon registry in Minimap. Maps icon IDs to their visual elements and position-tracking effects. |
| `Stack<DamageNumber>` | Internal pool in DamageNumber.Spawn for reusable instances. |

## Implementation Notes

### HealthBar

- **Ghost bar animation**: When `Current` decreases, the ghost bar retains the previous width for `GhostDrainDuration * 0.3` seconds (linger), then linearly drains to match the current fill over the remaining duration. Implemented via a UniTask-based tween on the ghost bar's USS `width` property.
- **Damage flash**: On `Current` decrease, the fill bar's USS class `jui-health--damage-flash` is added for `DamageFlashDuration`, then removed. The USS rule applies a bright overlay color.
- **Critical threshold**: An effect watches the computed percentage (`Current / Max`). When it drops below `CriticalThreshold`, the USS class `jui-health--critical` is added, which changes the fill color to red and optionally adds a pulse animation.
- **Shield overlay**: The shield bar is rendered as a separate fill on top of the health fill. Its width is `min(Shield / Max, 1.0)` and it uses a translucent blue color. When shield absorbs damage, it shrinks before the health bar, providing visual feedback.

### CooldownOverlay

- **Radial sweep**: Uses a custom USS render with a shader that draws a radial wipe. The sweep angle is computed as `(1 - Remaining / Duration) * 360` degrees. The shader is applied via a `CustomStyleProperty` or inline style with `background-image` using a procedurally generated texture updated per-frame while on cooldown.
- **Timer text formatting**: When `Remaining > 1`, shows integer seconds (e.g., "3"). When `Remaining <= 1`, shows one decimal (e.g., "0.4"). When `Remaining == 0`, the timer is hidden.
- **Ready flash**: On transition from `Remaining > 0` to `Remaining == 0`, a white glow animation plays: opacity 0 -> 0.8 -> 0 over 0.5 seconds. The `jui-cooldown--ready` class triggers this animation via USS `@keyframes`.

### InventoryGrid

- **VirtualGrid rendering**: The grid uses the virtualization system (Section 18) so only visible cells are rendered. This supports inventories with hundreds of slots without performance issues.
- **Drag-drop**: When dragging, a semi-transparent copy of the item icon follows the cursor. Dropping on another cell swaps or moves the item. Dropping outside the grid triggers an optional "drop" action (e.g., dropping items into the world).
- **Rarity border**: Each rarity tier maps to a USS class with a colored `box-shadow` or `border-color`. Legendary items additionally have a subtle glow animation.

### ResourceCounter

- **Delta label pooling**: A pool of 5 delta label elements is pre-created. On count change, the effect computes `newValue - oldValue`, rents a label, sets its text to `"+{delta}"` or `"{delta}"`, applies the appropriate color, positions it at the count label's location, and starts the float-up/fade animation. On animation complete, the label is returned to the pool.
- **Rapid changes**: If multiple changes occur within the animation duration, multiple delta labels can be active simultaneously (staggered vertically to avoid overlap).

### ActionBar

- **Slot rendering**: Uses `For<T>` to render each slot. Each slot contains an icon, a CooldownOverlay, and a keybind label. The CooldownOverlay's `Remaining` and `Duration` are bound to signals extracted via selectors.
- **Keyboard activation**: The ActionBar listens for keypress events matching the `KeybindLabels`. When a keybind is pressed, `OnSlotActivated` is invoked for the corresponding slot.
- **Drag-drop reorder**: Slots can be dragged and dropped to reorder abilities. The `OnSlotMoved` callback is invoked with the source and destination indices.

### DialogueBox

- **Typewriter implementation**: An effect starts a UniTask coroutine that incrementally reveals characters. Each frame, it calculates how many characters should be visible based on elapsed time and `TypewriterSpeed`, then sets the label's `text` to the substring. Rich text tags are handled by counting only visible characters (tags are always included in the output).
- **Skip behavior**: If `IsTyping` is true, `Skip()` cancels the typewriter coroutine and shows the full text. If `IsTyping` is false (text fully revealed), `Skip()` triggers the advance/choice callback.
- **Choice buttons**: After the typewriter completes, if `Choices` is non-empty, buttons are rendered via `For<DialogueChoice>`. Each button click resolves the `UniTaskCompletionSource` with the choice index.
- **Auto-advance**: After text is fully revealed and `AutoAdvanceDelay > 0`, a delayed UniTask resolves the completion source with index -1 after the delay.

### DamageNumber

- **Pooling**: Uses a static `Stack<DamageNumber>` with a max size of 50. `Spawn()` checks the pool first. If empty, creates a new instance. Pre-warms 10 instances on the first `Spawn` call.
- **Animation**: On `Show()`, the damage number is added to the overlay portal layer and a UniTask-based animation runs: translate Y from 0 to `-FloatDistance` with ease-out easing, and opacity from 1 to 0 starting at 70% of the animation duration. On completion, the element is removed from the visual tree and returned to the pool.
- **Critical scaling**: When `IsCritical` is true, the font size is doubled and a brief scale bounce animation plays (scale 1.5 -> 1.0 over 0.2s) at the start of the float.
- **Horizontal scatter**: Each spawn adds a random X offset within `[-HorizontalScatter, +HorizontalScatter]` to prevent multiple simultaneous damage numbers from stacking on the same pixel.

### QuestTracker

- **Objective completion animation**: When an objective's `IsCompleted` signal transitions from false to true, the checkmark icon plays a scale-in animation (0 -> 1 with overshoot easing), and the objective text receives a strikethrough USS class.
- **Progress bar integration**: Each objective with a target count > 1 shows a small inline ProgressBar bound to `Progress / Target`.
- **Max visible**: When more quests are tracked than `MaxVisible`, only the first N are shown with a "+X more" indicator at the bottom.

### Minimap

- **Icon positioning**: Each tracked icon has an internal effect that reads the `worldPosition` signal, passes it through `WorldToMinimap`, and sets the icon's USS `left` and `top` properties. Icons outside the minimap bounds are hidden.
- **Ping animation**: `Ping()` creates a ring element at the computed minimap position and animates it: scale 0 -> 2 with opacity 1 -> 0 over 1 second. The ring element is removed after the animation completes.
- **Zoom**: The `Zoom` signal controls the minimap camera's orthographic size (managed externally via an effect that watches `Zoom` and updates the camera). The minimap widget itself scales the icon positions by the zoom factor.
- **Circular mask**: When `circular = true`, the minimap frame uses USS `border-radius: 50%` and `overflow: hidden` to create a circular viewport.

### LoadingScreen

- **Scene load integration**: `ShowForSceneLoad` calls `SceneManager.LoadSceneAsync` and binds the operation's `progress` to the `Progress` signal each frame via a UniTask loop. After the scene activates, if `additionalWork` is provided, it runs that work while keeping the loading screen visible. Finally, `Hide()` is called with a fade-out.
- **Tip rotation**: A UniTask loop cycles through `Tips` every `TipRotationInterval` seconds, updating the `CurrentTip` signal. Tips are shown with a crossfade animation (opacity 1 -> 0 on old tip, 0 -> 1 on new tip).
- **Full-screen coverage**: The loading screen is rendered via a portal to the root overlay layer, ensuring it covers all other UI. Its USS `position: absolute` with `left/top/right/bottom: 0`.

## Source Generator Notes

N/A for this section. All game widgets are runtime components. No source generation is involved. Widget creation uses either direct construction (`new HealthBar(...)`) or the `El.Create<T>()` factory pattern.

## Usage Examples

```csharp
// HealthBar bound to a character's stats
public partial class PlayerHUD : Component
{
    [Inject] private PlayerStats _stats;

    protected override Element Render() => El.Div(
        El.Create<HealthBar>(showLabel: true)
            .Bind(_stats.CurrentHealth, _stats.MaxHealth),

        El.Create<Row>(gap: 8,
            El.Create<ResourceCounter>(goldIcon).Bind(_stats.Gold),
            El.Create<ResourceCounter>(gemIcon).Bind(_stats.Gems)
        ),

        El.Create<BuffBar<BuffData>>(_stats.ActiveBuffs)
        {
            IconSelector = static b => b.Icon,
            RemainingSelector = static b => b.Remaining,
            DurationSelector = static b => b.TotalDuration,
            StackCountSelector = static b => b.Stacks,
            IsDebuffSelector = static b => b.IsDebuff,
            TooltipBuilder = static b => El.Div(
                El.Create<Label>(b.Name).AddClass("tooltip-title"),
                El.Create<Label>(b.Description),
                El.Create<Label>($"{b.Remaining.Value:F1}s remaining")
            )
        },

        El.Create<ActionBar<AbilityData>>(_stats.Abilities, slotCount: 6)
        {
            IconSelector = static a => a.Icon,
            CooldownRemainingSelector = static a => a.CooldownRemaining,
            CooldownDurationSelector = static a => a.CooldownDuration,
            KeybindLabels = new[] { "1", "2", "3", "4", "5", "6" },
            OnSlotActivated = static (ability, _) => ability.TryActivate()
        }
    );
}

// Damage number spawning from combat system
public static class CombatVisuals
{
    public static void OnDamageDealt(int amount, Vector3 worldPos, DamageType type, bool crit)
    {
        var screenPos = Camera.main.WorldToScreenPoint(worldPos);
        DamageNumber.Spawn(amount, screenPos, type, crit);
    }

    public static void OnHeal(int amount, Vector3 worldPos)
    {
        var screenPos = Camera.main.WorldToScreenPoint(worldPos);
        DamageNumber.Spawn(amount, screenPos, DamageType.Heal, isCritical: false);
    }
}

// Dialogue sequence
public async UniTask RunDialogue(DialogueBox dialogue)
{
    var choice = await dialogue.ShowLineAsync(
        "Elder Sage",
        "The darkness grows stronger. Will you help us defend the village?",
        sagePortrait,
        new[]
        {
            new DialogueChoice { Text = "I will help.", Enabled = true },
            new DialogueChoice { Text = "I need more information.", Enabled = true },
            new DialogueChoice { Text = "[Strength 15] I'll handle it alone.",
                                 Enabled = playerStrength >= 15,
                                 Condition = "[Strength 15]" }
        });

    switch (choice)
    {
        case 0: await StartDefenseQuest(); break;
        case 1: await dialogue.ShowLineAsync("Elder Sage",
                    "Creatures emerge from the Shadow Rift each night..."); break;
        case 2: await StartSoloQuest(); break;
    }
}

// Quest tracker
public partial class QuestPanel : Component
{
    [Inject] private QuestManager _questManager;

    protected override Element Render() =>
        El.Create<QuestTracker<Quest, QuestObjective>>(_questManager.TrackedQuests)
        {
            TitleSelector = static q => q.Title,
            ObjectivesSelector = static q => q.Objectives,
            ObjectiveTextSelector = static o => o.Description,
            ObjectiveProgressSelector = static o => o.CurrentProgress,
            ObjectiveTargetSelector = static o => o.TargetCount,
            ObjectiveCompletedSelector = static o => o.IsCompleted,
            MaxVisible = 3
        };
}

// Inventory with context menu
public partial class InventoryPanel : Component
{
    [Inject] private InventorySystem _inventory;

    protected override Element Render() =>
        El.Create<InventoryGrid<ItemData>>(_inventory.Items, columns: 8, cellSize: 64)
        {
            IconSelector = static item => item.Icon,
            StackCountSelector = static item => item.StackCount,
            RaritySelector = static item => item.Rarity,
            TooltipBuilder = static item => El.Div(
                El.Create<Label>(item.Name).AddClass($"rarity-{item.Rarity}"),
                El.Create<Label>(item.Description),
                El.Create<Label>($"Level {item.RequiredLevel}")
            ),
            ContextMenuBuilder = static (item, slotIndex, menu) =>
            {
                menu.Item("Use", () => InventorySystem.Use(slotIndex));
                menu.Item("Split Stack", () => InventorySystem.Split(slotIndex));
                menu.Separator();
                menu.Item("Drop", () => InventorySystem.Drop(slotIndex));
            },
            AllowDragDrop = true,
            OnItemMoved = static (from, to) => InventorySystem.Swap(from, to)
        };
}

// Minimap setup
public partial class MinimapWidget : Component
{
    [Inject] private MinimapCamera _camera;
    [Inject] private EntityTracker _entities;

    protected override void OnInit()
    {
        var minimap = new Minimap(size: 200, circular: true);
        minimap.MapTexture.Value = _camera.RenderTexture;
        minimap.WorldToMinimap = _camera.WorldToViewport;
        minimap.Zoom.Value = 1.5f;

        // Track player
        minimap.AddIcon("player", _entities.PlayerPosition, playerIcon, Color.white);

        // Track enemies
        foreach (var enemy in _entities.Enemies)
            minimap.AddIcon(enemy.Id, enemy.Position, enemyIcon, Color.red);

        // Ping on alert
        AlertSystem.OnAlert += pos => minimap.Ping(pos, Color.yellow);
    }
}

// Loading screen for scene transitions
public async UniTask TransitionToLevel(string levelName)
{
    var loading = new LoadingScreen
    {
        Tips = new[]
        {
            "Tip: Hold Shift to sprint!",
            "Tip: Talk to NPCs for side quests.",
            "Tip: Craft potions at the alchemy table.",
            "Tip: Dodge roll has invincibility frames."
        },
        TipRotationInterval = 4f
    };

    await loading.ShowForSceneLoad(
        sceneName: levelName,
        title: $"Entering {levelName}...",
        additionalWork: async () =>
        {
            await GameSystems.InitializeForLevel(levelName);
            await AssetPreloader.PreloadEssentials();
        }
    );
}
```

## Test Plan

### HealthBar

1. **Fill width matches current/max ratio**: Set `Current = 75`, `Max = 100`, verify fill width is 75% of track.
2. **Ghost bar lingers on damage**: Set `Current` from 100 to 50, verify ghost bar stays at 100% briefly, then drains.
3. **Damage flash triggers on decrease**: Decrease `Current`, verify `jui-health--damage-flash` class is briefly applied.
4. **Critical threshold color change**: Set `Current` below `CriticalThreshold * Max`, verify `jui-health--critical` class is added.
5. **Shield overlay displays**: Set `Shield = 20`, `Max = 100`, verify shield bar is visible at 20% width.
6. **Label text updates**: Set `Current = 80`, `Max = 100`, verify label reads "80/100".

### CooldownOverlay

7. **Sweep angle matches remaining/duration**: Set `Remaining = 5`, `Duration = 10`, verify sweep covers 50%.
8. **Timer text shows remaining**: Set `Remaining = 3.5`, verify timer shows "3". Set `Remaining = 0.4`, verify shows "0.4".
9. **Ready flash on completion**: Transition `Remaining` from 1 to 0, verify `jui-cooldown--ready` class is applied.
10. **No overlay when remaining is 0**: Set `Remaining = 0`, verify sweep overlay is hidden.

### InventoryGrid

11. **Empty slots rendered**: Create grid with null entries, verify empty slot backgrounds are visible.
12. **Item icon displayed**: Set a non-null item, verify icon texture appears.
13. **Stack count badge**: Set item with stack count 5, verify badge shows "5".
14. **Rarity border**: Set item with `Rare` rarity, verify `jui-inventory__rarity--rare` class.
15. **Drag-drop reorder**: Simulate drag from slot 0 to slot 3, verify `OnItemMoved(0, 3)` invoked.
16. **Context menu on right-click**: Right-click a filled slot, verify context menu appears.

### ResourceCounter

17. **Count displays formatted**: Set `Count = 1234`, verify label shows "1,234".
18. **Positive delta animation**: Increase `Count` by 5, verify "+5" label floats up in green.
19. **Negative delta animation**: Decrease `Count` by 3, verify "-3" label floats up in red.
20. **Rapid changes show multiple deltas**: Change `Count` 3 times rapidly, verify 3 delta labels visible.

### ActionBar

21. **Slot rendering**: Create with 6 slots, verify 6 slot elements rendered.
22. **Cooldown overlay binding**: Set a slot's cooldown to active, verify CooldownOverlay is visible.
23. **Keybind label display**: Set `KeybindLabels`, verify each slot shows its keybind text.
24. **Slot activation callback**: Click a slot, verify `OnSlotActivated` invoked with correct data.
25. **Drag-drop reorder**: Drag slot 0 to slot 2, verify `OnSlotMoved(0, 2)` invoked.

### BuffBar

26. **Buffs rendered via For**: Add 3 buffs to the list, verify 3 buff icons rendered.
27. **Buff removal animates out**: Remove a buff, verify it is removed from the visual tree.
28. **Timer ring progress**: Set buff remaining to 50% of duration, verify ring is 50% filled.
29. **Stack count badge**: Set stacks to 3, verify badge shows "3". Set to 1, verify badge hidden.
30. **Debuff border color**: Set `IsDebuff = true`, verify `jui-buffs--debuff` class applied.

### DialogueBox

31. **Typewriter reveals characters**: Set text, verify characters appear one at a time.
32. **Skip shows full text**: During typewriter, call `Skip()`, verify all text instantly visible.
33. **Choices shown after text**: Set text with choices, wait for typewriter, verify choice buttons appear.
34. **Choice selection returns index**: Click choice 1, verify `ShowLineAsync` returns 1.
35. **Auto-advance timer**: Set `AutoAdvanceDelay = 1`, wait, verify advance triggered.
36. **Portrait display**: Set portrait texture, verify portrait image is visible.

### DamageNumber

37. **Spawn shows number**: Call `Spawn(42, pos)`, verify "42" text visible at position.
38. **Float-up animation**: After spawn, verify Y position decreases over time.
39. **Fade-out animation**: After spawn, verify opacity decreases to 0.
40. **Critical scaling**: Spawn with `isCritical = true`, verify font size is doubled.
41. **Pooling reuse**: Spawn and wait for animation, verify instance returned to pool. Spawn again, verify same instance reused.
42. **Horizontal scatter**: Spawn 5 at same position, verify X positions differ.

### QuestTracker

43. **Quests rendered**: Add 3 quests, verify 3 quest entries visible.
44. **Objective progress bar**: Set objective progress 5/10, verify progress bar at 50%.
45. **Objective completion animation**: Set objective completed, verify checkmark animation plays and text has strikethrough.
46. **MaxVisible limit**: Add 6 quests with `MaxVisible = 3`, verify only 3 shown.

### Minimap

47. **RenderTexture display**: Set `MapTexture`, verify texture is rendered in the minimap view.
48. **Icon positioning**: Add icon at world position, verify it appears at correct minimap position via `WorldToMinimap`.
49. **Icon removal**: Call `RemoveIcon`, verify icon element removed.
50. **Ping animation**: Call `Ping`, verify ring element animates and then is removed.
51. **Zoom signal**: Change `Zoom`, verify icon positions scale accordingly.

### LoadingScreen

52. **Progress bar binding**: Set `Progress = 0.5`, verify progress bar at 50%.
53. **Tip rotation**: Set 3 tips with 1s interval, wait 3s, verify all 3 tips shown in sequence.
54. **Scene load integration**: Call `ShowForSceneLoad`, verify loading screen visible and progress updates.
55. **Hide fade-out**: Call `Hide`, verify opacity animates to 0 and loading screen is removed.

## Acceptance Criteria

### HealthBar

- [ ] Displays fill bar at `Current / Max` proportion
- [ ] Ghost bar lingers and drains on health decrease
- [ ] Damage flash CSS class briefly applied on decrease
- [ ] Color transitions to critical below `CriticalThreshold`
- [ ] Shield overlay renders above health fill

### CooldownOverlay

- [ ] Radial sweep angle driven by `Remaining / Duration`
- [ ] Timer text formats correctly (integer > 1s, decimal <= 1s)
- [ ] Ready flash animation plays when cooldown reaches 0
- [ ] Overlay hidden when `Remaining == 0`

### InventoryGrid

- [ ] VirtualGrid rendering for large inventories
- [ ] Drag-drop reordering with `OnItemMoved` callback
- [ ] Tooltip on hover/longpress
- [ ] Context menu on right-click
- [ ] Rarity border coloring via USS classes

### ResourceCounter

- [ ] Formatted count display
- [ ] Animated delta labels on count change (pooled)
- [ ] Color differentiation for positive/negative deltas

### ActionBar

- [ ] For-based slot rendering with CooldownOverlay per slot
- [ ] Keybind labels displayed per slot
- [ ] Drag-drop reorder support
- [ ] `OnSlotActivated` callback on click/keypress

### BuffBar

- [ ] For-based reactive buff rendering
- [ ] Timer ring progress per buff
- [ ] Stack count badge visibility
- [ ] Buff/debuff border distinction

### DialogueBox

- [ ] Typewriter character-by-character reveal at configurable speed
- [ ] Skip instantly reveals full text
- [ ] Choice buttons rendered after text completion
- [ ] `ShowLineAsync` returns choice index
- [ ] Auto-advance timer support

### DamageNumber

- [ ] Pooled spawning with pre-warm (max 50, pre-warm 10)
- [ ] Float-up animation with opacity fade
- [ ] Critical hit 2x scaling with bounce
- [ ] Damage type color mapping
- [ ] Horizontal scatter to prevent overlap

### QuestTracker

- [ ] For-based quest list rendering
- [ ] Progress bars for count-based objectives
- [ ] Checkmark animation on objective completion
- [ ] `MaxVisible` limit with overflow indicator

### Minimap

- [ ] RenderTexture display with circular/square mask
- [ ] World-to-minimap icon positioning via transform function
- [ ] Ping expanding ring animation
- [ ] Zoom signal controls icon scale

### LoadingScreen

- [ ] Progress bar bound to `Progress` signal
- [ ] Tip rotation with crossfade animation
- [ ] `ShowForSceneLoad` integrates with `SceneManager.LoadSceneAsync`
- [ ] Full-screen overlay via portal

### General

- [ ] All game widgets extend `Component` and follow JUI lifecycle
- [ ] All widget state is signal-driven with reactive updates
- [ ] All public APIs have XML documentation
- [ ] All USS classes follow `jui-{widget}` naming convention
- [ ] Pooling strategies documented and implemented for high-frequency widgets
- [ ] All animations use the JUI animation system (Section 15)
