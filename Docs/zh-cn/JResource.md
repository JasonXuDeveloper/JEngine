## JResource

> JEngine now supports JResource, which is an solution for Resource Management
>
> Why use JResource?
>
> - Less code to get Resource from Hot-update resources
> - Support development mode
> - Generic method to get resources



### APIs

- ```c#
  LoadRes<T>(string path, MatchMode mode = MatchMode.AutoMatch) where T : UnityEngine.Object
  ```

- ```c#
  LoadResAsync<T>(string path, Action<T> callback, MatchMode mode = MatchMode.AutoMatch) where T : UnityEngine.Object
  ```

- ```c#
  public enum MatchMode
  {
    AutoMatch = 1,
    Animation = 2,
    Material = 3,
    Prefab = 4,
    Scene = 5,
    ScriptableObject = 6,
    TextAsset = 7,
    UI = 8,
    Other = 9
  }
  ```



### Examples

- Sync Method

```c#
var txt = JResource.LoadRes<TextAsset>("Text.txt");
Log.Print("Get Resource with Sync method: " + txt.text);
```

- Async Parallel Method 

```c#
JResource.LoadResAsync<TextAsset>("Text.txt",(txt)=>
{
	Log.Print("Get Resource with Async method: " + txt.text);
});
```

