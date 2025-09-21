## All Versions

### 0.8.0 (April 11 2023)

- **Supported** WebGL
- **Removed** JUI (will be redesigned shortly)
- **Optimized** LifeCycleMgr performance
- **Optimized** ThreadMgr features
- **Optimized** JStream performance
- **Optimized** JBehaviour performance
- **Optimized** MonoBehaviour performance
- **Optimized** FpsMonitor performance
- **Optimized** CryptoMgr interfaces
- **Optimized** JBehaviour/ClassBind Object/MonoBehaviour GC
- **Optimized** GetComponent(s)/FindObject(s)OfType performance
- **Switched** to YooAsset for asset management module (instead of Bundle Master)
- **NEW FEATURE** UnsafeMgr which provides unsafe (use for optimization) features 
- **NEW FEATURE** UnmanagedMemoryPool which supports allocating unmanaged memory with pooling
- **NEW FEATURE** CoroutineMgr which supports executing/stopping coroutines from non-MonoBehaviour class
- **NEW STRUCTURE** Split JEngine source code to multiple packages





### 0.7.5 (September 25 2022)

- **Updated** ILRuntime to v2.1.0
- **Optimized** code register workflow
- **Optimized** JStream buffer strategy
- **Optimized** ClassBind performance and GC
- **Optimized** JEngine Initialize procedure and performance
- **Optimized** LifeCycleMgr performance and GC
- **Optimized** ```FindObjectsOfType``` performance and GC
- **Optimized** JBehaviour performance and GC, removed its dependence to ```MonoBehaviour```
- **Optimized** Loom (Execute Actions on Main Thread) performance and GC, removed its dependence to ```MonoBehaviour```



### 0.7.4 (September 22 2022)

- Bug **fixed**
- **Imported ** high performance C# library Nino
- **Optimized** JBehaviour performance and GC
- **Optimized** MonoBehaviour update logics and GC
- **Optimized** blockwised decrypt intepret module's allocated memroy size and GC



### 0.7.3 (July 14 2022)

- Bug **fixed**

- **Updated** ILRuntime

- **Updated** Bundle Master

- **Enhanced** JBehaviour performance

- **Enhanced** ClassBind runtime performance

- **Almost no GC allocation**  async wait method

  

### 0.7.2 (May 15 2022)

- Bug **fixed**
- **Enhanced** framework code
- **Enhance ** Lifecycle
- **Updated** ETTask
- **Enhanced **JAction
- **Enhanced** JBehaviour
- **Enhanced** Unity **Exception StackTrace**
- **HotUpdateResource** support **Blacklist**
- **New** Protobuf **Serialize Demo**
- **Updated** Protobuf-net **library**

### 0.7.1 (March 28 2022)

- **Update** ILRuntime v2.0.2
- Support **multiple** hot update **packages**
- **Removed** XAsset
- Imported **BundleMaster**



