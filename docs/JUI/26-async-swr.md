# Section 26 — Async Integration & Data Caching (SWR)

## Overview

Three interrelated features for managing asynchronous data in JUI:

1. **AsyncSignal** -- A signal wrapper that models loading/loaded/error states for data fetched via `UniTask`. Provides a `State` signal (`Idle`, `Loading`, `Loaded`, `Error`), the resolved `Value`, and an `Error` property. Supports automatic cancellation on disposal and reload.

2. **Signal Extensions** -- Reactive combinators (`Debounce`, `Throttle`, `WaitUntil`, `WaitForChange`) that transform or await signal changes using `UniTask` and the frame-based effect system.

3. **SWR (Stale-While-Revalidate) Cache** -- A data-fetching and caching layer inspired by the SWR pattern. Returns stale data immediately while revalidating in the background. Supports deduplication of in-flight requests, time-based staleness, cache eviction, retry with exponential backoff, optimistic mutations, and key-based invalidation.

## Dependencies

- Section 1 (Reactive Primitives: Signal & Computed) -- `AsyncSignal` wraps `Signal<T>` and `Signal<AsyncState>`.
- Section 2 (Effect System & Batch) -- Signal extensions create internal effects for debounce/throttle logic.
- UniTask -- All async operations use `UniTask`, not `System.Threading.Tasks.Task`.

## File Structure

- `Runtime/JUI/Async/AsyncSignal.cs` -- Signal with loading state management.
- `Runtime/JUI/Async/AsyncState.cs` -- Enum for async lifecycle states.
- `Runtime/JUI/Async/SignalExtensions.cs` -- `Debounce`, `Throttle`, `WaitUntil`, `WaitForChange`.
- `Runtime/JUI/Async/SWRCache.cs` -- Global cache manager with deduplication and eviction.
- `Runtime/JUI/Async/SWRSignal.cs` -- Per-key cached signal with staleness tracking.
- `Runtime/JUI/Async/SWROptions.cs` -- Configuration struct for SWR fetch options.
- `Runtime/JUI/Async/SWRState.cs` -- Enum for SWR cache entry states.
- `Runtime/JUI/Attributes/SWRAttribute.cs` -- Source-generated SWR property binding.

## API Design

### AsyncSignal

```csharp
/// <summary>
/// Lifecycle states for an <see cref="AsyncSignal{T}"/>.
/// </summary>
public enum AsyncState
{
    /// <summary>No load has been initiated.</summary>
    Idle,

    /// <summary>A load operation is in progress.</summary>
    Loading,

    /// <summary>The most recent load completed successfully.</summary>
    Loaded,

    /// <summary>The most recent load failed with an exception.</summary>
    Error
}

/// <summary>
/// A reactive signal that wraps an asynchronous data-loading operation. Exposes the loaded
/// value, the current <see cref="AsyncState"/>, and any error. Automatically cancels in-flight
/// loads when a new load is initiated or the signal is disposed.
/// </summary>
/// <typeparam name="T">The type of the loaded data.</typeparam>
public sealed class AsyncSignal<T> : IReadOnlySignal<T>, IDisposable
{
    /// <summary>Create an async signal with an optional initial value.</summary>
    /// <param name="initialValue">The value to return before any load completes.</param>
    public AsyncSignal(T initialValue = default);

    /// <summary>
    /// The current loaded value. Returns the initial value if no load has completed,
    /// or the most recently loaded value. Reading this property tracks dependencies
    /// as with a normal signal.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// The current lifecycle state. Subscribe to this signal to reactively show
    /// loading spinners, error messages, or loaded content.
    /// </summary>
    public Signal<AsyncState> State { get; }

    /// <summary>
    /// The exception from the most recent failed load. Null if the last load succeeded
    /// or no load has been attempted.
    /// </summary>
    public Exception Error { get; }

    /// <summary>
    /// Initiate an async load. If a load is already in progress, it is cancelled before
    /// the new one starts. State transitions: Idle/Loaded/Error -> Loading -> Loaded|Error.
    /// </summary>
    /// <param name="loader">
    /// An async function that produces the value. Receives a <see cref="CancellationToken"/>
    /// that is cancelled if the load is superseded or the signal is disposed.
    /// </param>
    /// <returns>A UniTask that completes when the load finishes (success or failure).</returns>
    public UniTask LoadAsync(Func<CancellationToken, UniTask<T>> loader);

    /// <summary>
    /// Re-run the most recently provided loader function. No-op if <see cref="LoadAsync"/>
    /// has never been called. Cancels any in-flight load before starting.
    /// </summary>
    public void Reload();

    /// <summary>
    /// Directly set the value without loading. Transitions state to Loaded.
    /// Useful for optimistic updates or initializing with cached data.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public void SetValue(T value);

    /// <summary>Version tracking for dependency system.</summary>
    public int Version { get; }

    public void Subscribe(IEffect effect);
    public void Unsubscribe(IEffect effect);

    /// <summary>
    /// Cancel any in-flight load and release resources. The signal becomes inert
    /// after disposal (further loads are no-ops).
    /// </summary>
    public void Dispose();
}
```

