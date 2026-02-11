# Section 15 — Animation (LitMotion Integration)

## Overview

Zero-allocation animation system built on LitMotion's `BindWithState` pattern. Provides static factory methods for common VisualElement animations (fade, move, scale, rotate, color), extension methods for one-liner convenience, a sequence builder for chaining and parallel composition, and an `ITransition` interface consumed by Show, Switch, and the Screen Router for enter/exit animations. All animation methods return `MotionHandle` for cancellation and lifetime management. Signal-driven animation is supported via `AnimateSignal`, which binds a LitMotion tween output to a `Signal<float>` value.

## Dependencies

- Sections 1--6 (Sequential Core: signals, effects, collections, DI, bindings, components)
- Section 7 (Show & Switch -- consume `ITransition` for branch transitions)
- LitMotion package (external dependency)

## File Structure

- `Runtime/JUI/Animation/JUIMotion.cs`
- `Runtime/JUI/Animation/JUIMotionExtensions.cs`
- `Runtime/JUI/Animation/JUISequence.cs`
- `Runtime/JUI/Animation/Transitions/ITransition.cs`
- `Runtime/JUI/Animation/Transitions/FadeTransition.cs`
- `Runtime/JUI/Animation/Transitions/SlideTransition.cs`
- `Runtime/JUI/Animation/Transitions/DissolveTransition.cs`
- `Runtime/JUI/Animation/Transitions/CircleRevealTransition.cs`
- `Runtime/JUI/Animation/Transitions/InstantTransition.cs`

## API Design

### Core Static Methods

```csharp
/// <summary>
/// Static factory methods for animating VisualElement properties via LitMotion.
/// All methods use <c>BindWithState</c> internally to avoid closure allocations.
/// Each method returns a <see cref="MotionHandle"/> that can be used to cancel
/// or await the animation.
/// </summary>
public static class JUIMotion
{
    /// <summary>
    /// Animates the element's opacity from its current value to <paramref name="to"/>.
    /// </summary>
    /// <param name="el">The target element.</param>
    /// <param name="to">The target opacity (0 = fully transparent, 1 = fully opaque).</param>
    /// <param name="dur">Duration in seconds.</param>
    /// <param name="ease">Easing function. Defaults to <see cref="Ease.OutQuad"/>.</param>
    /// <returns>A handle to the running motion.</returns>
    public static MotionHandle Fade(VisualElement el, float to, float dur,
        Ease ease = Ease.OutQuad);

    /// <summary>
    /// Animates the element's <c>transform.position</c> from its current value
    /// to <paramref name="to"/>.
    /// </summary>
    /// <param name="el">The target element.</param>
    /// <param name="to">The target position in local coordinates.</param>
    /// <param name="dur">Duration in seconds.</param>
    /// <param name="ease">Easing function. Defaults to <see cref="Ease.OutQuad"/>.</param>
    /// <returns>A handle to the running motion.</returns>
    public static MotionHandle Move(VisualElement el, Vector2 to, float dur,
        Ease ease = Ease.OutQuad);

    /// <summary>
    /// Animates the element's <c>transform.scale</c> uniformly from its current value
    /// to <paramref name="to"/>.
    /// </summary>
    /// <param name="el">The target element.</param>
    /// <param name="to">The target uniform scale factor.</param>
    /// <param name="dur">Duration in seconds.</param>
    /// <param name="ease">Easing function. Defaults to <see cref="Ease.OutBack"/>.</param>
    /// <returns>A handle to the running motion.</returns>
    public static MotionHandle Scale(VisualElement el, float to, float dur,
        Ease ease = Ease.OutBack);

    /// <summary>
    /// Animates the element's <c>transform.rotation</c> from its current angle
    /// to the current angle plus <paramref name="degrees"/>.
    /// </summary>
    /// <param name="el">The target element.</param>
    /// <param name="degrees">The rotation delta in degrees (positive = clockwise).</param>
    /// <param name="dur">Duration in seconds.</param>
    /// <param name="ease">Easing function. Defaults to <see cref="Ease.InOutSine"/>.</param>
    /// <returns>A handle to the running motion.</returns>
    public static MotionHandle Rotate(VisualElement el, float degrees, float dur,
        Ease ease = Ease.InOutSine);

    /// <summary>
    /// Animates a <see cref="Signal{float}"/> value from its current value
    /// to <paramref name="to"/>. The motion output is bound directly to the signal's
    /// <c>Value</c> setter, causing any effects or bindings on the signal to react.
    /// </summary>
    /// <param name="sig">The signal to animate.</param>
    /// <param name="to">The target value.</param>
    /// <param name="dur">Duration in seconds.</param>
    /// <param name="ease">Easing function. Defaults to <see cref="Ease.Linear"/>.</param>
    /// <returns>A handle to the running motion.</returns>
    public static MotionHandle AnimateSignal(Signal<float> sig, float to, float dur,
        Ease ease = Ease.Linear);

    /// <summary>
    /// Animates the element's background color from its current value
    /// to <paramref name="to"/>.
    /// </summary>
    /// <param name="el">The target element.</param>
    /// <param name="to">The target color.</param>
    /// <param name="dur">Duration in seconds.</param>
    /// <param name="ease">Easing function. Defaults to <see cref="Ease.Linear"/>.</param>
    /// <returns>A handle to the running motion.</returns>
    public static MotionHandle Color(VisualElement el, UnityEngine.Color to, float dur,
        Ease ease = Ease.Linear);
}
```

