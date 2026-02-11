# Section 27 — Base Widgets & Layout Components

## Overview

A comprehensive widget library built on JUI's reactive component system, covering five categories:

1. **Base Controls** -- Fundamental interactive elements (Button, TextInput, Slider, Toggle, Dropdown, etc.) with variant/size systems, signal bindings, and USS class conventions.
2. **Layout Containers** -- Structural components (Stack, Row, Column, Grid, Wrap, Center, ScrollView, Accordion, TabView, Breadcrumb, Stepper, Splitter) for arranging children with flex/grid-based layout.
3. **Overlay Components** -- Layered UI (Modal, Toast, Tooltip, ContextMenu, Popover, Drawer) that render above the main content with focus management and dismissal logic.
4. **Feedback Indicators** -- Status and progress elements (ProgressBar, Skeleton, Spinner, Badge, Chip, Avatar, Tag) for communicating state to the user.
5. **Data Display** -- Complex data components (TreeView, Table, Calendar, Pagination) for structured information.

All widgets follow a consistent pattern: constructor with configuration parameters, signal-bound properties for reactive state, USS class naming (`jui-{widget}`, `jui-{widget}--{variant}`, `jui-{widget}--{size}`), and fluent API chaining.

## Dependencies

- Sections 1-2 (Signal, Computed, Effect) -- all widget state is signal-driven.
- Section 5 (Binding System) -- `BindSync`, `Bind` for two-way/one-way signal binding.
- Section 6 (Component Base Class) -- all widgets extend `Component`.
- Section 7 (Show/Switch) -- conditional rendering within widget templates.
- Section 8 (For) -- list rendering in Dropdown, TreeView, Table, etc.
- Section 13 (Event System) -- widgets publish typed events (e.g., `ButtonClickEvent`).
- Section 15 (Animation) -- ripple effects, transitions, skeleton shimmer.
- Section 16 (Theming) -- design tokens, variant colors, size scales.
- Section 18 (Virtualization) -- virtualized lists in Dropdown, Table, TreeView.

## File Structure

### Base Controls

- `Runtime/JUI/Widgets/Button.cs`
- `Runtime/JUI/Widgets/IconButton.cs`
- `Runtime/JUI/Widgets/ToggleButton.cs`
- `Runtime/JUI/Widgets/TextInput.cs`
- `Runtime/JUI/Widgets/Slider.cs`
- `Runtime/JUI/Widgets/Toggle.cs`
- `Runtime/JUI/Widgets/Dropdown.cs`
- `Runtime/JUI/Widgets/Label.cs`
- `Runtime/JUI/Widgets/Image.cs`
- `Runtime/JUI/Widgets/Divider.cs`
- `Runtime/JUI/Widgets/Spacer.cs`

### Layout

- `Runtime/JUI/Layout/Stack.cs`
- `Runtime/JUI/Layout/Row.cs`
- `Runtime/JUI/Layout/Column.cs`
- `Runtime/JUI/Layout/Grid.cs`
- `Runtime/JUI/Layout/Wrap.cs`
- `Runtime/JUI/Layout/Center.cs`
- `Runtime/JUI/Layout/ScrollView.cs`
- `Runtime/JUI/Layout/Accordion.cs`
- `Runtime/JUI/Layout/TabView.cs`
- `Runtime/JUI/Layout/Breadcrumb.cs`
- `Runtime/JUI/Layout/Stepper.cs`
- `Runtime/JUI/Layout/Splitter.cs`

### Overlay

- `Runtime/JUI/Widgets/Overlay/Modal.cs`
- `Runtime/JUI/Widgets/Overlay/Toast.cs`
- `Runtime/JUI/Widgets/Overlay/Tooltip.cs`
- `Runtime/JUI/Widgets/Overlay/ContextMenu.cs`
- `Runtime/JUI/Widgets/Overlay/Popover.cs`
- `Runtime/JUI/Widgets/Overlay/Drawer.cs`

### Feedback

- `Runtime/JUI/Widgets/Feedback/ProgressBar.cs`
- `Runtime/JUI/Widgets/Feedback/Skeleton.cs`
- `Runtime/JUI/Widgets/Feedback/Spinner.cs`
- `Runtime/JUI/Widgets/Feedback/Badge.cs`
- `Runtime/JUI/Widgets/Feedback/Chip.cs`
- `Runtime/JUI/Widgets/Feedback/Avatar.cs`
- `Runtime/JUI/Widgets/Feedback/Tag.cs`

### Data Display

- `Runtime/JUI/Widgets/Data/TreeView.cs`
- `Runtime/JUI/Widgets/Data/Table.cs`
- `Runtime/JUI/Widgets/Data/Calendar.cs`
- `Runtime/JUI/Widgets/Data/Pagination.cs`

## API Design

### Base Controls

#### Button

```csharp
/// <summary>
/// Variant styles for buttons, controlling color scheme and visual weight.
/// </summary>
public enum ButtonVariant
{
    /// <summary>High-emphasis filled button with primary theme color.</summary>
    Primary,
    /// <summary>Medium-emphasis outlined button.</summary>
    Secondary,
    /// <summary>Low-emphasis text-only button with no background or border.</summary>
    Ghost,
    /// <summary>High-emphasis filled button with danger/destructive color.</summary>
    Danger
}

/// <summary>
/// Size presets for buttons, controlling padding, font size, and min-height.
/// </summary>
public enum WidgetSize
{
    /// <summary>Compact size (24px height, 12px font).</summary>
    Small,
    /// <summary>Default size (32px height, 14px font).</summary>
    Medium,
    /// <summary>Large size (40px height, 16px font).</summary>
    Large
}

/// <summary>
/// A reactive button component with variant styles, size presets, icon support,
/// loading state, and a ripple click effect.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-btn</c>, <c>jui-btn--primary</c>, <c>jui-btn--secondary</c>,
/// <c>jui-btn--ghost</c>, <c>jui-btn--danger</c>, <c>jui-btn--sm</c>, <c>jui-btn--md</c>,
/// <c>jui-btn--lg</c>, <c>jui-btn--loading</c>, <c>jui-btn--disabled</c>.
/// </remarks>
public sealed class Button : Component
{
    /// <summary>Create a button with the given label text.</summary>
    /// <param name="text">The button label. Can be null for icon-only buttons.</param>
    /// <param name="variant">Visual variant. Default: Primary.</param>
    /// <param name="size">Size preset. Default: Medium.</param>
    public Button(string text = null, ButtonVariant variant = ButtonVariant.Primary,
                  WidgetSize size = WidgetSize.Medium);

    /// <summary>The button label text. Reactive: change to update the displayed text.</summary>
    public Signal<string> Text { get; }

    /// <summary>Whether the button is enabled and clickable. Reactive.</summary>
    public Signal<bool> IsEnabled { get; }

    /// <summary>Whether the button shows a loading spinner and disables interaction. Reactive.</summary>
    public Signal<bool> IsLoading { get; }

    /// <summary>Optional icon displayed before the label text.</summary>
    public Signal<Texture2D> Icon { get; }

    /// <summary>Visual variant. Can be changed at runtime.</summary>
    public Signal<ButtonVariant> Variant { get; }

    /// <summary>Size preset. Can be changed at runtime.</summary>
    public Signal<WidgetSize> Size { get; }

    /// <summary>
    /// Register a click handler with typed state to avoid closure allocation.
    /// </summary>
    /// <typeparam name="TState">The type of state passed to the handler.</typeparam>
    /// <param name="handler">The click handler. Receives the Button and state.</param>
    /// <param name="state">The state instance.</param>
    /// <returns>This button for fluent chaining.</returns>
    public Button OnClick<TState>(Action<Button, TState> handler, TState state);

    /// <summary>Bind the enabled state to a read-only signal.</summary>
    /// <param name="enabled">Signal controlling enabled state.</param>
    /// <returns>This button for fluent chaining.</returns>
    public Button Enabled(IReadOnlySignal<bool> enabled);
}
```

