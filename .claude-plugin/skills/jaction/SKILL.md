---
name: jaction
description: JAction fluent chainable task system for Unity. Triggers on: sequential tasks, delay, timer, repeat loop, WaitUntil, WaitWhile, async workflow, zero-allocation async, coroutine alternative, scheduled action, timed event, polling condition, action sequence, ExecuteAsync, parallel execution
---

# JAction - Chainable Task Execution

Fluent API for composing complex action sequences in Unity with automatic object pooling, zero-allocation async, and parallel execution support.

## When to Use

- Sequential workflows with delays
- Polling conditions (WaitUntil/WaitWhile)
- Repeat loops with intervals
- Game timers and scheduled events
- Zero-GC async operations
- Parallel concurrent executions

## Core Concepts

### Task Snapshot Isolation

When `Execute()` or `ExecuteAsync()` is called, the current task list is **snapshotted**. Modifications to the JAction after execution starts do NOT affect running executions:

```csharp
var action = JAction.Create()
    .Delay(1f)
    .Do(static () => Debug.Log("Original"));

var handle = action.ExecuteAsync();

// This task is NOT executed by the handle above - it was added after the snapshot
action.Do(static () => Debug.Log("Added Later"));

await handle; // Only prints "Original"
```

This isolation enables safe parallel execution where each handle operates on its own task snapshot.

### Return Types

**JActionExecution** (returned by Execute, awaited from ExecuteAsync):
- `.Action` - The JAction that was executed
- `.Cancelled` - Whether THIS specific execution was cancelled
- `.Executing` - Whether the action is still executing
- `.Dispose()` - Returns JAction to pool

**JActionExecutionHandle** (returned by ExecuteAsync before await):
- `.Action` - The JAction being executed
- `.Cancelled` - Whether this execution is cancelled
- `.Executing` - Whether still running
- `.Cancel()` - Cancel THIS specific execution
- `.AsUniTask()` - Convert to `UniTask<JActionExecution>`
- Awaitable: `await handle` returns `JActionExecution`

## API Reference

### Execution

| Method | Returns | Description |
|--------|---------|-------------|
| `.Execute(timeout)` | `JActionExecution` | Synchronous blocking execution |
| `.ExecuteAsync(timeout)` | `JActionExecutionHandle` | Async via PlayerLoop (recommended) |

### Actions

| Method | Description |
|--------|-------------|
| `.Do(Action)` | Execute synchronous action |
| `.Do<T>(Action<T>, T)` | Execute with state (zero-alloc for reference types) |
| `.Do(Func<JActionAwaitable>)` | Execute async action |
| `.Do<T>(Func<T, JActionAwaitable>, T)` | Async with state |

### Timing

| Method | Description |
|--------|-------------|
| `.Delay(seconds)` | Wait specified seconds |
| `.DelayFrame(frames)` | Wait specified frame count |
| `.WaitUntil(condition, frequency, timeout)` | Wait until condition true |
| `.WaitWhile(condition, frequency, timeout)` | Wait while condition true |

### Loops

| Method | Description |
|--------|-------------|
| `.Repeat(action, count, interval)` | Repeat N times |
| `.RepeatWhile(action, condition, frequency, timeout)` | Repeat while condition true |
| `.RepeatUntil(action, condition, frequency, timeout)` | Repeat until condition true |

All loop methods have `<TState>` overloads for zero-allocation with reference types.

### Configuration

| Method | Description |
|--------|-------------|
| `.Parallel()` | Enable concurrent execution mode |
| `.OnCancel(callback)` | Register cancellation callback |
| `.Cancel()` | Stop ALL active executions |
| `.Reset()` | Clear state for reuse |
| `.Dispose()` | Return to object pool |

### Static Members

| Member | Description |
|--------|-------------|
| `JAction.Create()` | Get pooled instance |
| `JAction.PooledCount` | Check available pooled instances |
| `JAction.ClearPool()` | Empty the pool |

## Patterns

### Basic Sequence

```csharp
using var result = await JAction.Create()
    .Do(static () => Debug.Log("Step 1"))
    .Delay(1f)
    .Do(static () => Debug.Log("Step 2"))
    .ExecuteAsync();
```

### Always Use `using var` (CRITICAL)

```csharp
// CORRECT - auto-disposes and returns to pool
using var result = await JAction.Create()
    .Do(() => LoadAsset())
    .WaitUntil(() => assetLoaded)
    .ExecuteAsync();

// WRONG - memory leak, never returns to pool
await JAction.Create()
    .Do(() => LoadAsset())
    .ExecuteAsync();
```

### Parallel Execution with Per-Execution Cancellation

```csharp
var action = JAction.Create()
    .Parallel()
    .Do(static () => Debug.Log("Start"))
    .Delay(5f)
    .Do(static () => Debug.Log("Done"));

// Start multiple concurrent executions (each gets own task snapshot)
var handle1 = action.ExecuteAsync();
var handle2 = action.ExecuteAsync();

// Cancel only the first execution
handle1.Cancel();

// Each has independent Cancelled state
var result1 = await handle1;  // result1.Cancelled == true
var result2 = await handle2;  // result2.Cancelled == false

action.Dispose();
```

