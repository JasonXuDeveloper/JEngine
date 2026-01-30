---
name: jaction
description: JAction fluent chainable task system for Unity. Triggers on: sequential tasks, delay, timer, repeat loop, WaitUntil, WaitWhile, async workflow, zero-allocation async, coroutine alternative, scheduled action, timed event, polling condition, action sequence, ExecuteAsync
---

# JAction - Chainable Task Execution

Fluent API for composing complex action sequences in Unity with automatic object pooling and zero-allocation async.

## When to Use
- Sequential workflows with delays
- Polling conditions (WaitUntil/WaitWhile)
- Repeat loops with intervals
- Game timers and scheduled events
- Zero-GC async operations

## Properties
- `.Executing` - Returns true if currently executing
- `.Cancelled` - Returns true if execution was cancelled
- `.IsParallel` - Returns true if parallel mode enabled

## Core API

### Execution Methods
- `.Execute(float timeout = 0)` - Synchronous execution (BLOCKS main thread - use sparingly)
- `.ExecuteAsync(float timeout = 0)` - Asynchronous via PlayerLoop (RECOMMENDED)

### Action Execution
- `.Do(Action)` - Execute synchronous action
- `.Do<TState>(Action<TState>, TState)` - Execute with state (zero-alloc for reference types)
- `.Do(Func<JActionAwaitable>)` - Execute async action
- `.Do<TState>(Func<TState, JActionAwaitable>, TState)` - Async with state

### Delays & Waits
- `.Delay(float seconds)` - Wait specified seconds
- `.DelayFrame(int frames)` - Wait specified frame count
- `.WaitUntil(Func<bool>, frequency, timeout)` - Wait until condition true
- `.WaitUntil<TState>(Func<TState, bool>, TState, frequency, timeout)` - With state
- `.WaitWhile(Func<bool>, frequency, timeout)` - Wait while condition true
- `.WaitWhile<TState>(Func<TState, bool>, TState, frequency, timeout)` - With state

### Loops
- `.Repeat(Action, count, interval)` - Repeat N times
- `.Repeat<TState>(Action<TState>, TState, count, interval)` - With state
- `.RepeatWhile(Action, Func<bool>, frequency, timeout)` - Repeat while condition
- `.RepeatWhile<TState>(Action<TState>, Func<TState, bool>, TState, frequency, timeout)` - With state
- `.RepeatUntil(Action, Func<bool>, frequency, timeout)` - Repeat until condition
- `.RepeatUntil<TState>(Action<TState>, Func<TState, bool>, TState, frequency, timeout)` - With state

### Configuration
- `.Parallel()` - Enable concurrent execution
- `.OnCancel(Action)` - Register cancellation callback
- `.OnCancel<TState>(Action<TState>, TState)` - With state

### Lifecycle
- `.Cancel()` - Stop execution
- `.Reset()` - Clear state for reuse
- `.Dispose()` - Return to object pool
- `JAction.PooledCount` - Check pooled instances
- `JAction.ClearPool()` - Empty the pool

## Patterns

### Basic Sequence (use ExecuteAsync in production)
```csharp
using var action = await JAction.Create()
    .Do(static () => Debug.Log("Step 1"))
    .Delay(1f)
    .Do(static () => Debug.Log("Step 2"))
    .ExecuteAsync();
```

### Always Use `using var` for Async (CRITICAL)
```csharp
// CORRECT - auto-disposes and returns to pool
using var action = await JAction.Create()
    .Do(() => LoadAsset())
    .WaitUntil(() => assetLoaded)
    .ExecuteAsync();

// WRONG - memory leak, never returns to pool
await JAction.Create()
    .Do(() => LoadAsset())
    .ExecuteAsync();
```

### State Parameter for Zero-Allocation (Reference Types Only)
```csharp
// CORRECT - no closure allocation with reference types
var data = new MyData();
JAction.Create()
    .Do(static (MyData d) => d.Process(), data)
    .Execute();

// WARNING: State overloads DO NOT work with value types (int, float, struct, bool, etc.)
// Value types get boxed when passed as generic parameters, defeating zero-allocation
// For value types, use closures instead (allocation is acceptable):
int count = 5;
JAction.Create()
    .Do(() => Debug.Log($"Count: {count}"))  // Closure is fine for value types
    .Execute();
```

### Set Timeouts for Production
```csharp
using var action = await JAction.Create()
    .WaitUntil(() => networkReady)
    .ExecuteAsync(timeout: 30f);  // Prevents infinite waits
```

