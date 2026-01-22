---
applyTo: "**/*.cs"
---

# JEngine C# Coding Instructions

## Project Context

JEngine is a Unity hot update framework using HybridCLR for runtime C# execution and YooAsset for resource management.

## Code Style

### Namespaces
- Runtime code: `JEngine.Core` or `JEngine.Core.*`
- Editor code: `JEngine.Core.Editor` or `JEngine.Core.Editor.*`
- Hot update code: `HotUpdate.Code`

### Async/Await
Always use `UniTask` instead of `Task`:
```csharp
// Good
public async UniTask DoSomethingAsync() { }

// Bad
public async Task DoSomethingAsync() { }
```

### XML Documentation
Document all public members:
```csharp
/// <summary>
/// Description of what this does.
/// </summary>
public void MyMethod() { }
```

## Editor Scripts

### Domain Reload Handling
Unity reloads the domain on recompile. Static state resets. For persistent state:
```csharp
// Use SessionState for editor session persistence
SessionState.SetBool("MyKey", value);
bool value = SessionState.GetBool("MyKey", defaultValue);
```

### Resource Cleanup
Always clean up ScriptableObjects and event handlers:
```csharp
static MyClass()
{
    EditorApplication.quitting += OnQuitting;
}

private static void OnQuitting()
{
    EditorApplication.quitting -= OnQuitting;
    if (_instance != null)
    {
        Object.Destroy(_instance);
        _instance = null;
    }
}
```

### Thread Safety
For properties accessed from callbacks:
```csharp
private static volatile bool _flag;
public static bool Flag => _flag;
```

## Encryption Code

Three algorithms supported: XOR, AES, ChaCha20
- Config classes in `Runtime/Encrypt/Config/`
- Bundle encryption in `Runtime/Encrypt/Bundle/`
- Manifest encryption in `Runtime/Encrypt/Manifest/`

## Common Patterns

### ScriptableObject Singletons
```csharp
public class MyConfig : ScriptableObject
{
    private static MyConfig _instance;
    public static MyConfig Instance => _instance ??= CreateInstance<MyConfig>();
}
```

### InitializeOnLoad
```csharp
[InitializeOnLoad]
internal class MyEditorClass
{
    static MyEditorClass()
    {
        // Runs on editor load and domain reload
    }
}
```

## Review Focus Areas

When reviewing JEngine code, check:
1. UniTask usage (not Task)
2. Domain reload handling in Editor code
3. Resource cleanup (ScriptableObjects, events)
4. Thread safety for callback-accessed state
5. Proper namespace usage