#### TextInput

```csharp
/// <summary>
/// A reactive text input with validation error display, character counter, placeholder,
/// password masking, and debounced input events.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-input</c>, <c>jui-input--error</c>, <c>jui-input--focused</c>,
/// <c>jui-input__field</c>, <c>jui-input__label</c>, <c>jui-input__error</c>,
/// <c>jui-input__counter</c>.
/// </remarks>
public sealed class TextInput : Component
{
    /// <summary>Create a text input with an optional floating label.</summary>
    /// <param name="label">Floating label text. Null for no label.</param>
    /// <param name="placeholder">Placeholder text shown when empty.</param>
    /// <param name="mask">If true, masks input characters (password mode).</param>
    public TextInput(string label = null, string placeholder = null, bool mask = false);

    /// <summary>The current input text. Two-way bindable with BindSync.</summary>
    public Signal<string> Value { get; }

    /// <summary>Placeholder text shown when the input is empty.</summary>
    public Signal<string> Placeholder { get; }

    /// <summary>Whether the input is enabled and editable.</summary>
    public Signal<bool> IsEnabled { get; }

    /// <summary>Maximum character count. 0 = unlimited. Shows a counter when > 0.</summary>
    public int MaxLength { get; set; }

    /// <summary>
    /// Bind a validation error signal from the validation subsystem.
    /// When the result is invalid, shows the error message below the input.
    /// </summary>
    /// <param name="error">The error signal from [Validate] generation.</param>
    /// <returns>This input for fluent chaining.</returns>
    public TextInput WithError(IReadOnlySignal<ValidationResult> error);

    /// <summary>Two-way bind the input value to a signal.</summary>
    /// <param name="signal">The signal to bind to.</param>
    /// <returns>This input for fluent chaining.</returns>
    public TextInput BindSync(Signal<string> signal);
}
```

#### Slider

```csharp
/// <summary>
/// A reactive slider control with configurable range, step size, tick marks,
/// and optional vertical orientation. Signal-bound value.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-slider</c>, <c>jui-slider--vertical</c>,
/// <c>jui-slider__track</c>, <c>jui-slider__fill</c>, <c>jui-slider__thumb</c>,
/// <c>jui-slider__mark</c>.
/// </remarks>
public sealed class Slider : Component
{
    /// <summary>Create a slider with the given range.</summary>
    /// <param name="min">Minimum value.</param>
    /// <param name="max">Maximum value.</param>
    /// <param name="step">Step increment. 0 = continuous.</param>
    /// <param name="vertical">If true, renders vertically.</param>
    public Slider(float min = 0f, float max = 1f, float step = 0f, bool vertical = false);

    /// <summary>The current slider value. Two-way bindable.</summary>
    public Signal<float> Value { get; }

    /// <summary>Whether the slider is enabled and draggable.</summary>
    public Signal<bool> IsEnabled { get; }

    /// <summary>Optional tick mark positions along the track.</summary>
    public float[] Marks { get; set; }

    /// <summary>Two-way bind the slider value to a signal.</summary>
    public Slider BindSync(Signal<float> signal);
}
```

#### Toggle

```csharp
/// <summary>
/// A reactive on/off toggle (switch style) with signal-bound checked state.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-toggle</c>, <c>jui-toggle--checked</c>, <c>jui-toggle--disabled</c>,
/// <c>jui-toggle__track</c>, <c>jui-toggle__thumb</c>, <c>jui-toggle__label</c>.
/// </remarks>
public sealed class Toggle : Component
{
    /// <summary>Create a toggle with an optional label.</summary>
    /// <param name="label">Label text displayed beside the toggle.</param>
    public Toggle(string label = null);

    /// <summary>Whether the toggle is checked (on). Two-way bindable.</summary>
    public Signal<bool> IsChecked { get; }

    /// <summary>Whether the toggle is enabled and interactive.</summary>
    public Signal<bool> IsEnabled { get; }

    /// <summary>Two-way bind the checked state to a signal.</summary>
    public Toggle BindSync(Signal<bool> signal);
}
```

#### Dropdown

```csharp
/// <summary>
/// A reactive dropdown with search filtering, multi-select mode, keyboard and gamepad
/// navigation, and virtualized option rendering for large lists.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-dropdown</c>, <c>jui-dropdown--open</c>, <c>jui-dropdown--multi</c>,
/// <c>jui-dropdown__trigger</c>, <c>jui-dropdown__panel</c>, <c>jui-dropdown__option</c>,
/// <c>jui-dropdown__option--selected</c>, <c>jui-dropdown__search</c>.
/// </remarks>
/// <typeparam name="T">The type of option values.</typeparam>
public sealed class Dropdown<T> : Component
{
    /// <summary>Create a dropdown with the given options.</summary>
    /// <param name="options">The selectable options.</param>
    /// <param name="labelSelector">Function to extract display text from an option.</param>
    /// <param name="multiSelect">If true, allows multiple selections.</param>
    /// <param name="searchable">If true, shows a search input to filter options.</param>
    public Dropdown(
        ReactiveList<T> options,
        Func<T, string> labelSelector,
        bool multiSelect = false,
        bool searchable = false);

    /// <summary>The currently selected value (single-select mode).</summary>
    public Signal<T> SelectedValue { get; }

    /// <summary>The currently selected values (multi-select mode).</summary>
    public ReactiveList<T> SelectedValues { get; }

    /// <summary>Whether the dropdown panel is open.</summary>
    public Signal<bool> IsOpen { get; }

    /// <summary>Placeholder text when nothing is selected.</summary>
    public Signal<string> Placeholder { get; }

    /// <summary>Whether the dropdown is enabled.</summary>
    public Signal<bool> IsEnabled { get; }

    /// <summary>Two-way bind the selected value to a signal (single-select).</summary>
    public Dropdown<T> BindSync(Signal<T> signal);
}
```

