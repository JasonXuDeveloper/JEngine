# JUI Framework v0.6.0 -- Design Documentation Index

## Overview

JUI is a **high-performance reactive UI framework for Unity** built on top of UI Toolkit. It delivers zero-allocation hot paths, source-generated bindings, and a declarative component model that eliminates the need for GameObjects entirely. JUI brings modern frontend paradigms -- reactive signals, computed values, effect batching, and dependency injection -- to Unity UI development while maintaining the performance characteristics required by real-time games.

---

## Key Information

| Property | Value |
|---|---|
| **Unity Version** | 2022.3+ |
| **Language** | C# 9+ |
| **Dependencies** | UI Toolkit, LitMotion, UniTask |
| **Package Location** | `Packages/com.jasonxudeveloper.jengine.ui/Runtime/` |
| **Source Generators** | `SourceGenerators/JEngine.JUI.Generators/` |
| **Version** | 0.6.0 |
| **License** | MIT |

---

## Design Philosophy

JUI is built on five core principles:

1. **Zero GameObjects** -- The entire UI layer runs on UI Toolkit's retained-mode visual tree. No `MonoBehaviour`, no `Transform` hierarchy, no per-frame `Update()` overhead. Components are plain C# classes that own a `VisualElement` subtree.

2. **Reactive by Default** -- All mutable UI state flows through `Signal<T>` and `Computed<T>` primitives. Changes propagate automatically through a topologically-sorted dependency graph, ensuring consistent updates with no manual invalidation.

3. **Pool Everything** -- Visual elements, collections, event args, and string builders are pooled aggressively. The hot path (frame-to-frame updates after initial build) targets zero managed allocations.

4. **Source Gen Everywhere** -- Roslyn source generators eliminate boilerplate for element creation, style application, dependency injection, binding declarations, effect registration, and event wiring. The developer writes near-zero ceremony code; the generator emits the optimal imperative equivalent.

5. **Near-Zero Code** -- The combination of reactive primitives, declarative components, and source generators means that a typical screen can be expressed in a fraction of the code required by raw UI Toolkit, UGUI, or traditional MVVM frameworks.

---

## Table of Contents

| # | File | Section Title | Description |
|---|---|---|---|
| 01 | `01-reactive-primitives.md` | Reactive Primitives: Signal & Computed | Core reactive state atoms, dependency tracking, and lazy evaluation |
| 02 | `02-effect-system.md` | Effect System & Batch | Side-effect scheduling, automatic batching, and flush lifecycle |
| 03 | `03-reactive-collections.md` | Reactive Collections | Observable list, map, and set with fine-grained change notifications |
| 04 | `04-di-container.md` | DI Container & UI Layer Manager | Hierarchical dependency injection and layer/root management |
| 05 | `05-binding-system.md` | Binding System | Declarative property-to-signal bindings with type coercion |
| 06 | `06-component-base.md` | Component Base Class | Lifecycle, element ownership, and the component tree model |
| 07 | `07-control-flow-show-switch.md` | Control Flow: Show & Switch | Conditional rendering and multi-branch switching |
| 08 | `08-control-flow-for-error-portal.md` | Control Flow: For, ErrorBoundary, Portal | List rendering, error isolation, and subtree relocation |
| 08b | `08b-slot-content-projection.md` | Slot / Content Projection System | Named slots, default content, and nested projection |
| 09 | `09-source-generator-setup.md` | Source Generator Project Setup | MSBuild configuration, analyzer references, and incremental generation |
| 10 | `10-element-style-generators.md` | ElementGenerator & StyleGenerator | Source-generated element factories and typed style sheets |
| 11 | `11-inject-binding-generators.md` | InjectGenerator & BindingGenerator | Automatic DI wiring and binding code emission |
| 12 | `12-effect-generator.md` | EffectGenerator | Source-generated effect registration from attributed methods |
| 13 | `13-event-system.md` | Event System (Source-Generated) | Strongly-typed event bus with generated subscribe/dispatch |
| 14 | `14-bridge-system.md` | Bridge System | Interop between JUI components and legacy Unity systems |
| 15 | `15-animation.md` | Animation (LitMotion Integration) | Tween-based animations, transitions, and motion primitives via LitMotion |
| 16 | `16-theming.md` | Theming & Design Tokens | Token-based theming, runtime theme switching, and style variables |
| 17 | `17-rich-text-gestures.md` | Rich Text & Gesture System | Inline markup, tap/swipe/drag gesture recognizers |
| 18 | `18-virtualization.md` | Virtualization Engine | Recycling scroll views for large data sets with minimal allocation |
| 19 | `19-screen-router.md` | Screen Router & Navigation | Stack-based navigation, deep linking, and transition animations |
| 20 | `20-audio-haptics.md` | UI Audio & Haptic Feedback | Interaction sounds, haptic patterns, and feedback scheduling |
| 21 | `21-shaders-vfx.md` | UI Shader & Visual Effects | Custom UI shaders, blur, glow, and particle overlays |
| 22 | `22-focus-navigation-input.md` | Focus, Navigation, Input & Hotkeys | Focus management, gamepad navigation, and hotkey bindings |
| 23 | `23-localization.md` | Localization | Reactive locale switching, pluralization, and string tables |
| 24 | `24-platform-mobile-a11y.md` | Platform, Mobile & Accessibility | Safe areas, notch handling, screen readers, and platform adapters |
| 25 | `25-forms-state-undo.md` | Form Validation, State Persistence & Undo/Redo | Schema validation, serializable state snapshots, and command history |
| 26 | `26-async-swr.md` | Async Integration & Data Caching (SWR) | UniTask-based data fetching, stale-while-revalidate, and retry logic |
| 27 | `27-base-widgets.md` | Base Widgets & Layout Components | Buttons, inputs, toggles, sliders, containers, and layout helpers |
| 28 | `28-game-widgets.md` | Game-Specific Widgets | HUD elements, health bars, inventory grids, minimaps, and chat panels |
| 29 | `29-ai-dev-layer.md` | AI Development Layer | Agent-friendly APIs, introspection hooks, and code generation helpers |
| 30 | `30-jui-manager.md` | JUIManager (Entry Point) | Root initialization, global lifecycle, and system bootstrapping |
| 31 | `31-developer-tooling.md` | Developer Tooling | Live inspector, signal debugger, performance profiler, and hot reload |
| 32 | `32-pooling-performance.md` | Pooling & Performance Polish | Object pools, allocation auditing, and frame budget enforcement |

