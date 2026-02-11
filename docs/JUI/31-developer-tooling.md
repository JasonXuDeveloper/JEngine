# Section 31 — Developer Tooling

## Overview

Developer Tooling provides a suite of EditorWindows for debugging, previewing, and profiling JUI components at runtime and in edit mode. Each tool inspects live runtime state -- signals, effects, bindings, gestures, focus, routes, audio, and shader effects -- and presents it in a structured, interactive UI. The tools are designed to accelerate the edit-debug-iterate cycle by making the framework's internal state visible and manipulable.

All tools are accessible via the `JUI > Debug >` menu hierarchy and can be docked alongside Unity's built-in inspector and hierarchy windows.

## Dependencies

| Section | What It Provides |
|---------|-----------------|
| 1 — Reactive Primitives | Signal/Computed state inspection |
| 2 — Effect System | Effect dependency graph and dirty state |
| 4 — DI Container | Scope hierarchy visualization |
| 5 — Binding System | Active binding inspection |
| 6 — Component Base Class | Component tree structure |
| 15 — Animation | Animation state inspection |
| 16 — Theming | Live token editing |
| 17 — Gestures | Recognizer state inspection |
| 19 — Screen Router | Navigation stack inspection |
| 20 — Audio | Audio trigger and debounce inspection |
| 21 — Shader Effects | Shader parameter preview |
| 22 — Focus & Navigation | Focus ring and NavMap visualization |
| 30 — JUIManager | Runtime state access for all subsystems |

## File Structure

```
Packages/com.jasonxudeveloper.jengine.ui/
└── Editor/JUI/
    ├── UIPreviewWindow.cs            # Live component preview without Play mode
    ├── ComponentDebugger.cs          # Component tree inspector
    ├── BindingDebugger.cs            # Active binding viewer
    ├── EffectDebugger.cs             # Effect dependency and dirty state viewer
    ├── FocusDebugger.cs              # Focus ring and NavMap visualizer
    ├── GestureDebugger.cs            # Active gesture recognizer viewer
    ├── PerformanceOverlay.cs         # In-game performance HUD
    ├── ThemeEditor.cs                # Live design token editor
    ├── SignalGraph.cs                # Visual signal→computed→effect dependency graph
    ├── RouteDebugger.cs              # Navigation stack inspector
    ├── AudioDebugger.cs              # Audio trigger and debounce inspector
    └── ShaderEffectPreview.cs        # Shader effect parameter preview
```

## API Design

### 1. UIPreviewWindow

```csharp
namespace JEngine.JUI.Editor;

/// <summary>
/// EditorWindow that renders a JUI component in an isolated UIDocument
/// without entering Play mode. Supports hot-reload on script recompilation
/// and live theme switching.
/// </summary>
/// <remarks>
/// The preview window creates a temporary UIDocument within the editor
/// and mounts the selected component into it. Signal values can be
/// manipulated via the inspector panel on the right side of the window.
/// Changes are not persisted -- the component is re-created on each
/// preview session.
/// </remarks>
public sealed class UIPreviewWindow : EditorWindow
{
    /// <summary>Opens the preview window from the menu: JUI > Debug > UI Preview.</summary>
    [MenuItem("JUI/Debug/UI Preview")]
    public static void ShowWindow();

    /// <summary>
    /// Sets the component type to preview. Can be called programmatically
    /// to open the preview for a specific component.
    /// </summary>
    /// <typeparam name="T">The component type to preview.</typeparam>
    public void Preview<T>() where T : Component, new();

    /// <summary>
    /// The currently previewed component instance, or null if nothing is mounted.
    /// </summary>
    public Component ActiveComponent { get; }

    /// <summary>
    /// The theme applied to the preview. Can be changed at runtime
    /// to test different theme variants.
    /// </summary>
    public ThemeAsset PreviewTheme { get; set; }
}
```

### 2. ComponentDebugger

