# Section 25 — Form Validation, State Persistence & Undo/Redo

## Overview

Three subsystems that extend JUI's reactive primitives with production-critical infrastructure:

1. **Form Validation** -- A `[Validate]` attribute on signal properties triggers source generation of per-field error signals and an `IsFormValid` computed signal. Built-in rules cover common patterns (not-empty, email, range, regex); custom validators implement `IValidator<T>`. Validation is reactive: error state updates automatically when the source signal changes.

2. **State Persistence** -- A `[Persist]` attribute on signal properties auto-registers them with `UIStatePersistence`, which tracks signal-key pairs and saves/restores values through a pluggable `IStateBackend`. The default backend uses `PlayerPrefs`; consumers can swap in file-based, cloud-based, or in-memory backends.

3. **Undo/Redo** -- A command-pattern `UndoRedoManager` with scoped stacks (via DI), `IUndoCommand` interface, and built-in `SignalUndoCommand<T>` / `BatchUndoCommand` types. The `[Undoable]` attribute on signal properties triggers source generation that wraps every `Value` set in an auto-recorded undo command.

## Dependencies

- Section 1 (Reactive Primitives: Signal & Computed) -- signals are the target of all three subsystems.
- Section 9 (Source Generator Project Setup) -- `[Validate]`, `[Persist]`, and `[Undoable]` attributes are processed by Roslyn source generators.
- Section 4 (DI Container) -- scoped `UndoRedoManager` instances use `Scope.Provide<UndoRedoManager>()`.
- Section 2 (Effect System) -- validation re-runs are driven by effects tracking the source signal.

## File Structure

### Validation

- `Runtime/JUI/Validation/ValidationRule.cs` -- enum of built-in rule types.
- `Runtime/JUI/Validation/IValidator.cs` -- generic custom validator interface.
- `Runtime/JUI/Validation/ValidationResult.cs` -- readonly struct for valid/error state.
- `Runtime/JUI/Validation/BuiltInValidators.cs` -- static implementations for each `ValidationRule`.
- `Runtime/JUI/Attributes/ValidateAttribute.cs` -- attribute triggering source generation.
- `SourceGenerators/JEngine.JUI.Generators/ValidateGenerator.cs` -- emits error signals and `IsFormValid`.

### Persistence

- `Runtime/JUI/Persistence/UIStatePersistence.cs` -- static API for tracking, saving, restoring signals.
- `Runtime/JUI/Persistence/IStateBackend.cs` -- pluggable storage interface.
- `Runtime/JUI/Persistence/PlayerPrefsBackend.cs` -- default `IStateBackend` using `PlayerPrefs`.
- `Runtime/JUI/Attributes/PersistAttribute.cs` -- attribute triggering source generation.
- `SourceGenerators/JEngine.JUI.Generators/PersistGenerator.cs` -- emits `Track` calls at component init.

### Undo/Redo

- `Runtime/JUI/History/UndoRedoManager.cs` -- command stack manager with `CanUndo`/`CanRedo` signals.
- `Runtime/JUI/History/IUndoCommand.cs` -- interface for executable/undoable commands.
- `Runtime/JUI/History/SignalUndoCommand.cs` -- captures old/new signal values.
- `Runtime/JUI/History/BatchUndoCommand.cs` -- groups multiple commands into one logical step.
- `Runtime/JUI/Attributes/UndoableAttribute.cs` -- attribute triggering source generation.
- `SourceGenerators/JEngine.JUI.Generators/UndoableGenerator.cs` -- wraps signal sets with auto-recorded commands.

## API Design

### Validation