---

## Dependency Graph

The following ASCII diagram shows how the 32 sections relate to each other. Arrows indicate "depends on" or "builds upon." The graph flows top-to-bottom in three tiers: **Sequential Core** (sections 01--14), **Feature Layers** (sections 15--28), and **Final Sections** (sections 29--32).

```
                        SEQUENTIAL CORE
                        ===============

                     [01 Reactive Primitives]
                              |
                     [02 Effect System & Batch]
                              |
                     [03 Reactive Collections]
                              |
                   [04 DI Container & UI Layer Mgr]
                              |
                     [05 Binding System]
                              |
                     [06 Component Base Class]
                         /         \
          [07 Show & Switch]     [08 For, ErrorBoundary, Portal]
                         \         /
                    [08b Slot / Content Projection]
                              |
                   [09 Source Generator Setup]
                         /         \
          [10 Element & Style Gen]  [11 Inject & Binding Gen]
                         \         /
                     [12 Effect Generator]
                              |
                  [13 Event System (Src-Gen)]
                              |
                     [14 Bridge System]


                        FEATURE LAYERS
                        ==============
         (Each feature layer depends on the Sequential Core above.
          Cross-dependencies between features are noted inline.)

     [15 Animation]          [16 Theming]          [17 Rich Text & Gestures]
          |                       |                         |
          |     +-----------------+-------------------------+
          |     |                 |
     [18 Virtualization]   [19 Screen Router]      [20 Audio & Haptics]
          |                       |                         |
          |     +-----------------+-------------------------+
          |     |                 |
     [21 Shaders & VFX]   [22 Focus, Nav, Input]   [23 Localization]
          |                       |                         |
          |     +-----------------+-------------------------+
          |     |                 |
     [24 Platform & A11y]  [25 Forms, State, Undo]  [26 Async & SWR]
          |                       |                         |
          +-----+-----------------+-------------------------+
                |                 |
         [27 Base Widgets]  [28 Game Widgets]
                \                 /
                 +-------+-------+
                         |

                      FINAL SECTIONS
                      ==============

                  [29 AI Development Layer]
                              |
                  [30 JUIManager (Entry Point)]
                              |
                  [31 Developer Tooling]
                              |
                  [32 Pooling & Performance Polish]
```

### Reading the Graph

- **Sequential Core (01--14):** These sections must be understood in order. Each builds directly on the previous. Section 01 (Signals) is the foundation; section 14 (Bridge) caps the core layer.
- **Feature Layers (15--28):** These sections all depend on the Sequential Core but are largely independent of each other. They can be read in any order, though some have natural affinities (e.g., 15 Animation pairs with 19 Screen Router for transitions; 16 Theming pairs with 27 Base Widgets for styled components).
- **Final Sections (29--32):** These integrate everything above. Section 30 (JUIManager) is the entry point that bootstraps the entire framework. Section 32 (Pooling & Performance) is the final optimization pass applied across all layers.

---

## Section Template Reference

All 32 section documents follow this standard template structure to ensure consistency across the design documentation:

```markdown
# Section Title

> One-line summary of what this section covers.

## 1. Motivation & Goals
Why this system exists, what problems it solves, and design constraints.

## 2. Public API
Full API surface: classes, structs, interfaces, methods, properties,
and delegates. Includes XML doc comments and code snippets.

## 3. Internal Architecture
Implementation details: data structures, algorithms, memory layout,
and threading model. Not user-facing but critical for contributors.

## 4. Source Generator Contract (if applicable)
What the generator emits, what attributes trigger generation,
and what the developer writes vs. what is generated.

## 5. Integration Points
How this system connects to other sections. Cross-references
to dependent and depended-upon sections by number.

## 6. Usage Examples
Practical, copy-pasteable examples covering common scenarios,
edge cases, and anti-patterns to avoid.

## 7. Performance Notes
Allocation profile, CPU cost, batching behavior, and
pooling strategy for this system.

## 8. Open Questions / Future Work
Unresolved design decisions and planned extensions.
```

---

*This document is the authoritative index for the JUI v0.6.0 design specification. All section files reside in the same `docs/JUI/` directory alongside this overview.*
