# JENGINE v0.2

JEngine is a streamlined and easy-to-use framework base on [XAsset](https://github.com/xasset/xasset) & [ILRuntime](https://github.com/Ourpalm/ILRuntime) which supports hot-update codes and resources in Unity.

[中文请点这里](#中文说明)



## Latest Features

- Supports development mode which **loads dll and resources from local**
- Automatically **clean up unnecessary dlls, pdbs, etc.** in DLL Resource Directory
- Automatically **convert dll into bytes** in Editor



## Features

- Automatically generate **Update Resources**
- Automatically handle **Hot-update DLL**
- Update [Hot-update](#What-is-Hot-update) codes and resources from server (Base on XAsset & ILRuntime)
- Supports development mode which **loads dll and resources from local**



## Future Features

- ~~Supports local hot-update resources development in Unity Editor~~
- Encrypt Hot-update DLL and decrypt in runtime
- Optiimize logics which can improve process speed
- *Unity Editor FTP Tool (Maybe)*



## What is Hot update

As we acknowledged, when developers update their apps/games, users will have to download the latest version from app stores, this is what we called "cold-update". Hot-update, sounds like a antonym of normal update process, it is a way which developers upload their newest codes & resources into their web server, and since users open their apps/games, they automatically download the latest codes and resources from the server, and those codes and resources reloads, overrides the old versions.

It sounds simple right? But it is **actually not** easy to implement.

In the old days, we can use a technology which is called "JIT" to implement hot-update, but then App Store blocks this way as their hardware doesn't supports JIT, which means developers needs to find a new way to solve it out.

There are two main ways to implement hot-update:

1. Use Lua

   - Lua is a script language just like javascript, when Unity loads the game, this solution will make a virtual environment which runs those lua codes
   - Lua files are just like other text files, it ends with .lua and can be readed as TextAsset
   - You have to learn Lua first to use this solution

2. Use ILRuntime

   - ILRuntime is a solution which loads dll (What you can get after you build your c# soulution) in game and loads the methods in it

   - ILRuntime runs faster then lua except doing calculation
   - ILRuntime is written in c# and easy to use, that is the reason why JEngine is based on ILRuntime, not Lua



## Directory Structure **(IMPORTANT)**

Please clone this framework into your project and keep this directory structure

```
.
├── Assets
│   ├── Dependencies
│   │   ├── ILRuntime
│   │   │   ├── Editor
│   │   │   ├── Essential
│   │   │   ├── Generated
│   │   │   └── ILRuntime
│   │   ├── JEngine
│   │   │   ├── Core
│   │   │   └── Editor
│   │   ├── LitJson
│   │   └── XAsset
│   │       ├── Editor
│   │       ├── Resources
│   │       ├── Runtime
│   │       ├── ScriptableObjects
│   │       └── UI
│   ├── HotUpdateResources
│   │   ├── Controller
│   │   ├── Dll
│   │   ├── Material
│   │   ├── Other
│   │   ├── Prefab
│   │   ├── Scene
│   │   ├── ScriptableObject
│   │   ├── TextAsset
│   │   └── UI
│   ├── Init.unity
│   └── Scripts
│       ├── Init.cs
│       └── APIs
├── Builds
├── DLC
├── HotUpdateScrpts
├── ProjectSettings
```

### Description of those Directories

- **Assets** - Source file for Unity
  - **Dependencies** - All 3rd parties source code can be put in here
    - **ILRuntime** - ILRuntime source code + demo
    - **JEngine** - JEngine source code (only a few right now, will be extended)
    - **XAsset** - XAsset source code (with modifications)
    - **LitJson** - LitJson source code, which provides converting json in c# (with modifications which ILRuntime can use it, which means you can use LitJson in your hot-update code and will be compatible with ILRuntime)
  - **HotUpdateResources** - All hot-updatable resources will be stored here
    - **Controller** - Drop your animation controller files here which will be hot-updated
    - **Dll** - Once you build your hot-update code solution, dll file will go into here and will automatically generate a .bytes file which can be hot-updated, *No need to put things here except HotUpdateScripts.dll*
    - **Material** - Drop your material files here which will be hot-updated
    - **Other** - Drop your files here which will be hot-updated (JSON files, WAV files,etc.)
    - **Prefab** - Drop your prefab files here which will be hot-updated
    - **Scene** - Drop your scene files here which will be hot-updated
    - **ScriptableObject** - Drop your scriptable object files here which will be hot-updated
    - **TextAsset** - Drop your text files here which will be hot-updated
    - **UI** - Drop your image files (.png format) here which will be hot-updated
  - **Scripts** - all scripts which will not be hot-updated but will be used in game
    - **Init.cs** - A **REALLY IMPORTANT** script which luanch the whole game
    - **APIs** - Drop all interfaces/methods/apis/etc you have here
  - **Init.unity** - the luanch scene of your game, which will do hot-update logics in here
- **Builds** - When you build your game, store those builds here (.apk, .xcodeproj, .app, .exe,etc.)
- **DLC** - Generated hot-update resources will be store here, all you need to do is to put the whole DLC directory and files include, into your web server
- **HotUpdateScripts** - Your hot-update scripts will be store here
  - **HotUpdateScripts/Program.cs** - Initialization for hot-update code, **you can change it but do not delete it, keep the RunGame method in Program.cs**
- **ProjectSettings** - Some project settings here, eg. allow unsafe code



## JEngine logics

![flowchart](https://s1.ax1x.com/2020/07/14/Uthp6A.png)



## Quick Start

#### Basics

> The basics tells you how to use this framework in basics

1. Clone this project

2. Open **ClonedProject/Assets** via Unity

3. Go to **HotUpdateResources/DLL & HotUpdateResources/Scene**, make sure you have **HotUpdateScripts.dll & Game.unity**

   <img src="https://s1.ax1x.com/2020/07/14/Ut6vWR.png" alt="check1" style="width:50%;margin-left:25%" />

   <img src="https://s1.ax1x.com/2020/07/14/UtcFTe.png" alt="check2" style="width:50%;margin-left:25%" />

4. On top menu, select **JEngine/XAsset/Build Bundles**

   <img src="https://s1.ax1x.com/2020/07/14/Ut6qwF.png" alt="menu" style="width:75%;margin-left:12.5%" />

5. Once it has built, upload your the whole **DLC** directory into your server (If you don't know how to do it, google search how to build a web server)

6. Go to **Init scene** in Unity, choose **Updater** in hierarchy

   <img src="https://s1.ax1x.com/2020/07/14/UtcuOf.png" alt="hierarchy" style="width:50%;margin-left:25%" />

7. In Inspector, change **Base URL** to your web host address which can locates hot-update files

   > Format: http://your-ip:your-port/DLC/ or http://your-domain/DLC/
   >
   > <img src="https://s1.ax1x.com/2020/07/16/UBC5uD.png" alt="inspector" style="width:50%;margin-left:25%" />

8. Now, run your game, and you will feel how hot-update works!

   > Note that stuffs shown in Unity Demo is in Chinese and will supports English in the future



#### Extensions

> The extensions will show you how to hot-update your game in coding levels

1. Open **ClonedProject/HotUpdateScripts/HotUpdateScripts.sln**
2. Find **Program.cs** and open it
3. Change something but **DO NOT DELETE** *RunGame()* Method
4. **Build Solution**
5. Go back to Unity, **redo step 4 & 5 in the Basics**
6. Run Game, and you will see the differences which you made



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



## Development Environment

- Debuging Unity Engine Version: 2019.3.13f1

  > Should supports Unity Engine From 2018 LTS til the latest

- .net Environment: .net 2.0 standard

- Operating System: MacOS 10.15.5

  > Should also supports windows

<br>
<br>
<br>
## 中文说明

> 将在v0.3更新中补充
