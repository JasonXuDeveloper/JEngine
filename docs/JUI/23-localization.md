# Section 23 — Localization

## Overview

JUI's localization system provides reactive, signal-driven locale management. When the active locale changes, every localized string in the UI updates automatically — no manual refresh or rebuild required.

The system has three layers:

1. **LocaleManager** — The static entry point. Holds the `CurrentLocale` signal, produces reactive localized strings via `Localized()` and `Formatted()`, and manages locale table registration.
2. **ILocaleTable** — An interface for locale data sources. JUI ships `JsonLocaleTable` for JSON files but consumers can implement custom backends (CSV, ScriptableObject, remote fetch, etc.).
3. **[Loc] Attribute** — A declarative attribute for binding localized strings to component properties. The source generator validates that referenced keys exist in locale files at compile time.

All localized strings are `IReadOnlySignal<string>`, so they integrate seamlessly with JUI's binding, effect, and rendering systems.

## Dependencies

| Section | What It Provides |
|---------|-----------------|
| 1 — Reactive Primitives | `Signal<T>`, `IReadOnlySignal<T>` for reactive locale strings |
| 2 — Effect System | `Effect()` for reacting to locale changes in custom logic |
| 5 — Binding System | `[Bind]` cooperates with `[Loc]` to push localized text to elements |
| 9 — Generator Setup | Source generator processes `[Loc]` attributes for compile-time key validation |

## File Structure

```
Packages/com.jasonxudeveloper.jengine.ui/
└── Runtime/JUI/Localization/
    ├── LocaleManager.cs        # Static API: locale switching, Localized(), Formatted()
    ├── ILocaleTable.cs         # Interface for locale data providers
    ├── JsonLocaleTable.cs      # JSON-file-backed locale table implementation
    └── Attributes/
        └── LocAttribute.cs     # Declarative localized string binding attribute
```

## API Design

### LocaleManager

```csharp
/// <summary>
/// Central localization manager. Manages locale tables, provides reactive localized strings.
/// </summary>
public static class LocaleManager
{
    /// <summary>
    /// The currently active locale identifier (e.g., "en", "ja", "zh-CN").
    /// Setting this signal triggers all Localized/Formatted signals to re-evaluate.
    /// </summary>
    public static Signal<string> CurrentLocale { get; }

    /// <summary>
    /// Returns a reactive signal that resolves to the localized string for the given key.
    /// Updates automatically when CurrentLocale changes.
    /// Returns the key itself (wrapped in brackets) if not found: "[missing.key]".
    /// </summary>
    public static IReadOnlySignal<string> Localized(string key);

    /// <summary>
    /// Returns a reactive signal that resolves to a formatted localized string.
    /// Supports positional placeholders ({0}, {1}, ...) with reactive arguments.
    /// Re-evaluates when the locale changes OR when any argument signal changes.
    /// </summary>
    public static IReadOnlySignal<string> Formatted(string key, params IReadOnlySignal<object>[] args);

    /// <summary>
    /// Set the active locale by identifier. Equivalent to CurrentLocale.Value = locale.
    /// Logs a warning if no table is registered for the locale.
    /// </summary>
    public static void SetLocale(string locale);

    /// <summary>
    /// Register a locale table. Multiple tables can be registered for the same locale
    /// (keys are merged, later registrations override earlier ones for duplicate keys).
    /// </summary>
    public static void RegisterTable(ILocaleTable table);

    /// <summary>
    /// Unregister a locale table.
    /// </summary>
    public static void UnregisterTable(ILocaleTable table);

    /// <summary>
    /// Get all registered locale identifiers.
    /// </summary>
    public static IReadOnlyList<string> AvailableLocales { get; }

    /// <summary>
    /// Get the raw (non-reactive) string for a key in the current locale.
    /// Returns null if not found. Useful for one-shot lookups.
    /// </summary>
    public static string GetRaw(string key);

    /// <summary>
    /// Event raised when a key lookup fails (key not found in any table for the locale).
    /// Useful for logging or development-time missing key tracking.
    /// </summary>
    public static event Action<string, string> OnMissingKey; // (key, locale)
}
```

### ILocaleTable

```csharp
/// <summary>
/// Interface for locale data providers.
/// Implementations load localized strings from any source (JSON, CSV, database, etc.).
/// </summary>
public interface ILocaleTable
{
    /// <summary>
    /// The locale identifier this table provides strings for (e.g., "en", "ja").
    /// </summary>
    string Locale { get; }

    /// <summary>
    /// Try to get the localized string for a key.
    /// Returns true if the key exists in this table.
    /// </summary>
    bool TryGet(string key, out string value);

    /// <summary>
    /// All keys available in this table. Used for compile-time validation and tooling.
    /// </summary>
    IEnumerable<string> AllKeys { get; }
}
```