### Signal Extensions

```csharp
/// <summary>
/// Extension methods for transforming and awaiting signal changes.
/// All returned signals are read-only and manage their own internal effects.
/// </summary>
public static class SignalExtensions
{
    /// <summary>
    /// Create a debounced signal that updates only after the source signal has been stable
    /// (unchanged) for the specified delay. Useful for search-as-you-type patterns where
    /// you want to wait until the user stops typing.
    /// </summary>
    /// <typeparam name="T">The signal value type.</typeparam>
    /// <param name="signal">The source signal to debounce.</param>
    /// <param name="delay">
    /// The quiet period required before the debounced signal updates. Each new source
    /// change resets the timer.
    /// </param>
    /// <returns>
    /// A new read-only signal whose value trails the source by <paramref name="delay"/>.
    /// Dispose the returned signal to stop the internal timer effect.
    /// </returns>
    public static IReadOnlySignal<T> Debounce<T>(this IReadOnlySignal<T> signal, TimeSpan delay);

    /// <summary>
    /// Create a throttled signal that updates at most once per interval, regardless of how
    /// frequently the source changes. The first change is emitted immediately; subsequent
    /// changes within the interval are dropped until the interval elapses.
    /// </summary>
    /// <typeparam name="T">The signal value type.</typeparam>
    /// <param name="signal">The source signal to throttle.</param>
    /// <param name="interval">The minimum time between updates.</param>
    /// <returns>
    /// A new read-only signal whose updates are rate-limited. Dispose to stop.
    /// </returns>
    public static IReadOnlySignal<T> Throttle<T>(this IReadOnlySignal<T> signal, TimeSpan interval);

    /// <summary>
    /// Await until the signal's value satisfies the given predicate. If the predicate is
    /// already satisfied, completes immediately. Otherwise, creates an internal effect that
    /// checks the predicate on each signal change and completes the UniTask when satisfied.
    /// </summary>
    /// <typeparam name="T">The signal value type.</typeparam>
    /// <param name="signal">The signal to watch.</param>
    /// <param name="predicate">The condition to wait for.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A UniTask that completes when the predicate is satisfied or the token is cancelled.</returns>
    public static UniTask WaitUntil<T>(
        this IReadOnlySignal<T> signal,
        Func<T, bool> predicate,
        CancellationToken ct = default);

    /// <summary>
    /// Await the next value change of the signal. Completes with the new value as soon as
    /// the signal's value changes (differs from the current value at the time of the call).
    /// </summary>
    /// <typeparam name="T">The signal value type.</typeparam>
    /// <param name="signal">The signal to watch.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A UniTask that completes with the new value on the next change.</returns>
    public static UniTask<T> WaitForChange<T>(
        this IReadOnlySignal<T> signal,
        CancellationToken ct = default);

    /// <summary>
    /// Create a mapped signal that transforms the source value through a pure function.
    /// Equivalent to <c>Computed.From(static (s) => selector(s.Value), signal)</c> but
    /// with a more fluent API.
    /// </summary>
    /// <typeparam name="TSource">The source signal value type.</typeparam>
    /// <typeparam name="TResult">The mapped result type.</typeparam>
    /// <param name="signal">The source signal.</param>
    /// <param name="selector">The mapping function.</param>
    /// <returns>A new read-only computed signal with the mapped value.</returns>
    public static IReadOnlySignal<TResult> Select<TSource, TResult>(
        this IReadOnlySignal<TSource> signal,
        Func<TSource, TResult> selector);
}
```

### SWR Cache

