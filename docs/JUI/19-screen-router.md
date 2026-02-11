# Section 19 — Screen Router & Navigation

## Overview

The Screen Router provides push/pop/replace navigation for full-screen views within a single UI Toolkit panel. It manages a navigation stack, reactive state signals for the current route and stack depth, lifecycle callbacks for entering/leaving screens, async transition support, and deep linking via string route keys.

Screens are `ScreenComponent` subclasses (extending `Component` from Section 6) that declare their route key and transition type via the `[Route]` attribute. The source generator auto-registers all `[Route]`-decorated types at startup.

The router is a static API -- there is one global navigation stack per application. This matches the common pattern in games where there is a single UI layer with one active screen at a time (plus overlays managed separately).

## Dependencies

| Dependency | Section | Purpose |
|---|---|---|
| Component | Section 6 | ScreenComponent extends Component |
| Animation / Transitions | Section 15 | Built-in screen transitions (Fade, Slide, etc.) |

## File Structure

```
Runtime/JUI/
├── Routing/
│   ├── ScreenRouter.cs
│   └── ScreenStack.cs
├── Components/
│   └── ScreenComponent.cs
└── Attributes/
    └── RouteAttribute.cs
```

## API Design

### ScreenRouter

```csharp
/// <summary>
/// Static navigation controller. Manages a stack of screens identified by
/// route keys. Supports push, pop, replace, and deep-link operations with
/// async transitions.
/// </summary>
public static class ScreenRouter
{
    /// <summary>
    /// Pushes a new screen onto the stack. The current screen receives
    /// <see cref="ScreenComponent.OnNavigatedFrom"/> and transitions out.
    /// The new screen is instantiated (or retrieved from cache), receives
    /// <see cref="ScreenComponent.OnNavigatedTo"/>, and transitions in.
    /// </summary>
    /// <param name="routeKey">The route key declared via [Route].</param>
    public static void Push(string routeKey);

    /// <summary>
    /// Pushes a new screen with typed parameters. The screen can retrieve
    /// the params via <see cref="ScreenComponent.GetParams{T}"/>.
    /// </summary>
    public static void Push<TParams>(string routeKey, TParams p);

    /// <summary>
    /// Pops the top screen off the stack. The popped screen receives
    /// <see cref="ScreenComponent.OnNavigatedFrom"/> and transitions out.
    /// The screen below receives <see cref="ScreenComponent.OnNavigatedBack"/>
    /// and transitions in.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the stack has only one screen.</exception>
    public static void Pop();

    /// <summary>
    /// Pops screens until the screen with the given route key is on top.
    /// Intermediate screens receive OnNavigatedFrom but skip transitions.
    /// The target screen receives OnNavigatedBack.
    /// </summary>
    /// <param name="routeKey">The route key to pop back to.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the route key is not in the stack.
    /// </exception>
    public static void PopTo(string routeKey);

    /// <summary>
    /// Replaces the top screen with a new one. The old screen is popped
    /// (without back-navigation) and the new screen is pushed. Useful for
    /// login -> main menu transitions where back should not return to login.
    /// </summary>
    public static void Replace(string routeKey);

    /// <summary>Signal containing the route key of the current (top) screen.</summary>
    public static IReadOnlySignal<string> CurrentRoute { get; }

    /// <summary>Signal containing the current depth of the navigation stack.</summary>
    public static IReadOnlySignal<int> StackDepth { get; }

    /// <summary>Signal that is true when there is more than one screen on the stack.</summary>
    public static IReadOnlySignal<bool> CanGoBack { get; }

    /// <summary>
    /// Returns the full navigation history as a read-only span of route keys,
    /// from bottom (index 0) to top (index Length-1).
    /// </summary>
    public static ReadOnlySpan<string> History { get; }

    /// <summary>
    /// Initializes the router with a root screen. Must be called once at
    /// application startup before any Push/Pop calls.
    /// </summary>
    /// <param name="rootRouteKey">The initial screen's route key.</param>
    /// <param name="container">The VisualElement that hosts screens.</param>
    public static void Initialize(string rootRouteKey, VisualElement container);

    /// <summary>
    /// Registers a route programmatically. Typically not needed when using
    /// [Route] attribute (auto-registered by source generator).
    /// </summary>
    public static void RegisterRoute(string routeKey, Type screenType, IScreenTransition transition = null);

    /// <summary>
    /// Clears the entire navigation stack and pushes the given route as root.
    /// All existing screens receive OnNavigatedFrom.
    /// </summary>
    public static void Reset(string rootRouteKey);
}
```

