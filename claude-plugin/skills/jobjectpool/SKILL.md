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
    Func<T> createFunc = null,    // Factory (defaults to Activator.CreateInstance<T>())
    Action<T> onRent = null,       // Called when renting
    Action<T> onReturn = null      // Called when returning
)
```

### Methods
- `.Rent()` - Get object from pool or create new
- `.Return(T obj)` - Return object to pool
- `.Clear()` - Remove all pooled objects
- `.Prewarm(int count)` - Pre-allocate objects
- `.Count` - Current available count

### Static Access
- `JObjectPool.Shared<T>()` - Global shared pool per type (simple objects only)

## Patterns

### Basic Pool
```csharp
var pool = new JObjectPool<Bullet>();
var bullet = pool.Rent();
// ... use bullet ...
pool.Return(bullet);
```

### With Custom Factory
```csharp
var pool = new JObjectPool<Enemy>(
    createFunc: () => new Enemy(defaultHealth: 100)
);
```

### Reset State on Return (RECOMMENDED)
```csharp
var pool = new JObjectPool<List<int>>(
    onReturn: static list => list.Clear()
);
```

### Initialize on Rent
```csharp
var pool = new JObjectPool<Projectile>(
    onRent: static p => p.Reset()
);
```

### Prewarm During Loading
```csharp
var pool = new JObjectPool<Effect>();
pool.Prewarm(50);  // Pre-create during loading screen
```

### Shared Pool (Simple Objects)
```csharp
// Good for simple reusable objects without custom initialization
var sb = JObjectPool.Shared<StringBuilder>().Rent();
sb.Append("Hello");
JObjectPool.Shared<StringBuilder>().Return(sb);
```

## Best Practices
1. **Pre-allocate during loading** to prevent in-game allocation spikes
2. **Reset state on return** to prevent data leaks between reuses
3. **Use custom pools** for complex objects requiring specific initialization
4. **Use shared pools only** for simple value objects with default construction
5. **Monitor pool Count** to optimize sizing

## Common Mistakes
- Returning null to pool (ignored, but wasteful)
- Not clearing object state on return (causes bugs from stale data)
- Forgetting to return objects (pool becomes ineffective)
- Using shared pool for complex objects needing initialization
