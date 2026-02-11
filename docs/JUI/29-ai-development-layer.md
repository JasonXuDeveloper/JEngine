# Section 29 — AI Development Layer

## Overview

The AI Development Layer makes JUI the most AI-friendly game UI framework by providing machine-readable metadata, scaffolding tools, and naming convention enforcement. It includes a component catalog (JSON), a layout pattern library (JSON), design token export (JSON), naming convention diagnostics, a JUIDL compiler (YAML to UXML+USS+CS), a scaffold generator, and a `.jui-rules` project context template.

The layer serves two audiences: (1) AI coding agents that generate or modify JUI code, and (2) human developers who want rapid scaffolding with compile-ready output. Every artifact is designed to be consumed programmatically -- structured JSON, deterministic YAML compilation, and diagnostic-enforced naming conventions that eliminate ambiguity.

## Dependencies

| Section | What It Provides |
|---------|-----------------|
| 9 — Source Generator Setup | Diagnostic infrastructure for naming convention enforcement (JUI diagnostic IDs) |
| 16 — Theming & Design Tokens | Token definitions exported to `jui-tokens.json` |
| 27 — Base Widgets | Component metadata for base widget catalog entries |
| 28 — Game Widgets | Component metadata for game-specific widget catalog entries |

## File Structure

```
Packages/com.jasonxudeveloper.jengine.ui/
├── Runtime/JUI/AI/
│   ├── jui-component-catalog.json       # Machine-readable catalog of all JUI components
│   ├── jui-layout-patterns.json         # Named layout patterns with templates
│   ├── jui-tokens.json                  # Design token export (colors, spacing, typography)
│   └── Templates/
│       ├── header-content-footer.uxml   # Standard app layout
│       ├── sidebar-content.uxml         # Navigation sidebar with content area
│       ├── card-grid.uxml               # Responsive card grid
│       ├── split-screen.uxml            # Left/right split
│       ├── center-dialog.uxml           # Centered modal dialog
│       ├── hud-overlay.uxml             # Game HUD overlay
│       ├── inventory-layout.uxml        # Inventory grid with detail panel
│       ├── list-detail.uxml             # Master-detail list view
│       ├── form-layout.uxml             # Vertical form with labels and inputs
│       └── settings-page.uxml           # Settings with sections and toggles
├── Editor/JUI/Scaffold/
│   ├── ScaffoldGenerator.cs             # CLI/Editor menu scaffold tool
│   └── JUIDLCompiler.cs                 # YAML → UXML+USS+CS compiler
└── .jui-rules.template.yaml            # Project-level AI context template

SourceGenerators/JEngine.JUI.Generators/
└── NamingConventionAnalyzer.cs          # Diagnostic analyzer for naming conventions
```

## API Design

### Component Catalog Schema (jui-component-catalog.json)

```json
{
  "version": "0.6.0",
  "generated": "2025-01-01T00:00:00Z",
  "components": {
    "Button": {
      "description": "Clickable button with states, variants, and optional ripple effect",
      "visual": "Rectangular element with text and/or icon, fills container width by default",
      "category": "input",
      "uxml_tag": "jui:Button",
      "uxml_attrs": {
        "text": { "type": "string", "description": "Button label text" },
        "variant": {
          "type": "enum",
          "values": ["primary", "secondary", "ghost", "danger"],
          "default": "primary",
          "description": "Visual style variant"
        },
        "icon": { "type": "string", "description": "Optional icon sprite name" },
        "disabled": { "type": "bool", "default": "false" }
      },
      "uss_classes": {
        "jui-btn": "Base button class",
        "jui-btn--primary": "Primary filled variant",
        "jui-btn--secondary": "Secondary outlined variant",
        "jui-btn--ghost": "Ghost (text-only) variant",
        "jui-btn--danger": "Destructive action variant",
        "jui-btn--disabled": "Disabled state"
      },
      "signals": {
        "IsPressed": { "type": "Signal<bool>", "description": "True while pointer is down" },
        "IsHovered": { "type": "Signal<bool>", "description": "True while pointer is over" }
      },
      "ai_visual_cues": [
        "Rounded rectangle with text centered",
        "Filled background = primary, outlined = secondary",
        "Ripple animation on click when enabled",
        "Opacity reduced when disabled"
      ],
      "common_mistakes": [
        "Forgetting to set variant -- defaults to primary which may not match intent",
        "Using raw VisualElement click handlers instead of Button's built-in OnClick signal"
      ],
      "related": ["IconButton", "ToggleButton", "ButtonGroup"]
    },
    "TextField": {
      "description": "Single-line text input with validation support",
      "visual": "Bordered input field with optional label and placeholder",
      "category": "input",
      "uxml_tag": "jui:TextField",
      "uxml_attrs": {
        "label": { "type": "string", "description": "Input label" },
        "placeholder": { "type": "string", "description": "Placeholder text" },
        "max-length": { "type": "int", "default": "-1", "description": "Max character count, -1 = unlimited" },
        "password": { "type": "bool", "default": "false", "description": "Mask input characters" }
      },
      "uss_classes": {
        "jui-text-field": "Base class",
        "jui-text-field--focused": "Focus state",
        "jui-text-field--error": "Validation error state",
        "jui-text-field--disabled": "Disabled state"
      },
      "signals": {
        "Value": { "type": "Signal<string>", "description": "Current text value (two-way)" },
        "IsFocused": { "type": "Signal<bool>", "description": "True when field has focus" }
      },
      "ai_visual_cues": [
        "Bordered rectangle with text cursor",
        "Label above or to the left of the input area",
        "Red border = error state"
      ],
      "common_mistakes": [
        "Reading Value outside of an effect -- won't auto-track",
        "Not setting max-length for constrained inputs like player names"
      ],
      "related": ["TextArea", "SearchField", "NumericField"]
    }
  }
}
```

