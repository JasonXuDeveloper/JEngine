## 目录介绍

- **Assets** - Unity项目根目录
  - **Dependencies** - 全部第三方插件可以放在这里，每次更新覆盖该目录
  - **HotUpdateResources** - 所有热更资源将存放在这里
    - **Controller** - 动画
    - **Dll** - 该目录存放热更代码
    - **Material** - 材质
    - **Prefab** - 预制体
    - **Scene** - 场景
    - **ScriptableObject** - Unity的可程序化物件
    - **TextAsset** - 文本资源
    - **UI** - 图片资源
    - **Other** - 其他任意东西，只要能被加载的都可以丢在这里
  - **Scripts** - 无法热更新的代码
    - **Init.cs&InitILRT.cs** - **十分重要**的文件，用于启动游戏，每次更新替换该2个文件
    - **APIs** - 往该文件夹里放您的代码
  - **Init.unity** - 启动游戏的场景
- **Builds** - 生成的客户端可以放在这里
- **DLC** - 热更资源导出目录
- **HotUpdateScripts** - 热更代码项目
  - **Program.cs** - 启动游戏的代码, **你可以更改里面的东西，但请不要删除或更改该脚本的RunGame方法**
  - **JEngine** - **请勿删除，JEngine部分源码在里面，**每次更新覆盖该目录