```csharp
/// <summary>
/// Built-in validation rule types. Each rule maps to a static validator in
/// <see cref="BuiltInValidators"/> that is resolved at source-generation time.
/// </summary>
public enum ValidationRule
{
    /// <summary>Value must not be null, empty, or whitespace (for strings).</summary>
    NotEmpty,

    /// <summary>String length must be at least <see cref="ValidateAttribute.Min"/>.</summary>
    MinLength,

    /// <summary>String length must be at most <see cref="ValidateAttribute.Max"/>.</summary>
    MaxLength,

    /// <summary>String must match a standard email pattern.</summary>
    Email,

    /// <summary>String must contain only numeric characters (with optional decimal point).</summary>
    Numeric,

    /// <summary>Numeric value must be within [<see cref="ValidateAttribute.Min"/>, <see cref="ValidateAttribute.Max"/>].</summary>
    Range,

    /// <summary>String must match the regex pattern in <see cref="ValidateAttribute.Pattern"/>.</summary>
    Regex,

    /// <summary>Value must equal the value of the signal named by <see cref="ValidateAttribute.MatchField"/>.</summary>
    Match,

    /// <summary>Use a custom <see cref="IValidator{T}"/> specified by <see cref="ValidateAttribute.ValidatorType"/>.</summary>
    Custom
}

/// <summary>
/// Generic validator interface for custom validation logic. Implement this interface
/// and reference the type in <see cref="ValidateAttribute.ValidatorType"/> to use with
/// <see cref="ValidationRule.Custom"/>.
/// </summary>
/// <typeparam name="T">The type of value to validate.</typeparam>
public interface IValidator<T>
{
    /// <summary>Validate the given value and return a result.</summary>
    /// <param name="value">The current signal value to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating valid or error with a key.</returns>
    ValidationResult Validate(T value);
}

/// <summary>
/// Immutable result of a validation check. Contains an <see cref="IsValid"/> flag and
/// an optional <see cref="ErrorKey"/> for localization lookup.
/// </summary>
public readonly struct ValidationResult
{
    /// <summary>Whether the validated value passed the rule.</summary>
    public bool IsValid { get; }

    /// <summary>
    /// A localization-friendly key identifying the error (e.g., "validation.required").
    /// Empty string when <see cref="IsValid"/> is true.
    /// </summary>
    public string ErrorKey { get; }

    /// <summary>A shared valid result instance. Use this instead of allocating new valid results.</summary>
    public static ValidationResult Valid { get; }

    /// <summary>Create an error result with the given localization key.</summary>
    /// <param name="errorKey">The error key for localized messages.</param>
    /// <returns>A new <see cref="ValidationResult"/> with <see cref="IsValid"/> = false.</returns>
    public static ValidationResult Error(string errorKey);
}

/// <summary>
/// Marks a signal property for source-generated validation. The generator creates a
/// companion error signal (<c>{PropertyName}Error</c>) and includes this property in
/// the auto-generated <c>IsFormValid</c> computed signal.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public sealed class ValidateAttribute : Attribute
{
    /// <summary>The built-in rule to apply.</summary>
    public ValidationRule Rule { get; }

    /// <summary>Error key for localization. Defaults to "validation.{rule}" if not set.</summary>
    public string ErrorKey { get; set; }

    /// <summary>Minimum value for MinLength and Range rules.</summary>
    public double Min { get; set; }

    /// <summary>Maximum value for MaxLength and Range rules.</summary>
    public double Max { get; set; }

    /// <summary>Regex pattern for the Regex rule.</summary>
    public string Pattern { get; set; }

    /// <summary>Name of the other signal property to match against for the Match rule.</summary>
    public string MatchField { get; set; }

    /// <summary>
    /// Custom validator type. Must implement <see cref="IValidator{T}"/> for the signal's value type.
    /// Used when Rule is <see cref="ValidationRule.Custom"/>.
    /// </summary>
    public Type ValidatorType { get; set; }

    /// <summary>Create a validate attribute with the given rule.</summary>
    /// <param name="rule">The validation rule to apply.</param>
    public ValidateAttribute(ValidationRule rule);
}

/// <summary>
/// Static implementations of built-in validation rules. Each method corresponds to a
/// <see cref="ValidationRule"/> enum value and is resolved by the source generator.
/// </summary>
public static class BuiltInValidators
{
    public static ValidationResult NotEmpty(string value);
    public static ValidationResult MinLength(string value, int minLength);
    public static ValidationResult MaxLength(string value, int maxLength);
    public static ValidationResult Email(string value);
    public static ValidationResult Numeric(string value);
    public static ValidationResult Range(double value, double min, double max);
    public static ValidationResult Regex(string value, string pattern);
    public static ValidationResult Match<T>(T value, T other);
}
```

### Persistence