```csharp
/// <summary>
/// Cache entry lifecycle states for an <see cref="SWRSignal{TKey, TValue}"/>.
/// </summary>
public enum SWRState
{
    /// <summary>Data is within the stale time and considered fresh. No revalidation needed.</summary>
    Fresh,

    /// <summary>Data is past the stale time. The cached value is still returned, but a
    /// background revalidation has been initiated.</summary>
    Stale,

    /// <summary>A background revalidation is in progress. The cached (stale) value is still available.</summary>
    Revalidating,

    /// <summary>The most recent fetch or revalidation failed. The last known good value
    /// (if any) is still available.</summary>
    Error
}

/// <summary>
/// Configuration options for SWR fetch operations.
/// </summary>
public struct SWROptions
{
    /// <summary>
    /// Duration after a successful fetch during which the data is considered fresh.
    /// Accessing the data within this window returns the cached value without revalidation.
    /// Default: 60 seconds.
    /// </summary>
    public TimeSpan StaleTime { get; init; }

    /// <summary>
    /// Duration after a successful fetch during which the cache entry is retained.
    /// After this time, the entry is evicted and the next access triggers a fresh fetch.
    /// Must be greater than or equal to <see cref="StaleTime"/>. Default: 300 seconds.
    /// </summary>
    public TimeSpan CacheTime { get; init; }

    /// <summary>
    /// Number of retry attempts on fetch failure before entering the Error state.
    /// Default: 3.
    /// </summary>
    public int RetryCount { get; init; }

    /// <summary>
    /// Base delay between retry attempts. Actual delay uses exponential backoff:
    /// delay * 2^(attempt-1). Default: 1 second.
    /// </summary>
    public TimeSpan RetryDelay { get; init; }

    /// <summary>
    /// If true, automatically revalidate when the signal is subscribed to
    /// (i.e., when a component mounts that reads this signal). Default: true.
    /// </summary>
    public bool RevalidateOnSubscribe { get; init; }

    /// <summary>
    /// If true, automatically revalidate when the application regains focus.
    /// Useful for data that may have changed while the app was backgrounded.
    /// Default: false.
    /// </summary>
    public bool RevalidateOnFocus { get; init; }
}

/// <summary>
/// A reactive signal backed by a cached, revalidating data fetch. Returns stale data
/// immediately while revalidating in the background. The value, state, and error are
/// all reactive signals that automatically update UI bindings.
/// </summary>
/// <typeparam name="TKey">The cache key type (typically string).</typeparam>
/// <typeparam name="TValue">The fetched data type.</typeparam>
public sealed class SWRSignal<TKey, TValue> : IReadOnlySignal<TValue>, IDisposable
{
    /// <summary>
    /// The current cached value. Returns default if no data has been fetched yet.
    /// Reading this triggers revalidation if the data is stale.
    /// </summary>
    public TValue Value { get; }

    /// <summary>The current cache entry state (Fresh, Stale, Revalidating, Error).</summary>
    public Signal<SWRState> State { get; }

    /// <summary>The exception from the most recent failed fetch/revalidation. Null on success.</summary>
    public Exception Error { get; }

    /// <summary>The stale time configured for this signal.</summary>
    public TimeSpan StaleTime { get; }

    /// <summary>The cache eviction time configured for this signal.</summary>
    public TimeSpan CacheTime { get; }

    /// <summary>The retry count configured for this signal.</summary>
    public int RetryCount { get; }

    /// <summary>The base retry delay configured for this signal.</summary>
    public TimeSpan RetryDelay { get; }

    /// <summary>
    /// Mark the cached data as stale, triggering a background revalidation on the next
    /// access. If a revalidation is already in progress, it is cancelled and restarted.
    /// </summary>
    public void Invalidate();

    /// <summary>
    /// Optimistically update the cached value without waiting for a fetch. The state
    /// remains Fresh. Useful for updating the UI immediately after a mutation while
    /// the server confirms the change in the background.
    /// </summary>
    /// <param name="value">The optimistic value to set.</param>
    public void Mutate(TValue value);

    /// <summary>
    /// Optimistically update with a rollback on revalidation failure. If the background
    /// revalidation fails, the value reverts to <paramref name="rollbackValue"/>.
    /// </summary>
    /// <param name="value">The optimistic value to set.</param>
    /// <param name="rollbackValue">The value to restore if revalidation fails.</param>
    public void Mutate(TValue value, TValue rollbackValue);

    public int Version { get; }
    public void Subscribe(IEffect effect);
    public void Unsubscribe(IEffect effect);

    /// <summary>Cancel in-flight fetch, remove from SWRCache, release resources.</summary>
    public void Dispose();
}

/// <summary>
/// Global SWR cache manager. Provides factory methods for creating <see cref="SWRSignal{TKey, TValue}"/>
/// instances and manages cache-wide operations (invalidation, prefetch, eviction).
/// </summary>
public static class SWRCache
{
    /// <summary>
    /// Fetch data for the given key. If a cache entry exists and is fresh, returns the
    /// cached signal immediately (no fetch). If stale, returns the cached signal and
    /// triggers a background revalidation. If no entry exists, creates a new signal
    /// and initiates the first fetch.
    /// </summary>
    /// <typeparam name="TKey">The cache key type.</typeparam>
    /// <typeparam name="TValue">The data type.</typeparam>
    /// <param name="key">The cache key. Same key always returns the same signal instance.</param>
    /// <param name="fetcher">
    /// The async function to fetch data for the key. Called on first fetch and revalidation.
    /// Must be idempotent (safe to retry).
    /// </param>
    /// <param name="options">SWR configuration. Defaults are used if not specified.</param>
    /// <returns>The cached signal for this key.</returns>
    public static SWRSignal<TKey, TValue> Fetch<TKey, TValue>(
        TKey key,
        Func<TKey, UniTask<TValue>> fetcher,
        SWROptions options = default);

    /// <summary>
    /// Pre-fetch data for a key without returning a signal. The data is cached and will
    /// be available immediately when <see cref="Fetch{TKey, TValue}"/> is called later.
    /// Useful for prefetching data during idle time or based on predicted navigation.
    /// </summary>
    /// <typeparam name="TKey">The cache key type.</typeparam>
    /// <typeparam name="TValue">The data type.</typeparam>
    /// <param name="key">The cache key to prefetch.</param>
    /// <param name="fetcher">The async function to fetch data.</param>
    public static void Prefetch<TKey, TValue>(
        TKey key,
        Func<TKey, UniTask<TValue>> fetcher);

    /// <summary>
    /// Invalidate all cache entries, triggering revalidation for any active signals
    /// on their next access.
    /// </summary>
    public static void InvalidateAll();

    /// <summary>
    /// Invalidate all cache entries whose string key starts with the given prefix.
    /// Useful for invalidating a category of related data (e.g., all "user/*" entries).
    /// Only applicable when TKey is string.
    /// </summary>
    /// <param name="prefix">The prefix to match.</param>
    public static void InvalidateByPrefix(string prefix);

    /// <summary>
    /// Invalidate a single cache entry by key.
    /// </summary>
    /// <typeparam name="TKey">The cache key type.</typeparam>
    /// <param name="key">The key to invalidate.</param>
    public static void Invalidate<TKey>(TKey key);

    /// <summary>
    /// Remove all cache entries whose cache time has expired. Called automatically on a
    /// periodic timer (every 60 seconds by default). Can be called manually to force
    /// immediate eviction.
    /// </summary>
    public static void EvictExpired();

    /// <summary>
    /// Remove all cache entries. Active signals will refetch on next access.
    /// </summary>
    public static void ClearAll();
}

/// <summary>
/// Marks a field for source-generated SWR signal creation. The generator emits
/// initialization code that calls <see cref="SWRCache.Fetch{TKey, TValue}"/> with
/// the specified fetcher method and options.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class SWRAttribute : Attribute
{
    /// <summary>The cache key string.</summary>
    public string Key { get; }

    /// <summary>
    /// Name of the method on this component that serves as the fetcher function.
    /// Must have signature <c>UniTask&lt;TValue&gt; MethodName(TKey key)</c>.
    /// </summary>
    public string Fetcher { get; set; }

    /// <summary>Stale time in seconds. Default: 60.</summary>
    public float StaleTime { get; set; } = 60f;

    /// <summary>Cache time in seconds. Default: 300.</summary>
    public float CacheTime { get; set; } = 300f;

    /// <summary>Number of retry attempts. Default: 3.</summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>Base retry delay in seconds. Default: 1.</summary>
    public float RetryDelay { get; set; } = 1f;

    /// <summary>Create an SWR attribute with the given cache key.</summary>
    /// <param name="key">The cache key string.</param>
    public SWRAttribute(string key);
}
```

