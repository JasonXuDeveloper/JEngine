# Section 20 — UI Audio & Haptic Feedback

## Overview

This section defines a declarative audio and haptic feedback system for JUI components. UI sounds are configured via `UIAudioBank` ScriptableObjects and triggered either imperatively through `UIAudioManager.Play()` or declaratively via the `[UIAudio]` source-generator attribute. Haptic feedback follows the same dual pattern with `HapticFeedback` static methods and the `[UIHaptic]` attribute.

Key design goals:

- **Declarative first**: Annotate component methods with `[UIAudio]` and `[UIHaptic]` to wire sounds and haptics to element events without manual callback registration.
- **Debounced playback**: `UIAudioManager` prevents the same clip from playing multiple times within a configurable cooldown window, avoiding audio stacking on rapid interactions.
- **Platform-aware haptics**: `HapticFeedback` delegates to an `IHapticProvider` interface. Mobile platforms use `Handheld.Vibrate()` or platform-specific APIs. Desktop is a no-op by default but extensible for gamepad rumble.
- **Reactive volume**: Master volume and mute state are `Signal<T>` values, so any UI bound to them updates automatically.

## Dependencies

| Dependency | Section | Purpose |
|---|---|---|
| Component | Section 6 | `[UIAudio]` and `[UIHaptic]` target Component subclasses |
| Generator Setup | Section 9 | Source generator wiring for declarative attributes |

## File Structure

```
Runtime/JUI/
├── Audio/
│   ├── UIAudioManager.cs
│   ├── UIAudioBank.cs
│   ├── UIAudioTrigger.cs
│   ├── HapticFeedback.cs
│   ├── HapticPattern.cs
│   └── IHapticProvider.cs
└── Attributes/
    ├── UIAudioAttribute.cs
    └── UIHapticAttribute.cs
```

## API Design

### UIAudioManager

```csharp
/// <summary>
/// Central manager for UI sound playback. Handles clip registry, volume
/// control, debounce, and AudioSource pooling.
/// </summary>
public static class UIAudioManager
{
    /// <summary>
    /// Master volume for all UI audio. Range [0, 1]. Bound as a signal
    /// so UI sliders can bind to it reactively.
    /// </summary>
    public static Signal<float> MasterVolume { get; }

    /// <summary>
    /// Global mute toggle. When true, <see cref="Play"/> calls are silently
    /// ignored. The signal enables reactive binding to a mute toggle button.
    /// </summary>
    public static Signal<bool> Muted { get; }

    /// <summary>
    /// Plays a registered audio clip by its string ID.
    /// Subject to debounce: if the same clipId was played within the
    /// cooldown window (default 50ms), the call is silently dropped.
    /// </summary>
    /// <param name="clipId">The clip identifier as defined in a UIAudioBank.</param>
    /// <param name="volumeScale">
    /// Per-call volume multiplier applied on top of MasterVolume and the
    /// clip's base volume. Default 1.0.
    /// </param>
    public static void Play(string clipId, float volumeScale = 1f);

    /// <summary>
    /// Registers all clips from an audio bank. Duplicate IDs overwrite
    /// previously registered clips (last-write-wins).
    /// </summary>
    public static void RegisterClips(UIAudioBank bank);

    /// <summary>
    /// Unregisters all clips from the given bank.
    /// </summary>
    public static void UnregisterClips(UIAudioBank bank);

    /// <summary>
    /// Sets the global debounce cooldown. Clips played within this window
    /// of a previous play of the same ID are dropped. Default: 50ms.
    /// </summary>
    public static float DebounceCooldown { get; set; }

    /// <summary>
    /// Called once per frame by the JUI runtime to advance debounce timers
    /// and return finished AudioSources to the pool.
    /// </summary>
    internal static void Tick();
}
```

### UIAudioBank