#### IconButton, ToggleButton, Label, Image, Divider, Spacer

```csharp
/// <summary>
/// An icon-only square button with optional badge and tooltip.
/// </summary>
/// <remarks>USS classes: <c>jui-icon-btn</c>, <c>jui-icon-btn--{size}</c>.</remarks>
public sealed class IconButton : Component
{
    public IconButton(Texture2D icon, WidgetSize size = WidgetSize.Medium);
    public Signal<Texture2D> Icon { get; }
    public Signal<bool> IsEnabled { get; }
    public Signal<int> BadgeCount { get; }
    public string TooltipText { get; set; }
    public IconButton OnClick<TState>(Action<IconButton, TState> handler, TState state);
}

/// <summary>
/// A button that toggles between on and off states with visual feedback.
/// </summary>
/// <remarks>USS classes: <c>jui-toggle-btn</c>, <c>jui-toggle-btn--active</c>.</remarks>
public sealed class ToggleButton : Component
{
    public ToggleButton(string text, Texture2D icon = null);
    public Signal<bool> IsActive { get; }
    public Signal<string> Text { get; }
    public ToggleButton BindSync(Signal<bool> signal);
}

/// <summary>
/// A reactive text label. Binds to a signal for automatic text updates.
/// </summary>
/// <remarks>USS classes: <c>jui-label</c>, <c>jui-label--{variant}</c>.</remarks>
public sealed class Label : Component
{
    public Label(string text = "");
    public Signal<string> Text { get; }
    public Label Bind(IReadOnlySignal<string> signal);
}

/// <summary>
/// A reactive image element that displays a texture or sprite.
/// </summary>
/// <remarks>USS classes: <c>jui-image</c>.</remarks>
public sealed class Image : Component
{
    public Image(Texture2D texture = null);
    public Signal<Texture2D> Source { get; }
    public ScaleMode ScaleMode { get; set; }
}

/// <summary>A horizontal or vertical divider line.</summary>
/// <remarks>USS classes: <c>jui-divider</c>, <c>jui-divider--vertical</c>.</remarks>
public sealed class Divider : Component
{
    public Divider(bool vertical = false);
}

/// <summary>A flexible spacer that fills available space in a flex container.</summary>
/// <remarks>USS classes: <c>jui-spacer</c>.</remarks>
public sealed class Spacer : Component
{
    /// <summary>Create a spacer with optional fixed size. 0 = flex-grow.</summary>
    public Spacer(float size = 0f);
}
```

### Layout Containers

```csharp
/// <summary>
/// A vertical stack container. Children are arranged top to bottom.
/// Shorthand for a flex column with gap.
/// </summary>
/// <remarks>USS classes: <c>jui-stack</c>.</remarks>
public sealed class Stack : Component
{
    /// <summary>Create a stack with the given gap between children.</summary>
    /// <param name="gap">Gap in pixels between children. Default: 8.</param>
    public Stack(float gap = 8f);

    /// <summary>Horizontal alignment of children within the stack.</summary>
    public Align AlignItems { get; set; }
}

/// <summary>
/// A horizontal row container. Children are arranged left to right.
/// </summary>
/// <remarks>USS classes: <c>jui-row</c>.</remarks>
public sealed class Row : Component
{
    public Row(float gap = 8f);
    public Align AlignItems { get; set; }
    public Justify JustifyContent { get; set; }
}

/// <summary>
/// A vertical column container. Alias for Stack with additional justify support.
/// </summary>
/// <remarks>USS classes: <c>jui-column</c>.</remarks>
public sealed class Column : Component
{
    public Column(float gap = 8f);
    public Align AlignItems { get; set; }
    public Justify JustifyContent { get; set; }
}

/// <summary>
/// A CSS Grid-like container with template columns and rows, gap, and cell spanning.
/// </summary>
/// <remarks>USS classes: <c>jui-grid</c>.</remarks>
public sealed class Grid : Component
{
    /// <summary>Create a grid with the given column template.</summary>
    /// <param name="columns">
    /// Column template (e.g., "1fr 2fr 1fr", "repeat(3, 1fr)", "200px auto 100px").
    /// </param>
    /// <param name="rows">Optional row template. Defaults to auto-rows.</param>
    /// <param name="gap">Gap between cells in pixels.</param>
    public Grid(string columns, string rows = null, float gap = 8f);
}

/// <summary>
/// A flex-wrap container. Children wrap to the next line when the container is full.
/// </summary>
/// <remarks>USS classes: <c>jui-wrap</c>.</remarks>
public sealed class Wrap : Component
{
    public Wrap(float gap = 8f);
}

/// <summary>
/// Centers its single child both horizontally and vertically.
/// </summary>
/// <remarks>USS classes: <c>jui-center</c>.</remarks>
public sealed class Center : Component
{
    public Center();
}

/// <summary>
/// A scrollable container with momentum scrolling, snap points, and pull-to-refresh.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-scroll</c>, <c>jui-scroll--horizontal</c>,
/// <c>jui-scroll__content</c>, <c>jui-scroll__bar</c>.
/// </remarks>
public sealed class ScrollView : Component
{
    public ScrollView(bool horizontal = false);

    /// <summary>Current scroll position (0..1). Reactive and settable.</summary>
    public Signal<float> ScrollPosition { get; }

    /// <summary>Enable snap-to-child scrolling with the given snap alignment.</summary>
    public SnapAlignment Snap { get; set; }

    /// <summary>Enable momentum/inertia scrolling.</summary>
    public bool Momentum { get; set; }

    /// <summary>
    /// Enable pull-to-refresh. When the user pulls past the top, the callback is invoked.
    /// </summary>
    public Action OnPullToRefresh { get; set; }
}

/// <summary>
/// An accordion container with expandable/collapsible sections.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-accordion</c>, <c>jui-accordion__section</c>,
/// <c>jui-accordion__header</c>, <c>jui-accordion__content</c>,
/// <c>jui-accordion__section--expanded</c>.
/// </remarks>
public sealed class Accordion : Component
{
    /// <summary>Create an accordion. If allowMultiple is false, only one section can be expanded at a time.</summary>
    public Accordion(bool allowMultiple = false);

    /// <summary>Add a collapsible section with a header label and content builder.</summary>
    public Accordion Section(string header, Func<Element> content, bool defaultExpanded = false);
}

/// <summary>
/// A tabbed container with horizontal or vertical tab alignment, lazy-loaded tab
/// content, badge support on tab headers, and keyboard navigation.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-tabs</c>, <c>jui-tabs--vertical</c>, <c>jui-tabs__header</c>,
/// <c>jui-tabs__tab</c>, <c>jui-tabs__tab--active</c>, <c>jui-tabs__content</c>,
/// <c>jui-tabs__badge</c>.
/// </remarks>
public sealed class TabView : Component
{
    /// <summary>Create a tab view.</summary>
    /// <param name="vertical">If true, tabs are arranged vertically on the left.</param>
    public TabView(bool vertical = false);

    /// <summary>The index of the currently active tab. Reactive.</summary>
    public Signal<int> ActiveTab { get; }

    /// <summary>Add a tab with a label, content builder, and optional badge.</summary>
    /// <param name="label">Tab header text.</param>
    /// <param name="content">
    /// Content builder. Called lazily on first tab activation if <paramref name="lazy"/> is true.
    /// </param>
    /// <param name="badge">Optional badge signal (e.g., unread count). Null = no badge.</param>
    /// <param name="lazy">If true, content is not built until the tab is first selected.</param>
    /// <returns>This TabView for fluent chaining.</returns>
    public TabView Tab(string label, Func<Element> content,
                       IReadOnlySignal<int> badge = null, bool lazy = true);
}

/// <summary>
/// A breadcrumb navigation trail showing the current location in a hierarchy.
/// </summary>
/// <remarks>USS classes: <c>jui-breadcrumb</c>, <c>jui-breadcrumb__item</c>, <c>jui-breadcrumb__separator</c>.</remarks>
public sealed class Breadcrumb : Component
{
    public Breadcrumb();
    public Breadcrumb Item(string label, Action onClick = null);
    public string Separator { get; set; }
}

/// <summary>
/// A multi-step progress indicator with numbered/labeled steps and active state.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-stepper</c>, <c>jui-stepper__step</c>,
/// <c>jui-stepper__step--active</c>, <c>jui-stepper__step--completed</c>,
/// <c>jui-stepper__connector</c>.
/// </remarks>
public sealed class Stepper : Component
{
    public Stepper();
    public Signal<int> ActiveStep { get; }
    public Stepper Step(string label, string description = null);
}

/// <summary>
/// A draggable splitter that divides two panels with a resizable handle.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-splitter</c>, <c>jui-splitter--vertical</c>,
/// <c>jui-splitter__panel</c>, <c>jui-splitter__handle</c>.
/// </remarks>
public sealed class Splitter : Component
{
    /// <summary>Create a splitter dividing two panels.</summary>
    /// <param name="vertical">If true, splits top/bottom instead of left/right.</param>
    /// <param name="initialRatio">Initial size ratio of the first panel (0..1). Default: 0.5.</param>
    public Splitter(bool vertical = false, float initialRatio = 0.5f);

    /// <summary>The current split ratio. Reactive and settable.</summary>
    public Signal<float> Ratio { get; }

    /// <summary>Minimum size of either panel in pixels.</summary>
    public float MinSize { get; set; }

    /// <summary>Set the content of the first (left/top) panel.</summary>
    public Splitter First(Func<Element> content);

    /// <summary>Set the content of the second (right/bottom) panel.</summary>
    public Splitter Second(Func<Element> content);
}
```