## Data Structures

| Type | Role |
|------|------|
| `Signal<T> _valueSignal` | Internal value signal inside `AsyncSignal<T>`. Holds the loaded value. |
| `Signal<AsyncState> State` | Public state signal on `AsyncSignal<T>`. Tracks Idle/Loading/Loaded/Error. |
| `CancellationTokenSource _cts` | Internal CTS in `AsyncSignal<T>`. Cancelled on reload, new load, or dispose. |
| `Func<CancellationToken, UniTask<T>> _lastLoader` | Stored loader for `Reload()` support in `AsyncSignal<T>`. |
| `Signal<T> _debouncedValue` | Internal output signal for `Debounce`. Updated after the quiet period. |
| `CancellationTokenSource _debounceCts` | Per-debounce timer CTS. Cancelled and recreated on each source change. |
| `float _lastEmitTime` | Timestamp for `Throttle`. Compared against `Time.unscaledTime` to enforce interval. |
| `Dictionary<(Type, object), object> _cache` | Internal cache store in `SWRCache`. Maps `(typeof(TValue), key)` to `SWRSignal` instances. |
| `Dictionary<(Type, object), UniTask> _inflight` | Deduplication map in `SWRCache`. Prevents multiple fetches for the same key. |
| `float _fetchTimestamp` | Per-entry timestamp in `SWRSignal`. Used to compute freshness against `StaleTime`. |
| `TValue _rollbackValue` | Stored rollback value in `SWRSignal` for optimistic mutation recovery. |