```csharp
/// <summary>
/// ScriptableObject asset that defines a collection of named audio clips
/// for UI feedback. Create via Assets > Create > JUI > Audio Bank.
/// </summary>
[CreateAssetMenu(fileName = "UIAudioBank", menuName = "JUI/Audio Bank", order = 100)]
public class UIAudioBank : ScriptableObject
{
    /// <summary>A single clip entry in the bank.</summary>
    [Serializable]
    public struct Entry
    {
        /// <summary>Unique string identifier (e.g., "ui_click", "ui_hover").</summary>
        public string Id;

        /// <summary>The AudioClip asset.</summary>
        public AudioClip Clip;

        /// <summary>Base volume for this clip. Range [0, 1]. Default 1.</summary>
        [Range(0f, 1f)]
        public float Volume;
    }

    /// <summary>All clip entries in this bank.</summary>
    public Entry[] Clips;
}
```

### UIAudioTrigger

```csharp
/// <summary>
/// Defines which UI Toolkit event triggers audio playback.
/// </summary>
public enum UIAudioTrigger
{
    /// <summary>ClickEvent on the target element.</summary>
    Click,

    /// <summary>PointerEnterEvent on the target element.</summary>
    Hover,

    /// <summary>PointerLeaveEvent on the target element.</summary>
    HoverExit,

    /// <summary>FocusInEvent on the target element.</summary>
    Focus,

    /// <summary>ChangeEvent (any type) on the target element.</summary>
    ValueChanged,

    /// <summary>AttachToPanelEvent (element becomes visible).</summary>
    Show,

    /// <summary>DetachFromPanelEvent (element is removed).</summary>
    Hide,

    /// <summary>Custom drop event (for drag-and-drop scenarios).</summary>
    Drop,

    /// <summary>Triggered programmatically to indicate an error state.</summary>
    Error,

    /// <summary>Triggered programmatically to indicate a success state.</summary>
    Success
}
```

### UIAudioAttribute

```csharp
/// <summary>
/// Source-generator attribute. Wires audio playback to a UI event on a
/// named element. Applied at the class level (can be stacked multiple times).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class UIAudioAttribute : Attribute
{
    /// <summary>
    /// Declares that the specified clip should play when the trigger fires
    /// on the named element.
    /// </summary>
    /// <param name="elementName">
    /// Name of the element field (from [El] attribute), using nameof().
    /// </param>
    public UIAudioAttribute(string elementName);

    /// <summary>The element name.</summary>
    public string ElementName { get; }

    /// <summary>Which event triggers playback. Default: Click.</summary>
    public UIAudioTrigger Trigger { get; set; } = UIAudioTrigger.Click;

    /// <summary>The clip ID to play (must be registered in a UIAudioBank).</summary>
    public string Clip { get; set; }

    /// <summary>Per-trigger volume scale. Default: 1.0.</summary>
    public float VolumeScale { get; set; } = 1f;
}
```

### UIHapticAttribute

```csharp
/// <summary>
/// Source-generator attribute. Wires haptic feedback to a UI event on a
/// named element. Can be combined with [UIAudio] for simultaneous audio+haptic.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class UIHapticAttribute : Attribute
{
    /// <param name="elementName">
    /// Name of the element field (from [El] attribute), using nameof().
    /// </param>
    public UIHapticAttribute(string elementName);

    /// <summary>The element name.</summary>
    public string ElementName { get; }

    /// <summary>Which event triggers the haptic. Default: Click.</summary>
    public UIAudioTrigger Trigger { get; set; } = UIAudioTrigger.Click;

    /// <summary>The haptic pattern to play. Default: Light.</summary>
    public HapticPattern Pattern { get; set; } = HapticPattern.Light;
}
```

### HapticFeedback