The catalog contains entries for every widget in Sections 27 and 28. Each entry provides enough context for an AI agent to generate correct UXML, USS, and C# code without consulting additional documentation.

### Layout Patterns Schema (jui-layout-patterns.json)

```json
{
  "version": "0.6.0",
  "patterns": {
    "header-content-footer": {
      "description": "Three-row layout with fixed header, scrollable content, and fixed footer",
      "visual": "Header bar at top, main content fills middle (scrollable), footer bar at bottom",
      "use_cases": ["Main menu", "Settings screen", "Inventory with action bar"],
      "template": "Templates/header-content-footer.uxml",
      "slots": {
        "header": { "description": "Fixed-height top bar (navigation, title, back button)" },
        "content": { "description": "Scrollable main content area" },
        "footer": { "description": "Fixed-height bottom bar (action buttons, status)" }
      },
      "ai_cues": [
        "Use FlexDirection.Column on root",
        "Header and footer have FlexShrink(0) and fixed height",
        "Content has FlexGrow(1) and overflow scroll"
      ]
    },
    "sidebar-content": {
      "description": "Two-column layout with fixed-width sidebar and flexible content area",
      "visual": "Narrow panel on left with navigation items, wide content area on right",
      "use_cases": ["Category browser", "Admin panel", "Crafting menu"],
      "template": "Templates/sidebar-content.uxml",
      "slots": {
        "sidebar": { "description": "Fixed-width left panel (navigation, category list)" },
        "content": { "description": "Flexible-width right panel (main content)" }
      },
      "ai_cues": [
        "Use FlexDirection.Row on root",
        "Sidebar has fixed width (200-300px) and FlexShrink(0)",
        "Content has FlexGrow(1)"
      ]
    },
    "card-grid": {
      "description": "Responsive grid of uniformly-sized cards",
      "visual": "Evenly spaced rectangular cards arranged in rows that wrap",
      "use_cases": ["Item shop", "Character select", "Achievement gallery"],
      "template": "Templates/card-grid.uxml",
      "slots": {
        "card": { "description": "Repeated card template (For loop)" }
      },
      "ai_cues": [
        "Use FlexDirection.Row with FlexWrap.Wrap on container",
        "Each card has fixed width, percentage or pixel",
        "Use For component to render card list from reactive collection"
      ]
    },
    "split-screen": {
      "description": "Two equal-width panels side by side",
      "visual": "Left and right panels each taking 50% width",
      "use_cases": ["Comparison view", "Trade interface", "Co-op split"],
      "template": "Templates/split-screen.uxml",
      "slots": {
        "left": { "description": "Left panel (50% width)" },
        "right": { "description": "Right panel (50% width)" }
      },
      "ai_cues": [
        "Use FlexDirection.Row on root",
        "Both children have FlexGrow(1) and FlexBasis(50%)"
      ]
    },
    "center-dialog": {
      "description": "Centered modal dialog with backdrop overlay",
      "visual": "Semi-transparent dark overlay with a centered card containing content and action buttons",
      "use_cases": ["Confirmation dialog", "Alert popup", "Item preview"],
      "template": "Templates/center-dialog.uxml",
      "slots": {
        "title": { "description": "Dialog title text" },
        "body": { "description": "Dialog content area" },
        "actions": { "description": "Action buttons (confirm, cancel)" }
      },
      "ai_cues": [
        "Root overlay uses Position.Absolute filling parent",
        "Dialog card uses AlignSelf.Center and JustifyContent.Center",
        "Backdrop click should dismiss (use Portal for layer management)"
      ]
    },
    "hud-overlay": {
      "description": "Game HUD with elements anchored to screen edges",
      "visual": "Transparent overlay with health bars, minimaps, and action bars anchored to corners and edges",
      "use_cases": ["In-game HUD", "Battle UI", "Racing overlay"],
      "template": "Templates/hud-overlay.uxml",
      "slots": {
        "top-left": { "description": "Player stats, health bar" },
        "top-right": { "description": "Minimap, score" },
        "bottom-center": { "description": "Action bar, ability cooldowns" },
        "bottom-left": { "description": "Chat, quest log" },
        "bottom-right": { "description": "Inventory quick-access" }
      },
      "ai_cues": [
        "Root uses Position.Absolute filling screen",
        "Each slot is Position.Absolute anchored to its corner/edge",
        "Use UILayer.HUD for proper layering",
        "All elements should be pointer-transparent except interactive ones"
      ]
    },
    "inventory-layout": {
      "description": "Grid of item slots with a detail panel",
      "visual": "Large grid of square slots on left, item detail panel on right showing selected item info",
      "use_cases": ["Player inventory", "Storage chest", "Equipment screen"],
      "template": "Templates/inventory-layout.uxml",
      "slots": {
        "grid": { "description": "Item slot grid (typically 8 columns)" },
        "detail": { "description": "Selected item detail panel (icon, name, stats, actions)" }
      },
      "ai_cues": [
        "Use sidebar-content base pattern",
        "Grid slots should be square with fixed size",
        "Use VirtualGrid for large inventories",
        "Detail panel updates reactively based on selectedIndex signal"
      ]
    },
    "list-detail": {
      "description": "Master list on left, detail view on right",
      "visual": "Scrollable list of selectable rows on left, expanded detail of selected item on right",
      "use_cases": ["Email inbox", "Quest log", "Bestiary"],
      "template": "Templates/list-detail.uxml",
      "slots": {
        "list": { "description": "Scrollable selectable list (use VirtualList for large datasets)" },
        "detail": { "description": "Detail view for selected list item" }
      },
      "ai_cues": [
        "Use sidebar-content base pattern with wider sidebar",
        "List items highlight on selection",
        "Detail panel shows empty state when nothing is selected"
      ]
    },
    "form-layout": {
      "description": "Vertical stack of labeled form fields",
      "visual": "Column of label-input pairs with optional validation messages and a submit button at the bottom",
      "use_cases": ["Login form", "Character creation", "Server settings"],
      "template": "Templates/form-layout.uxml",
      "slots": {
        "fields": { "description": "Vertical stack of form field rows" },
        "actions": { "description": "Submit/cancel buttons at bottom" }
      },
      "ai_cues": [
        "Use FlexDirection.Column with consistent spacing",
        "Each field row has a label and an input element",
        "Use Section 25 form validation for schema-based validation",
        "Disable submit button until form is valid"
      ]
    },
    "settings-page": {
      "description": "Grouped settings with section headers and toggle/slider/dropdown controls",
      "visual": "Scrollable page with labeled sections, each containing rows of setting controls",
      "use_cases": ["Game settings", "Audio settings", "Control bindings"],
      "template": "Templates/settings-page.uxml",
      "slots": {
        "sections": { "description": "Repeated section groups (header + controls)" },
        "footer": { "description": "Apply/reset/defaults buttons" }
      },
      "ai_cues": [
        "Use header-content-footer base pattern",
        "Group related settings under section headers",
        "Use Toggle for on/off, Slider for ranges, Dropdown for enums",
        "Persist state via Section 25 state persistence"
      ]
    }
  }
}
```

