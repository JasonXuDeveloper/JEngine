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
  LoadRes<T>(string path) where T : UnityEngine.Object
  ```

- ```c#
  LoadResAsync<T>(string path,Action<T> callback) where T : UnityEngine.Object
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
	Log.Print("Get Resource with Async Paralleled method: " + txt.text);
});
```

