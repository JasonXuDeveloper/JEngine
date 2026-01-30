---
name: game-patterns
description: Zero-GC game patterns with JEngine using modern C# 9+. Triggers on: game loop, spawn system, wave spawner, cooldown, ability timer, damage over time, DoT, health regen, bullet pool, enemy pool, object pool pattern, projectile system, combat system, zero allocation, no GC, performance optimization
---

# Game Patterns with JEngine

Zero-GC patterns using JAction + JObjectPool with modern C# 9+ features.

## Principles

1. **Async-First:** Always use `ExecuteAsync()` (non-blocking)
2. **Zero-GC:** Use static lambdas + reference type state + object pooling
3. **Modern C#:** Target-typed new, init properties, Span<T>, in parameters

## State Classes for Zero-GC

JAction state parameters must be reference types to avoid boxing.
Pool these state objects for true zero-GC:

```csharp
// Reusable state for abilities
public sealed class AbilityState
{
    public bool CanUse { get; set; } = true;
    public float Cooldown { get; init; }
}

// Reusable state for DoT effects
public sealed class DoTState
{
    public IDamageable Target { get; set; }
    public float DamagePerTick { get; set; }
    public int TicksRemaining { get; set; }

    public void Reset()
    {
        Target = null;
        DamagePerTick = 0;
        TicksRemaining = 0;
    }
}

// Reusable state for timers
public sealed class TimerState
{
    public float Duration { get; set; }
    public float Elapsed { get; set; }
    public Action OnComplete { get; set; }

    public void Reset()
    {
        Duration = 0;
        Elapsed = 0;
        OnComplete = null;
    }
}
```

## Combat Patterns

### Ability with Cooldown (Zero-GC)
```csharp
public sealed class AbilitySystem
{
    private readonly AbilityState _state = new() { Cooldown = 2f };

    public async UniTaskVoid UseAbility()
    {
        if (!_state.CanUse) return;
        _state.CanUse = false;

        PerformAbility();

        using var action = await JAction.Create()
            .Delay(_state.Cooldown)
            .Do(static s => s.CanUse = true, _state)
            .ExecuteAsync();
    }
}
```

### Damage Over Time (Zero-GC Pooled State)
```csharp
public static class DoTSystem
{
    public static async UniTaskVoid Apply(
        IDamageable target,
        float damagePerTick,
        int ticks,
        float interval)
    {
        var state = JObjectPool.Shared<DoTState>().Rent();
        state.Target = target;
        state.DamagePerTick = damagePerTick;

        using var action = await JAction.Create()
            .Repeat(
                static s => s.Target?.TakeDamage(s.DamagePerTick),
                state,
                count: ticks,
                interval: interval)
            .ExecuteAsync();

        state.Reset();
        JObjectPool.Shared<DoTState>().Return(state);
    }
}
```

### Combo System (Zero-GC)
```csharp
public sealed class ComboState
{
    public int Count;
    public JAction ResetAction;
}

public sealed class ComboSystem : IDisposable
{
    private readonly ComboState _state = new();

    public int ComboCount => _state.Count;

    public void OnHit()
    {
        _state.ResetAction?.Cancel();
        _state.ResetAction?.Dispose();
        _state.Count++;

        _state.ResetAction = JAction.Create()
            .Delay(1.5f)
            .Do(static s => s.Count = 0, _state);

        _ = _state.ResetAction.ExecuteAsync();
    }

    /// <summary>
    /// Call from OnDestroy to prevent callbacks on destroyed objects.
    /// </summary>
    public void Dispose()
    {
        _state.ResetAction?.Cancel();
        _state.ResetAction?.Dispose();
        _state.ResetAction = null;
    }
}
```

## Spawning Patterns

### Wave Spawner (Async)
```csharp
public sealed class WaveSpawner
{
    private readonly EnemySpawner _spawner;

    // Use ReadOnlyMemory<T> for async (ReadOnlySpan<T> is a ref struct, invalid in async)
    // Access .Span inside the loop for zero-allocation iteration
    public async UniTask RunWaves(ReadOnlyMemory<WaveConfig> waves, CancellationToken ct = default)
    {
        foreach (var wave in waves.Span)
        {
            using var action = await JAction.Create()
                .Delay(wave.StartDelay)
                .Do(() => SpawnWave(wave.EnemyCount))
                .WaitUntil(() => ActiveCount == 0, timeout: 120f)
                .ExecuteAsync();

            if (action.Cancelled || ct.IsCancellationRequested) break;
        }
    }

    private void SpawnWave(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var pos = GetSpawnPosition(i);
            _spawner.Spawn(in pos);
        }
    }
}
```

### Pooled Spawner (Generic, Zero-GC)
```csharp
public interface IPoolable
{
    void OnSpawn();
    void OnDespawn();
}

public sealed class PooledSpawner<T> where T : class, IPoolable, new()
{
    private readonly JObjectPool<T> _pool;

    public PooledSpawner(int maxSize = 64)
    {
        _pool = new(
            maxSize,
            onRent: static obj => obj.OnSpawn(),
            onReturn: static obj => obj.OnDespawn());
    }

    public T Spawn() => _pool.Rent();
    public void Despawn(T obj) => _pool.Return(obj);
    public void Prewarm(int count) => _pool.Prewarm(count);
    public int PooledCount => _pool.Count;
}
```

## Resource Patterns