### ScreenComponent

```csharp
/// <summary>
/// Base class for navigable screens. Extends Component with navigation
/// lifecycle hooks and transition support.
/// </summary>
public abstract class ScreenComponent : Component
{
    /// <summary>
    /// Called when this screen becomes the active (top) screen via Push or Replace.
    /// Override to initialize screen state, start animations, fetch data, etc.
    /// </summary>
    protected virtual void OnNavigatedTo() { }

    /// <summary>
    /// Called when this screen is no longer the active screen (another screen
    /// was pushed on top, or this screen was popped/replaced).
    /// Override to pause updates, save state, etc.
    /// </summary>
    protected virtual void OnNavigatedFrom() { }

    /// <summary>
    /// Called when this screen becomes active again via Pop (the screen above
    /// it was removed). Distinct from OnNavigatedTo to allow different behavior
    /// on "back" vs "forward" navigation.
    /// </summary>
    protected virtual void OnNavigatedBack() { }

    /// <summary>
    /// Async transition played when this screen enters the viewport.
    /// Return default (completed UniTask) for instant transitions.
    /// </summary>
    protected virtual UniTask OnTransitionIn() => default;

    /// <summary>
    /// Async transition played when this screen leaves the viewport.
    /// Return default (completed UniTask) for instant transitions.
    /// </summary>
    protected virtual UniTask OnTransitionOut() => default;

    /// <summary>
    /// Retrieves the navigation parameters passed via
    /// <see cref="ScreenRouter.Push{TParams}"/>.
    /// Returns default(T) if no params were passed or the type does not match.
    /// </summary>
    protected T GetParams<T>();

    /// <summary>
    /// The route key for this screen, as declared by [Route].
    /// </summary>
    public string RouteKey { get; internal set; }
}
```

### RouteAttribute

```csharp
/// <summary>
/// Declares a ScreenComponent subclass as a navigable route. The source
/// generator registers all [Route]-decorated types with ScreenRouter at startup.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RouteAttribute : Attribute
{
    /// <summary>
    /// Creates a route registration for this screen.
    /// </summary>
    /// <param name="key">
    /// Unique route key string. Convention: lowercase-kebab-case (e.g., "main-menu", "settings").
    /// </param>
    public RouteAttribute(string key);

    /// <summary>The route key.</summary>
    public string Key { get; }

    /// <summary>
    /// Optional transition type. Must implement <see cref="IScreenTransition"/>.
    /// Default: null (uses <see cref="InstantTransition"/>).
    /// </summary>
    public Type Transition { get; set; }

    /// <summary>
    /// If true, the screen instance is cached after first creation and reused
    /// on subsequent navigations. Default: true.
    /// </summary>
    public bool Cache { get; set; } = true;
}
```

### IScreenTransition

```csharp
/// <summary>
/// Defines a transition animation between two screens.
/// </summary>
public interface IScreenTransition
{
    /// <summary>
    /// Animates the outgoing screen out and the incoming screen in.
    /// Both elements are present in the container during the transition.
    /// </summary>
    /// <param name="outgoing">The screen leaving (may be null on initial push).</param>
    /// <param name="incoming">The screen entering.</param>
    /// <param name="direction">Whether this is a forward (Push) or backward (Pop) navigation.</param>
    UniTask Animate(VisualElement outgoing, VisualElement incoming, NavigationDirection direction);
}

public enum NavigationDirection { Forward, Backward }
```

