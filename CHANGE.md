## All Versions
## 1.0.7 (January 25 2026)

- **Add DCO sign-off check for pull requests ([#558](https://github.com/JasonXuDeveloper/JEngine/pull/558))** (ci)
- **Include non-conventional commits in changelog ([#557](https://github.com/JasonXuDeveloper/JEngine/pull/557))** (ci)
- **Add automated testing and release workflows ([#554](https://github.com/JasonXuDeveloper/JEngine/pull/554))** (ci)
- **Use pipe delimiter in sed to avoid URL slash conflict ([#569](https://github.com/JasonXuDeveloper/JEngine/pull/569))** (ci)
- **Improve changelog with PR hyperlinks and proper contributors ([#568](https://github.com/JasonXuDeveloper/JEngine/pull/568))** (ci)
- **Use awk for reliable README multiline replacement ([#567](https://github.com/JasonXuDeveloper/JEngine/pull/567))** (ci)
- **Improve release workflow ([#566](https://github.com/JasonXuDeveloper/JEngine/pull/566))** (ci)
- **Store regex in variable to fix bash parsing error ([#564](https://github.com/JasonXuDeveloper/JEngine/pull/564))** (ci)
- **Use buildless mode for CodeQL Unity analysis ([#563](https://github.com/JasonXuDeveloper/JEngine/pull/563))** (ci)
- **Configure CodeQL to scan only JEngine source code ([#562](https://github.com/JasonXuDeveloper/JEngine/pull/562))** (ci)
- **Correct test artifact paths in unity-tests workflow ([#561](https://github.com/JasonXuDeveloper/JEngine/pull/561))** (ci)
- Revert "chore(release): v1.0.6" ([#565](https://github.com/JasonXuDeveloper/JEngine/pull/565))
- Fix build error handling and refactor Panel.cs ([#552](https://github.com/JasonXuDeveloper/JEngine/pull/552))
- Fix play mode test issues and refactor test code ([#551](https://github.com/JasonXuDeveloper/JEngine/pull/551))
- Fix play mode tests jumping to init scene instead of running ([#550](https://github.com/JasonXuDeveloper/JEngine/pull/550))
- Fix C# 9.0 Compilation Errors ([#548](https://github.com/JasonXuDeveloper/JEngine/pull/548))
- Add JEngine.Util package ([#543](https://github.com/JasonXuDeveloper/JEngine/pull/543))
- Add rootNamespace and TestRunnerCallbacks to JEngine.Core ([#545](https://github.com/JasonXuDeveloper/JEngine/pull/545))
- Add CLAUDE.md and GitHub Copilot custom instructions ([#547](https://github.com/JasonXuDeveloper/JEngine/pull/547))
- Remove development branch from documentation ([#546](https://github.com/JasonXuDeveloper/JEngine/pull/546))
- Update YooAsset samples to 2.3.18 and update dependencies ([#544](https://github.com/JasonXuDeveloper/JEngine/pull/544))
- Update hybridclr to 8.9.0 and add Codacy configuration ([#542](https://github.com/JasonXuDeveloper/JEngine/pull/542))
- Claude Code Review workflow
- Claude PR Assistant workflow
- Add using UnityEditor.UIElements to SettingsUIBuilder.cs and EditorUIUtils.cs
- Fix Unitask HandleBaseExtensions Add underline
- v1.0.5
- add standalone feature
- fix monobehaviour issue
- v1.0.4
- Update packages-lock.json
- v1.0.3
- v1.0.2
- enhance editor ui
- enhance editor ui
- fix #522 ([#523](https://github.com/JasonXuDeveloper/JEngine/pull/523))
- fix #522
- Update package.json
- doc update
- upgrade hybridclr
- doc link update
- update doc link
- Update package.json
- Delete UnityProject/UserSettings directory
- Update README.md
- doc update
- doc update
- Delete UnityProject/UnityProject.sln.DotSettings.user
- remove redundant files
- Delete mono_crash.12fa052fea.0.json
- mini game support + first-package policy support
- change all texts into english
- protect encryption methods
- bundle option modification
- faster chacha20 + encryption config editor
- chacha20 bundle encryption option
- aes bundle encryption option
- fix xor decryption gc issue
- polymorphic dll protection
- modular bundle encryption support + bug fix
- bootstrap callback opt
- editor tool update
- core package cleanup
- bootstrap common functions
- optimize bootstrap
- simplify editor
- better editor ui
- editor ui
- core code cleanup
- make jengine.core a unity package + editor ui opt
- migrate to hclr + obfuz (alpha to v1.0)
- fix JBehaviour LoopDeltaTime
- fix FpsMonitor FPS Value
- fix JBehaviour LoopDeltaTime when set targetFrameRate
- optimise InitJEngine
- slightly optimised JStream performance
- delete layout config
- optimise editor filesystem watcher performance
- demo hot update dll
- optimise JStream performance
- update yooasset to v2.1.2 + update project to untiy 2021lts
- allow jbehaviour call end while application quitting
- fix editor script potential issues
- update to 0.8.0f7
- fixed #478
- fixed #496
- fixed #478
- fixed #496
- Update README.md
- Update README.md
- fixed monobehaviour lifecycle bug
- fixed monobehaviour lifecycle bug
- fixed jump scene error
- fixed jump scene error
- fixed #489
- fixed #487
- fixed #488
- fixed #485
- fixed #484
- fixed #485
- fixed #484
- fixed #479
- fixed #481
- fixed #482
- removed optimizer
- allow await queueOnOtherThread + optimize performance
- awaitable queueOnMainThread
- Update README.md
- v0.8.0f6
- v0.8.0f5
- update doc
- Update package.json
- fix android issue with download
- update optimizer
- experimental - dll optimization
- minor bug fix for loading hot type in editor
- Update package.json
- fixed #463 + optimization on ClassBind/LifeCycleMgr
- fix async lifecycle bug
- v0.8.0
- Delete .gitmodules
- Update README_zh_cn.md
- fixed #457 + 0.8.0 release
- officially support WebGL
- project setting
- remove INIT_JE symbol
- clean up structure
- migrate to yooasset
- move unity-reorderable-list to 3rdParty
- update ILRuntime
- clean up code
-  update readme
- JStream key optimization
- fixed unmanagedMemoryPool intersecting bug
- unmanagedMemoryPool + coroutineMgr + optimization
- fixed install issue
- separated protobuf-net and localization + optimized file structure
- fixed LifeCycleMgr bug while destroying an registered inactivated object
- updated readme
- optimized MonoBehaviour.Invoke/InvokeRepeating performance & GC
- optimized JBehaviour/ClassBind/MonoBehaviour performance & GC
- optimized JBehaviour + ClassBind/JBehaviour awake
- cleaned code + updated readme
- optimized LifeCycleMgr + new feature UnsafeMgr + removed LitJson
- upload dependencies
- optimized JStream
- optimized LifeCycleMgr
- optimized JBehaviour + optimized JStream + optimized LifeCycleMgr
- optimized JStream and CryptoMgr
- optimized cryptoMgr
- fixed FIndObjectsOfType bug
- Update README.md
- removed submodules
- JEngine v0.8 preview
- changed log level
- removed TimeMgr #446
- optimized JBehaviour loop + fixed nino demo error
- fixed #445
- fixed #402
- updated nino.serialization
- fixed #428
- updated nino.shared
- fixed #437 + updated nino
- fixed #436
- fixed protobuf-net issues
- fixed #432
- fixed ClassBind load Image bug
- fixed download error
- fixed #429
- fixed wrong class name on classbind editor inspector
- updated readme + updated npm
- fixed #424
- fixed #419
- fixed #423
- fixed #420
- fix Serialize spell
- fixed Nino issue #31
- fixed #416
- updated change.md
- v0.7.5
- fixed fatal reg sequence bug
- update first time open bug
- update nino
- update nino
- fixed #406
- fixed LifeCycleMgr lifecycle bug + update nino
- automatically register ilrt code
- add nino ilruntime reg
- fixed loom thread bug + add new symbol
- fixed reload bug in ClassBindMgr + optimized FindObjectsOfType performance
- v0.7.5beta1
- release v0.7.4
- Update README.md
- v0.7.4
- fixed that lifeCycleMgr should not invoke update if a gameObject is inactive
- fixed #361
- update Nino
- v0.7.4
- fixed that lifeCycleMgr should not invoke update if a gameObject is inactive
- fixed #361
- update Nino
- fixed #395
- merge ([#394](https://github.com/JasonXuDeveloper/JEngine/pull/394))
- optimize stringTools api
- update new showcase
- update new showcase
- memory optimization
- memory optimization
- 优化JStream
- 接入Nino
- no gc jbehaviourMgr update + fixed #385
- fixed timeMgr bug
- no gc lifecyclrMgr + better time mgr
- fixed #384
- Update docs address
- no gc jbehaviourMgr update + fixed #385
- fixed timeMgr bug
- no gc lifecyclrMgr + better time mgr
- fixed #384
- Update docs address
- fixed #380 + fixed JBehaviour bug
- Update Init.unity
- Update Init.unity
- Update Init.unity
- fixed #378 + fixed clear cache bug
- fixed #372 #376
- Update LoadSceneHandler.cs
- update docs
- Update AssetsLoadSetting_1.asset
- Update RegisterCLRMethodRedirctionHelper.cs
- Update RegisterCLRMethodRedirctionHelper.cs
- fixed #368 #367 #361
- release v0.7.3
- bug fix
- fix build original file bug + local mode bug
- fix JAction & JBehaviour bug
- bug fix
- adjust update text
- v0.7.3
- upgrade bundle master (runtime part)
- fix compile bug
- fix build exception
- Update README_zh_cn.md
- update ILRuntime
- optimization
- fixed #349
- updated readme
- fixed #347
- Update Updater.cs
- fixed #341
- fixed #342
- fixed #329 #339
- fixed #340
- fixed JPrefab bug
- fixed bug + updated readme
- fixed #334
- added feature
- updated a few features
- 更新ilrt
- fixed JPrefab bug
- Update JPrefab.cs
- bug fix
- updated code
- updated accepting using pdb file in runtime, rather than only editor
- fixed #327
- fixed JBehaviour cannot get in Awake due to lifecycle
- fixed two fatal errors
- removed redudant code
- fixed display error
- updated npm + readme
- Update README.md
- v0.7.2
- fixed #321 #311 #309 #279 #270
- rewrite source code (incomplete)
- updated ETTask + included ETTask Exceptionhandler
- fixed async method exception StackTrace issue
- rewrite UnityEngine.Debug.unityLogger
- fixed #320
- async method suggestion
- Update README_zh_cn.md
- update tools.cs
- updated readme
- enhanced code
- fixed #275
- fixed #318 #313
- Update protobuf-net-v2-for-ILRuntime.zip
- fetched submodule
- Update README_zh_cn.md
- fixed #308
- fixed #316
- fixed #317
- enhanced JAction
- new protobuf demo + update protobuf-net
- new Protobuf demo
- upgrade ILRuntime + set up vs code breakpoint config
- 监听下载进度 监听到的FinishDownLoadBundleCount数量少一个
- Update BuildAssetsTools.cs
- BPath变量名称出现减号错误情况修复
- bug fix
- fixed #296
- bug fix
- fixed #294 + fixed localization bug
- 优化MonoBehaviour适配器
- update assetmgr
- bug fixed
- update asset mgr
- update
- update
- update
- 完善提示
- 首次进入项目生成lock文件
- Update README_zh_cn.md
- 分包demo
- 打包工具
- JEngine v0.7.1
- Update Proto2CSEditor.cs
- Update Proto2CSEditor.cs
- fixed #271 and #272
- fixed JBehaviour bug + remove JAsset
- release on npm
- Update README.md
- 0.7.0 released
- update readme
- fixed localization bug
- updated ILRuntime to 2.0.2 & fixed #265
- fixed #264
- update readme
- fixed #261
- fixed bug for jbehaviour
- generate clr bindings that enhance performance
- fixed JUI demo bug
- update ClassBind
- fixed bug + new feature
- fixed bug while GetComponents
- update comment
- new download method
- Validator module with demo
- update readme
- update readme
- ui extension
- update readme
- fixed JBehaviour bug
- updated readme
- update readme
- fixed monobehaviour lifecycle issue
- updated lifecycle
- update
- updated submodules
- fixed #247
- ClassBindIgnore feature
- fixed #238
- fixed #245
- fixed #231
- modified FindObjectsOfType method
- new JPrefab
- update
- modify JStream
- fixed a few bugs
- modify adapter code generation
- update
- fix #233
- update pbnet
- proto demo file
- demo场景微调
- update
- bug fix
- ILRuntime 2.0 + JIT support + bug fix
- preview
- inspector插件 submodule引用
- pbnet submodule引用
- Update JAsset
- 子模块引用
- Update README.md
- Update README.md
- Update README.md
- Update README.md
- 新增离线模式
- fix #224
- fix #223
- Create package.json
- 修改一些小问题
- Update Clean.cs
- 解耦Updater
- fix #211
- fix #215
- fix #216
- fix #212
- Update ClassBindEditor.cs
- Update ClassBindEditor.cs
- Update ClassBindEditor.cs
- 解耦XAsset+优化热更新流程+ClassBindEditor不再占用热更DLL
- 托管热更资源模块
- update git ignore + revert bundle version
- fix bug + modify xasset
- 优化生成跨域适配器
- fix issue
- fix minor issue
- 优化打ab包流程
- 优化pbnet的proto转cs
- fix #198
- fix #191  + fix a bug with ClassBind match all field types
- fix #192
- Update README.md
- fixed compilation errors
- Update RegisterDelegateConvertorHelper.cs
- Update BindableProperty.cs
- Update JUI.cs
- fixed indentations to align with original JEngine code
- support for delta messages on BindableProperties
- 支持通过基类搜寻派生类
- 修改编辑器序列化小问题
- 更新JUI绑定机制
- ClassBind底层大优化
- 写了能让人看懂的注释
- fix #176
- 解决主工程Instantiate挂了ClassBind的prefab无法激活的问题
- JInt op_Implicit redirection
- 防止有人用Unity的Invoke的时候手残去Invoke一个带参数的方法从而导致重定向处发生错误
- 多个更新
- Update PType.cs
- 【强烈建议升级】再也不需要pb序列化的时候注册热更Type了！！！
- fix #177
- remove redundant
- fix #177
- fix #174
- fix #156
- fix bug
- fix #168
- fix #162 + enhance ClassBind
- fix #161
- fix #160
- Update README_en-us.md
- Update README.md
- fix #157
- Update JEvent.cs
- v0.6.3
- fixed #142
- 修正不定义热更类中的Awake()，就会造成热更类中的OnEnable()不执行的bug
- fix #146
- Merge PR from Gitee
- update
- Update RegisterCLRMethodRedirctionHelper.cs
- Update JsonMapper.cs
- fixed #139
- enhanced GameStats
- better proto example
- removed ui framework
- fixed #134 + enhanced ClassBind
- fix #127 #128
- Revert "适配 xasset-6.1，去掉宏，通过代理剥离运行时对编辑器的依赖
- 适配 xasset-6.1，去掉宏，通过代理剥离运行时对编辑器的依赖
- fix #125 & enhanced anticheat & removed xasset pro from JEngine panel
- 新增Demo + fix #123
- Update JsonMapper.cs
- 修复BUG+新增DEMO
- 复原proto2cs
- Revert "Revert "优化proto2cs"
- Revert "优化proto2cs
- Update Localization.cs
- Revert "Update Localization.cs
- 优化proto2cs
- Update Localization.cs
- Update Localization.cs
- Update Proto2CSEditor.cs
- Update Updater.cs
- Update Download.cs
- 文档更新
- 修复Instantiate生成带ClassBind的脚本时可能出现的异常
- 修复了JEngine面板自动跳转场景Bug和Instantiate子物体ClassBind和Adapter不兼容问题
- Protobuf序列化demo新增enum实例
- 修复了通过对比Assembly名字判断是否为热更类型的隐患
- fix #109
- fix #107
- 优化：LitJson序列化、LitJson实例
- 新增：Protobuf示例、复杂LitJson示例、LitJson优化升级、Protobuf-net优化升级
- demo换了个位置
- JEngine v0.6.2
- 代码优化，同时针对某些版本的.net可能会出现的bug进行了修复
- bug fix
- 修复了2个小问题
- fixed #101
- 修复了pbnet反序列化泛型方法的重定向
- 又完整的支持Instantiate了，支持复制了，之前老版本的Instantiate的Bug全修复了
- 新增InstantiateDemo，同时优化Instantiate(GameObject)，暂时去除Instantiate(UnityEngine.Object)
- 修复ClassBind匹配Fields会重复的bug
- ClassBind自动获取支持筛选
- 编辑器大升级
- 泛型适配器的Demo他来了，就在CrossDomainDemo内
- Update JUI
- fix #91
- fix #93
- fixed #91 + 优化
- bug fix + 语言优化
- fix #80 #77
- 优化
- 小改进
- pre release
- ILRuntime 1.6.7
- bug修复+优化
- bug fix
- Update JAction.cs
- Update Init.cs
- 同步
- Update MenuItems.cs
- 修正了Xasset Pro模拟模式和真机模式的判定；如果集成了Xasset Pro，打包采用AES加密
- fix #76
- JStream+部分bug修复
- 支持AddComponent(Type type)
- xasset bug fix
- bug fix
- fix #67
- v0.6.1
- 优化Invoke
- Delete WebSocketSharp.Net.csproj
- Delete WebSocketSharp.csproj
- Update .gitignore
- bug fix
- 更新自更新助手
- 更新一下自动更新框架
- 面板热更JEngine
- JSaver优化，SendMessageBug修复
- Update Core.unitypackage
- bug fix
- Update README_en-us.md
- Update README.md
- 新增GetComponent(string)这个不常用的方法
- Update README.md
- Update Core.unitypackage
- JAction也能定位行数
- v0.6.1 beta2
- Demo界面优化
- Update CrossBindingCodeGenerator.cs
- 小改善
- README修改
- 细节调整
- v0.6.1 p1
- JAction Bug修复+文档地址
- 更新文档
- Update README.md
- JWebSocket文档+Demo修改
- bug fix
- bug fix
- bug fix while building
- fix #56
- Uploaded Core.unitypackage
- Update README.md
- Update README.md
- v0.6.0 final version
- 增加说明
- v0.6.0代码全有了，就差文档了
- about to v0.6
- about to v0.6
- Bug Fix
- JEngine设置面板
- fixed #49
- Clean升级
- 修复BUG
- ClassBind支持自动获取fields
- Update JsonMapper.cs
- ClassBind优化
- ClassBindMgr小优化
- 优化JEvent
- JEvent消息事件派发
- fixed #45 #44 #43 #42 #40
- Update README.md
- Update README.md
- fix #38 #34 #33
- fix #36
- fix #37
- fix #32
- fix #30
- fix #29
- Update RegisterCLRMethodRedirctionHelper.cs
- 还原了一个操作，脑子有点不太好使搞错了东西。
- bug fix + enhance + modify
- Update ILRuntimeCrossBinding.cs
- bug fix
- bug fix & enhance
- bug fix
- bug fix
- enhance
- Bug fix & enhance
- Update JBehaviour.cs
- delete
- 增加教程
- 增加UIKit文档
- fix classbind
- enhance
- 优化ClassBind,JSaver,JBehaviour
- JBehaviour enhancement
- 更新
- 删除优化
- enhancement
- bug fix
- 还原JBehaviour
- 修复BUG
- Delete .suo
- initial
- UI Demo
- Size decreases
- 0.5.x initial
- v0.5.8
- Update README_en-us.md
- Update README.md
- JBehaviour & JResource enhancements
- Update Core.unitypackage
- v0.5.7 p3
- Update README.md
- Update README.md
- Update README.md
- update readme
- 0.5.7 p2
- v0.5.7
- Update README.md
- v0.5.6 p2
- v0.5.6
- Update README_en-us.md
- Update README.md
- Updated Core.unitypackage
- ILRuntime update
- bug fix
- Update MonoBehaviourAdapter.cs
- improvements
- bug fix & improvement
- v0.5.5
- v0.5.4
- Update README_en-us.md
- Updated Core package & Init scene
- Update README.md
- Update Init.unity
- Update BuildScript.cs
- Update README.md
- Update README.md
- v0.5.3
- v0.5.3 增加打包后的资源的copy工具，修复StreamingAssets资源解压逻辑
- Update Assets.cs
- Update README.md
- Update README.md
- Update README.md
- v0.5.2
- Update README_zh-cn.md
- Update README_zh-cn.md
- v0.5.2
- bug fix
- Create Core.unitypackage
- Create Core.unitypackage
- Update README_zh-cn.md
- bug fix
- updated docs
- Update README_zh-cn.md
- Update LocalizedText
- Documents
- v0.5.1
- JAction bug fix
- Update README_zh-cn.md
- Update Clean.cs
- Update README_zh-cn.md
- fixed clean.cs
- clean.cs bug fix
- enhance Clean.cs
- Enhance & Fix CPU bug in editor
- Enhanced MonoBehaviour Adapter Editor
- Enhanced MonoBehaviour Adapter Editor
- Merge pull request #2 from JasonXuDeveloper/master ([#3](https://github.com/JasonXuDeveloper/JEngine/pull/3))
- delete demo
- Update README_zh-cn.md
- Update README.md
- Update README.md
- Update README_zh-cn.md
- bug fix
- Update README.md
- Update README_zh-cn.md
- v0.5.0
- v0.4.10
- Update Init.cs
- Update README_zh-cn.md
- Update README.md
- Sponsorship
- bug fix
- v0.4.9
- Update README_zh-cn.md
- Update README.md
- v0.4.8
- Updated README
- Updated Document
- Update README_zh-cn.md
- Updated README
- 0.4.7
- Updated README
- 0.4.6
- Update README
- v0.4.5
- Update gitee
- Grammar
- Update Extension.md
- v0.4.4
- Update Basic.md
- Update Extension.md
- Update Extension.md
- Update Extension.md
- Documents
- cleaned repo
- Update CHANGE.md
- Delete JEngine.Unity.unitypackage
- v0.4.3
- Clean up
- Update README_zh-cn.md
- Update README.md
- Update README
- Update README_zh-cn.md
- Update README.md
- Updated Logo
- Update logo.png
- v0.4.2
- Update README_zh-cn.md
- Update README.md
- Update README_zh-cn.md
- Update README_zh-cn.md
- Update README.md
- Update README_zh-cn.md
- Update README.md
- Update README_zh-cn.md
- Update README.md
- Update README.md
- empty .vs
- Bug Fix
- Rename DevelopementGuide.md to DevelopmentGuide.md
- Update DLLMgr.cs
- Update README.md
- Update README_zh-cn.md
- Upload meta files
- Update .gitignore
- JAction able to run on Main Thread
- v0.4.1
- delete .idea
- Delete riderModule.iml
- Update .gitignore
- v0.4.0
- Update README_zh-cn.md
- Update README.md
- Update README_zh-cn.md
- Update README.md
- 中文文档
- Update README_zh-cn.md
- Update README.md
- Update README.md
- 中文文档
- Update README_zhcn.md
- Update README_zhcn.md
- Update README_zhcn.md
- Update README_zhcn.md
- Update README_zhcn.md
- Chinese Document
- ILRuntime performence enhances
- v0.3.6.5
- Delete HotUpdateScripts-Debug|anycpu.json
- Delete UserPrefs.xml
- Update README.md
- Update README.md
- Update README.md
- v0.3.6.4
- Update JBehaviour.md
- Update README.md
- bug fix
- Update README.md
- Update README.md
- v0.3.6.3
- Update README.md
- Update README.md
- v0.3.6.2
- Update JBehaviour.md
- Update README.md
- improve performence of JAction & JBehaviour
- Update README.md
- Update README.md
- Update README.md
- v0.3.6.1
- Update README.md
- v0.3.6
- Update README.md
- ILRuntime now supports Profiler
- Enhance Init.cs
- Update README.md
- Update README.md
- v0.3.5
- daily commit
- Daily commit
- encrypt editor window supported
- Update README.md
- Update README.md
- v0.3.4
- Update README.md
- v0.3.3 with dll encryption
- Update JUI.md
- Update JUI.md
- Update README.md
- Update README.md
- Add directories with files
- XAsset bug fix
- Tiny improve
- Updates XAsset
- Update README.md
- Update JUI.md
- Improved JBehaviour, improved JUI
- Tiny change
- Update Examples & Improve some scripts
- Update README.md
- v0.3.2 final version
- Update JUI.md
- Update JUI.md
- Update CHANGE.md
- v0.3.2
- Update README.md
- Update README.md
- Update README.md
- v0.3.1
- v0.3
- Update README.md
- Update README.md
- v0.2
- v0.1 changes
- JEngine v0.1
- Initial commit


## 1.0.6 (January 25 2026)

- **Add DCO sign-off check for pull requests (#558)** (ci)
- **Include non-conventional commits in changelog (#557)** (ci)
- **Add automated testing and release workflows (#554)** (ci)
- **Use awk for reliable README multiline replacement (#567)** (ci)
- **Improve release workflow (#566)** (ci)
- **Store regex in variable to fix bash parsing error (#564)** (ci)
- **Use buildless mode for CodeQL Unity analysis (#563)** (ci)
- **Configure CodeQL to scan only JEngine source code (#562)** (ci)
- **Correct test artifact paths in unity-tests workflow (#561)** (ci)
- Revert "chore(release): v1.0.6" (#565)
- Fix build error handling and refactor Panel.cs (#552)
- Fix play mode test issues and refactor test code (#551)
- Fix play mode tests jumping to init scene instead of running (#550)
- Fix C# 9.0 Compilation Errors (#548)
- Add JEngine.Util package (#543)
- Add rootNamespace and TestRunnerCallbacks to JEngine.Core (#545)
- Add CLAUDE.md and GitHub Copilot custom instructions (#547)
- Remove development branch from documentation (#546)
- Update YooAsset samples to 2.3.18 and update dependencies (#544)
- Update hybridclr to 8.9.0 and add Codacy configuration (#542)
- Claude Code Review workflow
- Claude PR Assistant workflow
- Add using UnityEditor.UIElements to SettingsUIBuilder.cs and EditorUIUtils.cs
- Fix Unitask HandleBaseExtensions Add underline



## 1.0.5 (October 29 2025)

- **Fixed** missing HotUpdate Monobehaviour issue
- **Supported** Standalone mode

## 1.0.4 (October 15 2025)

- **Fixed** `AddComponent<T>` and `GetComponent<T>` issue under editor

## 1.0.3 (October 15 2025)

- **Supported** Unity CDN (UOS)
- **Optimized** WebGL asset loading throughput 



## 1.0.2 (October 15 2025)

- **Fixed** development and server setting save issue
- **Allowed** loading hot code's pdb file in order to debug in unity editor
- **Updated** Nino to the latest verstion
- **Updated** Unity to the latest 2022 LTS (fixing CVE) 



## 1.0.1 (September 28 2025)

- **AssetBundle Encryption** support (XOR, AES, ChaCha20)
- **Code Obfuscation** support by using Obfuz to protect almost all code
- **Dramatically improved** hot update code execution performance by migrating to HybridCLR
- **Enhanced** game development experience (no more extra user procedure when writing any kinds of hot update code) 
- **MiniGame** support for WeChat, TikTok, Alipay and TapTap
> Minor Update:
> - Upgrade **HybridCLR** to v8.6.0 (resolves building issues with latest Xcode)

## 1.0.0 (September 21 2025)

- **AssetBundle Encryption** support (XOR, AES, ChaCha20)
- **Code Obfuscation** support by using Obfuz to protect almost all code
- **Dramatically improved** hot update code execution performance by migrating to HybridCLR
- **Enhanced** game development experience (no more extra user procedure when writing any kinds of hot update code) 
- **MiniGame** support for WeChat, TikTok, Alipay and TapTap

### 0.8.0 (April 11 2023)

- **Supported** WebGL
- **Removed** JUI (will be redesigned shortly)
- **Optimized** LifeCycleMgr performance
- **Optimized** ThreadMgr features
- **Optimized** JStream performance
- **Optimized** JBehaviour performance
- **Optimized** MonoBehaviour performance
- **Optimized** FpsMonitor performance
- **Optimized** CryptoMgr interfaces
- **Optimized** JBehaviour/ClassBind Object/MonoBehaviour GC
- **Optimized** GetComponent(s)/FindObject(s)OfType performance
- **Switched** to YooAsset for asset management module (instead of Bundle Master)
- **NEW FEATURE** UnsafeMgr which provides unsafe (use for optimization) features 
- **NEW FEATURE** UnmanagedMemoryPool which supports allocating unmanaged memory with pooling
- **NEW FEATURE** CoroutineMgr which supports executing/stopping coroutines from non-MonoBehaviour class
- **NEW STRUCTURE** Split JEngine source code to multiple packages





### 0.7.5 (September 25 2022)

- **Updated** ILRuntime to v2.1.0
- **Optimized** code register workflow
- **Optimized** JStream buffer strategy
- **Optimized** ClassBind performance and GC
- **Optimized** JEngine Initialize procedure and performance
- **Optimized** LifeCycleMgr performance and GC
- **Optimized** ```FindObjectsOfType``` performance and GC
- **Optimized** JBehaviour performance and GC, removed its dependence to ```MonoBehaviour```
- **Optimized** Loom (Execute Actions on Main Thread) performance and GC, removed its dependence to ```MonoBehaviour```



### 0.7.4 (September 22 2022)

- Bug **fixed**
- **Imported ** high performance C# library Nino
- **Optimized** JBehaviour performance and GC
- **Optimized** MonoBehaviour update logics and GC
- **Optimized** blockwised decrypt intepret module's allocated memroy size and GC



### 0.7.3 (July 14 2022)

- Bug **fixed**

- **Updated** ILRuntime

- **Updated** Bundle Master

- **Enhanced** JBehaviour performance

- **Enhanced** ClassBind runtime performance

- **Almost no GC allocation**  async wait method

  

### 0.7.2 (May 15 2022)

- Bug **fixed**
- **Enhanced** framework code
- **Enhance ** Lifecycle
- **Updated** ETTask
- **Enhanced **JAction
- **Enhanced** JBehaviour
- **Enhanced** Unity **Exception StackTrace**
- **HotUpdateResource** support **Blacklist**
- **New** Protobuf **Serialize Demo**
- **Updated** Protobuf-net **library**

### 0.7.1 (March 28 2022)

- **Update** ILRuntime v2.0.2
- Support **multiple** hot update **packages**
- **Removed** XAsset
- Imported **BundleMaster**



### 0.7.0 (Feburary 17 2022)

  - Mark dependencies as **modules** (More standardized)
  - Support **offline mode** (Run your game without building a resource server)
  - Modify **developing procedure** (Enhanced experience when developing)
  - **Decoupling framework code** (More lightweight and even easier to use)
  - **Enhanced framework code** (A better performance)
  - Enhanced **Litjson and Protobuf-net** (More reliable (de)serializing tools)
  - **Fixed all Issues** occured in 0.6.3 (More powerful and less bugs)
  - More **Demo** (Easier to start)
  - Add **extensible validator** (New feature)
  - Useful **UI extension tools** (New feature)
  - **ILRuntime2.0** (A better performance with less bugs)
  - **JPrefab Upgrade** (Support (a)synchronization to create and destory mutliple at once)
  - **JBehaviour new lifecycles** (Similar to MonoBehaviour's OnEnable and OnDisable)



### v0.6.3 (August 13 2021)

- **Implemented** ```[System.Serializable]``` when generating **c# from proto**
- **Changed** ClassBind default to **active after**
- **Enhanced** the precision for **JFloat**
- **Enhanced** unity project which no longer depends JBehaviour from HotUpdate Solution
- **NEW** JEvent + Event Demo



### v0.6.2 (April 30 2021)

- **Fixed** plenty of bugs
- **Support** XAsset 6.1 (Regards to branch xasset6.1)
- **Optimize** JAction
- **Optimize** Downloader
- **Optimize** ClassBind **performance**
- **Optimize** localization solution
- **Optimize** hot object inspector **serialization**
- **Optimize** ILRuntime **Cross domain adapter generator**
- **Optimize** ```GameObject.Instantiate``` CLR Redirection
- **Upgrade** JEngine**Panel**
- **Upgrade** JUI's **Bindable Property** feature
- **Upgrade** ILRuntime to  **v1.6.7**
- **Add** ClassBind support **filters** on automatically fetching fields
- **Add** hot **source code supports memory encryption** and does not effect performence
- **Add** JEngine **Demos** （Instantiate, Generic Cross Domain Adaptor, etc）



### v0.6.1 (Janurary 4 2021)

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
- Support  **```Invoke```, ```InvokeRepeating```, ```IsInvoking```, ```CancelInvoke```** in hot update scripts
- JEvent support **Subscribe methods from base class**
- **Bug fix** on saving data on prefabs
- **JSaver** supports **get Default Value**
- Better **Editor Panel**
- Support XAsset Pro (Main feature is patching, needs to subscribe)





### v0.6.0 (December 6 2020)

- **UI Framework**
- **Enhance** JSaver
- **JAction** supports ```DelayFrames```
- **Fixed a heaps of bugs**
- **JSON Checker Tool**
- **JEvent**, which is similar to Google core libraries for Java, eventbus
  - **Subscribe Class**
  - **Subscribe Method**
  - **Post** data **to all subscribed events**
  - Can be running on **Main Thread or sub Thread**
- **Enhance** ClassBind, much more powerful
  - Auto Bind when needed, **better performence**
  - Controls by **ClassBindMgr**, **more advanced while setting fields**
  - Support **Automatically get all field**s from a Hot Update Class
  - Support **Automattically find the FieldType** for a field
- **JExtensions** 
  - **Allows get hot update class** from GameObject **via Classbind**
  - **Add JBehaviour** to GameObject
  - **Add JUI** to GameObject
  - **Get JBehaviour** from GameObject
  - **Get JUI** from GameObject
- **JWebSocket**
  - **Connect** to websocket servers
  - **Send** message to websocket servers
  - **Listen** message from websocket servers
  - **Listen and Send in Sub Thread**
- **JEngine Setting Panel**
  - **Automatically jump to Init scene first while running**
  - **Hot Update Scenes Shortcuts**
  - **ClassBind Tools**
  - **Error Rescue Tools**
- **More CLR Redirection Methods**
  - Support **MonoBehaviour.SendMessage** from Hot Update Class
  - Support **MonoBehaviour.Instantiate** GameObject with Hot Update Classes & Single Hot Update Class
- **JBehaviour enhance**
  - **Perform better after built** than in Editor
  - Support **Deltatime, loop counts**, etc.
  - **Powerful Inspector**





### v0.5.8 (October 29 2020)

- **Bug fix** when copying streaming asset
- **C# type to Proto file** converter
- **Rearrange** menuitems
- **Enhance** ClassBind
- **More advanced** JBehaviour Editor
- **JBehaviour** supports create by call ```new()```
- **BindableProperty** supports binding on multiple events





###  v0.5.7 (October 25 2020)

- **JBehaviour** got rid of MonoBehaviour, a significant increasement of performance has been developed
- **JUI** got rid of MonoBehaviour
- **Register Helper** for CLR Method Redirection
- **ClassBind** enhancement



### v0.5.6 (October 19 2020)

- **Less GC** to inherit MonoBehaviour in Hot Update Solution
- **Value Type Binder Register Helper** allows register value type binders, which typically decreases GC on Value Types (EG. Vector3/2)
- **ClassBind improvements** which increases the performence
- **ClassBind supports properties** which allows set properties just like setting fields
- **Localization improvements** which increases the performence
- **Localization Text improvements** which spends less memory and GC
- **Bug fix** on ILRuntime Cross Bind Adapter Generator
- **JBehaviour improvements** which decreases GC and runs better
- **Bug fix** on JBehaviour when calls ```pause()``` and ```resume()```
- **JPrefab** which is a new solution for managing hot update prefabs





### v0.5.5 (October 11 2020)

- **ClassBind** allows auto **attach any class** to a GameObject
- **ClassBind** allows use **consturcor** as a initial of fields of a class
- **ClassBind** allows call ```Active()``` method whether the bound class inherited MonoBehaviour or JBehaviour or not
- **Editor** for **Hot Update Scripts to GameObject** supports present **more stuffs**
- **Demo** for using **AntiCheat Struct** & **Bindable Property** improves



### v0.5.4 (October 10 2020)

- **Anti-Cheat Structs** allows detect hackers who hacks your game
- **ClassBind improvements** more friendly to update from old versions to latests



### v0.5.3 (October 6 2020) (Support by XAsset's Author)

- Supports **copy DLC files to Streaming Assets**, which can pass App Store review easier
- Supports **move local Streaming Asstes hot update resouces to persistence path** which user don't need to download any old things from server



### v0.5.2 (October 1 2020)

- **Anti-Cheat Structs** which prevents cheating tools change data stores in memory
  - **Drag 'n drop** on Class Bind
  - **LitJSON Serialize and Desirialize Float**



### v0.5.1 (Sepetember 26 2020)

- **Localization** supports multiple languages in your game
- **Smarter** autobind, easier to set value of a Unity Component
- **More** event functions for MonoBehaviour in HotUpdate



### v0.5.0 (Sepetember 19 2020)

- **Rebuilt** demos
- **Rewrote** **StringifyHelper**
- **Updated** ILRuntime
- **Rewrote** JSaver
- **Enhanced** Auto Class Bind
- **Enhanced** Cleaning Hot Update Solution in Unity Editor
- **Enhanced** LitJson to serialize and deserialize **BindableProperty** (**Decreases 80%** of size when **converting BindableProperty to JSON**)
- **Adapt** protobuf-net
- **Proto2CS** converter
- **Rearranged** hot update solution
- **JResource** now supports **load scene async** and **get progress when loading scene**
- **Combined** JBehaviour & JUI to JEngine.Core
- **Enhanced** JBehaviour
- **Enhanced** BindableProperty ToString method
- **Enhanced** MonoBehaviour Adapter Inspector Window



### v0.4.10 (Sepetember 12 2020)

- **StringifyHelper** is a helper which serialize and deserialize data
  - Support **JSON & Protobuf & BinaryFormatter**
- **JSaver** now uses StringifyHelper to do JSON converts
- **Enhance** clean.cs



### v0.4.9 (September 10 2020)

- **Able to autobind hot update resource**
- **Auto bind** runs in Awake which becomes **faster**
- ILRuntime Adapter Generator supports **custom assembly**



### v0.4.8 (September 9 2020)

- **Right Click** to **copy GameObject Path from hierarchy**
- **Bug fix** on **release dll**
- **Bug fix** on **Build Setting scenes**



### v0.4.7 (September 8 2020)

- **Autobind** supports bind to GameObject **itself**
- Support **AES encrypt string**
- **JSaver**, support Data persistence



### v0.4.6 (September 7 2020)

- **MonoBehaviour Adapter Inspector window show error fields**
- **Autobind editor window**
- **Autobind support bool value**
- **Autobind support private and static and instance fields**





### v0.4.5 (September 6 2020)

- **ILRuntime Adpater Generater**
- **Enhanced Autobind**
- **Less GC on Autobind**
- **Auto Destory when Finish Autobind**
- **No need to call any method to require autobind**





### v0.4.4 (September 5, 2020)

- **Autobind** support auto set fields
- **Enhance Autobind**



### v0.4.3 (September 4, 2020)

- **Custom scenes** bug fixed
- **Pdb symbol** bug fixed
- **Enhanced** JBehaviour
- **Autobind** classes from hotupdate dll to prefabs
- **Support** more Unity versions



### v0.4.2 (September 3, 2020)

- **Advanced** hotupdate dependecies **cleaner**
- **Enhanced** development mode
- Allows **custom scenes addition**



### v0.4.1 (August 22, 2020)

- JAction now supports **execute in main thread**

  > As we know Task.Run in Unity will run in a new thread, which is not able to call most Unity APIs, now JAction found a solution of using Loom.

- Enhance **Developement Mode performance**



### v0.4.0 (August 16, 2020)

- **JEngine Framework** has been **devided** into following parts:

  - **JEngine**

    > You can include this into your hot-update scripts or your Unity main project

    - Core
    - UI
    - LifeCycle

  - **UnityPlugins**

    > You can choose to import these into unity

    - JEngine.Unity (compulsory to a new project)
    - JEngine.JSON (optional)
    - JEngine.Redis (optional)

- **Small bugs** has been **fixed**

- **ILRuntime back to Unity Solution**





### v0.3.6.5 (August 9, 2020)

- **JResource** supports **match pattern**, which prevents different resources witch same name can't be loaded

  ```c#
  public enum MatchMode
  {
    AutoMatch = 1,
    Animation = 2,
    Material = 3,
    Prefab = 4,
    Scene = 5,
    ScriptableObject = 6,
    TextAsset = 7,
    UI = 8,
    Other = 9
  }
  ```

  





### v0.3.6.4 (August 8, 2020)

- **Enhanced** JAciton

- **Enhanced** JBehaviour

- **JResource** is now coming

  - JResource is based on XAsset and it allows to load asset from hot-update resources via sync/async methods

  ```c#
  var txt = JResource.LoadRes<TextAsset>("Text.txt");
  Log.Print("Get Resource with Sync method: " + txt.text);
  ```

  



### v0.3.6.3 (August 6, 2020)

- JAction supports **Cancelation Callback**

  ```c#
  //Cancel a JAction
  JAction j8 = new JAction();
  j8.RepeatWhen(() => Log.Print("[j8] I am repeating!!!"), () => true, 1, timeout)
    .ExecuteAsyncParallel();
  //You can either add a cancel callback
  j8.OnCancel(() => Log.Print("[j8] has been cancelled!"));
  ```

- JAction supports **Reset**

  ```c#
  //Reset a JAction
  j8.Reset();
  ```

- Fixed bug on JAction.ExecuteAsyncParallel



###  v0.3.6.2 (August 5, 2020)

- JAction supports **Async & Async Parallel**

  ```c#
  //Execute Async
  JAction j6 = new JAction();
  _ = j6.Do(() => Log.Print("[j6] This is an async JAction"))
    .ExecuteAsync();
  
  //Execute Async Parallel
  JAction j7 = new JAction();
  j7.Do(()=>Log.Print("[j7] This is an async JAction but runs parallel, callback will be called after it has done"))
    .ExecuteAsyncParallel(()=>Log.Print("[j7] Done"));
  ```

- JAction supports **Cancelation**

  ```c#
  //Cancel a JAction
  JAction j8 = new JAction();	
  _ = j8.RepeatWhen(() => Log.Print("[j8] I am repeating!!!"), () => true, repeatDuration, timeout)
    .ExecuteAsync();
  JAction j9 = new JAction();
  j9.Delay(5)
    .Do(() =>
        {
          j8.Cancel();
          Log.Print("[j9] cancelled j8");
        })
    .Execute();
  ```

  



### v0.3.6.1 (August 3, 2020)

- JAction supports more features

  - Repeat with frequency

    ```c#
    int repeatCounts = 3;
    float repeatDuration = 0.5f;
    JAction j = new JAction();
    j.Repeat(() =>
           {
             Log.Print("I have repeated");
           }, repeatCounts, repeatDuration)
      .Excute();
    ```

  - Repeat with condition

    ```c#
    int num = 10;
    float repeatDuration = 0.5f;
    float timeout = 10f;
    JAction j = new JAction();
    j.RepeatWhen(() =>
                 {
                   Log.Print($"num is more than 0, num--");
                   num--;
                 },
                 () => num > 0, repeatDuration, timeout)
      .Excute();
    ```

  - Repeat until

    ```c#
    int num = 10;
    float repeatDuration = 0.5f;
    float timeout = 10f;
    JAction j = new JAction();
    j.RepeatUntil(() =>
                 {
                   Log.Print($"num is more than 0, num--");
                   num--;
                 },
                 () => num <= 0, repeatDuration, timeout)
      .Excute();
    ```

    

  - Wait Until

    ```c#
    JAction j = new JAction();
    j.Until(()=> something is done)
      .Do(something)
      .Excute();
    ```

    

### v0.3.6 (August  2, 2020)

- **JAction** which **supports less code but do more**

  ```c#
  JAction j = new JAction();
  j.Do(() =>
        {
          Log.Print("Hello from JAction!");
        })
    .Delay(3.0f)
    .Do(() =>
        {
          Log.Print("Bye from JAction");
        })
    .Excute();
  ```

  

- Hidden dictionary to save Hot Update DLL which **strongly increased unity excute speed for dll** (Unity will no longer load Hot Update DLL from editor)



### v0.3.5 (July 29, 2020)

- **Rewrite JBehaviour's source code** which enhances the performence
- **JObjectPool** is a new **solution for Object Pool**
- **Improve JBehaviour** which it now **supports method-chaning** in some part
- **Enhance JUI**



### v0.3.4 (July 25, 2020)

- **JBehaviour Example**
- **Rewrite** JBehaviour **Structure**
- **Improve** JUI
- **Fix** that **JBehaviou**r can't be called



### v0.3.3 (July 24, 2020)

- **Hot Update DLL Encryption** based on AES encryption, with a 16 bits key, it is harder to let others hack your dll
- More **ILRuntime Registerations support**, reduce the requirements of registerations when using ILRuntime
- Update **XAsset** and **ILRuntime** to the latest version



### v0.3.2 (July 19, 2020)

- **JUIText** becomes **JUI**

  - Supports any **UIBehaviour based class** to use

    > All UGUI components in Unity is UIBehaviour based

- **JUI** supports **data binding**

- **JUI** supports **message mode**, will be called when binded data has changed

- **BindableProperty** is coming, use it in your data class or sturcts

- Improve performence of looping in **JUIBehaviour**

- Improve performence on getting **generic components** in JUI

- Update showcases to a countdown showcase and a data update & binding showcase





###  v0.3.1 (July 18, 2020)

- Combined **Unity-GUI-Redis**, which is also part of **JEngine**, now belongs to **JEngine.Redis** namespace

- Update **XAsset** dependency to latest version:

  - Supports **network monitor**
  - Build bundles name by **hash**
  - Tiny improvment on the framework

- Rewrite **ILRuntime**:

  - Supports **OnDestory** method called by MonoBehaviour
  - Tiny improvment on the framework

- **JUI** is now coming:

  > Only supports Text at the moment

  - Based on **JUIBehaviour**
  - **Method-Chaining** which makes more stramlined and beautiful codes
  - Unique and managable **lifecycle**

- Improve **JUIBehaviour**



### v0.3 (July 17, 2020)

- Update **ILRuntime** to v1.6.3 which fixes heaps of bugs in the dependency
- **JEngine Lifecyle** is now included, it is **only a prototype** and will be extended in the future with more base codes.
- **JUIBehaviour** is coming, it is a behaviour base on MonoBehaviour but more friendly to manage UI components' lifecycles which are not require to change that frequently, **and runs better**, you can use less codes to implement more



### v0.2 (July 16, 2020)

- Supports development mode which **loads dll and resources from local**
- Automatically **clean up unnecessary dlls, pdbs, etc.** in DLL Resource Directory
- Automatically **convert dll into bytes** in Editor



### v0.1  (July 14, 2020)

- Automatically generate **Update Resources**
- Automatically handle **Hot-update DLL**
- Update [Hot-update](https://github.com/JasonXuDeveloper/JEngine/blob/4d63fec4027ff5c546fb15ec2469ead898922858/README.md#What-is-Hot-update) codes and resources from server (Base on XAsset & ILRuntime)
- Supports local hot-update code development in Unity Editor via dll in Asstes/HotUpdateResources/Dll/HotUpdateScripts.dll

