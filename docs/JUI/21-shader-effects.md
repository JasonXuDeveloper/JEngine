# Section 21 — UI Shader & Visual Effects

## Overview

UIEffectManager provides a managed pipeline for applying GPU-accelerated visual effects to UI Toolkit elements. The system centers on three pooled resources: **RenderTextures** (bucketed by power-of-two dimensions), **Materials** (keyed by `UIEffectType`), and a batched **CommandBuffer** that coalesces all pending redraws into a single GPU submission per frame.

The `[UIEffect]` attribute enables declarative, reactive binding of shader effects to named elements. When the associated Signal changes, the effect parameters update automatically and a redraw is requested. All shipped shaders are SRP-agnostic, compiling cleanly against Built-in, URP, and HDRP render pipelines.

## Dependencies

| Section | What It Provides |
|---------|-----------------|
| 1 — Reactive Primitives | `Signal<T>`, `IReadOnlySignal<T>` for reactive intensity/parameter binding |
| 2 — Effect System | `Effect()` scheduling for frame-batched redraws |
| 3 — Reactive Collections | Not directly used |
| 4 — DI Container | Optional injection of custom shader registrations |
| 5 — Binding System | `[Bind]` cooperation — effects apply after element resolution |
| 6 — Component Model | `JUIComponent` lifecycle (mount/unmount) for effect setup/teardown |
| 9 — Generator Setup | Source generator processes `[UIEffect]` attributes at compile time |

## File Structure

```
Packages/com.jasonxudeveloper.jengine.ui/
├── Runtime/JUI/Effects/
│   ├── UIEffectManager.cs          # RT pool, Material pool, CommandBuffer batching
│   └── UIEffectType.cs             # Enum of all built-in effect types
├── Runtime/JUI/Attributes/
│   └── UIEffectAttribute.cs        # Declarative effect binding attribute
└── Shaders/
    ├── JUI-Blur.shader             # Gaussian blur (element or background)
    ├── JUI-Dissolve.shader         # Noise-based dissolve
    ├── JUI-ColorGrade.shader       # Brightness, contrast, saturation, hue shift
    ├── JUI-Vignette.shader         # Rounded corner darkening
    ├── JUI-Scanlines.shader        # CRT/retro scanline overlay
    ├── JUI-Glow.shader             # Outer glow / inner shadow / outline
    ├── JUI-GradientOverlay.shader  # Linear/radial/angular gradient overlay
    ├── JUI-TextEffects.shader      # Text glow, gradient, distortion
    ├── JUI-ShineSweep.shader       # Animated highlight sweep
    ├── JUI-Ripple.shader           # Radial ripple distortion
    └── JUI-CircleReveal.shader     # Transition: circle reveal, slide wipe, fade
```

## API Design

### UIEffectType Enum

```csharp
public enum UIEffectType
{
    // Element effects
    Blur,
    BlurBackground,
    Dissolve,
    ColorGrade,
    Vignette,
    Scanlines,
    Glow,
    InnerShadow,
    Outline,
    GradientOverlay,

    // Text effects
    TextGlow,
    TextGradient,
    TextDistortion,

    // Animated effects
    ShineSweep,
    Ripple,
    PulseGlow,

    // Transition effects
    FadeBlack,
    DissolveTransition,
    SlideWipe,
    CircleReveal
}
```

### UIEffectManager

```csharp
public static class UIEffectManager
{
    // --- RenderTexture Pool ---
    /// <summary>
    /// Rent a RenderTexture sized to the element's resolved layout.
    /// Dimensions are rounded up to the next power-of-two bucket.
    /// </summary>
    public static RenderTexture RentRT(VisualElement el);

    /// <summary>
    /// Return a RenderTexture to the pool for reuse.
    /// </summary>
    public static void ReturnRT(RenderTexture rt);

    // --- Material Pool ---
    /// <summary>
    /// Get a Material instance for the given effect type.
    /// Materials are pooled per-type and returned with default property values.
    /// </summary>
    public static Material GetMaterial(UIEffectType type);

    /// <summary>
    /// Return a Material to the pool. Properties are reset on return.
    /// </summary>
    public static void ReturnMaterial(Material mat);

    // --- Batched Redraw ---
    /// <summary>
    /// Request a redraw of the given RT using the given Material.
    /// Redraws are coalesced and executed in a single CommandBuffer at end-of-frame.
    /// </summary>
    public static void RequestRedraw(RenderTexture rt, Material mat);

    /// <summary>
    /// Flush all pending redraws. Called internally at end-of-frame.
    /// </summary>
    internal static void FlushRedraws();

    // --- Custom Shader Registration ---
    /// <summary>
    /// Register a custom shader by name for use with UIEffectType extensions.
    /// </summary>
    public static void RegisterShader(string name, Shader shader);
}
```

