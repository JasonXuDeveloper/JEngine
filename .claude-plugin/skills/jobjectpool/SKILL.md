---
name: jobjectpool
description: JObjectPool thread-safe pooling. Triggers on: object pool, GC optimization, reusable instances, bullet pool, spawn pool, reduce garbage collection
---

# JObjectPool - Thread-Safe Object Pooling

Thread-safe, lock-free generic object pooling for Unity using CAS operations. Works with job system and async operations.

## When to Use
- Frequently instantiated objects (bullets, enemies, effects)
- Reducing GC during gameplay
- Multi-threaded object reuse (safe from any thread)
- High-frequency object lifecycle operations

## Core API

### Constructor
```csharp
new JObjectPool<T>(
    int maxSize = 64,           // Maximum pooled objects (excess discarded)
    Action<T> onRent = null,    // Called when renting (for initialization)
    Action<T> onReturn = null   // Called when returning (for cleanup)
)
```
Note: `T` must be a reference type with a parameterless constructor (`where T : class, new()`).

### Methods
- `.Rent()` - Get object from pool or create new
- `.Return(T obj)` - Return object to pool (null values ignored)
- `.Clear()` - Remove all pooled objects
- `.Prewarm(int count)` - Pre-allocate objects (won't exceed maxSize)
- `.Count` - Current available count (approximate, thread-safe)

### Static Access
- `JObjectPool.Shared<T>()` - Global shared pool per type (default config: maxSize=64)

## Patterns

### Basic Pool
```csharp
var pool = new JObjectPool<Bullet>(maxSize: 100);
var bullet = pool.Rent();
// ... use bullet ...
pool.Return(bullet);
```

### With Initialization on Rent
```csharp
var pool = new JObjectPool<Enemy>(
    maxSize: 50,
    onRent: static enemy => enemy.Reset()
);
```

### Reset State on Return (RECOMMENDED)
```csharp
var pool = new JObjectPool<List<int>>(
    maxSize: 32,
    onReturn: static list => list.Clear()
);
```

### Prewarm During Loading
```csharp
var pool = new JObjectPool<Effect>();
pool.Prewarm(50);  // Pre-create during loading screen
```

### Shared Pool (Simple Objects)
```csharp
// Good for simple reusable objects without custom callbacks
var sb = JObjectPool.Shared<StringBuilder>().Rent();
sb.Append("Hello");
sb.Clear();  // Clean up before returning
JObjectPool.Shared<StringBuilder>().Return(sb);
```

## Best Practices
1. **Pre-allocate during loading** to prevent in-game allocation spikes
2. **Reset state on return** to prevent data leaks between reuses
3. **Set appropriate maxSize** based on expected concurrent usage
4. **Use shared pools** for simple objects without custom callbacks
5. **Monitor pool Count** to optimize sizing

## Common Mistakes
- Returning null to pool (ignored, but wasteful)
- Not clearing object state on return (causes bugs from stale data)
- Forgetting to return objects (pool becomes ineffective)
- Setting maxSize too low (causes frequent allocations)
- Setting maxSize too high (wastes memory)