### Overlay Components

```csharp
/// <summary>
/// A modal dialog with backdrop blur, focus trap, Escape-to-close, and scoped DI.
/// Renders above all other content via a portal to the overlay layer.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-modal</c>, <c>jui-modal__backdrop</c>, <c>jui-modal__content</c>,
/// <c>jui-modal__header</c>, <c>jui-modal__body</c>, <c>jui-modal__footer</c>.
/// </remarks>
public sealed class Modal : Component
{
    /// <summary>Create a modal dialog.</summary>
    /// <param name="title">Header title text.</param>
    public Modal(string title = null);

    /// <summary>Whether the modal is currently visible. Set to true to open.</summary>
    public Signal<bool> IsOpen { get; }

    /// <summary>Whether clicking the backdrop closes the modal. Default: true.</summary>
    public bool CloseOnBackdropClick { get; set; }

    /// <summary>Whether pressing Escape closes the modal. Default: true.</summary>
    public bool CloseOnEscape { get; set; }

    /// <summary>Set the body content builder.</summary>
    public Modal Body(Func<Element> content);

    /// <summary>Set the footer content builder (typically action buttons).</summary>
    public Modal Footer(Func<Element> content);

    /// <summary>
    /// Open the modal as a UniTask that completes when the modal closes.
    /// Returns the result set via <see cref="Close{T}(T)"/>.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <returns>The modal result.</returns>
    public UniTask<T> ShowAsync<T>();

    /// <summary>Close the modal with an optional result value.</summary>
    public void Close<T>(T result = default);
}

/// <summary>
/// A toast notification with auto-dismiss, severity levels, queue management,
/// and configurable position.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-toast</c>, <c>jui-toast--info</c>, <c>jui-toast--success</c>,
/// <c>jui-toast--warning</c>, <c>jui-toast--error</c>.
/// </remarks>
public static class Toast
{
    public enum Severity { Info, Success, Warning, Error }
    public enum Position { TopRight, TopLeft, TopCenter, BottomRight, BottomLeft, BottomCenter }

    /// <summary>Show a toast notification.</summary>
    /// <param name="message">The message text.</param>
    /// <param name="severity">Visual severity level.</param>
    /// <param name="duration">Auto-dismiss duration. Zero = manual dismiss only.</param>
    /// <param name="position">Screen position. Default: TopRight.</param>
    public static void Show(string message, Severity severity = Severity.Info,
                            TimeSpan duration = default, Position position = Position.TopRight);

    /// <summary>Maximum number of toasts visible simultaneously. Default: 5.</summary>
    public static int MaxVisible { get; set; }

    /// <summary>Dismiss all visible toasts.</summary>
    public static void DismissAll();
}

/// <summary>
/// A tooltip that appears on hover (desktop) or long-press (touch) with configurable
/// delay and auto-positioning to avoid viewport overflow.
/// </summary>
/// <remarks>USS classes: <c>jui-tooltip</c>, <c>jui-tooltip--{position}</c>.</remarks>
public sealed class Tooltip : Component
{
    /// <summary>Create a tooltip wrapping a target element.</summary>
    /// <param name="text">Tooltip text.</param>
    /// <param name="delay">Delay before showing. Default: 500ms.</param>
    public Tooltip(string text, TimeSpan delay = default);

    /// <summary>Tooltip text. Reactive.</summary>
    public Signal<string> Text { get; }

    /// <summary>Preferred position. Auto-flips if it would overflow the viewport.</summary>
    public TooltipPosition Position { get; set; }
}

/// <summary>
/// A context menu with nested submenus, keyboard navigation, and auto-flip
/// positioning. Opens on right-click (desktop) or long-press (touch).
/// </summary>
/// <remarks>
/// USS classes: <c>jui-context-menu</c>, <c>jui-context-menu__item</c>,
/// <c>jui-context-menu__separator</c>, <c>jui-context-menu__submenu</c>.
/// </remarks>
public sealed class ContextMenu : Component
{
    public ContextMenu();
    public ContextMenu Item(string label, Action onClick, Texture2D icon = null, string shortcut = null);
    public ContextMenu Separator();
    public ContextMenu SubMenu(string label, Action<ContextMenu> builder);

    /// <summary>Show the context menu at the given screen position.</summary>
    public void ShowAt(Vector2 position);

    /// <summary>Dismiss the context menu.</summary>
    public void Dismiss();
}

/// <summary>
/// A popover that anchors to a target element and displays rich content.
/// </summary>
/// <remarks>USS classes: <c>jui-popover</c>, <c>jui-popover__arrow</c>.</remarks>
public sealed class Popover : Component
{
    public Popover(Func<Element> content);
    public Signal<bool> IsOpen { get; }
    public PopoverPosition Position { get; set; }
    public bool DismissOnClickOutside { get; set; }
}

/// <summary>
/// A side drawer that slides in from an edge of the screen.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-drawer</c>, <c>jui-drawer--left</c>, <c>jui-drawer--right</c>,
/// <c>jui-drawer--top</c>, <c>jui-drawer--bottom</c>, <c>jui-drawer__backdrop</c>.
/// </remarks>
public sealed class Drawer : Component
{
    public Drawer(DrawerEdge edge = DrawerEdge.Left, float width = 300f);
    public Signal<bool> IsOpen { get; }
    public Drawer Content(Func<Element> content);
}
```