```csharp
namespace JEngine.JUI.Editor;

/// <summary>
/// EditorWindow that displays a tree view of all mounted JUI components.
/// Each node shows the component type, signal count, child count, and scope.
/// Clicking a node selects it for detailed inspection in the right panel.
/// </summary>
/// <remarks>
/// The tree updates in real-time during Play mode. Components that re-rendered
/// in the current frame are highlighted with a flash animation.
/// </remarks>
public sealed class ComponentDebugger : EditorWindow
{
    /// <summary>Opens the window from the menu: JUI > Debug > Component Tree.</summary>
    [MenuItem("JUI/Debug/Component Tree")]
    public static void ShowWindow();

    /// <summary>
    /// The currently selected component in the tree view.
    /// </summary>
    public Component SelectedComponent { get; }

    /// <summary>
    /// When true, the tree auto-scrolls to components that re-rendered this frame.
    /// </summary>
    public bool AutoTrackUpdates { get; set; }
}
```

### 3. BindingDebugger

```csharp
namespace JEngine.JUI.Editor;

/// <summary>
/// EditorWindow that lists all active bindings with their mode (Push/Pull/Sync),
/// source signal, target element, target property, and current value.
/// Bindings that have not updated in the last 5 seconds are dimmed.
/// Stale bindings (source disposed but binding still registered) are highlighted in red.
/// </summary>
public sealed class BindingDebugger : EditorWindow
{
    /// <summary>Opens the window from the menu: JUI > Debug > Bindings.</summary>
    [MenuItem("JUI/Debug/Bindings")]
    public static void ShowWindow();

    /// <summary>Filter bindings by component. Null shows all bindings.</summary>
    public Component FilterComponent { get; set; }

    /// <summary>Filter bindings by mode. Null shows all modes.</summary>
    public BindingMode? FilterMode { get; set; }

    /// <summary>Total number of active bindings.</summary>
    public int ActiveBindingCount { get; }
}
```

### 4. EffectDebugger

```csharp
namespace JEngine.JUI.Editor;

/// <summary>
/// EditorWindow that displays all registered effects with their dependency graph,
/// dirty state, run count, and conditional status. Clicking an effect reveals
/// which signals it reads (dependencies) and which signals trigger it.
/// </summary>
/// <remarks>
/// The effect list is sortable by run count, last-run time, and dependency count.
/// Effects that ran in the current frame are highlighted.
/// Conditional effects (with When clauses) show their current condition state.
/// </remarks>
public sealed class EffectDebugger : EditorWindow
{
    /// <summary>Opens the window from the menu: JUI > Debug > Effects.</summary>
    [MenuItem("JUI/Debug/Effects")]
    public static void ShowWindow();

    /// <summary>Filter effects by component. Null shows all effects.</summary>
    public Component FilterComponent { get; set; }

    /// <summary>Show only effects that are currently dirty.</summary>
    public bool ShowDirtyOnly { get; set; }

    /// <summary>Total number of registered effects.</summary>
    public int TotalEffectCount { get; }

    /// <summary>Number of effects that ran in the most recent frame.</summary>
    public int EffectsRanThisFrame { get; }
}
```

### 5. FocusDebugger

```csharp
namespace JEngine.JUI.Editor;

/// <summary>
/// EditorWindow that visualizes the focus navigation map (NavMap).
/// Displays connections between focusable elements as a graph overlay.
/// Shows the current focused element, focus trap boundaries, and input mode
/// (keyboard vs. gamepad vs. pointer).
/// </summary>
/// <remarks>
/// The overlay is drawn directly on the Game view when the debugger is open.
/// Focus connections are shown as colored lines: blue for normal navigation,
/// red for trap boundaries, green for the current focus path.
/// </remarks>
public sealed class FocusDebugger : EditorWindow
{
    /// <summary>Opens the window from the menu: JUI > Debug > Focus.</summary>
    [MenuItem("JUI/Debug/Focus")]
    public static void ShowWindow();

    /// <summary>The currently focused element, or null.</summary>
    public VisualElement CurrentFocus { get; }

    /// <summary>The current input mode.</summary>
    public InputMode CurrentInputMode { get; }

    /// <summary>When true, shows NavMap connection lines on the Game view.</summary>
    public bool ShowOverlay { get; set; }
}
```

