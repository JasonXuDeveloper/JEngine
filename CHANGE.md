## All Versions

### v0.4.3(September 4, 2020)

- **Custom scenes** bug fixed
- **Pdb symbol** bug fixed
- **Enhanced** JBehaviour
- **Autobind** classes from hotupdate dll to prefabs
- **Support** more Unity versions



### v0.4.2 (September 3, 2020)

- **Advanced** hotupdate dependecies **cleaner**
- **Enhanced** development mode
- Allows **custom scenes addition**



### v0.4.1 (August 22, 2020)

- JAction now supports **execute in main thread**

  > As we know Task.Run in Unity will run in a new thread, which is not able to call most Unity APIs, now JAction found a solution of using Loom.

- Enhance **Developement Mode performance**



### v0.4.0 (August 16, 2020)

- **JEngine Framework** has been **devided** into following parts:

  - **JEngine**

    > You can include this into your hot-update scripts or your Unity main project

    - Core
    - UI
    - LifeCycle

  - **UnityPlugins**

    > You can choose to import these into unity

    - JEngine.Unity (compulsory to a new project)
    - JEngine.JSON (optional)
    - JEngine.Redis (optional)

- **Small bugs** has been **fixed**

- **ILRuntime back to Unity Solution**





### v0.3.6.5 (August 9, 2020)