### 0.7.0 (Feburary 17 2022)

  - Mark dependencies as **modules** (More standardized)
  - Support **offline mode** (Run your game without building a resource server)
  - Modify **developing procedure** (Enhanced experience when developing)
  - **Decoupling framework code** (More lightweight and even easier to use)
  - **Enhanced framework code** (A better performance)
  - Enhanced **Litjson and Protobuf-net** (More reliable (de)serializing tools)
  - **Fixed all Issues** occured in 0.6.3 (More powerful and less bugs)
  - More **Demo** (Easier to start)
  - Add **extensible validator** (New feature)
  - Useful **UI extension tools** (New feature)
  - **ILRuntime2.0** (A better performance with less bugs)
  - **JPrefab Upgrade** (Support (a)synchronization to create and destory mutliple at once)
  - **JBehaviour new lifecycles** (Similar to MonoBehaviour's OnEnable and OnDisable)



### v0.6.3 (August 13 2021)

- **Implemented** ```[System.Serializable]``` when generating **c# from proto**
- **Changed** ClassBind default to **active after**
- **Enhanced** the precision for **JFloat**
- **Enhanced** unity project which no longer depends JBehaviour from HotUpdate Solution
- **NEW** JEvent + Event Demo



### v0.6.2 (April 30 2021)

- **Fixed** plenty of bugs
- **Support** XAsset 6.1 (Regards to branch xasset6.1)
- **Optimize** JAction
- **Optimize** Downloader
- **Optimize** ClassBind **performance**
- **Optimize** localization solution
- **Optimize** hot object inspector **serialization**
- **Optimize** ILRuntime **Cross domain adapter generator**
- **Optimize** ```GameObject.Instantiate``` CLR Redirection
- **Upgrade** JEngine**Panel**
- **Upgrade** JUI's **Bindable Property** feature
- **Upgrade** ILRuntime to  **v1.6.7**
- **Add** ClassBind support **filters** on automatically fetching fields
- **Add** hot **source code supports memory encryption** and does not effect performence
- **Add** JEngine **Demos** （Instantiate, Generic Cross Domain Adaptor, etc）



### v0.6.1 (Janurary 4 2021)

- ILRuntime update to **v1.6.6**
- ILRuntime **Adapter Generator Enhance**
- ILRuntime **Automatically Register Adapters**
- More JEngine **Demos**
- Better **Logs**
- JBehaviour & JEvent & etc **supports stacktrace on specefic line**
- UIUtility with a cache dictionary to **load UI components faster**
- Support **all MonoBehaviour events** from now on
- Auto Generate MonoBehaviour Events for **Adapters** which inherits MonoBehaviour
- ClassBind supports binding **different adapters**
- Support **Add & Get Component** with **different adapters** which inherits MonoBehaviour
- Generate **Editor** for Adapters
- Support  **```Invoke```, ```InvokeRepeating```, ```IsInvoking```, ```CancelInvoke```** in hot update scripts
- JEvent support **Subscribe methods from base class**
- **Bug fix** on saving data on prefabs
- **JSaver** supports **get Default Value**
- Better **Editor Panel**
- Support XAsset Pro (Main feature is patching, needs to subscribe)





### v0.6.0 (December 6 2020)

- **UI Framework**
- **Enhance** JSaver
- **JAction** supports ```DelayFrames```
- **Fixed a heaps of bugs**
- **JSON Checker Tool**
- **JEvent**, which is similar to Google core libraries for Java, eventbus
  - **Subscribe Class**
  - **Subscribe Method**
  - **Post** data **to all subscribed events**
  - Can be running on **Main Thread or sub Thread**
- **Enhance** ClassBind, much more powerful
  - Auto Bind when needed, **better performence**
  - Controls by **ClassBindMgr**, **more advanced while setting fields**
  - Support **Automatically get all field**s from a Hot Update Class
  - Support **Automattically find the FieldType** for a field
- **JExtensions** 
  - **Allows get hot update class** from GameObject **via Classbind**
  - **Add JBehaviour** to GameObject
  - **Add JUI** to GameObject
  - **Get JBehaviour** from GameObject
  - **Get JUI** from GameObject
- **JWebSocket**
  - **Connect** to websocket servers
  - **Send** message to websocket servers
  - **Listen** message from websocket servers
  - **Listen and Send in Sub Thread**
- **JEngine Setting Panel**
  - **Automatically jump to Init scene first while running**
  - **Hot Update Scenes Shortcuts**
  - **ClassBind Tools**
  - **Error Rescue Tools**
- **More CLR Redirection Methods**
  - Support **MonoBehaviour.SendMessage** from Hot Update Class
  - Support **MonoBehaviour.Instantiate** GameObject with Hot Update Classes & Single Hot Update Class
- **JBehaviour enhance**
  - **Perform better after built** than in Editor
  - Support **Deltatime, loop counts**, etc.
  - **Powerful Inspector**





### v0.5.8 (October 29 2020)

- **Bug fix** when copying streaming asset
- **C# type to Proto file** converter
- **Rearrange** menuitems
- **Enhance** ClassBind
- **More advanced** JBehaviour Editor
- **JBehaviour** supports create by call ```new()```
- **BindableProperty** supports binding on multiple events





###  v0.5.7 (October 25 2020)

- **JBehaviour** got rid of MonoBehaviour, a significant increasement of performance has been developed
- **JUI** got rid of MonoBehaviour
- **Register Helper** for CLR Method Redirection
- **ClassBind** enhancement



### v0.5.6 (October 19 2020)

- **Less GC** to inherit MonoBehaviour in Hot Update Solution
- **Value Type Binder Register Helper** allows register value type binders, which typically decreases GC on Value Types (EG. Vector3/2)
- **ClassBind improvements** which increases the performence
- **ClassBind supports properties** which allows set properties just like setting fields
- **Localization improvements** which increases the performence
- **Localization Text improvements** which spends less memory and GC
- **Bug fix** on ILRuntime Cross Bind Adapter Generator
- **JBehaviour improvements** which decreases GC and runs better
- **Bug fix** on JBehaviour when calls ```pause()``` and ```resume()```
- **JPrefab** which is a new solution for managing hot update prefabs





### v0.5.5 (October 11 2020)

- **ClassBind** allows auto **attach any class** to a GameObject
- **ClassBind** allows use **consturcor** as a initial of fields of a class
- **ClassBind** allows call ```Active()``` method whether the bound class inherited MonoBehaviour or JBehaviour or not
- **Editor** for **Hot Update Scripts to GameObject** supports present **more stuffs**
- **Demo** for using **AntiCheat Struct** & **Bindable Property** improves



### v0.5.4 (October 10 2020)

- **Anti-Cheat Structs** allows detect hackers who hacks your game
- **ClassBind improvements** more friendly to update from old versions to latests



### v0.5.3 (October 6 2020) (Support by XAsset's Author)

- Supports **copy DLC files to Streaming Assets**, which can pass App Store review easier
- Supports **move local Streaming Asstes hot update resouces to persistence path** which user don't need to download any old things from server



### v0.5.2 (October 1 2020)

- **Anti-Cheat Structs** which prevents cheating tools change data stores in memory
  - **Drag 'n drop** on Class Bind
  - **LitJSON Serialize and Desirialize Float**



### v0.5.1 (Sepetember 26 2020)

- **Localization** supports multiple languages in your game
- **Smarter** autobind, easier to set value of a Unity Component
- **More** event functions for MonoBehaviour in HotUpdate



### v0.5.0 (Sepetember 19 2020)

- **Rebuilt** demos
- **Rewrote** **StringifyHelper**
- **Updated** ILRuntime
- **Rewrote** JSaver
- **Enhanced** Auto Class Bind
- **Enhanced** Cleaning Hot Update Solution in Unity Editor
- **Enhanced** LitJson to serialize and deserialize **BindableProperty** (**Decreases 80%** of size when **converting BindableProperty to JSON**)
- **Adapt** protobuf-net
- **Proto2CS** converter
- **Rearranged** hot update solution
- **JResource** now supports **load scene async** and **get progress when loading scene**
- **Combined** JBehaviour & JUI to JEngine.Core
- **Enhanced** JBehaviour
- **Enhanced** BindableProperty ToString method
- **Enhanced** MonoBehaviour Adapter Inspector Window



### v0.4.10 (Sepetember 12 2020)

- **StringifyHelper** is a helper which serialize and deserialize data
  - Support **JSON & Protobuf & BinaryFormatter**
- **JSaver** now uses StringifyHelper to do JSON converts
- **Enhance** clean.cs



### v0.4.9 (September 10 2020)

- **Able to autobind hot update resource**
- **Auto bind** runs in Awake which becomes **faster**
- ILRuntime Adapter Generator supports **custom assembly**



### v0.4.8 (September 9 2020)

- **Right Click** to **copy GameObject Path from hierarchy**
- **Bug fix** on **release dll**
- **Bug fix** on **Build Setting scenes**



### v0.4.7 (September 8 2020)

- **Autobind** supports bind to GameObject **itself**
- Support **AES encrypt string**
- **JSaver**, support Data persistence



### v0.4.6 (September 7 2020)

- **MonoBehaviour Adapter Inspector window show error fields**
- **Autobind editor window**
- **Autobind support bool value**
- **Autobind support private and static and instance fields**





### v0.4.5 (September 6 2020)

- **ILRuntime Adpater Generater**
- **Enhanced Autobind**
- **Less GC on Autobind**
- **Auto Destory when Finish Autobind**
- **No need to call any method to require autobind**





### v0.4.4 (September 5, 2020)

- **Autobind** support auto set fields
- **Enhance Autobind**



### v0.4.3 (September 4, 2020)

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