## Implementation Notes

### AsyncSignal

- **Cancellation on reload**: When `LoadAsync` is called while a previous load is in-flight, the old `CancellationTokenSource` is cancelled before creating a new one. This prevents stale results from overwriting fresh results in race conditions.
- **State transitions**: `Idle -> Loading -> Loaded` on success; `Idle -> Loading -> Error` on failure. `Reload()` transitions from `Loaded/Error -> Loading -> Loaded/Error`. The `State` signal is a normal `Signal<AsyncState>`, so all state transitions are reactive.
- **Error capture**: On `UniTask` failure, the exception is stored in the `Error` property and `State` transitions to `Error`. The previous `Value` is retained (not cleared) so the UI can show the last-known-good data alongside the error.
- **SetValue bypass**: `SetValue` directly sets the internal signal without going through a loader. This enables optimistic updates or initializing from local cache before the async load completes.
- **Dispose safety**: After disposal, `LoadAsync` and `Reload` are no-ops. `Value` continues to return the last value (no exception on read after dispose).

### Signal Extensions

- **Debounce implementation**: Creates an internal effect that watches the source signal. On each source change, cancels the previous `UniTask.Delay` and starts a new one. When the delay completes without cancellation, writes the source value to the output signal. Uses `Time.unscaledTime` to be independent of `Time.timeScale`.
- **Throttle implementation**: Creates an internal effect that watches the source signal. On each source change, checks whether `Time.unscaledTime - _lastEmitTime >= interval`. If yes, emits immediately and updates `_lastEmitTime`. If no, the change is dropped. An optional trailing-edge emission can be added via a parameter.
- **WaitUntil implementation**: Checks the predicate immediately. If satisfied, returns `UniTask.CompletedTask`. Otherwise, creates a `UniTaskCompletionSource`, subscribes an effect that checks the predicate on each change and calls `TrySetResult` when satisfied.
- **WaitForChange implementation**: Similar to `WaitUntil` but completes on any change (value differs from snapshot taken at call time).
- **Disposal**: All returned signals from `Debounce`/`Throttle`/`Select` implement `IDisposable`. Disposing them cleans up internal effects and cancellation tokens. The source signal is NOT disposed.

### SWR Cache

- **Deduplication**: When `Fetch` is called with a key that already has an in-flight request, the existing `UniTask` is awaited instead of starting a new fetch. The `_inflight` dictionary tracks in-progress fetches and is cleared when the fetch completes.
- **Stale-while-revalidate flow**: On access to a stale `SWRSignal`, the cached value is returned immediately (the read is synchronous), and a background `UniTask` is launched to revalidate. The `State` signal transitions to `Revalidating`. When the revalidation completes, `Value` is updated and `State` transitions to `Fresh`.
- **Retry with exponential backoff**: On fetch failure, the system retries up to `RetryCount` times with delays of `RetryDelay * 2^attempt`. If all retries fail, `State` transitions to `Error` and the exception is stored.
- **Optimistic mutation**: `Mutate(value)` immediately sets the cached value and keeps `State` as `Fresh`. The next revalidation will overwrite with server data. `Mutate(value, rollbackValue)` additionally stores a rollback value; if revalidation fails, the value reverts to `rollbackValue` instead of keeping the optimistic value.
- **Cache eviction**: A periodic timer (driven by `PlayerLoopTiming.EarlyUpdate`) calls `EvictExpired()` every 60 seconds. Entries past their `CacheTime` are removed. Active `SWRSignal` instances that are still subscribed (have effects reading them) are NOT evicted -- they are marked stale and revalidated on next access.
- **Prefetch**: `Prefetch` creates a cache entry and initiates a fetch, but does not return a signal. The data sits in the cache waiting for a `Fetch` call with the same key to pick it up.
- **Key-based invalidation**: `InvalidateByPrefix` iterates all string-keyed entries and invalidates those whose key starts with the prefix. This is O(n) over cache size but is expected to be called infrequently (e.g., after a mutation).
- **Thread safety**: All SWR operations are main-thread only. `UniTask` continuations are marshalled to the main thread via `PlayerLoopTiming`.