- **JResource** supports **match pattern**, which prevents different resources witch same name can't be loaded

  ```c#
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

  





### v0.3.6.4 (August 8, 2020)

- **Enhanced** JAciton

- **Enhanced** JBehaviour

- **JResource** is now coming

  - JResource is based on XAsset and it allows to load asset from hot-update resources via sync/async methods

  ```c#
  var txt = JResource.LoadRes<TextAsset>("Text.txt");
  Log.Print("Get Resource with Sync method: " + txt.text);
  ```

  



### v0.3.6.3 (August 6, 2020)

- JAction supports **Cancelation Callback**

  ```c#
  //Cancel a JAction
  JAction j8 = new JAction();
  j8.RepeatWhen(() => Log.Print("[j8] I am repeating!!!"), () => true, 1, timeout)
    .ExecuteAsyncParallel();
  //You can either add a cancel callback
  j8.OnCancel(() => Log.Print("[j8] has been cancelled!"));
  ```

- JAction supports **Reset**

  ```c#
  //Reset a JAction
  j8.Reset();
  ```

- Fixed bug on JAction.ExecuteAsyncParallel



###  v0.3.6.2 (August 5, 2020)

- JAction supports **Async & Async Parallel**

  ```c#
  //Execute Async
  JAction j6 = new JAction();
  _ = j6.Do(() => Log.Print("[j6] This is an async JAction"))
    .ExecuteAsync();
  
  //Execute Async Parallel
  JAction j7 = new JAction();
  j7.Do(()=>Log.Print("[j7] This is an async JAction but runs parallel, callback will be called after it has done"))
    .ExecuteAsyncParallel(()=>Log.Print("[j7] Done"));
  ```

- JAction supports **Cancelation**

  ```c#
  //Cancel a JAction
  JAction j8 = new JAction();	
  _ = j8.RepeatWhen(() => Log.Print("[j8] I am repeating!!!"), () => true, repeatDuration, timeout)
    .ExecuteAsync();
  JAction j9 = new JAction();
  j9.Delay(5)
    .Do(() =>
        {
          j8.Cancel();
          Log.Print("[j9] cancelled j8");
        })
    .Execute();
  ```

  



### v0.3.6.1 (August 3, 2020)

- JAction supports more features

  - Repeat with frequency

    ```c#
    int repeatCounts = 3;
    float repeatDuration = 0.5f;
    JAction j = new JAction();
    j.Repeat(() =>
           {
             Log.Print("I have repeated");
           }, repeatCounts, repeatDuration)
      .Excute();
    ```

  - Repeat with condition

    ```c#
    int num = 10;
    float repeatDuration = 0.5f;
    float timeout = 10f;
    JAction j = new JAction();
    j.RepeatWhen(() =>
                 {
                   Log.Print($"num is more than 0, num--");
                   num--;
                 },
                 () => num > 0, repeatDuration, timeout)
      .Excute();
    ```

  - Repeat until

    ```c#
    int num = 10;
    float repeatDuration = 0.5f;
    float timeout = 10f;
    JAction j = new JAction();
    j.RepeatUntil(() =>
                 {
                   Log.Print($"num is more than 0, num--");
                   num--;
                 },
                 () => num <= 0, repeatDuration, timeout)
      .Excute();
    ```

    

  - Wait Until

    ```c#
    JAction j = new JAction();
    j.Until(()=> something is done)
      .Do(something)
      .Excute();
    ```

    

### v0.3.6 (August  2, 2020)

- **JAction** which **supports less code but do more**

  ```c#
  JAction j = new JAction();
  j.Do(() =>
        {
          Log.Print("Hello from JAction!");
        })
    .Delay(3.0f)
    .Do(() =>
        {
          Log.Print("Bye from JAction");
        })
    .Excute();
  ```

  

- Hidden dictionary to save Hot Update DLL which **strongly increased unity excute speed for dll** (Unity will no longer load Hot Update DLL from editor)



### v0.3.5 (July 29, 2020)

- **Rewrite JBehaviour's source code** which enhances the performence
- **JObjectPool** is a new **solution for Object Pool**
- **Improve JBehaviour** which it now **supports method-chaning** in some part
- **Enhance JUI**



### v0.3.4 (July 25, 2020)

- **JBehaviour Example**
- **Rewrite** JBehaviour **Structure**
- **Improve** JUI
- **Fix** that **JBehaviou**r can't be called



### v0.3.3 (July 24, 2020)

- **Hot Update DLL Encryption** based on AES encryption, with a 16 bits key, it is harder to let others hack your dll
- More **ILRuntime Registerations support**, reduce the requirements of registerations when using ILRuntime
- Update **XAsset** and **ILRuntime** to the latest version



### v0.3.2 (July 19, 2020)

- **JUIText** becomes **JUI**

  - Supports any **UIBehaviour based class** to use

    > All UGUI components in Unity is UIBehaviour based

- **JUI** supports **data binding**

- **JUI** supports **message mode**, will be called when binded data has changed

- **BindableProperty** is coming, use it in your data class or sturcts

- Improve performence of looping in **JUIBehaviour**

- Improve performence on getting **generic components** in JUI

- Update showcases to a countdown showcase and a data update & binding showcase





###  v0.3.1 (July 18, 2020)

- Combined **Unity-GUI-Redis**, which is also part of **JEngine**, now belongs to **JEngine.Redis** namespace

- Update **XAsset** dependency to latest version:

  - Supports **network monitor**
  - Build bundles name by **hash**
  - Tiny improvment on the framework

- Rewrite **ILRuntime**:

  - Supports **OnDestory** method called by MonoBehaviour
  - Tiny improvment on the framework

- **JUI** is now coming:

  > Only supports Text at the moment

  - Based on **JUIBehaviour**
  - **Method-Chaining** which makes more stramlined and beautiful codes
  - Unique and managable **lifecycle**

- Improve **JUIBehaviour**



### v0.3 (July 17, 2020)

- Update **ILRuntime** to v1.6.3 which fixes heaps of bugs in the dependency
- **JEngine Lifecyle** is now included, it is **only a prototype** and will be extended in the future with more base codes.
- **JUIBehaviour** is coming, it is a behaviour base on MonoBehaviour but more friendly to manage UI components' lifecycles which are not require to change that frequently, **and runs better**, you can use less codes to implement more



### v0.2 (July 16, 2020)

- Supports development mode which **loads dll and resources from local**
- Automatically **clean up unnecessary dlls, pdbs, etc.** in DLL Resource Directory
- Automatically **convert dll into bytes** in Editor



### v0.1  (July 14, 2020)

- Automatically generate **Update Resources**
- Automatically handle **Hot-update DLL**
- Update [Hot-update](https://github.com/JasonXuDeveloper/JEngine/blob/4d63fec4027ff5c546fb15ec2469ead898922858/README.md#What-is-Hot-update) codes and resources from server (Base on XAsset & ILRuntime)
- Supports local hot-update code development in Unity Editor via dll in Asstes/HotUpdateResources/Dll/HotUpdateScripts.dll

