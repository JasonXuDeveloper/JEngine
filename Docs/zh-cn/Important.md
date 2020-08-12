## 须知指南

- 进行**building bundle**时，会弹出一个弹窗要求输入加密密码，共**16位**，用于**加密DLL**

  ![key1](https://s1.ax1x.com/2020/07/26/apuoHs.png)

- **Init场景中**，找到**HotFixCode**，Inspector中Init方法里的Key为DLL加密密码

  ![key2](https://s1.ax1x.com/2020/07/26/apu7En.png)

- 生成项目的时候，**为了避免冗余，请手动删除热更场景**（开发模式会自动将热更场景加入Build Settings）

  ![build](https://s1.ax1x.com/2020/07/20/Uhxcuj.jpg)