### Design Token Export (jui-tokens.json)

```json
{
  "version": "0.6.0",
  "tokens": {
    "color": {
      "primary": { "value": "#4A90D9", "uss_var": "--jui-color-primary" },
      "primary-hover": { "value": "#5BA0E9", "uss_var": "--jui-color-primary-hover" },
      "secondary": { "value": "#6C757D", "uss_var": "--jui-color-secondary" },
      "danger": { "value": "#DC3545", "uss_var": "--jui-color-danger" },
      "success": { "value": "#28A745", "uss_var": "--jui-color-success" },
      "warning": { "value": "#FFC107", "uss_var": "--jui-color-warning" },
      "bg-primary": { "value": "#1A1A2E", "uss_var": "--jui-bg-primary" },
      "bg-secondary": { "value": "#16213E", "uss_var": "--jui-bg-secondary" },
      "bg-surface": { "value": "#0F3460", "uss_var": "--jui-bg-surface" },
      "text-primary": { "value": "#E8E8E8", "uss_var": "--jui-text-primary" },
      "text-secondary": { "value": "#A0A0A0", "uss_var": "--jui-text-secondary" },
      "text-disabled": { "value": "#666666", "uss_var": "--jui-text-disabled" },
      "border-default": { "value": "#333333", "uss_var": "--jui-border-default" },
      "border-focus": { "value": "#4A90D9", "uss_var": "--jui-border-focus" }
    },
    "spacing": {
      "xs": { "value": "4px", "uss_var": "--jui-spacing-xs" },
      "sm": { "value": "8px", "uss_var": "--jui-spacing-sm" },
      "md": { "value": "16px", "uss_var": "--jui-spacing-md" },
      "lg": { "value": "24px", "uss_var": "--jui-spacing-lg" },
      "xl": { "value": "32px", "uss_var": "--jui-spacing-xl" },
      "xxl": { "value": "48px", "uss_var": "--jui-spacing-xxl" }
    },
    "typography": {
      "font-size-xs": { "value": "10px", "uss_var": "--jui-font-size-xs" },
      "font-size-sm": { "value": "12px", "uss_var": "--jui-font-size-sm" },
      "font-size-md": { "value": "14px", "uss_var": "--jui-font-size-md" },
      "font-size-lg": { "value": "18px", "uss_var": "--jui-font-size-lg" },
      "font-size-xl": { "value": "24px", "uss_var": "--jui-font-size-xl" },
      "font-size-xxl": { "value": "32px", "uss_var": "--jui-font-size-xxl" },
      "font-weight-normal": { "value": "400", "uss_var": "--jui-font-weight-normal" },
      "font-weight-bold": { "value": "700", "uss_var": "--jui-font-weight-bold" }
    },
    "border": {
      "radius-sm": { "value": "4px", "uss_var": "--jui-border-radius-sm" },
      "radius-md": { "value": "8px", "uss_var": "--jui-border-radius-md" },
      "radius-lg": { "value": "16px", "uss_var": "--jui-border-radius-lg" },
      "radius-full": { "value": "9999px", "uss_var": "--jui-border-radius-full" },
      "width-thin": { "value": "1px", "uss_var": "--jui-border-width-thin" },
      "width-medium": { "value": "2px", "uss_var": "--jui-border-width-medium" }
    },
    "animation": {
      "duration-fast": { "value": "100ms", "uss_var": "--jui-duration-fast" },
      "duration-normal": { "value": "200ms", "uss_var": "--jui-duration-normal" },
      "duration-slow": { "value": "400ms", "uss_var": "--jui-duration-slow" },
      "easing-default": { "value": "ease-in-out-cubic", "uss_var": "--jui-easing-default" }
    }
  }
}
```

