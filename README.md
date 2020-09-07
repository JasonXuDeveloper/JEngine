# JENGINE v0.4.7



JEngine is a **streamlined and easy-to-use** framework designed for Unity Programmers.

![topLanguage](https://img.shields.io/github/languages/top/JasonXuDeveloper/JEngine)
![size](https://img.shields.io/github/languages/code-size/JasonXuDeveloper/JEngine)
![issue](https://img.shields.io/github/issues/JasonXuDeveloper/JEngine)
![license](https://img.shields.io/github/license/JasonXuDeveloper/JEngine)
![last](https://img.shields.io/github/last-commit/JasonXuDeveloper/JEngine)

> QQ Group ID: [921271552](https://jq.qq.com/?_wv=1027&k=cF4hODjW)

> Will hold one live broadcast on 19th of August, 2020

[中文文档](README_zh-cn.md)

[Wiki Page](https://github.com/JasonXuDeveloper/JEngine/wiki), tutorials are in here

[Gitee link (Clone faster in China)](https://gitee.com/JasonXuDeveloper/JEngine)

What can JEngine do?

- **[Hot-update](Docs/en-us/WhyHotUpdate.md) solution**

  - **Resource hot update & management** is based on **[XAsset](https://github.com/xasset/xasset)** which JEngine's author has contributed into.
  - **Code hot update** is based on **[ILRuntime](https://github.com/Ourpalm/ILRuntime)** which JEngine's author has also contributed into its Unity Project.
  - **Encrypts** your hot-updatable **codes and resources**, codes will go into your assetbundles, assetbundles will be encrypt within **VFS by XAsset**, and also, your **codes will be encrypted in AES-128 ECB mode**
  - **Auto bind** hot update scripts to gameObject, no need to write codes to add stuffs

- **Own [Action](Docs/en-us/JAction.md) solution**

  - **Less code, does more**

  - Can be **Run in Main Thread**
  
    > As we know Task.Run in Unity will run in a new thread, which is not able to call most Unity APIs, now JAction found a solution of using Loom.
  
- **Own [UI](Docs/en-us/JUI.md) solution**

  - **Method-Chaining** style makes codes prettier and easier to visualize

  - **Easier to manage** lifecycle
  
    - Can **easily set up** **what** you want the UI element **to do in specific time**
  
  - **Bindable to data**
    
    - **UI can be binded to a data**, once data  has changed, it will call the method that you has binded
    
  
- **Own [Behaviour](Docs/en-us/JBehaviour.md)** based on MonoBehaviour

  - **More friendly to manage lifecycle**

    - You can make loop easier using JEngine

- **Own [Resource Management](Docs/en-us/JResource.md)** based on XAsset

  - Support generic method
  - Async/Sync methods

- **[Auto bind](Docs/en-us/AutoBind.md)** scripts from Hot-Update DLL to GameObjects & Prefabs

  - Want to make scripts from hot-update project on to GameObject? Too much to write codes to add it? Here is a solution!
  - Fill in namespace, and class of a hotupdate scripts, it will automatically bind to a GameObject
  - Can set values of public/private/static fields automatically
  - Supports numbers, bools, strings, GameObject and Components on GameObject

- **Object Pool soulution**

  - MUCH MORE Enhances the performencerather than using**Instantiate method**
    - With this solution, **you don't have to repeat instantiate gameObject**
    - Just **tell JObjectPool what gameObject you will repeatedly create**, and **how many you except to create at the start**, it will do it for you
    - Request **PoolObject** to get the GameObject
  - Easy and powerful
  - With algorithm which fairly controls gameObjects

- **[Data persistence solution](Docs/zh-cn/JSaver.md)**

  > JEngine now supports JSaver, which is a data persistance tool
  >
  > What can JSaver do?
  >
  > - Convert data into string/JSON string
  > - AES encryption
  > - Save to local storage
  > - Load data from local storage (supports generic method)
  > - Judge if has key in local
  > - Delete local Key data

- **More to explore!!!**

JEngine has its own purpose to help developers **write powerful codes which are streamlined and beautiful**; and to **help developers easier making their games**.

**If you enjoy using JEngine, please give this repo a star!**



## Latest Features

- **Autobind** supports bind to GameObject **itself**
- Support **AES encrypt string**
- **JSaver**, support Data persistence

[Click here to see all version updates](CHANGE.md)



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



#### Hot Update Code

> How to write

[Click here to have a read](Docs/en-us/Extension.md)



> How to auto bind codes

[Click here to have a read](Docs/en-us/AutoBind.md)



> ILRuntime documents which leads to C# hot update

[ILRuntime Document](https://ourpalm.github.io/ILRuntime/public/v1/guide/tutorial.html)



#### Developement Guide

> This will enable you quick develop your game and you don't need to upload your latest hot-update resources into your server **(which saves your time)**

[Click here to have a read](Docs/en-us/DevelopmentGuide.md)

#### Common "Bugs"

[Click here to have a read](Docs/en-us/CommonBugs.md)



#### IMPORTANT

[Click here to have a read](Docs/en-us/Important.md)



## Development Environment

- Debuging Unity Engine Version: 2019.3.13f1 (Please use at least this version)

- .net Environment: .net 2.0 standard

- Operating System: MacOS 10.15.5

  > Definityly supports windows



## Recommend Repositories

- [XAsset](https://github.com/xasset/xasset) - A more streamlined, efficient and secure Unity Resource Management Solution for you.
- [IFramework](https://github.com/OnClick9927/IFramework) - Simple Unity Tools
