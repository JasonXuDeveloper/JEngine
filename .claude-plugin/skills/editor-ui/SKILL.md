---
name: editor-ui
description: JEngine Editor UI component library with theming. Triggers on: custom inspector, editor window, Unity editor UI, UIElements, VisualElement, JButton, JStack, JCard, JTextField, JDropdown, design tokens, dark theme, light theme, editor styling, themed button, form layout, progress bar, status bar, toggle button, button group
---

# JEngine Editor UI Components

Modern UI component library for Unity Editor using UIElements with automatic dark/light theme support.

## When to Use
- Building custom inspectors
- Creating Editor windows
- Designing Editor tools with consistent styling

## Namespaces
```csharp
using JEngine.UI.Editor.Components.Button;
using JEngine.UI.Editor.Components.Layout;
using JEngine.UI.Editor.Components.Form;
using JEngine.UI.Editor.Components.Feedback;
using JEngine.UI.Editor.Components.Navigation;
using JEngine.UI.Editor.Theming;
```

## Button Components

### JButton - Themed Buttons
```csharp
// Variants: Primary, Secondary, Success, Danger, Warning
var btn = new JButton("Click Me", () => DoAction(), ButtonVariant.Primary);

// Fluent API
btn.SetVariant(ButtonVariant.Danger)
   .WithText("Delete")
   .WithEnabled(true)
   .FullWidth()
   .Compact()
   .WithMinWidth(100);
```

### JIconButton - Small Icon Buttons
```csharp
// For toolbars and inline actions
var iconBtn = new JIconButton("X", () => Close(), "Close panel")
    .WithSize(24, 24)
    .WithTooltip("Close");
```

### JToggleButton - Two-State Toggle
```csharp
var toggle = new JToggleButton(
    onText: "Enabled",
    offText: "Disabled",
    initialValue: false,
    onVariant: ButtonVariant.Success,
    offVariant: ButtonVariant.Danger,
    onValueChanged: value => Debug.Log($"Now: {value}"));

// Access value
toggle.Value = true;
toggle.SetValue(false, notify: false);
```

### JButtonGroup - Responsive Button Row
```csharp
var group = new JButtonGroup(
    new JButton("Save", Save, ButtonVariant.Primary),
    new JButton("Cancel", Cancel, ButtonVariant.Secondary))
    .NoWrap()
    .FixedWidth();
```

## Layout Components

### JStack - Vertical Layout
```csharp
// Gap sizes: Xs (2px), Sm (4px), MD (8px), Lg (12px), Xl (16px)
var stack = new JStack(GapSize.MD)
    .Add(new Label("Title"))
    .Add(new JButton("Action"))
    .WithGap(GapSize.Lg);
```

### JRow - Horizontal Layout
```csharp
var row = new JRow()
    .Add(new JButton("Left"))
    .Add(new JButton("Right"))
    .WithJustify(JustifyContent.SpaceBetween)  // Start, Center, End, SpaceBetween
    .WithAlign(AlignItems.Center)              // Start, Center, End, Stretch
    .NoWrap();
```

### JCard - Bordered Container
```csharp
var card = new JCard()
    .Add(new Label("Card Content"))
    .Compact()
    .NoMargin();
```

### JSection - Card with Header
```csharp
var section = new JSection("Settings")
    .Add(new JFormField("Name", new JTextField()))
    .Add(new JFormField("Enabled", new JToggle()))
    .WithTitle("New Title")
    .NoHeader()
    .NoMargin();

// Access header and content
section.Header.text = "Updated";
section.Content.Add(new Label("More content"));
```

## Form Components

### JTextField - Styled Text Input
```csharp
var field = new JTextField("initial value", "placeholder");
field.RegisterValueChangedCallback(evt => Debug.Log(evt.newValue));

// Fluent API
field.SetReadOnly(true)
     .SetMultiline(true);

// Access value
string text = field.Value;
field.Value = "new value";

// Bind to SerializedProperty
field.BindProperty(serializedProperty);
```