### Feedback Indicators

```csharp
/// <summary>
/// A progress bar with determinate and indeterminate modes, animated fill,
/// and optional label.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-progress</c>, <c>jui-progress--indeterminate</c>,
/// <c>jui-progress__track</c>, <c>jui-progress__fill</c>, <c>jui-progress__label</c>.
/// </remarks>
public sealed class ProgressBar : Component
{
    /// <summary>Create a progress bar.</summary>
    /// <param name="indeterminate">If true, shows an animated indeterminate state.</param>
    public ProgressBar(bool indeterminate = false);

    /// <summary>Current progress value (0..1). Animated fill transition on change.</summary>
    public Signal<float> Progress { get; }

    /// <summary>Optional label format. Use "{0:P0}" for percentage display.</summary>
    public string LabelFormat { get; set; }
}

/// <summary>
/// A shimmer loading placeholder that mimics the shape of content being loaded.
/// </summary>
/// <remarks>USS classes: <c>jui-skeleton</c>, <c>jui-skeleton--text</c>,
/// <c>jui-skeleton--circle</c>, <c>jui-skeleton--rect</c>.</remarks>
public sealed class Skeleton : Component
{
    public enum Shape { Text, Circle, Rectangle }
    public Skeleton(Shape shape = Shape.Rectangle, float width = 200f, float height = 20f);
}

/// <summary>A spinning loading indicator.</summary>
/// <remarks>USS classes: <c>jui-spinner</c>, <c>jui-spinner--{size}</c>.</remarks>
public sealed class Spinner : Component
{
    public Spinner(WidgetSize size = WidgetSize.Medium);
}

/// <summary>
/// A small count or status indicator, typically overlaid on another element.
/// </summary>
/// <remarks>USS classes: <c>jui-badge</c>, <c>jui-badge--dot</c>.</remarks>
public sealed class Badge : Component
{
    public Badge(IReadOnlySignal<int> count = null);
    public Signal<int> Count { get; }
    public int MaxCount { get; set; }
    public bool Dot { get; set; }
}

/// <summary>
/// A compact interactive element for filtering, tags, or actions. Supports
/// delete action and selected state.
/// </summary>
/// <remarks>USS classes: <c>jui-chip</c>, <c>jui-chip--selected</c>, <c>jui-chip--deletable</c>.</remarks>
public sealed class Chip : Component
{
    public Chip(string label, bool deletable = false);
    public Signal<string> Label { get; }
    public Signal<bool> IsSelected { get; }
    public Action OnDelete { get; set; }
}

/// <summary>
/// A circular avatar element showing an image, initials, or icon.
/// </summary>
/// <remarks>USS classes: <c>jui-avatar</c>, <c>jui-avatar--{size}</c>.</remarks>
public sealed class Avatar : Component
{
    public Avatar(Texture2D image = null, string initials = null, WidgetSize size = WidgetSize.Medium);
    public Signal<Texture2D> Image { get; }
    public Signal<string> Initials { get; }
}

/// <summary>
/// A colored label tag for categorization.
/// </summary>
/// <remarks>USS classes: <c>jui-tag</c>, <c>jui-tag--{color}</c>.</remarks>
public sealed class Tag : Component
{
    public Tag(string text, TagColor color = TagColor.Default);
    public Signal<string> Text { get; }
}
```

### Data Display