### With Cancellation
```csharp
var action = JAction.Create()
    .OnCancel(() => Debug.Log("Cancelled!"))
    .Do(() => LongRunningTask());

var task = action.ExecuteAsync();
// Later...
action.Cancel();
```

## Game Patterns

All patterns use `ExecuteAsync()` for non-blocking execution.

### Cooldown Timer (Zero-GC)
```csharp
public sealed class AbilityState
{
    public bool CanUse = true;
}

public class AbilitySystem
{
    private readonly AbilityState _state = new();
    private readonly float _cooldown;

    public async UniTaskVoid TryUseAbility()
    {
        if (!_state.CanUse) return;
        _state.CanUse = false;

        PerformAbility();

        // Zero-GC: static lambda + reference type state
        using var action = await JAction.Create()
            .Delay(_cooldown)
            .Do(static s => s.CanUse = true, _state)
            .ExecuteAsync();
    }
}
```

### Damage Over Time (Zero-GC)
```csharp
public sealed class DoTState
{
    public IDamageable Target;
    public float DamagePerTick;
}

public static async UniTaskVoid ApplyDoT(IDamageable target, float damage, int ticks, float interval)
{
    // Rent state from pool to avoid allocation
    var state = JObjectPool.Shared<DoTState>().Rent();
    state.Target = target;
    state.DamagePerTick = damage;

    using var action = await JAction.Create()
        .Repeat(
            static s => s.Target?.TakeDamage(s.DamagePerTick),
            state,
            count: ticks,
            interval: interval)
        .ExecuteAsync();

    // Return state to pool
    state.Target = null;
    JObjectPool.Shared<DoTState>().Return(state);
}
```

### Wave Spawner (Async)
```csharp
// Async methods cannot use ReadOnlySpan (ref struct), use array instead
public async UniTask RunWaves(WaveConfig[] waves)
{
    foreach (var wave in waves)
    {
        using var action = await JAction.Create()
            .Do(() => UI.ShowWaveStart(wave.Number))
            .Delay(2f)
            .Do(() => SpawnWave(wave))
            .WaitUntil(() => ActiveEnemyCount == 0, timeout: 120f)
            .Delay(wave.DelayAfter)
            .ExecuteAsync();

        if (action.Cancelled) break;
    }
}

// Sync methods can use ReadOnlySpan for zero-allocation iteration
public void RunWavesSync(ReadOnlySpan<WaveConfig> waves)
{
    foreach (ref readonly var wave in waves)
    {
        using var action = JAction.Create()
            .Do(() => UI.ShowWaveStart(wave.Number))
            .Delay(2f)
            .Do(() => SpawnWave(wave))
            .WaitUntil(() => ActiveEnemyCount == 0, timeout: 120f)
            .Delay(wave.DelayAfter);
        action.Execute();

        if (action.Cancelled) break;
    }
}
```

### Health Regeneration (Zero-GC)
```csharp
public sealed class RegenState
{
    public float Health;
    public float MaxHealth;
    public float HpPerTick;
}

public static async UniTaskVoid StartRegen(RegenState state)
{
    using var action = await JAction.Create()
        .RepeatWhile(
            static s => s.Health = MathF.Min(s.Health + s.HpPerTick, s.MaxHealth),
            static s => s.Health < s.MaxHealth,
            state,
            frequency: 0.1f)
        .ExecuteAsync();
}
```

## Troubleshooting

### Nothing Happens
- **Forgot ExecuteAsync:** Must call `.ExecuteAsync()` at the end
- **Already disposed:** Don't reuse a JAction after Dispose()

### Memory Leak
- **Missing `using var`:** Always use `using var action = await ...ExecuteAsync()`
- **Infinite loop:** Set timeouts on WaitUntil/WaitWhile in production

### Frame Drops
- **Using Execute():** Switch to ExecuteAsync() for non-blocking
- **Heavy callbacks:** Keep .Do() callbacks lightweight

### Unexpected Behavior
- **Value type state:** State overloads box value types; wrap in reference type
- **Check Cancelled:** After timeout, check `action.Cancelled` before continuing

### GC Allocations
- **Closures:** Use static lambdas with state parameters
- **State must be reference type:** Value types get boxed

## Common Mistakes
- NOT using `using var` after ExecuteAsync (memory leak, never returns to pool)
- Using Execute() in production (blocks main thread, causes frame drops)
- Using state overloads with value types (causes boxing - use closures instead)
- Forgetting to call Execute() or ExecuteAsync() (nothing happens)
- Code in .Do() runs atomically and cannot be interrupted - keep callbacks lightweight