### UIEffectAttribute

```csharp
/// <summary>
/// Declaratively bind a shader effect to a named element.
/// The attributed Signal controls the effect intensity or parameters.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class UIEffectAttribute : Attribute
{
    /// <summary>
    /// Name of the element (via nameof(El.XXX)) to apply the effect to.
    /// </summary>
    public string ElementName { get; }

    /// <summary>
    /// The type of shader effect to apply.
    /// </summary>
    public UIEffectType Effect { get; set; }

    /// <summary>
    /// Default intensity value (0-100 range, mapped per-shader).
    /// </summary>
    public float Intensity { get; set; } = 1f;

    /// <summary>
    /// Optional tint color as a hex string (e.g., "#FF0000").
    /// </summary>
    public string Color { get; set; }

    public UIEffectAttribute(string elementName) { }
}
```

### Declarative Usage

```csharp
// Reactive blur that updates when the signal changes
[UIEffect(nameof(El.BackdropPanel), Effect = UIEffectType.Blur, Intensity = 8f)]
private Signal<float> _blurIntensity;

// Glow on a button, intensity driven by hover state
[UIEffect(nameof(El.ConfirmButton), Effect = UIEffectType.Glow, Color = "#4488FF")]
private Signal<float> _glowStrength;

// Dissolve transition driven by a 0-1 signal
[UIEffect(nameof(El.OldScreen), Effect = UIEffectType.DissolveTransition)]
private Signal<float> _dissolveProgress;
```

## Data Structures

### RenderTexture Pool Buckets

```
Bucket sizes (width x height):
  64x64, 128x128, 256x256, 512x512, 1024x1024, 2048x2048

Each bucket maintains:
  - Stack<RenderTexture> available
  - int activeCount
  - int peakCount (diagnostic)
```

### Material Pool

```
Dictionary<UIEffectType, Stack<Material>> pool
  - On GetMaterial: pop from stack or create new
  - On ReturnMaterial: reset properties, push to stack
  - Materials share shader instances (not duplicated)
```

### Redraw Batch

```
List<(RenderTexture rt, Material mat)> pendingRedraws
  - Accumulated during frame via RequestRedraw()
  - Flushed into single CommandBuffer in FlushRedraws()
  - List cleared after flush
```

## Implementation Notes

### RenderTexture Pool Sizing

Element layout dimensions are rounded up to the nearest power-of-two to maximize pool reuse. For example, a 300x200 element receives a 512x256 RT. The UV mapping in the shader compensates for the size mismatch so the effect renders at the correct aspect ratio.

```csharp
static int NextPowerOfTwo(int v)
{
    v--;
    v |= v >> 1; v |= v >> 2; v |= v >> 4;
    v |= v >> 8; v |= v >> 16;
    return v + 1;
}
```

### CommandBuffer Batching

All `RequestRedraw()` calls during a frame are queued. At the end of the frame (via `Effect()` or `LateUpdate` hook), `FlushRedraws()` builds a single `CommandBuffer`, issues all `Blit` operations, and executes:

```csharp
internal static void FlushRedraws()
{
    if (_pendingRedraws.Count == 0) return;

    var cmd = CommandBufferPool.Get("JUI-Effects");
    foreach (var (rt, mat) in _pendingRedraws)
    {
        cmd.Blit(rt, rt, mat);
    }
    Graphics.ExecuteCommandBuffer(cmd);
    CommandBufferPool.Release(cmd);
    _pendingRedraws.Clear();
}
```

### SRP-Agnostic Shader Strategy

All shipped shaders use a multi-compile approach:

```hlsl
#if defined(UNIVERSAL_PIPELINE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#elif defined(HD_PIPELINE)
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#else
    #include "UnityCG.cginc"
#endif
```

Shader feature sets are kept minimal to avoid variant explosion. Each shader targets 2-3 keywords maximum.

### Custom Shader Registration