```csharp
/// <summary>
/// Pluggable storage interface for signal state persistence. Implementations serialize
/// signal values to a backing store (PlayerPrefs, file, cloud, etc.).
/// </summary>
public interface IStateBackend
{
    /// <summary>Save a value under the given key.</summary>
    /// <typeparam name="T">The type of value to save.</typeparam>
    /// <param name="key">Unique string key identifying this signal's state.</param>
    /// <param name="value">The value to persist.</param>
    void Save<T>(string key, T value);

    /// <summary>Load a previously saved value for the given key.</summary>
    /// <typeparam name="T">The type of value to load.</typeparam>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">The fallback value if the key does not exist.</param>
    /// <returns>The loaded value, or <paramref name="defaultValue"/> if not found.</returns>
    T Load<T>(string key, T defaultValue);

    /// <summary>Remove a persisted value by key.</summary>
    /// <param name="key">The key to remove.</param>
    void Delete(string key);

    /// <summary>Check whether a value exists for the given key.</summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key has a persisted value.</returns>
    bool Has(string key);
}

/// <summary>
/// Default <see cref="IStateBackend"/> using Unity's PlayerPrefs. Serializes non-primitive
/// types to JSON via <c>JsonUtility</c>. Suitable for small amounts of state (settings,
/// preferences). Not suitable for large data sets.
/// </summary>
public sealed class PlayerPrefsBackend : IStateBackend
{
    public void Save<T>(string key, T value);
    public T Load<T>(string key, T defaultValue);
    public void Delete(string key);
    public bool Has(string key);
}

/// <summary>
/// Central registry for signal state persistence. Tracks signal-key pairs and provides
/// batch save/restore/clear operations against the configured <see cref="Backend"/>.
/// </summary>
public static class UIStatePersistence
{
    /// <summary>
    /// The active storage backend. Defaults to <see cref="PlayerPrefsBackend"/>.
    /// Set this before calling <see cref="RestoreAll"/> to use a different backend.
    /// </summary>
    public static IStateBackend Backend { get; set; }

    /// <summary>
    /// Register a signal for persistence under the given key. The signal's current value
    /// is NOT immediately saved -- call <see cref="SaveAll"/> to persist.
    /// </summary>
    /// <typeparam name="T">The signal value type.</typeparam>
    /// <param name="key">Unique persistence key (e.g., "settings.volume").</param>
    /// <param name="signal">The signal to track.</param>
    public static void Track<T>(string key, Signal<T> signal);

    /// <summary>
    /// Unregister a signal from persistence. Does NOT delete the persisted value --
    /// call <see cref="Backend"/>.Delete() explicitly if needed.
    /// </summary>
    /// <param name="key">The key to untrack.</param>
    public static void Untrack(string key);

    /// <summary>Save all tracked signals' current values to the backend.</summary>
    public static void SaveAll();

    /// <summary>
    /// Restore all tracked signals' values from the backend. Signals whose keys are not
    /// found in the backend retain their current values.
    /// </summary>
    public static void RestoreAll();

    /// <summary>Delete all persisted values from the backend and untrack all signals.</summary>
    public static void Clear();
}

/// <summary>
/// Marks a signal property for automatic persistence registration. The source generator
/// emits a <c>UIStatePersistence.Track(key, signal)</c> call during component initialization
/// and a <c>UIStatePersistence.Untrack(key)</c> call during disposal.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class PersistAttribute : Attribute
{
    /// <summary>
    /// The persistence key. Must be unique across the application.
    /// Convention: "category.name" (e.g., "settings.volume", "editor.theme").
    /// </summary>
    public string Key { get; }

    /// <summary>Create a persist attribute with the given key.</summary>
    /// <param name="key">Unique persistence key.</param>
    public PersistAttribute(string key);
}
```

### Undo/Redo

```csharp
/// <summary>
/// Interface for executable and undoable commands in the undo/redo system.
/// Commands encapsulate a state change that can be reversed.
/// </summary>
public interface IUndoCommand
{
    /// <summary>Execute the command (apply the state change).</summary>
    void Execute();

    /// <summary>Reverse the command (restore the previous state).</summary>
    void Undo();

    /// <summary>
    /// Human-readable description of this command for debugging or UI display
    /// (e.g., "Set Health to 80").
    /// </summary>
    string Description { get; }
}

/// <summary>
/// A command that captures a signal value change. Stores the old and new values and
/// applies/reverts them on Execute/Undo. Used internally by the <c>[Undoable]</c>
/// source generator and available for manual command creation.
/// </summary>
/// <typeparam name="T">The signal value type.</typeparam>
public sealed class SignalUndoCommand<T> : IUndoCommand
{
    /// <summary>Create a command that changes a signal from oldValue to newValue.</summary>
    /// <param name="signal">The target signal.</param>
    /// <param name="oldValue">The value before the change.</param>
    /// <param name="newValue">The value after the change.</param>
    /// <param name="description">Optional description. Defaults to "Set {signalName}".</param>
    public SignalUndoCommand(Signal<T> signal, T oldValue, T newValue, string description = null);

    /// <summary>Apply the new value to the signal.</summary>
    public void Execute();

    /// <summary>Restore the old value to the signal.</summary>
    public void Undo();

    /// <inheritdoc />
    public string Description { get; }
}

/// <summary>
/// A composite command that groups multiple <see cref="IUndoCommand"/> instances into
/// a single logical operation. Executing undoes/redoes all sub-commands atomically
/// inside a <see cref="Batch.Run"/> call to prevent intermediate effect runs.
/// </summary>
public sealed class BatchUndoCommand : IUndoCommand
{
    /// <summary>Create a batch command from an ordered list of sub-commands.</summary>
    /// <param name="commands">The commands to group. Executed in order, undone in reverse order.</param>
    /// <param name="description">Description of the batch operation.</param>
    public BatchUndoCommand(IReadOnlyList<IUndoCommand> commands, string description);

    /// <summary>Execute all sub-commands in order within a Batch.Run call.</summary>
    public void Execute();

    /// <summary>Undo all sub-commands in reverse order within a Batch.Run call.</summary>
    public void Undo();

    /// <inheritdoc />
    public string Description { get; }
}

/// <summary>
/// Manages undo and redo stacks for a scope. Provides reactive <see cref="CanUndo"/>
/// and <see cref="CanRedo"/> signals for binding to UI buttons. Supports a configurable
/// maximum history depth.
/// </summary>
public sealed class UndoRedoManager
{
    /// <summary>Create a new undo/redo manager with default settings.</summary>
    public UndoRedoManager();

    /// <summary>
    /// Execute a command and push it onto the undo stack. Clears the redo stack
    /// (executing a new command invalidates the redo history).
    /// </summary>
    /// <param name="command">The command to execute.</param>
    public void Execute(IUndoCommand command);

    /// <summary>
    /// Record a command on the undo stack WITHOUT executing it. Use when the state change
    /// has already been applied and you only need to record it for undo purposes.
    /// Clears the redo stack.
    /// </summary>
    /// <param name="command">The command to record.</param>
    public void Record(IUndoCommand command);

    /// <summary>
    /// Undo the most recent command. Pops from the undo stack and pushes onto the redo stack.
    /// No-op if the undo stack is empty.
    /// </summary>
    public void Undo();

    /// <summary>
    /// Redo the most recently undone command. Pops from the redo stack and pushes onto the
    /// undo stack. No-op if the redo stack is empty.
    /// </summary>
    public void Redo();

    /// <summary>Reactive signal indicating whether undo is available. Bind to UI button enabled state.</summary>
    public IReadOnlySignal<bool> CanUndo { get; }

    /// <summary>Reactive signal indicating whether redo is available. Bind to UI button enabled state.</summary>
    public IReadOnlySignal<bool> CanRedo { get; }

    /// <summary>
    /// Maximum number of commands to retain in the undo stack. When exceeded, the oldest
    /// command is discarded. Defaults to 50.
    /// </summary>
    public int MaxHistory { get; set; }

    /// <summary>Clear both undo and redo stacks.</summary>
    public void Clear();

    /// <summary>
    /// Begin a batch operation. All commands recorded between <see cref="BeginBatch"/>
    /// and <see cref="EndBatch"/> are grouped into a single <see cref="BatchUndoCommand"/>.
    /// </summary>
    /// <param name="description">Description of the batch operation.</param>
    public void BeginBatch(string description);

    /// <summary>
    /// End a batch operation and push the grouped <see cref="BatchUndoCommand"/> onto
    /// the undo stack. No-op if no batch is in progress.
    /// </summary>
    public void EndBatch();
}

/// <summary>
/// Marks a signal property for source-generated undo/redo integration. The generator
/// wraps the signal's Value setter to automatically record <see cref="SignalUndoCommand{T}"/>
/// on the nearest scoped <see cref="UndoRedoManager"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class UndoableAttribute : Attribute
{
    /// <summary>
    /// Optional description template. Use "{old}" and "{new}" placeholders for old/new values.
    /// Defaults to "Set {PropertyName}".
    /// </summary>
    public string DescriptionTemplate { get; set; }
}
```