### JsonLocaleTable

```csharp
/// <summary>
/// Locale table backed by a JSON file.
/// Supports flat and nested JSON structures with dot-notation key paths.
/// </summary>
public class JsonLocaleTable : ILocaleTable
{
    /// <summary>
    /// The locale this table serves.
    /// </summary>
    public string Locale { get; }

    /// <summary>
    /// All keys in this table, flattened to dot-notation.
    /// </summary>
    public IEnumerable<string> AllKeys { get; }

    /// <summary>
    /// Create a locale table from a JSON string.
    /// </summary>
    public JsonLocaleTable(string locale, string json);

    /// <summary>
    /// Create a locale table from a TextAsset (loaded via Resources or Addressables).
    /// </summary>
    public JsonLocaleTable(string locale, TextAsset jsonAsset);

    /// <summary>
    /// Try to get a value by dot-notation key (e.g., "settings.audio.volume").
    /// </summary>
    public bool TryGet(string key, out string value);
}
```

### LocAttribute

```csharp
/// <summary>
/// Declaratively bind a localized string to a component property.
/// The source generator creates an IReadOnlySignal&lt;string&gt; that updates on locale change.
/// Combines with [Bind] to push localized text to named elements.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class LocAttribute : Attribute
{
    /// <summary>
    /// The localization key (e.g., "settings.title").
    /// Validated at compile time against registered locale files.
    /// </summary>
    public string Key { get; }

    public LocAttribute(string key) { }
}
```

## Data Structures

### Locale Table Registry

```
Dictionary<string, List<ILocaleTable>> _tables
  Key: locale identifier (e.g., "en")
  Value: ordered list of tables (later tables override earlier for duplicate keys)

Lookup order for a key:
  1. Iterate tables for CurrentLocale in reverse order (latest first)
  2. If found, return value
  3. If not found, fire OnMissingKey event
  4. Return "[key]" as fallback
```

### Localized Signal Cache

```
Dictionary<string, Signal<string>> _localizedCache
  Key: localization key
  Value: Signal that re-evaluates on locale change

On locale change:
  1. For each cached signal, re-lookup the key in the new locale's tables
  2. Update signal value (triggers all subscribers)

Cache is lazy — signals are created on first Localized() call.
```

### Formatted Signal

```
Each Formatted() call creates a Computed signal:
  - Depends on: CurrentLocale signal + all argument signals
  - Re-evaluates: string.Format(template, arg0.Value, arg1.Value, ...)
  - Template re-fetched from tables when locale changes
```

### JSON Structure Support

Flat JSON:
```json
{
  "settings.title": "Settings",
  "settings.audio.volume": "Volume"
}
```

Nested JSON (auto-flattened to dot-notation):
```json
{
  "settings": {
    "title": "Settings",
    "audio": {
      "volume": "Volume"
    }
  }
}
```

Both produce the same keys: `settings.title`, `settings.audio.volume`.

## Implementation Notes

### Reactive Locale Switching

The core mechanism relies on Signal subscriptions:

```csharp
public static IReadOnlySignal<string> Localized(string key)
{
    if (_localizedCache.TryGetValue(key, out var existing))
        return existing;

    var signal = new Signal<string>(ResolveKey(key, CurrentLocale.Value));
    _localizedCache[key] = signal;

    // Subscribe to locale changes
    CurrentLocale.Subscribe(locale =>
    {
        var resolved = ResolveKey(key, locale);
        signal.Value = resolved;
    });

    return signal;
}
```

This means switching locale is O(n) where n is the number of distinct keys currently in use (cached). In practice this is fast because it is a dictionary lookup per key, not a UI rebuild.

### Formatted Strings with Reactive Args

`Formatted()` creates a computed signal that depends on both the locale and all argument signals:

```csharp
public static IReadOnlySignal<string> Formatted(string key, params IReadOnlySignal<object>[] args)
{
    return Computed(() =>
    {
        var template = ResolveKey(key, CurrentLocale.Value);
        var values = new object[args.Length];
        for (int i = 0; i < args.Length; i++)
            values[i] = args[i].Value;
        return string.Format(template, values);
    });
}
```

This means if a locale template is `"Welcome, {0}! You have {1} items."` and `args[1]` changes, the string re-evaluates immediately without a locale switch.

### Missing Key Handling

When a key is not found:

1. The `OnMissingKey` event fires with `(key, locale)` — useful for development-time logging.
2. The returned string is the key wrapped in brackets: `"[missing.key]"`. This makes missing keys visible in the UI during development.
3. In release builds, the bracket format is still used but the event can be suppressed.