### Built-in Transitions

```csharp
/// <summary>No animation, instant swap.</summary>
public class InstantTransition : IScreenTransition { }

/// <summary>Cross-fade between screens over a configurable duration.</summary>
public class FadeTransition : IScreenTransition
{
    public float Duration { get; set; } = 0.3f;
}

/// <summary>Slide incoming screen from the edge. Direction reverses on Pop.</summary>
public class SlideTransition : IScreenTransition
{
    public float Duration { get; set; } = 0.35f;
    public SlideDirection Direction { get; set; } = SlideDirection.Left;
}

/// <summary>Dissolve effect using opacity and scale.</summary>
public class DissolveTransition : IScreenTransition
{
    public float Duration { get; set; } = 0.4f;
}

/// <summary>Circular reveal expanding from a point.</summary>
public class CircleRevealTransition : IScreenTransition
{
    public float Duration { get; set; } = 0.5f;
    public Vector2 Origin { get; set; } = new(0.5f, 0.5f); // Normalized
}

public enum SlideDirection { Left, Right, Up, Down }
```

## Data Structures

### ScreenStack

```
┌───────────────────────────────┐
│         ScreenStack           │
│                               │
│  _entries: List<StackEntry>   │
│                               │
│  [0] "splash"     (bottom)    │
│  [1] "main-menu"             │
│  [2] "settings"              │
│  [3] "keybinds"   (top) ◄──  │ CurrentRoute
│                               │
└───────────────────────────────┘
```

```csharp
internal struct StackEntry
{
    public string RouteKey;
    public ScreenComponent Instance;
    public object Params;           // Boxed TParams (nullable)
    public IScreenTransition Transition;
}
```

The stack uses a `List<StackEntry>` rather than `Stack<T>` because `PopTo` needs indexed access to find the target entry. The list is never larger than ~10 entries in practice.

### Route Registry

```csharp
internal static class RouteRegistry
{
    // Populated by source generator at [RuntimeInitializeOnLoadMethod]
    static readonly Dictionary<string, RouteRegistration> _routes;

    internal struct RouteRegistration
    {
        public Type ScreenType;
        public IScreenTransition DefaultTransition;
        public bool Cache;
    }
}
```

### Screen Cache

```csharp
// Keyed by route key. Only populated for [Route(Cache = true)] screens.
static readonly Dictionary<string, ScreenComponent> _cache;
```

Cached screens skip `Render()` on subsequent navigations. They receive `OnNavigatedTo` / `OnNavigatedBack` but their visual tree is reused.

## Implementation Notes

### Navigation Flow (Push)

```
Push("settings")
  │
  ├─ 1. Look up "settings" in RouteRegistry
  │
  ├─ 2. Check _cache for existing instance
  │     ├─ Cache hit:  reuse instance
  │     └─ Cache miss: instantiate ScreenComponent, call Render()
  │
  ├─ 3. Set params on new instance (if Push<TParams> was used)
  │
  ├─ 4. Call OnNavigatedFrom() on current top screen
  │
  ├─ 5. Add incoming screen element to container
  │
  ├─ 6. Run transition:
  │     await transition.Animate(outgoing, incoming, Forward)
  │
  ├─ 7. Remove outgoing screen element from container (kept in stack)
  │
  ├─ 8. Call OnNavigatedTo() on new screen
  │
  ├─ 9. Push StackEntry onto _entries
  │
  └─ 10. Update signals: CurrentRoute, StackDepth, CanGoBack
```

### Navigation Flow (Pop)

