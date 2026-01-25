# Contributing to JEngine

Thank you for your interest in contributing to JEngine! This document provides guidelines and information to help you contribute effectively.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Development Setup](#development-setup)
- [Project Structure](#project-structure)
- [Code Style Guidelines](#code-style-guidelines)
- [Commit Message Format](#commit-message-format)
- [Developer Certificate of Origin (DCO)](#developer-certificate-of-origin-dco)
- [Pull Request Process](#pull-request-process)
- [CI/CD Checks](#cicd-checks)
- [Community](#community)

## Prerequisites

Before contributing, ensure you have:

- **Unity 2022.3+** installed
- **Git** for version control
- A GitHub account

## Development Setup

1. **Fork the repository**

   Click the "Fork" button on the [JEngine repository](https://github.com/JasonXuDeveloper/JEngine).

2. **Clone your fork**

   ```bash
   git clone https://github.com/YOUR_USERNAME/JEngine.git
   cd JEngine
   ```

3. **Add upstream remote**

   ```bash
   git remote add upstream https://github.com/JasonXuDeveloper/JEngine.git
   ```

4. **Open in Unity**

   Open the `UnityProject` folder in Unity 2022.3 or later.

5. **Create a feature branch**

   ```bash
   git checkout -b feature/your-feature-name
   ```

## Project Structure

```
JEngine/
├── UnityProject/
│   ├── Packages/
│   │   ├── com.jasonxudeveloper.jengine.core/   # Core framework
│   │   │   ├── Runtime/                         # Runtime code
│   │   │   │   ├── Bootstrap.cs                 # Main entry point
│   │   │   │   ├── Encrypt/                     # Encryption subsystem
│   │   │   │   └── Update/                      # Update management
│   │   │   └── Editor/                          # Editor tools
│   │   └── com.jasonxudeveloper.jengine.util/   # Utility package
│   │       ├── Runtime/
│   │       │   ├── JAction.cs                   # Chainable async tasks
│   │       │   └── JObjectPool.cs               # Generic object pooling
│   │       └── Tests/                           # Unit tests
│   └── Assets/
│       ├── HotUpdate/                           # Hot-updateable code
│       └── Samples/                             # Sample implementations
├── .github/
│   └── workflows/                               # CI/CD workflows
├── README.md                                    # English documentation
├── README_zh_cn.md                              # Chinese documentation
└── CHANGE.md                                    # Changelog
```

## Code Style Guidelines

### Namespaces

- Core framework: `JEngine.Core.*`
- Editor code: `JEngine.Core.Editor.*`
- Utility code: `JEngine.Util`
- Hot update code: `HotUpdate.Code`

### File Headers

All C# files should include the standard header:

```csharp
// FileName.cs
//
// Author: JasonXuDeveloper <jason@xgamedev.net>
// Copyright (c) 2025 JEngine - MIT License
```

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes/Interfaces | PascalCase | `JAction`, `IStateStorage` |
| Methods/Properties | PascalCase | `Execute()`, `IsExecuting` |
| Private fields | camelCase or _camelCase | `_tasks`, `currentIndex` |
| Constants | PascalCase | `DefaultCapacity` |
| Enums | PascalCase | `EncryptionOption.Aes` |

### Async Patterns

Use `UniTask` for async operations, not `System.Threading.Tasks.Task`:

```csharp
// Correct
public async UniTask<bool> LoadAssetAsync() { }

// Incorrect
public async Task<bool> LoadAssetAsync() { }
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

### Code Quality

- Avoid `Debug.Log` in production code - use proper logging
- Handle Unity domain reloads in Editor code
- Ensure thread safety where needed (Editor callbacks, etc.)
- Properly clean up resources (ScriptableObjects, event handlers)

## Commit Message Format

All commits must follow the [Conventional Commits](https://www.conventionalcommits.org/) specification.

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

| Type | Description | Appears in Changelog |
|------|-------------|---------------------|
| `feat` | New feature | Yes (Features) |
| `fix` | Bug fix | Yes (Bug Fixes) |
| `docs` | Documentation changes | No |
| `style` | Code style/formatting | No |
| `refactor` | Code refactoring | No |
| `test` | Test changes | No |
| `chore` | Build/config changes | No |

### Scopes

- `core` - Changes to JEngine.Core package
- `util` - Changes to JEngine.Util package
- `ci` - Changes to CI/CD workflows
- `docs` - Changes to documentation

### Examples

```bash
# Feature
feat(core): add ChaCha20 encryption support

# Bug fix
fix(util): resolve JAction memory leak on cancellation

# Documentation
docs: update installation guide for Unity 2022.3

# Refactoring
refactor(core): simplify bootstrap initialization

# Tests
test(util): add coverage for JAction edge cases

# CI/CD
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

## Developer Certificate of Origin (DCO)

All commits **must be signed off** to certify that you have the right to submit the code under the project's open source license.

### Signing Your Commits

Use the `--signoff` (or `-s`) flag when committing:

```bash
git commit -s -m "feat(core): add new feature"
```

This adds a `Signed-off-by` line to your commit message:

```
feat(core): add new feature

Signed-off-by: Your Name <your.email@example.com>
```

### Configure Git Identity

Before signing off, configure your git identity:

```bash
git config user.name "Your Name"
git config user.email "your.email@example.com"
```

### What DCO Certifies

The sign-off certifies that you agree to the [Developer Certificate of Origin](https://developercertificate.org/):

1. You have the right to submit the work under the project's license
2. You understand it will be distributed under that license
3. You can track the contribution back to its origin

## Pull Request Process

1. **Sync with upstream**

   ```bash
   git fetch upstream
   git rebase upstream/master
   ```

2. **Make your changes**

   - Follow the code style guidelines
   - Add tests for new functionality
   - Update documentation if needed

3. **Run tests locally**

   Open Unity Test Runner (Window > General > Test Runner) and run all tests.

4. **Commit with sign-off**

   ```bash
   git add .
   git commit -s -m "feat(scope): your feature description"
   ```

5. **Push to your fork**

   ```bash
   git push origin feature/your-feature-name
   ```

6. **Create Pull Request**

   - Go to your fork on GitHub
   - Click "Compare & pull request"
   - Fill in the PR template
   - Link any related issues

7. **Address review feedback**

   - Make requested changes
   - Push additional commits (with sign-off)
   - Request re-review when ready

## CI/CD Checks

All pull requests must pass the following automated checks:

| Check | Description |
|-------|-------------|
| **Unity Tests** | Runs unit tests in Unity Test Framework |
| **DCO Check** | Verifies all commits are signed off |
| **CodeQL Analysis** | Security and code quality scanning |
| **Claude Code Review** | AI-assisted code review |

### If a Check Fails

1. Click "Details" on the failed check
2. Review the error message or logs
3. Fix the issue locally
4. Push a new commit (with sign-off)

## Community

### Documentation

- [English Documentation](https://jengine.xgamedev.net/)
- [Chinese Documentation](https://jengine.xgamedev.net/zh/)

### Support

- **QQ Group**: [921271552](https://jq.qq.com/?_wv=1027&k=cF4hODjW)
- **GitHub Issues**: [Report bugs or request features](https://github.com/JasonXuDeveloper/JEngine/issues)
- **GitHub Discussions**: [Ask questions](https://github.com/JasonXuDeveloper/JEngine/discussions)

### Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Help others learn and grow
- Follow the project maintainer's decisions

---

Thank you for contributing to JEngine!
