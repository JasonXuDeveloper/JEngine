# Plugin Maintenance

The `claude-plugin/` folder contains AI agent documentation for end users who install JEngine packages.

## When to Update

Update the plugin documentation when:
- New public API added to JEngine.Util or JEngine.UI
- API signature changes
- New usage patterns discovered
- Common mistakes identified

## Files to Update

| Package Change | Plugin File to Update |
|---------------|----------------------|
| JAction API | `claude-plugin/skills/jaction/SKILL.md` |
| JObjectPool API | `claude-plugin/skills/jobjectpool/SKILL.md` |
| MessageBox API | `claude-plugin/skills/messagebox/SKILL.md` |
| Editor UI components | `claude-plugin/skills/editor-ui/SKILL.md` |
| Design Tokens | `claude-plugin/skills/editor-ui/SKILL.md` |
| New utility class | Create new skill in `claude-plugin/skills/` |
| General patterns | `claude-plugin/CLAUDE.md` |

## Checklist for API Changes

- [ ] Update relevant SKILL.md with new/changed API
- [ ] Add code examples for new features
- [ ] Document common mistakes if any
- [ ] Bump version in `claude-plugin/claude.json` (CI does this automatically on release)

## Version Sync

Plugin version is automatically bumped by CI when packages are released.
For manual updates, bump the version in `claude-plugin/claude.json`:
- Patch (x.x.1) for doc fixes
- Minor (x.1.0) for new API coverage
- Major (1.0.0) for breaking skill changes