### 6. GestureDebugger

```csharp
namespace JEngine.JUI.Editor;

/// <summary>
/// EditorWindow that lists all active gesture recognizers with their type,
/// target element, current state, and threshold values. Includes a real-time
/// visualization of tap zones and swipe thresholds overlaid on the Game view.
/// </summary>
public sealed class GestureDebugger : EditorWindow
{
    /// <summary>Opens the window from the menu: JUI > Debug > Gestures.</summary>
    [MenuItem("JUI/Debug/Gestures")]
    public static void ShowWindow();

    /// <summary>Total number of active gesture recognizers.</summary>
    public int ActiveRecognizerCount { get; }

    /// <summary>When true, shows tap zones and swipe thresholds on the Game view.</summary>
    public bool ShowOverlay { get; set; }
}
```

### 7. PerformanceOverlay

```csharp
namespace JEngine.JUI.Editor;

/// <summary>
/// In-game performance overlay that shows real-time JUI metrics.
/// Can be toggled via a configurable debug key (default: F3) or
/// programmatically.
/// </summary>
/// <remarks>
/// The overlay renders into the System UILayer so it is always on top.
/// It shows:
/// - FPS (with min/max/avg over the last second)
/// - Effect runs per frame
/// - Binding updates per frame
/// - Event publishes per frame
/// - Signal changes per frame
/// - Allocation tracker (bytes allocated per frame, JUI-related only)
/// - Active component count
/// - Mounted layer breakdown
///
/// The overlay itself is implemented as a JUI component with minimal
/// signal usage to avoid skewing the metrics it reports.
/// </remarks>
public sealed class PerformanceOverlay : EditorWindow
{
    /// <summary>Opens the configuration window from the menu: JUI > Debug > Performance.</summary>
    [MenuItem("JUI/Debug/Performance")]
    public static void ShowWindow();

    /// <summary>Enables or disables the in-game overlay.</summary>
    public bool OverlayEnabled { get; set; }

    /// <summary>The key used to toggle the overlay at runtime.</summary>
    public KeyCode ToggleKey { get; set; }
}

/// <summary>
/// Runtime performance metrics collector. Collects per-frame statistics
/// for the PerformanceOverlay to display.
/// </summary>
public static class JUIMetrics
{
    /// <summary>Number of effects that ran in the current frame.</summary>
    public static int EffectsRanThisFrame { get; }

    /// <summary>Number of bindings that updated in the current frame.</summary>
    public static int BindingsUpdatedThisFrame { get; }

    /// <summary>Number of events published in the current frame.</summary>
    public static int EventsPublishedThisFrame { get; }

    /// <summary>Number of signal changes in the current frame.</summary>
    public static int SignalChangesThisFrame { get; }

    /// <summary>Estimated bytes allocated by JUI operations this frame.</summary>
    public static long AllocBytesThisFrame { get; }

    /// <summary>Current FPS (smoothed over 1 second).</summary>
    public static float CurrentFPS { get; }

    /// <summary>Total number of mounted components across all layers.</summary>
    public static int MountedComponentCount { get; }

    /// <summary>Resets all per-frame counters. Called at the start of each frame by JUIManager.</summary>
    internal static void ResetFrameCounters();
}
```

### 8. ThemeEditor

