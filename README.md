# JENGINE v0.6.3

JEngine是针对Unity开发者设计的**开箱即用**的框架，封装了强大的功能，小白也能**快速上手**，**轻松制作**可以**热更新的游戏**

```mater```分支为开发者认为的可以正常使用，不会有太大问题的最新版本，建议使用，功能最为强大；

```0.5.x```分支有部分商业项目正在使用，<u>不会在进行更新</u>；

```development```分支为开发分支，欢迎fork后在该分支进行修改并PR，也欢迎提交issue！

![topLanguage](https://img.shields.io/github/languages/top/JasonXuDeveloper/JEngine)
![issue](https://img.shields.io/github/issues/JasonXuDeveloper/JEngine)
![license](https://img.shields.io/github/license/JasonXuDeveloper/JEngine)
![last](https://img.shields.io/github/last-commit/JasonXuDeveloper/JEngine)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FJasonXuDeveloper%2FJEngine.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FJasonXuDeveloper%2FJEngine?ref=badge_shield)
[![CodeFactor](https://www.codefactor.io/repository/github/jasonxudeveloper/jengine/badge)](https://www.codefactor.io/repository/github/jasonxudeveloper/jengine)



> QQ群: [921271552](https://jq.qq.com/?_wv=1027&k=cF4hODjW)
>
> 已有项目使用JEngine热更新，并成功上架iOS
>
> 本框架目前使用的资源管理模块是魔改后的XAsset4.0，基本解决了原插件中概率性出现的bug，如要接入XAsset 7.0或XAsset Pro请自行解决！！！

[English Document](README_en-us.md)

![banner](https://s1.ax1x.com/2020/10/09/0rtUL4.png)



## 安装方式

### 推荐方式

进入master分支，选择clone，然后打开框架即可，移植时把老游戏项目的内容复制到框架工程（温馨提示，记得备份）

### 其他方式

npm下载方式：```npm i com.jasonxudeveloper.jengine```

upm下载方式：

1. 自动安装：```openupm add com.jasonxudeveloper.jengine```

2. 手动安装：

   1. 打开[Packages/manifest.json](https://docs.unity3d.com/Manual/upm-manifestPrj.html)

   2. 写入：

      ```json
      {
          "scopedRegistries": [
              {
                  "name": "package.openupm.com",
                  "url": "https://package.openupm.com",
                  "scopes": [
                      "com.jasonxudeveloper.jengine",
                      "com.ourpalm.ilruntime"
                  ]
              }
          ],
          "dependencies": {
              "com.jasonxudeveloper.jengine": "0.6.3"
          }
      }
      ```

      

  ## 框架相关

| 主题                                 | 地址                                                      |
| ------------------------------------ | --------------------------------------------------------- |
| JEngine v0.6直播回放                 | [点击跳转](https://www.bilibili.com/video/BV1My4y1B7FL/)  |
| JEngine v0.6功能一览                 | [点击跳转](https://www.bilibili.com/video/BV1Yv411j7wS/)  |
| 框架文档：短时间快速精通框架必看     | [点击跳转](https://xgamedev.uoyou.com/)                   |
| 知乎文章：JEngine介绍                | [点击跳转](https://zhuanlan.zhihu.com/p/218105381)        |
| 知乎文章：JEngine热更DLL内存加密方案 | [点击跳转](https://zhuanlan.zhihu.com/p/356693738)        |
| 项目原地址                           | [GitHub地址](https://github.com/JasonXuDeveloper/JEngine) |
| 国内快速下载地址                     | [Gitee跳转](https://gitee.com/JasonXuDeveloper/JEngine)   |
| QQ群                                 | [点击加入](https://jq.qq.com/?_wv=1027&k=cF4hODjW)        |

  ## 近期star趋势

  [![Stargazers over time](https://starchart.cc/JasonXuDeveloper/JEngine.svg)](https://starchart.cc/JasonXuDeveloper/JEngine)

  ## 为什么选择使用JEngine？

 JEngine的目的是针对游戏开发者提供**精简、美观且高效**的**代码**功能，并且使游戏开发者**更加轻松的制作游戏**

市面上的**Unity热更新框架**，**没有**一个**比JEngine**的**学习成本低**，使用**难度较大**，需要耗费**大量时间去入门、跑通框架**；

而**JEngine入门快的话可能几分钟即可，最多几小时即可跑通框架**，**简单易用**、**傻瓜式操作**，**不需要去关注管理热更资源**，**不需要关注任何热更新的底层**，**只管写游戏代码，做游戏场景即可**！

同时，需要没有热更功能，但是想要对接的老项目，接入只需1个月左右，且80%的业务代码无需修改，15%的业务代码只需优化，最后5%的业务代码是意外情况才需要进行修改的。同时，如果项目有拖拽MonoBehaviour到GameObject的习惯，JEngine现成的解决方案会让你事半功倍，让你的项目轻松支持热更新！

**JEngine**文档齐全，维护频繁，只要有issue，通常3日内必定解决，同时底层的热更源码经过大量上线项目验证，无需担心质量、性能、效率等问题！

 **如果你觉得JEngine对你有帮助，请给该框架一个Star！**

  

## v0.6.3 最新功能

- **实现** 自动在proto转c#的时候打 ```[System.Serializable]``` 标签
- **修改** ClassBind 默认 **active after**
- **优化**  **JFloat** 的精度
- **优化** 主工程 不再依赖热更工程的JBehaviour
- **全新** JEvent + Event Demo

[点击此处查看历史版本功能（英文）](CHANGE.md)



## JEngine专业版

JEngine专业版现已推出，大幅度提高开发效率和底层性能，可以根据需求进群联系作者进行购买，可以补差价升级

（满1K star之后会进行直播讲解专业版，同时会降价到39/299/799，维持一个月，之后变为99/599/999）

（开源版功能对于小项目开发和正常使用足矣，可以自行魔改来实现更完善的功能，或购买体验版进行尝试，好用再升级一年或永久版！提倡先用开源版开发，觉得好用再升级专业版，请理性消费！）

|                                           | 99元（体验版） | 399元（一年版） | 899元（永久版） |
| ----------------------------------------- | -------------- | --------------- | --------------- |
| 有效时长                                  | 永久           | 一年            | 永久            |
| 是否包含后续更新                          | 否             | 是              | 是              |
| 是否包含Bug修复                           | 否             | 是              | 是              |
| 是否包含专业版框架问题咨询                      | 否             | 是              | 是              |
| 免费远程框架问题解决次数                      | 0次            | 1次             | 3次             |
| 热重载                                    | 支持           | 支持            | 支持            |
| ClassBind对数组赋值                       | 支持           | 支持            | 支持            |
| 运行时Inspector支持显示数组               | 支持           | 支持            | 支持            |
| ClassBind可视化管理面板                     | 不支持           | 支持            | 支持            |
| ClassBind支持根据字段名自动匹配gameObject | 不支持         | 支持            | 支持            |
| 自定义ClassBind自动匹配正则               | 不支持         | 支持            | 支持            |
| 自行扩展Inspector运行时序列化             | 不支持         | 支持            | 支持            |
| 底层优化                                  | 部分           | 全面            | 全面            |



## JEngine热更逻辑

![flowchart](https://s1.ax1x.com/2020/09/06/wenIpV.png)



## 推荐项目

- [XAsset](https://github.com/xasset/xasset) - 精简高效的资源热更框架
- [IFramework](https://github.com/OnClick9927/IFramework) - Simple Unity Tools
- [QFramework](https://github.com/liangxiegame/QFramework) - Your first K.I.S.S Unity 3D Framework.
