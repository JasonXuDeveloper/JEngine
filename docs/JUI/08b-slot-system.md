# Section 8b — Slot / Content Projection System

## Overview

Slots enable named content insertion points within components, similar to web component slots or React's `children` and render props. A parent provides content factories, and the child component renders them in designated areas. The system supports a default slot (accessed via `Children`) and named slots (accessed via generated properties). Content projected into a slot inherits the parent's DI scope, not the slot-owning component's scope.

## Dependencies

- Section 6 (Component)

## File Structure

```
Runtime/JUI/Components/
  Slot.cs                    # SlotAttribute definition
  (update) Component.cs      # Add slot support to base Component
```

## API Design

```csharp
/// <summary>
/// Marks a <see cref="VisualElement"/> field as a content insertion point.
/// The source generator creates a corresponding property on the component
/// for type-safe content injection.
/// </summary>
/// <remarks>
/// <para>
/// A slot with no name (or <c>null</c> name) is the default slot,
/// populated by the component's <see cref="Component.Children"/> collection.
/// Named slots receive content via generated properties whose names match
/// the slot name in PascalCase.
/// </para>
/// <para>
/// Slot content inherits the <em>parent's</em> DI scope, not the component
/// that declares the slot. This ensures that projected content can resolve
/// services from the context where it was authored, not where it is rendered.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [Slot("header")] private VisualElement _headerSlot;
/// // Source gen produces: public Func&lt;Component&gt; Header { get; init; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class SlotAttribute : Attribute
{
    /// <summary>
    /// The slot name. If <c>null</c> or empty, this is the default slot
    /// (populated via <see cref="Component.Children"/>).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Creates a new <see cref="SlotAttribute"/>.
    /// </summary>
    /// <param name="name">
    /// The slot name, or <c>null</c> for the default slot.
    /// </param>
    public SlotAttribute(string name = null);
}

// ----- Additions to Component base class -----

/// <summary>
/// Base class for all JUI components.
/// </summary>
public abstract partial class Component
{
    /// <summary>
    /// Sets content for a named slot at runtime. Typically not called
    /// directly; use the generated typed properties instead.
    /// </summary>
    /// <param name="slotName">
    /// The slot name. Use <c>null</c> or empty string for the default slot.
    /// </param>
    /// <param name="content">
    /// Factory function that creates the component to render in the slot.
    /// Pass <c>null</c> to clear the slot.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="slotName"/> does not match any declared
    /// <see cref="SlotAttribute"/> on this component.
    /// </exception>
    public void SetSlotContent(string slotName, Func<Component> content);

    /// <summary>
    /// Default slot content. Items added to <see cref="Children"/> are
    /// rendered in the default slot (the <see cref="SlotAttribute"/> field
    /// with no name).
    /// </summary>
    /// <remarks>
    /// If the component declares no default slot, children are appended
    /// to the component's root <see cref="VisualElement"/>.
    /// </remarks>
    public List<Func<Component>> Children { get; init; }
}
```

## Data Structures

### SlotInfo (internal, used by source gen and runtime)

```csharp
internal struct SlotInfo
{
    /// <summary>Slot name (null for default).</summary>
    public string Name;

    /// <summary>Target VisualElement for content attachment.</summary>
    public VisualElement Target;

    /// <summary>Currently mounted content component (null if empty).</summary>
    public Component MountedContent;

    /// <summary>The DI scope to use for content resolution (parent's scope).</summary>
    public IServiceScope ContentScope;
}
```

### Component Internal State (additions)

```
_slotRegistry : Dictionary<string, SlotInfo>   // keyed by slot name ("" for default)
_slotContents : Dictionary<string, Func<Component>>  // pending content factories
```

## Implementation Notes

### Source Generator Behavior

The source generator (from Section 6) scans for `[Slot]` attributes on `VisualElement` fields and generates:

1. **A typed init property** for each named slot:

   ```csharp
   // For [Slot("header")] private VisualElement _headerSlot;
   // Generator produces:
   public Func<Component> Header { get; init; }
   ```

   The property name is derived from the slot name converted to PascalCase. The generator ensures no collision with existing members.