### Health Regeneration (Zero-GC)
```csharp
public sealed class RegenState
{
    public float Current;
    public float Max;
    public float PerTick;
    public JAction Action;

    public void Cleanup()
    {
        Action?.Cancel();
        Action?.Dispose();
        Action = null;
    }
}

public static class RegenSystem
{
    public static void Start(RegenState state, float hpPerSecond)
    {
        state.Cleanup();
        state.PerTick = hpPerSecond * 0.1f;

        state.Action = JAction.Create()
            .RepeatWhile(
                static s => s.Current = MathF.Min(s.Current + s.PerTick, s.Max),
                static s => s.Current < s.Max,
                state,
                frequency: 0.1f);

        _ = state.Action.ExecuteAsync();
    }

    /// <summary>
    /// Stop regeneration. Call Cleanup() from OnDestroy to fully dispose.
    /// </summary>
    public static void Stop(RegenState state) => state.Cleanup();
}
```

## Projectile Patterns

### Bullet Manager (Zero-GC)
```csharp
public sealed class Bullet
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float Damage;
    public bool Active;

    public void Reset()
    {
        Position = default;
        Velocity = default;
        Damage = 0;
        Active = false;
    }
}

public sealed class BulletManager
{
    public static BulletManager Instance { get; private set; }

    private readonly JObjectPool<Bullet> _pool = new(
        maxSize: 500,
        onReturn: static b => b.Reset());

    public void Initialize()
    {
        Instance = this;
        _pool.Prewarm(200);
    }

    public Bullet Fire(in Vector3 pos, in Vector3 dir, float speed, float damage)
    {
        var bullet = _pool.Rent();
        bullet.Position = pos;
        bullet.Velocity = dir * speed;
        bullet.Damage = damage;
        bullet.Active = true;
        return bullet;
    }

    public void Return(Bullet b) => _pool.Return(b);
}
```

### Auto-Return After Lifetime (Zero-GC)
```csharp
public sealed class BulletLifetimeState
{
    public Bullet Bullet;
    public BulletManager Manager;

    public void Reset()
    {
        Bullet = null;
        Manager = null;
    }
}

public static async UniTaskVoid FireWithLifetime(
    BulletManager manager,
    in Vector3 pos,
    in Vector3 dir,
    float speed,
    float damage,
    float lifetime)
{
    var bullet = manager.Fire(in pos, in dir, speed, damage);

    var state = JObjectPool.Shared<BulletLifetimeState>().Rent();
    state.Bullet = bullet;
    state.Manager = manager;

    using var action = await JAction.Create()
        .Delay(lifetime)
        .Do(static s => s.Manager?.Return(s.Bullet), state)
        .ExecuteAsync();

    state.Reset();
    JObjectPool.Shared<BulletLifetimeState>().Return(state);
}
```

## UI Patterns

### Delayed Tooltip (Zero-GC)
```csharp
public sealed class TooltipState
{
    public JAction Action;
    public Action ShowCallback;
    public Action HideCallback;

    public void Reset()
    {
        Action = null;
        ShowCallback = null;
        HideCallback = null;
    }
}

public sealed class TooltipTrigger : IDisposable
{
    private readonly TooltipState _state = new();

    public void OnPointerEnter(Action show, Action hide)
    {
        _state.Action?.Cancel();
        _state.Action?.Dispose();

        _state.ShowCallback = show;
        _state.HideCallback = hide;

        _state.Action = JAction.Create()
            .Delay(0.5f)
            .Do(static s => s.ShowCallback?.Invoke(), _state);

        _ = _state.Action.ExecuteAsync();
    }

    public void OnPointerExit()
    {
        _state.Action?.Cancel();
        _state.Action?.Dispose();
        _state.Action = null;
        _state.HideCallback?.Invoke();
    }

    /// <summary>
    /// Call from OnDestroy to prevent callbacks on destroyed objects.
    /// </summary>
    public void Dispose()
    {
        _state.Action?.Cancel();
        _state.Action?.Dispose();
        _state.Reset();
    }
}
```

### Typewriter Effect (Zero-GC)
```csharp
public sealed class TypewriterState
{
    public string FullText;
    public int CurrentIndex;
    public Action<string> OnUpdate;
    public StringBuilder Builder;

    public void Reset()
    {
        FullText = null;
        CurrentIndex = 0;
        OnUpdate = null;
        Builder = null;
    }
}

public static async UniTask TypeText(string content, float charDelay, Action<string> onUpdate)
{
    var state = JObjectPool.Shared<TypewriterState>().Rent();
    var sb = JObjectPool.Shared<StringBuilder>().Rent();

    state.FullText = content;
    state.CurrentIndex = 0;
    state.OnUpdate = onUpdate;
    state.Builder = sb;

    using var action = await JAction.Create()
        .Repeat(
            static s =>
            {
                if (s.CurrentIndex < s.FullText.Length)
                {
                    s.Builder.Append(s.FullText[s.CurrentIndex++]);
                    s.OnUpdate?.Invoke(s.Builder.ToString());
                }
            },
            state,
            count: content.Length,
            interval: charDelay)
        .ExecuteAsync();

    sb.Clear();
    JObjectPool.Shared<StringBuilder>().Return(sb);

    state.Reset();
    JObjectPool.Shared<TypewriterState>().Return(state);
}
```

## Best Practices

1. **Always use ExecuteAsync()** - Non-blocking, proper frame delays
2. **Always use `using var`** - Ensures JAction returns to pool
3. **Use static lambdas + state** - Avoids closure allocations
4. **State must be reference type** - Value types get boxed
5. **Pool state objects** - Use JObjectPool.Shared<T>() for state classes
6. **Reset state on return** - Clear all fields to prevent data leaks
7. **Use `in` parameters** - Avoid struct copies for Vector3, etc.
8. **Prewarm pools during loading** - Avoid runtime allocations
9. **Set timeouts** - Prevent infinite waits in production
10. **Cancel actions on disable** - Avoid callbacks on destroyed objects