### JUIDL Compiler

```csharp
namespace JEngine.JUI.Editor.Scaffold;

/// <summary>
/// Compiles JUIDL (JUI Definition Language) YAML files into UXML, USS, and C# source files.
/// JUIDL provides a concise declarative format that AI agents and developers can use to describe
/// screens without writing three separate files manually.
/// </summary>
public static class JUIDLCompiler
{
    /// <summary>
    /// Compiles a JUIDL YAML string into three output files.
    /// </summary>
    /// <param name="juidlSource">The JUIDL YAML source text.</param>
    /// <param name="outputDirectory">Directory where the UXML, USS, and CS files are written.</param>
    /// <returns>A result containing the paths of generated files or compilation errors.</returns>
    public static JUIDLResult Compile(string juidlSource, string outputDirectory);

    /// <summary>
    /// Validates a JUIDL YAML string without generating output files.
    /// </summary>
    /// <param name="juidlSource">The JUIDL YAML source text.</param>
    /// <returns>A list of validation errors, or empty if valid.</returns>
    public static IReadOnlyList<JUIDLError> Validate(string juidlSource);
}

/// <summary>Result of a JUIDL compilation.</summary>
public readonly struct JUIDLResult
{
    /// <summary>Whether compilation succeeded without errors.</summary>
    public bool Success { get; }

    /// <summary>Path to the generated UXML file, or null if compilation failed.</summary>
    public string UxmlPath { get; }

    /// <summary>Path to the generated USS file, or null if compilation failed.</summary>
    public string UssPath { get; }

    /// <summary>Path to the generated C# file, or null if compilation failed.</summary>
    public string CsPath { get; }

    /// <summary>Compilation errors, if any.</summary>
    public IReadOnlyList<JUIDLError> Errors { get; }
}

/// <summary>A compilation or validation error from the JUIDL compiler.</summary>
public readonly struct JUIDLError
{
    /// <summary>Line number in the JUIDL source where the error occurred.</summary>
    public int Line { get; }

    /// <summary>Human-readable error message.</summary>
    public string Message { get; }

    /// <summary>Error severity: Error prevents compilation, Warning does not.</summary>
    public JUIDLSeverity Severity { get; }
}

/// <summary>Severity levels for JUIDL diagnostics.</summary>
public enum JUIDLSeverity { Error, Warning, Info }
```

### JUIDL Language Specification

```yaml
# JUIDL (JUI Definition Language) — Concise screen definitions
# Compiles to: {screen}.uxml + {screen}.uss + {screen}Screen.cs

screen: inventory                   # Screen name (used for file naming and route)
route: "inventory"                  # Router path (Section 19)
layout: inventory-layout            # Base layout pattern from jui-layout-patterns.json
theme: dark                         # Theme variant (Section 16)

state:                              # Reactive state declarations
  items: ReactiveList<ItemData>     # Typed signal/collection fields
  selectedIndex: Signal<int>(default: -1)
  searchQuery: Signal<string>(default: "")
  isLoading: Signal<bool>(default: true)

inject:                             # DI dependencies (Section 4)
  - InventoryStore
  - ItemDatabase

sections:                           # Layout section bindings
  grid:                             # Maps to "grid" slot in layout pattern
    - inventory-grid:               # Element name: inventory-grid
        bind: items                 # Bind to ReactiveList<ItemData>
        columns: 8                  # Component-specific attribute
        on-select: selectedIndex    # Signal to update on selection
  detail:                           # Maps to "detail" slot in layout pattern
    - item-icon:
        type: Image
        bind: "items[selectedIndex].Icon"
    - item-name:
        type: Label
        bind: "items[selectedIndex].Name"
    - item-description:
        type: Label
        bind: "items[selectedIndex].Description"
    - use-button:
        type: Button
        text: "Use Item"
        variant: primary
        on-click: UseSelectedItem   # Generates partial method stub

effects:                            # Effect declarations
  - watch: [searchQuery]
    do: FilterItems                 # Generates partial method stub
  - watch: [selectedIndex]
    when: "selectedIndex >= 0"
    do: LoadItemDetail

events:                             # Event subscriptions
  - subscribe: InventoryChangedEvent
    handler: RefreshItems           # Generates partial method stub
```

The compiler produces three files from this input:

1. **InventoryScreen.uxml** -- Element tree based on the layout pattern with all named elements, types, and attributes.
2. **InventoryScreen.uss** -- Stylesheet with classes derived from the layout pattern, theme tokens applied.
3. **InventoryScreen.cs** -- Partial class extending `Component` with `[UIComponent]`, `[Inject]`, signal fields, `[Bind]` attributes, `[Effect]` methods, `[Subscribe]` handlers, and partial method stubs for event handlers.

### Scaffold Generator