2. **Slot registration** in the generated `InitializeSlots()` method (called during component initialization):

   ```csharp
   partial void InitializeSlots()
   {
       RegisterSlot("header", _headerSlot);
       RegisterSlot(null, _defaultSlot);  // default slot
   }
   ```

3. **Content mounting** in the generated `MountSlotContent()` method (called during mount):

   ```csharp
   partial void MountSlotContent()
   {
       if (Header != null) SetSlotContent("header", Header);
       // Children handled separately via default slot
   }
   ```

### Slot Content Lifecycle

**Mount flow:**

1. Parent creates child component via object initializer, setting slot properties (e.g., `Header = () => new Label("Title")`).
2. Child component initializes, `InitializeSlots()` registers slot targets.
3. During mount, `MountSlotContent()` calls `SetSlotContent` for each provided slot.
4. `SetSlotContent` invokes the factory, creating the content component.
5. Content component is mounted with the **parent's DI scope** (not the slot-owning component's scope).
6. Content's `VisualElement` is appended as a child of the slot's target `VisualElement`.

**Dispose flow:**

1. When the slot-owning component is disposed, all slot content components are disposed.
2. Alternatively, calling `SetSlotContent(name, null)` disposes and removes the current content.
3. Calling `SetSlotContent(name, newFactory)` disposes old content, then creates and mounts new content.

### DI Scope Resolution

Slot content must resolve dependencies from the parent's scope to maintain the principle of least surprise. The content was authored in the parent's context and should have access to the parent's services.

Implementation: when `SetSlotContent` creates the content component, it passes the parent's `IServiceScope` (captured during component initialization) rather than the slot owner's scope.

### Default Slot (Children)

- The `Children` property maps to the default slot (the `[Slot]` field with `name = null`).
- If multiple children are provided, they are each created and appended to the default slot target in order.
- If the component has no `[Slot]` with `name = null`, children are appended to the component's root `VisualElement`.

### Edge Cases

- **Empty slot**: A slot with no content assigned renders nothing. No error, no placeholder.
- **Slot without matching attribute**: Calling `SetSlotContent` with a name that has no corresponding `[Slot]` field throws `ArgumentException`.
- **Overwriting content**: Setting new content on an already-filled slot disposes the old content first.
- **Null factory**: Passing `null` to `SetSlotContent` clears the slot.

## Source Generator Notes

### Input

The source generator scans each `partial class` that extends `Component` for fields decorated with `[Slot]`:

```csharp
[UIComponent("Card.uxml")]
public partial class Card : Component
{
    [Slot] private VisualElement _defaultSlot;
    [Slot("header")] private VisualElement _headerSlot;
    [Slot("actions")] private VisualElement _actionsSlot;
}
```

### Output

The generator emits a partial class with:

```csharp
public partial class Card
{
    /// <summary>Content for the "header" slot.</summary>
    public Func<Component> Header { get; init; }

    /// <summary>Content for the "actions" slot.</summary>
    public Func<Component> Actions { get; init; }

    partial void InitializeSlots()
    {
        RegisterSlot(null, _defaultSlot);
        RegisterSlot("header", _headerSlot);
        RegisterSlot("actions", _actionsSlot);
    }

    partial void MountSlotContent()
    {
        if (Header != null)
            SetSlotContent("header", Header);
        if (Actions != null)
            SetSlotContent("actions", Actions);
        // Default slot handled by Children in base class
    }
}
```

### Naming Rules

| Slot Name | Generated Property |
|-----------|-------------------|
| `null` | (uses `Children`) |
| `"header"` | `Header` |
| `"actions"` | `Actions` |
| `"left-panel"` | `LeftPanel` |
| `"icon_area"` | `IconArea` |

Conversion: split on `-` and `_`, PascalCase each segment.

### Diagnostics

| ID | Severity | Description |
|----|----------|-------------|
| `JUI0301` | Error | `[Slot]` on non-`VisualElement` field |
| `JUI0302` | Error | Duplicate slot name in same component |
| `JUI0303` | Warning | Slot name collides with existing member name |
| `JUI0304` | Error | `[Slot]` on non-partial class |

## Usage Examples

### Defining a slotted component

