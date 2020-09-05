## 热更代码指南

> 该指南将帮助您热更代码

1. 找到并打开**Project/HotUpdateScripts/HotUpdateScripts.sln**
2. 选择**Program.cs** 并打开
3. 更改但**不要删除** *RunGame()*方法
4. **以Release模式打包DLL**
5. 回到Unity，进行基础指南的**4&5步**
6. 开启开发模式，运行游戏
7. 您会发现热更代码能够完美运行，恭喜！



### 代码绑定指南

> 有的时候，一个GameObject会包含很多热更脚本，一个个手动AddComponent太浪费时间，于是就有了自动绑定工具

1. 给GameObject**添加Class Bind脚本**
2. 想给该GameObject加几个热更脚本，**就把Classes的size修改为该数字**
3. **Namespace写命名空间 (默认HotUpdateScripts，即热更命名空间)，Class写脚本类名**
4. **运行**后会**自动绑定**，需要注意的是**添加的脚本不会赋值**，**里面的Properties需要自己赋值**，推荐使用**ScriptableObject**可以**可视化赋值**且**可以被热更新**，**只需要手动把自动添加的脚本的值和ScriptableObject绑定即可**
5. 继承**MonoBehaviour**的脚本，**激活需要enabled = true 以及 调用Awake()**
6. 继承**JBehabviour**的脚本，直接调用**Activate()**
7. 必要时，可以调用 **InitILrt.BindAllScripts();** 来请求自动绑定（Program.cs里面有，请勿删除）

![autobind](https://s1.ax1x.com/2020/09/04/wkGjqe.jpg)

