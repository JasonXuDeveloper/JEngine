---
name: jobjectpool
description: JObjectPool thread-safe object pooling for Unity. Triggers on: object pool, GC optimization, reusable instances, bullet pool, enemy pool, effect pool, spawn pool, reduce garbage collection, memory optimization, pool prewarm, Rent Return pattern, lock-free pool
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

## Game Patterns (Zero-GC)

### Bullet Pool with Struct Config
```csharp
public sealed class Bullet
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float Damage;
    public float Lifetime;

    public void Reset()
    {
        Position = default;
        Velocity = default;
        Damage = 0;
        Lifetime = 0;
    }
}

public sealed class BulletManager
{
    private readonly JObjectPool<Bullet> _pool = new(
        maxSize: 200,
        onReturn: static b => b.Reset());

    public void Initialize() => _pool.Prewarm(100);

    public Bullet Fire(in Vector3 pos, in Vector3 dir, float speed, float damage)
    {
        var bullet = _pool.Rent();
        bullet.Position = pos;
        bullet.Velocity = dir * speed;
        bullet.Damage = damage;
        return bullet;
    }

    public void Return(Bullet b) => _pool.Return(b);
}
```

### Enemy Spawner (Zero-GC State)
```csharp
public sealed class Enemy : IPoolable
{
    public float Health { get; set; }
    public Vector3 Position { get; set; }
    public event Action OnDeath;

    public void OnSpawn()
    {
        Health = 100f;
    }

    public void OnDespawn()
    {
        OnDeath = null;  // Clear delegates to prevent leaks
    }
}

public sealed class EnemySpawner
{
    private readonly JObjectPool<Enemy> _pool;

    public EnemySpawner(int maxSize = 50)
    {
        _pool = new(
            maxSize,
            onRent: static e => e.OnSpawn(),
            onReturn: static e => e.OnDespawn());
    }

    public Enemy Spawn(in Vector3 position)
    {
        var enemy = _pool.Rent();
        enemy.Position = position;
        return enemy;
    }

    public void Despawn(Enemy e) => _pool.Return(e);
}
```

### Temporary Collection (Zero-GC in Update)
```csharp
// Use in hot paths to avoid List<T> allocations
public void ProcessNearbyEnemies(in Vector3 center, float radius)
{
    var list = JObjectPool.Shared<List<Enemy>>().Rent();
    try
    {
        FindEnemiesNonAlloc(center, radius, list);
        foreach (var enemy in list)
        {
            ProcessEnemy(enemy);
        }
    }
    finally
    {
        list.Clear();
        JObjectPool.Shared<List<Enemy>>().Return(list);
    }
}
```

### StringBuilder Pool (Zero-GC String Building)
```csharp
public static string FormatDamage(float damage, string targetName)
{
    var sb = JObjectPool.Shared<StringBuilder>().Rent();
    try
    {
        sb.Append(targetName);
        sb.Append(" took ");
        sb.Append(damage.ToString("F1"));
        sb.Append(" damage");
        return sb.ToString();
    }
    finally
    {
        sb.Clear();
        JObjectPool.Shared<StringBuilder>().Return(sb);
    }
}
```

## Best Practices
1. **Pre-allocate during loading** to prevent in-game allocation spikes
2. **Reset state on return** to prevent data leaks between reuses
3. **Set appropriate maxSize** based on expected concurrent usage
4. **Use shared pools** for simple objects without custom callbacks
5. **Monitor pool Count** to optimize sizing

## Troubleshooting

### Objects Not Reused
- **Forgot to Return:** Always call `pool.Return(obj)` when done
- **Pool at capacity:** Increase maxSize if concurrent usage exceeds limit
- **Returning null:** Null values are silently ignored

### Stale Data / Memory Leaks
- **Not resetting state:** Clear all fields in onReturn callback
- **Event delegates:** Unsubscribe all events in onReturn to prevent leaks
- **References to disposed objects:** Null out references in onReturn

### Performance Issues
- **Not pre-warming:** Call `Prewarm()` during loading screens
- **maxSize too low:** Causes allocations when pool empties
- **maxSize too high:** Wastes memory

### Thread Safety
- JObjectPool IS thread-safe (lock-free CAS)
- Safe to Rent/Return from any thread
- onRent/onReturn run on calling thread

## Common Mistakes
- Returning null to pool (ignored, but wasteful)
- Not clearing object state on return (causes bugs from stale data)
- Forgetting to return objects (pool becomes ineffective)
- Setting maxSize too low (causes frequent allocations)
- Setting maxSize too high (wastes memory)
