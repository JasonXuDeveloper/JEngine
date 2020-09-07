## 代码绑定指南

> 有的时候，一个GameObject会包含很多热更脚本，一个个手动AddComponent太浪费时间，于是就有了自动绑定工具

1. 给GameObject或预制体**添加Class Bind脚本**
2. 想给该GameObject加几个热更脚本，**就把Classes的size修改为该数字**
3. **Namespace写命名空间 (默认HotUpdateScripts，即热更命名空间)，Class写脚本类名**
4. **运行**后会**自动绑定**，可以勾选Active After自动激活
5. 继承**MonoBehaviour**的脚本，**激活需要enabled = true 以及 调用Awake()**，
6. 继承**JBehabviour**的脚本，直接调用**Activate()**

### 参数介绍

- ScriptsToBind

  - 要添加**多少个**热更DLL里的**脚本**

- Namespace

  - 热更脚本的**命名空间**，**默认HotUpdateScripts**

- Class

  - 热更脚本的**类名**

- Active After

  - **勾选后**，会在绑定后，或绑定+赋值后，**自动激活**
  - **没勾选**，需手动，**参考指南第5和第6条**

- Require Bind Fields

  - **勾选后**，**会根据Fields里的数据自动赋值**
  - **没勾选**，**哪怕Fields里有东西也不赋值**

- Fields

  - **自动赋值**该脚本**多少个值**

- Field Type

  - 该值的**类型**（支持**数字类型**，**布尔值类型**，**字符串类型**，**GameObject**和**挂在GameObject上面的不可热更的脚本**）

- Field Name

  - 该值在热更脚本里的名字

    ![name](https://s1.ax1x.com/2020/09/05/wEyk9K.png)

- Value

  - 该值的数值
    - 如果是**数字或字符串**，直接写进去
    - 如果是**布尔值**，写true或false
    - 如果是**GameObject**，请写**完整路径**，**如果是一个GameObject的子物体，父物体要Active**；**如果不是子物体，本身要Active**，例如：**路径，Canvas/Text，其中Text可以不Active，Canvas必须Active；路径，Demo，其中Demo必须Active**，可以指向自己，指向自己，输入**${this}**
    - 如果是**Unity Component**，即不可热更的挂载脚本，需要输入**完整路径，参考GameObject**，**加上"."，最后加上脚本名称**，**例如：Canvas/Text.Text，即获取这个GameObject的Text组件**，可以指向自己身上的组件，输入**${this}.xxx**，xxx为组件名
  
  

![show](https://s1.ax1x.com/2020/09/06/wenolT.png)