```csharp
namespace JEngine.JUI.Editor.Scaffold;

/// <summary>
/// Generates compilable JUI screen scaffolds from natural language descriptions or
/// structured specifications. Accessible via Editor menu or CLI command.
/// </summary>
public static class ScaffoldGenerator
{
    /// <summary>
    /// Generates a JUI screen from a structured specification.
    /// Creates UXML, USS, and CS files in the output directory.
    /// </summary>
    /// <param name="spec">The scaffold specification.</param>
    /// <param name="outputDirectory">Directory where files are written.</param>
    /// <returns>Paths to the generated files.</returns>
    public static ScaffoldResult Generate(ScaffoldSpec spec, string outputDirectory);

    /// <summary>
    /// Generates a JUI screen from a JUIDL YAML file.
    /// Delegates to JUIDLCompiler internally.
    /// </summary>
    /// <param name="juidlPath">Path to the .juidl YAML file.</param>
    /// <param name="outputDirectory">Directory where files are written.</param>
    /// <returns>Paths to the generated files.</returns>
    public static ScaffoldResult GenerateFromJUIDL(string juidlPath, string outputDirectory);
}

/// <summary>Structured scaffold specification.</summary>
public sealed class ScaffoldSpec
{
    /// <summary>Screen name (PascalCase, used for class and file naming).</summary>
    public string ScreenName { get; set; }

    /// <summary>Layout pattern name from jui-layout-patterns.json.</summary>
    public string LayoutPattern { get; set; }

    /// <summary>List of named elements with their types.</summary>
    public List<ScaffoldElement> Elements { get; set; } = new();

    /// <summary>Signal field declarations.</summary>
    public List<ScaffoldSignal> Signals { get; set; } = new();

    /// <summary>DI dependency type names.</summary>
    public List<string> Injects { get; set; } = new();

    /// <summary>Route path for the screen router.</summary>
    public string Route { get; set; }
}

/// <summary>An element in the scaffold specification.</summary>
public sealed class ScaffoldElement
{
    /// <summary>Element name in kebab-case (e.g., "health-bar").</summary>
    public string Name { get; set; }

    /// <summary>UI Toolkit element type (e.g., "Label", "Button", "VisualElement").</summary>
    public string Type { get; set; }

    /// <summary>Optional slot name this element belongs to.</summary>
    public string Slot { get; set; }

    /// <summary>Optional signal name to bind to.</summary>
    public string BindSignal { get; set; }
}

/// <summary>A signal field in the scaffold specification.</summary>
public sealed class ScaffoldSignal
{
    /// <summary>Field name in camelCase (e.g., "playerHealth").</summary>
    public string Name { get; set; }

    /// <summary>Signal type (e.g., "Signal<int>", "ReactiveList<ItemData>").</summary>
    public string Type { get; set; }

    /// <summary>Default value expression (e.g., "100", "new()").</summary>
    public string DefaultValue { get; set; }
}

/// <summary>Result of scaffold generation.</summary>
public readonly struct ScaffoldResult
{
    public bool Success { get; }
    public string UxmlPath { get; }
    public string UssPath { get; }
    public string CsPath { get; }
    public IReadOnlyList<string> Warnings { get; }
}
```

### Naming Convention Specification

The naming convention analyzer enforces consistent naming across UXML, USS, and C# files. These conventions are checked at compile time via Roslyn diagnostics.

**UXML Element Names:**
- Must be kebab-case: `health-bar`, `confirm-button`, `player-name`
- Must follow `{purpose}-{type}` pattern: `damage-label`, `inventory-grid`, `settings-toggle`
- Must not use generic names: `container1`, `panel2`, `element`

**USS Class Names:**
- Must follow BEM-like convention: `jui-{component}--{variant}`
- Block: `jui-btn`, `jui-card`, `jui-input`
- Modifier: `jui-btn--primary`, `jui-card--selected`, `jui-input--error`
- Element (within block): `jui-card__title`, `jui-card__body`

**C# Naming:**
- Classes: PascalCase with role suffix
  - Screens: `{Name}Screen` (e.g., `InventoryScreen`, `SettingsScreen`)
  - Stores: `{Name}Store` (e.g., `PlayerStore`, `InventoryStore`)
  - Bridges: `{Name}Bridge` (e.g., `CharacterBridge`, `CameraBridge`)
  - Events: `{Name}Event` (e.g., `DamageEvent`, `ItemPickupEvent`)
- Signal fields: `_camelCase` (e.g., `_playerHealth`, `_selectedIndex`)
- Methods: PascalCase imperative verb (e.g., `LoadItems`, `RefreshUI`)

### Naming Convention Diagnostics

```csharp
namespace JEngine.JUI.Generators;

// These diagnostics are emitted by the NamingConventionAnalyzer

/// <summary>UXML element name is not kebab-case.</summary>
// JUI200, Warning: "Element name '{0}' should be kebab-case (e.g., 'health-bar')"

/// <summary>UXML element name does not follow {purpose}-{type} pattern.</summary>
// JUI201, Info: "Element name '{0}' should follow the {{purpose}}-{{type}} naming pattern"

/// <summary>UXML element name is too generic.</summary>
// JUI202, Warning: "Element name '{0}' is too generic; use a descriptive name like 'player-health-bar'"

/// <summary>USS class does not follow BEM-like jui-{block} convention.</summary>
// JUI203, Warning: "USS class '{0}' should follow 'jui-{{component}}' naming convention"

/// <summary>C# component class is missing required role suffix.</summary>
// JUI204, Info: "Component class '{0}' should have a role suffix (Screen, Store, Bridge, etc.)"

/// <summary>C# signal field does not follow _camelCase convention.</summary>
// JUI205, Warning: "Signal field '{0}' should follow _camelCase naming convention"

/// <summary>C# event struct does not end with 'Event' suffix.</summary>
// JUI206, Warning: "Event struct '{0}' should end with 'Event' suffix"
```