### Extension Methods

```csharp
/// <summary>
/// Convenience extension methods on <see cref="VisualElement"/> for common
/// animation patterns. Each method delegates to <see cref="JUIMotion"/> with
/// sensible defaults.
/// </summary>
public static class JUIMotionExtensions
{
    /// <summary>Fades the element from its current opacity to 1 (fully opaque).</summary>
    /// <param name="el">The target element.</param>
    /// <param name="dur">Duration in seconds. Defaults to 0.3s.</param>
    /// <returns>A handle to the running motion.</returns>
    public static MotionHandle FadeIn(this VisualElement el, float dur = 0.3f);

    /// <summary>Fades the element from its current opacity to 0 (fully transparent).</summary>
    /// <param name="el">The target element.</param>
    /// <param name="dur">Duration in seconds. Defaults to 0.3s.</param>
    /// <returns>A handle to the running motion.</returns>
    public static MotionHandle FadeOut(this VisualElement el, float dur = 0.3f);

    /// <summary>
    /// Slides the element into view from outside the specified direction.
    /// The element starts offset by its own width/height in the given direction
    /// and animates to its natural position (0, 0).
    /// </summary>
    /// <param name="el">The target element.</param>
    /// <param name="from">The direction the element slides in from.</param>
    /// <param name="dur">Duration in seconds. Defaults to 0.3s.</param>
    /// <returns>A handle to the running motion.</returns>
    public static MotionHandle SlideIn(this VisualElement el, Direction from, float dur = 0.3f);

    /// <summary>
    /// Slides the element out of view toward the specified direction.
    /// The element animates from its current position to an offset of its own
    /// width/height in the given direction.
    /// </summary>
    /// <param name="el">The target element.</param>
    /// <param name="to">The direction the element slides out toward.</param>
    /// <param name="dur">Duration in seconds. Defaults to 0.3s.</param>
    /// <returns>A handle to the running motion.</returns>
    public static MotionHandle SlideOut(this VisualElement el, Direction to, float dur = 0.3f);

    /// <summary>
    /// Scales the element up briefly and back to its original scale,
    /// creating a "pulse" or "pop" effect. Uses <see cref="Ease.OutElastic"/>.
    /// </summary>
    /// <param name="el">The target element.</param>
    /// <param name="intensity">Peak scale factor. Defaults to 1.1 (110%).</param>
    /// <param name="dur">Total duration in seconds. Defaults to 0.4s.</param>
    /// <returns>A handle to the running motion.</returns>
    public static MotionHandle Pulse(this VisualElement el, float intensity = 1.1f,
        float dur = 0.4f);

    /// <summary>
    /// Rapidly oscillates the element's horizontal position, creating a "shake"
    /// or "error" effect. The element returns to its original position at the end.
    /// </summary>
    /// <param name="el">The target element.</param>
    /// <param name="strength">Maximum displacement in pixels. Defaults to 10.</param>
    /// <param name="dur">Total duration in seconds. Defaults to 0.5s.</param>
    /// <returns>A handle to the running motion.</returns>
    public static MotionHandle Shake(this VisualElement el, float strength = 10f,
        float dur = 0.5f);

    /// <summary>
    /// Translates the element upward by <paramref name="height"/> and back down
    /// with a bounce easing, simulating a physical bounce.
    /// </summary>
    /// <param name="el">The target element.</param>
    /// <param name="height">Maximum bounce height in pixels. Defaults to 20.</param>
    /// <param name="dur">Total duration in seconds. Defaults to 0.6s.</param>
    /// <returns>A handle to the running motion.</returns>
    public static MotionHandle Bounce(this VisualElement el, float height = 20f,
        float dur = 0.6f);
}

/// <summary>Direction enum for slide animations.</summary>
public enum Direction
{
    Left,
    Right,
    Up,
    Down
}
```

