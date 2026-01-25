# JEngine Development Guidelines

## Project Overview

JEngine is a Unity framework for **runtime hot updates** in games. It enables developers to update game code and assets without requiring users to download new builds.

- **Unity Version**: 2022.3+
- **Primary Language**: C#
- **License**: MIT

## Architecture

### Package Structure

```
Packages/
├── com.jasonxudeveloper.jengine.core/   # Main framework
│   ├── Runtime/                         # Runtime code
│   │   ├── Bootstrap.cs                 # Main entry point
│   │   ├── Encrypt/                     # Encryption subsystem
│   │   └── Update/                      # Update management
│   └── Editor/                          # Editor tools
└── com.code-philosophy.hybridclr/       # HybridCLR integration

Assets/
├── HotUpdate/                           # Hot-updateable code
│   └── Code/EntryPoint.cs               # Hot update entry point
└── Samples/                             # Sample implementations
```

### Key Dependencies

| Package | Purpose |
|---------|---------|
| HybridCLR | Runtime C# code execution |
| YooAsset | Runtime resource management |
| UniTask | Async/await support |
| Nino | High-performance serialization |
| Obfuz | Code obfuscation |

## Coding Conventions

### Namespaces

- Core framework: `JEngine.Core.*`
- Editor code: `JEngine.Core.Editor.*`
- Hot update code: `HotUpdate.Code`

### File Headers

All C# files should include the standard header:
```csharp
// FileName.cs
//
//  Author:
//        JasonXuDeveloper <jason@xgamedev.net>
//
//  Copyright (c) 2025 JEngine
//
//  [MIT License...]
```

### Naming Conventions

- **Classes/Interfaces**: PascalCase (interfaces prefixed with `I`)
- **Methods/Properties**: PascalCase
- **Private fields**: camelCase or `_camelCase`
- **Constants**: PascalCase
- **Enums**: PascalCase for both type and members

### Async Patterns

Use `UniTask` for async operations, not `System.Threading.Tasks.Task`:
```csharp
public async UniTask<bool> LoadAssetAsync() { }
```

### XML Documentation

Document all public APIs with XML comments:
```csharp
/// <summary>
/// Brief description of the method.
/// </summary>
/// <param name="param">Parameter description.</param>
/// <returns>Return value description.</returns>
public ReturnType MethodName(ParamType param) { }
```

## Important Patterns

### Encryption Architecture

JEngine supports three encryption algorithms:
- **XOR** (`EncryptionOption.Xor`) - Fast, simple
- **AES** (`EncryptionOption.Aes`) - Moderate security
- **ChaCha20** (`EncryptionOption.ChaCha20`) - High security

Each has implementations for bundles and manifests in `Runtime/Encrypt/`.

### ScriptableObject Configuration

Use `ScriptableObject` for runtime configuration:
```csharp
public abstract class ConfigBase<T> : ScriptableObject where T : ConfigBase<T>
{
    public static T Instance { get; }
}
```

### Editor Scripts

- Use `[InitializeOnLoad]` for editor initialization
- Handle Unity domain reloads properly (state resets on recompile)
- Use `SessionState` or `EditorPrefs` for persistent editor state
- Clean up resources in `EditorApplication.quitting`

### Thread Safety

For properties accessed across callbacks:
```csharp
private static volatile bool _flag;
public static bool Flag => _flag;
```

## Testing

- Tests are in `Assets/Tests/` using Unity Test Framework
- Run tests via Unity Test Runner (Window > General > Test Runner)
- Editor code should check `TestRunnerCallbacks.IsRunningTests` to avoid interfering with test execution

## Build & Deployment

### Target Platforms

- `Standalone` - Desktop builds
- `WeChat` / `Douyin` / `Alipay` / `TapTap` - Mini-game platforms

### Hot Update Workflow

1. Build base application with HybridCLR
2. Update code in `Assets/HotUpdate/`
3. Build hot update DLLs
4. Deploy to CDN server

## Common Tasks

### Adding New Encryption

1. Create config class in `Runtime/Encrypt/Config/`
2. Implement bundle encryption in `Runtime/Encrypt/Bundle/`
3. Implement manifest encryption in `Runtime/Encrypt/Manifest/`
4. Add to `EncryptionOption` enum
5. Update `EncryptionMapping` class

### Adding Editor Features

1. Create class in `Editor/` with appropriate namespace
2. Use `[InitializeOnLoad]` for auto-initialization
3. Add menu items via `[MenuItem]` attribute
4. Handle domain reloads if maintaining state

## Code Review Checklist

- [ ] Follows namespace conventions
- [ ] Has appropriate XML documentation
- [ ] Uses UniTask for async (not Task)
- [ ] Handles Unity domain reloads in Editor code
- [ ] No direct `Debug.Log` in production code (use proper logging)
- [ ] Thread-safe where needed (Editor callbacks, etc.)
- [ ] Proper resource cleanup (ScriptableObjects, event handlers)

## Commit Message Format

All commits should follow the Conventional Commits specification to enable automatic changelog generation.

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat:` - New feature (appears in changelog as "Feature")
- `fix:` - Bug fix (appears in changelog as "Fixed")
- `docs:` - Documentation only changes (not in changelog)
- `style:` - Code style/formatting changes (not in changelog)
- `refactor:` - Code refactoring (not in changelog)
- `test:` - Test changes (not in changelog)
- `chore:` - Build/config changes (not in changelog)
- `BREAKING CHANGE:` - Breaking changes (in footer, triggers major version bump)

### Scopes

- `core` - Changes to JEngine.Core package
- `util` - Changes to JEngine.Util package
- `ci` - Changes to CI/CD workflows
- `docs` - Changes to documentation

### Examples

```bash
feat(core): add ChaCha20 encryption support
fix(util): resolve JAction memory leak on cancellation
docs: update installation guide for Unity 2022.3
refactor(core): simplify bootstrap initialization
test(util): add coverage for JAction edge cases
chore(ci): update GameCI Unity version to 2022.3.55f1
```

### Breaking Changes

For breaking changes, include `BREAKING CHANGE:` in the footer:

```bash
feat(core)!: redesign encryption API

BREAKING CHANGE: EncryptionManager.Encrypt() now requires EncryptionConfig parameter
```

### Guidelines

- Keep subject line under 72 characters
- Use imperative mood ("add" not "added" or "adds")
- Don't capitalize first letter of subject
- Don't end subject with a period
- Separate subject from body with blank line
- Wrap body at 72 characters
- Use body to explain what and why (not how)

### Developer Certificate of Origin (DCO)

All commits must be signed off to certify that you have the right to submit the code under the project's open source license. This is done using the `--signoff` (or `-s`) flag when committing.

**Always use signed commits:**

```bash
git commit -s -m "feat(core): add new feature"
```

This adds a `Signed-off-by` line to your commit message:

```
feat(core): add new feature

Signed-off-by: Your Name <your.email@example.com>
```

The sign-off certifies that you agree to the Developer Certificate of Origin (DCO), which states that you have the right to submit the work under the project's license and that you understand it will be distributed under that license.

**Important:** Configure your git identity before signing off:

```bash
git config user.name "Your Name"
git config user.email "your.email@example.com"
```
