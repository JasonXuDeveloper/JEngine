# JEngine Claude Code Plugin

AI-powered development guide for JEngine Unity framework.

## Installation (Recommended: Marketplace)

Add the JEngine marketplace for **automatic updates**:

```bash
# Add marketplace (one-time)
claude plugin marketplace add https://github.com/JasonXuDeveloper/JEngine/tree/master/claude-plugin/marketplace.json

# Install plugin
claude plugin install jengine@jengine-marketplace
```

After this, plugin updates automatically when Claude Code starts.

## Alternative: Direct Install

```bash
claude plugin install https://github.com/JasonXuDeveloper/JEngine/tree/master/claude-plugin
```

(Note: Direct installs require manual updates)

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
