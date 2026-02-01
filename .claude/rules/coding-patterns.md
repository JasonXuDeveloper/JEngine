# JEngine Coding Patterns

## Async Patterns

Use `UniTask` for async operations, not `System.Threading.Tasks.Task`:
```csharp
public async UniTask<bool> LoadAssetAsync() { }
```

## Thread Safety

For properties accessed across callbacks:
```csharp
private static volatile bool _flag;
public static bool Flag => _flag;
```

## Encryption Architecture

JEngine supports three encryption algorithms:
- **XOR** (`EncryptionOption.Xor`) - Fast, simple
- **AES** (`EncryptionOption.Aes`) - Moderate security
- **ChaCha20** (`EncryptionOption.ChaCha20`) - High security

Each has implementations for bundles and manifests in `Runtime/Encrypt/`.

### Adding New Encryption

1. Create config class in `Runtime/Encrypt/Config/`
2. Implement bundle encryption in `Runtime/Encrypt/Bundle/`
3. Implement manifest encryption in `Runtime/Encrypt/Manifest/`
4. Add to `EncryptionOption` enum
5. Update `EncryptionMapping` class

## ScriptableObject Configuration

Use `ScriptableObject` for runtime configuration:
```csharp
public abstract class ConfigBase<T> : ScriptableObject where T : ConfigBase<T>
{
    public static T Instance { get; }
}
```

## Editor Scripts

- Use `[InitializeOnLoad]` for editor initialization
- Handle Unity domain reloads properly (state resets on recompile)
- Use `SessionState` or `EditorPrefs` for persistent editor state
- Clean up resources in `EditorApplication.quitting`

## Unit Testing

**IMPORTANT**: When adding or modifying features in non-core packages (JEngine.UI, JEngine.Util), you MUST add unit tests.

### Test Location

Tests go in `Packages/com.jasonxudeveloper.jengine.<package>/Tests/Editor/`:
- Components: `Tests/Editor/Components/<Category>/<ComponentName>Tests.cs`
- Theming: `Tests/Editor/Theming/<ClassName>Tests.cs`
- Utilities: `Tests/Editor/Utilities/<ClassName>Tests.cs`

### Test Pattern

Follow the existing test structure:
```csharp
using NUnit.Framework;
using JEngine.UI.Editor.Components.Layout;

namespace JEngine.UI.Tests.Editor.Components.Layout
{
    [TestFixture]
    public class JContainerTests
    {
        private JContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = new JContainer();
        }

        [Test]
        public void Constructor_Default_AddsBaseClass()
        {
            Assert.IsTrue(_container.ClassListContains("j-container"));
        }
    }
}
```

### Test Coverage Requirements

- Constructor behavior (default and parameterized)
- All public methods and properties
- Fluent API chaining
- Edge cases and error conditions
- Inherited behavior from base classes