Users can register custom shaders for effects beyond the built-in set:

```csharp
// Register at startup
UIEffectManager.RegisterShader("MyCustomEffect", myShader);

// Use via material directly
var mat = new Material(myShader);
var rt = UIEffectManager.RentRT(element);
UIEffectManager.RequestRedraw(rt, mat);
```

## Source Generator Notes

The source generator processes `[UIEffect]` attributes and emits:

1. **Effect setup in `OnMount()`**: Acquires Material, subscribes to the Signal, and calls `RequestRedraw()` on change.
2. **Effect teardown in `OnUnmount()`**: Returns Material and RenderTexture to pools, disposes subscription.
3. **Compile-time validation**: Verifies the element name exists in the component's `El` enum and that the `UIEffectType` is valid for the target element type (e.g., `TextGlow` only valid on text elements).

Generated code pattern:

```csharp
// Generated in partial class
private Material _gen_blurIntensity_mat;
private RenderTexture _gen_blurIntensity_rt;
private IDisposable _gen_blurIntensity_sub;

partial void OnMount_Effects()
{
    _gen_blurIntensity_mat = UIEffectManager.GetMaterial(UIEffectType.Blur);
    _gen_blurIntensity_rt = UIEffectManager.RentRT(El.BackdropPanel);
    _gen_blurIntensity_sub = _blurIntensity.Subscribe(v =>
    {
        _gen_blurIntensity_mat.SetFloat("_Intensity", v);
        UIEffectManager.RequestRedraw(_gen_blurIntensity_rt, _gen_blurIntensity_mat);
    });
}

partial void OnUnmount_Effects()
{
    _gen_blurIntensity_sub?.Dispose();
    UIEffectManager.ReturnMaterial(_gen_blurIntensity_mat);
    UIEffectManager.ReturnRT(_gen_blurIntensity_rt);
}
```

## Shipped Shaders Table

| Shader File | UIEffectType(s) | Key USS Properties | Notes |
|---|---|---|---|
| `JUI-Blur.shader` | Blur, BlurBackground | `_Intensity` (0-20), `_Samples` (4-16) | Two-pass Gaussian; Background variant samples scene |
| `JUI-Dissolve.shader` | Dissolve, DissolveTransition | `_Progress` (0-1), `_EdgeWidth`, `_EdgeColor` | Noise texture driven |
| `JUI-ColorGrade.shader` | ColorGrade | `_Brightness`, `_Contrast`, `_Saturation`, `_HueShift` | All range -1 to 1 |
| `JUI-Vignette.shader` | Vignette | `_Intensity` (0-1), `_Smoothness`, `_Color` | Rounded rect aware |
| `JUI-Scanlines.shader` | Scanlines | `_Density`, `_Speed`, `_Intensity` | Animated; retro CRT look |
| `JUI-Glow.shader` | Glow, InnerShadow, Outline | `_Size` (px), `_Color`, `_Intensity` | Mode keyword selects variant |
| `JUI-GradientOverlay.shader` | GradientOverlay | `_ColorA`, `_ColorB`, `_Angle`, `_Type` | Linear/radial/angular modes |
| `JUI-TextEffects.shader` | TextGlow, TextGradient, TextDistortion | `_GlowColor`, `_GradientColors`, `_DistortAmount` | SDF-aware for UITK text |
| `JUI-ShineSweep.shader` | ShineSweep | `_Speed`, `_Width`, `_Angle`, `_Color` | Looping highlight sweep |
| `JUI-Ripple.shader` | Ripple, PulseGlow | `_Center`, `_Frequency`, `_Amplitude`, `_Speed` | Click-point or center origin |
| `JUI-CircleReveal.shader` | FadeBlack, SlideWipe, CircleReveal | `_Progress` (0-1), `_Center`, `_Direction` | Transition effects; mode keyword |

## Usage Examples

### Basic Blur Backdrop

```csharp
public partial class SettingsPanel : JUIComponent
{
    [UIEffect(nameof(El.Backdrop), Effect = UIEffectType.BlurBackground, Intensity = 6f)]
    private Signal<float> _backdropBlur = new(6f);

    protected override void OnMount()
    {
        // Blur is applied automatically by generated code.
        // Adjust at runtime:
        _backdropBlur.Value = 12f; // increases blur
    }
}
```

### Animated Glow on Hover

