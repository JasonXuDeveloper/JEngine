---
name: messagebox
description: MessageBox async modal dialogs for Unity with UniTask. Triggers on: confirmation dialog, modal popup, prompt, alert, user confirmation, yes/no dialog, OK/Cancel, async dialog, await user input, delete confirmation, save confirmation
---

# MessageBox - Async Modal Dialogs

Built on UniTask for non-blocking async operations with automatic object pooling.

## When to Use
- Confirmation dialogs (Yes/No, OK/Cancel)
- Information prompts (OK only)
- Awaiting user decisions in gameplay

## API

### Show Method
```csharp
public static UniTask<bool> Show(
    string title,
    string content,
    string ok = "OK",
    string no = "Cancel"
)
```
Returns `true` for confirm, `false` for cancel.

### Management
- `MessageBox.CloseAll()` - Dismiss all active dialogs (for scene transitions)
- `MessageBox.Dispose()` - Release all pooled instances (app shutdown)
- `MessageBox.ActiveCount` - Currently displayed dialogs
- `MessageBox.PooledCount` - Cached instances in pool

## Patterns

### Confirmation Dialog
```csharp
bool confirmed = await MessageBox.Show(
    "Delete Item",
    "Are you sure you want to delete this item?",
    ok: "Delete",
    no: "Cancel"
);

if (confirmed)
{
    DeleteItem();
}
```

### Custom Button Text
```csharp
bool saved = await MessageBox.Show(
    "Save Changes",
    "Keep your changes?",
    ok: "Save",
    no: "Discard"
);
```

### Single Button (Notification)
```csharp
// Pass null or empty string to hide cancel button
await MessageBox.Show(
    "Success",
    "Operation completed!",
    ok: "OK",
    no: null
);
```

### Clean Up Before Scene Change
```csharp
MessageBox.CloseAll();
SceneManager.LoadScene("NextScene");
```

## Common Mistakes
- Forgetting to await (dialog shows but code continues immediately)
- Not handling both true/false return values
- Not calling CloseAll() before scene transitions