### Sequence Builder

```csharp
/// <summary>
/// Fluent builder for composing sequential and parallel animations.
/// Motions added with <see cref="Then"/> run one after another.
/// Motions added with <see cref="With"/> run in parallel with the previous motion.
/// </summary>
public class JUISequence
{
    /// <summary>Creates a new sequence starting with the given motion.</summary>
    /// <param name="motion">The first motion in the sequence.</param>
    public JUISequence(MotionHandle motion);

    /// <summary>
    /// Appends a motion to run AFTER the previous motion completes.
    /// </summary>
    /// <param name="motion">The motion to append sequentially.</param>
    /// <returns>This sequence for fluent chaining.</returns>
    public JUISequence Then(MotionHandle motion);

    /// <summary>
    /// Adds a motion to run IN PARALLEL with the most recently added motion.
    /// Both motions start at the same time.
    /// </summary>
    /// <param name="motion">The motion to run in parallel.</param>
    /// <returns>This sequence for fluent chaining.</returns>
    public JUISequence With(MotionHandle motion);

    /// <summary>
    /// Inserts a delay before the next motion in the sequence.
    /// </summary>
    /// <param name="seconds">Delay duration in seconds.</param>
    /// <returns>This sequence for fluent chaining.</returns>
    public JUISequence Delay(float seconds);

    /// <summary>
    /// Registers a callback invoked when the entire sequence completes.
    /// Multiple callbacks can be registered and are invoked in registration order.
    /// </summary>
    /// <param name="callback">The callback to invoke on completion.</param>
    /// <returns>This sequence for fluent chaining.</returns>
    public JUISequence OnComplete(Action callback);

    /// <summary>
    /// Starts playback of the sequence and returns a handle representing the
    /// entire sequence. Cancelling the handle cancels all remaining motions.
    /// </summary>
    /// <returns>A handle to the running sequence.</returns>
    public MotionHandle Play();

    /// <summary>
    /// Starts playback and returns a UniTask that completes when the full
    /// sequence finishes. Useful for <c>await</c> in async transitions.
    /// </summary>
    /// <returns>A UniTask that completes on sequence end.</returns>
    public UniTask PlayAsync();
}
```

### Transition Interface

```csharp
/// <summary>
/// Defines enter and exit animations for use with <see cref="Show"/>,
/// <see cref="Switch{T}"/>, and the Screen Router. Implementations receive
/// the target <see cref="VisualElement"/> and return a <see cref="UniTask"/>
/// that completes when the animation finishes.
/// </summary>
public interface ITransition
{
    /// <summary>
    /// Plays the enter animation on the element (element is becoming visible).
    /// Called after the element is added to the visual tree but before it is
    /// considered fully mounted.
    /// </summary>
    /// <param name="element">The element to animate in.</param>
    /// <returns>A UniTask that completes when the enter animation finishes.</returns>
    UniTask PlayEnter(VisualElement element);

    /// <summary>
    /// Plays the exit animation on the element (element is becoming hidden).
    /// Called before the element is removed from the visual tree or set to
    /// <c>display:none</c>. The element is removed/hidden after the returned
    /// task completes.
    /// </summary>
    /// <param name="element">The element to animate out.</param>
    /// <returns>A UniTask that completes when the exit animation finishes.</returns>
    UniTask PlayExit(VisualElement element);
}
```

### Built-in Transitions