### .jui-rules Template

```yaml
# .jui-rules.yaml — Project-level AI context for JUI development
# Place this file in your project root to provide AI agents with project-specific context.
# Copy from .jui-rules.template.yaml and customize.

project:
  name: "MyGame"
  description: "A fantasy RPG with crafting and exploration"
  genre: "RPG"
  art-style: "Pixel art with modern UI"

theme:
  base: dark                          # dark | light | custom
  primary-color: "#4A90D9"
  accent-color: "#E94560"
  font-family: "Roboto"
  custom-tokens:                      # Override any token from jui-tokens.json
    --jui-bg-primary: "#0D1117"
    --jui-border-radius-md: "4px"

stores:                               # Global stores available via DI
  - name: PlayerStore
    signals: [Health, Mana, Level, Experience, Gold]
  - name: InventoryStore
    signals: [Items, EquippedSlots, Capacity]
  - name: QuestStore
    signals: [ActiveQuests, CompletedQuests]
  - name: SettingsStore
    signals: [Volume, MusicVolume, SfxVolume, Resolution, Fullscreen]

bridges:                              # Unity-to-JUI bridge objects
  - name: CameraBridge
    description: "Provides camera position and viewport data"
  - name: CharacterBridge
    description: "Provides character stats, buffs, and equipment"

widgets:                              # Custom widget overrides and preferences
  health-bar:
    style: "segmented"                # segmented | smooth | chunked
    color-thresholds:
      critical: 0.2
      low: 0.4
  inventory-grid:
    columns: 8
    slot-size: "64px"
    show-empty-slots: true

conventions:
  naming:
    screens: "{Name}Screen"           # InventoryScreen, SettingsScreen
    stores: "{Name}Store"             # PlayerStore, InventoryStore
    bridges: "{Name}Bridge"           # CameraBridge
    events: "{Name}Event"             # DamageEvent
  file-organization:
    screens: "Assets/UI/Screens/"
    stores: "Assets/UI/Stores/"
    bridges: "Assets/UI/Bridges/"
    events: "Assets/UI/Events/"
    shared-uss: "Assets/UI/Styles/"

layout-preferences:
  default-layout: header-content-footer
  dialog-style: center-dialog
  hud-style: hud-overlay
  prefer-virtual-lists: true          # Use VirtualList for lists > 50 items
  mobile-safe-area: true              # Apply safe area insets on mobile

audio:
  click: "ui_click_01"
  hover: "ui_hover_01"
  open-screen: "ui_whoosh_01"
  close-screen: "ui_whoosh_02"
  error: "ui_error_01"
  success: "ui_success_01"
```

## Data Structures

| Type | Purpose |
|------|---------|
| `ComponentCatalogEntry` | In-memory representation of a single component from `jui-component-catalog.json`. Contains description, UXML tag, attributes, USS classes, signals, visual cues, common mistakes, and related components. |
| `LayoutPattern` | In-memory representation of a named layout pattern. Contains description, visual description, use cases, template path, slots, and AI cues. |
| `DesignToken` | A single token entry with value, USS variable name, and category. |
| `JUIDLDocument` | Parsed representation of a JUIDL YAML file: screen name, route, layout, state declarations, inject list, sections, effects, and events. |
| `ScaffoldSpec` | Structured specification for scaffold generation, independent of JUIDL. |
| `NamingRule` | A naming convention rule with pattern, diagnostic ID, severity, and message template. |

## Implementation Notes

### Component Catalog Generation

The `jui-component-catalog.json` file is authored manually but can be regenerated from source code using an editor script that reflects over all types with `[UIComponent]` attributes. The catalog should be updated whenever a new widget is added to Sections 27 or 28.

### JUIDL Compilation Strategy

1. **Parse**: YAML is parsed using a lightweight YAML parser (no external dependency -- a simple recursive descent parser for the subset of YAML used by JUIDL).
2. **Resolve**: Layout pattern is looked up from `jui-layout-patterns.json`. Slot names in the `sections` block are validated against the pattern's slot definitions.
3. **Emit UXML**: The layout template UXML is loaded and populated with the declared elements. Each element is placed in its corresponding slot. Element types and attributes are mapped from the JUIDL shorthand to full UXML tag syntax.
4. **Emit USS**: A base USS file is generated with classes for each named element. Theme tokens are applied based on the `theme` declaration. Custom styles can be added manually after generation.
5. **Emit C#**: A partial class is generated with the `[UIComponent]` attribute, `[Inject]` fields, signal declarations, `[Bind]` attributes, `[Effect]` methods, and partial method stubs for event handlers. The class compiles immediately but partial methods need user implementation.

### Scaffold Generator Integration

The scaffold generator is accessible via:
- **Editor menu**: `JUI > Scaffold > New Screen...` opens a wizard dialog
- **Editor menu**: `JUI > Scaffold > From JUIDL File...` opens a file picker
- **CLI**: `jui scaffold --from inventory.juidl --out Assets/UI/Screens/`

### Naming Convention Enforcement

The `NamingConventionAnalyzer` is an additional Roslyn analyzer (not a generator) in the `JEngine.JUI.Generators` assembly. It runs alongside the other generators and reports diagnostics without generating code. Diagnostics are configurable via `.editorconfig`:

```ini
# .editorconfig
[*.cs]
dotnet_diagnostic.JUI200.severity = warning
dotnet_diagnostic.JUI201.severity = suggestion
dotnet_diagnostic.JUI202.severity = warning
dotnet_diagnostic.JUI204.severity = none           # Disable role suffix check
```

