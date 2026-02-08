# JEngine Framework

Unity framework for runtime hot updates. Unity 2022.3+, C#.

## Available Packages

### JEngine.Util (com.jasonxudeveloper.jengine.util)
- **JAction**: Fluent API for chainable action sequences with zero-allocation async
- **JObjectPool<T>**: Thread-safe generic object pool using lock-free CAS

### JEngine.UI (com.jasonxudeveloper.jengine.ui)
- **MessageBox**: Async modal dialogs with UniTask (runtime)
- **Editor Components**: JButton, JTextField, JStack, JCard, JTabView, etc. (editor)
- **Design Tokens**: Theme-aware colors, spacing, typography

## Key Patterns

### Always Use UniTask (not System.Threading.Tasks.Task)
```csharp
using Cysharp.Threading.Tasks;
public async UniTask<bool> LoadAsync() { }
```

### JAction - Always Dispose After Async
```csharp
using var action = await JAction.Create()
    .Do(static () => Debug.Log("Start"))
    .Delay(1f)
    .Do(static () => Debug.Log("After 1s"))
    .ExecuteAsync();
```

### JObjectPool - Reset State on Return
```csharp
var pool = new JObjectPool<List<int>>(
    onReturn: static list => list.Clear()
);
var list = pool.Rent();
pool.Return(list);
```

### MessageBox - Await the Result
```csharp
bool ok = await MessageBox.Show("Title", "Message?", "Yes", "No");
if (ok) { /* confirmed */ }
```

### Editor UI - Use Design Tokens
```csharp
using JEngine.UI.Editor.Theming;
using JEngine.UI.Editor.Components.Button;
using JEngine.UI.Editor.Components.Layout;

var stack = new JStack(GapSize.MD)
    .Add(new JButton("Action", () => DoIt(), ButtonVariant.Primary));
```