```csharp
/// <summary>
/// Fades elements in/out by animating opacity. Enter: 0 to 1. Exit: current to 0.
/// </summary>
public class FadeTransition : ITransition
{
    /// <summary>Animation duration in seconds. Defaults to 0.3s.</summary>
    public float Duration { get; init; } = 0.3f;

    /// <summary>Easing function. Defaults to <see cref="Ease.OutQuad"/>.</summary>
    public Ease Ease { get; init; } = Ease.OutQuad;

    public UniTask PlayEnter(VisualElement element);
    public UniTask PlayExit(VisualElement element);
}

/// <summary>
/// Slides elements in/out from a specified direction by animating transform.position.
/// </summary>
public class SlideTransition : ITransition
{
    /// <summary>Direction to slide from (enter) and toward (exit). Defaults to <see cref="Direction.Left"/>.</summary>
    public Direction From { get; init; } = Direction.Left;

    /// <summary>Animation duration in seconds. Defaults to 0.3s.</summary>
    public float Duration { get; init; } = 0.3f;

    /// <summary>Easing function. Defaults to <see cref="Ease.OutCubic"/>.</summary>
    public Ease Ease { get; init; } = Ease.OutCubic;

    public UniTask PlayEnter(VisualElement element);
    public UniTask PlayExit(VisualElement element);
}

/// <summary>
/// Dissolves elements in/out using a procedural noise mask applied via a
/// USS custom property and a UI Toolkit shader. The mask threshold is animated
/// from 0 (fully hidden) to 1 (fully revealed).
/// </summary>
public class DissolveTransition : ITransition
{
    /// <summary>Animation duration in seconds. Defaults to 0.5s.</summary>
    public float Duration { get; init; } = 0.5f;

    /// <summary>Edge softness of the dissolve effect. Defaults to 0.1.</summary>
    public float EdgeSoftness { get; init; } = 0.1f;

    public UniTask PlayEnter(VisualElement element);
    public UniTask PlayExit(VisualElement element);
}

/// <summary>
/// Reveals elements with a circular wipe originating from a specified center point.
/// Uses a clip-path or mask approach animating the circle radius from 0 to full coverage.
/// </summary>
public class CircleRevealTransition : ITransition
{
    /// <summary>
    /// Center of the reveal circle in normalized coordinates (0,0 = top-left, 1,1 = bottom-right).
    /// Defaults to (0.5, 0.5) (center of the element).
    /// </summary>
    public Vector2 Center { get; init; } = new(0.5f, 0.5f);

    /// <summary>Animation duration in seconds. Defaults to 0.5s.</summary>
    public float Duration { get; init; } = 0.5f;

    public UniTask PlayEnter(VisualElement element);
    public UniTask PlayExit(VisualElement element);
}

/// <summary>
/// No-animation transition. Enter and exit complete immediately. Useful as a
/// default or placeholder when transitions are optional.
/// </summary>
public class InstantTransition : ITransition
{
    /// <summary>Returns <see cref="UniTask.CompletedTask"/> immediately.</summary>
    public UniTask PlayEnter(VisualElement element);

    /// <summary>Returns <see cref="UniTask.CompletedTask"/> immediately.</summary>
    public UniTask PlayExit(VisualElement element);
}
```

## Data Structures

| Type | Role |
|------|------|
| `MotionHandle` | LitMotion handle representing a running animation. Used for cancellation and completion tracking. |
| `JUISequence._steps` | Internal `List<SequenceStep>` where each step is either a motion, a parallel group, or a delay. |
| `SequenceStep` | Internal struct: `{ MotionHandle[] Motions, float Delay, Action OnComplete }` |
| `Direction` | Enum: Left, Right, Up, Down. Used by slide animations and `SlideTransition`. |

## Implementation Notes