## Source Generator Notes

### SWRGenerator (in `SWRAttribute` processing)

- **Trigger**: `[SWR]` attribute on a field of type `SWRSignal<TKey, TValue>` in a `partial class` inheriting from `Component`.
- **Validation**: The field type must be `SWRSignal<TKey, TValue>`. The `Fetcher` method must exist on the component with signature `UniTask<TValue>(TKey)`. The class must be `partial`.
- **Output**: Emits initialization code in `InitSWR()` partial method:
  ```csharp
  partial void InitSWR()
  {
      _leaderboard = SWRCache.Fetch<string, LeaderboardData>(
          "leaderboard",
          static (key, self) => self.FetchLeaderboard(key),
          this,
          new SWROptions
          {
              StaleTime = TimeSpan.FromSeconds(30),
              CacheTime = TimeSpan.FromSeconds(300),
              RetryCount = 3,
              RetryDelay = TimeSpan.FromSeconds(1)
          });
  }
  ```
- **Disposal**: Emits `_leaderboard?.Dispose()` in `DisposeSWR()` partial method.
- **Incremental**: Uses `ForAttributeWithMetadataName` keyed to `SWRAttribute`.

## Usage Examples

### AsyncSignal

```csharp
public partial class PlayerProfile : Component
{
    private readonly AsyncSignal<PlayerData> _profile = new();

    protected override async UniTask OnInitAsync()
    {
        await _profile.LoadAsync(async ct =>
        {
            var response = await HttpClient.GetAsync("/api/player", ct);
            return JsonUtility.FromJson<PlayerData>(response);
        });
    }

    protected override Element Render() => El.Div(
        // Show different UI based on async state
        Show(_profile.State, static s => s == AsyncState.Loading,
            () => El.Create<Spinner>()),

        Show(_profile.State, static s => s == AsyncState.Error,
            () => El.Label($"Error: {_profile.Error.Message}")
                    .Add(El.Button("Retry").OnClick(static (_, p) => p.Reload(), _profile))),

        Show(_profile.State, static s => s == AsyncState.Loaded,
            () => El.Div(
                El.Label(_profile.Value.Name),
                El.Label($"Level {_profile.Value.Level}")
            ))
    );
}
```

### Signal Extensions

```csharp
// Debounced search input
public partial class SearchPanel : Component
{
    public Signal<string> SearchQuery { get; } = new("");
    private IReadOnlySignal<string> _debouncedQuery;
    private readonly AsyncSignal<SearchResult[]> _results = new();

    protected override void OnInit()
    {
        // Wait 300ms after the user stops typing before searching
        _debouncedQuery = SearchQuery.Debounce(TimeSpan.FromMilliseconds(300));

        // Effect watches the debounced query and triggers a search
        CreateEffect(static (state) =>
        {
            var query = state.debounced.Value;
            if (string.IsNullOrEmpty(query)) return;
            state.results.LoadAsync(ct => SearchAPI.Search(query, ct)).Forget();
        }, (debounced: _debouncedQuery, results: _results));
    }
}

// Throttled telemetry
var mousePosSignal = new Signal<Vector2>(Vector2.zero);
var throttledPos = mousePosSignal.Throttle(TimeSpan.FromMilliseconds(100));
// throttledPos updates at most 10 times per second

// Await signal condition
await healthSignal.WaitUntil(static h => h <= 0);
// Player is dead -- show game over screen

// Await any change
var newValue = await scoreSignal.WaitForChange();
// Score just changed to newValue

// Select (map) signal
var healthPct = healthSignal.Select(static h => h / 100f);
// healthPct.Value is always health / 100
```

### SWR Cache