## Data Structures

### Validation

| Type | Role |
|------|------|
| `Dictionary<string, (ISignalBase source, Delegate validator, Signal<ValidationResult> error)>` | Internal registry in the generated component partial, mapping property names to their validation state. |
| `ComputedSignal<bool> IsFormValid` | Generated computed that returns true only when all `{Prop}Error.Value.IsValid` are true. |
| `Signal<ValidationResult> {Prop}Error` | Per-field error signal. Updated reactively when the source signal changes. |

### Persistence

| Type | Role |
|------|------|
| `Dictionary<string, (ISignalBase signal, Type valueType)>` | Internal registry in `UIStatePersistence` mapping keys to tracked signals. |
| `IStateBackend` | Pluggable backend interface. Serialization strategy is implementation-dependent. |

### Undo/Redo

| Type | Role |
|------|------|
| `Stack<IUndoCommand> _undoStack` | LIFO stack of executed commands in `UndoRedoManager`. |
| `Stack<IUndoCommand> _redoStack` | LIFO stack of undone commands in `UndoRedoManager`. |
| `List<IUndoCommand> _batchBuffer` | Temporary buffer collecting commands during a `BeginBatch`/`EndBatch` block. |
| `Signal<bool> _canUndo` | Internal signal backing `CanUndo`. Updated after each `Execute`/`Undo`/`Redo`/`Clear`. |
| `Signal<bool> _canRedo` | Internal signal backing `CanRedo`. Updated after each `Execute`/`Undo`/`Redo`/`Clear`. |

## Implementation Notes

### Validation

- **Reactive error update**: The source generator creates an `Effect` per validated property. The effect reads the source signal's `Value`, runs the validator, and writes the result to the `{Prop}Error` signal. Because error signals are themselves signals, UI elements bound to them update automatically.
- **Multiple rules per property**: `[Validate]` is `AllowMultiple = true`. When multiple rules exist, the generated effect evaluates them in declaration order and reports the first failing rule. This prevents showing multiple errors simultaneously for a single field.
- **IsFormValid composition**: The generator collects all `{Prop}Error` signals in the component and creates `IsFormValid = Computed.From(static (errors) => errors.All(e => e.Value.IsValid), errorArray)`. This computed is exposed as a public `IReadOnlySignal<bool>` for form-level submit button binding.
- **Custom validators**: When `Rule = ValidationRule.Custom`, the generator resolves `ValidatorType` at compile time and emits `new TValidator().Validate(value)`. The validator is instantiated once and cached in a static field to avoid per-validation allocation.
- **Thread safety**: Validation runs within the effect system, which is main-thread only.

### Persistence