```csharp
namespace JEngine.JUI.Editor;

/// <summary>
/// EditorWindow for live editing of JUI design tokens. Changes apply in
/// real-time to all mounted components. Includes color pickers, spacing
/// sliders, typography controls, and border radius adjustments.
/// </summary>
/// <remarks>
/// Modifications can be saved as a new ThemeAsset ScriptableObject or
/// applied to an existing one. The editor also supports importing/exporting
/// tokens from/to <c>jui-tokens.json</c>.
/// </remarks>
public sealed class ThemeEditor : EditorWindow
{
    /// <summary>Opens the window from the menu: JUI > Debug > Theme Editor.</summary>
    [MenuItem("JUI/Debug/Theme Editor")]
    public static void ShowWindow();

    /// <summary>The theme asset currently being edited.</summary>
    public ThemeAsset ActiveTheme { get; set; }

    /// <summary>Saves the current token values to a new ThemeAsset.</summary>
    /// <param name="path">Asset path for the new ScriptableObject.</param>
    /// <returns>The created ThemeAsset.</returns>
    public ThemeAsset SaveAsNewTheme(string path);

    /// <summary>Exports the current tokens to a JSON file compatible with jui-tokens.json.</summary>
    /// <param name="path">File path for the JSON export.</param>
    public void ExportToJson(string path);

    /// <summary>Imports tokens from a JSON file and applies them to the active theme.</summary>
    /// <param name="path">File path of the JSON to import.</param>
    public void ImportFromJson(string path);

    /// <summary>Resets all tokens to the active theme's saved values.</summary>
    public void ResetToSaved();
}
```

### 9. SignalGraph

```csharp
namespace JEngine.JUI.Editor;

/// <summary>
/// EditorWindow that renders a visual node-based graph of signal, computed,
/// and effect dependencies. Nodes are color-coded by type:
/// - Blue: Signal
/// - Purple: ComputedSignal
/// - Green: Effect
/// - Orange: Binding
///
/// Edges show data flow direction. Active paths (signals that changed this
/// frame) are highlighted with animated edges. Clicking a node shows its
/// current value, version, and subscriber count.
/// </summary>
public sealed class SignalGraph : EditorWindow
{
    /// <summary>Opens the window from the menu: JUI > Debug > Signal Graph.</summary>
    [MenuItem("JUI/Debug/Signal Graph")]
    public static void ShowWindow();

    /// <summary>Filter the graph to show only nodes related to a specific component.</summary>
    public Component FilterComponent { get; set; }

    /// <summary>When true, highlights edges for signals that changed this frame.</summary>
    public bool HighlightActiveEdges { get; set; }

    /// <summary>Layout algorithm for the graph: Auto, ForceDirected, Hierarchical.</summary>
    public GraphLayout Layout { get; set; }
}

/// <summary>Layout algorithm options for the signal graph.</summary>
public enum GraphLayout
{
    /// <summary>Automatic layout based on node count.</summary>
    Auto,

    /// <summary>Force-directed physics simulation layout.</summary>
    ForceDirected,

    /// <summary>Top-to-bottom hierarchical layout following data flow.</summary>
    Hierarchical
}
```

### 10. RouteDebugger

```csharp
namespace JEngine.JUI.Editor;

/// <summary>
/// EditorWindow that displays the navigation stack, screen history, and
/// current route. Provides interactive buttons to push, pop, and replace
/// routes for testing navigation flows without writing code.
/// </summary>
public sealed class RouteDebugger : EditorWindow
{
    /// <summary>Opens the window from the menu: JUI > Debug > Routes.</summary>
    [MenuItem("JUI/Debug/Routes")]
    public static void ShowWindow();

    /// <summary>The current active route path.</summary>
    public string CurrentRoute { get; }

    /// <summary>The full navigation stack (bottom to top).</summary>
    public IReadOnlyList<string> NavigationStack { get; }

    /// <summary>History of all route changes since Play mode started.</summary>
    public IReadOnlyList<RouteHistoryEntry> History { get; }

    /// <summary>Push a route onto the navigation stack (for testing).</summary>
    public void PushRoute(string route);

    /// <summary>Pop the top route from the navigation stack (for testing).</summary>
    public void PopRoute();

    /// <summary>Replace the current route (for testing).</summary>
    public void ReplaceRoute(string route);
}

/// <summary>An entry in the route history log.</summary>
public readonly struct RouteHistoryEntry
{
    /// <summary>The route path.</summary>
    public string Route { get; }

    /// <summary>The navigation action (Push, Pop, Replace).</summary>
    public string Action { get; }

    /// <summary>Timestamp of the navigation event.</summary>
    public float Timestamp { get; }
}
```