### JDropdown - Generic Dropdown
```csharp
// String dropdown
var stringDropdown = new JDropdown(
    new List<string> { "Option A", "Option B" },
    defaultValue: "Option A");

// Enum dropdown (recommended)
var enumDropdown = JDropdown<MyEnum>.ForEnum(MyEnum.Default);
enumDropdown.OnValueChanged(value => Debug.Log(value));

// Generic dropdown with custom formatting
var customDropdown = new JDropdown<MyClass>(
    items,
    defaultValue: items[0],
    formatSelectedValue: x => x.DisplayName,
    formatListItem: x => x.FullDescription);

// Access
enumDropdown.Value = MyEnum.Other;
enumDropdown.Choices = newList;
```

### JToggle - Toggle Switch
```csharp
var toggle = new JToggle(initialValue: false)
    .OnValueChanged(value => Debug.Log(value))
    .WithClass("my-toggle");

toggle.Value = true;
toggle.SetValueWithoutNotify(false);  // No callback
```

### JObjectField - Unity Object Picker
```csharp
var objectField = new JObjectField<Texture2D>(allowSceneObjects: false);
objectField.RegisterValueChangedCallback(evt =>
    Debug.Log($"Selected: {evt.newValue?.name}"));

// Access
Texture2D texture = objectField.Value;
objectField.BindProperty(serializedProperty);
```

### JFormField - Label + Control Layout
```csharp
var formField = new JFormField("Player Name", new JTextField())
    .WithLabelWidth(150)
    .NoLabel();

// Add multiple controls
formField.Add(new JButton("Browse"));
```

## Feedback Components

### JProgressBar - Progress Indicator
```csharp
var progress = new JProgressBar(initialProgress: 0f)
    .SetProgress(0.5f)
    .WithHeight(12)
    .WithColor(Color.green)
    .WithVariant(ButtonVariant.Success)
    .WithSuccessOnComplete();

progress.Progress = 0.75f;
```

### JStatusBar - Status Message with Accent
```csharp
// Status types: Info, Success, Warning, Error
var status = new JStatusBar("Ready", StatusType.Info)
    .SetStatus(StatusType.Success)
    .WithText("Operation complete!");

status.Text = "Processing...";
status.Status = StatusType.Warning;
```

### JLogView - Scrollable Log Output
```csharp
var logView = new JLogView(maxLines: 100)
    .LogInfo("Started processing")
    .LogError("Something went wrong")
    .Log("Custom message", isError: false)
    .WithMinHeight(150)
    .WithMaxHeight(400);

logView.Clear();
logView.MaxLines = 200;
```

## Navigation Components

### JBreadcrumb - Path Navigation
```csharp
// Quick creation
var breadcrumb = JBreadcrumb.FromPath("Package", "Scene", "Object");

// Manual building
var bc = new JBreadcrumb()
    .AddItem("Root")
    .AddItem("Child");
bc.Build();

bc.SetPath("New", "Path");
bc.Clear();
```

## Design Tokens

The `Tokens` class provides named constants that adapt to Unity's dark/light theme.

### Colors
```csharp
// Backgrounds (layered from deep to elevated)
Tokens.Colors.BgBase       // Deepest background
Tokens.Colors.BgSubtle     // Secondary containers
Tokens.Colors.BgSurface    // Cards, panels (most common)
Tokens.Colors.BgElevated   // Hover states, important elements
Tokens.Colors.BgOverlay    // Modals, tooltips
Tokens.Colors.BgHover      // Hover state
Tokens.Colors.BgInput      // Input field background

// Text hierarchy
Tokens.Colors.TextPrimary      // Highest contrast
Tokens.Colors.TextSecondary    // Body text
Tokens.Colors.TextMuted        // Helper text
Tokens.Colors.TextHeader       // Headers
Tokens.Colors.TextSectionHeader // Section titles

// Button colors
Tokens.Colors.Primary, PrimaryHover, PrimaryActive, PrimaryText
Tokens.Colors.Secondary, SecondaryHover, SecondaryActive, SecondaryText
Tokens.Colors.Success, SuccessHover, SuccessActive
Tokens.Colors.Danger, DangerHover, DangerActive
Tokens.Colors.Warning, WarningHover, WarningActive

// Borders
Tokens.Colors.Border, BorderFocus, BorderHover, BorderSubtle

// Status (aliases)
Tokens.Colors.StatusInfo, StatusSuccess, StatusWarning, StatusError

// Theme check
if (Tokens.IsDarkTheme) { }
```