- **Track/Untrack lifecycle**: `Track` adds the signal to an internal dictionary keyed by the persistence key. `Untrack` removes it. The signal itself is not modified.
- **SaveAll serialization**: Iterates all tracked entries. For primitive types (`int`, `float`, `string`, `bool`), `PlayerPrefsBackend` uses the corresponding `PlayerPrefs.Set*` method. For complex types, `JsonUtility.ToJson` serializes to a string stored via `PlayerPrefs.SetString`.
- **RestoreAll deserialization**: Iterates all tracked entries. If the backend has a value for the key, deserializes it and sets `signal.Value`. This triggers normal reactive updates. If the key is missing, the signal retains its current (default) value.
- **Auto-save on change**: The `[Persist]` generator optionally emits an effect that calls `Backend.Save(key, signal.Value)` on every signal change. This is opt-in via `[Persist("key", AutoSave = true)]` to avoid excessive `PlayerPrefs` writes.
- **Backend swapping**: Setting `UIStatePersistence.Backend` does not automatically re-persist or re-load. Consumers should call `SaveAll()` with the old backend and `RestoreAll()` with the new backend if migration is needed.

### Undo/Redo

- **Execute vs. Record**: `Execute` both applies the command and records it. `Record` only records it. The `[Undoable]` generator uses `Record` because the signal value has already been set by the time the wrapper runs.
- **Redo invalidation**: Executing or recording a new command clears the redo stack entirely. This is the standard undo/redo behavior -- branching from an undo point discards the forward history.
- **MaxHistory enforcement**: When the undo stack exceeds `MaxHistory`, the bottom-most (oldest) command is dropped. This uses an internal `LinkedList<IUndoCommand>` to allow O(1) removal from both ends. The `Stack` description in Data Structures is conceptual; the actual implementation is a bounded doubly-linked list.
- **Batch execution**: `BatchUndoCommand.Execute()` and `Undo()` wrap all sub-commands in `Batch.Run` to ensure effects see only the final state after all sub-commands complete. Undo executes sub-commands in reverse declaration order.
- **Scoped managers via DI**: Each `UndoRedoManager` is an independent instance. Providing one via `Scope.Provide<UndoRedoManager>()` creates a scoped undo history. Child components within that scope share the same manager. A top-level global manager can be provided at the root scope, while editors or sub-panels can have their own isolated undo stacks.
- **[Undoable] signal wrapping**: The generator emits a partial property that intercepts `set` calls. Before applying the new value, it reads the old value via `Peek()`, applies the new value, then calls `manager.Record(new SignalUndoCommand<T>(signal, oldValue, newValue))`. The manager is resolved from the component's DI scope.
- **Thread safety**: Undo/redo operations are main-thread only.

## Source Generator Notes

### ValidateGenerator

- **Trigger**: `[Validate]` attribute on a property of type `Signal<T>` in a `partial class` inheriting from `Component`.
- **Validation**: Property must be of type `Signal<T>`. The class must be `partial`. For `ValidationRule.Custom`, `ValidatorType` must implement `IValidator<T>` where `T` matches the signal's type parameter. For `Match`, `MatchField` must reference another `Signal<T>` property with the same type parameter.
- **Output per property**: Emits a `Signal<ValidationResult> {PropertyName}Error` field initialized to `ValidationResult.Valid`. Emits an effect in `InitValidation()` that reads the source signal and writes to the error signal.
- **Output per component**: Emits `ComputedSignal<bool> IsFormValid` that reads all error signals. Emits `InitValidation()` and `DisposeValidation()` partial methods called by the component lifecycle.
- **Incremental**: Uses `ForAttributeWithMetadataName` keyed to `ValidateAttribute`. Regenerates when attributes or signal properties change.

### PersistGenerator

- **Trigger**: `[Persist]` attribute on a property of type `Signal<T>` in a `partial class` inheriting from `Component`.
- **Validation**: Property must be of type `Signal<T>`. The class must be `partial`. Key must be a non-empty string literal.
- **Output**: Emits `UIStatePersistence.Track(key, signal)` in `InitPersistence()` and `UIStatePersistence.Untrack(key)` in `DisposePersistence()`. If `AutoSave = true`, emits an additional effect that saves on change.
- **Incremental**: Uses `ForAttributeWithMetadataName` keyed to `PersistAttribute`.

### UndoableGenerator

- **Trigger**: `[Undoable]` attribute on a property of type `Signal<T>` in a `partial class` inheriting from `Component`.
- **Validation**: Property must be of type `Signal<T>`. The class must be `partial`. The component's DI scope must be able to resolve `UndoRedoManager` (this is a runtime check, not a compile-time check).
- **Output**: Emits a wrapper method `Set{PropertyName}(T newValue)` that captures the old value, applies the new value, and records a `SignalUndoCommand<T>`. Also emits initialization code that resolves `UndoRedoManager` from the DI scope and caches it.
- **Incremental**: Uses `ForAttributeWithMetadataName` keyed to `UndoableAttribute`.

## Usage Examples

### Form Validation