### 11. AudioDebugger

```csharp
namespace JEngine.JUI.Editor;

/// <summary>
/// EditorWindow that lists all audio triggers registered with UIAudioManager.
/// Shows clip name, play count, last-play time, debounce state, and
/// volume/mute status. Includes a preview button to play clips in the editor.
/// </summary>
public sealed class AudioDebugger : EditorWindow
{
    /// <summary>Opens the window from the menu: JUI > Debug > Audio.</summary>
    [MenuItem("JUI/Debug/Audio")]
    public static void ShowWindow();

    /// <summary>Total number of registered audio triggers.</summary>
    public int TriggerCount { get; }

    /// <summary>Total number of audio plays since Play mode started.</summary>
    public int TotalPlayCount { get; }

    /// <summary>Preview an audio clip in the editor without entering Play mode.</summary>
    /// <param name="clipName">The name of the clip to preview.</param>
    public void PreviewClip(string clipName);
}
```

### 12. ShaderEffectPreview

```csharp
namespace JEngine.JUI.Editor;

/// <summary>
/// EditorWindow that previews UIEffect shaders on selected elements in the
/// editor. Provides sliders for intensity, color pickers for tint, and
/// real-time preview of shader parameters without entering Play mode.
/// </summary>
/// <remarks>
/// The preview uses a temporary RenderTexture and Material from the
/// UIEffectManager pools. Changes are not persisted -- they are for
/// visual exploration only.
/// </remarks>
public sealed class ShaderEffectPreview : EditorWindow
{
    /// <summary>Opens the window from the menu: JUI > Debug > Shader Effects.</summary>
    [MenuItem("JUI/Debug/Shader Effects")]
    public static void ShowWindow();

    /// <summary>The element currently being previewed.</summary>
    public VisualElement TargetElement { get; set; }

    /// <summary>The shader effect type being previewed.</summary>
    public UIEffectType EffectType { get; set; }

    /// <summary>Current intensity parameter value.</summary>
    public float Intensity { get; set; }

    /// <summary>Current tint color.</summary>
    public Color TintColor { get; set; }
}
```

## Data Structures

| Type | Purpose |
|------|---------|
| `JUIMetrics` | Static class collecting per-frame counters: effect runs, binding updates, event publishes, signal changes, alloc bytes, FPS. Reset at frame start by JUIManager. |
| `RouteHistoryEntry` | Readonly struct logging each navigation event with route, action type, and timestamp. |
| `GraphLayout` | Enum for signal graph layout algorithms: Auto, ForceDirected, Hierarchical. |
| `ComponentTreeNode` | Internal tree data structure used by ComponentDebugger. Each node holds a reference to a Component, its children, and display metadata (signal count, dirty state). |
| `BindingDebugEntry` | Internal struct holding a snapshot of a binding's state for display: source name, target name, mode, current value, last-update time, is-stale flag. |
| `EffectDebugEntry` | Internal struct holding effect metadata: owning component, dependency signal names, run count, is-dirty, condition expression, is-conditional-active. |
| `GestureDebugEntry` | Internal struct holding recognizer state: type, target element name, current state (Idle/Recognizing/Fired), threshold values, elapsed time. |

## Implementation Notes

### EditorWindow Base Pattern

All debugger windows follow a common pattern:

```csharp
public sealed class ExampleDebugger : EditorWindow
{
    [MenuItem("JUI/Debug/Example")]
    public static void ShowWindow()
    {
        GetWindow<ExampleDebugger>("JUI Example").Show();
    }

    private void OnEnable()
    {
        // Subscribe to runtime state changes
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    }

    private void OnGUI() // or CreateGUI for UIToolkit-based windows
    {
        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play mode to debug JUI state.", MessageType.Info);
            return;
        }

        if (JUIManager.Instance == null)
        {
            EditorGUILayout.HelpBox("JUIManager not found in scene.", MessageType.Warning);
            return;
        }

        DrawDebugUI();
    }

    private void Update()
    {
        // Force repaint every frame during Play mode for live data
        if (EditorApplication.isPlaying)
            Repaint();
    }
}
```

### UIPreviewWindow Architecture

The preview window operates without Play mode by:

1. Creating a temporary `UIDocument` in the scene (hidden from Hierarchy).
2. Creating a temporary `ProviderScope` with mock providers.
3. Instantiating the component and mounting it into the temporary UIDocument.
4. Rendering the UIDocument into the EditorWindow via `rootVisualElement`.
5. Watching for script recompilation (`AssemblyReloadEvents`) and re-mounting on change.

Limitations:
- Components that depend on runtime-only services (like scene references) need mock providers.
- Async operations (UniTask) are not supported in edit mode.
- Gestures and input-driven behaviors cannot be tested in preview.

### SignalGraph Layout

The signal graph uses three layout algorithms:

1. **Auto**: Chooses ForceDirected for < 50 nodes, Hierarchical for >= 50.
2. **ForceDirected**: Physics-based simulation where connected nodes attract and disconnected nodes repel. Converges after ~100 iterations. Interactive -- nodes can be dragged.
3. **Hierarchical**: Topological sort places signals at the top, computeds in the middle, and effects at the bottom. Edges flow downward. Best for understanding data flow.

### PerformanceOverlay Architecture

The overlay runs as a JUI component in `UILayer.System` (sort order 500, always on top). It reads from `JUIMetrics` static properties that are updated by the subsystems themselves:

- `Batch.FlushPending()` increments `JUIMetrics.SignalChangesThisFrame`
- `EffectRunner.RunDirtyEffects()` increments `JUIMetrics.EffectsRanThisFrame`
- `BindingEngine.Tick()` increments `JUIMetrics.BindingsUpdatedThisFrame`
- Each event's `Publish()` increments `JUIMetrics.EventsPublishedThisFrame`

`JUIMetrics.ResetFrameCounters()` is called at the very start of `JUIManager.Update()`, before Phase 1, so the counters reflect the current frame's work.

### ThemeEditor Live Updates

The ThemeEditor modifies USS custom property values directly on the runtime `PanelSettings`:

```csharp
// When a token slider changes:
private void OnTokenChanged(string ussVarName, StyleValue newValue)
{
    // Apply to all active panels
    foreach (var doc in JUIManager.Instance.ActiveDocuments)
    {
        doc.rootVisualElement.style.SetCustomProperty(ussVarName, newValue);
    }
}
```

Changes propagate immediately to all mounted components because USS custom properties are inherited. This enables real-time theme tweaking without recompilation.

## Source Generator Notes

The developer tooling does not use source generators. All tools are standard EditorWindows that inspect runtime state via reflection and direct API access. The tools read from:

- `JUIManager.Instance.MountedComponents` for component tree data
- `EffectRunner` internal state for effect debugging (accessed via `InternalsVisibleTo`)
- `Batch` internal state for signal change tracking
- `BindingEngine` internal state for binding inspection
- `GestureEngine` internal state for recognizer state
- `FocusNavigator` internal state for focus ring data
- `ScreenRouter` for navigation stack
- `UIAudioManager` for audio trigger data
- `UIEffectManager` for shader effect state

The runtime assembly (`JEngine.JUI`) exposes internals to the editor assembly via:

```csharp
[assembly: InternalsVisibleTo("JEngine.JUI.Editor")]
```

## Usage Examples

### Debugging a Binding Issue