```csharp
public partial class LeaderboardPanel : Component
{
    [SWR("leaderboard", Fetcher = nameof(FetchLeaderboard), StaleTime = 30)]
    private SWRSignal<string, LeaderboardData> _leaderboard;

    private async UniTask<LeaderboardData> FetchLeaderboard(string key)
    {
        var response = await HttpClient.GetAsync($"/api/{key}");
        return JsonUtility.FromJson<LeaderboardData>(response);
    }

    protected override Element Render() => El.Div(
        // Always shows data (stale or fresh) -- never a blank screen
        El.Create<LeaderboardTable>().Bind(_leaderboard),

        // Show a subtle revalidating indicator
        Show(_leaderboard.State, static s => s == SWRState.Revalidating,
            () => El.Create<Spinner>(size: SpinnerSize.Small)),

        // Refresh button
        El.Button("Refresh").OnClick(static (_, lb) => lb.Invalidate(), _leaderboard)
    );
}

// Manual SWR usage without attribute
var userSignal = SWRCache.Fetch<string, UserData>(
    "user/123",
    static async key => await API.GetUser(key),
    new SWROptions
    {
        StaleTime = TimeSpan.FromSeconds(30),
        CacheTime = TimeSpan.FromMinutes(5),
        RetryCount = 3,
        RetryDelay = TimeSpan.FromSeconds(1),
        RevalidateOnFocus = true
    });

// Optimistic update after mutation
await API.UpdateUserName("123", "NewName");
userSignal.Mutate(
    userSignal.Value with { Name = "NewName" },    // Optimistic value
    userSignal.Value                                  // Rollback if revalidation fails
);

// Prefetch data the user is likely to navigate to
SWRCache.Prefetch<string, ProfileData>("profile/456", static key => API.GetProfile(key));

// Invalidate related data after a mutation
await API.PostScore(newScore);
SWRCache.InvalidateByPrefix("leaderboard");  // All leaderboard variants revalidate

// Deduplication: multiple components requesting the same key share one fetch
var a = SWRCache.Fetch<string, UserData>("user/123", fetcher);
var b = SWRCache.Fetch<string, UserData>("user/123", fetcher);
// a and b are the same SWRSignal instance -- only one HTTP request is made
```

## Test Plan

### AsyncSignal

1. **Initial state is Idle**: Create an `AsyncSignal`, verify `State.Value == AsyncState.Idle` and `Value` is default.
2. **LoadAsync transitions to Loading then Loaded**: Call `LoadAsync`, verify `State` goes to `Loading`, await completion, verify `State` is `Loaded` and `Value` is the fetched value.
3. **LoadAsync failure transitions to Error**: Provide a loader that throws, verify `State` is `Error` and `Error` property is the exception.
4. **Reload re-runs last loader**: Call `LoadAsync`, change remote data, call `Reload`, verify `Value` updates to new data.
5. **Reload cancels in-flight load**: Start a slow `LoadAsync`, call `Reload`, verify the first load's CancellationToken is cancelled.
6. **Concurrent LoadAsync cancels previous**: Start `LoadAsync(A)`, immediately start `LoadAsync(B)`, verify only B's result is applied.
7. **SetValue bypasses loader**: Call `SetValue(x)`, verify `Value == x` and `State == Loaded`.
8. **Dispose cancels in-flight and prevents further loads**: Dispose during a load, verify no exception and subsequent `LoadAsync` is a no-op.
9. **Value retained on error**: Load successfully, then load again with a failing loader, verify `Value` is still the first result.
10. **State signal is reactive**: Subscribe an effect to `State`, trigger load, verify effect runs on each state transition.

### Signal Extensions

11. **Debounce delays output**: Set source 5 times rapidly, verify debounced signal updates only once (with the last value) after the delay.
12. **Debounce resets timer on each change**: Set source, wait half the delay, set again, verify output arrives delay-after-second-set (not delay-after-first).
13. **Throttle emits first immediately**: Set source, verify throttled signal updates immediately. Set again within interval, verify no update.
14. **Throttle emits after interval**: Set source within interval, wait for interval to elapse, verify the last value is emitted.
15. **WaitUntil completes immediately if satisfied**: Signal value already satisfies predicate, verify `WaitUntil` completes synchronously.
16. **WaitUntil waits for condition**: Signal value does not satisfy predicate, change value to satisfy, verify `WaitUntil` completes.
17. **WaitUntil respects cancellation**: Cancel the token before the condition is met, verify `OperationCanceledException`.
18. **WaitForChange completes on next change**: Call `WaitForChange`, change signal, verify it returns the new value.
19. **WaitForChange ignores same-value set**: Set signal to the same value (no actual change), verify `WaitForChange` does NOT complete.
20. **Select maps values reactively**: Create a `Select` signal, change source, verify the mapped signal updates.
21. **Dispose stops debounce/throttle**: Dispose the returned signal, change source, verify no output.