```csharp
/// <summary>
/// Static API for triggering haptic feedback. Platform-aware: uses native
/// haptic APIs on mobile, no-op on desktop (extensible via IHapticProvider).
/// </summary>
public static class HapticFeedback
{
    /// <summary>Light haptic tap. iOS: UIImpactFeedbackGenerator.Light.</summary>
    public static void Light();

    /// <summary>Medium haptic tap. iOS: UIImpactFeedbackGenerator.Medium.</summary>
    public static void Medium();

    /// <summary>Heavy haptic tap. iOS: UIImpactFeedbackGenerator.Heavy.</summary>
    public static void Heavy();

    /// <summary>Selection feedback. iOS: UISelectionFeedbackGenerator.</summary>
    public static void Selection();

    /// <summary>
    /// Custom haptic with specified duration and intensity.
    /// Falls back to the closest predefined pattern on platforms that
    /// do not support custom haptics.
    /// </summary>
    /// <param name="duration">Duration in seconds.</param>
    /// <param name="intensity">Intensity in range [0, 1].</param>
    public static void Custom(float duration, float intensity);

    /// <summary>
    /// Plays a predefined haptic pattern.
    /// </summary>
    public static void Play(HapticPattern pattern);

    /// <summary>
    /// Globally enable or disable haptic feedback. Disabled by default
    /// on desktop platforms.
    /// </summary>
    public static bool Enabled { get; set; }

    /// <summary>
    /// Registers a custom haptic provider. Replaces the default platform
    /// provider. Useful for gamepad rumble or custom hardware.
    /// </summary>
    public static void SetProvider(IHapticProvider provider);
}
```

### HapticPattern

```csharp
/// <summary>
/// Predefined haptic patterns for common UI interactions.
/// </summary>
public enum HapticPattern
{
    /// <summary>Subtle tap for minor interactions (toggles, selections).</summary>
    Light,

    /// <summary>Standard tap for primary interactions (button press).</summary>
    Medium,

    /// <summary>Strong tap for impactful actions (delete, confirm).</summary>
    Heavy,

    /// <summary>Subtle selection change feedback (scrolling through a picker).</summary>
    Selection,

    /// <summary>Double-pulse pattern indicating success.</summary>
    Success,

    /// <summary>Triple-pulse ascending pattern indicating a warning.</summary>
    Warning,

    /// <summary>Sharp buzz pattern indicating an error.</summary>
    Error
}
```

### IHapticProvider

```csharp
/// <summary>
/// Platform abstraction for haptic feedback. Implement this interface to
/// support custom hardware (gamepad rumble, haptic gloves, etc.).
/// </summary>
public interface IHapticProvider
{
    /// <summary>Whether this provider is available on the current platform.</summary>
    bool IsAvailable { get; }

    /// <summary>Triggers a predefined haptic pattern.</summary>
    void Trigger(HapticPattern pattern);

    /// <summary>Triggers a custom haptic with duration and intensity.</summary>
    void TriggerCustom(float duration, float intensity);
}
```

## Data Structures

### Clip Registry

```csharp
// Internal to UIAudioManager
static readonly Dictionary<string, ClipEntry> _clipRegistry;

struct ClipEntry
{
    public AudioClip Clip;
    public float BaseVolume;
}
```

### Debounce Tracker

```csharp
// Internal to UIAudioManager
static readonly Dictionary<string, float> _lastPlayTime;
// Key: clipId, Value: Time.unscaledTime of last Play() call
```

On each `Play(clipId)` call:
1. Check `_lastPlayTime[clipId]`. If `Time.unscaledTime - lastTime < DebounceCooldown`, drop the call.
2. Otherwise, update `_lastPlayTime[clipId] = Time.unscaledTime` and proceed.

### AudioSource Pool

```csharp
// Internal to UIAudioManager
static readonly Stack<AudioSource> _sourcePool;
static readonly List<AudioSource> _activeSources;
```

- `Play()` rents an `AudioSource` from the pool (or creates one on a hidden `GameObject`).
- `Tick()` checks `_activeSources` for clips that have finished playing and returns them to `_sourcePool`.
- Pool capacity is unbounded but self-limits because UI typically plays 1-3 simultaneous clips.

### Default Haptic Providers

```
┌──────────────────────────────────┐
│ Platform       │ Provider        │
├──────────────────────────────────┤
│ iOS            │ IOSHapticProvider (Handheld.Vibrate + native plugin) │
│ Android        │ AndroidHapticProvider (Handheld.Vibrate + VibrationEffect) │
│ Desktop        │ NoOpHapticProvider  │
│ Custom         │ User-supplied via SetProvider() │
└──────────────────────────────────┘
```