```
1. Enter Play mode
2. Open JUI > Debug > Bindings
3. Find the binding for "health-bar" element
4. Check the "Source Signal" column -- verify it shows the correct signal
5. Check "Current Value" -- verify it matches expected state
6. If "Stale" is highlighted red, the source signal was disposed prematurely
7. Click the binding row to jump to the source code location
```

### Debugging an Effect That Does Not Fire

```
1. Enter Play mode
2. Open JUI > Debug > Effects
3. Find the effect by owning component name
4. Check "Is Dirty" column -- if false, the dependency signal did not change
5. Click the effect to see its dependency list
6. Check each dependency signal's "Version" -- did it increment?
7. If the effect has a "When" condition, check "Condition Active" column
8. Open JUI > Debug > Signal Graph to visualize the dependency chain
```

### Live Theme Tweaking

```
1. Enter Play mode
2. Open JUI > Debug > Theme Editor
3. Expand "Color" section
4. Adjust "--jui-color-primary" color picker
5. All mounted components update in real-time
6. Click "Save As New Theme" to persist the changes as a ThemeAsset
```

### Testing Navigation Flows

```
1. Enter Play mode
2. Open JUI > Debug > Routes
3. Current route and navigation stack are displayed
4. Type "inventory" in the "Push Route" field and click "Push"
5. The inventory screen appears and the stack updates
6. Click "Pop" to go back
7. Check "History" tab to see the full navigation log
```

### Previewing a Component Without Play Mode

```
1. Open JUI > Debug > UI Preview
2. In the component selector, choose "PlayerHUD"
3. The HUD renders in the preview panel
4. Adjust signal values in the right-side inspector
5. Change PreviewTheme to test dark/light themes
6. Modify the component script -- preview auto-reloads on recompile
```

### Performance Profiling

```
1. Enter Play mode
2. Press F3 (or configured toggle key) to show the performance overlay
3. Monitor "Effects/frame" -- should be stable, not climbing
4. Monitor "Alloc/frame" -- should be 0B for steady-state UI
5. If allocations appear, check which subsystem is allocating
6. Open JUI > Debug > Performance for configuration and history graphs
```

## Test Plan

### UIPreviewWindow Tests

| # | Test | Expectation |
|---|------|-------------|
| 1 | Open preview window | Window opens without errors |
| 2 | Preview a component | Component renders in the preview panel |
| 3 | Change PreviewTheme | Component re-renders with new theme tokens |
| 4 | Script recompilation | Preview auto-reloads the component |
| 5 | Preview with no JUIManager | Window shows info message, no errors |

### ComponentDebugger Tests

| # | Test | Expectation |
|---|------|-------------|
| 6 | Open during Play mode | Tree view populated with mounted components |
| 7 | Mount new component | Tree view updates to include new node |
| 8 | Unmount component | Tree view removes the node |
| 9 | Select node | Right panel shows component details (signals, children, scope) |
| 10 | AutoTrackUpdates enabled | Tree auto-scrolls to re-rendered components |

### BindingDebugger Tests

| # | Test | Expectation |
|---|------|-------------|
| 11 | Open during Play mode | All active bindings listed |
| 12 | Signal value changes | "Current Value" column updates in real-time |
| 13 | Dispose source signal | Binding row highlighted as stale (red) |
| 14 | Filter by component | Only bindings for selected component shown |
| 15 | Filter by mode | Only Push/Pull/Sync bindings shown as selected |

### EffectDebugger Tests

| # | Test | Expectation |
|---|------|-------------|
| 16 | Open during Play mode | All effects listed with run counts |
| 17 | Signal change triggers effect | Effect row highlighted, run count increments |
| 18 | Click effect row | Dependency list shown (signal names and versions) |
| 19 | ShowDirtyOnly filter | Only dirty effects visible |
| 20 | Conditional effect with false condition | "Condition Active" column shows false |

### PerformanceOverlay Tests

