# JENGINE v0.4.2



JEngine is a **streamlined and easy-to-use** framework designed for Unity Programmers.

![topLanguage](https://img.shields.io/github/languages/top/JasonXuDeveloper/JEngine)
![size](https://img.shields.io/github/languages/code-size/JasonXuDeveloper/JEngine)
![issue](https://img.shields.io/github/issues/JasonXuDeveloper/JEngine)
![license](https://img.shields.io/github/license/JasonXuDeveloper/JEngine)
![last](https://img.shields.io/github/last-commit/JasonXuDeveloper/JEngine)

> QQ Group ID: [921271552](https://jq.qq.com/?_wv=1027&k=cF4hODjW)

> Will hold one live broadcast

[中文文档](README_zh-cn.md)

What can JEngine do?

- **[Hot-update](Docs/en-us/WhyHotUpdate.md) solution**

  - **Resource hot update & management** is based on **[XAsset](https://github.com/xasset/xasset)** which JEngine's author has contributed into.
  - **Code hot update** is based on **[ILRuntime](https://github.com/Ourpalm/ILRuntime)** which JEngine's author has also contributed into its Unity Project.
  - **Encrypts** your hot-updatable **codes and resources**, codes will go into your assetbundles, assetbundles will be encrypt within **VFS by XAsset**, and also, your **codes will be encrypted in AES-128 ECB mode**

- **Own [Action](Docs/en-us/JAction.md) solution**

  - **Less code, does more**

    ```c#
    JAction j = new JAction();
    j.Do(() => something toDo)
      .Until(() => something is done)
      .Repeat(() => something toDo, repeatCounts)
      .RepeatWhen(() => something toDo,
                  () => condition)
      .Delay(some times)
      .Execute();
    ```
    
  - Can be **Run in Main Thread**

    > As we know Task.Run in Unity will run in a new thread, which is not able to call most Unity APIs, now JAction found a solution of using Loom.

- **Own [UI](Docs/en-us/JUI.md) solution**

  - **Method-Chaining** style makes codes prettier and easier to visualize

    ```c#
    var JUI = Showcase.AddComponent<JUI>()
      .onLoop(t1 =>
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
  ```
  
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
  
- **Own [Behaviour](Docs/en-us/JBehaviour.md)** based on MonoBehaviour

  - **More friendly to manage lifecycle**

    - You can make loop easier using JEngine

    ```c#
    public class JBehaviourExample : JBehaviour
    {
      public override void Init()
      {...}
    
      public override void Run()
  {...}
    
      public override void Loop()
      {...}
    
    ```
  
  public override void End()
      {...}
    }
  
    ```
  
    ```
  
- **Own [Resource Management](Docs/en-us/JResource.md)** based on XAsset

  ```c#
  //Get Resource via Sync method
  TextAsset txt = JResource.LoadRes<TextAsset>("Text.txt");
  Log.Print("Get Resource with Sync method: " + txt.text);
  ```

  ```c#
  //Get Resource via Async method with callback
  JResource.LoadResAsync<TextAsset>("Text.txt",(txt)=>
  {
      Log.Print("Get Resource with Async method: " + txt.text);
  });
  ```

- **More to explore!!!**

JEngine has its own purpose to help developers **write powerful codes which are streamlined and beautiful**; and to **help developers easier making their games**.

**If you enjoy using JEngine, please give this repo a star!**



## Latest Features

- Advanced hotupdate dependecies cleaner

- Enhanced development mode
- Allows custom scenes addition

[Click here to see all version updates](CHANGE.md)



## Features

- **[Hot-update](Docs/en-us/WhyHotUpdate.md)** solution

  - No need to learn Lua, **C# codes can be hot-updated**
  - Drop your resources in specific directories and can be **generate hot-updatable resources automatically**, all you need to do is to press "Build Bundle" button, and to put  your what it generated into your server
  - **Encrypts DLL**, your hot-update codes are safe now unless someone got your encrypted password

- **[JBehaviour](Docs/en-us/JBehaviour.md)** is a Behaviour in JEngine  which is based on MonoBehaviour, and it is **easier to manage lifecycle** of UI elements

- **[JUI](Docs/en-us/JUI.md)** is a class in JEngine which can **enhance the performence of UI** elements based on UGUI

  - JUI borrowed concept from **MVVM Framework** and rewrote it, JUI supports **binding a data with an action,** once data has changed, the action will be called
  - You can choose to **either update your UI in specific Loop** with Frequency, or to update your UI only if the binded data changed
  - You can **get UI components more efficiently** with JUI via the generic method **Method<T>**
  - **Method-Chaning** style of coding makes your codes **prettier and easier to read**

- **[JAction](Docs/en-us/JAction.md)** is an extension rather than usual Action

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
    - Run in **Main Thread**
      - Call Unity APIs anytime
  
- **[Resource Management](Docs/en-us/JResource.md)** solution

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
- JPrefab, a better way to manage Prefab from Resources
- JUI with more extended APIs
- UI Special Effects
- Optiimize logics which can improve process speed (As always doing it)
- *Unity Editor FTP Tool (Maybe)*



## What is Hot update and Why

[Click here to have a read](Docs/en-us/WhyHotUpdate.md)



## Directory Structure **(IMPORTANT)**

Please clone this framework into your project and keep this directory structure

```
.
├── Assets
│   ├── Dependencies
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
```

### Description for those Directories

[Click here to have a read](Docs/en-us/DirectoriesDiscription.md)



## JEngine Hot Update Logics

![flowchart](https://s1.ax1x.com/2020/07/14/Uthp6A.png)



## Quick Start

#### Basics

> The basics tells you how to use this framework in basics (How to make your project hot-updatable)

[Click here to have a read](Docs/en-us/Basic.md)

#### Extensions

> The extensions will show you how to hot-update your game in coding levels (How to make hot-updatable codes)

[Click here to have a read](Docs/en-us/Extension.md)



#### Developement Guide

> This will enable you quick develop your game and you don't need to upload your latest hot-update resources into your server **(which saves your time)**

[Click here to have a read](Docs/en-us/DevelopmentGuide.md)

#### Common "Bugs"

[Click here to have a read](Docs/en-us/CommonBugs.md)



#### IMPORTANT

[Click here to have a read](Docs/en-us/Important.md)



## Development Environment

- Debuging Unity Engine Version: 2019.3.13f1

  > Should supports Unity Engine From 2018 LTS till the latest

- .net Environment: .net 2.0 standard

- Operating System: MacOS 10.15.5

  > Definityly supports windows



## Recommend Repositories

- [XAsset](https://github.com/xasset/xasset) - A more streamlined, efficient and secure Unity Resource Management Solution for you.
- [IFramework](https://github.com/OnClick9927/IFramework) - Simple Unity Tools