```csharp
/// <summary>
/// A hierarchical tree view with expand/collapse, selection, drag-drop reordering,
/// and virtualized rendering.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-tree</c>, <c>jui-tree__node</c>, <c>jui-tree__node--expanded</c>,
/// <c>jui-tree__node--selected</c>, <c>jui-tree__indent</c>, <c>jui-tree__toggle</c>.
/// </remarks>
/// <typeparam name="T">The data type of each tree node.</typeparam>
public sealed class TreeView<T> : Component
{
    /// <summary>Create a tree view from a reactive list of root nodes.</summary>
    /// <param name="roots">The root-level tree data.</param>
    /// <param name="childrenSelector">Function to get children of a node.</param>
    /// <param name="renderer">Function to render a single node.</param>
    public TreeView(
        ReactiveList<T> roots,
        Func<T, IReadOnlyList<T>> childrenSelector,
        Func<T, Element> renderer);

    /// <summary>The currently selected node. Null if none selected.</summary>
    public Signal<T> SelectedNode { get; }

    /// <summary>Whether drag-drop reordering is enabled.</summary>
    public bool AllowDragDrop { get; set; }
}

/// <summary>
/// A data table with sortable columns, row selection, virtualized scrolling,
/// and configurable cell renderers.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-table</c>, <c>jui-table__header</c>, <c>jui-table__row</c>,
/// <c>jui-table__row--selected</c>, <c>jui-table__cell</c>, <c>jui-table__sort-icon</c>.
/// </remarks>
/// <typeparam name="T">The row data type.</typeparam>
public sealed class Table<T> : Component
{
    /// <summary>Create a table with the given data source.</summary>
    /// <param name="data">The reactive list of row data.</param>
    public Table(ReactiveList<T> data);

    /// <summary>Add a column definition.</summary>
    /// <param name="header">Column header text.</param>
    /// <param name="cellRenderer">Function to render a cell from row data.</param>
    /// <param name="sortKey">Optional sort key selector for sortable columns.</param>
    /// <param name="width">Column width (e.g., "200px", "1fr", "auto").</param>
    /// <returns>This table for fluent chaining.</returns>
    public Table<T> Column<TKey>(string header, Func<T, Element> cellRenderer,
                                  Func<T, TKey> sortKey = null, string width = "1fr")
        where TKey : IComparable<TKey>;

    /// <summary>The currently selected rows.</summary>
    public ReactiveList<T> SelectedRows { get; }

    /// <summary>Whether multi-row selection is enabled.</summary>
    public bool MultiSelect { get; set; }
}

/// <summary>
/// A month-view calendar with selectable dates, range selection, and
/// min/max date constraints.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-calendar</c>, <c>jui-calendar__header</c>,
/// <c>jui-calendar__day</c>, <c>jui-calendar__day--selected</c>,
/// <c>jui-calendar__day--today</c>, <c>jui-calendar__day--disabled</c>.
/// </remarks>
public sealed class Calendar : Component
{
    public Calendar();
    public Signal<DateTime> SelectedDate { get; }
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
    public bool RangeSelect { get; set; }
    public Signal<DateTime> RangeStart { get; }
    public Signal<DateTime> RangeEnd { get; }
}

/// <summary>
/// A pagination control with page numbers, previous/next buttons, and
/// items-per-page selector.
/// </summary>
/// <remarks>
/// USS classes: <c>jui-pagination</c>, <c>jui-pagination__btn</c>,
/// <c>jui-pagination__page</c>, <c>jui-pagination__page--active</c>.
/// </remarks>
public sealed class Pagination : Component
{
    public Pagination(int totalItems, int itemsPerPage = 20);
    public Signal<int> CurrentPage { get; }
    public Signal<int> TotalItems { get; }
    public Signal<int> ItemsPerPage { get; }
    public IReadOnlySignal<int> TotalPages { get; }
}
```

## Data Structures

| Type | Role |
|------|------|
| `ButtonVariant` enum | Controls USS class and color token selection for Button. |
| `WidgetSize` enum | Size preset (`Small`, `Medium`, `Large`) mapped to USS modifiers and design token scales. |
| `SnapAlignment` enum | ScrollView snap alignment (`Start`, `Center`, `End`). |
| `TooltipPosition` enum | Preferred tooltip placement (`Top`, `Bottom`, `Left`, `Right`). Auto-flips on overflow. |
| `PopoverPosition` enum | Preferred popover anchor position. |
| `DrawerEdge` enum | Edge from which the Drawer slides in (`Left`, `Right`, `Top`, `Bottom`). |
| `TagColor` enum | Predefined tag color presets (`Default`, `Red`, `Blue`, `Green`, `Yellow`, `Purple`). |
| `Skeleton.Shape` enum | Shape of the skeleton placeholder (`Text`, `Circle`, `Rectangle`). |
| `Toast.Severity` enum | Toast notification severity (`Info`, `Success`, `Warning`, `Error`). |
| `Toast.Position` enum | Screen position for toast notifications. |

## Implementation Notes

### Base Controls

- **Ripple effect**: `Button` creates a child `VisualElement` on click that expands radially from the click point with opacity fade. The ripple element is pooled and reused across clicks.
- **Variant/Size USS application**: Effects watch `Variant` and `Size` signals and toggle USS classes accordingly. When `Variant` changes from `Primary` to `Ghost`, the effect removes `jui-btn--primary` and adds `jui-btn--ghost`.
- **Loading state**: When `IsLoading` is true, the button label is hidden (opacity 0) and a `Spinner` is shown in its place. The button is also disabled to prevent double-clicks.
- **Dropdown virtualization**: When the option list exceeds 100 items, the dropdown panel uses a `VirtualList` (Section 18) for the options. Below 100 items, standard `For` rendering is used. The threshold is configurable.
- **Dropdown keyboard/gamepad**: Arrow keys navigate options, Enter selects, Escape closes. Gamepad D-pad maps to arrow keys. Focus is trapped within the open panel.
- **TextInput character counter**: When `MaxLength > 0`, a counter label (`{current}/{max}`) is shown below the input. The counter turns red when `current >= max`.

### Layout

- **Stack/Row/Column USS**: These map directly to USS flex properties. `Stack` sets `flex-direction: column`. `Row` sets `flex-direction: row`. `Column` is an alias for `Stack` with additional `justify-content` support.
- **Grid template parsing**: The `Grid` constructor accepts a CSS Grid template string and applies it via USS `grid-template-columns` and `grid-template-rows`. Unity's USS supports a subset of CSS Grid.
- **ScrollView momentum**: Implemented via a per-frame effect that applies velocity decay to the scroll position after the user releases touch/drag. Uses `Time.unscaledDeltaTime` for frame-independent physics.
- **TabView lazy loading**: Tab content builders are stored as `Func<Element>` and invoked only when the tab is first activated. Once built, the content is cached and reused on subsequent tab switches.
- **Accordion exclusive mode**: When `allowMultiple = false`, expanding one section collapses all others. This is managed by a shared `Signal<int>` tracking the expanded section index.

### Overlay

- **Modal focus trap**: When open, Tab and Shift+Tab cycle focus within the modal content. Focus is moved to the first focusable element on open and restored to the previously focused element on close.
- **Modal backdrop blur**: Uses USS `backdrop-filter: blur(4px)` where supported. Falls back to semi-transparent black overlay where not supported.
- **Modal scoped DI**: Each Modal creates a child DI scope. Providers added within the modal body are scoped to the modal's lifetime and disposed when the modal closes.
- **Toast queue**: When more toasts are shown than `MaxVisible`, excess toasts are queued and shown as existing toasts are dismissed. Toasts animate in with a slide-down and out with a slide-up + fade.
- **Tooltip auto-position**: On show, the tooltip measures its own size against the viewport bounds. If the preferred position would cause overflow, it flips to the opposite side. If all four sides overflow, it falls back to center-above.
- **ContextMenu auto-flip**: Submenus open to the right by default. If the submenu would overflow the viewport horizontally, it flips to open left of the parent item.
- **Drawer backdrop**: Clicking the backdrop closes the drawer. The backdrop animates opacity in sync with the drawer slide animation.