```csharp
public partial class FancyButton : JUIComponent
{
    [UIEffect(nameof(El.ButtonBg), Effect = UIEffectType.Glow, Color = "#44AAFF")]
    private Signal<float> _glow = new(0f);

    protected override void OnMount()
    {
        var isHovered = new Signal<bool>(false);
        El.ButtonBg.RegisterCallback<PointerEnterEvent>(_ => isHovered.Value = true);
        El.ButtonBg.RegisterCallback<PointerLeaveEvent>(_ => isHovered.Value = false);

        // Animate glow intensity based on hover
        Effect(() =>
        {
            _glow.Value = isHovered.Value ? 1f : 0f;
        });
    }
}
```

### Screen Transition with Dissolve

```csharp
public partial class TransitionOverlay : JUIComponent
{
    [UIEffect(nameof(El.OverlayPanel), Effect = UIEffectType.DissolveTransition)]
    private Signal<float> _dissolve = new(0f);

    public async UniTask TransitionOut()
    {
        // Animate from 0 to 1 over 0.5 seconds
        await AnimateSignal(_dissolve, 0f, 1f, 0.5f, Easing.EaseInOutCubic);
    }

    public async UniTask TransitionIn()
    {
        await AnimateSignal(_dissolve, 1f, 0f, 0.5f, Easing.EaseInOutCubic);
    }
}
```

### Manual Effect Pipeline

```csharp
// For advanced users who need direct control
var rt = UIEffectManager.RentRT(myElement);
var mat = UIEffectManager.GetMaterial(UIEffectType.ColorGrade);
mat.SetFloat("_Brightness", 0.2f);
mat.SetFloat("_Contrast", 0.1f);
UIEffectManager.RequestRedraw(rt, mat);

// Later, clean up
UIEffectManager.ReturnMaterial(mat);
UIEffectManager.ReturnRT(rt);
```

## Test Plan

| # | Test Case | Expectation |
|---|-----------|-------------|
| 1 | `RentRT` for 300x200 element | Returns RT with dimensions 512x256 (next power-of-two) |
| 2 | `ReturnRT` then `RentRT` same size | Returns the same RT instance (pool reuse) |
| 3 | `GetMaterial(Blur)` twice | Returns two distinct Material instances |
| 4 | `ReturnMaterial` resets properties | Returned material has default `_Intensity` value |
| 5 | `RequestRedraw` x3, then `FlushRedraws` | Single CommandBuffer with 3 Blit operations |
| 6 | `FlushRedraws` with empty queue | No CommandBuffer created, no errors |
| 7 | `[UIEffect]` on mount | Material acquired, subscription active, initial redraw requested |
| 8 | `[UIEffect]` signal change | `RequestRedraw` called with updated material property |
| 9 | `[UIEffect]` on unmount | Material and RT returned to pools, subscription disposed |
| 10 | `RegisterShader` with custom shader | Material created with custom shader on `GetMaterial` |
| 11 | Generator: invalid element name in `[UIEffect]` | Compile-time error: element not found |
| 12 | Generator: `TextGlow` on non-text element | Compile-time warning: effect type mismatch |
| 13 | Shader compiles on Built-in RP | No errors, correct include paths |
| 14 | Shader compiles on URP | No errors, correct include paths |
| 15 | RT pool under memory pressure | Pool evicts least-recently-used buckets |

## Acceptance Criteria

- [ ] `UIEffectManager.RentRT` returns power-of-two bucketed RenderTextures and pool reuses them on return
- [ ] `UIEffectManager.GetMaterial` / `ReturnMaterial` pool Materials per `UIEffectType` and reset properties on return
- [ ] `RequestRedraw` batches all redraws into a single `CommandBuffer` flushed once per frame
- [ ] `[UIEffect]` attribute generates mount/unmount code that manages Material, RT, and Signal subscription lifecycle
- [ ] All 11 shipped shaders compile without errors on Built-in, URP, and HDRP pipelines
- [ ] Each shader exposes documented USS properties (`_Intensity`, `_Progress`, etc.)
- [ ] `RegisterShader` allows custom shaders to be used with the pooling and batching infrastructure
- [ ] Source generator emits compile-time errors for invalid element names and warnings for type mismatches
- [ ] RT pool uses power-of-two sizing with UV compensation so effects render at correct aspect ratio
- [ ] No RenderTexture or Material leaks — all resources returned to pools on component unmount