```csharp
public partial class SignUpForm : Component
{
    [Validate(ValidationRule.NotEmpty, ErrorKey = "validation.required")]
    [Validate(ValidationRule.MinLength, Min = 3, ErrorKey = "validation.username.min")]
    [Validate(ValidationRule.MaxLength, Max = 20, ErrorKey = "validation.username.max")]
    [BindSync(nameof(El.UsernameInput))]
    public Signal<string> Username { get; } = new("");

    [Validate(ValidationRule.NotEmpty, ErrorKey = "validation.required")]
    [Validate(ValidationRule.Email, ErrorKey = "validation.email.invalid")]
    [BindSync(nameof(El.EmailInput))]
    public Signal<string> Email { get; } = new("");

    [Validate(ValidationRule.NotEmpty, ErrorKey = "validation.required")]
    [Validate(ValidationRule.MinLength, Min = 8, ErrorKey = "validation.password.min")]
    [BindSync(nameof(El.PasswordInput))]
    public Signal<string> Password { get; } = new("");

    [Validate(ValidationRule.Match, MatchField = nameof(Password), ErrorKey = "validation.password.mismatch")]
    [BindSync(nameof(El.ConfirmPasswordInput))]
    public Signal<string> ConfirmPassword { get; } = new("");

    // Generated by ValidateGenerator:
    // public Signal<ValidationResult> UsernameError { get; }
    // public Signal<ValidationResult> EmailError { get; }
    // public Signal<ValidationResult> PasswordError { get; }
    // public Signal<ValidationResult> ConfirmPasswordError { get; }
    // public IReadOnlySignal<bool> IsFormValid { get; }

    protected override Element Render() => El.Div(
        El.TextInput("Username").BindSync(Username)
            .WithError(UsernameError),
        El.TextInput("Email").BindSync(Email)
            .WithError(EmailError),
        El.TextInput("Password", mask: true).BindSync(Password)
            .WithError(PasswordError),
        El.TextInput("Confirm Password", mask: true).BindSync(ConfirmPassword)
            .WithError(ConfirmPasswordError),
        El.Button("Sign Up")
            .Enabled(IsFormValid)
            .OnClick(static (_, self) => self.Submit(), this)
    );
}

// Custom validator example
public class StrongPasswordValidator : IValidator<string>
{
    public ValidationResult Validate(string value)
    {
        if (string.IsNullOrEmpty(value)) return ValidationResult.Valid; // NotEmpty handles this
        bool hasUpper = value.Any(char.IsUpper);
        bool hasDigit = value.Any(char.IsDigit);
        bool hasSpecial = value.Any(c => !char.IsLetterOrDigit(c));
        if (!hasUpper || !hasDigit || !hasSpecial)
            return ValidationResult.Error("validation.password.weak");
        return ValidationResult.Valid;
    }
}

// Usage with custom validator
[Validate(ValidationRule.Custom, ValidatorType = typeof(StrongPasswordValidator))]
public Signal<string> Password { get; } = new("");
```

### State Persistence

```csharp
public partial class SettingsPanel : Component
{
    [Persist("settings.volume")]
    public Signal<float> Volume { get; } = new(0.8f);

    [Persist("settings.music")]
    public Signal<bool> MusicEnabled { get; } = new(true);

    [Persist("settings.language")]
    public Signal<string> Language { get; } = new("en");

    // Generated by PersistGenerator:
    // partial void InitPersistence()
    // {
    //     UIStatePersistence.Track("settings.volume", Volume);
    //     UIStatePersistence.Track("settings.music", MusicEnabled);
    //     UIStatePersistence.Track("settings.language", Language);
    // }
    // partial void DisposePersistence()
    // {
    //     UIStatePersistence.Untrack("settings.volume");
    //     UIStatePersistence.Untrack("settings.music");
    //     UIStatePersistence.Untrack("settings.language");
    // }

    protected override void OnInit()
    {
        UIStatePersistence.RestoreAll(); // Load saved settings
    }

    private void OnSaveClicked()
    {
        UIStatePersistence.SaveAll(); // Persist all tracked signals
    }
}

// Custom backend example
public class FileBackend : IStateBackend
{
    private readonly string _filePath;
    private Dictionary<string, string> _data;

    public FileBackend(string filePath) { _filePath = filePath; Load(); }
    public void Save<T>(string key, T value) { _data[key] = JsonUtility.ToJson(value); Flush(); }
    public T Load<T>(string key, T defaultValue) =>
        _data.TryGetValue(key, out var json) ? JsonUtility.FromJson<T>(json) : defaultValue;
    public void Delete(string key) { _data.Remove(key); Flush(); }
    public bool Has(string key) => _data.ContainsKey(key);
    private void Load() { /* read from file */ }
    private void Flush() { /* write to file */ }
}

// Swap backend at startup
UIStatePersistence.Backend = new FileBackend(Application.persistentDataPath + "/ui-state.json");
UIStatePersistence.RestoreAll();
```

### Undo/Redo