- **BindWithState pattern**: All `JUIMotion` methods use LitMotion's `LMotion.Create(from, to, duration).BindWithState(element, static (value, el) => ...)` pattern. This avoids closure allocation by passing the `VisualElement` as typed state to a static lambda. The static lambda sets the appropriate style property on the element.
- **AnimateSignal**: Uses `LMotion.Create(sig.Peek(), to, dur).BindWithState(sig, static (value, s) => s.Value = value)`. The signal is passed as state. Because the signal setter performs equality checks and batching, the animated value integrates seamlessly with the reactive system -- effects and bindings update as the value changes each frame.
- **Sequence internals**: `JUISequence` builds an internal list of steps. `Then()` appends a new step. `With()` adds a motion to the current step's parallel group. `Play()` iterates steps sequentially, awaiting each step's parallel group (all motions in a group are started simultaneously; the step completes when the longest motion finishes). `PlayAsync()` wraps the same logic in a `UniTask`.
- **Transition lifecycle**: `ITransition.PlayEnter` is called after the element is added to the visual tree (it has layout). `ITransition.PlayExit` is called before removal; the element is removed only after the returned `UniTask` completes. This ensures exit animations are visible.
- **DissolveTransition**: Applies a USS custom property `--jui-dissolve-threshold` to the element and animates it from 0 to 1 (enter) or 1 to 0 (exit). A USS rule matches elements with this property and applies a procedural mask shader. Requires the `jui-tokens.uss` stylesheet to be loaded.
- **CircleRevealTransition**: Animates a `--jui-reveal-radius` custom property. The element's USS applies `overflow: hidden` and uses a circular clip mask computed from the radius and center point.
- **Cancellation**: All `MotionHandle` instances can be cancelled via `handle.Cancel()`. In `JUISequence`, cancelling the sequence handle cancels all currently running motions and skips remaining steps.
- **Element measurement**: `SlideIn`/`SlideOut` require the element's resolved width/height to compute the offset. They schedule the motion on the next frame if the element has not been laid out yet (width/height == 0), using `el.RegisterCallback<GeometryChangedEvent>`.
- **Thread safety**: All animation APIs are main-thread only.

## Source Generator Notes

N/A for this section -- animation is a purely runtime system. No source generation is required.

## Usage Examples

```csharp
// --- Basic animations ---
JUIMotion.Fade(myElement, 0f, 0.5f);                    // fade to invisible
JUIMotion.Move(myElement, new Vector2(100, 0), 0.3f);    // slide right 100px
JUIMotion.Scale(myElement, 1.5f, 0.2f);                  // scale up 150%
JUIMotion.Rotate(myElement, 360f, 1f);                   // full rotation
JUIMotion.Color(myElement, UnityEngine.Color.red, 0.3f); // tint red

// --- Extension method shortcuts ---
myPanel.FadeIn();                           // opacity 0 -> 1, 0.3s
myPanel.FadeOut(0.5f);                      // opacity -> 0, 0.5s
myPanel.SlideIn(Direction.Left);            // slide from left
myPanel.SlideOut(Direction.Down, 0.2f);     // slide down and out
myButton.Pulse();                           // quick scale pop
errorField.Shake();                         // shake horizontally
icon.Bounce(30f);                           // bounce up 30px

// --- Animate a signal (drives reactive bindings) ---
var progress = new Signal<float>(0f);
JUIMotion.AnimateSignal(progress, 1f, 2f);
// Any effect or binding watching progress.Value will update smoothly over 2 seconds

// --- Sequence builder ---
new JUISequence(myPanel.FadeIn())
    .Then(JUIMotion.Move(myPanel, new Vector2(0, -50), 0.3f))
    .With(JUIMotion.Scale(myPanel, 1.2f, 0.3f))    // parallel with Move
    .Delay(0.5f)
    .Then(myPanel.FadeOut())
    .OnComplete(() => Debug.Log("Animation complete"))
    .Play();

// --- Async sequence ---
await new JUISequence(myPanel.FadeIn())
    .Then(JUIMotion.Move(myPanel, Vector2.zero, 0.3f))
    .PlayAsync();
// Execution continues after the full sequence completes

// --- Transitions with Show ---
new Show
{
    When = isVisible,
    Then = () => new TooltipPanel(),
    Transition = new FadeTransition { Duration = 0.2f }
};

// --- Transitions with Switch ---
new Switch<SettingsTab>
{
    Value = activeTab,
    Cases = new()
    {
        [SettingsTab.Audio] = () => new AudioPanel(),
        [SettingsTab.Video] = () => new VideoPanel(),
    },
    Transition = new SlideTransition
    {
        From = Direction.Right,
        Duration = 0.3f,
        Ease = Ease.OutCubic
    }
};

// --- Custom transition ---
public class ScaleTransition : ITransition
{
    public float Duration { get; init; } = 0.3f;

    public async UniTask PlayEnter(VisualElement element)
    {
        element.style.scale = new Scale(Vector2.zero);
        await LMotion.Create(0f, 1f, Duration)
            .WithEase(Ease.OutBack)
            .BindWithState(element, static (v, el) =>
                el.style.scale = new Scale(new Vector2(v, v)))
            .ToUniTask();
    }

    public async UniTask PlayExit(VisualElement element)
    {
        await LMotion.Create(1f, 0f, Duration)
            .WithEase(Ease.InBack)
            .BindWithState(element, static (v, el) =>
                el.style.scale = new Scale(new Vector2(v, v)))
            .ToUniTask();
    }
}

// --- Cancellation ---
var handle = myElement.FadeIn(2f);
// ... later:
handle.Cancel(); // stops the animation immediately
```

