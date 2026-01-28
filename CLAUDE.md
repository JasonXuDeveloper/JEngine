# JEngine Development Guidelines

## Additional Rules

@.claude/rules/coding-patterns.md
@.claude/rules/commit-conventions.md
@.claude/rules/package-creation.md

## Project Overview

JEngine is a Unity framework for **runtime hot updates** in games. It enables developers to update game code and assets without requiring users to download new builds.

- **Unity Version**: 2022.3+
- **Primary Language**: C#
- **License**: MIT

## Architecture

### Package Structure

```
Packages/
├── com.jasonxudeveloper.jengine.core/   # Main framework (Bootstrap, Encrypt, Update)
├── com.jasonxudeveloper.jengine.util/   # General utilities (JAction, JObjectPool)
├── com.jasonxudeveloper.jengine.ui/     # UI utilities (MessageBox)
└── com.code-philosophy.hybridclr/       # HybridCLR integration

Assets/
├── HotUpdate/Code/                      # Hot-updateable code
└── Scripts/                             # Non-hot-update scripts
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
- UI package: `JEngine.UI`
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
- **Constants/Enums**: PascalCase

### XML Documentation

Document all public APIs with XML comments.

## Testing

- Tests are in `Assets/Tests/` and package `Tests/` folders
- Run tests via Unity Test Runner
- Use `[UnityTest]` with `UniTask.ToCoroutine()` for async tests
- Editor code should check `TestRunnerCallbacks.IsRunningTests`

## Code Review Checklist

- [ ] Follows namespace conventions
- [ ] Has appropriate XML documentation
- [ ] Uses UniTask for async (not Task)
- [ ] Thread-safe where needed
- [ ] Proper resource cleanup