### Feedback

- **ProgressBar animated fill**: The fill width transitions smoothly using USS `transition: width 300ms ease-out`. The `Progress` signal drives the width percentage.
- **Skeleton shimmer**: A gradient mask animates left-to-right across the skeleton shape using USS `@keyframes` and `animation`. The shimmer speed is a design token.
- **Badge overflow**: When `Count > MaxCount`, the badge displays `"{MaxCount}+"` (e.g., "99+"). When `Dot = true`, only a dot indicator is shown regardless of count.

### Data Display

- **TreeView virtualization**: Uses the virtualization system (Section 18) for large trees. Only visible nodes are rendered. Expand/collapse updates the virtual item list.
- **Table sort**: Clicking a sortable column header toggles ascending/descending sort. The sort is applied to the `ReactiveList` via `Sort()` and the sorted state is shown with a sort icon in the header.
- **Calendar date generation**: Each month view generates a 6x7 grid of day cells. Days outside the current month are shown dimmed. The current day is highlighted. Disabled dates (outside min/max) are non-interactive.
- **Pagination page numbers**: Shows a window of page numbers around the current page with ellipsis for large page counts. The window size is configurable.

## Source Generator Notes

N/A for this section. All widgets are runtime components, not generated. Widget construction is done via the `El` helper factory methods (e.g., `El.Button(...)`, `El.TextInput(...)`) which are hand-written factory methods, not source-generated.

## Usage Examples

```csharp
// Complete form example combining multiple widgets
public partial class UserProfileEditor : Component
{
    public Signal<string> Name { get; } = new("");
    public Signal<string> Bio { get; } = new("");
    public Signal<float> Volume { get; } = new(0.8f);
    public Signal<bool> Notifications { get; } = new(true);
    public Signal<bool> IsLoading { get; } = new(false);

    protected override Element Render() => El.Create<Stack>(gap: 16,
        El.Create<TextInput>(label: "Name", placeholder: "Enter your name")
            .BindSync(Name),

        El.Create<TextInput>(label: "Bio", placeholder: "Tell us about yourself"),

        El.Create<Row>(gap: 12,
            El.Create<Label>("Volume"),
            El.Create<Slider>(min: 0, max: 1, step: 0.1f)
                .BindSync(Volume),
            El.Create<Label>().Bind(Volume.Select(static v => $"{v:P0}"))
        ),

        El.Create<Toggle>(label: "Enable Notifications")
            .BindSync(Notifications),

        El.Create<Divider>(),

        El.Create<Row>(gap: 8,
            El.Create<Spacer>(),
            El.Create<Button>("Cancel", ButtonVariant.Ghost)
                .OnClick(static (_, self) => self.Close(), this),
            El.Create<Button>("Save", ButtonVariant.Primary)
                .OnClick(static (_, self) => self.Save(), this)
        )
    );
}

// Tab view with badges
public partial class MainPanel : Component
{
    private readonly Signal<int> _unreadMessages = new(3);

    protected override Element Render() => El.Create<TabView>(
        tab => tab
            .Tab("Dashboard", () => El.Create<DashboardView>())
            .Tab("Messages", () => El.Create<MessagesView>(), badge: _unreadMessages)
            .Tab("Settings", () => El.Create<SettingsView>(), lazy: true)
    );
}

// Modal with async result
public async UniTask<bool> ConfirmDelete(string itemName)
{
    var modal = new Modal("Confirm Delete");
    modal.Body(() => El.Create<Label>($"Are you sure you want to delete '{itemName}'?"));
    modal.Footer(() => El.Create<Row>(gap: 8,
        El.Create<Spacer>(),
        El.Create<Button>("Cancel", ButtonVariant.Ghost)
            .OnClick(static (_, m) => m.Close(false), modal),
        El.Create<Button>("Delete", ButtonVariant.Danger)
            .OnClick(static (_, m) => m.Close(true), modal)
    ));
    return await modal.ShowAsync<bool>();
}

// Toast notifications
Toast.Show("Settings saved!", Toast.Severity.Success, TimeSpan.FromSeconds(3));
Toast.Show("Connection lost.", Toast.Severity.Error);

// Context menu
var menu = new ContextMenu();
menu.Item("Cut", OnCut, icon: cutIcon, shortcut: "Ctrl+X");
menu.Item("Copy", OnCopy, icon: copyIcon, shortcut: "Ctrl+C");
menu.Item("Paste", OnPaste, icon: pasteIcon, shortcut: "Ctrl+V");
menu.Separator();
menu.SubMenu("Transform", sub =>
{
    sub.Item("Rotate 90", OnRotate90);
    sub.Item("Flip Horizontal", OnFlipH);
    sub.Item("Flip Vertical", OnFlipV);
});
menu.ShowAt(mousePosition);

// Data table
public partial class PlayerList : Component
{
    private readonly ReactiveList<PlayerData> _players = new();

    protected override Element Render() => El.Create<Table<PlayerData>>(_players)
        .Column("Name", static p => El.Create<Label>(p.Name), sortKey: static p => p.Name, width: "2fr")
        .Column("Level", static p => El.Create<Label>($"{p.Level}"), sortKey: static p => p.Level, width: "1fr")
        .Column("Score", static p => El.Create<Label>($"{p.Score:N0}"), sortKey: static p => p.Score, width: "1fr")
        .Column("Actions", static p => El.Create<Row>(gap: 4,
            El.Create<IconButton>(editIcon).OnClick(static (_, d) => EditPlayer(d), p),
            El.Create<IconButton>(deleteIcon).OnClick(static (_, d) => DeletePlayer(d), p)
        ), width: "auto");
}

// Splitter with resizable panels
El.Create<Splitter>(vertical: false, initialRatio: 0.3f)
    .First(() => El.Create<TreeView<FileNode>>(fileTree, n => n.Children, RenderNode))
    .Second(() => El.Create<CodeEditor>());
```

## Test Plan

### Base Controls