## Source Generator Notes

### NamingConventionAnalyzer

- **Type**: `DiagnosticAnalyzer` (not `IIncrementalGenerator`), since it only reports diagnostics and does not generate code.
- **Trigger**: Analyzes all classes with `[UIComponent]` attribute and their associated UXML/USS additional files.
- **Diagnostic IDs**: JUI200-JUI206 (see Naming Convention Diagnostics above).
- **Configurability**: All diagnostics respect `.editorconfig` severity overrides, allowing projects to opt out of specific rules.
- **Performance**: Uses `SymbolAction` and `AdditionalFileAction` callbacks for incremental analysis. Does not re-analyze unchanged files.

### JUIDL is NOT a Source Generator

The JUIDL compiler is an editor-time tool, not a source generator. It runs on demand (via menu or CLI) and produces standard files that are then processed by the existing JUI source generators. This two-stage approach means:
1. JUIDL output is human-editable -- developers can refine the generated UXML/USS/CS.
2. The JUI generators (Element, Style, Inject, Binding, Effect) run on the generated files just like any hand-written code.
3. No build-time dependency on YAML parsing.

## Usage Examples

### Generating a Screen from JUIDL

```yaml
# File: Assets/UI/Screens/inventory.juidl

screen: inventory
route: "inventory"
layout: inventory-layout

state:
  items: ReactiveList<ItemData>
  selectedIndex: Signal<int>(default: -1)

inject:
  - InventoryStore

sections:
  grid:
    - inventory-grid:
        bind: items
        columns: 8
        on-select: selectedIndex
  detail:
    - item-icon:
        type: Image
        bind: "items[selectedIndex].Icon"
    - item-name:
        type: Label
        bind: "items[selectedIndex].Name"
    - use-button:
        type: Button
        text: "Use"
        variant: primary
        on-click: UseSelectedItem

effects:
  - watch: [selectedIndex]
    when: "selectedIndex >= 0"
    do: LoadItemDetail
```

Compile via menu `JUI > Scaffold > From JUIDL File...` or CLI:

```bash
jui scaffold --from Assets/UI/Screens/inventory.juidl --out Assets/UI/Screens/
```

Produces:
- `Assets/UI/Screens/InventoryScreen.uxml`
- `Assets/UI/Screens/InventoryScreen.uss`
- `Assets/UI/Screens/InventoryScreen.cs`

### Generated C# from JUIDL

```csharp
// InventoryScreen.cs (generated by JUIDLCompiler)
using JEngine.JUI;
using JEngine.JUI.State;
using JEngine.JUI.Collections;

[UIComponent("InventoryScreen.uxml", "InventoryScreen.uss")]
public partial class InventoryScreen : Component
{
    // --- State ---
    private ReactiveList<ItemData> _items = new();
    private Signal<int> _selectedIndex = new(-1);

    // --- DI ---
    [Inject] private InventoryStore _inventoryStore;

    // --- Bindings ---
    [Bind(nameof(El.InventoryGrid), nameof(_items))]
    [Bind(nameof(El.ItemIcon), "items[selectedIndex].Icon")]
    [Bind(nameof(El.ItemName), "items[selectedIndex].Name")]

    // --- Effects ---
    [Effect(Watch = new[] { nameof(_selectedIndex) }, When = nameof(_selectedIndex) + " >= 0")]
    private void LoadItemDetail()
    {
        // TODO: Implement item detail loading
    }

    // --- Event Handlers ---
    private partial void UseSelectedItem();
}
```

### Using the Component Catalog in AI Prompts

An AI agent can load `jui-component-catalog.json` and use it to understand available components:

```
Agent reads catalog -> knows Button has variants [primary, secondary, ghost, danger]
Agent reads catalog -> knows TextField has signals [Value, IsFocused]
Agent reads catalog -> knows common mistakes for each component
Agent generates correct UXML: <jui:Button variant="danger" text="Delete" />
```

### Using .jui-rules for Project Context

An AI agent reads `.jui-rules.yaml` to understand the project:

```
Agent reads .jui-rules -> knows PlayerStore has Health, Mana, Level signals
Agent reads .jui-rules -> knows screens go in Assets/UI/Screens/
Agent reads .jui-rules -> knows health bars use segmented style
Agent generates a HUD screen using [Inject] PlayerStore and binds Health signal
```

### Scaffold Generator -- Programmatic API

```csharp
var spec = new ScaffoldSpec
{
    ScreenName = "PlayerHUD",
    LayoutPattern = "hud-overlay",
    Route = "hud",
    Injects = new List<string> { "PlayerStore", "CameraBridge" },
    Signals = new List<ScaffoldSignal>
    {
        new() { Name = "health", Type = "Signal<float>", DefaultValue = "1f" },
        new() { Name = "mana", Type = "Signal<float>", DefaultValue = "1f" },
        new() { Name = "level", Type = "Signal<int>", DefaultValue = "1" }
    },
    Elements = new List<ScaffoldElement>
    {
        new() { Name = "health-bar", Type = "ProgressBar", Slot = "top-left", BindSignal = "health" },
        new() { Name = "mana-bar", Type = "ProgressBar", Slot = "top-left", BindSignal = "mana" },
        new() { Name = "level-label", Type = "Label", Slot = "top-left", BindSignal = "level" },
        new() { Name = "minimap", Type = "VisualElement", Slot = "top-right" }
    }
};

var result = ScaffoldGenerator.Generate(spec, "Assets/UI/Screens/");
// result.UxmlPath = "Assets/UI/Screens/PlayerHUD.uxml"
// result.UssPath  = "Assets/UI/Screens/PlayerHUD.uss"
// result.CsPath   = "Assets/UI/Screens/PlayerHUDScreen.cs"
```

