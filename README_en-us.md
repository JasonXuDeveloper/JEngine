# JENGINE v0.6.1 p1

JEngine is a **streamlined** and **easy-to-use** framework designed for **Unity Programmers** which contains **powerful features**, beginners can **start up quickly and making hot update-able games easily**

This is branch is ```v0.5``` series branch, which is a Long term support (**LTS**) version, although **no updates will be added**, however, **bugs will be fixed**, feel free to commit PR and issue

![topLanguage](https://img.shields.io/github/languages/top/JasonXuDeveloper/JEngine)
![size](https://img.shields.io/github/languages/code-size/JasonXuDeveloper/JEngine)
![issue](https://img.shields.io/github/issues/JasonXuDeveloper/JEngine)
![license](https://img.shields.io/github/license/JasonXuDeveloper/JEngine)
![last](https://img.shields.io/github/last-commit/JasonXuDeveloper/JEngine)

> QQ Group ID: [921271552](https://jq.qq.com/?_wv=1027&k=cF4hODjW)

[中文文档](README.md)

![banner](https://s1.ax1x.com/2020/10/09/0rtUL4.png)

  ## About JEngine (Chinese)

| Theme                                                 | Location                                                  |
| ----------------------------------------------------- | --------------------------------------------------------- |
| 教学视频：介绍+使用+更新+入门                         | [点击跳转](https://www.bilibili.com/video/BV14Z4y1N79J/)  |
| 教学视频：核心功能（上）自动绑定+基类+UI+资源解决方案 | [点击跳转](https://www.bilibili.com/video/BV1v54y1y7x7/)  |
| 教学视频：核心功能（中）JAction队列解决方案全系列     | [点击跳转](https://www.bilibili.com/video/BV1Pt4y1q7X2/)  |
| 教学视频：核心功能（下）数据持久化+对象池+助手类+面板 | [点击跳转](https://www.bilibili.com/video/BV12Z4y1N7eG/)  |
| 框架文档                                              | [点击跳转](https://xgamedev.net)                          |
| 知乎文章                                              | [点击跳转](https://zhuanlan.zhihu.com/p/218105381)        |
| 项目地址                                              | [GitHub地址](https://github.com/JasonXuDeveloper/JEngine) |
| 国内镜像                                              | [Gitee跳转](https://gitee.com/JasonXuDeveloper/JEngine)   |
| QQ群聊                                                | [点击加入](https://jq.qq.com/?_wv=1027&k=cF4hODjW)        |

  ## JEngine Practice

| 主题                                                         | 地址                                                      |
| ------------------------------------------------------------ | --------------------------------------------------------- |
| 【Unity x JEngine 开发可热更爆款小游戏】第一集（系列介绍及游戏策划） | [点击跳转](https://www.bilibili.com/video/BV1sV41117ka/)  |
| 【Unity x JEngine 开发可热更爆款小游戏】第二集（构建项目及界面设计） | [点击跳转](https://www.bilibili.com/video/BV1m54y117vz/)  |
| 【Unity x JEngine 开发可热更爆款小游戏】第三集（场景界面设计） | [点击跳转](https://www.bilibili.com/video/BV1sk4y1C7b5/)  |
| 【Unity x JEngine 开发可热更爆款小游戏】第四集（界面设计及代码开发） | [点击跳转](https://www.bilibili.com/video/BV1hv411y7iC/)  |
| 【Unity x JEngine 开发可热更爆款小游戏】第五集（首页UI代码开发） | [点击跳转](https://www.bilibili.com/video/BV1ZT4y1c7t7/)  |
| 【Unity x JEngine 开发可热更爆款小游戏】第六集（Bug修复+代码开发+音乐配置） | [点击跳转](https://www.bilibili.com/video/BV1bi4y1E7e7/)  |
| 【Unity x JEngine 开发可热更爆款小游戏】第七集（关卡设计+编辑器开发+数据生成） | [点击跳转](https://www.bilibili.com/video/BV1sv411y7gF/)  |
| 【Unity x JEngine 开发可热更爆款小游戏】第八集（关卡读取+界面生成） | [点击跳转](https://www.bilibili.com/video/BV15f4y1B7oQ/)  |
| 【Unity x JEngine 开发可热更爆款小游戏】第九集（关卡生成+按钮事件+游戏管理类） | [点击跳转 ](https://www.bilibili.com/video/BV1zp4y1Y7cF/) |





## Stargazers over time

[![Stargazers over time](https://starchart.cc/JasonXuDeveloper/JEngine.svg)](https://starchart.cc/JasonXuDeveloper/JEngine)

## What can JEngine do?

  - **[Hot Update Solution ](https://github.com/JasonXuDeveloper/JEngine/wiki/开始使用)**
    - **Resource update** based on [**XAsset**](https://github.com/xasset/xasset), which JEngine's author is a contributor of this repo
    - **C# code update** based on [**ILRuntime**](https://github.com/Ourpalm/ILRuntime),  which JEngine's author is a contributor of this repo's Unity Demo
    - **Hot update code encryption**, Hot update **DLL** will be encrypted wiithin **AES-128-ECB** toAssetbundle, and will be decrypted while loading the game
    - **Resource encryption**, XAsset contains the feature of VFS, which can make some sort of encyption on resource, which AssetStudio is not able to hack resource
    - **Auto Class Bind**, scripts from hot update project is able to bind onto a gameObject/prefab, and can set values

  - **[Action Sequence Solution](https://github.com/JasonXuDeveloper/JEngine/wiki/JAction教程)**
    - **Less code, more powerful**
    - Codes are able to **runs, delay, wait, loop in counts, loop in conditions, run async/non-blocking**
    - **Can run in main thread**

  - **[UI Solution](https://github.com/JasonXuDeveloper/JEngine/wiki/JUI教程)**
    - **Easy to manage** life cycle of an UI, **support chaining-style** which makes codes **prettier**
    - Supports **Loop in frequency**, can choose **Loop in frames or milliseconds**

    - UI is **bindable to data**, when **data updates**, will **call the bound method of an UI**

  - **[Base Class Solution](https://github.com/JasonXuDeveloper/JEngine/wiki/JBehaviour教程)**
    - **Easy to manage** lifecycle
    - Can **Loop in frame**, or **Loop in milliseconds**
    - **Does not need MonoBehaviour**
    - **Easy to use**

  - **[Resource solution based on XAsset](https://github.com/JasonXuDeveloper/JEngine/wiki/JResource教程)** 
    - Support **Load resource sync/async**
    - **Generic method**, easy to get different format of resources
    - **Load scene async**

- **[Auto Class Bind](https://github.com/JasonXuDeveloper/JEngine/wiki/代码绑定)**
  - Write namespance, and class name, than it will **auto add hot update component** to a **gameObject/prefab**
  - Can set value **on public/private/static fields and properties**
  - **Support drag & drop on specific types**
  - **Does not require class to inherit MonoBehaviour**

- **[Data Persistence Solution](https://github.com/JasonXuDeveloper/JEngine/wiki/JSaver教程)**
  - Save string to local
  - Save **JSON**
  - Save **Protobuf**
  - **Auto encrypt**

- **[Localization](https://github.com/JasonXuDeveloper/JEngine/wiki/Localization教程)**
  - **CSV document to visualize and set up the words**
  - Easy to **switch language**
  - Can **get strings for differen keys in specific language**
  - **Text** can **bind to specific key**

- **[Anti-Memory Cheat solution](https://github.com/JasonXuDeveloper/JEngine/wiki/内存加密结构)**
  - Support **90% of data stucts**
  - **Offset** encryption
  - **Same size** while serialize to **JSON & Protobuf**
  - Allow to **catch hacker**

- **GameObject Pool Solution**
  - Compare to Instantiate, this saves memory and enhanced performance
  - **No need to create a GameObject repeatly**
  - **Auto match gameObject**

- **Encryption Solution**
  - **AES** encrypt
  - Support encrypt **string**
  - Support encrypt **binary**

- **Serialization Solution**
  - Convert to **String**
  - Convert to **JSON**
  - Convert to **Protobuf binary**

- **Editor Windows**
  - **ILRuntime Adapter auto generator**
  - **Protobfuf file** and **CS type** **two-way converter**

  - More to explore!



JEngine has its aim to support game developers with **streamlined, pretty and high-performance features**, and can help developers **make games easier**.

**If JEngine helps you, please give JEngine a star!**



## v0.6.1 p1 Latest Features

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

[Click here to see all version updates](CHANGE.md)



## Future Features

- ~~Supports local hot-update resources development in Unity Editor (Done)~~
- ~~Encrypt Hot-update DLL and decrypt in runtime~~
- ~~Object Pool which significantly improves performance rather than using Instantiate method~~
- ~~JPrefab, a better way to manage Prefab from Resources~~
- ~~Socket~~
- JUI with more extended APIs
- UI Special Effects
- Optiimize logics which can improve process speed (As always doing it)
- *Unity Editor FTP Tool (Maybe)*



## JEngine Hot Update Logics

![flowchart](https://s1.ax1x.com/2020/07/14/Uthp6A.png)

## Recommend Repositories

- [XAsset](https://github.com/xasset/xasset) - A more streamlined, efficient and secure Unity Resource Management Solution for you.
- [IFramework](https://github.com/OnClick9927/IFramework) - Simple Unity Tools
- [QFramework](https://github.com/liangxiegame/QFramework) - Your first K.I.S.S Unity 3D Framework.
