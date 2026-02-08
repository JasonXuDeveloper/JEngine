<div align="center">

![JEngine](https://socialify.git.ci/JasonXuDeveloper/JEngine/image?font=JetBrains+Mono&forks=1&issues=1&logo=https%3A%2F%2Fjengine.xgamedev.net%2Flogo.png&name=1&owner=1&pulls=1&stargazers=1&theme=Auto)

**线上热更，即时生效。**

全平台运行时热更新——无需重新打包，即可推送代码、资源和逻辑。<br>
基于 HybridCLR，内置加密与混淆。<br>
累计 1,000+ 款游戏上线 · 100+ 家企业生产环境验证 · 内置 Claude Code AI 工作流。

[![Version](https://img.shields.io/github/v/release/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine/releases) [![Tests](https://github.com/JasonXuDeveloper/JEngine/actions/workflows/pr-tests.yml/badge.svg)](https://github.com/JasonXuDeveloper/JEngine/actions/workflows/pr-tests.yml) [![License](https://img.shields.io/github/license/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine/blob/master/LICENSE) [![Last Commit](https://img.shields.io/github/last-commit/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine/commits) [![Top Language](https://img.shields.io/github/languages/top/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine) [![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FJasonXuDeveloper%2FJEngine.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FJasonXuDeveloper%2FJEngine) [![CodeFactor](https://www.codefactor.io/repository/github/jasonxudeveloper/jengine/badge)](https://www.codefactor.io/repository/github/jasonxudeveloper/jengine) [![OpenSSF Scorecard](https://api.securityscorecards.dev/projects/github.com/JasonXuDeveloper/JEngine/badge)](https://securityscorecards.dev/viewer/?uri=github.com/JasonXuDeveloper/JEngine) [![OpenSSF Best Practices](https://www.bestpractices.dev/projects/11830/badge)](https://www.bestpractices.dev/projects/11830) [![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/JasonXuDeveloper/JEngine)

[框架文档](https://jengine.xgamedev.net/zh/) | [English](https://github.com/JasonXuDeveloper/JEngine/blob/master/README.md) | [QQ群: 921271552](https://jq.qq.com/?_wv=1027&k=cF4hODjW)

</div>

&nbsp;

## 为什么选择 JEngine

<table role="presentation">
<tr>
<td width="33%" valign="top">

### 极致轻量

核心包仅数个文件——导入即用，无需编写任何模板代码即可实现热更新。

</td>
<td width="33%" valign="top">

### 10倍+性能

HybridCLR 原生运行标准 C#——性能是 ILRuntime 和 Lua 的 10 倍以上。UniTask 无 GC 异步，Nino 零分配序列化。

</td>
<td width="33%" valign="top">

### 内置安全

XOR、AES、ChaCha20 三种算法加密资源和 DLL。Obfuz 混淆热更代码。同类框架中独此一家。

</td>
</tr>
<tr>
<td width="33%" valign="top">

### 一键构建

编译代码、生成 AOT 元数据、混淆、加密、打包资源——JEngine 面板一键搞定。

</td>
<td width="33%" valign="top">

### 全平台

iOS、Android、Windows、macOS、WebGL——以及微信、抖音、支付宝、TapTap 小游戏平台。YooAsset 驱动百万 DAU 级游戏。

</td>
<td width="33%" valign="top">

### AI赋能

首个集成 [Claude Code 插件](.claude-plugin/)的热更框架。AI 深度理解 JEngine 模式，为你的游戏编写地道代码。

</td>
</tr>
</table>

&nbsp;

## 对比

| 传统方案 | 使用 JEngine |
|---------|-------------|
| 热更需要学 Lua 或 TypeScript | 标准 C#——同一语言、同一 IDE、同一调试器 |
| 注册跨域适配器 (ILRuntime) | 零适配器——HybridCLR 原生运行 C# |
| 无内置资源保护 | XOR / AES / ChaCha20 加密 + 代码混淆 |
| 手动多步构建流程 | JEngine 面板一键构建 |

&nbsp;

## 快速开始

需要 **Unity 2022.3+**。参阅[快速上手指南](https://jengine.xgamedev.net/zh/)，几分钟即可跑通。

&nbsp;

## 功能包

| 包名 | 类型 | 描述 |
|------|------|------|
| **JEngine.Core** | 必需 | 热更运行时——引导、加密、资源管理 |
| **JEngine.Util** | 可选 | JAction（零GC异步任务）· JObjectPool（无锁对象池） |
| **JEngine.UI** | 可选 | MessageBox（异步对话框）· 编辑器UI（现代 UI Toolkit 组件，支持主题系统） |

基于 [HybridCLR](https://github.com/focus-creative-games/hybridclr) · [YooAsset](https://github.com/tuyoogame/YooAsset) · [UniTask](https://github.com/Cysharp/UniTask) · [Nino](https://github.com/JasonXuDeveloper/Nino) · [Obfuz](https://github.com/focus-creative-games/obfuz) 构建

&nbsp;

## 🎉 最新功能 (v1.1.6)

- **core,ui**: add configurable Bootstrap text and JTabView component ([#624](https://github.com/JasonXuDeveloper/JEngine/pull/624))
- **core**: bump Nino to fix Dictionary serialization in HybridCLR ([#627](https://github.com/JasonXuDeveloper/JEngine/pull/627))
- **core**: auto-detect manifest decryption in PreprocessBuildCatalog ([#626](https://github.com/JasonXuDeveloper/JEngine/pull/626))

[📋 查看完整更新日志](CHANGE.md)

&nbsp;

## 社区

[![Stargazers over time](https://starchart.cc/JasonXuDeveloper/JEngine.svg)](https://starchart.cc/JasonXuDeveloper/JEngine)

<img src="https://contrib.rocks/image?repo=JasonXuDeveloper/JEngine"/>

&nbsp;

## 相关项目

**JEngine 生态：**
- [MetaJUI](https://github.com/Meta404Dev/MetaJUI) — 为 JEngine 定制的 UI 框架
- [MetaExcelDataTool](https://github.com/Meta404Dev/MetaExcelDataTool) — Excel 导表工具

**其他优秀 Unity 项目：**
- [IFramework](https://github.com/OnClick9927/IFramework) · [QFramework](https://github.com/liangxiegame/QFramework) · [TEngine](https://github.com/ALEXTANGXIAO/TEngine) · [BundleMaster](https://github.com/mister91jiao/BundleMaster) · [Nino](https://github.com/JasonXuDeveloper/Nino)

---

<div align="center">

**如果 JEngine 对你有帮助，请给个 ⭐**

</div>
