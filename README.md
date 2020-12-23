# JENGINE v0.6.1 beta2

JEngine是针对Unity开发者设计的**开箱即用**的框架，封装了强大的功能，小白也能**快速上手**，**轻松制作**可以**热更新的游戏**

```mater```分支为开发者认为的可以正常使用，不会有太大问题的最新版本；

稳定版本请使用```0.5.x```分支；

```development```分支为开发分支，欢迎fork后在该分支进行修改并PR，也欢迎提交issue！



> 作者将于2020年12月11日，北京时间晚上9点进行JEngine0.6直播讲解，将讲解底层机制、与0.5版本的差距、功能使用等，敬请期待！
> v0.6系列文档将在直播后开发！



![topLanguage](https://img.shields.io/github/languages/top/JasonXuDeveloper/JEngine)
![size](https://img.shields.io/github/languages/code-size/JasonXuDeveloper/JEngine)
![issue](https://img.shields.io/github/issues/JasonXuDeveloper/JEngine)
![license](https://img.shields.io/github/license/JasonXuDeveloper/JEngine)
![last](https://img.shields.io/github/last-commit/JasonXuDeveloper/JEngine)

> QQ群: [921271552](https://jq.qq.com/?_wv=1027&k=cF4hODjW)
>
> 已有项目使用JEngine热更新，并成功上架iOS

[English Document](README_en-us.md)

![banner](https://s1.ax1x.com/2020/10/09/0rtUL4.png)


  ## 框架相关
| 主题                                                  | 地址                                                      |
| ----------------------------------------------------- | --------------------------------------------------------- |
| 教学视频：介绍+使用+更新+入门                         | [点击跳转](https://www.bilibili.com/video/BV14Z4y1N79J/)  |
| 教学视频：核心功能（上）自动绑定+基类+UI+资源解决方案 | [点击跳转](https://www.bilibili.com/video/BV1v54y1y7x7/)  |
| 教学视频：核心功能（中）JAction队列解决方案全系列     | [点击跳转](https://www.bilibili.com/video/BV1Pt4y1q7X2/)  |
| 教学视频：核心功能（下）数据持久化+对象池+助手类+面板 | [点击跳转](https://www.bilibili.com/video/BV12Z4y1N7eG/)  |
| 框架文档                                              | [点击跳转](https://xgamedev.uoyou.com/)                   |
| 知乎文章                                              | [点击跳转](https://zhuanlan.zhihu.com/p/218105381)        |
| 项目地址                                              | [GitHub地址](https://github.com/JasonXuDeveloper/JEngine) |
| 国内镜像                                              | [Gitee跳转](https://gitee.com/JasonXuDeveloper/JEngine)   |
| QQ群聊                                                | [点击加入](https://jq.qq.com/?_wv=1027&k=cF4hODjW)        |

  ## 框架实战
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

  ## 近期star趋势

  [![Stargazers over time](https://starchart.cc/JasonXuDeveloper/JEngine.svg)](https://starchart.cc/JasonXuDeveloper/JEngine)

  ## JEngine能够做些什么？

  - **[热更新解决方案](https://xgamedev.uoyou.com/startup-v0-6.html)**
    - **资源热更**基于[**XAsset**](https://github.com/xasset/xasset)，JEngine的作者是该框架贡献成员之一
    - **C#代码热更**基于[**ILRuntime**](https://github.com/Ourpalm/ILRuntime)，JEngine的作者也是该框架U3D工程贡献成员之一
    - **代码加密**，C#热更代码生成的**DLL**会通过**AES-128-ECB**模式加密进Assetbundle，运行游戏时动态解密
    - **资源加密**，XAsset包含VFS功能，可以对资源进行一定程度的加密，AssetStudio无法破解资源
    - **自动赋值**，热更脚本可自动添加到游戏物体或预制体，且可自动赋值，不需要手动写代码赋值
  - **[Action队列解决方案](https://xgamedev.uoyou.com/jaction-v0-6.html)**
    - **更少的代码，实现更多功能，效率大幅度提高**！
    - 轻松**执行、延时、等待、定期循环、条件循环、同步/异步运行、取消队列**
    - **主线程运行代码**
  - **[UI解决方案](https://xgamedev.uoyou.com/jui-v0-6.html)**
    - **UI框架**，单例模式更好管理界面
    - **组件自动获取**，继承面板基类后**通过name获取UI组件**
    - **UI周期**，轻松管理**生命周期**，**链式编程**让代码**更美观**
    - UI**定期循环**更新，可以选择**毫秒更新或帧更新**，可以指定更新**频率**
    - UI**绑定数据**，当**数据更新**，即可**执行绑定的方法**
  - **[基类解决方案](https://xgamedev.uoyou.com/jbehaviour-v0-6.html)**
    - **轻松管理**生命周期
    - 可以**帧循环**，或者**毫秒循环**
    - **不依赖MonoBehaviour**
    - **简单好用**
  - **[基于XAsset的资源加载方案](https://xgamedev.uoyou.com/jresource-v0-6.html)** 
    - 支持**同步/异步加载**资源
    - **泛型**方法，轻松使用
    - **异步加载**热更**场景**
  - **[自动绑定热更脚本到GameObject](https://xgamedev.uoyou.com/classbind-v0-6.html)**
    - 输入命名空间，类名，即可**自动绑定**热更脚本**至物体**
    - 可对**该脚本public/private/static数值进行赋值**
    - **特殊类型支持拖拽赋值**
    - **不需要是Mono类**，非MonoBehaviour派生类也可正常绑定至GameObject进行赋值，并且可以正常获取
  - **[数据持久化解决方案](https://xgamedev.uoyou.com/jsaver-v0-6.html)**
    - 字符串存本地
    - **JSON**存本地
    - **Protobuf**二进制存本地
    - **自带加密**
  - **[多语言解决方案](https://xgamedev.uoyou.com/localization-v0-6.html)**

    - **CSV配表**
    - 轻松**转换语种**
    - 可通过静态方法**获取key对应的字符串**
    - 可让Text**自动根据语言切换文字**
  - **[内存加密解决方案](https://xgamedev.uoyou.com/crypto-struct-v0-6.html)**

    - 支持**90%数值类型**
    - **偏移值**加密
    - **转JSON和Protobuf**于**普通数据结构一样**
    - 可以**捕获内存作弊**
  - **事件派发**解决方案
    - **不同于其他**事件派发解决**方案**，**不需要注册方法名**，只需要注册类
    - 给**类打标签**自动**注册类里全部方法**
    - **可**选**主线程或子线程**派发
  - **网络层**解决方案
    - 目前支持**Websocket**
    - 有一套**SocketIO**的消息模型，开箱即用
    - 支持同步、异步并行、纯异步发送消息
  - **对象池**解决方案
    - 大幅度提升性能及减少内存开销，相比于常规Instantiate操作
    - **无需重复**创建新对象！
    - **智能算法**，贪心算法匹配GameObject，对象池满可自动添加！
  - **加密解密**解决方案

    - **AES**加密
    - 支持**字符串**
    - 支持**二进制**
  - **序列化**解决方案

    - 转**String**
    - 转**JSON**
    - 转**Protobuf-net**二进制
  - 面板类
    - **ILRuntime适配器自动生成**
    - **Protobfuf文件与CS类互转**
  - **JEngine面板**
  
  - 还有更多功能，尽情自行探索！



 JEngine的目的是针对游戏开发者提供**精简、美观且高效**的**代码**功能，并且使游戏开发者**更加轻松的制作游戏**



 **如果你觉得JEngine对你有帮助，请给该框架一个Star！**

  





## v0.6.1 beta2 最新功能

- ILRuntime升级到 **v1.6.6**
- ILRuntime **跨域适配器自动生成优化**
- ILRuntime **自动注册适配器**
- 更多 JEngine **Demos**
- 更简洁的 **Logs**
- JBehaviour & JEvent 等 **支持定位报错到准确行数**
- UIUtility 新增缓存字典使 **加载UI组件更快**
- 支持 **全部MonoBehaviour事件** 
- 继承MonoBehaviour的基类自动生成 **带有MonoBehaviour周期事件注册的适配器**
- ClassBind支持根据情况智能挂载 **不同的适配器**
- 支持 **Add & Get Component** 更多 **基类继承MonoBehaviour**的类型
- 针对MonoBehaviour派生类的**适配器自动生成编辑器脚本**
- 支持  **```Invoke```, ```InvokeRepeating```, ```IsInvoking```, ```CancelInvoke```** 在热更里使用
- JEvent 支持 **监听基类方法**
- **Bug 修复** 关于保存预制体的修改

[点击此处查看历史版本功能（英文）](CHANGE.md)



## 即将推出

- ~~热更资源及代码的开发模式~~
- ~~加密解密DLL~~
- ~~对象池~~
- ~~内存加密类~~
- ~~JPrefab，一个更容易管理热更预制体的解决方案~~
- ~~Socket~~
- JUI延伸API
- UI特效
- 优化算法、代码（一直在优化）



## JEngine热更逻辑

![flowchart](https://s1.ax1x.com/2020/09/06/wenIpV.png)



## 推荐项目

- [XAsset](https://github.com/xasset/xasset) - 精简高效的资源热更框架
- [IFramework](https://github.com/OnClick9927/IFramework) - Simple Unity Tools
- [QFramework](https://github.com/liangxiegame/QFramework) - Your first K.I.S.S Unity 3D Framework.