### SWR Cache

22. **First fetch creates signal and loads data**: Call `Fetch` with a new key, verify signal is created and data is loaded.
23. **Second fetch with same key returns same signal**: Call `Fetch` twice with the same key, verify reference equality.
24. **Fresh data not refetched**: Fetch, access within `StaleTime`, verify no second fetch.
25. **Stale data triggers revalidation**: Fetch, wait past `StaleTime`, access, verify background revalidation and `State` transitions to `Revalidating`.
26. **Revalidation updates value**: After stale revalidation completes, verify `Value` is updated and `State` is `Fresh`.
27. **Cache eviction after CacheTime**: Fetch, wait past `CacheTime`, call `EvictExpired`, verify entry removed. Next `Fetch` triggers a fresh load.
28. **Deduplication prevents concurrent fetches**: Call `Fetch` with the same key concurrently, verify only one fetcher invocation.
29. **Retry on failure**: Provide a fetcher that fails twice then succeeds, set `RetryCount = 3`, verify data loads on third attempt.
30. **Retry exhaustion transitions to Error**: Provide a fetcher that always fails, set `RetryCount = 2`, verify `State` is `Error` after 2 retries.
31. **Optimistic mutation updates value**: Call `Mutate(newValue)`, verify `Value == newValue` immediately.
32. **Optimistic mutation with rollback**: Call `Mutate(newValue, oldValue)`, trigger revalidation that fails, verify `Value` reverts to `oldValue`.
33. **InvalidateAll marks all entries stale**: Fetch multiple keys, call `InvalidateAll`, verify all signals' `State` transitions on next access.
34. **InvalidateByPrefix targets correct entries**: Fetch "user/1", "user/2", "post/1", call `InvalidateByPrefix("user/")`, verify only user entries are invalidated.
35. **Prefetch populates cache**: Call `Prefetch`, then `Fetch` with same key, verify no second fetch and data is immediately available.
36. **Dispose removes from cache**: Dispose an `SWRSignal`, verify it is removed from the cache and the key can be re-fetched.

## Acceptance Criteria

### AsyncSignal

- [ ] `AsyncSignal<T>` implements `IReadOnlySignal<T>` and `IDisposable`
- [ ] `State` signal transitions through `Idle -> Loading -> Loaded|Error` lifecycle
- [ ] `LoadAsync` cancels previous in-flight load via `CancellationTokenSource`
- [ ] `Reload()` re-runs the last provided loader
- [ ] `SetValue` directly sets value and transitions to `Loaded`
- [ ] `Error` property holds the exception from the last failed load
- [ ] Previous `Value` is retained on error (not cleared)
- [ ] Dispose cancels in-flight load and makes the signal inert

### Signal Extensions

- [ ] `Debounce` creates a new signal that updates only after the source is stable for the specified delay
- [ ] `Throttle` creates a new signal that updates at most once per interval
- [ ] `WaitUntil` returns a `UniTask` that completes when the predicate is satisfied
- [ ] `WaitForChange` returns a `UniTask<T>` that completes on the next value change
- [ ] `Select` creates a mapped read-only signal (equivalent to a computed)
- [ ] All returned signals implement `IDisposable` for cleanup
- [ ] Debounce and Throttle use `Time.unscaledTime` (not affected by timeScale)

### SWR Cache

- [ ] `SWRSignal<TKey, TValue>` implements `IReadOnlySignal<TValue>` and `IDisposable`
- [ ] `SWRCache.Fetch` returns the same signal instance for the same key (deduplication)
- [ ] Fresh data is returned without re-fetch within `StaleTime`
- [ ] Stale data triggers background revalidation while returning cached value
- [ ] Retry with exponential backoff up to `RetryCount` attempts
- [ ] `Mutate` provides optimistic updates with optional rollback
- [ ] `InvalidateAll` and `InvalidateByPrefix` trigger revalidation on next access
- [ ] `Prefetch` populates cache without returning a signal
- [ ] `EvictExpired` removes entries past `CacheTime` (but not active subscribed signals)
- [ ] `[SWR]` attribute triggers source generation of initialization and disposal code
- [ ] All public APIs have XML documentation
- [ ] Zero allocations on cache hit (reading a fresh `SWRSignal.Value`)