```csharp
// Scoped undo manager for a level editor
public partial class LevelEditor : Component
{
    private readonly UndoRedoManager _undoManager = new() { MaxHistory = 100 };

    protected override void OnInit()
    {
        Scope.Provide(_undoManager);
    }

    protected override Element Render() => El.Div(
        El.Button("Undo").OnClick(static (_, mgr) => mgr.Undo(), _undoManager)
            .Enabled(_undoManager.CanUndo),
        El.Button("Redo").OnClick(static (_, mgr) => mgr.Redo(), _undoManager)
            .Enabled(_undoManager.CanRedo),
        El.Create<TileEditor>()
    );
}

// Child component with [Undoable] signals
public partial class TileEditor : Component
{
    [Undoable(DescriptionTemplate = "Set tile type to {new}")]
    public Signal<TileType> SelectedTile { get; } = new(TileType.Grass);

    [Undoable]
    public Signal<int> BrushSize { get; } = new(1);

    // Generated by UndoableGenerator:
    // private UndoRedoManager _undoRedoManager;
    // partial void InitUndoable()
    // {
    //     _undoRedoManager = Scope.Resolve<UndoRedoManager>();
    // }
    // public void SetSelectedTile(TileType newValue)
    // {
    //     var oldValue = SelectedTile.Peek();
    //     if (EqualityComparer<TileType>.Default.Equals(oldValue, newValue)) return;
    //     SelectedTile.Value = newValue;
    //     _undoRedoManager.Record(new SignalUndoCommand<TileType>(
    //         SelectedTile, oldValue, newValue, $"Set tile type to {newValue}"));
    // }
}

// Manual batch undo
public void PlaceMultipleTiles(IReadOnlyList<(int x, int y, TileType type)> tiles)
{
    var undoManager = Scope.Resolve<UndoRedoManager>();
    undoManager.BeginBatch("Place multiple tiles");
    foreach (var (x, y, type) in tiles)
    {
        var oldType = _grid[x, y].Peek();
        _grid[x, y].Value = type;
        undoManager.Record(new SignalUndoCommand<TileType>(
            _grid[x, y], oldType, type));
    }
    undoManager.EndBatch();
    // All tile changes undo/redo as a single operation
}

// Manual command for non-signal operations
public class AddEntityCommand : IUndoCommand
{
    private readonly EntityManager _manager;
    private readonly EntityData _data;
    private Entity _entity;

    public AddEntityCommand(EntityManager manager, EntityData data)
    {
        _manager = manager;
        _data = data;
    }

    public string Description => $"Add {_data.Name}";
    public void Execute() => _entity = _manager.Spawn(_data);
    public void Undo() => _manager.Despawn(_entity);
}

var undoManager = Scope.Resolve<UndoRedoManager>();
undoManager.Execute(new AddEntityCommand(entityManager, enemyData));
// Entity spawned and recorded -- Ctrl+Z will despawn it
```

## Test Plan

### Validation

1. **NotEmpty rule rejects empty string**: Create a signal with `[Validate(NotEmpty)]`, set value to `""`, verify `{Prop}Error.Value.IsValid` is false.
2. **NotEmpty rule accepts non-empty string**: Set value to `"hello"`, verify `{Prop}Error.Value.IsValid` is true.
3. **MinLength rule boundary**: Set value to string of exactly `Min` length, verify valid. Set to `Min - 1`, verify invalid.
4. **MaxLength rule boundary**: Set value to string of exactly `Max` length, verify valid. Set to `Max + 1`, verify invalid.
5. **Email rule accepts valid emails**: Test `"user@example.com"`, `"a@b.co"`, verify valid.
6. **Email rule rejects invalid emails**: Test `"notanemail"`, `"@missing.com"`, `"user@"`, verify invalid.
7. **Range rule boundaries**: Test min, max, mid, below-min, above-max values.
8. **Regex rule matches pattern**: Provide a custom regex, verify matching and non-matching strings.
9. **Match rule with matching signals**: Set two signals to same value, verify valid. Set different, verify invalid.
10. **Custom validator integration**: Implement `IValidator<string>`, apply via `[Validate(Custom)]`, verify it is called.
11. **IsFormValid reflects all fields**: Create component with three validated signals, set all valid, verify `IsFormValid.Value` is true. Set one invalid, verify `IsFormValid.Value` is false.
12. **Multiple rules per property**: Apply `NotEmpty` and `MinLength` to same property. Empty string triggers `NotEmpty` error (first rule). Short string triggers `MinLength` error.
13. **Error signal updates reactively**: Bind a label to `{Prop}Error`. Change signal value. Verify label updates without manual intervention.
14. **ErrorKey is correct**: Verify that `ValidationResult.ErrorKey` matches the `ErrorKey` specified in the attribute.

### Persistence

15. **Track and SaveAll round-trip**: Track a signal, set value, call `SaveAll`, create new signal with same key, call `RestoreAll`, verify value restored.
16. **RestoreAll with missing key**: Track a signal with key not in backend, call `RestoreAll`, verify signal retains default value.
17. **Untrack prevents save**: Track a signal, call `Untrack`, call `SaveAll`, verify key not saved.
18. **Clear removes all data**: Track signals, save, call `Clear`, verify backend has no data.
19. **Custom backend integration**: Set `Backend` to a mock, call `SaveAll`, verify mock's `Save` called for each tracked signal.
20. **Complex type serialization**: Track a signal of a custom struct type, save and restore, verify all fields match.
21. **Multiple components share persistence**: Two components track signals with different keys, both save and restore correctly.
22. **Auto-save on change**: Use `[Persist("key", AutoSave = true)]`, change signal value, verify backend's `Save` called without explicit `SaveAll`.

