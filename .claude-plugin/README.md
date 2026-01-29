# JEngine Claude Code Plugin

AI-powered development guide for JEngine Unity framework.

## Installation

```bash
# Add JEngine marketplace (one-time)
claude plugin marketplace add JasonXuDeveloper/JEngine

# Install plugin
claude plugin install jengine@jengine-marketplace
```

## Enable Auto-Updates (Recommended)

Third-party plugins require manual opt-in for auto-updates:

1. Run `/plugin` in Claude Code
2. Go to **Marketplaces** tab
3. Select **jengine-marketplace**
4. Toggle **Enable auto-update**

Once enabled, plugin updates automatically when Claude Code starts.

## Features

After installation, Claude Code automatically:
- Suggests JAction for sequential tasks, timers, and delays
- Suggests JObjectPool for object pooling and GC optimization
- Suggests MessageBox for confirmation dialogs
- Suggests Editor UI components for custom inspectors and editor windows
- Follows JEngine coding conventions (UniTask, namespaces, etc.)

## Skills Included

### JEngine.Util (Runtime)
- `/jaction` - Chainable task system with zero-allocation async
- `/jobjectpool` - Thread-safe object pooling

### JEngine.UI (Runtime + Editor)
- `/messagebox` - Async modal dialogs
- `/editor-ui` - Editor UI components (JButton, JStack, Tokens, etc.)

## Requirements

- Claude Code CLI
- JEngine packages installed in your Unity project:
  - `com.jasonxudeveloper.jengine.util`
  - `com.jasonxudeveloper.jengine.ui`
