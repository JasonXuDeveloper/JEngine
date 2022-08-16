<p align="center">
    <img src="https://s4.ax1x.com/2022/01/16/7tP1V1.png" alt="logo" width="256" height="256">
</p>


<h3 align="center">JEngine</h3>

<p align="center">
  The solution that allows unity games update in runtime.
    <br>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/license/JasonXuDeveloper/JEngine" alt="license" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/last-commit/JasonXuDeveloper/JEngine" alt="last" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/issues/JasonXuDeveloper/JEngine" alt="issue" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/languages/top/JasonXuDeveloper/JEngine" alt="topLanguage" />
  </a>
  <a style="text-decoration:none">
    <img src="https://app.fossa.com/api/projects/git%2Bgithub.com%2FJasonXuDeveloper%2FJEngine.svg?type=shield" alt="status" />
  </a>
  <a style="text-decoration:none">
    <img src="https://www.codefactor.io/repository/github/jasonxudeveloper/jengine/badge" alt="badge" />
  </a>
  <br>
  <br>
  <a href="https://xgamedev.net/"><strong>Documentation »</strong></a>
  <br>
  <small>(The documentation is yet in Chinese and can use Google Translated version from <a href="https://translate.google.com/translate?sl=zh-CN&tl=en&u=https://xgamedev.net" target="_blank">here</a>)</small>
  <br>
  ·
  <br>
  <a href="https://github.com/JasonXuDeveloper/JEngine/blob/master/README_zh_cn.md">中文页面 »</a>
</p>


# JENGINE v0.7.3

**JEngine is an out-of-the-box framework designed for Unity developers. It encapsulates powerful functions. Beginners can also get started quickly and easily create games that can be updated hotly.**

The ```master``` branch is the latest version that the developers think can be used normally and will not have too many problems. It is recommended to use and has the most powerful functions (fixes bugs in versions 0.6 and 0.5 at the same time);

The ```0.6.x``` branch is currently in use by most commercial projects, <u>complete content, sufficient documentation and videos, and will not be updated or maintained any more</u>;

The ``0.5.x`` branch is being used by some commercial projects, <u>will not be updated or maintained</u>;

The ``development`` branch is the development branch. You are welcome to make changes and PRs in this branch after fork, and you are also welcome to submit issues!




## How to Download (VERY IMPORTANT)

> Because JEngine uses Git's Submodule module to install submodules, downloading the source code of this framework is not recommended to download zip directly from the website. There are currently 3 download methods:

1. Method 1, download the zip directly, then go to UnityProject/Assets/Dependencies, unzip the zip inside (must do if you download the zip archive)

1. Method 2, first git clone and then install the submodule

    ```bash
    git clone git@github.com:JasonXuDeveloper/JEngine.git
    cd JEngine
    git submodule init
    git submodule update
    ````

2. Method 3, install submodules along with git clone (recommended)

    ```bash
    git clone git@github.com:JasonXuDeveloper/JEngine.git --recursive
    ````

> Note, the github address here can be replaced with a gitee address
>
> If an error is reported when installing the submodule, you need to configure the ssh key of Github (or Gitee)



## Stargazers over time

[![Stargazers over time](https://starchart.cc/JasonXuDeveloper/JEngine.svg)](https://starchart.cc/JasonXuDeveloper/JEngine)

## JEngine Benefits

The purpose of JEngine is to provide **lean, beautiful and efficient** code functions for game developers, and to make game developers **easier to make games**

The **Unity hot update framework** on the market, **no one has a lower learning cost than JEngine**, it is more **difficult to use, and it takes a lot of time to get started , run through the framework**;

And JEngine may take a few minutes to get started quickly, and it can run through the framework in a few hours at most. **No need to pay attention to the bottom layer of any hot updates, Just write the game code and make the game scene**!

### Old Project

For projects that need to transform old projects that do not contain hot updates into hot update projects, JEngine is the best choice.

It usually only takes a few days to get familiar with the framework process. Then, you only need to classify and organize the resources that need to be updated into the corresponding folder. Next, copy the code that needs to be updated into the hot update project. Finally, use MonoBehaviour to hang all the scripts on the Inspector. Re-drag through ClassBind (you can implement a tool yourself to simplify the process).

Under normal circumstances, it only takes about 1 month to access, and 80% of the business code does not need to be modified, 15% of the business code only needs to be optimized, and the last 5% of the business code needs to be modified only in unexpected situations. At the same time, if the project has the habit of dragging MonoBehaviour to GameObject, JEngine's ready-made solution will make you do more with less and make your project easily support hot update!

### New Project

JEngine is very suitable for small and medium-sized projects, especially for independent game developers. The framework is convenient, fast, and powerful. Developers can control it in a short time and master hot update at a very small cost, basically without learning the principle of hot update.

For new projects, you only need to pull a copy of the JEngine source code, then import various plug-ins and SDKs into it, and finally formulate your own project specifications to start making games happily. Unlike other hot update frameworks, JEngine Pay more attention to development efficiency and improve the experience of operating hot update projects in the editor, so the efficiency of development under the editor is far better than other hot update frameworks.

### Summarize

**JEngine** has complete documentation and frequent maintenance. As long as there is an issue, it will usually be resolved within 3 days. At the same time, the underlying hot update source code has been verified by a large number of online projects, so there is no need to worry about quality, performance, efficiency and other issues!

**If you think JEngine is helpful to you, please give this framework a Star!**



## v0.7.3 New Features

- Bug **fixed**
- **Updated** ILRuntime
- **Updated** Bundle Master
- **Enhanced** JBehaviour performance
- **Enhanced** ClassBind runtime performance
- **Almost no GC allocation**  async wait method

[Click here to see the change log](CHANGE.md)



## Suggestions

  - [IFramework](https://github.com/OnClick9927/IFramework) - Simple Unity Tools
  - [QFramework](https://github.com/liangxiegame/QFramework) - Your first K.I.S.S Unity 3D Framework.
  - [TEngine](https://github.com/ALEXTANGXIAO/TEngine ) - Unity框架解决方案
  - [MetaJUI](https://github.com/Meta404Dev/MetaJUI) - MetaJUI是为JEngine定制的UI框架，当然你也可以通过很简单的修改，移植到自己的工程项目
  - [MetaExcelDataTool](https://github.com/Meta404Dev/MetaExcelDataTool) - MetaExcelDataTool是为JEngine定制的Excel导表工具，当然你也可以通过很简单的修改，移植到自己的工程项目
  - [BundleMaster](https://github.com/mister91jiao/BundleMaster) - Unity资源加载大师
  - [Nino](https://github.com/JasonXuDeveloper/Nino) - Definitely useful and high performance modules for C# projects, especially for Unity.