### Undo/Redo

23. **Execute pushes to undo stack**: Execute a command, verify `CanUndo.Value` is true.
24. **Undo pops and pushes to redo**: Execute a command, call `Undo`, verify `CanUndo.Value` is false and `CanRedo.Value` is true.
25. **Redo pops and pushes to undo**: After undo, call `Redo`, verify `CanUndo.Value` is true and `CanRedo.Value` is false.
26. **Execute clears redo stack**: Execute A, undo, execute B, verify `CanRedo.Value` is false (A's redo discarded).
27. **MaxHistory eviction**: Set `MaxHistory = 3`, execute 5 commands, verify only last 3 are undoable.
28. **Clear empties both stacks**: Execute commands, call `Clear`, verify both `CanUndo` and `CanRedo` are false.
29. **SignalUndoCommand restores value**: Create a `SignalUndoCommand`, execute, verify new value. Undo, verify old value.
30. **BatchUndoCommand atomicity**: Create batch with 3 sub-commands, undo, verify all 3 reverted. Verify effects ran only once (Batch.Run wrapping).
31. **BeginBatch/EndBatch grouping**: Call `BeginBatch`, record 3 commands, call `EndBatch`, verify single undo reverts all 3.
32. **[Undoable] generated wrapper**: Set an `[Undoable]` signal via generated `Set{Prop}` method, verify undo restores previous value.
33. **Scoped managers are independent**: Create two `UndoRedoManager` instances in different scopes, execute commands on each, verify undo on one does not affect the other.
34. **Empty undo/redo is no-op**: Call `Undo` on empty stack, verify no exception and no state change.
35. **CanUndo/CanRedo are reactive**: Bind a button's enabled state to `CanUndo`, execute a command, verify button becomes enabled without manual update.

## Acceptance Criteria

### Validation

- [ ] `ValidationRule` enum contains all nine built-in rules
- [ ] `IValidator<T>` interface is generic and has a single `Validate` method
- [ ] `ValidationResult` is a readonly struct with `IsValid`, `ErrorKey`, static `Valid`, and static `Error(string)`
- [ ] `[Validate]` attribute supports `AllowMultiple = true` for stacking rules
- [ ] `ValidateGenerator` emits `{PropertyName}Error` signal per validated property
- [ ] `ValidateGenerator` emits `IsFormValid` computed signal per component
- [ ] Multiple rules on one property evaluate in declaration order, reporting first failure
- [ ] Custom validators are instantiated once and cached statically
- [ ] Error signals update reactively when source signal changes
- [ ] Generator emits diagnostic for non-Signal properties or missing `partial` keyword

### Persistence

- [ ] `IStateBackend` interface supports `Save`, `Load`, `Delete`, `Has` with generic type parameter
- [ ] `PlayerPrefsBackend` implements `IStateBackend` using `PlayerPrefs`
- [ ] `UIStatePersistence.Track` registers signal-key pairs
- [ ] `UIStatePersistence.SaveAll` persists all tracked signal values
- [ ] `UIStatePersistence.RestoreAll` loads and sets all tracked signal values
- [ ] `UIStatePersistence.Clear` removes all persisted data and untracks all signals
- [ ] `[Persist]` generator emits `Track` at init and `Untrack` at dispose
- [ ] Backend is swappable at runtime via `UIStatePersistence.Backend`
- [ ] Complex types serialize via `JsonUtility` in `PlayerPrefsBackend`

### Undo/Redo

- [ ] `IUndoCommand` interface has `Execute`, `Undo`, and `Description`
- [ ] `SignalUndoCommand<T>` captures and restores signal values
- [ ] `BatchUndoCommand` wraps sub-commands in `Batch.Run` for atomicity
- [ ] `UndoRedoManager.Execute` pushes to undo stack and clears redo stack
- [ ] `UndoRedoManager.Record` records without executing
- [ ] `UndoRedoManager.Undo` and `Redo` transfer commands between stacks
- [ ] `CanUndo` and `CanRedo` are reactive `IReadOnlySignal<bool>`
- [ ] `MaxHistory` enforces bounded undo stack with O(1) eviction
- [ ] `BeginBatch`/`EndBatch` groups commands into a single `BatchUndoCommand`
- [ ] `[Undoable]` generator emits `Set{PropertyName}` wrapper with auto-recorded command
- [ ] `[Undoable]` generator resolves `UndoRedoManager` from DI scope
- [ ] Scoped `UndoRedoManager` instances are fully independent
- [ ] All public APIs have XML documentation
- [ ] Zero allocations on undo/redo hot path (command objects are allocated on execute, not on undo/redo)