| # | Test | Expectation |
|---|------|-------------|
| 21 | Press toggle key | Overlay appears/disappears |
| 22 | Idle UI | All per-frame counters show 0 |
| 23 | Signal change | "Signals/frame" counter increments |
| 24 | Effect runs | "Effects/frame" counter increments |
| 25 | Alloc tracker | Shows 0B for steady-state, non-zero during initial mount |

### SignalGraph Tests

| # | Test | Expectation |
|---|------|-------------|
| 26 | Open during Play mode | Graph renders with signal, computed, and effect nodes |
| 27 | Signal changes | Active edge highlighted with animation |
| 28 | Filter by component | Only nodes for selected component shown |
| 29 | Layout switch | Graph re-layouts with selected algorithm |

### ThemeEditor Tests

| # | Test | Expectation |
|---|------|-------------|
| 30 | Change color token | All mounted components update immediately |
| 31 | Change spacing token | Layout adjusts in real-time |
| 32 | Save as new theme | ThemeAsset created at specified path |
| 33 | Reset to saved | All tokens revert to saved values |
| 34 | Export to JSON | JSON file matches jui-tokens.json schema |
| 35 | Import from JSON | Tokens updated from imported file |

### RouteDebugger Tests

| # | Test | Expectation |
|---|------|-------------|
| 36 | Open during Play mode | Current route and stack displayed |
| 37 | Push route via button | Screen transitions, stack updates |
| 38 | Pop route via button | Previous screen restored, stack pops |
| 39 | History tab | All navigation events logged with timestamps |

### Other Debugger Tests

| # | Test | Expectation |
|---|------|-------------|
| 40 | FocusDebugger shows overlay | NavMap connections visible on Game view |
| 41 | GestureDebugger lists recognizers | Active recognizers with state shown |
| 42 | AudioDebugger preview clip | Audio clip plays in editor |
| 43 | ShaderEffectPreview intensity slider | Effect updates on target element in real-time |
| 44 | All windows handle Play/Edit mode transition | No exceptions on mode change |
| 45 | All windows handle missing JUIManager | Graceful message, no exceptions |

## Acceptance Criteria

- [ ] All 12 debugger windows are accessible via `JUI > Debug >` menu hierarchy
- [ ] UIPreviewWindow renders components without Play mode using an isolated UIDocument
- [ ] UIPreviewWindow hot-reloads on script recompilation
- [ ] ComponentDebugger shows a live tree view of all mounted components with signal counts and child counts
- [ ] BindingDebugger lists all active bindings with mode, source, target, current value, and stale detection
- [ ] EffectDebugger shows dependency graph, dirty state, run count, and conditional status for all effects
- [ ] FocusDebugger visualizes NavMap connections as a Game view overlay
- [ ] GestureDebugger lists active recognizers with type, target, state, and threshold visualization
- [ ] PerformanceOverlay shows per-frame metrics (effects, bindings, events, signals, allocs, FPS) as an in-game HUD
- [ ] PerformanceOverlay is toggleable via configurable key (default F3)
- [ ] `JUIMetrics` provides accurate per-frame counters reset at the start of each frame
- [ ] ThemeEditor provides live color pickers, spacing sliders, and typography controls that update mounted components in real-time
- [ ] ThemeEditor supports save-as-new-theme, export-to-JSON, and import-from-JSON
- [ ] SignalGraph renders a node-based dependency graph with color-coded nodes and animated active edges
- [ ] SignalGraph supports Auto, ForceDirected, and Hierarchical layout algorithms
- [ ] RouteDebugger shows navigation stack with interactive push/pop/replace buttons and full history log
- [ ] AudioDebugger lists triggers with play counts and supports editor clip preview
- [ ] ShaderEffectPreview provides live parameter adjustment for UIEffect shaders
- [ ] All windows gracefully handle Play/Edit mode transitions and missing JUIManager
- [ ] Runtime assembly exposes internals to editor assembly via `InternalsVisibleTo`
- [ ] All public APIs have XML documentation
