---
applyTo: "**"
---

# Code Review Instructions for JEngine

## Priority Checks

### 1. Unity Editor Domain Reload
Editor code with static state MUST handle domain reloads:
- Static fields reset to default on recompile
- Use `SessionState` for session-persistent data
- Use `EditorPrefs` for cross-session data

### 2. Resource Management
Check for proper cleanup:
- ScriptableObjects created with `CreateInstance` must be destroyed
- Event handlers must be unsubscribed
- Use `Object.Destroy` (not `DestroyImmediate`) for most cases

### 3. Async Patterns
Verify async code uses:
- `UniTask` (not `System.Threading.Tasks.Task`)
- Proper `await` usage
- No blocking calls (`Result`, `Wait()`)

### 4. Thread Safety
For state accessed across Unity callbacks:
- Use `volatile` for simple flags
- Consider thread-safe patterns for complex state

### 5. Namespace Compliance
- Runtime: `JEngine.Core.*`
- Editor: `JEngine.Core.Editor.*`
- UI package: `JEngine.UI`
- Hot update: `HotUpdate.Code`
- **Exception**: `Assets/Scripts/` may contain user-level code without namespace (intentional for user customization)

### 6. Performance Patterns
Avoid LINQ in hot paths and UI code for performance:
- Use `for`/`foreach` loops with inline null checks instead of `.Where()`
- Use `Count > 0` or `Length > 0` instead of `.Any()`
- Use array/list indexing instead of `.First()` / `.Last()`
- LINQ allocates iterators and delegates - avoid in frequently called code

### 7. Unit Test Coverage
New features and new logic in non-core packages (JEngine.UI, JEngine.Util, and any future packages) MUST include unit tests:
- Target **93%+ code coverage** for all new/modified code
- **Applies to**: All `Packages/com.jasonxudeveloper.jengine.*` packages **except** `jengine.core`
- Prefer **EditMode tests** (`Tests/Editor/`) for most logic
- Use **PlayMode tests** (`Tests/Runtime/`) when runtime behavior requires it (MonoBehaviour lifecycle, scene loading, etc.) — these must run **non-interactively** (no user input, no manual scene setup)
- Cover: constructors, public API, fluent chaining, edge cases, event handlers
- Use reflection to test private methods (e.g. `OnAttachToPanel`, hover handlers) when they contain meaningful logic
- Verify tests exercise both happy paths and error/boundary conditions

## Common Issues to Flag

- Missing or insufficient unit tests for new features
- Missing XML documentation on public APIs
- Direct `Debug.Log` (should use proper logging)
- `Task` instead of `UniTask`
- Static state in Editor without domain reload handling
- Missing null checks for Unity objects
- `DestroyImmediate` without clear justification

## Suggestions Format

When suggesting code changes:
1. Explain the issue clearly
2. Provide complete code suggestion
3. Reference Unity/JEngine conventions
