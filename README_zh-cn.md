# JENGINE v0.4.1

JEngine是针对Unity开发者设计的**精简易用**的框架

> QQ群号: 921271552

> 将在100收藏后进行一次直播教学

[English Document](README.md)

JEngine能够做些什么？

- **[热更新解决方案 ](Docs/zh-cn/WhyHotUpdate.md)**

  - **资源热更**来自[**XAsset**](https://github.com/xasset/xasset)，JEngine的作者是该框架贡献成员之一
  - **C#代码热更**来自[**ILRuntime**](https://github.com/Ourpalm/ILRuntime)，JEngine的作者也是该框架U3D工程贡献成员之一
  - **代码加密**，C#热更代码生成的**DLL**会通过**AES-128-ECB**模式加密进Assetbundle，运行游戏时动态解密
  - **资源加密**，XAsset包含VFS功能，可以对资源进行一定程度的加密，AssetStudio无法破解资源

- **[Action队列解决方案](Docs/zh-cn/JAction.md)**

  - **更少的代码，实现更多功能，效率大幅度提高**！

  - 轻松**执行、延时、等待、定期循环、条件循环、同步/异步运行、取消队列**

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
    
  - **主线程运行代码**

- **[UI生命周期解决方案](Docs/zh-cn/JUI.md)**

  - **轻松**管理**UI周期**，**链式编程**让代码**更美观**

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

  - UI**定期循环**更新？再也不是问题！

    - 可以选择**毫秒更新或帧更新**，可以指定更新**频率**

    ```c#
    t.FrameMode = false;//Run in ms
    t.Frequency = 1000;//Loop each 1s
    ```

  - UI**绑定数据**？轻松搞定！

    - 将**UI和数据绑定**，当**数据更新**，即可**执行绑定的方法**

    ```c#
    var JUI = b.AddComponent<JUI>()
      .Bind(data.b)
      .onMessage(t1 =>
                 {
                   ...
                 })
      .Activate();
    ```

- **[基于MonoBehaviour扩展的生命周期](Docs/zh-cn/JBehaviour.md)**

  - **轻松管理**生命周期

    - 类似JUI，可以**调整循环频率**，或者**不循环**

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

- **[基于XAsset的资源加载方案](Docs/zh-cn/JResource.md)** 

  - 支持**同步/异步加载**资源
  - **泛型**方法，轻松使用

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

- 还有更多功能，尽情自行探索！

JEngine的目的是针对游戏开发者提供**精简、美观且高效**的**代码**功能，并且使游戏开发者**更加轻松的制作游戏**

**如果你觉得JEngine对你有帮助，请给该框架一个Star！**



## 最新功能

- JAction**现已支持运行在主线程**

  > Task.Run的异步方法，在Unity中并不可以在主线程运行，因此，会导致绝大部分Unity的API无法使用，JAction现已克服该问题，运用Loom解决方案
  
- 提高**开发模式性能**

[点击此处查看历史版本功能（英文）](CHANGE.md)


## 特色功能

- **[热更新解决方案](Docs/zh-cn/WhyHotUpdate.md)**

  - 无需学习Lua，**C#代码能够直接热更**！
  - 将**不同类型的资源放进指定文件夹**，只需**点击**目录中的*JEngine/XAsset/Build Bundles***按钮**，即可**自动生成热更资源！**
  - **热更DLL加密**，AES加密，目前**最安全的加密方式**！

- **[JBehaviour](Docs/zh-cn/JBehaviour.md)**是一个**基于但代替MonoBehaviour**的解决方案

  - **链式编程**，让代码可读性提高！
  - **易于管理**生命周期，**于MonoBehaviour类似**的生命周期，但**对循环进行了大幅度优化提升**！
  - **指定**循环**模式及频率**，可以**毫秒循环更新**，也可以**帧循环更新**，**频率**随心所欲控制！

- **[JUI](Docs/zh-cn/JUI.md)**是一个**提升UI控件性能**的解决方案

  - **链式编程**，让代码可读性提高！
  - **类似MVVM数值绑定**，单个UI控件可**绑定数值**，当**数值改变**，会**执行**指定**方法**，**操作性比常规数值绑定更高效**！
  - **可定期循环更新UI**，基于JBehaviour，可以**指定更新模式和频率**！
  - **轻松获取UI组件**，调用**内部泛型方法获取UI控件上挂的组件**，有缓存字典提高效率！

- **[JAction](Docs/zh-cn/JAction.md)**是一个针对**Action构建的任务队列**解决方案

  - **链式编程**，让代码可读性提高！
  - **支持最常用的功能！**
    - 执行方法
    - 延时
    - 条件等待
    - 定期（次数）循环
    - 条件循环
    - *还有更多未列出...*
  - **更少的代码，更强的功能！**
  - 支持在**主线程运行代码**
  
- **[资源加载解决方案](Docs/zh-cn/JResource.md)**

  - 基于XAsset！
  - 可以**同步/异步**获取资源！
  - **泛型方法**加载资源！

- **对象池解决方案** 

  - **大幅度提升性能及内存开销**，相比于常规Instantiate操作
    - **无需重复**创建新对象！
    - **智能算法**，贪心算法匹配GameObject，对象池满可自动添加！
  - 简单友好

  > 教程暂未完成

- [**GUI-Redis**](https://github.com/JasonXuDeveloper/Unity-GUI-Redis)是一个可视化Redis数据库管理工具

  - 可**SSH连接**，安全高效！
  - 可**常规连接**，IP+端口连接
  - 支持**常规Key-Value数据**的**增删改查**



## 即将推出

- ~~热更资源及代码的开发模式~~
- ~~加密解密DLL~~
- ~~对象池~~
- JPrefab一个更容易管理热更预制体的解决方案
- JUI延伸API
- UI特效
- 优化算法、代码（一直在优化）
- Unity内置FTP工具（可能会做）



## 为何热更新

[点击阅读](Docs/zh-cn/WhyHotUpdate.md)



## 目录结构（重要）

请Clone该项目并保持Project目录结构

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

### 目录介绍

[点击阅读](Docs/zh-cn/DirectoriesDiscription.md)



## JEngine热更逻辑

![flowchart](https://s1.ax1x.com/2020/07/14/Uthp6A.png)



## 快速开始

#### 热更基础

> 热更基础将告诉你如何让游戏支持热更资源和代码

[点击阅读](Docs/zh-cn/Basic.md)

#### 热更代码

> 该环节将告诉你如何编写可以热更的代码

[点击阅读](Docs/zh-cn/Extension.md)

<br>

[ILRuntime文档](https://ourpalm.github.io/ILRuntime/public/v1/guide/tutorial.html)



#### 开发模式

> 在开发时，开启该模式，将大幅度减少您的开发耗时

[点击阅读](Docs/zh-cn/DevelopmentGuide.md)



#### 常见“问题”

[点击阅读](Docs/zh-cn/CommonBugs.md)



#### 重要须知

[点击阅读](Docs/zh-cn/Important.md)



## 开发环境

- Unity版本：2019.3.13f1

  > 理论上支持 Unity 2018 LTS 至最新版本

- .net环境： .net 2.0 standard

- 开发系统：MacOS 10.15.5

  > 100%支持Windows



## 推荐项目

- [XAsset](https://github.com/xasset/xasset) - 精简高效的资源热更框架
- [IFramework](https://github.com/OnClick9927/IFramework) - Simple Unity Tools