### UniTask.WhenAll with Parallel

```csharp
var action = JAction.Create()
    .Parallel()
    .Delay(1f)
    .Do(static () => Debug.Log("Done"));

var handle1 = action.ExecuteAsync();
var handle2 = action.ExecuteAsync();

await UniTask.WhenAll(handle1.AsUniTask(), handle2.AsUniTask());

action.Dispose();
```

### Zero-Allocation with Reference Types

```csharp
// CORRECT - static lambda + reference type state = zero allocation
var data = new MyData();
JAction.Create()
    .Do(static (MyData d) => d.Process(), data)
    .Execute();

// Pass 'this' when inside a class - no wrapper needed
public class Enemy : MonoBehaviour
{
    public bool IsStunned;

    public void ApplyStun(float duration)
    {
        IsStunned = true;
        JAction.Create()
            .Delay(duration)
            .Do(static (Enemy self) => self.IsStunned = false, this)
            .ExecuteAsync().Forget();
    }
}

// Value types use closures (boxing would defeat zero-alloc anyway)
int count = 5;
JAction.Create()
    .Do(() => Debug.Log($"Count: {count}"))
    .Execute();
```

### Timeout Handling

```csharp
using var result = await JAction.Create()
    .WaitUntil(() => networkReady)
    .ExecuteAsync(timeout: 30f);

if (result.Cancelled)
    Debug.Log("Timed out!");
```

### Cancellation Callback

```csharp
var action = JAction.Create()
    .OnCancel(() => Debug.Log("Cancelled!"))
    .Delay(10f);

var handle = action.ExecuteAsync();
handle.Cancel();  // Triggers OnCancel callback
```

## Game Patterns

### Cooldown Timer

```csharp
public class AbilitySystem
{
    public bool CanUse = true;

    public async UniTaskVoid TryUseAbility(float cooldown)
    {
        if (!CanUse) return;
        CanUse = false;

        PerformAbility();

        // Pass 'this' as state - no extra class needed
        using var _ = await JAction.Create()
            .Delay(cooldown)
            .Do(static s => s.CanUse = true, this)
            .ExecuteAsync();
    }
}
```

### Damage Over Time

```csharp
public sealed class DoTState
{
    public IDamageable Target;
    public float DamagePerTick;
}

public static async UniTaskVoid ApplyDoT(
    IDamageable target, float damage, int ticks, float interval)
{
    var state = JObjectPool.Shared<DoTState>().Rent();
    state.Target = target;
    state.DamagePerTick = damage;

    using var _ = await JAction.Create()
        .Repeat(
            static s => s.Target?.TakeDamage(s.DamagePerTick),
            state, count: ticks, interval: interval)
        .ExecuteAsync();

    state.Target = null;
    JObjectPool.Shared<DoTState>().Return(state);
}
```

### Wave Spawner

```csharp
public async UniTask RunWaves(WaveConfig[] waves)
{
    foreach (var wave in waves)
    {
        using var result = await JAction.Create()
            .Do(() => UI.ShowWaveStart(wave.Number))
            .Delay(2f)
            .Do(() => SpawnWave(wave))
            .WaitUntil(() => ActiveEnemyCount == 0, timeout: 120f)
            .Delay(wave.DelayAfter)
            .ExecuteAsync();

        if (result.Cancelled) break;
    }
}
```

### Health Regeneration

```csharp
public sealed class RegenState
{
    public float Health, MaxHealth, HpPerTick;
}

public static async UniTaskVoid StartRegen(RegenState state)
{
    using var _ = await JAction.Create()
        .RepeatWhile(
            static s => s.Health = MathF.Min(s.Health + s.HpPerTick, s.MaxHealth),
            static s => s.Health < s.MaxHealth,
            state, frequency: 0.1f)
        .ExecuteAsync();
}
```

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| Nothing happens | Forgot to call Execute/ExecuteAsync | Add `.ExecuteAsync()` at the end |
| Memory leak | Missing `using var` | Always use `using var result = await ...` |
| Frame drops | Using `Execute()` | Switch to `ExecuteAsync()` |
| GC allocations | Closures with reference types | Use static lambda + state parameter |
| Unexpected timing | Value type state | Wrap in reference type or use closure |
| Handle shows wrong Cancelled | Reading after modification | Snapshot is isolated - this is expected |

## Common Mistakes

1. **Missing `using var`** - Memory leak, JAction never returns to pool
2. **Using `Execute()` in production** - Blocks main thread, causes frame drops
3. **State overloads with value types** - Causes boxing; use closures instead
4. **Forgetting Execute/ExecuteAsync** - Nothing happens
5. **Heavy work in `.Do()`** - Callbacks run atomically; keep them lightweight
6. **Using `action.Cancel()` in parallel** - Cancels ALL executions; use `handle.Cancel()` for specific execution
7. **Modifying JAction after ExecuteAsync** - Changes don't affect running execution (task snapshot isolation)
