## Description

<!-- Provide a brief description of the changes in this PR -->

## Type of Change

<!-- Check the type that applies to this PR -->

- [ ] 🐛 Bug fix (fixes an issue)
- [ ] ✨ New feature (adds functionality)
- [ ] 💥 Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] 📝 Documentation update
- [ ] 🎨 Code style/formatting
- [ ] ♻️ Code refactoring (no functional changes)
- [ ] ✅ Test updates
- [ ] 🔧 Build/config changes

## Package Scope

<!-- Check which package(s) this PR affects -->

- [ ] JEngine.Core (`com.jasonxudeveloper.jengine.core`)
- [ ] JEngine.Util (`com.jasonxudeveloper.jengine.util`)
- [ ] Other (specify):

## Checklist

- [ ] My code follows the project's coding conventions (see [CLAUDE.md](../CLAUDE.md))
- [ ] I have performed a self-review of my code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have added/updated XML documentation for public APIs
- [ ] I have added/updated tests that prove my fix is effective or that my feature works
- [ ] New and existing unit tests pass locally with my changes
- [ ] I have used `UniTask` for async operations (not `System.Threading.Tasks.Task`)
- [ ] My changes handle Unity domain reloads properly (if applicable to Editor code)

## Commit Message Format

We use **Conventional Commits** for automated changelog generation. Please format your commit messages as:

```
<type>(<scope>): <subject>
```

### Examples

```bash
feat(core): add ChaCha20 encryption support
fix(util): resolve JAction memory leak on cancellation
docs: update installation guide for Unity 2022.3
refactor(core): simplify bootstrap initialization
test(util): add coverage for JAction edge cases
chore(ci): update GameCI Unity version to 2022.3.55f1
```

### Types

- `feat:` - New feature (appears in changelog)
- `fix:` - Bug fix (appears in changelog)
- `docs:` - Documentation only
- `style:` - Code style/formatting
- `refactor:` - Code refactoring
- `test:` - Test changes
- `chore:` - Build/config changes

### Scopes

- `core` - JEngine.Core package
- `util` - JEngine.Util package
- `ci` - CI/CD workflows
- `docs` - Documentation

### Breaking Changes

For breaking changes, add `!` after the type/scope or include `BREAKING CHANGE:` in the footer:

```bash
feat(core)!: redesign encryption API

BREAKING CHANGE: EncryptionManager.Encrypt() now requires EncryptionConfig parameter
```

## Testing

<!-- Describe how you tested your changes -->

**Test environment:**
- Unity version:
- Platform(s):
- Test mode: [ ] EditMode [ ] PlayMode [ ] Manual

**Steps to test:**
1.
2.
3.

## Additional Notes

<!-- Any additional information that reviewers should know -->
