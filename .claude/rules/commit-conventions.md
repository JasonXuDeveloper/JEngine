# Git Workflow & Commit Conventions

## Branching

**Always create new branches from `master`** (the main branch), not from feature branches:

```bash
# Correct workflow
git checkout master
git pull origin master
git checkout -b fix/my-fix

# Wrong - creates branch from current branch which may not be master
git checkout -b fix/my-fix
```

Before creating a PR branch, always:
1. Switch to `master`
2. Pull latest changes
3. Then create the new branch

## Never Push Directly to Master

**All changes must go through pull requests.** Never push commits directly to master, even for documentation changes. This ensures:
- Code review for all changes
- CI checks run before merge
- Clean git history with traceable PRs

## Commit Message Format

All commits should follow the Conventional Commits specification to enable automatic changelog generation.

## Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

## Types

- `feat:` - New feature (appears in changelog as "Feature")
- `fix:` - Bug fix (appears in changelog as "Fixed")
- `docs:` - Documentation only changes (not in changelog)
- `style:` - Code style/formatting changes (not in changelog)
- `refactor:` - Code refactoring (not in changelog)
- `test:` - Test changes (not in changelog)
- `chore:` - Build/config changes (not in changelog)
- `BREAKING CHANGE:` - Breaking changes (in footer, triggers major version bump)

## Scopes

- `core` - Changes to JEngine.Core package
- `util` - Changes to JEngine.Util package
- `ui` - Changes to JEngine.UI package
- `ci` - Changes to CI/CD workflows
- `docs` - Changes to documentation

## Examples

```bash
feat(core): add ChaCha20 encryption support
fix(util): resolve JAction memory leak on cancellation
docs: update installation guide for Unity 2022.3
refactor(core): simplify bootstrap initialization
test(util): add coverage for JAction edge cases
chore(ci): update GameCI Unity version to 2022.3.55f1
```

## Breaking Changes

For breaking changes, include `BREAKING CHANGE:` in the footer:

```bash
feat(core)!: redesign encryption API

BREAKING CHANGE: EncryptionManager.Encrypt() now requires EncryptionConfig parameter
```

## Guidelines

- Keep subject line under 72 characters
- Use imperative mood ("add" not "added" or "adds")
- Don't capitalize first letter of subject
- Don't end subject with a period
- Separate subject from body with blank line
- Wrap body at 72 characters
- Use body to explain what and why (not how)

## Developer Certificate of Origin (DCO)

All commits must be signed off using `--signoff` (or `-s`) flag:

```bash
git commit -s -m "feat(core): add new feature"
```

This adds a `Signed-off-by` line certifying you have the right to submit the code under the project's license.

## PR Code Review Handling

When addressing code review comments on a PR:

1. **Fix the issue** in code
2. **Commit and push** the fix
3. **Reply** to the review comment explaining the fix
4. **Resolve the conversation** immediately after replying

To resolve conversations via CLI:

```bash
# Get thread IDs
gh api graphql -f query='
{
  repository(owner: "OWNER", name: "REPO") {
    pullRequest(number: PR_NUMBER) {
      reviewThreads(first: 20) {
        nodes {
          id
          isResolved
        }
      }
    }
  }
}'

# Resolve a thread
gh api graphql -f query='
mutation {
  resolveReviewThread(input: {threadId: "THREAD_ID"}) {
    thread { isResolved }
  }
}'
```

**Important:** Always resolve conversations after fixing and replying - don't leave them open.