The default provider is selected at startup via `RuntimePlatform` check. `HapticFeedback.Enabled` defaults to `true` on mobile, `false` on desktop.

## Implementation Notes

### AudioSource Management

`UIAudioManager` creates a persistent `GameObject` named `[JUI Audio]` marked `DontDestroyOnLoad`. All pooled `AudioSource` components live on this object. The object is created lazily on the first `Play()` call or `RegisterClips()` call.

```csharp
static GameObject _audioHost;

static void EnsureHost()
{
    if (_audioHost == null)
    {
        _audioHost = new GameObject("[JUI Audio]");
        Object.DontDestroyOnLoad(_audioHost);
        _audioHost.hideFlags = HideFlags.HideAndDontSave;
    }
}
```

### Volume Calculation

Final volume for a `Play(clipId, volumeScale)` call:

```
finalVolume = clipEntry.BaseVolume * volumeScale * MasterVolume.Value
```

If `Muted.Value` is true, the call returns immediately without renting an `AudioSource`.

### Debounce Timing

`DebounceCooldown` uses `Time.unscaledTime` so it works correctly during pause. The default of 50ms prevents audible doubling when a button click fires both a `ClickEvent` and a programmatic `Play()` call in the same frame.

### Declarative Attribute Compilation

`[UIAudio]` and `[UIHaptic]` are class-level attributes (not method-level) because they describe element-event bindings, not method handlers. The source generator reads all attributes on the class and emits event registration in the generated `OnAttach` partial method.

### Haptic Pattern Implementation

For `HapticPattern.Success`, `Warning`, and `Error`, the provider plays a sequence of pulses:

| Pattern | Pulses |
|---|---|
| Success | 2 pulses: Light (20ms pause) Medium |
| Warning | 3 pulses: Light (15ms pause) Light (15ms pause) Medium |
| Error | 1 pulse: Heavy (100ms duration) |

These are implemented as coroutines within the provider, using `UniTask.Delay` for timing.

### Thread Safety

`UIAudioManager.Play()` must be called from the main thread (AudioSource operations require it). A debug assertion guards this. `HapticFeedback` methods also require the main thread on mobile platforms.

## Source Generator Notes

### [UIAudio] and [UIHaptic] Wiring

Given this user code:

```csharp
[UIAudio(nameof(El.ConfirmButton), Trigger = UIAudioTrigger.Click, Clip = "ui_click")]
[UIAudio(nameof(El.ConfirmButton), Trigger = UIAudioTrigger.Hover, Clip = "ui_hover", VolumeScale = 0.5f)]
[UIHaptic(nameof(El.ConfirmButton), Trigger = UIAudioTrigger.Click, Pattern = HapticPattern.Light)]
public partial class ConfirmDialog : Component
{
    // ...
}
```

The generator emits:

```csharp
partial class ConfirmDialog
{
    private EventCallback<ClickEvent> __audio_ConfirmButton_Click;
    private EventCallback<PointerEnterEvent> __audio_ConfirmButton_Hover;
    private EventCallback<ClickEvent> __haptic_ConfirmButton_Click;

    partial void __AttachAudioHaptics()
    {
        __audio_ConfirmButton_Click = _ => UIAudioManager.Play("ui_click", 1f);
        El.ConfirmButton.RegisterCallback(__audio_ConfirmButton_Click);

        __audio_ConfirmButton_Hover = _ => UIAudioManager.Play("ui_hover", 0.5f);
        El.ConfirmButton.RegisterCallback(__audio_ConfirmButton_Hover);

        __haptic_ConfirmButton_Click = _ => HapticFeedback.Play(HapticPattern.Light);
        El.ConfirmButton.RegisterCallback(__haptic_ConfirmButton_Click);
    }

    partial void __DetachAudioHaptics()
    {
        El.ConfirmButton.UnregisterCallback(__audio_ConfirmButton_Click);
        El.ConfirmButton.UnregisterCallback(__audio_ConfirmButton_Hover);
        El.ConfirmButton.UnregisterCallback(__haptic_ConfirmButton_Click);
    }
}
```