1. **Button click handler fires**: Create a Button, simulate click, verify handler invoked.
2. **Button disabled state prevents click**: Set `IsEnabled` to false, simulate click, verify handler NOT invoked.
3. **Button loading state shows spinner**: Set `IsLoading` to true, verify spinner child is visible and button is disabled.
4. **Button variant USS class switching**: Change `Variant` from Primary to Danger, verify `jui-btn--primary` removed and `jui-btn--danger` added.
5. **TextInput two-way binding**: Type text, verify `Value` signal updated. Set `Value` signal, verify displayed text updated.
6. **TextInput validation error display**: Bind a `ValidationResult.Error`, verify error message visible and `jui-input--error` class present.
7. **TextInput character counter**: Set `MaxLength = 10`, type 8 characters, verify counter shows "8/10".
8. **Slider value clamping**: Set value below min or above max, verify clamped to range.
9. **Slider step snapping**: Set step to 0.25, drag to 0.3, verify value snaps to 0.25.
10. **Toggle bind sync**: Toggle the switch, verify bound signal changes. Change signal, verify switch visual updates.
11. **Dropdown search filtering**: Open dropdown with 100 items, type search text, verify visible options filtered.
12. **Dropdown multi-select**: Enable multi-select, select 3 items, verify `SelectedValues` contains all 3.
13. **Dropdown keyboard navigation**: Open dropdown, press ArrowDown, press Enter, verify item selected.

### Layout

14. **Stack arranges children vertically**: Add 3 children, verify they render top-to-bottom with gap.
15. **Row arranges children horizontally**: Add 3 children, verify they render left-to-right with gap.
16. **Grid respects column template**: Set "1fr 2fr", add children, verify column proportions.
17. **ScrollView scroll position signal**: Scroll programmatically, verify `ScrollPosition` signal updates.
18. **Accordion exclusive mode**: Expand section A, expand section B, verify section A collapsed.
19. **TabView lazy loading**: Create TabView with 3 tabs, verify only active tab's content is built.
20. **TabView badge display**: Bind badge signal to a tab, verify badge element shows count.
21. **Splitter drag updates ratio**: Simulate drag on handle, verify `Ratio` signal changes.

### Overlay

22. **Modal opens and closes**: Set `IsOpen` to true, verify visible. Set to false, verify hidden.
23. **Modal focus trap**: Open modal, press Tab repeatedly, verify focus cycles within modal.
24. **Modal Escape to close**: Open modal with `CloseOnEscape = true`, press Escape, verify modal closes.
25. **Modal async result**: Call `ShowAsync`, close with `Close(42)`, verify returned value is 42.
26. **Toast auto-dismiss**: Show toast with 1s duration, wait 1s, verify toast dismissed.
27. **Toast queue management**: Show 6 toasts with `MaxVisible = 5`, verify 5 visible, 1 queued.
28. **Tooltip delay**: Hover over target, verify tooltip appears after delay.
29. **Tooltip auto-position**: Position tooltip near viewport edge, verify it flips to avoid overflow.
30. **ContextMenu submenu**: Open context menu, hover over submenu item, verify submenu appears.
31. **Drawer slide animation**: Open drawer, verify slide-in animation. Close, verify slide-out.

### Feedback

32. **ProgressBar animated fill**: Set `Progress` from 0 to 0.5, verify fill width animates.
33. **ProgressBar indeterminate**: Create indeterminate progress bar, verify animation is running.
34. **Skeleton shimmer**: Create skeleton, verify shimmer animation USS class is present.
35. **Badge overflow display**: Set `Count = 150`, `MaxCount = 99`, verify displays "99+".
36. **Chip delete action**: Create deletable chip, click delete, verify `OnDelete` invoked.

### Data Display

37. **TreeView expand/collapse**: Click expand toggle, verify children visible. Click again, verify hidden.
38. **TreeView selection**: Click a node, verify `SelectedNode` signal updated.
39. **Table column sort**: Click sortable column header, verify rows reordered. Click again, verify reversed.
40. **Table row selection**: Click a row, verify it appears in `SelectedRows`.
41. **Calendar date selection**: Click a date, verify `SelectedDate` signal updated.
42. **Calendar disabled dates**: Set `MinDate`, verify dates before it are non-interactive.
43. **Pagination page change**: Click page 3, verify `CurrentPage.Value == 3`.
44. **Pagination total pages computed**: Set `TotalItems = 100`, `ItemsPerPage = 20`, verify `TotalPages.Value == 5`.

## Acceptance Criteria

### Base Controls

- [ ] `Button` supports `Primary`, `Secondary`, `Ghost`, `Danger` variants with correct USS classes
- [ ] `Button` supports `Small`, `Medium`, `Large` sizes with correct USS classes
- [ ] `Button` `IsLoading` state shows spinner and disables interaction
- [ ] `TextInput` supports two-way `BindSync` with validation error display
- [ ] `Slider` clamps to range and snaps to step
- [ ] `Toggle` two-way binds `Signal<bool>` with visual feedback
- [ ] `Dropdown<T>` supports search, multi-select, keyboard/gamepad navigation, and virtualization
- [ ] All base controls support `IsEnabled` signal for disabling

### Layout

- [ ] `Stack`, `Row`, `Column` render children in correct direction with gap
- [ ] `Grid` applies CSS Grid template columns and rows
- [ ] `ScrollView` supports momentum scroll, snap points, and `ScrollPosition` signal
- [ ] `TabView` supports lazy loading, badge signals, and keyboard tab switching
- [ ] `Accordion` exclusive mode collapses other sections on expand
- [ ] `Splitter` drag handle updates `Ratio` signal with min-size enforcement

### Overlay

- [ ] `Modal` supports backdrop click dismiss, Escape to close, focus trap, and `ShowAsync<T>` result
- [ ] `Toast` queue system respects `MaxVisible` and auto-dismiss duration
- [ ] `Tooltip` respects delay and auto-positions to avoid viewport overflow
- [ ] `ContextMenu` supports nested submenus, keyboard navigation, and auto-flip
- [ ] `Drawer` slides from configurable edge with backdrop dismiss

### Feedback

- [ ] `ProgressBar` supports determinate (0..1 signal) and indeterminate modes with animation
- [ ] `Skeleton` displays shimmer animation in text, circle, and rectangle shapes
- [ ] `Badge` displays count with `MaxCount` overflow formatting

### Data Display

- [ ] `TreeView<T>` renders hierarchical data with expand/collapse and selection
- [ ] `Table<T>` supports sortable columns, row selection, and virtualized scrolling
- [ ] `Calendar` supports date selection, range selection, and min/max constraints
- [ ] `Pagination` computes total pages and exposes reactive `CurrentPage` signal

### General

- [ ] All widgets follow USS class naming convention: `jui-{widget}`, `jui-{widget}--{variant}`, `jui-{widget}--{size}`
- [ ] All widgets expose signal-based state for reactive binding
- [ ] All public APIs have XML documentation
- [ ] All widgets support the fluent API chaining pattern
