<div align="center">

![JEngine](https://socialify.git.ci/JasonXuDeveloper/JEngine/image?font=JetBrains+Mono&forks=1&issues=1&logo=https%3A%2F%2Fjengine.xgamedev.net%2Flogo.png&name=1&owner=1&pulls=1&stargazers=1&theme=Auto)

**Ship game updates without shipping a build.**

Runtime hot updates for all platforms — push code, assets, and logic without rebuilding.<br>
Powered by HybridCLR with built-in encryption and obfuscation.<br>
1,000+ games shipped · 100+ companies in production · built-in Claude Code AI integration.

[![Version](https://img.shields.io/github/v/release/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine/releases) [![Tests](https://github.com/JasonXuDeveloper/JEngine/actions/workflows/pr-tests.yml/badge.svg)](https://github.com/JasonXuDeveloper/JEngine/actions/workflows/pr-tests.yml) [![License](https://img.shields.io/github/license/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine/blob/master/LICENSE) [![Last Commit](https://img.shields.io/github/last-commit/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine/commits) [![Top Language](https://img.shields.io/github/languages/top/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine) [![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FJasonXuDeveloper%2FJEngine.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FJasonXuDeveloper%2FJEngine) [![CodeFactor](https://www.codefactor.io/repository/github/jasonxudeveloper/jengine/badge)](https://www.codefactor.io/repository/github/jasonxudeveloper/jengine) [![OpenSSF Scorecard](https://api.securityscorecards.dev/projects/github.com/JasonXuDeveloper/JEngine/badge)](https://securityscorecards.dev/viewer/?uri=github.com/JasonXuDeveloper/JEngine) [![OpenSSF Best Practices](https://www.bestpractices.dev/projects/11830/badge)](https://www.bestpractices.dev/projects/11830) [![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/JasonXuDeveloper/JEngine)

[Documentation](https://jengine.xgamedev.net/) | [中文文档](https://github.com/JasonXuDeveloper/JEngine/blob/master/README_zh_cn.md)

</div>

&nbsp;

## Why JEngine

<table role="presentation">
<tr>
<td width="33%" valign="top">

### Ultralight Core

A handful of files — that's the entire core package. Drop it in and hot updates just work. No boilerplate code required.

</td>
<td width="33%" valign="top">

### 10x+ Performance

HybridCLR runs standard C# natively — 10x+ faster than ILRuntime or Lua. UniTask for GC-free async, Nino for zero-allocation serialization.

</td>
<td width="33%" valign="top">

### Built-in Security

Encrypt assets and DLLs with XOR, AES, or ChaCha20. Obfuscate hot update code with Obfuz. No other hot update framework ships this.

</td>
</tr>
<tr>
<td width="33%" valign="top">

### One-Click Build

Compile code, generate AOT metadata, obfuscate, encrypt, and package resources — all from one button in the JEngine Panel.

</td>
<td width="33%" valign="top">

### All Platforms

iOS, Android, Windows, macOS, WebGL — plus WeChat, Douyin, Alipay, and TapTap mini-game platforms. YooAsset powers games with 1M+ DAU.

</td>
<td width="33%" valign="top">

### AI-Powered Development

First hot update framework with a [Claude Code plugin](.claude-plugin/). AI that understands JEngine patterns and writes idiomatic code for your game.

</td>
</tr>
</table>

&nbsp;

## Before & After

| Traditional Approach | With JEngine |
|---------------------|--------------|
| Learn Lua or TypeScript for hot update code | Standard C# — same language, same IDE, same debugging |
| Register cross-domain adapters (ILRuntime) | Zero adapters — HybridCLR runs C# natively |
| No built-in asset protection | XOR / AES / ChaCha20 encryption + code obfuscation |
| Manual multi-step build pipeline | One-click build from JEngine Panel |

&nbsp;

## Quick Start

**Unity 2022.3+** required. Follow the [Getting Started Guide](https://jengine.xgamedev.net/) to be up and running in minutes.

&nbsp;

## Packages

| Package | Type | Description |
|---------|------|-------------|
| **JEngine.Core** | Required | Hot update runtime — bootstrap, encryption, resource management |
| **JEngine.Util** | Optional | JAction (zero-GC async tasks) · JObjectPool (lock-free object pooling) |
| **JEngine.UI** | Optional | MessageBox (async dialog prompts) · Editor UI (modern UI Toolkit components with theming) |

Built on [HybridCLR](https://github.com/focus-creative-games/hybridclr) · [YooAsset](https://github.com/tuyoogame/YooAsset) · [UniTask](https://github.com/Cysharp/UniTask) · [Nino](https://github.com/JasonXuDeveloper/Nino) · [Obfuz](https://github.com/focus-creative-games/obfuz)

&nbsp;

## 🎉 Latest Features (v1.1.6)

- **core,ui**: add configurable Bootstrap text and JTabView component ([#624](https://github.com/JasonXuDeveloper/JEngine/pull/624))
- **core**: bump Nino to fix Dictionary serialization in HybridCLR ([#627](https://github.com/JasonXuDeveloper/JEngine/pull/627))
- **core**: auto-detect manifest decryption in PreprocessBuildCatalog ([#626](https://github.com/JasonXuDeveloper/JEngine/pull/626))

[📋 View Complete Changelog](CHANGE.md)

&nbsp;

## Community

[![Stargazers over time](https://starchart.cc/JasonXuDeveloper/JEngine.svg)](https://starchart.cc/JasonXuDeveloper/JEngine)

<img src="https://contrib.rocks/image?repo=JasonXuDeveloper/JEngine"/>

&nbsp;

## Related Projects

**JEngine Ecosystem:**
- [MetaJUI](https://github.com/Meta404Dev/MetaJUI) — Customized UI framework for JEngine
- [MetaExcelDataTool](https://github.com/Meta404Dev/MetaExcelDataTool) — Excel table guide tool

**Other Great Unity Projects:**
- [IFramework](https://github.com/OnClick9927/IFramework) · [QFramework](https://github.com/liangxiegame/QFramework) · [TEngine](https://github.com/ALEXTANGXIAO/TEngine) · [BundleMaster](https://github.com/mister91jiao/BundleMaster) · [Nino](https://github.com/JasonXuDeveloper/Nino)

---

<div align="center">

**If JEngine helps your project, give it a ⭐**

</div>