### Trigger-to-Event Mapping

| UIAudioTrigger | VisualElement Event |
|---|---|
| Click | `ClickEvent` |
| Hover | `PointerEnterEvent` |
| HoverExit | `PointerLeaveEvent` |
| Focus | `FocusInEvent` |
| ValueChanged | `ChangeEvent<T>` (any T) |
| Show | `AttachToPanelEvent` |
| Hide | `DetachFromPanelEvent` |
| Drop | `DragPerformEvent` |
| Error | N/A (programmatic only) |
| Success | N/A (programmatic only) |

For `Error` and `Success` triggers, the generator does not emit event callbacks. These are intended for programmatic use via `UIAudioManager.Play("error_sound")` and `HapticFeedback.Play(HapticPattern.Error)`.

### Diagnostics

| ID | Severity | Message |
|---|---|---|
| JUI600 | Error | `[UIAudio]` element name does not match any `[El]` field |
| JUI601 | Error | `[UIHaptic]` element name does not match any `[El]` field |
| JUI602 | Warning | `[UIAudio]` Clip property is empty |
| JUI603 | Warning | `[UIAudio]`/`[UIHaptic]` with Trigger = Error or Success has no effect (programmatic only) |
| JUI604 | Error | `[UIAudio]`/`[UIHaptic]` applied to non-Component class |

## Usage Examples

### Setting Up Audio

```csharp
// 1. Create a UIAudioBank asset in the editor (Assets > Create > JUI > Audio Bank)
// 2. Add entries: "ui_click", "ui_hover", "ui_error", "ui_success"
// 3. Register at startup:

public class AudioSetup : MonoBehaviour
{
    [SerializeField] private UIAudioBank _uiBank;

    void Awake()
    {
        UIAudioManager.RegisterClips(_uiBank);
        UIAudioManager.MasterVolume.Value = 0.8f;
    }
}
```

### Declarative Audio + Haptics

```csharp
[UIAudio(nameof(El.PlayButton), Trigger = UIAudioTrigger.Click, Clip = "ui_click")]
[UIAudio(nameof(El.PlayButton), Trigger = UIAudioTrigger.Hover, Clip = "ui_hover", VolumeScale = 0.3f)]
[UIHaptic(nameof(El.PlayButton), Trigger = UIAudioTrigger.Click, Pattern = HapticPattern.Medium)]
[UIAudio(nameof(El.DeleteButton), Trigger = UIAudioTrigger.Click, Clip = "ui_delete")]
[UIHaptic(nameof(El.DeleteButton), Trigger = UIAudioTrigger.Click, Pattern = HapticPattern.Heavy)]
public partial class MainMenu : Component
{
    protected override VisualElement Render()
    {
        // Elements are set up via [El] attribute; audio/haptic wired by generator
        return new VisualElement();
    }
}
```

### Imperative Audio

```csharp
// Play a sound directly
UIAudioManager.Play("ui_click");

// With volume scaling
UIAudioManager.Play("ui_hover", volumeScale: 0.5f);

// Conditional error feedback
if (!isValid)
{
    UIAudioManager.Play("ui_error");
    HapticFeedback.Play(HapticPattern.Error);
}
```

### Reactive Volume Control

```csharp
public partial class SettingsScreen : Component
{
    protected override VisualElement Render()
    {
        var slider = new Slider("UI Volume", 0f, 1f);

        // Two-way bind slider to MasterVolume signal
        Bind(UIAudioManager.MasterVolume, v => slider.value = v);
        slider.RegisterValueChangedCallback(evt =>
            UIAudioManager.MasterVolume.Value = evt.newValue);

        var muteToggle = new Toggle("Mute");
        Bind(UIAudioManager.Muted, m => muteToggle.value = m);
        muteToggle.RegisterValueChangedCallback(evt =>
            UIAudioManager.Muted.Value = evt.newValue);

        var root = new VisualElement();
        root.Add(slider);
        root.Add(muteToggle);
        return root;
    }
}
```