```csharp
[UIComponent("Card.uxml")]
public partial class Card : Component
{
    [Slot] private VisualElement _defaultSlot;
    [Slot("header")] private VisualElement _headerSlot;
    [Slot("actions")] private VisualElement _actionsSlot;

    protected override void Render()
    {
        // Slots are populated automatically via generated code.
        // Additional rendering logic can go here.
    }
}
```

### Using a slotted component (parent provides content)

```csharp
new Card
{
    Header = () => new Row
    {
        Children =
        {
            () => new Icon("sword"),
            () => new Label("Legendary Sword")
        }
    },
    Actions = () => new Row
    {
        Children =
        {
            () => new Button("Equip") { OnClick = EquipItem },
            () => new Button("Drop") { OnClick = DropItem }
        }
    },
    Children =
    {
        () => new Label("A powerful sword forged in ancient flames."),
        () => new StatBlock(itemStats)
    }
};
```

### Reactive slot content

```csharp
new Card
{
    Header = () => new Show
    {
        When = isEditing,
        Then = () => new TextInput { Value = itemName },
        Else = () => new Label { Text = itemName }
    },
    Children = { () => new Label("Card body") }
};
```

### Nested slots

```csharp
// Outer layout with slots
[UIComponent("PageLayout.uxml")]
public partial class PageLayout : Component
{
    [Slot("sidebar")] private VisualElement _sidebarSlot;
    [Slot] private VisualElement _defaultSlot;
}

// Usage with nested slotted components
new PageLayout
{
    Sidebar = () => new NavMenu(menuItems),
    Children =
    {
        () => new Card
        {
            Header = () => new Label("Welcome"),
            Children = { () => new Label("Page content here") }
        }
    }
};
```

## Test Plan

1. **Default slot renders children**: Create a component with `[Slot]` (no name) and provide `Children`. Verify children are rendered inside the slot's `VisualElement`.
2. **Named slot renders content**: Create a component with `[Slot("header")]` and provide `Header`. Verify content is rendered inside `_headerSlot`.
3. **Multiple named slots**: Component with three named slots, all provided. Verify each renders in the correct `VisualElement`.
4. **Empty slot renders nothing**: Provide no content for a named slot. Verify slot target is empty, no error thrown.
5. **Slot content inherits parent's DI scope**: Register a service in parent's scope. Verify `[Inject]` in slot content resolves from parent, not from the Card's scope.
6. **Disposal propagates**: Dispose the slot-owning component. Verify all slot content components are disposed.
7. **SetSlotContent replaces content**: Call `SetSlotContent` with new factory. Verify old content disposed, new content mounted.
8. **SetSlotContent with null clears slot**: Call `SetSlotContent(name, null)`. Verify content disposed and slot is empty.
9. **Invalid slot name throws**: Call `SetSlotContent("nonexistent", factory)`. Verify `ArgumentException`.
10. **Source gen produces correct properties**: Verify generated partial class has typed `Func<Component>` properties matching slot names.
11. **Source gen PascalCase conversion**: Slot name `"left-panel"` produces property `LeftPanel`.
12. **Source gen diagnostic JUI0301**: `[Slot]` on non-`VisualElement` field produces error.
13. **Source gen diagnostic JUI0302**: Duplicate slot names produce error.
14. **Reactive slot content**: Slot content containing a `Show` reacts to signal changes correctly.
15. **Multiple children in default slot**: Three children provided, all render in order.

## Acceptance Criteria

- [ ] `[Slot]` attribute can be applied to `VisualElement` fields
- [ ] `[Slot]` with no name designates the default slot (populated via `Children`)
- [ ] `[Slot("name")]` designates a named slot with a generated typed property
- [ ] Source generator produces `Func<Component>` init properties for each named slot
- [ ] Source generator converts slot names to PascalCase properties
- [ ] Source generator emits diagnostics for invalid slot declarations
- [ ] Slot content is mounted inside the target `VisualElement`
- [ ] Slot content inherits the parent's DI scope, not the slot owner's scope
- [ ] Slot content lifecycle (mount/unmount/dispose) is managed by the slot owner
- [ ] `SetSlotContent` can dynamically replace or clear slot content
- [ ] Empty slots render nothing without errors
- [ ] `Children` collection maps to the default slot
- [ ] All tests from the test plan pass
