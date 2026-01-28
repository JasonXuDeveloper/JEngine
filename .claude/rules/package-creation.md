# Creating New JEngine Packages

When creating a new JEngine package:

1. **Version**: Start at `0.0.0` (not `1.0.0`)
2. **Naming**: Use `com.jasonxudeveloper.jengine.<name>` format
3. **Location**: `Packages/com.jasonxudeveloper.jengine.<name>/`

## Required Files

- `package.json` - Package manifest with version `0.0.0`
- `Runtime/<Name>.asmdef` - Assembly definition
- `README.md` - Package documentation (optional)

## Example package.json

```json
{
  "name": "com.jasonxudeveloper.jengine.<name>",
  "version": "0.0.0",
  "displayName": "JEngine.<Name>",
  "description": "Description here.",
  "license": "MIT",
  "unity": "2022.3",
  "author": {
    "name": "Jason Xu",
    "email": "jason@xgamedev.com",
    "url": "https://github.com/JasonXuDeveloper"
  },
  "dependencies": {}
}
```

## CI Updates Required

1. Add package path to `.github/workflows/pr-tests.yml`
2. Add release support to `.github/workflows/release.yml`
3. Add new scope to commit message conventions