### Custom Haptic Provider (Gamepad Rumble)

```csharp
public class GamepadHapticProvider : IHapticProvider
{
    private readonly Gamepad _gamepad;

    public GamepadHapticProvider(Gamepad gamepad)
    {
        _gamepad = gamepad;
    }

    public bool IsAvailable => _gamepad != null && _gamepad.added;

    public void Trigger(HapticPattern pattern)
    {
        var (low, high, duration) = pattern switch
        {
            HapticPattern.Light     => (0.1f, 0.0f, 0.05f),
            HapticPattern.Medium    => (0.3f, 0.1f, 0.08f),
            HapticPattern.Heavy     => (0.6f, 0.4f, 0.12f),
            HapticPattern.Selection => (0.05f, 0.0f, 0.03f),
            HapticPattern.Success   => (0.2f, 0.3f, 0.15f),
            HapticPattern.Warning   => (0.4f, 0.2f, 0.2f),
            HapticPattern.Error     => (0.7f, 0.5f, 0.25f),
            _ => (0f, 0f, 0f)
        };

        _gamepad.SetMotorSpeeds(low, high);
        // Stop after duration via UniTask
        StopAfterDelay(duration).Forget();
    }

    public void TriggerCustom(float duration, float intensity)
    {
        _gamepad.SetMotorSpeeds(intensity * 0.6f, intensity * 0.4f);
        StopAfterDelay(duration).Forget();
    }

    private async UniTaskVoid StopAfterDelay(float duration)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(duration),
            ignoreTimeScale: true);
        _gamepad.SetMotorSpeeds(0f, 0f);
    }
}

// Register at startup
HapticFeedback.SetProvider(new GamepadHapticProvider(Gamepad.current));
HapticFeedback.Enabled = true;
```

### Pairing Audio and Haptics for Common Patterns

```csharp
/// <summary>
/// Convenience methods for common audio+haptic pairs.
/// </summary>
public static class UIFeedback
{
    public static void Click()
    {
        UIAudioManager.Play("ui_click");
        HapticFeedback.Light();
    }

    public static void Error()
    {
        UIAudioManager.Play("ui_error");
        HapticFeedback.Play(HapticPattern.Error);
    }

    public static void Success()
    {
        UIAudioManager.Play("ui_success");
        HapticFeedback.Play(HapticPattern.Success);
    }
}
```

## Test Plan

### UIAudioManager Tests

| # | Test | Expectation |
|---|---|---|
| 1 | `RegisterClips(bank)` then `Play("ui_click")` | AudioSource plays the correct clip |
| 2 | `Play("nonexistent")` | No exception, warning logged in debug |
| 3 | `Play` twice within debounce window (50ms) | Second call silently dropped |
| 4 | `Play` twice with debounce window expired | Both calls play |
| 5 | `Muted.Value = true` then `Play` | No AudioSource rented, no sound |
| 6 | `MasterVolume.Value = 0.5f` then `Play` | AudioSource volume = baseVolume * 0.5 * volumeScale |
| 7 | `UnregisterClips(bank)` then `Play("ui_click")` | No exception, warning logged |
| 8 | `Tick()` after clip finishes | AudioSource returned to pool |
| 9 | Multiple simultaneous plays (different clips) | All play concurrently |
| 10 | `DebounceCooldown` set to 0 | No debouncing, all plays go through |

### UIAudioBank Tests

| # | Test | Expectation |
|---|---|---|
| 11 | Bank with duplicate IDs | Last entry wins on registration |
| 12 | Bank with null AudioClip | Entry skipped, warning logged |
| 13 | Empty bank | No entries registered, no error |

### HapticFeedback Tests

| # | Test | Expectation |
|---|---|---|
| 14 | `Light()` on mobile | Provider.Trigger(HapticPattern.Light) called |
| 15 | `Light()` on desktop (no provider) | No-op, no exception |
| 16 | `Enabled = false` then `Medium()` | Provider not called |
| 17 | `Custom(0.1f, 0.8f)` | Provider.TriggerCustom called with correct args |
| 18 | `SetProvider(custom)` then `Heavy()` | Custom provider's Trigger called |
| 19 | `Play(HapticPattern.Success)` | Provider triggers Success pattern |
| 20 | Provider.IsAvailable returns false | All calls are no-ops |

