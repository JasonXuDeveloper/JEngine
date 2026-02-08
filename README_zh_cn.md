<div align="center">

![JEngine](https://socialify.git.ci/JasonXuDeveloper/JEngine/image?font=JetBrains+Mono&forks=1&issues=1&logo=https%3A%2F%2Fjengine.xgamedev.net%2Flogo.png&name=1&owner=1&pulls=1&stargazers=1&theme=Auto)

[![Version](https://img.shields.io/github/v/release/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine/releases) [![Tests](https://github.com/JasonXuDeveloper/JEngine/actions/workflows/pr-tests.yml/badge.svg)](https://github.com/JasonXuDeveloper/JEngine/actions/workflows/pr-tests.yml) [![License](https://img.shields.io/github/license/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine/blob/master/LICENSE) [![Last Commit](https://img.shields.io/github/last-commit/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine/commits) [![Top Language](https://img.shields.io/github/languages/top/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine) [![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FJasonXuDeveloper%2FJEngine.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FJasonXuDeveloper%2FJEngine) [![CodeFactor](https://www.codefactor.io/repository/github/jasonxudeveloper/jengine/badge)](https://www.codefactor.io/repository/github/jasonxudeveloper/jengine) [![OpenSSF Scorecard](https://api.securityscorecards.dev/projects/github.com/JasonXuDeveloper/JEngine/badge)](https://securityscorecards.dev/viewer/?uri=github.com/JasonXuDeveloper/JEngine) [![OpenSSF Best Practices](https://www.bestpractices.dev/projects/11830/badge)](https://www.bestpractices.dev/projects/11830) [![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/JasonXuDeveloper/JEngine)

**针对Unity开发者的开箱即用热更新框架**

[框架文档](https://jengine.xgamedev.net/zh/) | [English Documentation](https://github.com/JasonXuDeveloper/JEngine/blob/master/README.md)

</div>

## 为什么选择JEngine？

- **轻量化** - 仅需核心包（少量文件）；按需添加可选包
- **一键构建** - JEngine面板一键构建代码+资源，支持混淆
- **10倍+性能** - HybridCLR性能远超ILRuntime/Lua热更方案
- **零/极低GC** - UniTask（无GC异步）+ Nino（高性能序列化）
- **全平台支持** - iOS、Android、Windows、macOS、WebGL、微信、抖音、支付宝、TapTap
- **安全更新** - 热更DLL混淆 + 资源（资产和DLL/PDB）支持XOR/AES/ChaCha20加密
- **AI赋能** - AI[深度理解JEngine](.claude-plugin/)，无缝融入开发流程，效率倍增
- **商用验证** - 经个人和企业团队生产环境验证

## 概述

JEngine是针对Unity开发者设计的**开箱即用**的框架，封装了强大的功能，小白也能**快速上手**，**轻松制作**可以**热更新的游戏**。

> QQ群: [921271552](https://jq.qq.com/?_wv=1027&k=cF4hODjW)

## 快速开始

### 使用要求
- **Unity 2022.3+**

### 分支信息

| 分支 | 状态 | 描述 |
|------|------|------|
| `master` | **推荐使用** | 最新稳定版本，功能最为强大 |
| `0.8.x` | 历史版本 | 2023年最热门版本（不再维护） |
| `0.7.x` | 历史版本 | 2022年最热门版本（不再维护） |
| `0.6.x` | 历史版本 | 内容完善，文档视频充足（不再维护） |
| `0.5.x` | 历史版本 | 部分商业项目在使用（不再维护） |

## 功能包

### 官方包

| 包名 | 类型 | 覆盖率 | 描述 |
|------|------|--------|------|
| **JEngine.Core** | 必需 | N/A | 支持运行时热更，附带安全且高性能的功能 |
| **JEngine.Util** | 可选 | [![Util Coverage](https://codecov.io/gh/JasonXuDeveloper/JEngine/branch/master/graph/badge.svg?flag=util)](https://codecov.io/gh/JasonXuDeveloper/JEngine) | 工具类库，包含JAction和JObjectPool |
| **JEngine.UI** | 可选 | [![UI Coverage](https://codecov.io/gh/JasonXuDeveloper/JEngine/branch/master/graph/badge.svg?flag=ui)](https://codecov.io/gh/JasonXuDeveloper/JEngine) | 运行时和编辑器UI工具类 |

**JEngine.Util 功能:**
- **JAction** - 零GC链式异步任务框架，提供流畅API、对象池、延迟、条件和循环功能
- **JObjectPool** - 线程安全、无锁的通用对象池，使用CAS操作

**JEngine.UI 功能:**
- **MessageBox** - 零分配异步提示系统，支持UniTask集成和对象池，用于运行时UI
- **编辑器UI** - 借鉴shadcn的现代化UI Toolkit框架，支持主题系统和Bootstrap/Panel UI集成

### 第三方包

| 包名 | 描述 | 链接 |
|------|------|------|
| **MetaJUI** | 为JEngine定制的UI框架 | [仓库](https://github.com/Meta404Dev/MetaJUI) |
| **MetaExcelDataTool** | 为JEngine定制的Excel导表工具 | [仓库](https://github.com/Meta404Dev/MetaExcelDataTool) |

## 主要依赖

| 依赖项 | 描述 | 仓库 |
|--------|------|------|
| **Obfuz** | 代码混淆 | [GitHub](https://github.com/focus-creative-games/obfuz) |
| **HybridCLR** | 运行时代码执行 | [GitHub](https://github.com/focus-creative-games/hybridclr) |
| **YooAssets** | 运行时资源更新 | [GitHub](https://github.com/tuyoogame/YooAsset) |

## 🎉 最新功能 (v1.1.6)

- **core,ui**: add configurable Bootstrap text and JTabView component ([#624](https://github.com/JasonXuDeveloper/JEngine/pull/624))
- **core**: bump Nino to fix Dictionary serialization in HybridCLR ([#627](https://github.com/JasonXuDeveloper/JEngine/pull/627))
- **core**: auto-detect manifest decryption in PreprocessBuildCatalog ([#626](https://github.com/JasonXuDeveloper/JEngine/pull/626))

[📋 查看完整更新日志](CHANGE.md)

## 项目统计

[![Stargazers over time](https://starchart.cc/JasonXuDeveloper/JEngine.svg)](https://starchart.cc/JasonXuDeveloper/JEngine)

## 框架上线项目

> 若您的项目使用了JEngine，欢迎联系作者，作者很乐意把您的作品展示出来~

| <img src="https://img.tapimg.com/market/lcs/b2d125432dffa4741c929ddceb3bf95f_360.png?imageMogr2/auto-orient/strip" alt="爱丽丝造梦空间" width="128" height="128" /> | <img src="https://img.tapimg.com/market/lcs/378c87731ce06ab8338977c90761a187_360.png?imageMogr2/auto-orient/strip" alt="肝到发慌" width="128" height="128" /> | <img src="https://img.tapimg.com/market/lcs/0ac05aa58706032db05c5dbf1df77cf4_360.png?imageMogr2/auto-orient/strip" alt="割韭菜" width="128" height="128" /> | <img src="https://img.tapimg.com/market/lcs/5c13a62dfeec50334f16e2f31db151e2_360.png?imageMogr2/auto-orient/strip" alt="明明消消乐" width="128" height="128" /> |
| ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
| <p align="center">[爱丽丝造梦空间](https://www.taptap.com/app/224117)</p> | <p align="center">[肝到发慌](https://www.taptap.com/app/225175)</p> | <p align="center">[割韭菜](https://www.taptap.com/app/232558)</p> | <p align="center">[明明消消乐](https://www.taptap.com/app/233988)</p> |
| <img src="https://img.tapimg.com/market/lcs/d561f17020081307ff08366233070d88_360.png?imageMogr2/auto-orient/strip" alt="兔子与餐厅" width="128" height="128" /> | <img src="https://media.9game.cn/gamebase/2022/6/29/a2fabeb2e1f4c048b58a7861d60affc3.png?x-oss-process=image/resize,w_256,m_lfit" alt="沧元世界" width="128" height="128" /> | <img src="https://img.tapimg.com/market/lcs/475635baa9a75856ea21a216a215f5b2_360.png?imageMogr2/auto-orient/strip" alt="Kz之灵" width="128" height="128" /> | <img src="https://github.com/kkmjnh11110/MyResouse/blob/master/icon.PNG?raw=true" alt="无界电影" width="128" height="128" /> |
| <p align="center">[兔子与餐厅](https://www.taptap.com/app/235127)</p> | <p align="center">[沧元世界](https://www.9game.cn/hongmengshijie/)</p> | <p align="center">[Kz之灵](https://www.taptap.com/app/238569)</p> | <p align="center">[无界电影](https://apps.apple.com/us/app/无限生化/id1506237271)</p> |

## 贡献成员

<img src="https://contrib.rocks/image?repo=JasonXuDeveloper/JEngine"/>

## 推荐项目

查看这些其他优秀的Unity框架：

- [IFramework](https://github.com/OnClick9927/IFramework) - Simple Unity Tools
- [QFramework](https://github.com/liangxiegame/QFramework) - Your first K.I.S.S Unity 3D Framework
- [TEngine](https://github.com/ALEXTANGXIAO/TEngine) - Unity框架解决方案
- [BundleMaster](https://github.com/mister91jiao/BundleMaster) - Unity资源加载大师
- [Nino](https://github.com/JasonXuDeveloper/Nino) - 终极高性能C#二进制序列化库

---

<div align="center">

**如果你觉得JEngine对你有帮助，请给该框架一个Star！**

</div>