### Multiple Tables Per Locale

Multiple `ILocaleTable` instances can be registered for the same locale. This supports:

- **Modular loading**: Base strings in one table, feature-specific strings in another.
- **Override chains**: A DLC or mod can register a table that overrides base strings.
- **Hot reload**: Unregister old table, register new one, all signals update.

Tables are searched in reverse registration order (last registered wins).

## Source Generator Notes

The source generator processes `[Loc]` attributes on properties within `JUIComponent` subclasses and emits:

1. **Property backing signal**: Creates an `IReadOnlySignal<string>` backed by `LocaleManager.Localized(key)`.
2. **Compile-time key validation**: Scans JSON locale files referenced in the project for the specified key. Emits a warning if the key is not found in any locale file.
3. **Integration with `[Bind]`**: When both `[Loc]` and `[Bind]` are present, the generated code subscribes the localized signal to the bound element's text property.

Generated code pattern:

```csharp
// Source: user code
[Loc("settings.title"), Bind(nameof(El.TitleLabel))]
public IReadOnlySignal<string> Title { get; }

// Generated in partial class
partial void OnMount_Localization()
{
    Title = LocaleManager.Localized("settings.title");
    // [Bind] generator handles pushing Title.Value to El.TitleLabel.text
}
```

### Compile-Time Key Validation

The generator looks for locale JSON files in well-known paths:

1. `Assets/Locales/*.json`
2. `Assets/Resources/Locales/*.json`
3. Paths configured via `[assembly: LocaleSearchPath("...")]`

For each `[Loc("key")]`, the generator checks that `key` exists in at least one JSON file. If not found, it emits:

```
warning JUI2301: Localization key "settings.titl" not found in any locale file.
  Did you mean "settings.title"? [Levenshtein distance: 1]
```

The generator uses Levenshtein distance to suggest similar keys when a typo is detected.

## Usage Examples

### Basic Setup and Locale Registration

```csharp
// At application startup
public class LocaleBootstrap : MonoBehaviour
{
    [SerializeField] private TextAsset _englishJson;
    [SerializeField] private TextAsset _japaneseJson;
    [SerializeField] private TextAsset _chineseJson;

    void Awake()
    {
        LocaleManager.RegisterTable(new JsonLocaleTable("en", _englishJson));
        LocaleManager.RegisterTable(new JsonLocaleTable("ja", _japaneseJson));
        LocaleManager.RegisterTable(new JsonLocaleTable("zh-CN", _chineseJson));

        // Set initial locale (could come from PlayerPrefs)
        LocaleManager.SetLocale(
            PlayerPrefs.GetString("locale", "en"));
    }
}
```

### Declarative Localized UI

```csharp
public partial class SettingsScreen : JUIComponent
{
    [Loc("settings.title"), Bind(nameof(El.TitleLabel))]
    public IReadOnlySignal<string> Title { get; }

    [Loc("settings.audio"), Bind(nameof(El.AudioSectionLabel))]
    public IReadOnlySignal<string> AudioSection { get; }

    [Loc("settings.video"), Bind(nameof(El.VideoSectionLabel))]
    public IReadOnlySignal<string> VideoSection { get; }

    // All three labels update automatically when locale changes.
    // No additional code needed.
}
```

### Formatted Strings with Reactive Arguments

```csharp
public partial class PlayerHUD : JUIComponent
{
    private Signal<string> _playerName = new("Hero");
    private Signal<int> _itemCount = new(0);

    protected override void OnMount()
    {
        // Locale JSON: { "hud.welcome": "Welcome, {0}! You have {1} items." }
        // Japanese:    { "hud.welcome": "{0}さん、アイテムが{1}個あります。" }

        var welcomeText = LocaleManager.Formatted("hud.welcome",
            _playerName.Select(n => (object)n),
            _itemCount.Select(c => (object)c));

        El.WelcomeLabel.BindText(welcomeText);

        // Updates when:
        // - _playerName changes
        // - _itemCount changes
        // - Locale switches from "en" to "ja"
    }
}
```

### Language Picker

```csharp
public partial class LanguageSelector : JUIComponent
{
    protected override void OnMount()
    {
        var locales = LocaleManager.AvailableLocales; // ["en", "ja", "zh-CN"]

        foreach (var locale in locales)
        {
            var button = new Button(() =>
            {
                LocaleManager.SetLocale(locale);
                PlayerPrefs.SetString("locale", locale);
            });
            button.text = locale.ToUpper();

            // Highlight current locale reactively
            Effect(() =>
            {
                bool isCurrent = LocaleManager.CurrentLocale.Value == locale;
                button.EnableInClassList("selected", isCurrent);
            });

            El.LocaleList.Add(button);
        }
    }
}
```