## Test Plan

1. **Fade animates opacity**: Call `JUIMotion.Fade(el, 0, 0.5f)`, advance time by 0.5s, verify `el.style.opacity` is 0.
2. **Move animates position**: Call `JUIMotion.Move(el, target, 0.3f)`, advance time, verify `el.transform.position` equals target.
3. **Scale animates uniformly**: Call `JUIMotion.Scale(el, 2f, 0.3f)`, verify scale reaches (2, 2, 1).
4. **Rotate animates rotation**: Call `JUIMotion.Rotate(el, 90f, 0.3f)`, verify rotation delta is 90 degrees.
5. **Color animates background**: Call `JUIMotion.Color(el, Color.red, 0.3f)`, verify background color transitions to red.
6. **AnimateSignal drives signal value**: Create a signal at 0, animate to 1 over 1s. Subscribe an effect. Verify the effect runs as the value changes. Verify signal value is 1 at completion.
7. **Extension methods use correct defaults**: Call `FadeIn()` with no arguments, verify duration is 0.3s and target opacity is 1.
8. **SlideIn computes offset from element dimensions**: Create an element with known width, call `SlideIn(Direction.Left)`, verify initial position is offset by negative width.
9. **Sequence Then runs motions sequentially**: Create a sequence with two `Then` steps. Verify the second motion starts only after the first completes.
10. **Sequence With runs motions in parallel**: Create a sequence with `Then` + `With`. Verify both motions in the parallel group start at the same time.
11. **Sequence Delay inserts pause**: Create a sequence with a 0.5s delay between motions. Verify the total duration includes the delay.
12. **Sequence OnComplete fires after last motion**: Register a callback, play the sequence, verify the callback fires exactly once after the last motion completes.
13. **PlayAsync awaitable**: Call `PlayAsync()`, `await` it, verify execution resumes after the sequence completes.
14. **FadeTransition enter/exit**: Verify enter animates opacity 0 to 1, exit animates opacity to 0.
15. **SlideTransition enter/exit**: Verify enter slides element from offscreen to natural position, exit slides to offscreen.
16. **InstantTransition completes immediately**: Verify both `PlayEnter` and `PlayExit` return completed tasks with no animation.
17. **Cancellation stops motion**: Start a 2s fade, cancel after 0.5s, verify opacity is not at the target value.
18. **Zero allocation on BindWithState path**: Run a fade animation in a profiled loop, verify no managed allocations per frame.

## Acceptance Criteria

- [ ] `JUIMotion.Fade`, `Move`, `Scale`, `Rotate`, `Color` animate the correct VisualElement style properties
- [ ] All `JUIMotion` methods use `BindWithState` with static lambdas (no closures)
- [ ] `AnimateSignal` binds motion output to `Signal<float>.Value`, triggering reactive updates
- [ ] Extension methods (`FadeIn`, `FadeOut`, `SlideIn`, `SlideOut`, `Pulse`, `Shake`, `Bounce`) delegate to `JUIMotion` with sensible defaults
- [ ] `JUISequence.Then` runs motions sequentially; `With` runs them in parallel
- [ ] `JUISequence.Delay` inserts a timed pause between steps
- [ ] `JUISequence.OnComplete` fires after the entire sequence finishes
- [ ] `JUISequence.Play` returns a `MotionHandle`; `PlayAsync` returns a `UniTask`
- [ ] `ITransition.PlayEnter` and `PlayExit` return `UniTask` for async lifecycle integration
- [ ] `FadeTransition`, `SlideTransition`, `DissolveTransition`, `CircleRevealTransition` implement `ITransition`
- [ ] `InstantTransition` completes both enter and exit immediately
- [ ] Cancelling a `MotionHandle` stops the animation immediately
- [ ] `SlideIn`/`SlideOut` handle elements without resolved layout via `GeometryChangedEvent`
- [ ] All public APIs have XML documentation
- [ ] Zero allocations per frame on the animation hot path (after initial setup)