```
Pop()
  │
  ├─ 1. Guard: StackDepth > 1, else throw
  │
  ├─ 2. Remove top StackEntry from _entries
  │
  ├─ 3. Call OnNavigatedFrom() on popped screen
  │
  ├─ 4. Add previous screen element back to container
  │
  ├─ 5. Run transition:
  │     await transition.Animate(outgoing, incoming, Backward)
  │
  ├─ 6. Remove popped screen element from container
  │     ├─ Cached: keep instance in _cache
  │     └─ Not cached: call Dispose() on the screen
  │
  ├─ 7. Call OnNavigatedBack() on now-active screen
  │
  └─ 8. Update signals
```

### PopTo Optimization

`PopTo("main-menu")` pops all screens above "main-menu" in one operation. Intermediate screens receive `OnNavigatedFrom` but only the final transition (from current top to target) is animated. This avoids playing N sequential transitions.

### Replace Semantics

`Replace("main-menu")` is equivalent to:
1. `Pop()` the current screen (no transition out).
2. `Push("main-menu")` the new screen (transition in only).

The popped screen does not receive `OnNavigatedBack` because it is being replaced, not returned to. This is important for flows like splash -> login -> main-menu where the user should never navigate back to login.

### Deep Linking

Deep linking is supported by calling `Push` with a series of route keys to build up the desired stack:

```csharp
ScreenRouter.Initialize("main-menu", container);
ScreenRouter.Push("settings");
ScreenRouter.Push("keybinds");
// Stack is now: main-menu -> settings -> keybinds
// User can Pop back through the full chain
```

A convenience method may be added in a future iteration:

```csharp
ScreenRouter.DeepLink("main-menu", "settings", "keybinds");
```

### Transition Concurrency

Only one transition runs at a time. If `Push` or `Pop` is called while a transition is in progress, the new navigation is queued and executed after the current transition completes. This prevents visual corruption from overlapping animations.

Internally, a `UniTask`-based queue serializes navigation requests:

```csharp
static readonly Channel<NavigationRequest> _navQueue = Channel.CreateUnbounded<NavigationRequest>();
```

### Signal Updates