### Missing Key Debugging

```csharp
// During development, log missing keys
#if UNITY_EDITOR
LocaleManager.OnMissingKey += (key, locale) =>
{
    Debug.LogWarning($"[Localization] Missing key '{key}' for locale '{locale}'");
};
#endif
```

### Custom Locale Table (ScriptableObject)

```csharp
[CreateAssetMenu(menuName = "JUI/SO Locale Table")]
public class SOLocaleTable : ScriptableObject, ILocaleTable
{
    [SerializeField] private string _locale;
    [SerializeField] private List<LocaleEntry> _entries;

    public string Locale => _locale;

    public bool TryGet(string key, out string value)
    {
        var entry = _entries.Find(e => e.Key == key);
        if (entry != null) { value = entry.Value; return true; }
        value = null; return false;
    }

    public IEnumerable<string> AllKeys => _entries.Select(e => e.Key);

    [Serializable]
    private class LocaleEntry
    {
        public string Key;
        public string Value;
    }
}
```

### Hot Reload Locale Data

```csharp
// Swap locale table at runtime (e.g., after downloading updated translations)
public void ReloadLocale(string locale, string newJson)
{
    // Remove old table
    var oldTable = _activeTables[locale];
    LocaleManager.UnregisterTable(oldTable);

    // Register new table — all signals re-evaluate automatically
    var newTable = new JsonLocaleTable(locale, newJson);
    LocaleManager.RegisterTable(newTable);
    _activeTables[locale] = newTable;
}
```

## Test Plan

| # | Test Case | Expectation |
|---|-----------|-------------|
| 1 | `RegisterTable` then `Localized("key")` | Returns signal with correct value |
| 2 | `SetLocale` to different locale | All cached `Localized` signals update to new locale values |
| 3 | `Localized("missing.key")` | Returns signal with value `"[missing.key]"` |
| 4 | `Localized("missing.key")` fires `OnMissingKey` | Event receives `("missing.key", currentLocale)` |
| 5 | `Formatted("key", arg1, arg2)` | Returns correctly formatted string |
| 6 | `Formatted` arg signal changes | Formatted signal re-evaluates with new arg value |
| 7 | `Formatted` locale changes | Template re-fetched, formatted with current args |
| 8 | Register two tables for same locale, duplicate key | Later table's value wins |
| 9 | `UnregisterTable` then `Localized` | Falls back to remaining table or missing key |
| 10 | `JsonLocaleTable` with flat JSON | All keys accessible |
| 11 | `JsonLocaleTable` with nested JSON | Keys flattened to dot-notation |
| 12 | `JsonLocaleTable` with empty JSON | No keys, `TryGet` returns false |
| 13 | `[Loc]` attribute generates signal property | Property returns `IReadOnlySignal<string>` from `LocaleManager.Localized` |
| 14 | `[Loc]` with `[Bind]` | Localized text pushed to bound element |
| 15 | Generator: key exists in locale file | No warnings |
| 16 | Generator: key does not exist | Warning emitted with Levenshtein suggestion |
| 17 | `AvailableLocales` after registering 3 tables | Returns list of 3 locale identifiers |
| 18 | `GetRaw("key")` | Returns raw string, not a signal |
| 19 | `SetLocale` to unregistered locale | Warning logged, signals return missing key format |
| 20 | Concurrent `Localized` calls for same key | Same cached signal instance returned |

## Acceptance Criteria

- [ ] `LocaleManager.CurrentLocale` is a `Signal<string>` that triggers re-evaluation of all localized signals on change
- [ ] `Localized(key)` returns an `IReadOnlySignal<string>` that updates when the locale switches
- [ ] `Formatted(key, args)` returns a reactive signal that re-evaluates on locale change OR argument signal change
- [ ] `ILocaleTable` interface allows pluggable locale data sources (JSON, ScriptableObject, remote, etc.)
- [ ] `JsonLocaleTable` supports both flat and nested JSON structures with dot-notation key paths
- [ ] Multiple tables per locale are supported with last-registered-wins override semantics
- [ ] Missing keys return `"[key]"` format and fire `OnMissingKey` event for development-time tracking
- [ ] `[Loc]` attribute generates `IReadOnlySignal<string>` properties backed by `LocaleManager.Localized()`
- [ ] `[Loc]` combined with `[Bind]` pushes localized text to named elements automatically
- [ ] Source generator validates `[Loc]` keys against locale JSON files at compile time with typo suggestions
- [ ] Locale switching is O(n) in cached keys (dictionary lookups, no UI rebuild)
- [ ] `UnregisterTable` and re-registration support hot reload of locale data at runtime