### Spacing
```csharp
Tokens.Spacing.Xs   // 2px
Tokens.Spacing.Sm   // 4px
Tokens.Spacing.MD   // 8px
Tokens.Spacing.Lg   // 12px
Tokens.Spacing.Xl   // 16px
Tokens.Spacing.Xxl  // 24px
```

### Font Sizes
```csharp
Tokens.FontSize.Xs     // 10px - metadata
Tokens.FontSize.Sm     // 11px - hints
Tokens.FontSize.Base   // 12px - body (default)
Tokens.FontSize.MD     // 13px - emphasis
Tokens.FontSize.Lg     // 14px - section labels
Tokens.FontSize.Xl     // 16px - section headers
Tokens.FontSize.Title  // 18px - panel headers
```

### Border Radius
```csharp
Tokens.BorderRadius.Sm  // 3px
Tokens.BorderRadius.MD  // 5px
Tokens.BorderRadius.Lg  // 8px
```

### Layout Constants
```csharp
Tokens.Layout.FormLabelWidth     // 140px
Tokens.Layout.FormLabelMinWidth  // 60px
Tokens.Layout.MinTouchTarget     // 24px
Tokens.Layout.MinControlWidth    // 80px
```

### Transitions
```csharp
Tokens.Transition.Fast    // 150ms
Tokens.Transition.Normal  // 200ms
```

## JTheme Utilities

```csharp
// Apply common styles
JTheme.ApplyTransition(element);      // Smooth hover transitions
JTheme.ApplyPointerCursor(element);   // Hand cursor
JTheme.ApplyTextCursor(element);      // Text I-beam cursor
JTheme.ApplyGlassCard(element);       // Card styling

// Input field styles
JTheme.ApplyInputContainerStyle(element);
JTheme.ApplyInputElementStyle(element);
JTheme.ApplyInputTextStyle(element);
JTheme.ApplyInputHoverState(element);
JTheme.ApplyInputFocusState(element);
JTheme.ApplyInputNormalState(element);
JTheme.HideFieldLabel(field);

// Get button colors by variant
Color btnColor = JTheme.GetButtonColor(ButtonVariant.Primary);
Color hoverColor = JTheme.GetButtonHoverColor(ButtonVariant.Primary);
Color activeColor = JTheme.GetButtonActiveColor(ButtonVariant.Primary);
```

## Enums

```csharp
// Button styling
enum ButtonVariant { Primary, Secondary, Success, Danger, Warning }

// Layout gaps
enum GapSize { Xs, Sm, MD, Lg, Xl }

// Status indicators
enum StatusType { Info, Success, Warning, Error }

// Row alignment
enum JustifyContent { Start, Center, End, SpaceBetween }
enum AlignItems { Start, Center, End, Stretch }
```

## Example: Complete Editor Window

```csharp
public class MyEditorWindow : EditorWindow
{
    [MenuItem("Tools/My Window")]
    public static void ShowWindow() => GetWindow<MyEditorWindow>("My Window");

    public void CreateGUI()
    {
        var root = new JStack(GapSize.MD);
        root.style.paddingTop = Tokens.Spacing.Lg;
        root.style.paddingRight = Tokens.Spacing.Lg;
        root.style.paddingBottom = Tokens.Spacing.Lg;
        root.style.paddingLeft = Tokens.Spacing.Lg;

        // Breadcrumb navigation
        root.Add(JBreadcrumb.FromPath("Tools", "My Window"));

        // Settings section
        var settings = new JSection("Settings")
            .Add(new JFormField("Name", new JTextField()))
            .Add(new JFormField("Type", JDropdown<MyType>.ForEnum()))
            .Add(new JFormField("Enabled", new JToggle(true)));
        root.Add(settings);

        // Progress section
        var progress = new JProgressBar(0.3f).WithSuccessOnComplete();
        root.Add(new JFormField("Progress", progress));

        // Status
        root.Add(new JStatusBar("Ready", StatusType.Info));

        // Buttons
        root.Add(new JButtonGroup(
            new JButton("Apply", Apply, ButtonVariant.Primary),
            new JButton("Reset", Reset, ButtonVariant.Secondary)));

        // Log output
        var log = new JLogView(50).WithMaxHeight(200);
        root.Add(log);

        rootVisualElement.Add(root);
    }
}
```