`CurrentRoute`, `StackDepth`, and `CanGoBack` are updated synchronously before the transition begins. This ensures that UI elements bound to these signals (e.g., a back button's visibility) update immediately, even though the visual transition is still animating.

## Source Generator Notes

### Route Auto-Registration

The source generator scans for all classes decorated with `[Route]` and emits a registration method:

```csharp
// Generated: RouteRegistration.g.cs
internal static class __GeneratedRouteRegistration
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Register()
    {
        ScreenRouter.RegisterRoute("main-menu", typeof(MainMenuScreen),
            new FadeTransition(), cache: true);
        ScreenRouter.RegisterRoute("settings", typeof(SettingsScreen),
            new SlideTransition(), cache: true);
        ScreenRouter.RegisterRoute("keybinds", typeof(KeybindsScreen),
            new InstantTransition(), cache: false);
    }
}
```

### Diagnostics

| ID | Severity | Message |
|---|---|---|
| JUI500 | Error | Duplicate route key "{key}" declared on {TypeA} and {TypeB} |
| JUI501 | Error | `[Route]` class must extend ScreenComponent |
| JUI502 | Error | `[Route]` Transition type must implement IScreenTransition |
| JUI503 | Warning | `[Route]` class is not partial; generated registration may be limited |

## Usage Examples

### Defining Screens

```csharp
[Route("main-menu", Transition = typeof(FadeTransition))]
public partial class MainMenuScreen : ScreenComponent
{
    protected override VisualElement Render()
    {
        var root = new VisualElement();
        root.AddToClassList("main-menu");

        var playBtn = new Button(() => ScreenRouter.Push("level-select"))
        {
            text = "Play"
        };
        var settingsBtn = new Button(() => ScreenRouter.Push("settings"))
        {
            text = "Settings"
        };

        root.Add(playBtn);
        root.Add(settingsBtn);
        return root;
    }

    protected override void OnNavigatedTo()
    {
        MusicManager.Play("menu-theme");
    }

    protected override void OnNavigatedBack()
    {
        // Returned from settings or level-select
        MusicManager.Play("menu-theme");
    }
}
```

### Passing Parameters

```csharp
// Pushing with params
ScreenRouter.Push<LevelSelectParams>("level-detail", new LevelSelectParams
{
    LevelId = 42,
    Difficulty = Difficulty.Hard
});

// Receiving params
[Route("level-detail", Transition = typeof(SlideTransition))]
public partial class LevelDetailScreen : ScreenComponent
{
    protected override void OnNavigatedTo()
    {
        var p = GetParams<LevelSelectParams>();
        LoadLevel(p.LevelId, p.Difficulty);
    }
}

public struct LevelSelectParams
{
    public int LevelId;
    public Difficulty Difficulty;
}
```

### Back Button Binding

```csharp
public partial class HeaderBar : Component
{
    protected override VisualElement Render()
    {
        var backBtn = new Button(() => ScreenRouter.Pop())
        {
            text = "Back"
        };

        // Reactively show/hide based on stack depth
        Bind(ScreenRouter.CanGoBack, canGoBack =>
        {
            backBtn.style.display = canGoBack
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        });

        return backBtn;
    }
}
```

### Custom Transition

```csharp
public class WipeTransition : IScreenTransition
{
    public float Duration { get; set; } = 0.6f;

    public async UniTask Animate(
        VisualElement outgoing,
        VisualElement incoming,
        NavigationDirection direction)
    {
        // Incoming starts off-screen to the right
        float startX = direction == NavigationDirection.Forward ? 100f : -100f;
        incoming.style.translate = new Translate(Length.Percent(startX), 0);
        incoming.style.opacity = 1f;

        // Animate both simultaneously
        var tween = new TweenBuilder(Duration, Easing.CubicOut);
        await tween.Run(t =>
        {
            float outX = direction == NavigationDirection.Forward ? -100f * t : 100f * t;
            outgoing.style.translate = new Translate(Length.Percent(outX), 0);
            incoming.style.translate = new Translate(Length.Percent(startX * (1f - t)), 0);
        });

        // Cleanup
        outgoing.style.translate = StyleKeyword.Null;
        incoming.style.translate = StyleKeyword.Null;
    }
}
```

### Application Bootstrap

```csharp
public class GameUI : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;

    void Start()
    {
        var root = _uiDocument.rootVisualElement;
        var container = new VisualElement();
        container.style.flexGrow = 1;
        root.Add(container);

        // Initialize router with the splash screen as root
        ScreenRouter.Initialize("splash", container);
    }
}
```

### Replace for Auth Flow

```csharp
[Route("login")]
public partial class LoginScreen : ScreenComponent
{
    private async void OnLoginSuccess()
    {
        // Replace so the user cannot navigate back to login
        ScreenRouter.Replace("main-menu");
    }
}
```

## Test Plan

### ScreenRouter Core Tests

| # | Test | Expectation |
|---|---|---|
| 1 | `Initialize("root", container)` | CurrentRoute == "root", StackDepth == 1, CanGoBack == false |
| 2 | `Push("a")` | CurrentRoute == "a", StackDepth == 2, CanGoBack == true |
| 3 | `Pop()` from depth 2 | CurrentRoute == "root", StackDepth == 1 |
| 4 | `Pop()` from depth 1 | Throws InvalidOperationException |
| 5 | `Push<TParams>("a", params)` then `GetParams<T>()` | Returns correct params |
| 6 | `Replace("b")` from ["root", "a"] | Stack is ["root", "b"], CanGoBack == true |
| 7 | `PopTo("root")` from ["root", "a", "b", "c"] | Stack is ["root"], CanGoBack == false |
| 8 | `PopTo("x")` where "x" not in stack | Throws InvalidOperationException |
| 9 | `Reset("new-root")` | Stack is ["new-root"], StackDepth == 1 |
| 10 | `History` returns correct span | Bottom-to-top order of route keys |

### Lifecycle Callback Tests

| # | Test | Expectation |
|---|---|---|
| 11 | Push screen A then B | A.OnNavigatedFrom called, B.OnNavigatedTo called |
| 12 | Pop screen B | B.OnNavigatedFrom called, A.OnNavigatedBack called |
| 13 | Replace A with C | A.OnNavigatedFrom called (not OnNavigatedBack), C.OnNavigatedTo called |
| 14 | PopTo skipping intermediates | All skipped screens receive OnNavigatedFrom |
| 15 | Push same route twice | Two separate instances (or same cached instance), lifecycle called correctly |

### Transition Tests

| # | Test | Expectation |
|---|---|---|
| 16 | Push with FadeTransition | Both elements in container during transition, opacity animates |
| 17 | Pop with SlideTransition | Direction is Backward, slide direction reverses |
| 18 | InstantTransition | No async delay, immediate swap |
| 19 | Rapid Push during transition | Second push queued, executes after first completes |
| 20 | Push then immediate Pop | Both transitions run in sequence, no corruption |

### Caching Tests

| # | Test | Expectation |
|---|---|---|
| 21 | Push cached screen, pop, push again | Same instance reused, Render() called once |
| 22 | Push non-cached screen, pop, push again | New instance created, Render() called twice |
| 23 | Reset clears cache | Previously cached screens are disposed |

### Signal Reactivity Tests

| # | Test | Expectation |
|---|---|---|
| 24 | Bind UI to CanGoBack | Back button hides when depth == 1, shows when depth > 1 |
| 25 | Bind UI to CurrentRoute | Label updates to current route key on each navigation |

### Source Generator Tests

| # | Test | Expectation |
|---|---|---|
| 26 | Two classes with same route key | JUI500 diagnostic |
| 27 | `[Route]` on non-ScreenComponent class | JUI501 diagnostic |
| 28 | `[Route(Transition = typeof(string))]` | JUI502 diagnostic |

## Acceptance Criteria

- [ ] `ScreenRouter` provides static `Push`, `Pop`, `PopTo`, `Replace`, and `Reset` methods
- [ ] `Push<TParams>` passes parameters retrievable via `ScreenComponent.GetParams<T>()`
- [ ] `Pop()` throws `InvalidOperationException` when stack depth is 1
- [ ] `PopTo` pops all screens above the target, calling `OnNavigatedFrom` on each
- [ ] `Replace` removes the current screen without triggering back-navigation callbacks
- [ ] `CurrentRoute`, `StackDepth`, and `CanGoBack` are reactive signals updated on each navigation
- [ ] `History` returns a `ReadOnlySpan<string>` of the full stack from bottom to top
- [ ] `ScreenComponent` provides `OnNavigatedTo`, `OnNavigatedFrom`, `OnNavigatedBack` lifecycle hooks
- [ ] `OnTransitionIn` and `OnTransitionOut` support async transitions via `UniTask`
- [ ] Built-in transitions: `InstantTransition`, `FadeTransition`, `SlideTransition`, `DissolveTransition`, `CircleRevealTransition`
- [ ] `IScreenTransition.Animate` receives `NavigationDirection` (Forward/Backward) to reverse animations on Pop
- [ ] `[Route]` attribute declares route key, transition type, and cache policy
- [ ] `[Route(Cache = true)]` (default) reuses screen instances across navigations
- [ ] Source generator auto-registers all `[Route]`-decorated types via `[RuntimeInitializeOnLoadMethod]`
- [ ] Navigation requests are queued when a transition is in progress (no concurrent transitions)
- [ ] Signals update synchronously before the transition begins
- [ ] Diagnostics JUI500-JUI503 are emitted for invalid `[Route]` configurations
- [ ] Non-cached screens are disposed when popped
- [ ] `Initialize` must be called before any navigation; calling Push/Pop before Initialize throws