### Source Generator Tests

| # | Test | Expectation |
|---|---|---|
| 21 | `[UIAudio]` with valid element and trigger | Generates RegisterCallback in __AttachAudioHaptics |
| 22 | `[UIHaptic]` with valid element | Generates RegisterCallback for haptic |
| 23 | `[UIAudio]` referencing missing element | JUI600 diagnostic |
| 24 | `[UIHaptic]` referencing missing element | JUI601 diagnostic |
| 25 | `[UIAudio]` with empty Clip | JUI602 warning |
| 26 | `[UIAudio(Trigger = UIAudioTrigger.Error)]` | JUI603 warning (programmatic only) |
| 27 | `[UIAudio]` on non-Component class | JUI604 error |
| 28 | Multiple `[UIAudio]` on same class | All wired independently |

### Integration Tests

| # | Test | Expectation |
|---|---|---|
| 29 | Click a button with `[UIAudio]` + `[UIHaptic]` | Sound plays and haptic fires simultaneously |
| 30 | Hover then click (two audio triggers on same element) | Both clips play at correct triggers |
| 31 | Component detach then re-attach | Callbacks unregistered on detach, re-registered on attach |
| 32 | Volume slider bound to MasterVolume signal | Sliding updates volume reactively |

## Acceptance Criteria

- [ ] `UIAudioManager` plays registered clips via string ID with `AudioSource` pooling
- [ ] `UIAudioBank` is a `ScriptableObject` creatable via `Assets > Create > JUI > Audio Bank`
- [ ] `UIAudioBank.Entry` contains `Id`, `Clip`, and `Volume` fields
- [ ] `UIAudioManager.Play()` respects `MasterVolume`, `Muted`, and per-call `volumeScale`
- [ ] Final volume = `baseVolume * volumeScale * MasterVolume.Value`
- [ ] `UIAudioManager.Play()` debounces: same `clipId` within `DebounceCooldown` window is dropped
- [ ] `DebounceCooldown` defaults to 50ms and uses `Time.unscaledTime`
- [ ] `MasterVolume` and `Muted` are `Signal<T>` values for reactive UI binding
- [ ] `UIAudioManager.Tick()` returns finished `AudioSource` instances to the pool
- [ ] `AudioSource` pool lives on a `DontDestroyOnLoad` hidden `GameObject`
- [ ] `HapticFeedback` provides `Light()`, `Medium()`, `Heavy()`, `Selection()`, `Custom()`, and `Play(HapticPattern)` static methods
- [ ] `HapticFeedback` defaults to enabled on mobile, disabled on desktop
- [ ] `HapticFeedback.SetProvider()` allows custom `IHapticProvider` implementations
- [ ] `IHapticProvider` defines `IsAvailable`, `Trigger(HapticPattern)`, and `TriggerCustom(float, float)`
- [ ] `HapticPattern` enum includes `Light`, `Medium`, `Heavy`, `Selection`, `Success`, `Warning`, `Error`
- [ ] `[UIAudio]` attribute is class-level, `AllowMultiple = true`, with `ElementName`, `Trigger`, `Clip`, `VolumeScale`
- [ ] `[UIHaptic]` attribute is class-level, `AllowMultiple = true`, with `ElementName`, `Trigger`, `Pattern`
- [ ] Source generator emits `__AttachAudioHaptics` / `__DetachAudioHaptics` partial methods
- [ ] Generated code maps `UIAudioTrigger` enum values to the correct `VisualElement` event types
- [ ] `Error` and `Success` triggers are programmatic-only; generator emits JUI603 warning if used in attributes
- [ ] Diagnostics JUI600-JUI604 are emitted for invalid attribute configurations
- [ ] All `UIAudioManager` and `HapticFeedback` calls require the main thread (debug assertion)
