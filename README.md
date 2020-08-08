# JENGINE v0.3.6.4

JEngine is a **streamlined and easy-to-use** framework designed for Unity Programmers.

What can JEngine do?

- **[Hot-update](Docs/WhyHotUpdate.md) solution**
  
  - **Resource hot update & management** is based on **[XAsset](https://github.com/xasset/xasset)** which JEngine's author has contributed into.
  - **Code hot update** is based on **[ILRuntime](https://github.com/Ourpalm/ILRuntime)** which JEngine's author has also contributed into.
  - **Encrypts** your hot-updatable **codes and resources**, codes will go into your assetbundles, assetbundles will be encrypt within **VFS by XAsset**, and also, your **codes will be encrypted in AES-128 ECB mode**
  
- Own **[Action](Docs/JAction.md) solution**
  
  - **Less code, does more**
  
    ```c#
    int num = 0;
    int repeatCounts = 3;
    float repeatDuration = 0.5f;
    float timeout = 10f;
    JAction j = new JAction();
    j.Do(() => Log.Print("Hello from JAction!"))
      .Repeat(() =>
              {
                num++;
                Log.Print($"num is: {num}");
              }, repeatCounts, repeatDuration)
      .Do(() => Log.Print($"num has increased {repeatCounts} times"))
      .RepeatWhen(() =>
                  {
                    Log.Print($"num is more than 0, num--");
                    num--;
                  },
                  () => num > 0, repeatDuration, timeout)
      .Do(() => Log.Print("JAction will do something else in 3 seconds"))
      .Delay(3.0f)
      .Do(() => Log.Print("Bye from JAction"))
      .Excute();
    ```
  
- **Own [UI](Docs/JUI.md) solution**
  
  - **Method-Chaining** style makes codes prettier and easier to visualize
  
    ```c#
    var JUI = Showcase.AddComponent<JUI>()
      .onInit(t =>
              {
                ...
              })
      .onLoop(t1 =>
              {
                ...
              })
      .onEnd(t2 =>
             {
               ...
             })
      .Activate();
    ```
  
  - **Easier to manage** lifecycle
  
    - Can **easily set up** **what** you want the UI element **to do in specific time**
  
    ```c#
    t.FrameMode = false;//Run in ms
    t.Frequency = 1000;//Loop each 1s
    ```
  
  - **Bindable to data**
  
    - **UI can be binded to a data**, once data  has changed, it will call the method that you has binded
  
    ```c#
    var JUI = b.AddComponent<JUI>()
      .Bind(data.b)
      .onMessage(t1 =>
                 {
                   ...
                 })
      .Activate();
    ```
  
- **Own [Behaviour](Docs/JBehaviour.md)** based on MonoBehaviour
  
  - **More friendly to manage lifecycle**
    
    - You can make loop easier using JEngine
    
    ```c#
    public class JBehaviourExample : JBehaviour
    {
      public override void Init()
      {
        ...
      }
      
      public override void Run()
      {
        ...
      }
      
      public override void Loop()
      {
        ...
      }
      
      public override void End()
      {
        ...
      }
    }
    ```
    
  
- **Own [Resource Management](Docs/JResource.md)** based on XAsset

  ```c#
  TextAsset txt = JResource.LoadRes<TextAsset>("Text.txt");
  Log.Print("Get Resource with Sync method: " + txt.text);
  ```

- **More to explore!!!**

JEngine has its own purpose to help developers **write powerful codes which are streamlined and beautiful**; and to **help developers easier making their games**.

**If you enjoy using JEngine, please give this repo a star!**

[中文请点这里](#中文说明)



## Latest Features

- **Enhanced** JAciton

- **Enhanced** JBehaviour

- **JResource** is now coming

  - JResource is based on XAsset and it allows to load asset from hot-update resources via sync/async methods

  ```c#
  var txt = JResource.LoadRes<TextAsset>("Text.txt");
  Log.Print("Get Resource with Sync method: " + txt.text);
  ```



[Click here to see all version updates](CHANGE.md)



## Features

- **[Hot-update](Docs/WhyHotUpdate.md)** solution

  - No need to learn Lua, **C# codes can be hot-updated**
  - Drop your resources in specific directories and can be **generate hot-updatable resources automatically**, all you need to do is to press "Build Bundle" button, and to put  your what it generated into your server
  - **Encrypts DLL**, your hot-update codes are safe now unless someone got your encrypted password

- **[JBehaviour](Docs/JBehaviour.md)** is a Behaviour in JEngine  which is based on MonoBehaviour, and it is **easier to manage lifecycle** of UI elements

- **[JUI](Docs/JUI.md)** is a class in JEngine which can **enhance the performence of UI** elements based on UGUI
  
  - JUI borrowed concept from **MVVM Framework** and rewrote it, JUI supports **binding a data with an action,** once data has changed, the action will be called
  - You can choose to **either update your UI in specific Loop** with Frequency, or to update your UI only if the binded data changed
  - You can **get UI components more efficiently** with JUI via the generic method **Method<T>**
  - **Method-Chaning** style of coding makes your codes **prettier and easier to read**
  
- **[JAction](Docs/JAction.md)** is an extension of Action

  - **Method-chaining** Style

  - Great variation of features

    - Do action
    - Delay
    - Wait Until
    - Repeat
    - Repeat When
    - Repeat Until
    - etc...
    
  - **Shorter and more powerful**
  
    - Less code can do more things
  
  - **Extension of System.Action**
  
    - Add what to do, add delayings, JAction will do them in order
  
- **[Resource Management](Docs/JResource.md)** solution

  - Based on XAsset
  - **Can load resources in sync/async method**
  - **Generic methods**
  
- **Object Pool** solution

  - **MUCH MORE Enhances the performence** rather than using ***Instantiate method***
    - With this solution, **you don't have to repeat instantiate gameObject**
    - Just **tell JObjectPool what gameObject you will repeatedly create**, and **how many you except to create at the start**, it will do it for you
    - Request **PoolObject** to get the GameObject
  - Easy and powerful
  - With algorithm which fairly controls gameObjects

  > Example will come soon
  
- **[GUI-Redis](https://github.com/JasonXuDeveloper/Unity-GUI-Redis)** helps visualize data in Redis Databases and can **modify data** in it.

  - Supports connect through **SSH tunnel**
  - Supports connect through **normay way** (IP, Port connection)
  - Supports **add/modify/delete/search** key-value pairs



## Future Features

- ~~Supports local hot-update resources development in Unity Editor (Done)~~
- ~~Encrypt Hot-update DLL and decrypt in runtime~~
- ~~Object Pool which significantly improves performance rather than using Instantiate method~~
- JUI with more extended APIs
- UI Special Effects
- Optiimize logics which can improve process speed (As always doing it)
- *Unity Editor FTP Tool (Maybe)*



## What is Hot update and Why

[Click here to have a read](Docs/WhyHotUpdate.md)



## Directory Structure **(IMPORTANT)**

Please clone this framework into your project and keep this directory structure

```
.
├── Assets
│   ├── Dependencies
│   │   ├── ILRuntime
│   │   ├── JEngine
│   │   ├── LitJson
│   │   └── XAsset
│   ├── HotUpdateResources
│   │   ├── Controller
│   │   ├── Dll
│   │   ├── Material
│   │   ├── Other
│   │   ├── Prefab
│   │   ├── Scene
│   │   ├── ScriptableObject
│   │   ├── TextAsset
│   │   └── UI
│   ├── Init.unity
│   └── Scripts
│       ├── Init.cs
│       ├── InitILrt.cs
│       └── APIs
├── Builds
├── DLC
├── HotUpdateScrpts
├── ProjectSettings
```

### Description for those Directories

[Click here to have a read](Docs/DirectoriesDiscription.md)



## JEngine Hot Update Logics

![flowchart](https://s1.ax1x.com/2020/07/14/Uthp6A.png)



## Quick Start

#### Basics

> The basics tells you how to use this framework in basics (How to make your project hot-updatable)

[Click here to have a read](Docs/Basic.md)

#### Extensions

> The extensions will show you how to hot-update your game in coding levels (How to make hot-updatable codes)

[Click here to have a read](Docs/Extension.md)



#### Developement Guide

> This will enable you quick develop your game and you don't need to upload your latest hot-update resources into your server **(which saves your time)**

1. Go to Unity Editor, open **Init scene**

2. Choose **Updater** from inspector

3. **Enable development mode** in Updater script section

   <img src="https://s1.ax1x.com/2020/07/16/UBC5uD.png" alt="guide1" style="width:50%;margin-left:25%" />



#### Common "Bugs"

- Cannot find Delegate Adapter for: **XXX**:

  > Just do what it asked you to add into **Scripts/InitIlrt.cs,  'InitializeILRuntime()' Method**
  >
  > ![bug1](https://s1.ax1x.com/2020/07/14/Ut2RoD.png)



#### IMPORTANT

- When **building bundle**, a window will pop up, and you need to **provide the encrypt key which encrypts your dll**

  ![key1](https://s1.ax1x.com/2020/07/26/apuoHs.png)
  
- In **Init scene**, select HotFixCode on hierarchy, you will see **Init (Script) on inspector**, there will be a section called **Key**, just **fill the encrypt key** in, and it will decrypt dll in runtime

  ![key2](https://s1.ax1x.com/2020/07/26/apu7En.png)

- When you build your project, **remember to remove hot-update scenes** in build setting panel to avoid redundancy

  ![build](https://s1.ax1x.com/2020/07/20/Uhxcuj.jpg)



## Development Environment

- Debuging Unity Engine Version: 2019.3.13f1

  > Should supports Unity Engine From 2018 LTS till the latest

- .net Environment: .net 2.0 standard

- Operating System: MacOS 10.15.5

  > Definityly supports windows



## Recommend Repositories

- [XAsset](https://github.com/xasset/xasset) - A more streamlined, efficient and secure Unity Resource Management Solution for you.
- [IFramework](https://github.com/OnClick9927/IFramework) - Simple Unity Tools

<br>
<br>
<br>

## 中文说明

> 将在获得 100 stars之后补充
