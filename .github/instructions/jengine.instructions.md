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
- UI package: `JEngine.UI`
- Hot update code: `HotUpdate.Code`
- **Exception**: `Assets/Scripts/` may contain user-level code without namespace (for user customization)

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

## Unit Testing

### Scope
Unit tests are required for all non-core JEngine packages — i.e. any package under `Packages/com.jasonxudeveloper.jengine.*` **except** `jengine.core`. This includes JEngine.UI, JEngine.Util, and any future packages.

### Coverage Requirement
New features and new logic MUST include unit tests targeting **93%+ code coverage**:
- All public methods, properties, and constructors
- Fluent API chaining
- Edge cases and error conditions
- Event handlers and callbacks (use reflection for private handlers)

### Test Modes
- **EditMode tests** (`Tests/Editor/`): Preferred for most logic — fast, no scene required.
- **PlayMode tests** (`Tests/Runtime/`): Use when the test needs a running game loop, MonoBehaviour lifecycle, or scene loading. PlayMode tests **must run non-interactively** (no user input, no manual scene setup). Use `[UnityTest]` with `UniTask.ToCoroutine()` for async PlayMode tests.

### Test Location
Tests mirror the source structure under each package's test folders:
```
Packages/com.jasonxudeveloper.jengine.ui/Editor/Components/Button/JButton.cs
  → Packages/com.jasonxudeveloper.jengine.ui/Tests/Editor/Components/Button/JButtonTests.cs

Packages/com.jasonxudeveloper.jengine.util/Runtime/JAction.cs
  → Packages/com.jasonxudeveloper.jengine.util/Tests/Editor/JActionTests.cs

# PlayMode tests when runtime behavior requires it:
Packages/com.jasonxudeveloper.jengine.util/Runtime/SomeFeature.cs
  → Packages/com.jasonxudeveloper.jengine.util/Tests/Runtime/SomeFeatureTests.cs
```

### EditMode Test Pattern
```csharp
[TestFixture]
public class MyComponentTests
{
    private MyComponent _component;

    [SetUp]
    public void SetUp()
    {
        _component = new MyComponent();
    }

    [Test]
    public void Constructor_Default_AddsBaseClass()
    {
        Assert.IsTrue(_component.ClassListContains("my-component"));
    }
}
```

### PlayMode Test Pattern
PlayMode tests must be fully automated — no interactive input or manual scene setup:
```csharp
[TestFixture]
public class MyRuntimeTests
{
    [UnityTest]
    public IEnumerator MyAsyncTest() => UniTask.ToCoroutine(async () =>
    {
        var go = new GameObject();
        var component = go.AddComponent<MyBehaviour>();
        await UniTask.DelayFrame(1);
        Assert.IsTrue(component.IsInitialized);
        Object.Destroy(go);
    });
}
```

### Testing Private Methods via Reflection
For private event handlers and internal styling methods:
```csharp
var method = typeof(MyComponent).GetMethod("OnMouseEnter",
    BindingFlags.NonPublic | BindingFlags.Instance);
method.Invoke(_component, new object[] { null });
Assert.AreEqual(expectedColor, _component.style.backgroundColor.value);
```

## Review Focus Areas

When reviewing JEngine code, check:
1. UniTask usage (not Task)
2. Domain reload handling in Editor code
3. Resource cleanup (ScriptableObjects, events)
4. Thread safety for callback-accessed state
5. Proper namespace usage
6. Unit tests with 93%+ coverage for new features/logic
