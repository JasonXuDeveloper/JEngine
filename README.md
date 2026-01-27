<div align="center">

![JEngine](https://socialify.git.ci/JasonXuDeveloper/JEngine/image?font=JetBrains+Mono&forks=1&issues=1&logo=https%3A%2F%2Fjengine.xgamedev.net%2Flogo.png&name=1&owner=1&pulls=1&stargazers=1&theme=Auto)

[![Version](https://img.shields.io/github/v/release/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine/releases) [![Tests](https://github.com/JasonXuDeveloper/JEngine/actions/workflows/pr-tests.yml/badge.svg)](https://github.com/JasonXuDeveloper/JEngine/actions/workflows/pr-tests.yml) [![License](https://img.shields.io/github/license/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine/blob/master/LICENSE) [![Last Commit](https://img.shields.io/github/last-commit/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine/commits) [![Top Language](https://img.shields.io/github/languages/top/JasonXuDeveloper/JEngine)](https://github.com/JasonXuDeveloper/JEngine) [![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FJasonXuDeveloper%2FJEngine.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FJasonXuDeveloper%2FJEngine) [![CodeFactor](https://www.codefactor.io/repository/github/jasonxudeveloper/jengine/badge)](https://www.codefactor.io/repository/github/jasonxudeveloper/jengine) [![OpenSSF Scorecard](https://api.securityscorecards.dev/projects/github.com/JasonXuDeveloper/JEngine/badge)](https://securityscorecards.dev/viewer/?uri=github.com/JasonXuDeveloper/JEngine) [![OpenSSF Best Practices](https://www.bestpractices.dev/projects/11830/badge)](https://www.bestpractices.dev/projects/11830) [![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/JasonXuDeveloper/JEngine)

**An out-of-the-box Unity framework for hot updatable games**

[Documentation](https://jengine.xgamedev.net/) | [中文文档](https://github.com/JasonXuDeveloper/JEngine/blob/master/README_zh_cn.md)

</div>

## Why JEngine?

- **Lightweight** - Only core package (few files) required; add optional packages as needed
- **One-Click Build** - JEngine Panel builds code + assets with obfuscation in one click
- **10x+ Performance** - HybridCLR outperforms ILRuntime/Lua hot update solutions
- **Zero/Minimal GC** - UniTask (GC-free async) + Nino (high-performance serialization)
- **All Platforms** - iOS, Android, Windows, macOS, WebGL, WeChat, Douyin, Alipay, TapTap
- **Secure Updates** - Obfuscate hot update DLL + encrypt resources (assets & DLL/PDB) with XOR/AES/ChaCha20
- **Commercial Ready** - Production-proven by individuals and enterprise teams

## Overview

JEngine is a Unity framework that enables **runtime hot updates** for games. Designed for both beginners and professionals, it provides secure, high-performance features out of the box.

## Quick Start

### Requirements
- **Unity 2022.3+**

### Branch Information

| Branch | Status | Description |
|--------|---------|-------------|
| `master` | **Recommended** | Latest stable version with the most powerful features |
| `0.8.x` | Legacy | Most popular 2023 version (no longer maintained) |
| `0.7.x` | Legacy | Most popular 2022 version (no longer maintained) |
| `0.6.x` | Legacy | Complete with full documentation (no longer maintained) |
| `0.5.x` | Legacy | Used by some commercial projects (no longer maintained) |

## Packages

### Official Packages

| Package | Type | Coverage | Description |
|---------|------|----------|-------------|
| **JEngine.Core** | Required | N/A | Hot update support with secure, high-performance features |
| **JEngine.Util** | Optional | [![Util Coverage](https://codecov.io/gh/JasonXuDeveloper/JEngine/branch/master/graph/badge.svg?flag=util)](https://codecov.io/gh/JasonXuDeveloper/JEngine) | Utility classes including JAction and JObjectPool |

**JEngine.Util Features:**
- **JAction** - Zero-GC chainable async task framework with fluent API, object pooling, delays, conditions, and loops
- **JObjectPool** - Thread-safe, lock-free generic object pooling using CAS operations

### Third-Party Packages

| Package | Description | Link |
|---------|-------------|------|
| **MetaJUI** | Customized UI framework for JEngine | [Repository](https://github.com/Meta404Dev/MetaJUI) |
| **MetaExcelDataTool** | Excel table guide tool | [Repository](https://github.com/Meta404Dev/MetaExcelDataTool) |

## Dependencies

| Dependency | Description | Repository |
|------------|-------------|------------|
| **Obfuz** | Code obfuscation | [GitHub](https://github.com/focus-creative-games/obfuz) |
| **HybridCLR** | Runtime code execution | [GitHub](https://github.com/focus-creative-games/hybridclr) |
| **YooAssets** | Runtime resource updates | [GitHub](https://github.com/tuyoogame/YooAsset) |

## 🎉 Latest Features (v1.0.10)

- **ci**: migrate from Claude to GitHub Copilot code review ([#588](https://github.com/JasonXuDeveloper/JEngine/pull/588))
- add CLA Assistant and Scorecard configuration ([#579](https://github.com/JasonXuDeveloper/JEngine/pull/579))
- add OSS health improvements and automation ([#577](https://github.com/JasonXuDeveloper/JEngine/pull/577))
- **core**: resolve SceneHandle ambiguous reference in Unity 6 ([#589](https://github.com/JasonXuDeveloper/JEngine/pull/589))
- **ci**: add pull-requests write permission for Claude review comments ([#585](https://github.com/JasonXuDeveloper/JEngine/pull/585))
- **ci**: fix auto-approve to check claude[bot] comment ([#583](https://github.com/JasonXuDeveloper/JEngine/pull/583))
- **ci**: move write permissions to job level for Scorecard compliance ([#581](https://github.com/JasonXuDeveloper/JEngine/pull/581))
- add auto-approve workflow and fix scorecard config ([#580](https://github.com/JasonXuDeveloper/JEngine/pull/580))
- resolve remaining CodeQL security issues ([#573](https://github.com/JasonXuDeveloper/JEngine/pull/573))
- address CodeQL security and code quality issues ([#572](https://github.com/JasonXuDeveloper/JEngine/pull/572))

[📋 View Complete Changelog](CHANGE.md)

## Project Statistics

[![Stargazers over time](https://starchart.cc/JasonXuDeveloper/JEngine.svg)](https://starchart.cc/JasonXuDeveloper/JEngine)

## Showcase

> If your project uses JEngine, feel free to contact the author - we'd be happy to showcase your work here!

| <img src="https://img.tapimg.com/market/lcs/b2d125432dffa4741c929ddceb3bf95f_360.png?imageMogr2/auto-orient/strip" alt="Alice's Dream Space" width="128" height="128" /> | <img src="https://img.tapimg.com/market/lcs/378c87731ce06ab8338977c90761a187_360.png?imageMogr2/auto-orient/strip" alt="Grinding Panic" width="128" height="128" /> | <img src="https://img.tapimg.com/market/lcs/0ac05aa58706032db05c5dbf1df77cf4_360.png?imageMogr2/auto-orient/strip" alt="Harvest Leeks" width="128" height="128" /> | <img src="https://img.tapimg.com/market/lcs/5c13a62dfeec50334f16e2f31db151e2_360.png?imageMogr2/auto-orient/strip" alt="MingMing Match" width="128" height="128" /> |
| ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
| <p align="center">[Alice's Dream Space](https://www.taptap.com/app/224117)</p> | <p align="center">[Grinding Panic](https://www.taptap.com/app/225175)</p> | <p align="center">[Harvest Leeks](https://www.taptap.com/app/232558)</p> | <p align="center">[MingMing Match](https://www.taptap.com/app/233988)</p> |
| <img src="https://img.tapimg.com/market/lcs/d561f17020081307ff08366233070d88_360.png?imageMogr2/auto-orient/strip" alt="Rabbit Restaurant" width="128" height="128" /> | <img src="https://media.9game.cn/gamebase/2022/6/29/a2fabeb2e1f4c048b58a7861d60affc3.png?x-oss-process=image/resize,w_256,m_lfit" alt="Cangyuan World" width="128" height="128" /> | <img src="https://img.tapimg.com/market/lcs/475635baa9a75856ea21a216a215f5b2_360.png?imageMogr2/auto-orient/strip" alt="Kz Spirit" width="128" height="128" /> | <img src="https://github.com/kkmjnh11110/MyResouse/blob/master/icon.PNG?raw=true" alt="Boundless Cinema" width="128" height="128" /> |
| <p align="center">[Rabbit Restaurant](https://www.taptap.com/app/235127)</p> | <p align="center">[Cangyuan World](https://www.9game.cn/hongmengshijie/)</p> | <p align="center">[Kz Spirit](https://www.taptap.com/app/238569)</p> | <p align="center">[Boundless Cinema](https://apps.apple.com/us/app/id1506237271)</p> |

## Contributors

<img src="https://contrib.rocks/image?repo=JasonXuDeveloper/JEngine"/>

## Recommendations

Check out these other excellent Unity frameworks:

- [IFramework](https://github.com/OnClick9927/IFramework) - Simple Unity Tools
- [QFramework](https://github.com/liangxiegame/QFramework) - Your first K.I.S.S Unity 3D Framework
- [TEngine](https://github.com/ALEXTANGXIAO/TEngine) - Unity framework solution
- [BundleMaster](https://github.com/mister91jiao/BundleMaster) - Unity resource loading master
- [Nino](https://github.com/JasonXuDeveloper/Nino) - Ultimate high-performance binary serialization library for C#.

---

<div align="center">

**If JEngine helps you, please give it a Star!**

</div>