## Test Plan

### Component Catalog Tests

| # | Test | Expectation |
|---|------|-------------|
| 1 | Parse `jui-component-catalog.json` | All component entries deserialize correctly with all fields populated |
| 2 | Every component has `uxml_tag`, `description`, and at least one `uss_class` | Validation passes for all entries |
| 3 | Component `related` references point to existing catalog entries | No dangling references |

### Layout Pattern Tests

| # | Test | Expectation |
|---|------|-------------|
| 4 | Parse `jui-layout-patterns.json` | All 10 patterns deserialize correctly |
| 5 | Every pattern has a valid `template` path | Template UXML file exists at the referenced path |
| 6 | Pattern slot names are unique within each pattern | No duplicate slot names |

### JUIDL Compiler Tests

| # | Test | Expectation |
|---|------|-------------|
| 7 | Compile valid JUIDL with all features | Three files generated, all compile without errors |
| 8 | JUIDL with unknown layout pattern | Compiler returns error: "Unknown layout pattern '{name}'" |
| 9 | JUIDL with invalid signal type syntax | Compiler returns error with correct line number |
| 10 | JUIDL with element in non-existent slot | Compiler returns error: "Slot '{name}' not found in layout '{pattern}'" |
| 11 | JUIDL `Validate()` on valid input | Returns empty error list |
| 12 | JUIDL `Validate()` on invalid input | Returns errors without generating files |
| 13 | Generated UXML contains all declared elements with correct types | Elements present with correct UXML tags |
| 14 | Generated C# has `[UIComponent]` attribute and correct class name | Class compiles and inherits from Component |
| 15 | Generated C# has `[Inject]` fields for all declared injects | Fields present with correct types |
| 16 | Generated C# has signal fields with default values | Signals initialized with specified defaults |

### Scaffold Generator Tests

| # | Test | Expectation |
|---|------|-------------|
| 17 | Generate from ScaffoldSpec with all fields | Three files generated with correct content |
| 18 | Generate from JUIDL file path | Delegates to JUIDLCompiler and returns same result |
| 19 | ScaffoldSpec with empty elements list | Generates files with layout pattern but no custom elements |
| 20 | ScaffoldSpec with duplicate element names | Returns warning about duplicates |

### Naming Convention Analyzer Tests

| # | Test | Expectation |
|---|------|-------------|
| 21 | Element name `healthBar` (camelCase, not kebab) | JUI200 warning emitted |
| 22 | Element name `health-bar` (correct kebab-case) | No diagnostic |
| 23 | Element name `container1` (too generic) | JUI202 warning emitted |
| 24 | USS class `my-button` (missing `jui-` prefix) | JUI203 warning emitted |
| 25 | USS class `jui-btn--primary` (correct BEM) | No diagnostic |
| 26 | Component class `Inventory` (missing suffix) | JUI204 info emitted |
| 27 | Component class `InventoryScreen` (correct suffix) | No diagnostic |
| 28 | Signal field `PlayerHealth` (not _camelCase) | JUI205 warning emitted |
| 29 | Event struct `Damage` (missing `Event` suffix) | JUI206 warning emitted |
| 30 | Diagnostics respect `.editorconfig` severity override | Suppressed diagnostic not reported |

### Design Token Tests

| # | Test | Expectation |
|---|------|-------------|
| 31 | Parse `jui-tokens.json` | All token categories deserialize correctly |
| 32 | Every token has `value` and `uss_var` fields | Validation passes |
| 33 | Token USS variable names start with `--jui-` prefix | All variables follow convention |

## Acceptance Criteria

- [ ] `jui-component-catalog.json` contains entries for all base widgets (Section 27) and game widgets (Section 28) with UXML tags, attributes, USS classes, signals, visual cues, and common mistakes
- [ ] `jui-layout-patterns.json` contains all 10 named patterns (header-content-footer, sidebar-content, card-grid, split-screen, center-dialog, hud-overlay, inventory-layout, list-detail, form-layout, settings-page)
- [ ] Each layout pattern has a corresponding UXML template file in `Templates/`
- [ ] `jui-tokens.json` exports all design tokens from Section 16 with values and USS variable names
- [ ] JUIDL compiler parses valid YAML and produces compilable UXML, USS, and C# files
- [ ] JUIDL compiler reports errors with line numbers for invalid input
- [ ] JUIDL `Validate()` checks input without generating files
- [ ] Scaffold generator produces compilable output from `ScaffoldSpec`
- [ ] Scaffold generator accepts JUIDL file paths and delegates to the compiler
- [ ] Naming convention analyzer emits diagnostics JUI200-JUI206 for violations
- [ ] Naming convention diagnostics are configurable via `.editorconfig`
- [ ] `.jui-rules.template.yaml` provides a comprehensive project context template
- [ ] AI agents can consume all three JSON files to generate correct JUI code without consulting other documentation
- [ ] Generated C# files from JUIDL include `[UIComponent]`, `[Inject]`, signal fields, `[Bind]`, `[Effect]`, and partial method stubs
- [ ] All editor tools are accessible via `JUI >` menu hierarchy
