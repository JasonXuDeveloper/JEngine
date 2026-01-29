---
name: jaction
description: JAction chainable task system. Triggers on: sequential tasks, delays, timers, repeat loops, WaitUntil, async workflows, zero-allocation async
---

# JAction - Chainable Task Execution

Fluent API for composing complex action sequences in Unity with automatic object pooling and zero-allocation async.

## When to Use
- Sequential workflows with delays
- Polling conditions (WaitUntil/WaitWhile)
- Repeat loops with intervals
- Game timers and scheduled events
- Zero-GC async operations

## Core API

### Execution Methods
- `.Execute(float timeout = 0)` - Synchronous execution (BLOCKS main thread - use sparingly)
- `.ExecuteAsync(float timeout = 0)` - Asynchronous via PlayerLoop (RECOMMENDED)

### Action Execution
- `.Do(Action)` - Execute synchronous action
- `.Do(Action<T>, T state)` - Execute with state parameter (zero-allocation for reference types)
- `.Do(Func<JActionAwaitable>)` - Execute async action
- `.Do(Func<T, JActionAwaitable>, T state)` - Async with state

### Delays & Waits
- `.Delay(float seconds)` - Wait specified seconds
- `.DelayFrame(int frames)` - Wait specified frame count
- `.WaitUntil(Func<bool>)` - Wait until condition true
- `.WaitWhile(Func<bool>)` - Wait while condition true

### Loops
- `.Repeat(Action, int count, float interval)` - Repeat N times with interval
- `.RepeatWhile(Action, Func<bool>, float frequency, float timeout)` - Repeat while condition
- `.RepeatUntil(Action, Func<bool>, float frequency, float timeout)` - Repeat until condition

### Configuration
- `.Parallel()` - Enable concurrent action execution
- `.OnCancel(Action)` - Register cancellation callback

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

### State Parameter for Zero-Allocation
```csharp
// CORRECT - no closure allocation (works with both reference and value types)
var data = new MyData();
JAction.Create()
    .Do(static (MyData d) => d.Process(), data)
    .Execute();

// Also works with value types without boxing
int count = 5;
JAction.Create()
    .Do(static (int c) => Debug.Log($"Count: {c}"), count)
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

## Common Mistakes
- NOT using `using var` after ExecuteAsync (memory leak, never returns to pool)
- Using Execute() in production (blocks main thread, causes frame drops)
- Forgetting to call Execute() or ExecuteAsync() (nothing happens)
- Code in .Do() runs atomically and cannot be interrupted - keep callbacks lightweight
