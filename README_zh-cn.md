# JENGINE v0.5.0

JEngine是针对Unity开发者设计的**开箱即用**的框架

![topLanguage](https://img.shields.io/github/languages/top/JasonXuDeveloper/JEngine)
![size](https://img.shields.io/github/languages/code-size/JasonXuDeveloper/JEngine)
![issue](https://img.shields.io/github/issues/JasonXuDeveloper/JEngine)
![license](https://img.shields.io/github/license/JasonXuDeveloper/JEngine)
![last](https://img.shields.io/github/last-commit/JasonXuDeveloper/JEngine)

> QQ群: [921271552](https://jq.qq.com/?_wv=1027&k=cF4hODjW)

[English Document](README.md)

[Wiki文档](https://github.com/JasonXuDeveloper/JEngine/wiki)，框架教程在这里

[知乎文章](https://zhuanlan.zhihu.com/p/218105381)

[Gitee码云链接 （国内快速克隆通道）](https://gitee.com/JasonXuDeveloper/JEngine)

## 近期star趋势

[![Stargazers over time](https://starchart.cc/JasonXuDeveloper/JEngine.svg)](https://starchart.cc/JasonXuDeveloper/JEngine)

## JEngine能够做些什么？

- **[热更新解决方案 ](https://github.com/JasonXuDeveloper/JEngine/wiki/开始使用)**

  - **资源热更**基于[**XAsset**](https://github.com/xasset/xasset)，JEngine的作者是该框架贡献成员之一
  - **C#代码热更**基于[**ILRuntime**](https://github.com/Ourpalm/ILRuntime)，JEngine的作者也是该框架U3D工程贡献成员之一
  - **代码加密**，C#热更代码生成的**DLL**会通过**AES-128-ECB**模式加密进Assetbundle，运行游戏时动态解密
  - **资源加密**，XAsset包含VFS功能，可以对资源进行一定程度的加密，AssetStudio无法破解资源
  - **自动赋值**，热更脚本可自动添加到游戏物体或预制体，且可自动赋值，不需要手动写代码赋值

- **[Action队列解决方案](https://github.com/JasonXuDeveloper/JEngine/wiki/JAction教程)**

  - **更少的代码，实现更多功能，效率大幅度提高**！

  - 轻松**执行、延时、等待、定期循环、条件循环、同步/异步运行、取消队列**

  - **主线程运行代码**
  
- **[UI生命周期解决方案](https://github.com/JasonXuDeveloper/JEngine/wiki/JUI教程)**

  - **轻松**管理**UI周期**，**链式编程**让代码**更美观**

  - UI**定期循环**更新？再也不是问题！
  
    - 可以选择**毫秒更新或帧更新**，可以指定更新**频率**
  
  - UI**绑定数据**？轻松搞定！
    
    - 将**UI和数据绑定**，当**数据更新**，即可**执行绑定的方法**
    
  
- **[基于MonoBehaviour扩展的生命周期](https://github.com/JasonXuDeveloper/JEngine/wiki/JBehaviour教程)**

  - **轻松管理**生命周期

    - 类似JUI，可以**调整循环频率**，或者**不循环**

- **[基于XAsset的资源加载方案](https://github.com/JasonXuDeveloper/JEngine/wiki/JResource教程)** 

  - 支持**同步/异步加载**资源
  - **泛型**方法，轻松使用

- **[自动绑定热更脚本到GameObject](https://github.com/JasonXuDeveloper/JEngine/wiki/代码绑定)**

  - 热更工程脚本想添加到GameObject太复杂？一个脚本，直接搞定！
  - 输入命名空间，类名，即可自动绑定热更脚本至物体
  - 可对该脚本public/private/static数值进行赋值
  - 支持数字，布尔，字符串，GameObject和GameObject上挂在的脚本

- **对象池**解决方案

  - 大幅度提升性能及内存开销，相比于常规Instantiate操作
  - **无需重复**创建新对象！
  - **智能算法**，贪心算法匹配GameObject，对象池满可自动添加！
  - 简单友好

- **[数据持久化解决方案](https://github.com/JasonXuDeveloper/JEngine/wiki/JSaver教程)**

  > JEngine现已支持JSaver，是数据持久化的工具
  >
  > JSaver能干什么？
  >
  > - 将数据转字符串/JSON/Protobuf字符串
  > - AES加密
  > - 存储本地
  > - 从本地加载（支持泛型）
  > - 获取本地是否有Key
  > - 删除本地Key

- 加密解密解决方案

  - AES加密
  - 支持字符串
  - 支持二进制

- 序列化解决方案

  - 转String
  - 转JSON
  - 转Protobuf-net二进制

- 还有更多功能，尽情自行探索！

JEngine的目的是针对游戏开发者提供**精简、美观且高效**的**代码**功能，并且使游戏开发者**更加轻松的制作游戏**

**如果你觉得JEngine对你有帮助，请给该框架一个Star！**



## 最新功能

- **重构** demos
- **重写** **StringifyHelper**
- **更新** ILRuntime
- **重写** JSaver
- **优化** 自动绑定
- **优化** 自动清理编辑器下热更工程
- **优化** LitJson当序列化和反序列化**BindableProperty可绑定数据** (**减少 80%字符长度** 当序列化可绑定数据)
- **适配** protobuf-net
- **Proto2CS** 转换生成器
- **重新整理** 热更工程
- **JResource** 先已支持**异步加载场景** 以及 **获取加载场景的进度**
- **合并** JBehaviour & JUI到JEngine.Core
- **优化** JBehaviour
- **优化** BindableProperty的ToString方法
- **优化** MonoBehaviour Adapter编辑器界面

[点击此处查看历史版本功能（英文）](CHANGE.md)



## 即将推出

- ~~热更资源及代码的开发模式~~
- ~~加密解密DLL~~
- ~~对象池~~
- JPrefab，一个更容易管理热更预制体的解决方案
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

![flowchart](https://s1.ax1x.com/2020/09/06/wenIpV.png)



## 开发环境

- Unity版本：2019.3.13f1 （请使用该版本及以上）

- .net环境： .net 2.0 standard

- 开发系统：MacOS 10.15.5

  > 100%支持Windows



## 推荐项目

- [XAsset](https://github.com/xasset/xasset) - 精简高效的资源热更框架
- [IFramework](https://github.com/OnClick9927/IFramework) - Simple Unity Tools
- [QFramework](https://github.com/liangxiegame/QFramework) - Your first K.I.S.S Unity 3D Framework.



## 由衷感谢

- Ekson（赞助）
- 程序-华仔（赞助）
- 马三（赞助）
