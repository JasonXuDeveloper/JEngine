# JENGINE v0.3.4

JEngine is a **streamlined and easy-to-use** framework for Unity Programmers.

JEngine has its **own behaviour** based on MonoBehaviour but much **more friendly to manage lifecycle**. (You can make loop easier using JEngine).

JEngine has its own **UI solution** which **enhance the performence** of an UI element. (You can choose either update UI in specific frequency or bind a data to the UI and update it when binded data has changed)

JEngine has a **method-chaining** style of coding, which makes your code much more stramlined and beautiful.

JEngine supports **hot update**, which is base on [XAsset](https://github.com/xasset/xasset) & [ILRuntime](https://github.com/Ourpalm/ILRuntime) which supports hot-update codes and resources in Unity.

JEngine has its own purpose to help developers **write powerful codes which are streamlined and beautiful**; and to **help developers easier making their games**.

[中文请点这里](#中文说明)



## Latest Features

- **JBehaviour Example**
- **Rewrite** JBehaviour **Structure**
- **Improve** JUI
- **Fix** that **JBehaviou**r can't be called

[Click here to see all version updates](CHANGE.md)



## Features

- **[Hot-update](WhyHotUpdate.md)** solution

  - No need to learn Lua, **C# codes can be hot-updated**
  - Drop your resources in specific directories and can be **generate hot-updatable resources automatically**, all you need to do is to press "Build Bundle" button, and to put  your what it generated into your server
  - **Encrypts DLL**, your hot-update dll won't be hacked easily

- Unique **Lifecycle** & **UI solution**

  - **[JBehaviour](JBehaviour.md)** is a Behaviour in JEngine  which is based on MonoBehaviour, and it is **easier to manage lifecycle** of UI elements

    <img src="https://s1.ax1x.com/2020/07/19/URW5mn.png" alt="JBehaviour" style="zoom:50%;" />

  - **[JUI](JUI.md)** is a class in JEngine which can **enhance the performence of UI** elements based on UGUI, with JUI, you can write codes shorter and in **Method-Chaning** form, which means **your codes can be shorter and more powerful**

    - JUI borrowed concept from MVVM Framework and changed it, JUI can bind a data easily with a **Generic Class**
    - You can choose to either update your UI in specific Loop with Frequency, or to update your UI only if the binded data changed

    <img src="https://s1.ax1x.com/2020/07/19/URWIwq.png" alt="JUI" style="zoom:50%;" />

    

- **GUI-Redis** helps visualize data in Redis Databases and can **modify data** in it.

  - Supports connect through **SSH tunnel**
  - Supports connect through **normay way** (IP, Port connection)
  - Supports **add/modify/delete/search** key-value pairs



## Future Features

- ~~Supports local hot-update resources development in Unity Editor (Done)~~
- ~~Encrypt Hot-update DLL and decrypt in runtime~~
- Optiimize logics which can improve process speed (As always doing it)
- UI Special Effects
- *Unity Editor FTP Tool (Maybe)*



## What is Hot update and Why

[Click here to have a read](WhyHotUpdate.md)



## Directory Structure **(IMPORTANT)**

Please clone this framework into your project and keep this directory structure

```
.
├── Assets
│   ├── Dependencies
│   │   ├── ILRuntime
│   │   │   ├── Editor
│   │   │   ├── Essential
│   │   │   ├── Generated
│   │   │   └── ILRuntime
│   │   ├── JEngine
│   │   │   ├── Core
│   │   │   └── Editor
│   │   ├── LitJson
│   │   └── XAsset
│   │       ├── Editor
│   │       ├── Resources
│   │       ├── Runtime
│   │       ├── ScriptableObjects
│   │       └── UI
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
│       └── APIs
├── Builds
├── DLC
├── HotUpdateScrpts
├── ProjectSettings
```

### Description of those Directories

[Click here to have a read](DirectoriesDiscription.md)



## JEngine Hot Update Logics

![flowchart](https://s1.ax1x.com/2020/07/14/Uthp6A.png)



## Quick Start

#### Basics

> The basics tells you how to use this framework in basics (How to make your project hot-updatable)

[Click here to have a read](Basic.md)

#### Extensions

> The extensions will show you how to hot-update your game in coding levels (How to make hot-updatable codes)

[Click here to have a read](Extension.md)



#### Developement Guide

> This will enable you quick develop your game and you don't need to upload your latest hot-update resources into your server **(which saves your time)**

1. Go to Unity Editor, open **Init scene**

2. Choose **Updater** from inspector

3. **Enable development mode** in Updater script section

   <img src="https://s1.ax1x.com/2020/07/16/UBC5uD.png" alt="guide1" style="width:50%;margin-left:25%" />



#### Common "Bugs"

- Cannot find Delegate Adapter for: **XXX**, Please add following code into Assets/Scripts/Init.cs 'InitializeILRuntime()' Method:

  > Just do what it asked you to add into **Scripts/Init.cs,  'InitializeILRuntime()' Method**
  >
  > ![bug1](https://s1.ax1x.com/2020/07/14/Ut2RoD.png)



#### IMPORTANT

- When **building bundle**, a window will pop up, and you need to **provide the encrypt key which encrypts your dll**

  ![key1](https://s1.ax1x.com/2020/07/26/apuoHs.png)
  
- In **Init scene**, select HotFixCode on hierarchy, you will see **Init (Script) on inspector**, there will be a section called **Key**, just **fill the encrypt key** in, and it will decrypt dll in runtime

  ![key2](https://s1.ax1x.com/2020/07/26/apu7En.png)

- When you build your project, **remember to remove hot-update scenes** in build setting panel to avoid redundancy

  ![build](https://s1.ax1x.com/2020/07/20/Uhxcuj.jpg)



## Using JEngine Features in Hot Updatable Scripts

#### JEngine.UI (JUI)

> JEngine now contains a new class which enhance the productivity of your UI (Supports any UGUI components, eg. Button, Text, Slider,etc.)
>
> Why choose JEngine.UI (JUI)?
>
> - Method-Chaning coding
> - Bind datas and update UI when datas are changed
> - Update UI in specific frequency
> - Simple but powerful

[Click here to have a read](JUI.md)





#### JBehaviour (Recommend to use JUI if you want to use this behaviour on an UI)

> JEngine now contains a **new behaviour** base on MonoBehaviour but **runs better**
>
> Why choose JBehaviour?
>
> - Simple lifecycle
> - Less codes to implement loops
> - Uses coroutine rather than methods to do updates

[Click here to have a read](JBehaviour.md)



#### Examples

- Just switch on development mode of the project you git
- Run the game and you can try the examples



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
